using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorEmergencyElectricalService
{
    PropertyAdministratorEmergencyFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    PropertyAdministratorNearestProViewModel BuildNearestPro();
    Task<PropertyAdministratorEmergencyElectricalFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyElectricalReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyElectricalSubmitInput input, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorEmergencyElectricalSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyElectricalConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorEmergencyElectricalService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorEmergencyElectricalService
{
    public PropertyAdministratorEmergencyFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("EmergencyElectricalDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public PropertyAdministratorNearestProViewModel BuildNearestPro() => new();

    public async Task<PropertyAdministratorEmergencyElectricalFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorEmergencyElectricalFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            ContactPhone = ""
        };
    }

    public async Task<PropertyAdministratorEmergencyElectricalReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyElectricalSubmitInput input, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorEmergencyElectricalReviewViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            Input = input,
            SummaryRows = BuildReviewRows(input, mapped)
        };
    }

    public static bool IsSubmitComplete(PropertyAdministratorEmergencyElectricalSubmitInput input) =>
        !string.IsNullOrWhiteSpace(input.IssueType)
        && !string.IsNullOrWhiteSpace(input.PowerFullyOut)
        && !string.IsNullOrWhiteSpace(input.GuestsInside)
        && !string.IsNullOrWhiteSpace(input.Urgency)
        && !string.IsNullOrWhiteSpace(input.ProblemLocation)
        && !string.IsNullOrWhiteSpace(input.EntryAccess)
        && PropertyAdministratorContactPhone.IsProvided(input.ContactPhone);

    public async Task<int> SubmitAsync(
        PropertyAdministratorEmergencyElectricalSubmitInput input, CancellationToken cancellationToken = default)
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
            Title = $"{LabelIssue(input.IssueType)} • {LabelLocation(input.ProblemLocation)}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = null,
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Matching a verified electrician"),
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType),
            DetailsJson = detailsJson,
            TimelineStep = 1
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorEmergencyElectricalConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorEmergencyElectricalConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "",
            TechnicianRating = request.TechnicianRating ?? 0m,
            TechnicianTitle = request.TechnicianTitle ?? "",
            EtaLabel = request.EtaLabel ?? "",
            VehicleLabel = request.VehicleLabel ?? "",
            Timeline = BuildTimeline(request),
            Summary = BuildSummary(input)
        };
    }

    private static PropertyAdministratorEmergencyElectricalSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorEmergencyElectricalSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorEmergencyElectricalSubmitInput>(json)
                ?? new PropertyAdministratorEmergencyElectricalSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorEmergencyElectricalSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalReviewRowViewModel> BuildReviewRows(
        PropertyAdministratorEmergencyElectricalSubmitInput input,
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
            Value = $"{LabelIssue(input.IssueType)} / {LabelLocation(input.ProblemLocation)}",
            IconClass = "fa-bolt"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Power fully out"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.PowerFullyOut),
            IconClass = "fa-plug-circle-xmark",
            Highlight = input.PowerFullyOut == "Yes"
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
            Label = PropertyAdministratorDisplayLocalization.L("Urgency"),
            Value = LabelUrgency(input.Urgency),
            IconClass = "fa-circle-exclamation",
            Highlight = input.Urgency == "Emergency"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Access"),
            Value = LabelAccess(input.EntryAccess),
            IconClass = "fa-key"
        }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        PropertyAdministratorEmergencyElectricalSubmitInput input) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Issue"),
            Value = $"{LabelIssue(input.IssueType)} / {LabelLocation(input.ProblemLocation)}",
            IconClass = "fa-triangle-exclamation"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Power fully out"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.PowerFullyOut),
            IconClass = "fa-user",
            Highlight = input.PowerFullyOut == "Yes"
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
            Label = PropertyAdministratorDisplayLocalization.L("Access"),
            Value = LabelAccess(input.EntryAccess),
            IconClass = "fa-key"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Updates"),
            Value = PropertyAdministratorDisplayLocalization.L("Me + Guest"),
            IconClass = "fa-bell"
        }
    ];

    private static string LabelUrgency(string value) => value switch
    {
        "Urgent" => PropertyAdministratorDisplayLocalization.L("Urgent"),
        "Emergency" => PropertyAdministratorDisplayLocalization.L("Emergency"),
        _ => PropertyAdministratorDisplayLocalization.L("Normal")
    };

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion);
        var assigned = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(5));
        var enRoute = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(8));
        var step = request.TimelineStep;

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("—"), IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Diagnosis"), StatusLabel = PropertyAdministratorDisplayLocalization.L("—"), IconClass = "fa-clipboard-list", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelIssue(string value) => value switch
    {
        "BreakerTripping" => PropertyAdministratorDisplayLocalization.L("Breaker keeps tripping"),
        "BurningSmell" => PropertyAdministratorDisplayLocalization.L("Burning smell"),
        "SparksOutlet" => PropertyAdministratorDisplayLocalization.L("Sparks / outlet issue"),
        "PartialPower" => PropertyAdministratorDisplayLocalization.L("Partial power"),
        "ExposedWire" => PropertyAdministratorDisplayLocalization.L("Exposed wire"),
        _ => PropertyAdministratorDisplayLocalization.L("Power outage")
    };

    private static string LabelLocation(string value) => value switch
    {
        "Kitchen" => PropertyAdministratorDisplayLocalization.L("Kitchen"),
        "Bedroom" => PropertyAdministratorDisplayLocalization.L("Bedroom"),
        "Bathroom" => PropertyAdministratorDisplayLocalization.L("Bathroom"),
        "PanelBreaker" => PropertyAdministratorDisplayLocalization.L("Panel / breaker box"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => PropertyAdministratorDisplayLocalization.L("Living room")
    };

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "GuestApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => PropertyAdministratorDisplayLocalization.L("Smart lock code provided")
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
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType),
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}