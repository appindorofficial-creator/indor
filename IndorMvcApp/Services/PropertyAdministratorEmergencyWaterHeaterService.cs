using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorEmergencyWaterHeaterService
{
    Task<PropertyAdministratorEmergencyWaterHeaterStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyWaterHeaterReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyWaterHeaterStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorEmergencyWaterHeaterSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorEmergencyWaterHeaterConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorEmergencyWaterHeaterService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorEmergencyWaterHeaterService
{
    public async Task<PropertyAdministratorEmergencyWaterHeaterStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorEmergencyWaterHeaterStep1ViewModel
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

    public async Task<PropertyAdministratorEmergencyWaterHeaterReviewViewModel?> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorEmergencyWaterHeaterStep1Input step1, CancellationToken cancellationToken = default)
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
        var input = new PropertyAdministratorEmergencyWaterHeaterSubmitInput
        {
            PropertyId = property.Id,
            ProblemType = step1.ProblemType,
            ActivelyLeaking = step1.ActivelyLeaking,
            HomeOccupied = step1.HomeOccupied,
            GuestsInside = step1.GuestsInside,
            Urgency = step1.Urgency,
            HeaterType = step1.HeaterType,
            QuickDetails = step1.QuickDetails,
            EntryAccess = "GarageSide",
            AccessNotes = "Garage side entry available",
            UpdateRecipientsList = ["Me", "Guest"],
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? ""
        };

        return new PropertyAdministratorEmergencyWaterHeaterReviewViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            Input = input,
            SummaryRows = BuildSummaryRows(input, mapped)
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorEmergencyWaterHeaterSubmitInput input, CancellationToken cancellationToken = default)
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
            Title = $"{LabelProblem(input.ProblemType)} • {LabelHeaterType(input.HeaterType)}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Emergency",
            ScheduledUtc = now,
            IsEmergency = true,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("24 min"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Carlos M. • Water heater"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Carlos M.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Water Heater Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorEmergencyWaterHeaterConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorEmergencyWaterHeaterConfirmedViewModel
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
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Water Heater Pro",
            EtaLabel = request.EtaLabel ?? "24 min",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Timeline = BuildTimeline(request)
        };
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyWaterHeaterReviewRowViewModel> BuildSummaryRows(
        PropertyAdministratorEmergencyWaterHeaterSubmitInput input,
        PropertyAdministratorFlowPropertyViewModel property) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = $"Viewing: {property.PropertyName}", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Issue"), Value = FormatIssueSummary(input), IconClass = "fa-fire-flame-simple" },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Occupied"),
            Value = input.HomeOccupied,
            IconClass = "fa-user",
            Highlight = input.HomeOccupied == "Yes"
        },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Guests inside"),
            Value = input.GuestsInside,
            IconClass = "fa-users",
            Highlight = input.GuestsInside == "Yes"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Water heater type"), Value = LabelHeaterType(input.HeaterType), IconClass = "fa-fire" },
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Water status"),
            Value = input.ActivelyLeaking == "Yes" ? "Actively leaking" : "Not actively leaking",
            IconClass = "fa-droplet"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess, input.AccessNotes), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = FormatRecipients(input.UpdateRecipientsList), IconClass = "fa-bell" }
    ];

    private static string FormatIssueSummary(PropertyAdministratorEmergencyWaterHeaterSubmitInput input)
    {
        if (input.ActivelyLeaking == "Yes" || input.ProblemType == "LeakingTank")
        {
            return $"{LabelProblem(input.ProblemType)} / leaking tank";
        }

        return LabelProblem(input.ProblemType);
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime().ToString("Today, h:mm tt");
        var assigned = request.FechaCreacion.AddMinutes(4).ToLocalTime().ToString("Today, h:mm tt");
        var enRoute = request.FechaCreacion.AddMinutes(7).ToLocalTime().ToString("Today, h:mm tt");
        var step = request.TimelineStep;

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = step >= 1 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute, IconClass = "fa-truck", State = step >= 2 ? "active" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrival"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-location-dot", State = step >= 3 ? "done" : "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Diagnosis"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-clipboard-list", State = step >= 4 ? "done" : "pending" }
        ];
    }

    private static string LabelProblem(string value) => value switch
    {
        "LeakingTank" => PropertyAdministratorDisplayLocalization.L("Leaking tank"),
        "PilotLight" => PropertyAdministratorDisplayLocalization.L("Pilot light / ignition"),
        "StrangeNoise" => PropertyAdministratorDisplayLocalization.L("Strange noise"),
        "LowHotWater" => PropertyAdministratorDisplayLocalization.L("Low hot water"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => PropertyAdministratorDisplayLocalization.L("No hot water")
    };

    private static string LabelHeaterType(string value) => value switch
    {
        "Electric" => PropertyAdministratorDisplayLocalization.L("Electric"),
        "Tankless" => PropertyAdministratorDisplayLocalization.L("Tankless"),
        "NotSure" => PropertyAdministratorDisplayLocalization.L("Not sure"),
        _ => PropertyAdministratorDisplayLocalization.L("Gas")
    };

    private static string LabelAccess(string value, string? notes) => value switch
    {
        "SmartLock" => PropertyAdministratorDisplayLocalization.L("Smart lock code provided"),
        "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "GuestApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => string.IsNullOrWhiteSpace(notes) ? "Garage side entry available" : notes
    };

    private static string FormatRecipients(IEnumerable<string> recipients)
    {
        var labels = recipients.Select(r => r switch
        {
            "Guest" => "Guest",
            "CoHost" => "Co-host",
            _ => "Me"
        });
        return string.Join(" + ", labels);
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