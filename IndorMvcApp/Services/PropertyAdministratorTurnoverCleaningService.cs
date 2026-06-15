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
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "(919) 555-0123",
            Details = mapped.OccupancyLabel != null
                ? "Please finish before the next guest checks in and leave the welcome items on the kitchen counter."
                : ""
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
            Title = $"Turnover Cleaning at {property.PropertyName}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Cleaning",
            ScheduledUtc = todayVisit,
            IsEmergency = false,
            EtaLabel = "Today • 11:00 AM – 2:00 PM",
            TeamLabel = "Maria R. • Turnover",
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Maria R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Homecare Pro",
            VehicleLabel = "White service van",
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
        new() { Label = "Property", Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = "Service", Value = "Turnover Cleaning", IconClass = "fa-broom" },
        new() { Label = "Next guest arrival", Value = LabelGuestArrival(input.GuestArrival, input.GuestArrivalTime), IconClass = "fa-calendar" },
        new() { Label = "Scope", Value = LabelServiceType(input.ServiceType), IconClass = "fa-clipboard-list" },
        new() { Label = "Linens", Value = input.IncludedTasksList.Contains("FreshLinens") ? "Included" : "Not included", IconClass = "fa-bed" },
        new() { Label = "Restock", Value = LabelRestock(input.IncludedTasksList), IconClass = "fa-box" },
        new() { Label = "Access", Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = "Updates", Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-bell" }
    ];

    private static IReadOnlyList<PropertyAdministratorTurnoverCleaningTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime().ToString("Today, h:mm tt");
        var assigned = request.FechaCreacion.AddMinutes(3).ToLocalTime().ToString("Today, h:mm tt");
        var scheduled = request.FechaCreacion.AddMinutes(5).ToLocalTime().ToString("Today, h:mm tt");

        return
        [
            new() { Label = "Request submitted", StatusLabel = submitted, IconClass = "fa-circle-check", State = "done" },
            new() { Label = "Crew assigned", StatusLabel = assigned, IconClass = "fa-circle-check", State = "done" },
            new() { Label = "Scheduled visit", StatusLabel = scheduled, IconClass = "fa-calendar-check", State = "done" },
            new() { Label = "Final turnover cleaning", StatusLabel = "Today, 11:00 AM – 2:00 PM", IconClass = "fa-broom", State = "active" }
        ];
    }

    private static string LabelServiceType(string value) => value switch
    {
        "LightRefresh" => "Light refresh",
        "SameDayTurnover" => "Same-day turnover",
        "CheckoutClean" => "Checkout clean",
        _ => "Full turnover"
    };

    private static string LabelGuestArrival(string arrival, string time) => arrival switch
    {
        "Tomorrow" => $"Tomorrow {time}",
        "Later" => $"Later • {time}",
        _ => $"Today {time}"
    };

    private static string LabelRestock(IReadOnlyList<string> tasks)
    {
        var hasToiletries = tasks.Contains("RestockToiletries");
        var hasEssentials = tasks.Contains("KitchenReset") || tasks.Contains("TrashOut");
        return (hasToiletries, hasEssentials) switch
        {
            (true, true) => "Toiletries + essentials",
            (true, false) => "Toiletries",
            (false, true) => "Essentials",
            _ => "Not included"
        };
    }

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => "Host will meet",
        "NeedApproval" => "Need guest approval",
        _ => "Smart lock code provided"
    };

    private static string LabelUpdates(IReadOnlyList<string> recipients)
    {
        var labels = recipients.Select(r => r switch
        {
            "Guest" => "Guest",
            "CoHost" => "Co-host",
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
