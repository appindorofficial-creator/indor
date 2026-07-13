using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorEmergencyPlumbingService
{
    Task<PropertyAdministratorEmergencyPlumbingStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyPlumbingStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorEmergencyPlumbingStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorEmergencyPlumbingSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyPlumbingConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorEmergencyPlumbingService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorEmergencyPlumbingService
{
    public async Task<PropertyAdministratorEmergencyPlumbingStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorEmergencyPlumbingStep1ViewModel
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

    public async Task<PropertyAdministratorEmergencyPlumbingStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorEmergencyPlumbingStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorEmergencyPlumbingStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = mapped,
            GuestsOnSiteLabel = step1.GuestsInside == "Yes" ? PropertyAdministratorDisplayLocalization.L("Guests on-site") : null,
            IssueType = step1.IssueType,
            ActivelyLeaking = step1.ActivelyLeaking,
            GuestsInside = step1.GuestsInside,
            Urgency = step1.Urgency,
            ProblemLocation = step1.ProblemLocation,
            QuickDetails = step1.QuickDetails ?? "",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorEmergencyPlumbingSubmitInput input, CancellationToken cancellationToken = default)
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
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("22 min"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Carlos M. • Plumbing"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Carlos M.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Plumbing Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorEmergencyPlumbingConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorEmergencyPlumbingConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Carlos M.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Plumbing Pro",
            EtaLabel = request.EtaLabel ?? "22 min",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Timeline = BuildTimeline(request),
            Summary = BuildSummary(input)
        };
    }

    private static PropertyAdministratorEmergencyPlumbingSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorEmergencyPlumbingSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorEmergencyPlumbingSubmitInput>(json)
                ?? new PropertyAdministratorEmergencyPlumbingSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorEmergencyPlumbingSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        PropertyAdministratorEmergencyPlumbingSubmitInput input) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Issue"),
            Value = $"{LabelIssue(input.IssueType)} / {LabelLocation(input.ProblemLocation).ToLowerInvariant()}",
            IconClass = "fa-droplet"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Occupied"),
            Value = input.ActivelyLeaking == "Yes" ? "Yes" : "No",
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
            Label = PropertyAdministratorDisplayLocalization.L("Access"),
            Value = LabelAccess(input.EntryAccess),
            IconClass = "fa-key"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Updates"),
            Value = string.Join(" + ", input.UpdateRecipientsList),
            IconClass = "fa-bell"
        }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime().ToString("Today, h:mm tt");
        var assigned = request.FechaCreacion.AddMinutes(5).ToLocalTime().ToString("Today, h:mm tt");
        var enRoute = request.FechaCreacion.AddMinutes(8).ToLocalTime().ToString("Today, h:mm tt");
        var step = request.TimelineStep;

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Diagnosis"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-clipboard-list", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelIssue(string value) => value switch
    {
        "ClogBackup" => PropertyAdministratorDisplayLocalization.L("Clog / backup"),
        "NoHotWater" => PropertyAdministratorDisplayLocalization.L("No hot water"),
        "ToiletIssue" => PropertyAdministratorDisplayLocalization.L("Toilet issue"),
        "BurstPipe" => PropertyAdministratorDisplayLocalization.L("Burst pipe"),
        "NotDraining" => PropertyAdministratorDisplayLocalization.L("Water not draining"),
        _ => PropertyAdministratorDisplayLocalization.L("Active leak")
    };

    private static string LabelLocation(string value) => value switch
    {
        "Kitchen" => PropertyAdministratorDisplayLocalization.L("Kitchen"),
        "Laundry" => PropertyAdministratorDisplayLocalization.L("Laundry"),
        "WaterHeater" => PropertyAdministratorDisplayLocalization.L("Water heater"),
        "MainLine" => PropertyAdministratorDisplayLocalization.L("Main line"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => PropertyAdministratorDisplayLocalization.L("Bathroom")
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
            ImageUrl = property.ImageUrl,
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}