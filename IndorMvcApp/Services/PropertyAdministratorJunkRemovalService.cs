using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorJunkRemovalService
{
    PropertyAdministratorJunkRemovalFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorJunkRemovalStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorJunkRemovalStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorJunkRemovalStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorJunkRemovalSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorJunkRemovalConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorJunkRemovalService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorJunkRemovalService
{
    public PropertyAdministratorJunkRemovalFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("JunkRemovalDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorJunkRemovalStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);

        return new PropertyAdministratorJunkRemovalStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property)
        };
    }

    public async Task<PropertyAdministratorJunkRemovalStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorJunkRemovalStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorJunkRemovalStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property.PropertyType == "ShortTermRental" ? "Guest checkout today" : null,
            RemovalItems = string.Join(",", step1.RemovalItemsList),
            LoadSize = step1.LoadSize,
            IsOccupied = step1.IsOccupied,
            GuestsInside = step1.GuestsInside,
            PickupType = step1.PickupType,
            QuickDetails = step1.QuickDetails ?? "",
            PickupWhen = "",
            EntryCode = "",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorJunkRemovalSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = input.PickupWhen == "ScheduledTime"
            ? DateTime.Today.AddDays(1).AddHours(14)
            : DateTime.Today.AddHours(14.5);
        var etaLabel = PropertyAdministratorDisplayLocalization.L("35 min");
        var pickupLabel = LabelPickupWhen(input.PickupWhen);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", "Junk Removal", property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.InProgress,
            Category = "Junk",
            ScheduledUtc = visitDate,
            IsEmergency = input.PickupWhen == "Asap",
            EtaLabel = etaLabel,
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Marcus T. • Junk Removal"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Marcus T.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Verified",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("Box truck"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Junk removal pickup",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = LabelTimeWindow(input.TimeWindow),
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorJunkRemovalConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorJunkRemovalConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Marcus T.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Verified",
            TechnicianRole = "Junk Removal Pro",
            EtaLabel = request.EtaLabel ?? "35 min",
            VehicleLabel = request.VehicleLabel ?? "Box truck",
            Summary = BuildSummary(input),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorJunkRemovalSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorJunkRemovalSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorJunkRemovalSubmitInput>(json)
                ?? new PropertyAdministratorJunkRemovalSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorJunkRemovalSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorJunkRemovalSummaryItemViewModel> BuildSummary(
        PropertyAdministratorJunkRemovalSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Items"), Value = LabelItems(input.RemovalItemsList), IconClass = "fa-couch" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Load size"), Value = LabelLoadSize(input.LoadSize), IconClass = "fa-trash" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Pickup time"), Value = LabelPickupWhen(input.PickupWhen), IconClass = "fa-clock" }
    ];

    private static IReadOnlyList<PropertyAdministratorJunkRemovalTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(1);
        var enRoute = submitted.AddMinutes(3);

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = submitted.ToString("h:mm tt"), IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Crew assigned"), StatusLabel = assigned.ToString("h:mm tt"), IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = enRoute.ToString("h:mm tt"), IconClass = "fa-truck", State = "active" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-location-dot", State = "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Pickup in progress"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-dolly", State = "pending" }
        ];
    }

    private static string LabelItems(IReadOnlyList<string> items)
    {
        var labels = items.Select(i => i switch
        {
            "Boxes" => PropertyAdministratorDisplayLocalization.L("boxes"),
            "BaggedTrash" => PropertyAdministratorDisplayLocalization.L("bagged trash"),
            "Appliances" => PropertyAdministratorDisplayLocalization.L("appliances"),
            "YardDebris" => PropertyAdministratorDisplayLocalization.L("yard debris"),
            "MixedItems" => PropertyAdministratorDisplayLocalization.L("mixed items"),
            _ => PropertyAdministratorDisplayLocalization.L("furniture")
        }).Distinct().ToList();

        return labels.Count switch
        {
            0 => "—",
            1 => labels[0],
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels)
        };
    }

    private static string LabelLoadSize(string value) => value switch
    {
        "SmallPickup" => PropertyAdministratorDisplayLocalization.L("Small pickup"),
        "FullLoad" => PropertyAdministratorDisplayLocalization.L("Full load"),
        "MultipleLoads" => PropertyAdministratorDisplayLocalization.L("Multiple loads"),
        _ => PropertyAdministratorDisplayLocalization.L("Half load")
    };

    private static string LabelAccess(PropertyAdministratorJunkRemovalSubmitInput input)
    {
        var entry = input.EntryAccess switch
        {
            "HostMeet" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
            "CurbsideOnly" => PropertyAdministratorDisplayLocalization.L("Curbside pickup only"),
            _ => PropertyAdministratorDisplayLocalization.L("Smart lock")
        };

        var pickup = input.PickupType switch
        {
            "Curbside" => "curbside only",
            "Either" => "inside or curbside",
            _ => "inside pickup"
        };

        return $"{entry} / {pickup}";
    }

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
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels)
        };
    }

    private static string LabelPickupWhen(string value) => value switch
    {
        "Asap" => PropertyAdministratorDisplayLocalization.L("ASAP"),
        "ScheduledTime" => PropertyAdministratorDisplayLocalization.L("Scheduled time"),
        _ => PropertyAdministratorDisplayLocalization.L("Today after checkout")
    };

    private static string LabelTimeWindow(string value) => value switch
    {
        "Morning" => PropertyAdministratorDisplayLocalization.L("8 AM – 12 PM"),
        "Evening" => PropertyAdministratorDisplayLocalization.L("5 PM – 9 PM"),
        _ => PropertyAdministratorDisplayLocalization.L("12 PM – 5 PM")
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