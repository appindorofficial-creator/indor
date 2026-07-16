using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorPressureWashingService
{
    PropertyAdministratorPressureWashingFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorPressureWashingStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPressureWashingStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorPressureWashingStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorPressureWashingSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPressureWashingConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorPressureWashingService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPressureWashingService
{
    public PropertyAdministratorPressureWashingFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("PressureWashingDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorPressureWashingStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        return new PropertyAdministratorPressureWashingStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property)
        };
    }

    public async Task<PropertyAdministratorPressureWashingStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorPressureWashingStep1Input step1, CancellationToken cancellationToken = default)
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

        var isRental = property.PropertyType == "ShortTermRental";

        return new PropertyAdministratorPressureWashingStep2ViewModel
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
            WashAreas = string.Join(",", step1.WashAreasList),
            AreaSize = step1.AreaSize,
            ServiceReason = step1.ServiceReason,
            IsOccupied = step1.IsOccupied,
            GuestNotification = step1.GuestNotification,
            QuickNotes = step1.QuickNotes ?? "",
            ServiceTiming = "",
            EntryMethod = "",
            AccessNotes = ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorPressureWashingSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = input.ServiceTiming == "NextAvailable"
            ? DateTime.Today.AddDays(1).AddHours(11)
            : DateTime.Today.AddDays(1).AddHours(11);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", PropertyAdministratorDisplayLocalization.L("Pressure Washing"), property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "PressureWashing",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = LabelEta(input.ArrivalWindow),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("CleanWash Exterior Crew"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "CleanWash Exterior Crew",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Verified",
            VehicleLabel = LabelAreasShort(input.WashAreasList),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Pressure washing service",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = LabelTimeWindow(input.ArrivalWindow),
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorPressureWashingConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorPressureWashingConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "CleanWash Exterior Crew",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianReviewCount = 128,
            TechnicianTitle = request.TechnicianTitle ?? "Verified",
            TechnicianSubtitle = "Specialists in exterior cleaning",
            EtaLabel = request.EtaLabel ?? "Tomorrow, 11:00 AM – 1:00 PM",
            ServiceAreasLabel = LabelAreasShort(input.WashAreasList),
            EstimatedTotal = EstimateTotal(input.AreaSize),
            Summary = BuildSummary(input, property),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorPressureWashingSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorPressureWashingSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorPressureWashingSubmitInput>(json)
                ?? new PropertyAdministratorPressureWashingSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorPressureWashingSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorPressureWashingSummaryItemViewModel> BuildSummary(
        PropertyAdministratorPressureWashingSubmitInput input,
        IndorPropertyAdminPortfolioProperty? property) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = "Pressure washing", IconClass = "fa-spray-can" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Areas"), Value = LabelAreas(input.WashAreasList), IconClass = "fa-border-all" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Size"), Value = LabelAreaSize(input.AreaSize), IconClass = "fa-ruler-combined" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Reason"), Value = LabelReason(input.ServiceReason), IconClass = "fa-broom" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Estimated total"), Value = EstimateTotal(input.AreaSize), IconClass = "fa-dollar-sign" }
    ];

    private static IReadOnlyList<PropertyAdministratorPressureWashingTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(2);
        var scheduled = submitted.AddMinutes(3);

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = $"Today, {submitted:h:mm tt}", IconClass = "fa-circle-check", State = "done", StepNumber = 1 },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Crew assigned"), StatusLabel = $"Today, {assigned:h:mm tt}", IconClass = "fa-users", State = "done", StepNumber = 2 },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled"), StatusLabel = $"Today, {scheduled:h:mm tt}", IconClass = "fa-calendar-day", State = "done", StepNumber = 3 },
            new() { Label = PropertyAdministratorDisplayLocalization.L("On the way"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Tomorrow"), IconClass = "fa-truck", State = "pending", StepNumber = 4 }
        ];
    }

    private static string LabelAreas(IReadOnlyList<string> areas) =>
        areas.Count == 0 ? "—" : string.Join(", ", areas.Select(LabelArea));

    private static string LabelAreasShort(IReadOnlyList<string> areas) =>
        areas.Count == 0 ? "Exterior surfaces" : string.Join(" + ", areas.Select(LabelArea));

    private static string LabelArea(string value) => value switch
    {
        "Driveway" => PropertyAdministratorDisplayLocalization.L("Driveway"),
        "Walkway" => PropertyAdministratorDisplayLocalization.L("Walkway"),
        "Patio" => PropertyAdministratorDisplayLocalization.L("Patio"),
        "ExteriorWalls" => PropertyAdministratorDisplayLocalization.L("Exterior walls"),
        "Fence" => PropertyAdministratorDisplayLocalization.L("Fence"),
        "PoolDeck" => PropertyAdministratorDisplayLocalization.L("Pool deck"),
        "TrashArea" => PropertyAdministratorDisplayLocalization.L("Trash area"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => value
    };

    private static string LabelAreaSize(string value) => value switch
    {
        "Small" => PropertyAdministratorDisplayLocalization.L("Small (up to 500 sq ft)"),
        "Large" => PropertyAdministratorDisplayLocalization.L("Large (over 2,000 sq ft)"),
        _ => PropertyAdministratorDisplayLocalization.L("Medium (500 – 2,000 sq ft)")
    };

    private static string LabelReason(string value) => value switch
    {
        "RoutineUpkeep" => PropertyAdministratorDisplayLocalization.L("Routine upkeep"),
        "HeavyDirtStains" => PropertyAdministratorDisplayLocalization.L("Heavy dirt / stains"),
        "HoaCurbAppeal" => PropertyAdministratorDisplayLocalization.L("HOA / curb appeal"),
        _ => PropertyAdministratorDisplayLocalization.L("Guest turnover")
    };

    private static string LabelAccess(PropertyAdministratorPressureWashingSubmitInput input) =>
        input.EntryMethod switch
        {
            "HostOnSite" => PropertyAdministratorDisplayLocalization.L("Host on-site"),
            "GuestAware" => PropertyAdministratorDisplayLocalization.L("Guest aware"),
            _ => string.IsNullOrWhiteSpace(input.AccessNotes) ? "Gate code provided" : "Gate code provided"
        };

    private static string LabelUpdates(IReadOnlyList<string> recipients)
    {
        var labels = recipients.Select(r => r switch
        {
            "Guest" => PropertyAdministratorDisplayLocalization.L("Guest"),
            "CoHost" => PropertyAdministratorDisplayLocalization.L("Co-host"),
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

    private static string LabelEta(string arrivalWindow) => arrivalWindow switch
    {
        "Morning" => PropertyAdministratorDisplayLocalization.L("Tomorrow, 8:00 AM – 12:00 PM"),
        "Afternoon" => PropertyAdministratorDisplayLocalization.L("Tomorrow, 2:00 PM – 5:00 PM"),
        _ => PropertyAdministratorDisplayLocalization.L("Tomorrow, 11:00 AM – 1:00 PM")
    };

    private static string LabelTimeWindow(string value) => value switch
    {
        "Morning" => PropertyAdministratorDisplayLocalization.L("8 AM – 12 PM"),
        "Afternoon" => PropertyAdministratorDisplayLocalization.L("2 PM – 5 PM"),
        _ => PropertyAdministratorDisplayLocalization.L("11 AM – 1 PM")
    };

    private static string EstimateTotal(string areaSize) => areaSize switch
    {
        "Small" => "$95 – $145",
        "Large" => "$220 – $350",
        _ => "$145 – $220"
    };

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