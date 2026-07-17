using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorEmergencyTreeBranchService
{
    Task<PropertyAdministratorEmergencyTreeBranchStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyTreeBranchReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyTreeBranchStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorEmergencyTreeBranchSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyTreeBranchConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorEmergencyTreeBranchService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorEmergencyTreeBranchService
{
    public async Task<PropertyAdministratorEmergencyTreeBranchStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorEmergencyTreeBranchStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped
        };
    }

    public async Task<PropertyAdministratorEmergencyTreeBranchReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyTreeBranchStep1Input step1, CancellationToken cancellationToken = default)
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
        var mapped = MapProperty(property);
        var input = new PropertyAdministratorEmergencyTreeBranchSubmitInput
        {
            PropertyId = property.Id,
            IssueType = step1.IssueType,
            ImmediateDanger = step1.ImmediateDanger,
            GuestsInside = step1.GuestsInside,
            Urgency = step1.Urgency,
            DamageAreasList = step1.DamageAreasList,
            TarpNeeded = step1.TarpNeeded,
            QuickDetails = step1.QuickDetails,
            EntryAccess = "ExteriorOnly",
            GateParkingNotes = "Use front drive",
            UpdateRecipientsList = ["Me", "Guest"],
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "",
            InsuranceHelp = "NeedDocumentation"
        };

        return new PropertyAdministratorEmergencyTreeBranchReviewViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            GuestsOnSiteLabel = step1.GuestsInside == "Yes"
                ? PropertyAdministratorDisplayLocalization.L("Guests on-site: Yes")
                : null,
            Input = input,
            RequestSummaryRows = BuildRequestSummaryRows(input, mapped),
            AccessContactRows = BuildAccessContactRows(input)
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorEmergencyTreeBranchSubmitInput input, CancellationToken cancellationToken = default)
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
            Title = $"{LabelIssue(input.IssueType)} • {LabelDamageAreas(input.DamageAreasList)}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("28 min"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Miguel R. • Tree service"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Miguel R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Tree Service Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("Utility truck"),
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorEmergencyTreeBranchConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorEmergencyTreeBranchConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Miguel R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Tree Service Pro",
            EtaLabel = request.EtaLabel ?? "28 min",
            VehicleLabel = request.VehicleLabel ?? "Utility truck",
            Timeline = BuildTimeline(request)
        };
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyTreeBranchReviewRowViewModel> BuildRequestSummaryRows(
        PropertyAdministratorEmergencyTreeBranchSubmitInput input,
        PropertyAdministratorFlowPropertyViewModel property) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Property"),
            Value = PropertyAdministratorDisplayLocalization.T("Viewing: {0}", property.PropertyName),
            IconClass = "fa-house"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Issue"), Value = LabelIssue(input.IssueType), IconClass = "fa-tree" },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Guests inside"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.GuestsInside),
            IconClass = "fa-users"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Danger level"),
            Value = LabelUrgency(input.Urgency),
            IconClass = "fa-circle-exclamation",
            IsDangerBadge = input.Urgency == "Emergency"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Damage area"), Value = LabelDamageAreas(input.DamageAreasList), IconClass = "fa-location-dot" },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Tarp needed"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.TarpNeeded),
            IconClass = "fa-umbrella"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Power line involvement"),
            Value = PropertyAdministratorFlowServiceSupport.YesNo(input.IssueType == "NearPowerLine" ? "Yes" : "No"),
            IconClass = "fa-bolt"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Notes"),
            Value = string.IsNullOrWhiteSpace(input.QuickDetails) ? "—" : input.QuickDetails,
            IconClass = "fa-note-sticky"
        }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyTreeBranchReviewRowViewModel> BuildAccessContactRows(
        PropertyAdministratorEmergencyTreeBranchSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Entry access"), Value = LabelEntryAccess(input.EntryAccess), IconClass = "fa-door-open" },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Gate / parking notes"),
            Value = LabelGateParkingNotes(input.GateParkingNotes),
            IconClass = "fa-road"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Who gets updates"),
            Value = FormatRecipients(input.UpdateRecipientsList),
            IconClass = "fa-bell"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Best contact"), Value = input.ContactPhone, IconClass = "fa-phone" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Insurance help"), Value = LabelInsuranceHelp(input.InsuranceHelp), IconClass = "fa-shield-halved" }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion);
        var assigned = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(4));
        var enRoute = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(6));
        var step = request.TimelineStep;

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Crew assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Hazard removed"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-tree", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelIssue(string value) => value switch
    {
        "TreeFellOnHouse" => PropertyAdministratorDisplayLocalization.L("Tree fell on house"),
        "DrivewayBlocked" => PropertyAdministratorDisplayLocalization.L("Driveway blocked"),
        "FenceDamage" => PropertyAdministratorDisplayLocalization.L("Fence damage"),
        "RoofImpact" => PropertyAdministratorDisplayLocalization.L("Roof impact"),
        "NearPowerLine" => PropertyAdministratorDisplayLocalization.L("Near power line"),
        _ => PropertyAdministratorDisplayLocalization.L("Large hanging branch")
    };

    private static string LabelDamageArea(string value) => value switch
    {
        "FrontYard" => PropertyAdministratorDisplayLocalization.L("Front yard"),
        "Backyard" => PropertyAdministratorDisplayLocalization.L("Backyard"),
        "Roof" => PropertyAdministratorDisplayLocalization.L("Roof"),
        "Driveway" => PropertyAdministratorDisplayLocalization.L("Driveway"),
        "Fence" => PropertyAdministratorDisplayLocalization.L("Fence"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => value
    };

    private static string LabelDamageAreas(IEnumerable<string> areas)
    {
        var labels = areas.Select(LabelDamageArea).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        return labels.Count == 0
            ? PropertyAdministratorDisplayLocalization.L("Not specified")
            : string.Join(" + ", labels);
    }

    private static string LabelUrgency(string value) => value switch
    {
        "Emergency" => PropertyAdministratorDisplayLocalization.L("Emergency"),
        "Normal" => PropertyAdministratorDisplayLocalization.L("Normal"),
        _ => PropertyAdministratorDisplayLocalization.L("Urgent")
    };

    private static string LabelGateParkingNotes(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? "—"
            : PropertyAdministratorDisplayLocalization.L(value);

    private static string LabelEntryAccess(string value) => value switch
    {
        "SmartLock" => PropertyAdministratorDisplayLocalization.L("Smart lock code provided"),
        "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "GuestApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => PropertyAdministratorDisplayLocalization.L("Exterior only")
    };

    private static string LabelInsuranceHelp(string value) => value switch
    {
        "Yes" => PropertyAdministratorDisplayLocalization.L("Yes, help with claim"),
        "No" => PropertyAdministratorDisplayLocalization.L("No"),
        "UploadLater" => PropertyAdministratorDisplayLocalization.L("I'll upload later"),
        _ => PropertyAdministratorDisplayLocalization.L("Need documentation for claim")
    };

    private static string FormatRecipients(IEnumerable<string> recipients)
    {
        var labels = recipients.Select(r => r switch
        {
            "Guest" => PropertyAdministratorDisplayLocalization.L("Guest"),
            "CoHost" => PropertyAdministratorDisplayLocalization.L("Co-host"),
            _ => PropertyAdministratorDisplayLocalization.L("Me")
        }).ToList();

        return labels.Count == 0
            ? PropertyAdministratorDisplayLocalization.L("Me")
            : string.Join(" + ", labels);
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
            ImageUrl = property.ImageUrl,
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}