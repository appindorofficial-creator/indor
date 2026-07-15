using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorTrashOutService
{
    PropertyAdministratorTrashOutFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorTrashOutStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorTrashOutStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorTrashOutStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorTrashOutSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorTrashOutConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorTrashOutService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorTrashOutService
{
    public PropertyAdministratorTrashOutFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("TrashOutDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorTrashOutStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);

        return new PropertyAdministratorTrashOutStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property?.PropertyType == "ShortTermRental" ? "Guest checkout tomorrow" : null,
            FlatRateLabel = ResolveFlatRate("TakeOutBringBack")
        };
    }

    public async Task<PropertyAdministratorTrashOutStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorTrashOutStep1Input step1, CancellationToken cancellationToken = default)
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
        var flatRate = ResolveFlatRate(step1.ServiceNeed);

        return new PropertyAdministratorTrashOutStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property.PropertyType == "ShortTermRental" ? "Guest checkout tomorrow" : null,
            ServiceNeed = step1.ServiceNeed,
            Bins = string.Join(",", step1.BinsList),
            BinCount = step1.BinCount,
            BinLocation = step1.BinLocation,
            PickupDay = step1.PickupDay,
            QuickNotes = step1.QuickNotes ?? "",
            UpdateRecipients = "",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "",
            ServiceTotalLabel = flatRate,
            ServiceTotalDescription = LabelServiceNeed(step1.ServiceNeed),
            AvailabilityLabel = step1.PickupDay == "Tomorrow" ? "Available tomorrow evening" : "Available for scheduling"
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorTrashOutSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = input.PickupDay == "Today"
            ? DateTime.Today.AddHours(19)
            : DateTime.Today.AddDays(1).AddHours(19);
        var flatRate = ResolveFlatRate(input.ServiceNeed);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", "Trash Out", property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Trash",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = PropertyAdministratorDisplayLocalization.L("7:00–9:00 PM"),
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Homecare runner • Trash Out"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Homecare runner",
            TechnicianRating = 4.8m,
            TechnicianTitle = "Verified",
            VehicleLabel = flatRate,
            TimelineStep = 2
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Trash out service",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = "7:00–9:00 PM",
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorTrashOutConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorTrashOutConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            Summary = BuildSummary(property, input),
            Timeline = BuildTimeline(input),
            ArrivalWindow = request.EtaLabel ?? "7:00–9:00 PM"
        };
    }

    private static PropertyAdministratorTrashOutSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorTrashOutSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorTrashOutSubmitInput>(json)
                ?? new PropertyAdministratorTrashOutSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorTrashOutSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorTrashOutSummaryItemViewModel> BuildSummary(
        IndorPropertyAdminPortfolioProperty? property, PropertyAdministratorTrashOutSubmitInput input) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Property"),
            Value = property != null ? $"Viewing: {property.PropertyName}" : "—",
            IconClass = "fa-house"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceNeed(input.ServiceNeed), IconClass = "fa-trash-arrow-up" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Bins"), Value = LabelBins(input.BinsList), IconClass = "fa-trash-can" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Quantity"), Value = LabelBinCount(input.BinCount), IconClass = "fa-hashtag" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Pickup day"), Value = LabelPickupDay(input.PickupDay), IconClass = "fa-calendar" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Timing"), Value = $"{LabelTakeOutTiming(input.TakeOutTiming)} / {LabelBringInTiming(input.BringInTiming)}", IconClass = "fa-clock" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Price"), Value = $"{ResolveFlatRate(input.ServiceNeed)} flat rate", IconClass = "fa-tag" }
    ];

    private static IReadOnlyList<PropertyAdministratorTrashOutTimelineItemViewModel> BuildTimeline(
        PropertyAdministratorTrashOutSubmitInput input)
    {
        var now = DateTime.Now;
        var pickupLabel = input.PickupDay == "Today" ? "This evening" : "Tomorrow evening";

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = $"Today, {now:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled"), StatusLabel = $"Today, {now:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Bins out for pickup"), StatusLabel = pickupLabel, IconClass = "fa-trash-arrow-up", State = "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Collection in progress"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-truck", State = "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Bins returned"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-trash-arrow-down", State = "pending" }
        ];
    }

    private static string ResolveFlatRate(string serviceNeed) => serviceNeed switch
    {
        "TakeBinsOut" => "$18",
        "BringBinsBackIn" => "$18",
        _ => "$30"
    };

    private static string LabelServiceNeed(string value) => value switch
    {
        "TakeBinsOut" => PropertyAdministratorDisplayLocalization.L("Take bins out"),
        "BringBinsBackIn" => PropertyAdministratorDisplayLocalization.L("Bring bins back in"),
        _ => PropertyAdministratorDisplayLocalization.L("Take out + bring back")
    };

    private static string LabelBins(IReadOnlyList<string> bins)
    {
        var labels = bins.Select(b => b switch
        {
            "Recycle" => PropertyAdministratorDisplayLocalization.L("Recycle"),
            "YardWaste" => PropertyAdministratorDisplayLocalization.L("Yard waste"),
            _ => PropertyAdministratorDisplayLocalization.L("Trash")
        }).Distinct().ToList();

        return labels.Count switch
        {
            0 => "—",
            1 => labels[0],
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels)
        };
    }

    private static string LabelBinCount(string value) => value switch
    {
        "One" => PropertyAdministratorDisplayLocalization.L("1 bin"),
        "ThreePlus" => PropertyAdministratorDisplayLocalization.L("3+ bins"),
        _ => PropertyAdministratorDisplayLocalization.L("2 bins")
    };

    private static string LabelPickupDay(string value) => value switch
    {
        "Today" => PropertyAdministratorDisplayLocalization.L("Today"),
        "Later" => PropertyAdministratorDisplayLocalization.L("Later"),
        _ => PropertyAdministratorDisplayLocalization.L("Tomorrow")
    };

    private static string LabelTakeOutTiming(string value) => value switch
    {
        "MorningOfPickup" => PropertyAdministratorDisplayLocalization.L("Morning out"),
        "CustomTime" => PropertyAdministratorDisplayLocalization.L("Custom time out"),
        _ => PropertyAdministratorDisplayLocalization.L("Evening out")
    };

    private static string LabelBringInTiming(string value) => value switch
    {
        "LateAfternoon" => PropertyAdministratorDisplayLocalization.L("Late afternoon back in"),
        "EndOfDay" => PropertyAdministratorDisplayLocalization.L("End of day back in"),
        _ => PropertyAdministratorDisplayLocalization.L("After collection back in")
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
            2 => $"{labels[0]} + {labels[1]}",
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