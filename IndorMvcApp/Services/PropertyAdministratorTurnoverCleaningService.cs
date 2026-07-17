using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorTurnoverCleaningService
{
    PropertyAdministratorTurnoverCleaningFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorTurnoverCleaningFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorTurnoverCleaningSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorTurnoverCleaningConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorTurnoverCleaningService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorTurnoverCleaningService
{
    public PropertyAdministratorTurnoverCleaningFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("TurnoverCleaningDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorTurnoverCleaningFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var user = await GetUserAsync();
        var mapped = MapProperty(property);

        return new PropertyAdministratorTurnoverCleaningFormViewModel
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
        PropertyAdministratorTurnoverCleaningSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var todayVisit = DateTime.Today.AddHours(11);
        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", PropertyAdministratorDisplayLocalization.L("Turnover Cleaning"), property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Cleaning",
            ScheduledUtc = todayVisit,
            IsEmergency = false,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("Today • 11:00 AM – 2:00 PM"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Maria R. • Turnover"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Maria R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Homecare Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Turnover cleaning",
            PropertyName = property.PropertyName,
            VisitDate = todayVisit.Date,
            TimeWindow = "11:00 AM – 2:00 PM",
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorTurnoverCleaningConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorTurnoverCleaningConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Maria R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Homecare Pro",
            ScheduleLabel = request.EtaLabel ?? "Today • 11:00 AM – 2:00 PM",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Summary = BuildSummary(property, input),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorTurnoverCleaningSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorTurnoverCleaningSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorTurnoverCleaningSubmitInput>(json)
                ?? new PropertyAdministratorTurnoverCleaningSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorTurnoverCleaningSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> BuildSummary(
        IndorPropertyAdminPortfolioProperty? property, PropertyAdministratorTurnoverCleaningSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = PropertyAdministratorDisplayLocalization.L("Turnover Cleaning"), IconClass = "fa-broom" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Next guest arrival"), Value = LabelGuestArrival(input.GuestArrival, input.GuestArrivalTime), IconClass = "fa-calendar" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Scope"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-clipboard-list" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Linens"), Value = input.IncludedTasksList.Contains("FreshLinens") ? PropertyAdministratorDisplayLocalization.L("Included") : PropertyAdministratorDisplayLocalization.L("Not included"), IconClass = "fa-bed" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Restock"), Value = LabelRestock(input.IncludedTasksList), IconClass = "fa-box" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-bell" }
    ];

    private static IReadOnlyList<PropertyAdministratorTurnoverCleaningTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion);
        var assigned = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(3));
        var scheduled = PropertyAdministratorFlowServiceSupport.FormatTodayTime(request.FechaCreacion.AddMinutes(5));

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Crew assigned"), StatusLabel = assigned, IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled visit"), StatusLabel = scheduled, IconClass = "fa-calendar-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Final turnover cleaning"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Today, 11:00 AM – 2:00 PM"), IconClass = "fa-broom", State = "active" }
        ];
    }

    private static string LabelServiceType(string value) => value switch
    {
        "LightRefresh" => PropertyAdministratorDisplayLocalization.L("Light refresh"),
        "SameDayTurnover" => PropertyAdministratorDisplayLocalization.L("Same-day turnover"),
        "CheckoutClean" => PropertyAdministratorDisplayLocalization.L("Checkout clean"),
        _ => PropertyAdministratorDisplayLocalization.L("Full turnover")
    };

    private static string LabelGuestArrival(string arrival, string time)
    {
        var localizedTime = string.IsNullOrWhiteSpace(time) ? "" : time.Trim();
        return arrival switch
        {
            "Tomorrow" => string.IsNullOrWhiteSpace(localizedTime)
                ? PropertyAdministratorDisplayLocalization.L("Tomorrow")
                : PropertyAdministratorDisplayLocalization.T("Tomorrow {0}", localizedTime),
            "Later" => string.IsNullOrWhiteSpace(localizedTime)
                ? PropertyAdministratorDisplayLocalization.L("Later")
                : PropertyAdministratorDisplayLocalization.T("Later • {0}", localizedTime),
            _ => string.IsNullOrWhiteSpace(localizedTime)
                ? PropertyAdministratorDisplayLocalization.L("Today")
                : PropertyAdministratorDisplayLocalization.T("Today {0}", localizedTime)
        };
    }

    private static string LabelRestock(IReadOnlyList<string> tasks)
    {
        var hasToiletries = tasks.Contains("RestockToiletries");
        var hasEssentials = tasks.Contains("KitchenReset") || tasks.Contains("TrashOut");
        return (hasToiletries, hasEssentials) switch
        {
            (true, true) => PropertyAdministratorDisplayLocalization.L("Toiletries + essentials"),
            (true, false) => PropertyAdministratorDisplayLocalization.L("Toiletries"),
            (false, true) => PropertyAdministratorDisplayLocalization.L("Essentials"),
            _ => PropertyAdministratorDisplayLocalization.L("Not included")
        };
    }

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
            _ => PropertyAdministratorDisplayLocalization.L("Me")
        }).Distinct().ToList();

        return labels.Count switch
        {
            0 => PropertyAdministratorDisplayLocalization.L("Me"),
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