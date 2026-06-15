using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorEmergencyRoofLeakService
{
    Task<PropertyAdministratorEmergencyRoofLeakStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyRoofLeakStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorEmergencyRoofLeakStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorEmergencyRoofLeakSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyRoofLeakConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorEmergencyRoofLeakService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorEmergencyRoofLeakService
{
    public async Task<PropertyAdministratorEmergencyRoofLeakStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorEmergencyRoofLeakStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            QuickDetails = mapped.OccupancyLabel != null
                ? "Water is dripping from the ceiling in the upstairs guest bedroom after heavy rain."
                : ""
        };
    }

    public async Task<PropertyAdministratorEmergencyRoofLeakStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorEmergencyRoofLeakStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorEmergencyRoofLeakStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = mapped,
            GuestsOnSiteLabel = step1.GuestsInside == "Yes" ? "Guests on-site" : null,
            RoofIssue = step1.RoofIssue,
            WaterEnteringNow = step1.WaterEnteringNow,
            GuestsInside = step1.GuestsInside,
            Urgency = step1.Urgency,
            LeakLocation = step1.LeakLocation,
            QuickDetails = step1.QuickDetails ?? "",
            EntryCode = $"Front door code: {1000 + property.Id}",
            InteriorDamage = "Ceiling,Floor",
            AccessNotes = "Use driveway on left side. Call before arrival.",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "(919) 555-0187"
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorEmergencyRoofLeakSubmitInput input, CancellationToken cancellationToken = default)
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
            Title = $"{LabelIssue(input.RoofIssue)} • {LabelLocation(input.LeakLocation)}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = "26 min",
            TeamLabel = "Miguel R. • Roof repair",
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Miguel R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Roof Repair Pro",
            VehicleLabel = "White service truck",
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorEmergencyRoofLeakConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorEmergencyRoofLeakConfirmedViewModel
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
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Roof Repair Pro",
            EtaLabel = request.EtaLabel ?? "26 min",
            VehicleLabel = request.VehicleLabel ?? "White service truck",
            Timeline = BuildTimeline(request),
            Summary = BuildSummary(input)
        };
    }

    private static PropertyAdministratorEmergencyRoofLeakSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorEmergencyRoofLeakSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorEmergencyRoofLeakSubmitInput>(json)
                ?? new PropertyAdministratorEmergencyRoofLeakSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorEmergencyRoofLeakSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        PropertyAdministratorEmergencyRoofLeakSubmitInput input) =>
    [
        new()
        {
            Label = "Issue",
            Value = $"{LabelIssue(input.RoofIssue)} / {LabelLocation(input.LeakLocation).ToLowerInvariant()}",
            IconClass = "fa-house-chimney-crack"
        },
        new()
        {
            Label = "Water entering",
            Value = input.WaterEnteringNow,
            IconClass = "fa-droplet",
            Highlight = input.WaterEnteringNow == "Yes"
        },
        new()
        {
            Label = "Guests inside",
            Value = input.GuestsInside,
            IconClass = "fa-users",
            Highlight = input.GuestsInside == "Yes"
        },
        new()
        {
            Label = "Access",
            Value = LabelAccess(input.EntryAccess),
            IconClass = "fa-key"
        },
        new()
        {
            Label = "Updates",
            Value = string.Join(" + ", input.UpdateRecipientsList),
            IconClass = "fa-bell"
        }
    ];

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime().ToString("Today, h:mm tt");
        var assigned = request.FechaCreacion.AddMinutes(6).ToLocalTime().ToString("Today, h:mm tt");
        var enRoute = request.FechaCreacion.AddMinutes(8).ToLocalTime().ToString("Today, h:mm tt");
        var step = request.TimelineStep;

        return
        [
            new() { Label = "Request submitted", StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = "Technician assigned", StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = "En route", StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = "Arrived", StatusLabel = "Pending", IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = "Temporary tarp / inspection", StatusLabel = "Pending", IconClass = "fa-house-chimney", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelIssue(string value) => value switch
    {
        "StormDamage" => "Storm damage",
        "MissingShingles" => "Missing shingles",
        "CeilingStain" => "Ceiling stain",
        "SkylightLeak" => "Skylight leak",
        "TreeImpact" => "Tree impact",
        _ => "Active leak"
    };

    private static string LabelLocation(string value) => value switch
    {
        "LivingRoom" => "Living room",
        "Kitchen" => "Kitchen",
        "Attic" => "Attic",
        "Hallway" => "Hallway",
        "Other" => "Other",
        _ => "Bedroom"
    };

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => "Host will meet",
        "GuestApproval" => "Need guest approval",
        _ => "Smart lock code provided"
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
        var firstName = admin.DisplayName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";
        var hour = DateTime.Now.Hour;
        var greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        var portfolioName = !string.IsNullOrWhiteSpace(admin.PortfolioBusinessName)
            ? admin.PortfolioBusinessName
            : $"{firstName} Portfolio";

        var shell = new PropertyAdministratorPortalShellViewModel
        {
            DisplayName = admin.DisplayName ?? "Property Owner",
            PortfolioName = portfolioName,
            ActivePropertyCount = admin.PortfolioProperties.Count,
            Greeting = $"{greeting}, {firstName}",
            NotificationCount = admin.ServiceRequests.Count(r =>
                r.Status is PropertyAdministratorRequestStatuses.Open
                    or PropertyAdministratorRequestStatuses.Emergency
                    or PropertyAdministratorRequestStatuses.InProgress)
        };

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
            PropertyTypeLabel = PropertyAdministratorCatalog.LabelPropertyType(property.PropertyType),
            ImageUrl = property.ImageUrl,
            OccupancyLabel = property.PropertyType == "ShortTermRental" ? "Occupied now" : null
        };
    }
}
