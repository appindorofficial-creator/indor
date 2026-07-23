using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorStandardCleaningService
{
    PropertyAdministratorStandardCleaningFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorStandardCleaningFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorStandardCleaningSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorStandardCleaningConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorStandardCleaningService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorStandardCleaningService
{
    public PropertyAdministratorStandardCleaningFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("StandardCleaningDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorStandardCleaningFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorStandardCleaningFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            ScheduleTimeWindow = "11:00 AM",
            ContactPhone = ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorStandardCleaningSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var scheduleWhen = string.IsNullOrWhiteSpace(input.ScheduleWhen)
            ? "Tomorrow"
            : input.ScheduleWhen.Trim();
        var scheduleTimeWindow = PropertyAdministratorTimeSlots.Resolve(input.ScheduleTimeWindow);
        input.ScheduleWhen = scheduleWhen;
        input.ScheduleTimeWindow = scheduleTimeWindow;

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = scheduleWhen == "Today"
            ? DateTime.Today.AddHours(11)
            : DateTime.Today.AddDays(1).AddHours(11);
        var etaLabel = $"{LabelScheduleWhen(scheduleWhen)} • {scheduleTimeWindow}";

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", PropertyAdministratorDisplayLocalization.L("Standard Cleaning"), property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Cleaning",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = etaLabel,
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Maria R. • Cleaning"),
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType),
            DetailsJson = detailsJson,
            TechnicianName = "Maria R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Homecare Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Standard cleaning",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = scheduleTimeWindow,
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType)
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorStandardCleaningConfirmedViewModel?> GetConfirmedAsync(
        IUrlHelper url, int requestId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken);
        if (admin == null)
        {
            return null;
        }

        var request = admin.ServiceRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return null;
        }

        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = request.PortfolioPropertyId.HasValue
            ? admin.PortfolioProperties.FirstOrDefault(p => p.Id == request.PortfolioPropertyId.Value)
            : admin.PortfolioProperties.FirstOrDefault();

        var input = DeserializeInput(request.DetailsJson);

        return new PropertyAdministratorStandardCleaningConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Maria R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Homecare Pro",
            ScheduleLabel = request.EtaLabel ?? "Tomorrow • 11:00 AM",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Summary = BuildSummary(property, input),
            Timeline = BuildTimeline(request, input)
        };
    }

    private static PropertyAdministratorStandardCleaningSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorStandardCleaningSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorStandardCleaningSubmitInput>(json)
                ?? new PropertyAdministratorStandardCleaningSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorStandardCleaningSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        IndorPropertyAdminPortfolioProperty? property, PropertyAdministratorStandardCleaningSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = PropertyAdministratorDisplayLocalization.L("Standard Cleaning"), IconClass = "fa-spray-can-sparkles" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Time"), Value = $"{LabelScheduleWhen(input.ScheduleWhen)} {input.ScheduleTimeWindow}", IconClass = "fa-clock" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Scope"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-clipboard-list" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Includes"), Value = LabelIncludes(input.IncludedTasksList), IconClass = "fa-list-check" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-bell" }
    ];

    private static IReadOnlyList<PropertyAdministratorStandardCleaningTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request, PropertyAdministratorStandardCleaningSubmitInput input)
    {
        var submitted = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion);
        var assigned = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(1));
        var scheduled = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(3));
        var visitLabel = $"{LabelScheduleWhen(input.ScheduleWhen)} {input.ScheduleTimeWindow}";

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Crew assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled visit"), StatusLabel = scheduled, IconClass = "fa-calendar-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Cleaning visit"), StatusLabel = visitLabel, IconClass = "fa-spray-can-sparkles", State = "active" }
        ];
    }

    private static string LabelServiceType(string value) => value switch
    {
        "DeepTouchUp" => PropertyAdministratorDisplayLocalization.L("Deep touch-up"),
        "BeforeGuestArrival" => PropertyAdministratorDisplayLocalization.L("Before guest arrival"),
        "AfterGuestCheckout" => PropertyAdministratorDisplayLocalization.L("After guest checkout"),
        _ => PropertyAdministratorDisplayLocalization.L("Routine cleaning")
    };

    private static string LabelScheduleWhen(string value) => value switch
    {
        "Today" => PropertyAdministratorDisplayLocalization.L("Today"),
        "Later" => PropertyAdministratorDisplayLocalization.L("Later"),
        _ => PropertyAdministratorDisplayLocalization.L("Tomorrow")
    };

    private static string LabelIncludes(IReadOnlyList<string> tasks)
    {
        var labels = tasks.Select(t => t switch
        {
            "TrashRemoval" => PropertyAdministratorDisplayLocalization.L("trash removal"),
            "RestockSupplies" => PropertyAdministratorDisplayLocalization.L("restock supplies"),
            _ => t.ToLowerInvariant()
        }).ToList();

        return labels.Count == 0 ? "—" : string.Join(", ", labels);
    }

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "NeedApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => PropertyAdministratorDisplayLocalization.L("Smart lock code provided")
    };

    private static string LabelUpdates(IReadOnlyList<string> recipients)
    {
        var labels = recipients.Select(r => r switch
        {
            "Guest" => PropertyAdministratorDisplayLocalization.L("Guest"),
            "CoHost" => PropertyAdministratorDisplayLocalization.L("Co-host"),
            _ => PropertyAdministratorDisplayLocalization.L("Me")
        }).Distinct().ToList();

        return labels.Count switch
        {
            0 => PropertyAdministratorDisplayLocalization.L("Me"),
            1 => labels[0],
            _ => string.Join(" + ", labels)
        };
    }

    private async Task<IndorPropertyAdministrator?> LoadAdminAsync(
        bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var query = db.IndorPropertyAdministrators
            .Include(a => a.PortfolioProperties)
            .Include(a => a.ServiceRequests)
            .Where(a => a.UserId == userId && a.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed);

        return trackChanges
            ? await query.FirstOrDefaultAsync(cancellationToken)
            : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PropertyAdministratorPortalShellViewModel> BuildShellAsync(
        IndorPropertyAdministrator admin, CancellationToken cancellationToken)
    {
        var shell = PropertyAdministratorFlowServiceSupport.BuildShell(admin);

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await userManager.FindByIdAsync(userId);
            shell.ProfilePhotoUrl = user?.FotoUrl;
        }

        return shell;
    }

    private async Task<ApplicationUser?> GetUserAsync()
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        return string.IsNullOrEmpty(userId) ? null : await userManager.FindByIdAsync(userId);
    }

    private static IndorPropertyAdminPortfolioProperty? ResolveProperty(
        IndorPropertyAdministrator admin, int? propertyId) =>
        propertyId.HasValue
            ? admin.PortfolioProperties.FirstOrDefault(p => p.Id == propertyId.Value)
                ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            : admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault();

    private static PropertyAdministratorFlowPropertyViewModel MapProperty(IndorPropertyAdminPortfolioProperty? property)
    {
        if (property == null)
        {
            return new PropertyAdministratorFlowPropertyViewModel();
        }

        return new PropertyAdministratorFlowPropertyViewModel
        {
            Id = property.Id,
            PropertyName = property.PropertyName,
            Location = property.Location,
            PropertyTypeLabel = PropertyAdministratorDisplayLocalization.LabelPropertyType(property.PropertyType),
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType),
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}