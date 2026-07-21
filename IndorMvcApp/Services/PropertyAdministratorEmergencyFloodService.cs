using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorEmergencyFloodService
{
    PropertyAdministratorEmergencyFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    PropertyAdministratorNearestProViewModel BuildNearestPro();
    Task<PropertyAdministratorEmergencyFloodFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyFloodReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyFloodSubmitInput input, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorEmergencyFloodSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyFloodConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorEmergencyFloodService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorEmergencyFloodService
{
    public PropertyAdministratorEmergencyFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            Title = "Emergency Flood",
            Subtitle = "Fast help for major water leaks, flooding, and water damage.",
            IconClass = "fa-water",
            PriorityBadge = "",
            StartUrl = url.Action("EmergencyFloodDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public PropertyAdministratorNearestProViewModel BuildNearestPro() =>
        new()
        {
            ProTypeLabel = PropertyAdministratorDisplayLocalization.L("water mitigation"),
            EtaMinutes = "19",
            TrustLabel = PropertyAdministratorDisplayLocalization.L("Licensed • Insured • IICRC-ready")
        };

    public async Task<PropertyAdministratorEmergencyFloodFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorEmergencyFloodFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            ContactPhone = "",
            ProEtaLabel = PropertyAdministratorDisplayLocalization.NearestProAvailableInMinutes("water mitigation", 19)
        };
    }

    public async Task<PropertyAdministratorEmergencyFloodReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyFloodSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken);
        if (admin == null)
        {
            return null;
        }

        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? ResolveProperty(admin, input.PropertyId);
        if (property == null)
        {
            return null;
        }

        var mapped = MapProperty(property);
        input.PropertyId = property.Id;

        return new PropertyAdministratorEmergencyFloodReviewViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            Input = input,
            SummaryRows = BuildReviewSummaryRows(input, mapped),
            ProEtaLabel = PropertyAdministratorDisplayLocalization.NearestProAvailableInMinutes("water mitigation", 19)
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorEmergencyFloodSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var now = DateTime.UtcNow;
        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = $"{LabelProblem(input.ProblemType)} • {LabelLocation(input.WaterLocation)}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("19 min"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Alex M. • Mitigation"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Alex M.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Water Mitigation Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorEmergencyFloodConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorEmergencyFloodConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Alex M.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Water Mitigation Pro",
            EtaLabel = request.EtaLabel ?? "19 min",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            ShowInsuranceBanner = input.ProcessThroughInsurance == "Yes",
            Timeline = BuildTimeline(request),
            Summary = BuildSummary(input)
        };
    }

    private static PropertyAdministratorEmergencyFloodSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorEmergencyFloodSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorEmergencyFloodSubmitInput>(json)
                ?? new PropertyAdministratorEmergencyFloodSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorEmergencyFloodSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyFloodReviewRowViewModel> BuildReviewSummaryRows(
        PropertyAdministratorEmergencyFloodSubmitInput input,
        PropertyAdministratorFlowPropertyViewModel property) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Property"),
            Value = PropertyAdministratorDisplayLocalization.T("Viewing: {0}", property.PropertyName),
            IconClass = "fa-house"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Issue"),
            Value = $"{LabelProblem(input.ProblemType)} / {LabelLocation(input.WaterLocation)}",
            IconClass = "fa-droplet"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Is water actively coming in right now?"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.WaterActivelyComingIn),
            IconClass = "fa-water",
            Highlight = input.WaterActivelyComingIn == "Yes"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Guests inside"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.GuestsInside),
            IconClass = "fa-users",
            Highlight = input.GuestsInside == "Yes"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("How urgent is it?"),
            Value = LabelUrgency(input.Urgency),
            IconClass = "fa-triangle-exclamation",
            Highlight = input.Urgency == "Emergency"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Location"),
            Value = LabelLocation(input.WaterLocation),
            IconClass = "fa-location-dot"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Access"),
            Value = LabelAccess(input.EntryAccess),
            IconClass = "fa-key"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Insurance"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.ProcessThroughInsurance),
            IconClass = "fa-shield-halved"
        }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        PropertyAdministratorEmergencyFloodSubmitInput input) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Issue"),
            Value = $"{LabelProblem(input.ProblemType)} / {LabelLocation(input.WaterLocation).ToLowerInvariant()} leak",
            IconClass = "fa-droplet"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Occupied"),
            Value = input.WaterActivelyComingIn == "Yes" ? "Yes" : "No",
            IconClass = "fa-user",
            Highlight = input.GuestsInside == "Yes"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Guests inside"),
            Value = input.GuestsInside,
            IconClass = "fa-users",
            Highlight = input.GuestsInside == "Yes"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Location"),
            Value = LabelLocation(input.WaterLocation),
            IconClass = "fa-location-dot"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Access"),
            Value = LabelAccess(input.EntryAccess),
            IconClass = "fa-key"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Insurance"),
            Value = input.ProcessThroughInsurance,
            IconClass = "fa-shield-halved"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Claim opened"),
            Value = input.ClaimOpened,
            IconClass = "fa-file-lines"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Docs"),
            Value = input.ProcessThroughInsurance == "Yes" ? "Policy info requested" : "—",
            IconClass = "fa-cloud-arrow-up"
        }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime().ToString("h:mm tt");
        var assigned = request.FechaCreacion.AddMinutes(1).ToLocalTime().ToString("h:mm tt");
        var enRoute = request.FechaCreacion.AddMinutes(3).ToLocalTime().ToString("h:mm tt");
        var step = request.TimelineStep;

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Water extraction"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-water", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelProblem(string value) => value switch
    {
        "BurstPipe" => PropertyAdministratorDisplayLocalization.L("Burst pipe"),
        "RoofLeak" => PropertyAdministratorDisplayLocalization.L("Roof leak"),
        "ApplianceLeak" => PropertyAdministratorDisplayLocalization.L("Appliance leak"),
        "SewageBackup" => PropertyAdministratorDisplayLocalization.L("Sewage backup"),
        "Overflow" => PropertyAdministratorDisplayLocalization.L("Overflow"),
        _ => PropertyAdministratorDisplayLocalization.L("Active flooding")
    };

    private static string LabelLocation(string value) => value switch
    {
        "Bathroom" => PropertyAdministratorDisplayLocalization.L("Bathroom"),
        "Kitchen" => PropertyAdministratorDisplayLocalization.L("Kitchen"),
        "Laundry" => PropertyAdministratorDisplayLocalization.L("Laundry"),
        "Ceiling" => PropertyAdministratorDisplayLocalization.L("Ceiling"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => PropertyAdministratorDisplayLocalization.L("Living room")
    };

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "GuestApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => PropertyAdministratorDisplayLocalization.L("Smart lock code provided")
    };

    private static string LabelUrgency(string value) => value switch
    {
        "Urgent" => PropertyAdministratorDisplayLocalization.L("Urgent"),
        "Soon" => PropertyAdministratorDisplayLocalization.L("Soon"),
        "NotUrgent" => PropertyAdministratorDisplayLocalization.L("Not urgent"),
        _ => PropertyAdministratorDisplayLocalization.L("Emergency")
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
            ImageUrl = property.ImageUrl,
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}