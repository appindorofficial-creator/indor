using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorMovingHelpService
{
    PropertyAdministratorMovingHelpFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorMovingHelpFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorMovingHelpReviewViewModel> GetReviewAsync(IUrlHelper url, PropertyAdministratorMovingHelpSubmitInput input, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorMovingHelpSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorMovingHelpConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorMovingHelpService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorMovingHelpService
{
    public PropertyAdministratorMovingHelpFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("MovingHelpDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorMovingHelpFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorMovingHelpFormViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = mapped,
            PropertyStatusLabel = property?.PropertyType == "ShortTermRental" ? "Guest checkout today" : null,
            Details = property?.PropertyType == "ShortTermRental"
                ? "Need help resetting furniture, moving supply bins, and staging bedrooms after checkout."
                : ""
        };
    }

    public async Task<PropertyAdministratorMovingHelpReviewViewModel> GetReviewAsync(
        IUrlHelper url, PropertyAdministratorMovingHelpSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? ResolveProperty(admin, input.PropertyId);

        return new PropertyAdministratorMovingHelpReviewViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            Input = input,
            ViewingProperty = MapProperty(property),
            SummaryRows = BuildSummaryRows(property, input)
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorMovingHelpSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = input.ScheduleWhen == "Today"
            ? DateTime.Today.AddHours(10)
            : DateTime.Today.AddDays(1).AddHours(10);
        var etaLabel = $"{LabelScheduleWhen(input.ScheduleWhen)} • {input.ScheduleTimeWindow}";

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = $"Moving Help at {property.PropertyName}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Moving",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = etaLabel,
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Luis R. • Moving"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Luis R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Verified Homecare Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Moving help visit",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = input.ScheduleTimeWindow,
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorMovingHelpConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorMovingHelpConfirmedViewModel
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
            TechnicianTitle = request.TechnicianTitle ?? "Verified Homecare Pro",
            TechnicianRole = "Moving & turnover support",
            ScheduleLabel = LabelScheduleWhen(input.ScheduleWhen),
            TimeWindowLabel = input.ScheduleTimeWindow,
            BookingDetails = BuildBookingDetails(input),
            Timeline = BuildTimeline(request, input)
        };
    }

    private static PropertyAdministratorMovingHelpSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorMovingHelpSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorMovingHelpSubmitInput>(json)
                ?? new PropertyAdministratorMovingHelpSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorMovingHelpSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorMovingHelpReviewRowViewModel> BuildSummaryRows(
        IndorPropertyAdminPortfolioProperty? property, PropertyAdministratorMovingHelpSubmitInput input) =>
    [
        new()
        {
            Label = PropertyAdministratorDisplayLocalization.L("Property"),
            Value = property != null
                ? $"Viewing: {property.PropertyName}\n{PropertyAdministratorDisplayLocalization.LabelPropertyType(property.PropertyType)} • {property.Location}"
                : "—",
            IconClass = "fa-house"
        },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-dolly" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Items"), Value = LabelItems(input.ItemsToMoveList), IconClass = "fa-couch" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Help needed"), Value = LabelHelpers(input.HelperCount), IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Date & time"), Value = $"{LabelScheduleWhen(input.ScheduleWhen)} • {input.ScheduleTimeWindow}", IconClass = "fa-calendar" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorMovingHelpReviewRowViewModel> BuildBookingDetails(
        PropertyAdministratorMovingHelpSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-tag" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Helpers"), Value = LabelHelpers(input.HelperCount), IconClass = "fa-users" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Items"), Value = LabelItems(input.ItemsToMoveList), IconClass = "fa-box" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorMovingHelpTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request, PropertyAdministratorMovingHelpSubmitInput input)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(4);
        var visitLabel = $"{LabelScheduleWhen(input.ScheduleWhen)}, {input.ScheduleTimeWindow}";

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = $"Today, {submitted:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Team assigned"), StatusLabel = $"Today, {assigned:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled visit"), StatusLabel = visitLabel, IconClass = "fa-calendar-check", State = "active" }
        ];
    }

    private static string LabelServiceType(string value) => value switch
    {
        "FurnitureMoving" => "Furniture moving",
        "BoxesSupplies" => "Boxes & supplies",
        "GuestMoveInOut" => "Guest move-in/out",
        _ => "Turnover setup"
    };

    private static string LabelItems(IReadOnlyList<string> items)
    {
        var labels = items.Select(i => i switch
        {
            "Boxes" => "boxes",
            "BeddingLinens" => "bedding / linens",
            "DecorSupplies" => "decor / supplies",
            _ => "small furniture"
        }).Distinct().ToList();

        return labels.Count switch
        {
            0 => "—",
            1 => labels[0],
            2 => $"{labels[0]} + {labels[1]}",
            _ => string.Join(" + ", labels)
        };
    }

    private static string LabelHelpers(string value) => value switch
    {
        "One" => "1 helper",
        "ThreePlus" => "3+ helpers",
        _ => "2 helpers"
    };

    private static string LabelScheduleWhen(string value) => value switch
    {
        "Today" => "Today",
        "Later" => "Later",
        _ => "Tomorrow"
    };

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => "Host will meet",
        "GuestOnSite" => "Guest on-site",
        _ => "Smart lock provided"
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
        var shell = PropertyAdministratorFlowServiceSupport.BuildShell(admin);

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await userManager.FindByIdAsync(userId);
            shell.ProfilePhotoUrl = user?.FotoUrl;
        }

        return shell;
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
