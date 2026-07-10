using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorPoolHotTubService
{
    PropertyAdministratorPoolHotTubFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorPoolHotTubStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPoolHotTubStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorPoolHotTubStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorPoolHotTubSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPoolHotTubConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorPoolHotTubService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPoolHotTubService
{
    public PropertyAdministratorPoolHotTubFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("PoolHotTubDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorPoolHotTubStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var isRental = property?.PropertyType == "ShortTermRental";

        return new PropertyAdministratorPoolHotTubStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property),
            ServiceHelpType = isRental ? "HotTubRepair" : "PoolRepair",
            MainIssue = isRental ? "HeaterIssue" : "PumpNotWorking",
            GuestStayAffected = isRental ? "Yes" : "No",
            Urgency = isRental ? "Urgent" : "Routine",
            QuickDetails = isRental
                ? "Guests say the hot tub is not heating and the water is getting cooler."
                : ""
        };
    }

    public async Task<PropertyAdministratorPoolHotTubStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorPoolHotTubStep1Input step1, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken);
        if (admin == null)
        {
            return null;
        }

        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == step1.PropertyId)
            ?? ResolveProperty(admin, step1.PropertyId);
        if (property == null)
        {
            return null;
        }

        var user = await GetUserAsync();
        var isRental = property.PropertyType == "ShortTermRental";
        var etaMinutes = step1.Urgency == "Emergency" ? 20 : step1.Urgency == "Urgent" ? 27 : 90;

        return new PropertyAdministratorPoolHotTubStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = isRental ? PropertyAdministratorDisplayLocalization.L("Occupied now") : null,
            ServiceHelpType = step1.ServiceHelpType,
            MainIssue = step1.MainIssue,
            GuestStayAffected = step1.GuestStayAffected,
            Urgency = step1.Urgency,
            QuickDetails = step1.QuickDetails ?? "",
            Step1Summary = BuildStep1SummaryChips(step1),
            EquipmentLocation = step1.ServiceHelpType is "HotTubRepair" or "SpaHotTubService"
                ? "BackyardSpa"
                : "PoolPad",
            EntryAccess = "GateCode",
            AccessCode = $"Gate code: {2840 + property.Id}",
            UpdateRecipients = isRental ? "Me,Guest" : "Me",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "(704) 555-0132",
            ProEtaLabel = $"Nearest pool & spa pro available in {etaMinutes} minutes",
            DiagnosticEstimate = "$129 – $169",
            EmergencyFeeLabel = step1.Urgency is "Urgent" or "Emergency" ? "Included" : "Not applicable"
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorPoolHotTubSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var etaMinutes = input.Urgency == "Emergency" ? 20 : input.Urgency == "Urgent" ? 27 : 90;
        var visitDate = DateTime.Now.AddMinutes(etaMinutes);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = $"Pool & Hot Tub at {property.PropertyName}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = input.Urgency is "Urgent" or "Emergency"
                ? PropertyAdministratorRequestStatuses.InProgress
                : PropertyAdministratorRequestStatuses.Open,
            Category = "PoolHotTub",
            ScheduledUtc = visitDate,
            IsEmergency = input.Urgency == "Emergency",
            EtaLabel = $"{etaMinutes} min",
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Marco R. • Pool & Spa"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Marco R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Pool & Spa Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Pool & hot tub repair",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = "ASAP",
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorPoolHotTubConfirmedViewModel?> GetConfirmedAsync(
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
        var isRental = property?.PropertyType == "ShortTermRental";

        return new PropertyAdministratorPoolHotTubConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            PropertyStatusLabel = isRental ? PropertyAdministratorDisplayLocalization.L("Occupied now") : null,
            TechnicianName = request.TechnicianName ?? "Marco R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Pool & Spa Pro",
            EtaLabel = request.EtaLabel ?? "27 min",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Summary = BuildSummary(input, property),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorPoolHotTubSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorPoolHotTubSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorPoolHotTubSubmitInput>(json)
                ?? new PropertyAdministratorPoolHotTubSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorPoolHotTubSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorPoolHotTubSummaryChipViewModel> BuildStep1SummaryChips(
        PropertyAdministratorPoolHotTubStep1Input input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service type"), Value = LabelServiceHelpType(input.ServiceHelpType), IconClass = "fa-hot-tub-person" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Main issue"), Value = LabelMainIssue(input.MainIssue), IconClass = "fa-temperature-high" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Urgency"), Value = LabelUrgency(input.Urgency), IconClass = "fa-clock" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Guest stay affected"), Value = input.GuestStayAffected == "Yes" ? "Yes" : "No", IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorPoolHotTubSummaryItemViewModel> BuildSummary(
        PropertyAdministratorPoolHotTubSubmitInput input,
        IndorPropertyAdminPortfolioProperty? property) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceHelpType(input.ServiceHelpType), IconClass = "fa-water-ladder" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Issue"), Value = LabelMainIssue(input.MainIssue), IconClass = "fa-wrench" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Equipment"), Value = LabelEquipmentLocation(input.EquipmentLocation), IconClass = "fa-location-dot" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelEntryAccess(input), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Diagnostic estimate"), Value = "$129 – $169", IconClass = "fa-dollar-sign" }
    ];

    private static IReadOnlyList<PropertyAdministratorPoolHotTubTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(1);
        var enRoute = submitted.AddMinutes(2);

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = $"Today, {submitted:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = $"Today, {assigned:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = $"Today, {enRoute:h:mm tt}", IconClass = "fa-truck", State = "active" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Upcoming"), IconClass = "fa-house", State = "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Diagnosis"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Upcoming"), IconClass = "fa-clipboard-list", State = "pending" }
        ];
    }

    private static string LabelServiceHelpType(string value) => value switch
    {
        "PoolRepair" => "Pool repair",
        "RoutinePoolService" => "Routine pool service",
        "SpaHotTubService" => "Spa / hot tub service",
        _ => "Hot tub repair"
    };

    private static string LabelMainIssue(string value) => value switch
    {
        "PumpNotWorking" => "Pump not working",
        "WaterLeak" => "Water leak",
        "CloudyDirtyWater" => "Cloudy / dirty water",
        "JetsNotWorking" => "Jets not working",
        "FilterProblem" => "Filter problem",
        _ => "Heater issue"
    };

    private static string LabelUrgency(string value) => value switch
    {
        "Emergency" => "Emergency",
        "Routine" => "Routine",
        _ => "Urgent"
    };

    private static string LabelEquipmentLocation(string value) => value switch
    {
        "PoolPad" => "Pool pad",
        "EquipmentRoom" => "Equipment room",
        "Other" => "Other",
        _ => "Backyard spa"
    };

    private static string LabelEntryAccess(PropertyAdministratorPoolHotTubSubmitInput input) =>
        input.EntryAccess switch
        {
            "HostOnSite" => "Host will meet",
            "GuestApproval" => "Need guest approval",
            _ => string.IsNullOrWhiteSpace(input.AccessCode) ? "Gate code provided" : input.AccessCode
        };

    private static string LabelUpdates(IReadOnlyList<string> recipients)
    {
        var labels = recipients.Select(r => r switch
        {
            "Guest" => "Guest",
            "CoHost" => "Co-host",
            _ => "Me"
        }).Distinct().ToList();

        return labels.Count switch
        {
            0 => "Me",
            1 => labels[0],
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels)
        };
    }

    private async Task<ApplicationUser?> GetUserAsync()
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        return string.IsNullOrEmpty(userId) ? null : await userManager.FindByIdAsync(userId);
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
            ImageUrl = property.ImageUrl,
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}
