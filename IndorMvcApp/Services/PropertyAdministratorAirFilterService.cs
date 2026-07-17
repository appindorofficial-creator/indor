using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorAirFilterService
{
    PropertyAdministratorAirFilterFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorAirFilterFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorAirFilterSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorAirFilterConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorAirFilterService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorAirFilterService
{
    public PropertyAdministratorAirFilterFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("AirFilterDetails", "Administrador", new { propertyId }) ?? "#",
            Benefits =
            [
                new() { Label = PropertyAdministratorDisplayLocalization.L("Quick & reliable — On-time service"), IconClass = "fa-clock" },
                new() { Label = PropertyAdministratorDisplayLocalization.L("Healthier air — Cleaner, safer spaces"), IconClass = "fa-shield-halved" },
                new() { Label = PropertyAdministratorDisplayLocalization.L("Stay ready — Ideal for turnovers"), IconClass = "fa-calendar-check" }
            ]
        };

    public async Task<PropertyAdministratorAirFilterFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var user = await GetUserAsync();
        var mapped = MapProperty(property);

        return new PropertyAdministratorAirFilterFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "",
            // Start blank — do not pre-check Update Recipients (Bug 12/25 pattern).
            UpdateRecipients = string.Empty,
            ServiceType = string.Empty,
            IsOccupied = string.Empty,
            GuestsInside = string.Empty,
            FilterSize = string.Empty,
            Frequency = string.Empty,
            EntryAccess = string.Empty
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorAirFilterSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var tomorrow = DateTime.Today.AddDays(1).AddHours(10);
        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", PropertyAdministratorDisplayLocalization.L("Air Filter Change"), property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Homecare",
            ScheduledUtc = tomorrow,
            IsEmergency = false,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("Tomorrow • 10:00 AM – 12:00 PM"),
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
            Title = "Air filter change",
            PropertyName = property.PropertyName,
            VisitDate = tomorrow.Date,
            TimeWindow = "10:00 AM – 12:00 PM",
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorAirFilterConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorAirFilterConfirmedViewModel
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
            ScheduleLabel = request.EtaLabel ?? "Tomorrow • 10:00 AM – 12:00 PM",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Summary = BuildSummary(property, input),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorAirFilterSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorAirFilterSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorAirFilterSubmitInput>(json)
                ?? new PropertyAdministratorAirFilterSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorAirFilterSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        IndorPropertyAdminPortfolioProperty? property, PropertyAdministratorAirFilterSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-fan" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Occupied"), Value = input.IsOccupied, IconClass = "fa-door-open" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Guests inside"), Value = input.GuestsInside, IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Filter size"), Value = LabelFilterSize(input.FilterSize), IconClass = "fa-ruler-combined" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Frequency"), Value = LabelFrequency(input.Frequency), IconClass = "fa-calendar" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-bell" }
    ];

    private static IReadOnlyList<PropertyAdministratorAirFilterTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = PropertyAdministratorFlowServiceSupport.FormatTodayBulletTime(request.FechaCreacion);
        var assigned = PropertyAdministratorFlowServiceSupport.FormatTodayBulletTime(request.FechaCreacion.AddMinutes(1));
        var scheduled = PropertyAdministratorFlowServiceSupport.FormatTodayBulletTime(request.FechaCreacion.AddMinutes(2));

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled visit"), StatusLabel = scheduled, IconClass = "fa-calendar-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Filter replacement"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Upcoming"), IconClass = "fa-fan", State = "pending" }
        ];
    }

    private static string LabelServiceType(string value) => value switch
    {
        "BringFilters" => PropertyAdministratorDisplayLocalization.L("Bring filters"),
        "CheckSize" => PropertyAdministratorDisplayLocalization.L("Check filter size"),
        "FullService" => PropertyAdministratorDisplayLocalization.L("Full HVAC filter service"),
        _ => PropertyAdministratorDisplayLocalization.L("Replace filter")
    };

    private static string LabelFilterSize(string value) => value switch
    {
        "16x20x1" => PropertyAdministratorDisplayLocalization.L("16x20x1"),
        "20x25x1" => PropertyAdministratorDisplayLocalization.L("20x25x1"),
        "NotSure" => PropertyAdministratorDisplayLocalization.L("Not sure"),
        _ => PropertyAdministratorDisplayLocalization.L("20x20x1")
    };

    private static string LabelFrequency(string value) => value switch
    {
        "OneTime" => PropertyAdministratorDisplayLocalization.L("One-time"),
        "Every2Months" => PropertyAdministratorDisplayLocalization.L("Every 2 months"),
        _ => PropertyAdministratorDisplayLocalization.L("Every 3 months")
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