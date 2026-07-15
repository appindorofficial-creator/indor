using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorLandscapingService
{
    PropertyAdministratorLandscapingFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorLandscapingStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorLandscapingStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorLandscapingStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorLandscapingSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorLandscapingConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorLandscapingService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorLandscapingService
{
    public PropertyAdministratorLandscapingFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("LandscapingDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorLandscapingStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);

        return new PropertyAdministratorLandscapingStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property?.PropertyType == "ShortTermRental" ? "Guest check-in this week" : null
        };
    }

    public async Task<PropertyAdministratorLandscapingStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorLandscapingStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorLandscapingStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property.PropertyType == "ShortTermRental" ? "Guest check-in this week" : null,
            ServiceType = step1.ServiceType,
            WorkArea = step1.WorkArea,
            ServiceReason = step1.ServiceReason,
            Timeline = step1.Timeline,
            IsOccupied = step1.IsOccupied,
            QuickNotes = step1.QuickNotes ?? "",
            HaulAwayType = "",
            ProjectNotes = step1.QuickNotes ?? ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorLandscapingSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = input.PreferredDate == "Tomorrow"
            ? DateTime.Today.AddDays(1).AddHours(10)
            : DateTime.Today.AddDays(3).AddHours(10);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", "Landscaping", property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Landscaping",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("Tomorrow, 10:00 AM"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Marco R. • Landscaping"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Marco R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Verified landscaping pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("Green service truck"),
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Landscaping consultation",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = LabelTimeWindow(input.TimeWindow),
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorLandscapingConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorLandscapingConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Marco R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianReviewCount = 128,
            TechnicianTitle = request.TechnicianTitle ?? "Verified landscaping pro",
            ConsultationLabel = request.EtaLabel ?? "Tomorrow, 10:00 AM",
            VehicleLabel = request.VehicleLabel ?? "Green service truck",
            Summary = BuildSummary(input),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorLandscapingSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorLandscapingSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorLandscapingSubmitInput>(json)
                ?? new PropertyAdministratorLandscapingSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorLandscapingSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorLandscapingSummaryItemViewModel> BuildSummary(
        PropertyAdministratorLandscapingSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelService(input), IconClass = "fa-leaf" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Area"), Value = LabelArea(input.WorkArea), IconClass = "fa-tree" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Reason"), Value = LabelReason(input.ServiceReason), IconClass = "fa-car" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Timing"), Value = LabelTimingRequirement(input), IconClass = "fa-clock" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Materials"), Value = input.ProvideMaterials == "Yes" ? "Plants/materials provided by pro" : "Owner provides materials", IconClass = "fa-seedling" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Haul away"), Value = LabelHaulAway(input.HaulAwayType), IconClass = "fa-truck" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorLandscapingTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime();

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request received"), StatusLabel = $"Today, {submitted:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Consultation scheduled"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Tomorrow, 10:00 AM"), IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Pro on the way"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-truck", State = "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Estimate / scope of work"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-file-lines", State = "pending" }
        ];
    }

    private static string LabelService(PropertyAdministratorLandscapingSubmitInput input)
    {
        var baseLabel = input.ServiceType switch
        {
            "ReplaceDamagedPlants" => PropertyAdministratorDisplayLocalization.L("Plant replacement"),
            "TreeShrubInstall" => PropertyAdministratorDisplayLocalization.L("Tree / shrub install"),
            "PatioHardscape" => PropertyAdministratorDisplayLocalization.L("Patio / hardscape"),
            _ => PropertyAdministratorDisplayLocalization.L("Garden refresh")
        };

        return input.ServiceType == "GardenRefresh" && input.ServiceReason == "CarDroveOverPlants"
            ? $"{baseLabel} + plant replacement"
            : baseLabel;
    }

    private static string LabelArea(string value) => value switch
    {
        "Backyard" => PropertyAdministratorDisplayLocalization.L("Backyard"),
        "Both" => PropertyAdministratorDisplayLocalization.L("Front yard + backyard"),
        _ => PropertyAdministratorDisplayLocalization.L("Front yard")
    };

    private static string LabelReason(string value) => value switch
    {
        "StormDamage" => PropertyAdministratorDisplayLocalization.L("Storm damage"),
        "CarDroveOverPlants" => PropertyAdministratorDisplayLocalization.L("Car drove over plants"),
        "PreparingForGuests" => PropertyAdministratorDisplayLocalization.L("Preparing for guests"),
        _ => PropertyAdministratorDisplayLocalization.L("Routine upgrade")
    };

    private static string LabelTimingRequirement(PropertyAdministratorLandscapingSubmitInput input) =>
        input.Timeline == "Asap" || input.PreferredDate == "Tomorrow"
            ? "Before next guest arrival"
            : LabelTimeline(input.Timeline);

    private static string LabelTimeline(string value) => value switch
    {
        "Asap" => PropertyAdministratorDisplayLocalization.L("As soon as possible"),
        "ScheduleLater" => PropertyAdministratorDisplayLocalization.L("Schedule later"),
        _ => PropertyAdministratorDisplayLocalization.L("This week")
    };

    private static string LabelHaulAway(string value) => value switch
    {
        "RemoveTreeDebris" => PropertyAdministratorDisplayLocalization.L("Tree debris removed"),
        "RemoveOldMulch" => PropertyAdministratorDisplayLocalization.L("Old mulch removed"),
        "NoHaulAway" => PropertyAdministratorDisplayLocalization.L("No haul away"),
        _ => PropertyAdministratorDisplayLocalization.L("Damaged plants removed")
    };

    private static string LabelTimeWindow(string value) => value switch
    {
        "Afternoon" => PropertyAdministratorDisplayLocalization.L("12 PM – 4 PM"),
        "AnyTime" => PropertyAdministratorDisplayLocalization.L("Any time"),
        _ => PropertyAdministratorDisplayLocalization.L("8 AM – 12 PM")
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