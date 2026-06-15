using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorPetDeepCleanService
{
    PropertyAdministratorPetDeepCleanFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorPetDeepCleanFormViewModel> GetFormAsync(IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default);
    Task<int> SubmitAsync(PropertyAdministratorPetDeepCleanSubmitInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPetDeepCleanConfirmedViewModel?> GetConfirmedAsync(IUrlHelper url, int requestId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorPetDeepCleanService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPetDeepCleanService
{
    public PropertyAdministratorPetDeepCleanFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("PetDeepCleanDetails", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorPetDeepCleanFormViewModel> GetFormAsync(
        IUrlHelper url, int? propertyId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var mapped = MapProperty(property);

        return new PropertyAdministratorPetDeepCleanFormViewModel
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
                ? "Guests checked out with 2 dogs. Please deep clean sofas, rugs, and bedrooms."
                : ""
        };
    }

    public async Task<int> SubmitAsync(
        PropertyAdministratorPetDeepCleanSubmitInput input, CancellationToken cancellationToken = default)
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
            Title = $"Pet Deep Clean at {property.PropertyName}",
            PropertyName = property.PropertyName,
            Location = property.Location,
            Status = PropertyAdministratorRequestStatuses.Open,
            Category = "Cleaning",
            ScheduledUtc = visitDate,
            IsEmergency = false,
            EtaLabel = etaLabel,
            TeamLabel = "Maria Gutierrez • Pet cleaning",
            ImageUrl = property.ImageUrl,
            DetailsJson = detailsJson,
            TechnicianName = "Maria Gutierrez",
            TechnicianRating = 4.9m,
            TechnicianTitle = "Licensed Homecare Pro",
            VehicleLabel = "White service van",
            TimelineStep = 3
        };

        db.IndorPropertyAdminServiceRequests.Add(request);

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Pet deep clean",
            PropertyName = property.PropertyName,
            VisitDate = visitDate.Date,
            TimeWindow = input.ScheduleTimeWindow,
            ImageUrl = property.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
        return request.Id;
    }

    public async Task<PropertyAdministratorPetDeepCleanConfirmedViewModel?> GetConfirmedAsync(
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
        var visitDate = request.ScheduledUtc?.Date ?? DateTime.Today.AddDays(1);

        return new PropertyAdministratorPetDeepCleanConfirmedViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount + 1,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            RequestId = request.Id,
            ViewingProperty = property == null ? null : MapProperty(property),
            TechnicianName = request.TechnicianName ?? "Maria Gutierrez",
            TechnicianRating = request.TechnicianRating ?? 4.9m,
            TechnicianReviewCount = 135,
            TechnicianTitle = request.TechnicianTitle ?? "Licensed Homecare Pro",
            TechnicianExperience = "8+ years of homecare experience",
            BookingDetails = BuildBookingDetails(property, input, visitDate),
            Timeline = BuildTimeline(request, visitDate, input)
        };
    }

    private static PropertyAdministratorPetDeepCleanSubmitInput DeserializeInput(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new PropertyAdministratorPetDeepCleanSubmitInput();
        }

        try
        {
            return JsonSerializer.Deserialize<PropertyAdministratorPetDeepCleanSubmitInput>(json)
                ?? new PropertyAdministratorPetDeepCleanSubmitInput();
        }
        catch
        {
            return new PropertyAdministratorPetDeepCleanSubmitInput();
        }
    }

    private static IReadOnlyList<PropertyAdministratorPetDeepCleanBookingItemViewModel> BuildBookingDetails(
        IndorPropertyAdminPortfolioProperty? property,
        PropertyAdministratorPetDeepCleanSubmitInput input,
        DateTime visitDate) =>
    [
        new() { Label = "Service", Value = "Pet Deep Cleaning", IconClass = "fa-paw" },
        new() { Label = "Date & time", Value = $"{visitDate:MMM d, yyyy} • {input.ScheduleTimeWindow}", IconClass = "fa-calendar" },
        new() { Label = "Pets", Value = LabelPets(input.PetCount, input.PetType), IconClass = "fa-dog" },
        new() { Label = "Focus areas", Value = LabelFocusAreas(input.FocusAreasList), IconClass = "fa-wand-magic-sparkles" },
        new() { Label = "Access method", Value = LabelAccess(input.EntryAccess), IconClass = "fa-key" },
        new() { Label = "Updates", Value = LabelUpdates(input.UpdateRecipientsList), IconClass = "fa-users" }
    ];

    private static IReadOnlyList<PropertyAdministratorPetDeepCleanTimelineItemViewModel> BuildTimeline(
        IndorPropertyAdminServiceRequest request, DateTime visitDate, PropertyAdministratorPetDeepCleanSubmitInput input)
    {
        var submitted = request.FechaCreacion.ToLocalTime();
        var assigned = submitted.AddMinutes(2);
        var visitLabel = $"{visitDate:MMM d, yyyy}, {input.ScheduleTimeWindow}";

        return
        [
            new() { Label = "Request submitted", StatusLabel = $"{submitted:MMM d, yyyy} • {submitted:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = "Crew assigned", StatusLabel = $"{assigned:MMM d, yyyy} • {assigned:h:mm tt}", IconClass = "fa-circle-check", State = "done" },
            new() { Label = "Scheduled visit", StatusLabel = visitLabel, IconClass = "fa-calendar-check", State = "active" }
        ];
    }

    private static string LabelPets(string count, string type)
    {
        var countLabel = count switch
        {
            "1" => "1",
            "3" => "3",
            "FourPlus" => "4+",
            _ => "2"
        };
        var typeLabel = type switch
        {
            "Cat" => countLabel == "1" ? "cat" : "cats",
            "Other" => "pets",
            _ => countLabel == "1" ? "dog" : "dogs"
        };
        return $"{countLabel} {typeLabel}";
    }

    private static string LabelFocusAreas(IReadOnlyList<string> areas)
    {
        var labels = areas.Select(a => a switch
        {
            "OdorRemoval" => "odors",
            "AccidentsStains" => "stains",
            "BedsUpholstery" => "beds & upholstery",
            "CratePetArea" => "crate / pet area",
            "Floors" => "floors",
            _ => "pet hair"
        }).Distinct().ToList();

        return labels.Count == 0 ? "—" : string.Join(", ", labels);
    }

    private static string LabelScheduleWhen(string value) => value switch
    {
        "Today" => "Today",
        "Later" => "Later",
        _ => "Tomorrow"
    };

    private static string LabelAccess(string value) => value switch
    {
        "HostMeet" => "Host will meet",
        "GuestStillInside" => "Guest still inside",
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
