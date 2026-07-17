using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorBrokenWindowBoardUpService
{
    Task<PropertyAdministratorBrokenWindowBoardUpStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorBrokenWindowBoardUpStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorBrokenWindowBoardUpStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorBrokenWindowBoardUpSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorBrokenWindowBoardUpConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorBrokenWindowBoardUpService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorBrokenWindowBoardUpService
{
    public async Task<PropertyAdministratorBrokenWindowBoardUpStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorBrokenWindowBoardUpStep1ViewModel
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

    public async Task<PropertyAdministratorBrokenWindowBoardUpStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorBrokenWindowBoardUpStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorBrokenWindowBoardUpStep2ViewModel
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
            HelpType = step1.HelpType,
            Urgency = step1.Urgency,
            DamageLocation = step1.DamageLocation,
            GuestsInside = step1.GuestsInside,
            ExposedToRisk = step1.ExposedToRisk,
            QuickDetails = step1.QuickDetails ?? "",
            EntryInstructions = PropertyAdministratorDisplayLocalization.T(
                "Front door code: {0}",
                1000 + property.Id),
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorBrokenWindowBoardUpSubmitInput input, CancellationToken cancellationToken = default)
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
            Title = $"{LabelHelpType(input.HelpType)} • {LabelDamageLocation(input.DamageLocation)}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("24 min"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Carlos M. • Board-up"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Carlos M.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Emergency board-up pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorBrokenWindowBoardUpConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorBrokenWindowBoardUpConfirmedViewModel
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
            TechnicianTitle = request.TechnicianTitle ?? "Emergency board-up pro",
            EtaLabel = request.EtaLabel ?? "24 min",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Timeline = BuildTimeline(request)
        };
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion);
        var assigned = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(2));
        var enRoute = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(5));
        var step = request.TimelineStep;

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Temporary board-up complete"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-window-maximize", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelHelpType(string value) => value switch
    {
        "ShatteredGlass" => PropertyAdministratorDisplayLocalization.L("Shattered glass"),
        "DoorGlass" => PropertyAdministratorDisplayLocalization.L("Door glass"),
        "BoardUpOnly" => PropertyAdministratorDisplayLocalization.L("Board-up only"),
        "ActiveBreakInDamage" => PropertyAdministratorDisplayLocalization.L("Active break-in damage"),
        _ => PropertyAdministratorDisplayLocalization.L("Broken window")
    };

    private static string LabelDamageLocation(string value) => value switch
    {
        "BackWindow" => PropertyAdministratorDisplayLocalization.L("Back window"),
        "SlidingDoor" => PropertyAdministratorDisplayLocalization.L("Sliding door"),
        "BedroomWindow" => PropertyAdministratorDisplayLocalization.L("Bedroom window"),
        "StorefrontOther" => PropertyAdministratorDisplayLocalization.L("Storefront / other"),
        _ => PropertyAdministratorDisplayLocalization.L("Front window")
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