using System.Text.Json;
using System.Text.Json.Serialization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class HomeownerPropertyService(
    AppDbContext db,
    IAddressLookupService addressLookupService,
    IOpenAiMaintenanceRecommendationService maintenanceRecommendationService,
    PropertyEnrichmentCache enrichmentCache,
    ILogger<HomeownerPropertyService> logger) : IHomeownerPropertyService
{
    private static readonly TimeSpan MaintenanceTimeout = TimeSpan.FromSeconds(90);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public Task<Propiedad?> GetPrimaryPropertyAsync(string userId, CancellationToken cancellationToken = default) =>
        db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PropertyInfoViewModel?> EnrichAddressAsync(
        AddPropertyViewModel model,
        CancellationToken cancellationToken = default)
    {
        var lookupAddress = model.BuildLookupAddress();
        logger.LogInformation("Enriching homeowner property address: {Address}", lookupAddress);

        enrichmentCache.Remove(lookupAddress);

        var propertyInfo = await addressLookupService.GetPropertyInfoAsync(lookupAddress);
        if (propertyInfo == null)
        {
            return null;
        }

        ApplyAddressFields(propertyInfo, model);
        propertyInfo.MaintenanceRecommendations = await TryGenerateMaintenanceAsync(propertyInfo, cancellationToken);
        return propertyInfo;
    }

    public async Task<int> SaveOrUpdatePropertyAsync(
        PropertyInfoViewModel propertyInfo,
        string userId,
        int? existingPropertyId = null,
        CancellationToken cancellationToken = default)
    {
        var attomRawJson = propertyInfo.AttomRawJson;
        propertyInfo.AttomRawJson = null;
        var jsonRaw = JsonSerializer.Serialize(propertyInfo, JsonOptions);
        var hasAttomPayload = !string.IsNullOrWhiteSpace(attomRawJson);

        Propiedad propiedad;
        if (existingPropertyId is > 0)
        {
            propiedad = await db.Propiedades
                .FirstAsync(p => p.Id == existingPropertyId && p.UserId == userId, cancellationToken);
        }
        else
        {
            propiedad = new Propiedad
            {
                UserId = userId,
                FechaCreacion = DateTime.Now,
                Activo = true
            };
            db.Propiedades.Add(propiedad);
        }

        propiedad.Direccion = FormatAddressWithUnit(propertyInfo.FormattedAddress, propertyInfo.Unit);
        propiedad.DatosJson = jsonRaw;
        propiedad.AttomPropertyId = propertyInfo.AttomPropertyId;
        propiedad.AttomRawJson = attomRawJson;
        propiedad.AttomLastSyncUtc = hasAttomPayload ? DateTime.UtcNow : propiedad.AttomLastSyncUtc;
        propiedad.AttomSyncStatus = IsSuccessfulEnrichment(propertyInfo.DataSource) || propertyInfo.AttomPropertyId.HasValue
            ? "Success"
            : (hasAttomPayload ? "Partial" : "Estimated");
        propiedad.AttomSyncError = hasAttomPayload || IsSuccessfulEnrichment(propertyInfo.DataSource)
            ? null
            : "Property enrichment not available";

        if (PropertyMaintenanceDisplayService.IsRealAiPlan(propertyInfo.MaintenanceRecommendations))
        {
            var maintenancePlan = propertyInfo.MaintenanceRecommendations!;
            maintenancePlan.GeneratedUtc ??= DateTime.UtcNow;
            propiedad.MantenimientoRecomendadoJson = JsonSerializer.Serialize(maintenancePlan, JsonOptions);
            propiedad.MantenimientoRecomendadoUtc = maintenancePlan.GeneratedUtc;
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Homeowner property saved (Id={Id}) for user {UserId}", propiedad.Id, userId);
        return propiedad.Id;
    }

    public async Task<PropertyMaintenancePlanViewModel> TryGenerateMaintenanceAsync(
        PropertyInfoViewModel propertyInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(MaintenanceTimeout);

            var plan = await maintenanceRecommendationService.GenerateAsync(propertyInfo, cts.Token);
            if (PropertyMaintenanceDisplayService.IsRealAiPlan(plan))
            {
                logger.LogInformation(
                    "OpenAI maintenance plan generated for {Address} ({Count} items)",
                    propertyInfo.FormattedAddress,
                    plan.Items.Count);
                return plan;
            }

            logger.LogWarning(
                "OpenAI maintenance unavailable for {Address}: {Reason}",
                propertyInfo.FormattedAddress,
                plan.Summary);
            return plan;
        }
        catch (Exception ex) when (ex is OperationCanceledException or TimeoutException)
        {
            logger.LogWarning(ex, "Maintenance recommendation timed out for {Address}", propertyInfo.FormattedAddress);
            return BuildUnavailableMaintenancePlan(
                "Maintenance suggestions are still loading. You can review them later from your home dashboard.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Maintenance recommendation failed for {Address}", propertyInfo.FormattedAddress);
            return BuildUnavailableMaintenancePlan(
                "We couldn't generate maintenance suggestions right now. Try again from Edit Profile.");
        }
    }

    public void ApplyAddressFields(PropertyInfoViewModel propertyInfo, AddPropertyViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Unit))
        {
            propertyInfo.Unit = model.Unit.Trim();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.Street))
        {
            propertyInfo.Street = model.StreetAddress.Trim();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.City))
        {
            propertyInfo.City = model.City.Trim();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.State))
        {
            propertyInfo.State = model.State.Trim().ToUpperInvariant();
        }

        if (string.IsNullOrWhiteSpace(propertyInfo.PostalCode))
        {
            propertyInfo.PostalCode = model.ZipCode.Trim();
        }

        propertyInfo.Country ??= "US";

        if (string.IsNullOrWhiteSpace(propertyInfo.FormattedAddress))
        {
            propertyInfo.FormattedAddress = model.BuildLookupAddress();
        }
    }

    private static PropertyMaintenancePlanViewModel BuildUnavailableMaintenancePlan(string message) =>
        new()
        {
            Summary = message,
            DataSource = "Unavailable",
            IsAiGenerated = false,
            GeneratedUtc = DateTime.UtcNow,
            Items = []
        };

    private static bool IsSuccessfulEnrichment(string? dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource)) return false;
        return dataSource.Contains("AI", StringComparison.OrdinalIgnoreCase)
            || dataSource.Contains("ATTOM", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatAddressWithUnit(string address, string? unit)
    {
        if (string.IsNullOrWhiteSpace(unit)) return address;
        return $"{address}, {unit.Trim()}";
    }
}
