using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorLawnCareService
{
    PropertyAdministratorLawnCareFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorLawnCareStep1ViewModel> GetStep1Async(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorLawnCareStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorLawnCareStep1Input step1, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorLawnCareSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorLawnCareConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorLawnCareService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorLawnCareService
{
    public PropertyAdministratorLawnCareFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("LawnCareDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorLawnCareStep1ViewModel> GetStep1Async(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);

        return new PropertyAdministratorLawnCareStep1ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property?.PropertyType == "ShortTermRental" ? "Guest check-in tomorrow" : null
        };
    }

    public async Task<PropertyAdministratorLawnCareStep2ViewModel?> GetStep2Async(
        IUrlHelper url, PropertyAdministratorLawnCareStep1Input step1, CancellationToken cancellationToken = default)
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

        return new PropertyAdministratorLawnCareStep2ViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property.PropertyType == "ShortTermRental" ? "Guest check-in tomorrow" : null,
            ServiceType = step1.ServiceType,
            YardArea = step1.YardArea,
            YardSize = step1.YardSize,
            Frequency = step1.Frequency,
            IsOccupied = step1.IsOccupied,
            AccessDetails = step1.AccessDetails ?? "",
            QuickNotes = step1.QuickNotes ?? "",
            UpdateRecipients = property.PropertyType == "ShortTermRental" ? "Me,Guest" : "Me"
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorLawnCareSubmitInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).FirstOrDefault()
            ?? throw new InvalidOperationException("No portfolio property found.");

        var detailsJson = JsonSerializer.Serialize(input);
        var visitDate = DateTime.Today.AddDays(1).AddHours(8);
        var etaLabel = $"Tomorrow, {LabelArrivalWindow(input.ArrivalWindow)}";

        var request = new IndorPropertyAdminServiceRequest
        {
            AdministratorId = admin.Id,
            PortfolioPropertyId = property.Id,
            Title = PropertyAdministratorDisplayLocalization.T("{0} at {1}", "Lawn Care", property.PropertyName),
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Lawn",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = etaLabel,
            TeamLabel = PropertyAdministratorDisplayLocalization.L("Miguel R. • Lawn Care"),
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Miguel R.",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Verified lawn care pro",
            VehicleLabel = PropertyAdministratorDisplayLocalization.L("Service truck"),
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Lawn care visit",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = LabelArrivalWindow(input.ArrivalWindow),
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorLawnCareConfirmedViewModel?> GetConfirmedAsync(
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

        return new PropertyAdministratorLawnCareConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Miguel R.",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianTitle = request.TechnicianTitle ?? "Verified lawn care pro",
            EtaLabel = request.EtaLabel ?? "Tomorrow, 8:00–12:00",
            VehicleLabel = request.VehicleLabel ?? "Service truck",
            Summary = BuildSummary(input),
            Timeline = BuildTimeline(input)
        };
    }

    private static PropertyAdministratorLawnCareSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorLawnCareSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorLawnCareSubmitInput>(json)
                ?? new PropertyAdministratorLawnCareSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorLawnCareSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorLawnCareSummaryItemViewModel> BuildSummary(
        PropertyAdministratorLawnCareSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Service"), Value = LabelService(input), IconClass = "fa-seedling" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Area"), Value = LabelArea(input.YardArea), IconClass = "fa-tree" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Yard size"), Value = LabelYardSize(input.YardSize), IconClass = "fa-ruler-combined" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Frequency"), Value = LabelFrequency(input.Frequency), IconClass = "fa-calendar" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Occupied"), Value = input.IsOccupied == "Yes" ? "Yes" : "No", IconClass = "fa-house-user" },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Updates"), Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorLawnCareTimelineItemViewModel> BuildTimeline(
        PropertyAdministratorLawnCareSubmitInput input) =>
    [
        new() { Label = PropertyAdministratorDisplayLocalization.L("Request confirmed"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Done"), IconClass = "fa-circle-check", State = "done", StepNumber = 1 },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Pro assigned"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Done"), IconClass = "fa-circle-check", State = "done", StepNumber = 2 },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Scheduled arrival"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Tomorrow, 8:00 AM"), IconClass = "fa-calendar", State = "active", StepNumber = 3 },
        new() { Label = PropertyAdministratorDisplayLocalization.L("Photos uploaded"), StatusLabel = PropertyAdministratorDisplayLocalization.L("Pending"), IconClass = "fa-camera", State = "pending", StepNumber = 4 }
    ];

    private static string LabelService(PropertyAdministratorLawnCareSubmitInput input)
    {
        var parts = new List<string>
        {
            input.ServiceType switch
            {
                "Edging" => PropertyAdministratorDisplayLocalization.L("Edging"),
                "WeedTrimming" => PropertyAdministratorDisplayLocalization.L("Weed trimming"),
                "FullYardRefresh" => PropertyAdministratorDisplayLocalization.L("Full yard refresh"),
                _ => PropertyAdministratorDisplayLocalization.L("Grass cutting")
            }
        };

        foreach (var addon in input.AddOnsList)
        {
            var label = addon switch
            {
                "BlowDebris" => "blow debris",
                "WeedTrim" => "weed trim",
                "LeafPickup" => "leaf pickup",
                "Edging" => "edging",
                _ => null
            };

            if (label != null && !parts.Any(p => string.Equals(p, label, StringComparison.OrdinalIgnoreCase)))
            {
                parts.Add(label);
            }
        }

        return string.Join(" + ", parts);
    }

    private static string LabelArea(string value) => value switch
    {
        "FrontYard" => PropertyAdministratorDisplayLocalization.L("Front yard"),
        "Backyard" => PropertyAdministratorDisplayLocalization.L("Backyard"),
        _ => PropertyAdministratorDisplayLocalization.L("Front yard + backyard")
    };

    private static string LabelYardSize(string value) => value switch
    {
        "Small" => PropertyAdministratorDisplayLocalization.L("Small"),
        "Large" => PropertyAdministratorDisplayLocalization.L("Large"),
        _ => PropertyAdministratorDisplayLocalization.L("Medium")
    };

    private static string LabelFrequency(string value) => value switch
    {
        "Weekly" => PropertyAdministratorDisplayLocalization.L("Weekly"),
        "BiWeekly" => PropertyAdministratorDisplayLocalization.L("Bi-weekly"),
        _ => PropertyAdministratorDisplayLocalization.L("One-time")
    };

    private static string LabelArrivalWindow(string value) => value switch
    {
        "Afternoon" => PropertyAdministratorDisplayLocalization.L("12:00–4:00 PM"),
        "Evening" => PropertyAdministratorDisplayLocalization.L("4:00–7:00 PM"),
        _ => PropertyAdministratorDisplayLocalization.L("8:00–12:00 PM")
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