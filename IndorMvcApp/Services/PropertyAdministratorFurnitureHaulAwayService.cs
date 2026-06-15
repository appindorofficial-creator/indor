using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorFurnitureHaulAwayService
{
    PropertyAdministratorFurnitureHaulAwayFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorFurnitureHaulAwayStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorFurnitureHaulAwayStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorFurnitureHaulAwayStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorFurnitureHaulAwaySubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorFurnitureHaulAwayConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorFurnitureHaulAwayService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorFurnitureHaulAwayService
{
    public PropertyAdministratorFurnitureHaulAwayFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("FurnitureHaulAwayDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorFurnitureHaulAwayStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);

        return new PropertyAdministratorFurnitureHaulAwayStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property?.PropertyType == "ShortTermRental" ? "Guest checkout today" : null,
            QuickDetails = property?.PropertyType == "ShortTermRental"
                ? "1 couch, 1 mattress, and 2 chairs after guest move-out."
                : ""
        };
    }

    public async Task<PropertyAdministratorFurnitureHaulAwayStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorFurnitureHaulAwayStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorFurnitureHaulAwayStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = step1.GuestsInside == "Yes" ? "Guests on-site" : null,
            FurnitureItems = string.Join(",", step1.FurnitureItemsList),
            ItemCount = step1.ItemCount,
            PickupSize = step1.PickupSize,
            IsOccupied = step1.IsOccupied,
            GuestsInside = step1.GuestsInside,
            QuickDetails = step1.QuickDetails ?? "",
            PickupWhen = property.PropertyType == "ShortTermRental" ? "TodayAfterCheckout" : "Asap",
            EntryCode = $"{2000 + property.Id}#",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "(305) 555-0198"
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorFurnitureHaulAwaySubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = input.PickupWhen == "ScheduledTime"
            ? DateTime.Today.AddDays(1).AddHours(15)
            : DateTime.Today.AddHours(15.17);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = $"Furniture Haul Away at {property.PropertyName}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Furniture",
            ScheduledUtc = visitDate,
            IsEmergency = input.PickupWhen == "Asap",
            EtaLabel = "38 min",
            TeamLabel = "Luis R. • Furniture Haul Away",
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Luis R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Verified",
            VehicleLabel = "Box truck",
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Furniture haul-away pickup",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = LabelTimeWindow(input.TimeWindow),
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorFurnitureHaulAwayConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorFurnitureHaulAwayConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Luis R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Verified",
            TechnicianRole = "Furniture Haul Away Pro",
            EtaLabel = request.EtaLabel ?? "38 min",
            VehicleLabel = request.VehicleLabel ?? "Box truck",
            Summary = BuildSummary(input),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorFurnitureHaulAwaySubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorFurnitureHaulAwaySubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorFurnitureHaulAwaySubmitInput>(json)
                ?? new PropertyAdministratorFurnitureHaulAwaySubmitInput();
        }
        catch
        {
            return new PropertyAdministratorFurnitureHaulAwaySubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorFurnitureHaulAwaySummaryItemViewModel> BuildSummary(
        PropertyAdministratorFurnitureHaulAwaySubmitInput input) =>
    [
        new() { Label = "Items", Value = LabelItems(input), IconClass = "fa-couch" },
        new() { Label = "Load size", Value = LabelPickupSize(input.PickupSize), IconClass = "fa-box" },
        new() { Label = "Access", Value = LabelAccess(input), IconClass = "fa-key" },
        new() { Label = "Updates", Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" },
        new() { Label = "Pickup time", Value = LabelPickupWhen(input.PickupWhen), IconClass = "fa-clock" }
    ];

    private static IReadOnlyList<PropertyAdministratorFurnitureHaulAwayTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(4);
        var enRoute = submitted.AddMinutes(8);

        return
        [
            new() { Label = "Request submitted", StatusLabel = submitted.ToString("h:mm tt"), IconClass = "fa-circle-check", State = "done" },
            new() { Label = "Crew assigned", StatusLabel = assigned.ToString("h:mm tt"), IconClass = "fa-circle-check", State = "done" },
            new() { Label = "En route", StatusLabel = enRoute.ToString("h:mm tt"), IconClass = "fa-truck", State = "active" },
            new() { Label = "Arrived", StatusLabel = "—", IconClass = "fa-location-dot", State = "pending" },
            new() { Label = "Pickup in progress", StatusLabel = "—", IconClass = "fa-dolly", State = "pending" }
        ];
    }

    private static string LabelItems(PropertyAdministratorFurnitureHaulAwaySubmitInput input)
    {
        var labels = input.FurnitureItemsList.Select(i => i switch
        {
            "Mattress" => "mattress",
            "BedFrame" => "bed frame",
            "Dresser" => "dresser",
            "DiningTable" => "dining table",
            "Refrigerator" => "refrigerator",
            "WasherDryer" => "washer / dryer",
            "TvElectronics" => "TV / electronics",
            "Other" => "other items",
            _ => "couch"
        }).Distinct().ToList();

        var countSuffix = input.ItemCount switch
        {
            "One" => "",
            "FourSix" => " (4–6 items)",
            "SevenPlus" => " (7+ items)",
            _ => " + 2 chairs"
        };

        if (labels.Count == 0)
        {
            return "—";
        }

        var baseLabel = labels.Count switch
        {
            1 => labels[0],
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels.Take(2)) + (labels.Count > 2 ? " + more" : "")
        };

        return input.ItemCount == "TwoThree" ? $"{baseLabel}{countSuffix}" : $"{baseLabel}{countSuffix}".Trim();
    }

    private static string LabelPickupSize(string value) => value switch
    {
        "SmallPickup" => "Small pickup",
        "FullLoad" => "Full load",
        _ => "Half load"
    };

    private static string LabelAccess(PropertyAdministratorFurnitureHaulAwaySubmitInput input)
    {
        var entry = input.EntryAccess switch
        {
            "HostMeet" => "Host will meet",
            "CurbsideOnly" => "Curbside pickup only",
            _ => "Smart lock"
        };

        return $"{entry} / inside pickup";
    }

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
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels)
        };
    }

    private static string LabelPickupWhen(string value) => value switch
    {
        "Asap" => "ASAP",
        "ScheduledTime" => "Scheduled time",
        _ => "Today after checkout"
    };

    private static string LabelTimeWindow(string value) => value switch
    {
        "Morning" => "8 AM – 12 PM",
        "Evening" => "5 PM – 9 PM",
        _ => "12 PM – 5 PM"
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
