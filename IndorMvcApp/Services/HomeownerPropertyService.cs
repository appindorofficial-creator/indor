using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class HomeownerPropertyService(
    AppDbContext db,
    IAddressLookupService addressLookupService,
    IOpenAiMaintenanceRecommendationService maintenanceRecommendationService,
    IServiceScopeFactory scopeFactory,
    PropertyEnrichmentCache enrichmentCache,
    IOptions<OpenAiPropertyOptions> openAiOptions,
    ILogger<HomeownerPropertyService> logger) : IHomeownerPropertyService
{
    private static readonly ConcurrentDictionary<int, byte> ActiveMaintenanceJobs = new();
    private static readonly ConcurrentDictionary<int, byte> ActiveEnrichmentJobs = new();
    private static readonly TimeSpan MaintenanceTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan BackgroundEnrichmentTimeout = TimeSpan.FromMinutes(4);
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
        bool requestFullHouseFactResearch = false,
        CancellationToken cancellationToken = default)
    {
        var lookupAddress = model.BuildLookupAddress();
        logger.LogInformation("Enriching homeowner property address: {Address}", lookupAddress);

        enrichmentCache.Remove(lookupAddress);

        var propertyInfo = await addressLookupService.GetGeocodedPropertyAsync(lookupAddress, cancellationToken);
        if (propertyInfo == null)
        {
            logger.LogWarning(
                "Geocoding returned no match for {Address}; continuing with form address for AI enrichment",
                lookupAddress);
            propertyInfo = BuildPropertyInfoFromForm(model);
            if (propertyInfo == null)
            {
                return null;
            }
        }

        ApplyAddressFields(propertyInfo, model);
        propertyInfo.RequestFullHouseFactResearch = requestFullHouseFactResearch;

        try
        {
            using var enrichCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var syncTimeoutSeconds = requestFullHouseFactResearch ? 300 : 120;
            enrichCts.CancelAfter(TimeSpan.FromSeconds(syncTimeoutSeconds));
            await addressLookupService.EnrichPropertyInfoAsync(propertyInfo);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TimeoutException)
        {
            logger.LogWarning(ex, "Synchronous enrichment timed out for {Address}; will retry in background if needed", lookupAddress);
        }

        if (openAiOptions.Value.DeferMaintenanceRecommendations)
        {
            propertyInfo.MaintenanceRecommendations = BuildPendingMaintenancePlan();
        }
        else
        {
            propertyInfo.MaintenanceRecommendations = await TryGenerateMaintenanceAsync(propertyInfo, cancellationToken);
        }

        return propertyInfo;
    }

    public async Task<int?> SaveHomeAddressAsync(
        AddPropertyViewModel model,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.StreetAddress)
            || string.IsNullOrWhiteSpace(model.City)
            || string.IsNullOrWhiteSpace(model.State)
            || string.IsNullOrWhiteSpace(model.ZipCode))
        {
            return null;
        }

        var existing = await GetPrimaryPropertyAsync(userId, cancellationToken);
        var lookupAddress = model.BuildLookupAddress();
        var propertyInfo = await addressLookupService.GetGeocodedPropertyAsync(lookupAddress, cancellationToken)
            ?? BuildPropertyInfoFromForm(model);
        if (propertyInfo == null)
        {
            return null;
        }

        ApplyAddressFields(propertyInfo, model);
        return await SaveOrUpdatePropertyAsync(propertyInfo, userId, existing?.Id, cancellationToken);
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

        if (openAiOptions.Value.DeferMaintenanceRecommendations
            && !PropertyMaintenanceDisplayService.IsRealAiPlan(propertyInfo.MaintenanceRecommendations))
        {
            QueueMaintenanceGeneration(propiedad.Id);
        }

        if (string.IsNullOrWhiteSpace(attomRawJson)
            && !PropertyEnrichmentMapper.HasMeaningfulDetails(propertyInfo.PropertyDetails ?? new PropertyDetailsInfo()))
        {
            QueuePropertyEnrichment(propiedad.Id, fullResearch: true);
        }
        else if (hasAttomPayload && OpenAiPropertyEnrichmentService.IsQuickOnlyPayload(attomRawJson))
        {
            QueuePropertyEnrichment(propiedad.Id, fullResearch: true);
        }

        return propiedad.Id;
    }

    public void QueuePropertyEnrichment(int propiedadId, bool fullResearch = false)
    {
        if (!ActiveEnrichmentJobs.TryAdd(propiedadId, 0))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var scopedLookup = scope.ServiceProvider.GetRequiredService<IAddressLookupService>();
                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<HomeownerPropertyService>>();
                var scopedCache = scope.ServiceProvider.GetRequiredService<PropertyEnrichmentCache>();

                var propiedad = await scopedDb.Propiedades.FirstOrDefaultAsync(p => p.Id == propiedadId && p.Activo);
                if (propiedad == null)
                {
                    return;
                }

                var propertyInfo = DeserializePropertyInfo(propiedad);
                if (propertyInfo == null)
                {
                    return;
                }

                propertyInfo.FormattedAddress ??= propiedad.Direccion ?? string.Empty;
                propertyInfo.AttomRawJson = propiedad.AttomRawJson;
                propertyInfo.RequestFullHouseFactResearch = fullResearch;

                scopedCache.Remove(propertyInfo.FormattedAddress);
                using var cts = new CancellationTokenSource(BackgroundEnrichmentTimeout);
                await scopedLookup.EnrichPropertyInfoAsync(propertyInfo);

                if (string.IsNullOrWhiteSpace(propertyInfo.AttomRawJson))
                {
                    scopedLogger.LogWarning("Background enrichment produced no JSON for property {PropertyId}", propiedadId);
                    return;
                }

                var attomRawJson = propertyInfo.AttomRawJson;
                propertyInfo.AttomRawJson = null;
                propiedad.DatosJson = JsonSerializer.Serialize(propertyInfo, JsonOptions);
                propiedad.AttomRawJson = attomRawJson;
                propiedad.AttomLastSyncUtc = DateTime.UtcNow;
                propiedad.AttomSyncStatus = "Success";
                propiedad.AttomSyncError = null;
                await scopedDb.SaveChangesAsync();

                scopedLogger.LogInformation("Background property enrichment saved for property {PropertyId}", propiedadId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Background property enrichment failed for property {PropertyId}", propiedadId);
            }
            finally
            {
                ActiveEnrichmentJobs.TryRemove(propiedadId, out _);
            }
        });
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

        if (!string.IsNullOrWhiteSpace(model.State))
        {
            propertyInfo.State = PropertyAdministratorCatalog.NormalizeUsStateCode(model.State.Trim())
                ?? model.State.Trim().ToUpperInvariant();
        }
        else if (!string.IsNullOrWhiteSpace(propertyInfo.State))
        {
            propertyInfo.State = PropertyAdministratorCatalog.NormalizeUsStateCode(propertyInfo.State)
                ?? propertyInfo.State.Trim().ToUpperInvariant();
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

    private static PropertyInfoViewModel? BuildPropertyInfoFromForm(AddPropertyViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.StreetAddress)
            || string.IsNullOrWhiteSpace(model.City)
            || string.IsNullOrWhiteSpace(model.State)
            || string.IsNullOrWhiteSpace(model.ZipCode))
        {
            return null;
        }

        return new PropertyInfoViewModel
        {
            FormattedAddress = model.BuildLookupAddress(),
            Street = model.StreetAddress.Trim(),
            City = model.City.Trim(),
            State = model.State.Trim().ToUpperInvariant(),
            PostalCode = model.ZipCode.Trim(),
            Country = "US",
            DataSource = "Estimated",
            PropertyDetails = new PropertyDetailsInfo()
        };
    }

    private static PropertyMaintenancePlanViewModel BuildPendingMaintenancePlan() =>
        new()
        {
            Summary = "Maintenance suggestions are being generated. Refresh this page in a moment.",
            DataSource = "Pending",
            IsAiGenerated = false,
            GeneratedUtc = DateTime.UtcNow,
            Items = []
        };

    private static PropertyMaintenancePlanViewModel BuildUnavailableMaintenancePlan(string message) =>
        new()
        {
            Summary = message,
            DataSource = "Unavailable",
            IsAiGenerated = false,
            GeneratedUtc = DateTime.UtcNow,
            Items = []
        };

    private void QueueMaintenanceGeneration(int propiedadId)
    {
        if (!ActiveMaintenanceJobs.TryAdd(propiedadId, 0))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var scopedMaintenance = scope.ServiceProvider.GetRequiredService<IOpenAiMaintenanceRecommendationService>();
                var scopedLogger = scope.ServiceProvider.GetRequiredService<ILogger<HomeownerPropertyService>>();

                var propiedad = await scopedDb.Propiedades.FirstOrDefaultAsync(p => p.Id == propiedadId && p.Activo);
                if (propiedad == null)
                {
                    return;
                }

                var propertyInfo = DeserializePropertyInfo(propiedad);
                if (propertyInfo == null)
                {
                    return;
                }

                using var cts = new CancellationTokenSource(MaintenanceTimeout);
                var plan = await scopedMaintenance.GenerateAsync(propertyInfo, cts.Token);
                if (!PropertyMaintenanceDisplayService.IsRealAiPlan(plan))
                {
                    scopedLogger.LogWarning(
                        "Deferred maintenance generation did not produce a plan for property {PropertyId}",
                        propiedadId);
                    return;
                }

                plan.GeneratedUtc ??= DateTime.UtcNow;
                propiedad.MantenimientoRecomendadoJson = JsonSerializer.Serialize(plan, JsonOptions);
                propiedad.MantenimientoRecomendadoUtc = plan.GeneratedUtc;
                await scopedDb.SaveChangesAsync();

                scopedLogger.LogInformation(
                    "Deferred maintenance plan saved for property {PropertyId} ({Count} items)",
                    propiedadId,
                    plan.Items.Count);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Deferred maintenance generation failed for property {PropertyId}", propiedadId);
            }
            finally
            {
                ActiveMaintenanceJobs.TryRemove(propiedadId, out _);
            }
        });
    }

    private static PropertyInfoViewModel? DeserializePropertyInfo(Propiedad propiedad)
    {
        if (string.IsNullOrWhiteSpace(propiedad.DatosJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyInfoViewModel>(propiedad.DatosJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

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
