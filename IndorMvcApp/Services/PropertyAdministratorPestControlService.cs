using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorPestControlService
{
    PropertyAdministratorPestControlFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorPestControlStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPestControlStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorPestControlStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorPestControlSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPestControlConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorPestControlService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPestControlService
{
    public PropertyAdministratorPestControlFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("PestControlDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorPestControlStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        return new PropertyAdministratorPestControlStep1ViewModel
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

    public async Task<PropertyAdministratorPestControlStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorPestControlStep1Input step1, CancellationToken cancellationToken = default)
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
        var isRental = property.PropertyType == "ShortTermRental";
        var etaMinutes = step1.Urgency == "Emergency" ? 25 : step1.Urgency == "Urgent" ? 38 : 90;

        return new PropertyAdministratorPestControlStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = isRental ? PropertyAdministratorDisplayLocalization.L("Occupied now") : null,
            PestType = step1.PestType,
            IssueLocation = step1.IssueLocation,
            Urgency = step1.Urgency,
            GuestsStaying = step1.GuestsStaying,
            LivePestsToday = step1.LivePestsToday,
            QuickDetails = step1.QuickDetails ?? "",
            PreferredArrival = "",
            EntryAccess = "",
            HasPets = "",
            TreatAreas = "",
            UpdateRecipients = "",
            AccessNotes = "",
            ContactPhone = user?.PhoneNumber ?? admin.Phone ?? "",
            ProEtaLabel = $"Earliest available pest control pro: {etaMinutes} min"
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorPestControlSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var etaMinutes = input.Urgency == "Emergency" ? 25 : input.Urgency == "Urgent" ? 38 : 90;
        var visitDate = DateTime.Now.AddMinutes(etaMinutes);

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", "Pest Control", property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = input.Urgency is "Urgent" or "Emergency"
                ? PropertyAdministratorRequestStatuses.InProgress
                : PropertyAdministratorRequestStatuses.Open,
            Category = "PestControl",
            ScheduledUtc = visitDate,
            IsEmergency = input.Urgency == "Emergency",
            EtaLabel = $"{etaMinutes} min",
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Luis R. • Pest Control"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Luis R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Pest Control Pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("White service van"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Pest control treatment",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = LabelArrivalWindow(input.PreferredArrival),
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorPestControlConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorPestControlConfirmedViewModel
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
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Pest Control Pro",
            EtaLabel = request.EtaLabel ?? "38 min",
            VehicleLabel = request.VehicleLabel ?? "White service van",
            Summary = BuildSummary(input, property),
            Timeline = BuildTimeline(request)
        };
    }

    private static PropertyAdministratorPestControlSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorPestControlSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorPestControlSubmitInput>(json)
                ?? new PropertyAdministratorPestControlSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorPestControlSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorPestControlSummaryItemViewModel> BuildSummary(
        PropertyAdministratorPestControlSubmitInput input,
        IndorPropertyAdminPortfolioProperty? property) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Property"), Value = property?.PropertyName ?? "—", IconClass = "fa-house" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Pest issue"), Value = LabelPestType(input.PestType), IconClass = "fa-bug" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Location"), Value = LabelLocation(input.IssueLocation), IconClass = "fa-location-dot" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelServiceType(input.ServiceType), IconClass = "fa-spray-can" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Areas to treat"), Value = LabelTreatAreas(input.TreatAreasList), IconClass = "fa-border-all" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Access"), Value = LabelEntryAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorPestControlTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(4);
        var enRoute = submitted.AddMinutes(9);

        return
        [
            new() { Label = PropertyAdministratorDisplayLocalization.L("Request submitted"), StatusLabel = $"Today, {submitted:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Technician assigned"), StatusLabel = $"Today, {assigned:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("En route"), StatusLabel = $"Today, {enRoute:h:mm tt}", IconClass = "fa-location-crosshairs", State = "active" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Arrived"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-house", State = "pending" },
            new() { Label = PropertyAdministratorDisplayLocalization.L("Treatment started"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-spray-can", State = "pending" }
        ];
    }

    private static string LabelPestType(string value) => value switch
    {
        "Ants" => PropertyAdministratorDisplayLocalization.L("Ants"),
        "Rodents" => PropertyAdministratorDisplayLocalization.L("Rodents"),
        "Termites" => PropertyAdministratorDisplayLocalization.L("Termites"),
        "WaspsBees" => PropertyAdministratorDisplayLocalization.L("Wasps / bees"),
        "Spiders" => PropertyAdministratorDisplayLocalization.L("Spiders"),
        "Other" => PropertyAdministratorDisplayLocalization.L("Other"),
        _ => PropertyAdministratorDisplayLocalization.L("Roaches")
    };

    private static string LabelLocation(string value) => value switch
    {
        "Bathroom" => PropertyAdministratorDisplayLocalization.L("Bathroom"),
        "Bedroom" => PropertyAdministratorDisplayLocalization.L("Bedroom"),
        "PatioExterior" => PropertyAdministratorDisplayLocalization.L("Patio / exterior"),
        "Garage" => PropertyAdministratorDisplayLocalization.L("Garage"),
        "WholeProperty" => PropertyAdministratorDisplayLocalization.L("Whole property"),
        _ => PropertyAdministratorDisplayLocalization.L("Kitchen")
    };

    private static string LabelServiceType(string value) => value switch
    {
        "RecurringService" => PropertyAdministratorDisplayLocalization.L("Recurring service"),
        "InspectionOnly" => PropertyAdministratorDisplayLocalization.L("Inspection only"),
        _ => PropertyAdministratorDisplayLocalization.L("One-time treatment")
    };

    private static string LabelTreatAreas(IReadOnlyList<string> areas) =>
        areas.Count == 0 ? "—" : string.Join(", ", areas.Select(a => a switch
        {
            "Pantry" => PropertyAdministratorDisplayLocalization.L("Pantry"),
            "Bathroom" => PropertyAdministratorDisplayLocalization.L("Bathroom"),
            "Bedrooms" => PropertyAdministratorDisplayLocalization.L("Bedrooms"),
            "PatioExterior" => PropertyAdministratorDisplayLocalization.L("Patio / exterior"),
            _ => PropertyAdministratorDisplayLocalization.L("Kitchen")
        }));

    private static string LabelEntryAccess(string value) => value switch
    {
        "HostOnSite" => PropertyAdministratorDisplayLocalization.L("Host will meet"),
        "GuestApproval" => PropertyAdministratorDisplayLocalization.L("Need guest approval"),
        _ => PropertyAdministratorDisplayLocalization.L("Smart lock access")
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

    private static string LabelArrivalWindow(string value) => value switch
    {
        "TodayLater" => PropertyAdministratorDisplayLocalization.L("Later today"),
        "Tomorrow" => PropertyAdministratorDisplayLocalization.L("Tomorrow"),
        _ => PropertyAdministratorDisplayLocalization.L("ASAP")
    };

    private async Task<ApplicationUser?> GetUserAsync()
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        return string.IsNullOrEmpty(userId) ? null : await userManager.FindByIdAsync(userId);
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