using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorSmokeDetectorService
{
    PropertyAdministratorSmokeDetectorFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorSmokeDetectorFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorSmokeDetectorSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorSmokeDetectorConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorSmokeDetectorService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorSmokeDetectorService
{
    public PropertyAdministratorSmokeDetectorFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("SmokeDetectorDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorSmokeDetectorFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var user = await GetUserAsync();
        var mapped = MapProperty(property);

        return new PropertyAdministratorSmokeDetectorFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorSmokeDetectorSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var tomorrow = DateTime.Today.AddDays(1).AddHours(13);
        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", "Smoke Detector Check", property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Homecare",
            ScheduledUtc = tomorrow,
            IsEmergency = false,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("Tomorrow • 1:00 PM – 3:00 PM"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Daniel M. • Homecare"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Daniel M.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Homecare Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Smoke detector check",
            PropertyName = property.PropertyName,
            VisitDate = tomorrow.Date,
            TimeWindow = "1:00 PM – 3:00 PM",
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorSmokeDetectorConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorSmokeDetectorConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Daniel M.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Homecare Pro",
            ScheduleLabel = request.EtaLabel ?? "Tomorrow • 1:00 PM – 3:00 PM",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Summary = BuildSummary(property, input),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorSmokeDetectorSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorSmokeDetectorSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorSmokeDetectorSubmitInput>(json)
                ?? new PropertyAdministratorSmokeDetectorSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorSmokeDetectorSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        IndorPropertyAdminPortfolioProperty? property, PropertyAdministratorSmokeDetectorSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-bell" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Occupied"), Value = property?.PropertyType == "ShortTermRental" ? "Yes" : "No", IconClass = "fa-door-open" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Detectors"), Value = LabelDetectorCount(input.DetectorCount), IconClass = "fa-bell" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Type"), Value = LabelDetectorType(input.DetectorType), IconClass = "fa-plug" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-bell" }
    ];

    private static IReadOnlyList<PropertyAdministratorSmokeDetectorTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime().ToString("Today • h:mm tt");
        var assigned = request.FechaCreacion.AddMinutes(1).ToLocalTime().ToString("Today • h:mm tt");
        var scheduled = "Tomorrow • 1:00 PM – 3:00 PM";

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled visit"), StatusLabel = scheduled, IconClass = "fa-calendar-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Final safety check"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Upcoming"), IconClass = "fa-shield-halved", State = "pending" }
        ];
    }

    private static string LabelServiceType(string value) => value switch
    {
        "ReplaceBatteries" => PropertyAdministratorDisplayLocalization.L("Replace batteries"),
        "ChirpingDetector" => PropertyAdministratorDisplayLocalization.L("Chirping detector"),
        "InstallNewDetector" => PropertyAdministratorDisplayLocalization.L("Install new detector"),
        _ => PropertyAdministratorDisplayLocalization.L("Full detector check")
    };

    private static string LabelDetectorCount(string value) => value switch
    {
        "OneToTwo" => PropertyAdministratorDisplayLocalization.L("1–2"),
        "SixPlus" => PropertyAdministratorDisplayLocalization.L("6+"),
        _ => PropertyAdministratorDisplayLocalization.L("3–5")
    };

    private static string LabelDetectorType(string value) => value switch
    {
        "Battery" => PropertyAdministratorDisplayLocalization.L("Battery"),
        "NotSure" => PropertyAdministratorDisplayLocalization.L("Not sure"),
        _ => PropertyAdministratorDisplayLocalization.L("Hardwired")
    };

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "NeedApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => PropertyAdministratorDisplayLocalization.L("Smart lock code provided")
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