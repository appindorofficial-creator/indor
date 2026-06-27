using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorPortalService
{
    Task<PropertyAdministratorHomeViewModel> GetHomeAsync(IUrlHelper url, int? propertyId = null, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorCalendarViewModel> GetCalendarAsync(IUrlHelper url, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPropertiesPortalViewModel> GetPropertiesAsync(IUrlHelper url, string? from = null, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPropertyDetailViewModel?> GetPropertyDetailAsync(IUrlHelper url, int propertyId, string? tab = null, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorServicesViewModel> GetServicesAsync(IUrlHelper url, string? filter, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorTasksViewModel> GetTasksAsync(IUrlHelper url, string? filter, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorProfileViewModel> GetProfileAsync(IUrlHelper url, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPersonalInformationViewModel> GetPersonalInformationAsync(IUrlHelper url, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorNotificationPreferencesViewModel> GetNotificationPreferencesAsync(IUrlHelper url, bool saved = false, CancellationToken cancellationToken = default);
    Task<bool> SaveNotificationPreferencesAsync(PropertyAdministratorNotificationPreferencesInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorSecurityViewModel> GetSecurityAsync(IUrlHelper url, bool saved = false, string? errorMessage = null, CancellationToken cancellationToken = default);
    Task EnsurePortalDataAsync(CancellationToken cancellationToken = default);
}

public class PropertyAdministratorPortalService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPortalService
{
    public async Task EnsurePortalDataAsync(CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken);
        if (admin == null)
        {
            return;
        }

        var hasData = await db.IndorPropertyAdminHomecarePlans
            .AnyAsync(p => p.AdministratorId == admin.Id, cancellationToken);
        if (hasData)
        {
            return;
        }

        var properties = admin.PortfolioProperties.OrderBy(p => p.Id).ToList();
        var propertyCount = Math.Max(properties.Count, 1);
        var homesCovered = properties.Count > 0 ? properties.Count : ParsePropertyCountRange(admin.PropertyCountRange);

        if (admin.ToolMaintenanceRequests || admin.ToolTurnoverCleaning)
        {
            db.IndorPropertyAdminHomecarePlans.AddRange(
                new IndorPropertyAdminHomecarePlan
                {
                    AdministratorId = admin.Id,
                    PlanName = "HVAC Filter Change",
                    Frequency = "Every 2 months",
                    HomesCovered = homesCovered,
                    NextDueDate = DateTime.Today.AddDays(18),
                    IconClass = "fa-fan",
                    ToneClass = "tone-blue",
                    Orden = 1
                },
                new IndorPropertyAdminHomecarePlan
                {
                    AdministratorId = admin.Id,
                    PlanName = "Smoke Detector Check",
                    Frequency = "Every 6 months",
                    HomesCovered = homesCovered,
                    NextDueDate = DateTime.Today.AddDays(12),
                    IconClass = "fa-bell",
                    ToneClass = "tone-green",
                    Orden = 2
                },
                new IndorPropertyAdminHomecarePlan
                {
                    AdministratorId = admin.Id,
                    PlanName = "Turnover Cleaning",
                    Frequency = "Per booking",
                    HomesCovered = homesCovered,
                    NextDueDate = DateTime.Today.AddDays(7),
                    IconClass = "fa-broom",
                    ToneClass = "tone-purple",
                    Orden = 3
                });
        }

        var visitTemplates = new[]
        {
            ("Smoke detector replacement", 16, "10:00 AM"),
            ("Pet deep clean", 17, "11:00 AM"),
            ("HVAC filter visit", 18, "2:00 PM")
        };

        for (var i = 0; i < visitTemplates.Length; i++)
        {
            var prop = properties.ElementAtOrDefault(i % Math.Max(properties.Count, 1));
            db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
            {
                AdministratorId = admin.Id,
                Title = visitTemplates[i].Item1,
                PropertyName = prop?.PropertyName ?? "Portfolio property",
                VisitDate = DateTime.Today.AddDays(visitTemplates[i].Item2),
                TimeWindow = visitTemplates[i].Item3,
                ImageUrl = prop?.ImageUrl ?? "/inspeccion2.jpeg"
            });
        }

        if (properties.Count > 0)
        {
            db.IndorPropertyAdminServiceRequests.Add(new IndorPropertyAdminServiceRequest
            {
                AdministratorId = admin.Id,
                PortfolioPropertyId = properties[0].Id,
                Title = "Power outage • Living room",
                PropertyName = properties[0].PropertyName,
                Location = properties[0].Location,
                Status = PropertyAdministratorRequestStatuses.InProgress,
                Category = "Emergency",
                ScheduledUtc = DateTime.UtcNow,
                EtaLabel = "24 min",
                TeamLabel = "Marcus R. • Electrical",
                ImageUrl = properties[0].ImageUrl,
                IsEmergency = true,
                TechnicianName = "Marcus R.",
                TechnicianRating = 4.9m,
                TechnicianTitle = "Licensed Electrical Pro",
                VehicleLabel = "White service van",
                TimelineStep = 2
            });

            db.IndorPropertyAdminServiceRequests.Add(new IndorPropertyAdminServiceRequest
            {
                AdministratorId = admin.Id,
                PortfolioPropertyId = properties[0].Id,
                Title = $"Emergency AC at {properties[0].PropertyName}",
                PropertyName = properties[0].PropertyName,
                Location = properties[0].Location,
                Status = PropertyAdministratorRequestStatuses.Emergency,
                Category = "Emergency",
                ScheduledUtc = DateTime.UtcNow,
                EtaLabel = "ETA 12 min",
                TeamLabel = "HVAC Team",
                ImageUrl = properties[0].ImageUrl,
                IsEmergency = true
            });

            if (properties.Count > 1)
            {
                db.IndorPropertyAdminServiceRequests.Add(new IndorPropertyAdminServiceRequest
                {
                    AdministratorId = admin.Id,
                    PortfolioPropertyId = properties[1].Id,
                    Title = $"Scheduled cleaning at {properties[1].PropertyName}",
                    PropertyName = properties[1].PropertyName,
                    Location = properties[1].Location,
                    Status = PropertyAdministratorRequestStatuses.Scheduled,
                    Category = "Cleaning",
                    ScheduledUtc = DateTime.UtcNow.AddDays(1),
                    EtaLabel = "Tomorrow 10:00 AM",
                    TeamLabel = "Cleaning crew",
                    ImageUrl = properties[1].ImageUrl
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PropertyAdministratorHomeViewModel> GetHomeAsync(
        IUrlHelper url, int? propertyId = null, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);

        var properties = admin.PortfolioProperties
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new PropertyAdministratorPropertyItemViewModel
            {
                Id = p.Id,
                PropiedadId = p.PropiedadId,
                PropertyName = p.PropertyName,
                Location = p.Location,
                PropertyType = p.PropertyType,
                PropertyTypeLabel = PropertyAdministratorCatalog.LabelPropertyType(p.PropertyType),
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                OccupancyLabel = p.PropertyType == "ShortTermRental" ? "Occupied now" : null
            })
            .ToList();

        var viewingProperty = propertyId.HasValue
            ? properties.FirstOrDefault(p => p.Id == propertyId.Value) ?? properties.FirstOrDefault()
            : properties.FirstOrDefault();

        var openRequests = admin.ServiceRequests.Count(r =>
            r.Status is PropertyAdministratorRequestStatuses.Open
                or PropertyAdministratorRequestStatuses.Emergency
                or PropertyAdministratorRequestStatuses.InProgress);
        var activePlans = admin.HomecarePlans.Count(p => p.Activo);
        var upcomingVisits = admin.ScheduledVisits.Count(v => v.VisitDate >= DateTime.Today);

        var catalog = await db.IndorPropertyAdminServiceCatalog.AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.CategoryOrder).ThenBy(c => c.Orden)
            .Take(12)
            .ToListAsync(cancellationToken);

        return new PropertyAdministratorHomeViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            Properties = properties,
            ViewingProperty = viewingProperty,
            SummaryStats =
            [
                new PropertyAdministratorStatCardViewModel
                {
                    Label = "Open service requests",
                    Value = openRequests.ToString(),
                    IconClass = "fa-clipboard-list",
                    ToneClass = "tone-blue",
                    LinkLabel = "View requests",
                    LinkUrl = url.Action("Tasks", "Administrador") ?? "#"
                },
                new PropertyAdministratorStatCardViewModel
                {
                    Label = "Emergency help",
                    Value = "24/7",
                    IconClass = "fa-truck-medical",
                    ToneClass = "tone-red",
                    LinkLabel = "Call now",
                    LinkUrl = url.Action("Services", "Administrador", new { filter = "emergency" }) ?? "#"
                },
                new PropertyAdministratorStatCardViewModel
                {
                    Label = "Active homecare plans",
                    Value = activePlans.ToString(),
                    IconClass = "fa-house-chimney",
                    ToneClass = "tone-green",
                    LinkLabel = "View plans",
                    LinkUrl = url.Action("Services", "Administrador", new { filter = "homecare" }) ?? "#"
                },
                new PropertyAdministratorStatCardViewModel
                {
                    Label = "Upcoming visits",
                    Value = upcomingVisits.ToString(),
                    IconClass = "fa-calendar-days",
                    ToneClass = "tone-purple",
                    LinkLabel = "View calendar",
                    LinkUrl = url.Action("Calendar", "Administrador") ?? "#"
                }
            ],
            ServiceHub = catalog.Select(c => new PropertyAdministratorServiceHubItemViewModel
            {
                Label = c.ServiceName,
                IconClass = c.IconClass,
                ToneClass = c.ToneClass,
                Url = BuildCatalogUrl(url, c)
            }).ToList(),
            TodayActivity =
            [
                new() { Label = "Check-ins", Value = Math.Max(1, admin.PortfolioProperties.Count / 2).ToString(), IconClass = "fa-key" },
                new() { Label = "Cleanings", Value = admin.HomecarePlans.Count(p => p.PlanName.Contains("Clean", StringComparison.OrdinalIgnoreCase)).ToString(), IconClass = "fa-broom" },
                new() { Label = "Guest messages", Value = admin.ToolGuestMessaging ? "12" : "0", IconClass = "fa-comment" },
                new() { Label = "Service requests", Value = openRequests.ToString(), IconClass = "fa-wrench" }
            ],
            UpcomingVisits = admin.ScheduledVisits
                .Where(v => v.VisitDate >= DateTime.Today)
                .OrderBy(v => v.VisitDate)
                .Take(6)
                .Select(v => MapVisit(v))
                .ToList(),
            HomecarePlans = admin.HomecarePlans.Where(p => p.Activo).OrderBy(p => p.Orden)
                .Select(MapPlan).ToList(),
            RecentRequests = admin.ServiceRequests
                .Where(r => r.Status is PropertyAdministratorRequestStatuses.InProgress
                    or PropertyAdministratorRequestStatuses.Emergency
                    or PropertyAdministratorRequestStatuses.Open)
                .OrderByDescending(r => r.FechaCreacion)
                .Take(3)
                .Select(r => MapRecentRequest(url, r))
                .ToList()
        };
    }

    private static PropertyAdministratorRecentRequestViewModel MapRecentRequest(
        IUrlHelper url, IndorPropertyAdminServiceRequest request)
    {
        var (label, css) = request.Status switch
        {
            PropertyAdministratorRequestStatuses.InProgress => ("En route", "inprogress"),
            PropertyAdministratorRequestStatuses.Emergency => ("Emergency", "emergency"),
            _ => ("Open", "open")
        };

        var trackUrl = request.Title.Contains("AC", StringComparison.OrdinalIgnoreCase)
            ? url.Action("EmergencyAcConfirmed", "Administrador", new { id = request.Id }) ?? "#"
            : request.Title.Contains("flood", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("flooding", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("mitigation", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Sewage", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Overflow", StringComparison.OrdinalIgnoreCase)
                ? url.Action("EmergencyFloodConfirmed", "Administrador", new { id = request.Id }) ?? "#"
            : request.Title.Contains("leak", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Plumb", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("toilet", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Clog", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Burst", StringComparison.OrdinalIgnoreCase)
                ? url.Action("EmergencyPlumbingConfirmed", "Administrador", new { id = request.Id }) ?? "#"
            : request.Title.Contains("outage", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Power", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Electrical", StringComparison.OrdinalIgnoreCase)
                || request.Title.Contains("Breaker", StringComparison.OrdinalIgnoreCase)
                ? url.Action("EmergencyElectricalConfirmed", "Administrador", new { id = request.Id }) ?? "#"
                : url.Action("Tasks", "Administrador", new { filter = "inprogress" }) ?? "#";

        return new PropertyAdministratorRecentRequestViewModel
        {
            Id = request.Id,
            Title = request.Title,
            PropertyName = request.PropertyName,
            StatusLabel = label,
            StatusCss = css,
            Url = trackUrl
        };
    }

    public async Task<PropertyAdministratorCalendarViewModel> GetCalendarAsync(IUrlHelper url, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);

        return new PropertyAdministratorCalendarViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            Visits = admin.ScheduledVisits.OrderBy(v => v.VisitDate)
                .Select(MapVisit).ToList(),
            ScheduledRequests = admin.ServiceRequests
                .Where(r => r.Status == PropertyAdministratorRequestStatuses.Scheduled)
                .OrderBy(r => r.ScheduledUtc)
                .Select(r => MapRequest(r))
                .ToList()
        };
    }

    public async Task<PropertyAdministratorPropertiesPortalViewModel> GetPropertiesAsync(
        IUrlHelper url, string? from = null, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var properties = admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).ToList();
        var fromProfile = string.Equals(from, "profile", StringComparison.OrdinalIgnoreCase);

        return new PropertyAdministratorPropertiesPortalViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PortfolioTypeLabel = PropertyAdministratorCatalog.LabelPortfolioType(admin.PortfolioType),
            ManagementStyleLabel = PropertyAdministratorCatalog.LabelManagementStyle(admin.ManagementStyle),
            TotalPropertyCount = properties.Count,
            ActivePropertiesCount = properties.Count(p => IsActivePropertyStatus(p.Status)),
            ServiceTasksPendingCount = admin.ServiceRequests.Count(r =>
                r.Status is PropertyAdministratorRequestStatuses.Open
                    or PropertyAdministratorRequestStatuses.Emergency
                    or PropertyAdministratorRequestStatuses.InProgress),
            ShowBackHeader = fromProfile,
            BackUrl = fromProfile
                ? url.Action("Profile", "Administrador") ?? "#"
                : url.Action("Index", "Administrador") ?? "#",
            AddPropertyUrl = url.Action("Properties", "PropertyAdministratorRegistration") ?? "#",
            Properties = properties
                .Select(p => MapPropertyListItem(p, url))
                .ToList()
        };
    }

    public async Task<PropertyAdministratorPropertyDetailViewModel?> GetPropertyDetailAsync(
        IUrlHelper url, int propertyId, string? tab = null, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);

        var property = await db.IndorPropertyAdminPortfolioProperties
            .AsNoTracking()
            .Include(p => p.Propiedad)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.AdministratorId == admin.Id, cancellationToken);
        if (property == null)
        {
            return null;
        }

        var activeTab = NormalizePropertyTab(tab);
        var propertyInfo = property.Propiedad != null
            ? MyHomeDisplayService.DeserializeProperty(property.Propiedad)
            : null;
        var summary = property.Propiedad != null
            ? MyHomeDisplayService.BuildSummary(property.Propiedad, propertyInfo)
            : null;
        var details = property.Propiedad != null
            ? MyHomeDisplayService.BuildDetails(property.Propiedad, propertyInfo)
            : null;

        var yearBuilt = summary?.YearBuilt?.ToString() ?? details?.YearBuilt?.ToString();
        var livingArea = summary?.LivingArea ?? details?.LivingArea;
        var bedrooms = summary?.Bedrooms ?? details?.Bedrooms;
        var bathrooms = summary?.Bathrooms ?? details?.Bathrooms;
        var propertyTypeLabel = !string.IsNullOrWhiteSpace(details?.PropertyType)
            ? details.PropertyType!
            : PropertyAdministratorCatalog.LabelPropertyType(property.PropertyType);

        var activityItems = admin.ServiceRequests
            .Where(r => r.PortfolioPropertyId == property.Id)
            .OrderByDescending(r => r.ScheduledUtc)
            .Select(MapRequest)
            .ToList();

        return new PropertyAdministratorPropertyDetailViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PropertyId = property.Id,
            PropertyName = property.PropertyName,
            Location = property.Location,
            ImageUrl = property.ImageUrl ?? "/inspeccion2.jpeg",
            StatusLabel = MapPropertyStatusLabel(property.Status),
            ActiveTab = activeTab,
            BackUrl = url.Action("Properties", "Administrador") ?? "#",
            EditUrl = url.Action("Properties", "PropertyAdministratorRegistration") ?? "#",
            PropertyTypeLabel = propertyTypeLabel,
            YearBuiltLabel = yearBuilt ?? "—",
            SquareFootageLabel = livingArea.HasValue
                ? $"{livingArea.Value:N0} sq ft"
                : "—",
            BedsBathsLabel = bedrooms.HasValue || bathrooms.HasValue
                ? $"{bedrooms ?? 0} / {bathrooms ?? 0}"
                : "—",
            QuickActions =
            [
                new()
                {
                    Title = "View documents",
                    Subtitle = "Deeds, insurance, and more",
                    IconClass = "fa-file-lines",
                    Url = url.Action("PropertyDetail", "Administrador", new { id = property.Id, tab = "documents" }) ?? "#"
                },
                new()
                {
                    Title = "Request a service",
                    Subtitle = "Schedule maintenance or repairs",
                    IconClass = "fa-wrench",
                    Url = url.Action("Services", "Administrador") ?? "#"
                },
                new()
                {
                    Title = "View property tasks",
                    Subtitle = "See tasks and upcoming items",
                    IconClass = "fa-list-check",
                    Url = url.Action("Tasks", "Administrador") ?? "#"
                }
            ],
            DetailRows = BuildPropertyDetailRows(details, property),
            ActivityItems = activityItems
        };
    }

    public async Task<PropertyAdministratorServicesViewModel> GetServicesAsync(
        IUrlHelper url, string? filter, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var activeFilter = string.IsNullOrWhiteSpace(filter) ? "all" : filter.Trim().ToLowerInvariant();

        var catalogQuery = db.IndorPropertyAdminServiceCatalog.AsNoTracking().Where(c => c.Activo);
        if (activeFilter != "all")
        {
            catalogQuery = catalogQuery.Where(c => c.CategoryKey == activeFilter);
        }

        var catalog = await catalogQuery
            .OrderBy(c => c.CategoryOrder).ThenBy(c => c.Orden)
            .ToListAsync(cancellationToken);

        var categories = catalog
            .GroupBy(c => new { c.CategoryKey, c.CategoryTitle, c.CategoryOrder })
            .OrderBy(g => g.Key.CategoryOrder)
            .Select(g => new PropertyAdministratorServiceCategoryViewModel
            {
                CategoryKey = g.Key.CategoryKey,
                CategoryTitle = g.Key.CategoryTitle,
                CategoryOrder = g.Key.CategoryOrder,
                Items = g.Select(item => new PropertyAdministratorServiceCatalogItemViewModel
                {
                    ServiceName = item.ServiceName,
                    IconClass = item.IconClass,
                    ToneClass = item.ToneClass,
                    Url = BuildCatalogUrl(url, item)
                }).ToList()
            }).ToList();

        return new PropertyAdministratorServicesViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ActiveFilter = activeFilter,
            Categories = categories,
            ActivePlans = admin.HomecarePlans.Where(p => p.Activo).OrderBy(p => p.Orden).Select(MapPlan).ToList()
        };
    }

    public async Task<PropertyAdministratorTasksViewModel> GetTasksAsync(
        IUrlHelper url, string? filter, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var activeFilter = string.IsNullOrWhiteSpace(filter) ? "all" : filter.Trim().ToLowerInvariant();

        var requests = admin.ServiceRequests.AsEnumerable();
        if (activeFilter != "all")
        {
            requests = activeFilter switch
            {
                "emergency" => requests.Where(r => r.IsEmergency || r.Status == PropertyAdministratorRequestStatuses.Emergency),
                "scheduled" => requests.Where(r => r.Status == PropertyAdministratorRequestStatuses.Scheduled),
                "inprogress" => requests.Where(r => r.Status == PropertyAdministratorRequestStatuses.InProgress),
                "completed" => requests.Where(r => r.Status == PropertyAdministratorRequestStatuses.Completed),
                _ => requests
            };
        }

        var list = requests.OrderByDescending(r => r.FechaCreacion).Select(MapRequest).ToList();

        return new PropertyAdministratorTasksViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ActiveFilter = activeFilter,
            SummaryStats =
            [
                new() { Label = "Open requests", Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.Open).ToString(), IconClass = "fa-clipboard-list", ToneClass = "tone-blue", LinkLabel = "View all", LinkUrl = url.Action("Tasks", "Administrador") ?? "#" },
                new() { Label = "Providers en route", Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.InProgress).ToString(), IconClass = "fa-truck", ToneClass = "tone-green", LinkLabel = "Track", LinkUrl = url.Action("Tasks", "Administrador", new { filter = "inprogress" }) ?? "#" },
                new() { Label = "Scheduled today", Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.Scheduled && r.ScheduledUtc?.Date == DateTime.UtcNow.Date).ToString(), IconClass = "fa-calendar", ToneClass = "tone-purple", LinkLabel = "Today", LinkUrl = url.Action("Calendar", "Administrador") ?? "#" },
                new() { Label = "Completed this month", Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.Completed).ToString(), IconClass = "fa-circle-check", ToneClass = "tone-green", LinkLabel = "Report", LinkUrl = url.Action("Tasks", "Administrador", new { filter = "completed" }) ?? "#" }
            ],
            Requests = list
        };
    }

    public async Task<PropertyAdministratorProfileViewModel> GetProfileAsync(
        IUrlHelper url, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        var user = !string.IsNullOrEmpty(userId)
            ? await userManager.FindByIdAsync(userId)
            : null;

        var profilePhoto = user?.FotoUrl ?? shell.ProfilePhotoUrl;
        var email = admin.Email ?? user?.Email ?? "";
        var phone = !string.IsNullOrWhiteSpace(admin.Phone)
            ? admin.Phone!
            : user?.PhoneNumber ?? user?.Telefono ?? "";
        var location = !string.IsNullOrWhiteSpace(admin.PrimaryMarket)
            ? admin.PrimaryMarket!
            : "United States";

        return new PropertyAdministratorProfileViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = profilePhoto,
            Email = email,
            Phone = phone,
            Location = location,
            MenuItems =
            [
                new()
                {
                    Label = "Personal information",
                    IconClass = "fa-user",
                    Url = url.Action("PersonalInformation", "Administrador") ?? "#"
                },
                new()
                {
                    Label = "Portfolio & properties",
                    IconClass = "fa-building",
                    Url = url.Action("Properties", "Administrador", new { from = "profile" }) ?? "#"
                },
                new()
                {
                    Label = "Notifications",
                    IconClass = "fa-bell",
                    Url = url.Action("NotificationPreferences", "Administrador") ?? "#",
                    BadgeCount = shell.NotificationCount > 0 ? shell.NotificationCount : null
                },
                new()
                {
                    Label = "Payments & billing",
                    IconClass = "fa-credit-card",
                    Url = url.Action("Services", "Administrador") ?? "#"
                },
                new()
                {
                    Label = "Saved providers",
                    IconClass = "fa-user-plus",
                    Url = url.Action("Services", "Administrador") ?? "#"
                },
                new()
                {
                    Label = "Homecare plans",
                    IconClass = "fa-shield-halved",
                    Url = url.Action("Services", "Administrador", new { filter = "homecare" }) ?? "#"
                },
                new()
                {
                    Label = "Security",
                    IconClass = "fa-lock",
                    Url = url.Action("Security", "Administrador") ?? "#"
                },
                new()
                {
                    Label = "Help & support",
                    IconClass = "fa-circle-question",
                    Url = url.Action("Index", "Home") ?? "#"
                },
                new()
                {
                    Label = "Sign out",
                    IconClass = "fa-right-from-bracket",
                    Url = url.Action("Logout", "Account") ?? "#",
                    IsDanger = true
                }
            ]
        };
    }

    public async Task<PropertyAdministratorPersonalInformationViewModel> GetPersonalInformationAsync(
        IUrlHelper url, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        var user = !string.IsNullOrEmpty(userId)
            ? await userManager.FindByIdAsync(userId)
            : null;

        var profilePhoto = user?.FotoUrl ?? shell.ProfilePhotoUrl;
        var email = admin.Email ?? user?.Email ?? "";
        var phone = !string.IsNullOrWhiteSpace(admin.Phone)
            ? admin.Phone!
            : user?.PhoneNumber ?? user?.Telefono ?? "";
        var location = !string.IsNullOrWhiteSpace(admin.PrimaryMarket)
            ? $"{admin.PrimaryMarket} United States"
            : "United States";

        return new PropertyAdministratorPersonalInformationViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = profilePhoto,
            FullName = shell.DisplayName,
            Email = email,
            Phone = phone,
            Address = location,
            MarketingEmailsEnabled = admin.MarketingOptIn,
            BackUrl = url.Action("Profile", "Administrador") ?? "#",
            PrivacyPolicyUrl = url.Action("Privacy", "Account") ?? "#",
            ChangePasswordUrl = url.Action("Security", "Administrador") ?? "#"
        };
    }

    public async Task<PropertyAdministratorSecurityViewModel> GetSecurityAsync(
        IUrlHelper url,
        bool saved = false,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);

        return new PropertyAdministratorSecurityViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BackUrl = url.Action("Profile", "Administrador") ?? "#",
            PrivacyPolicyUrl = url.Action("Privacy", "Account") ?? "#",
            PasswordChanged = saved,
            ErrorMessage = errorMessage
        };
    }

    public async Task<PropertyAdministratorNotificationPreferencesViewModel> GetNotificationPreferencesAsync(
        IUrlHelper url, bool saved = false, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var quietStart = NormalizeQuietHour(admin.QuietHoursStart, "22:00");
        var quietEnd = NormalizeQuietHour(admin.QuietHoursEnd, "07:00");

        return new PropertyAdministratorNotificationPreferencesViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BackUrl = url.Action("Profile", "Administrador") ?? "#",
            Saved = saved,
            PushEnabled = admin.NotifyPushEnabled,
            EmailEnabled = admin.NotifyEmailEnabled,
            SmsEnabled = admin.NotifySmsEnabled,
            PropertyUpdatesEnabled = admin.NotifyPropertyUpdates,
            ServiceUpdatesEnabled = admin.NotifyServiceUpdates,
            TaskRemindersEnabled = admin.NotifyTaskReminders,
            PaymentsBillingEnabled = admin.NotifyPaymentsBilling,
            PromotionsTipsEnabled = admin.MarketingOptIn,
            QuietHoursStart = quietStart,
            QuietHoursEnd = quietEnd,
            QuietHoursLabel = FormatQuietHoursLabel(quietStart, quietEnd)
        };
    }

    public async Task<bool> SaveNotificationPreferencesAsync(
        PropertyAdministratorNotificationPreferencesInput input, CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        var admin = await db.IndorPropertyAdministrators
            .FirstOrDefaultAsync(a => a.UserId == userId
                && a.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed, cancellationToken);
        if (admin == null)
        {
            return false;
        }

        admin.NotifyPushEnabled = input.PushEnabled;
        admin.NotifyEmailEnabled = input.EmailEnabled;
        admin.NotifySmsEnabled = input.SmsEnabled;
        admin.NotifyPropertyUpdates = input.PropertyUpdatesEnabled;
        admin.NotifyServiceUpdates = input.ServiceUpdatesEnabled;
        admin.NotifyTaskReminders = input.TaskRemindersEnabled;
        admin.NotifyPaymentsBilling = input.PaymentsBillingEnabled;
        admin.MarketingOptIn = input.PromotionsTipsEnabled;
        admin.QuietHoursStart = NormalizeQuietHour(input.QuietHoursStart, "22:00");
        admin.QuietHoursEnd = NormalizeQuietHour(input.QuietHoursEnd, "07:00");
        admin.NotifyBookingLeaseUpdates = input.PropertyUpdatesEnabled;
        admin.NotifyUrgentMaintenance = input.ServiceUpdatesEnabled;
        admin.NotifyWeeklySummary = input.TaskRemindersEnabled;
        admin.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<IndorPropertyAdministrator?> LoadAdminAsync(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await db.IndorPropertyAdministrators
            .AsNoTracking()
            .Include(a => a.PortfolioProperties)
            .Include(a => a.ServiceRequests)
            .Include(a => a.HomecarePlans)
            .Include(a => a.ScheduledVisits)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed, cancellationToken);
    }

    private async Task<PropertyAdministratorPortalShellViewModel> BuildShellAsync(
        IndorPropertyAdministrator admin, CancellationToken cancellationToken)
    {
        var shell = BuildShell(admin);
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await userManager.FindByIdAsync(userId);
            shell.ProfilePhotoUrl = user?.FotoUrl;
        }

        return shell;
    }

    private static PropertyAdministratorPortalShellViewModel BuildShell(IndorPropertyAdministrator admin)
    {
        var firstName = admin.DisplayName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";
        var hour = DateTime.Now.Hour;
        var greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        var portfolioName = !string.IsNullOrWhiteSpace(admin.PortfolioBusinessName)
            ? admin.PortfolioBusinessName
            : $"{firstName} Portfolio";

        return new PropertyAdministratorPortalShellViewModel
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
    }

    private static PropertyAdministratorVisitCardViewModel MapVisit(IndorPropertyAdminScheduledVisit visit) =>
        new()
        {
            Title = visit.Title,
            PropertyName = visit.PropertyName,
            DateLabel = $"{visit.VisitDate:MMM d, yyyy} • {visit.TimeWindow}",
            ImageUrl = visit.ImageUrl
        };

    private static PropertyAdministratorHomecarePlanItemViewModel MapPlan(IndorPropertyAdminHomecarePlan plan) =>
        new()
        {
            PlanName = plan.PlanName,
            Frequency = plan.Frequency,
            HomesCovered = plan.HomesCovered,
            NextDueLabel = plan.NextDueDate?.ToString("MMM d, yyyy") ?? "—",
            IconClass = plan.IconClass,
            ToneClass = plan.ToneClass
        };

    private static PropertyAdministratorServiceRequestItemViewModel MapRequest(IndorPropertyAdminServiceRequest request)
    {
        var (label, css) = request.Status switch
        {
            PropertyAdministratorRequestStatuses.Emergency => ("EMERGENCY", "emergency"),
            PropertyAdministratorRequestStatuses.Scheduled => ("SCHEDULED", "scheduled"),
            PropertyAdministratorRequestStatuses.InProgress => ("IN PROGRESS", "inprogress"),
            PropertyAdministratorRequestStatuses.Completed => ("COMPLETED", "completed"),
            _ => ("OPEN", "open")
        };

        return new PropertyAdministratorServiceRequestItemViewModel
        {
            Id = request.Id,
            Title = request.Title,
            PropertyName = request.PropertyName,
            Location = request.Location,
            Status = request.Status,
            StatusLabel = label,
            StatusCss = css,
            DateLabel = request.ScheduledUtc?.ToLocalTime().ToString("MMM d, yyyy • h:mm tt") ?? "Pending schedule",
            TeamLabel = request.TeamLabel,
            EtaLabel = request.EtaLabel,
            ImageUrl = request.ImageUrl,
            IsEmergency = request.IsEmergency
        };
    }

    private static string BuildCatalogUrl(IUrlHelper url, IndorPropertyAdminServiceCatalogItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.LinkController) && !string.IsNullOrWhiteSpace(item.LinkAction))
        {
            return item.LinkRouteId.HasValue
                ? url.Action(item.LinkAction, item.LinkController, new { id = item.LinkRouteId }) ?? "#"
                : url.Action(item.LinkAction, item.LinkController) ?? "#";
        }

        return url.Action("RequestService", "Administrador", new { service = item.ServiceSlug }) ?? "#";
    }

    private static PropertyAdministratorPropertyItemViewModel MapPropertyListItem(
        IndorPropertyAdminPortfolioProperty property, IUrlHelper url) =>
        new()
        {
            Id = property.Id,
            PropiedadId = property.PropiedadId,
            PropertyName = property.PropertyName,
            Location = property.Location,
            PropertyType = property.PropertyType,
            PropertyTypeLabel = PropertyAdministratorCatalog.LabelPropertyType(property.PropertyType),
            ImageUrl = property.ImageUrl,
            Status = property.Status,
            StatusLabel = MapPropertyStatusLabel(property.Status),
            DetailUrl = url.Action("PropertyDetail", "Administrador", new { id = property.Id }) ?? "#",
            OccupancyLabel = property.PropertyType == "ShortTermRental" ? "Occupied now" : null
        };

    private static string MapPropertyStatusLabel(string? status) =>
        string.IsNullOrWhiteSpace(status) || status is "Added" or "Active"
            ? "Active"
            : status;

    private static bool IsActivePropertyStatus(string? status) =>
        string.IsNullOrWhiteSpace(status) || status is "Added" or "Active";

    private static string NormalizePropertyTab(string? tab) => tab?.Trim().ToLowerInvariant() switch
    {
        "details" => "details",
        "documents" => "documents",
        "activity" => "activity",
        _ => "overview"
    };

    private static IReadOnlyList<PropertyAdministratorPropertyDetailRowViewModel> BuildPropertyDetailRows(
        MyHomePropertyDetailsViewModel? details,
        IndorPropertyAdminPortfolioProperty property)
    {
        if (details == null)
        {
            return
            [
                new()
                {
                    Label = "Property type",
                    Value = PropertyAdministratorCatalog.LabelPropertyType(property.PropertyType),
                    IconClass = "fa-house"
                },
                new()
                {
                    Label = "Address",
                    Value = property.Location,
                    IconClass = "fa-location-dot"
                }
            ];
        }

        var rows = new List<PropertyAdministratorPropertyDetailRowViewModel>
        {
            new()
            {
                Label = "Parcel ID",
                Value = details.ParcelId ?? "—",
                IconClass = "fa-hashtag"
            },
            new()
            {
                Label = "County",
                Value = details.County ?? "—",
                IconClass = "fa-map"
            },
            new()
            {
                Label = "Lot size",
                Value = details.LotSizeSqFt.HasValue
                    ? $"{details.LotSizeSqFt.Value:N0} sq ft"
                    : details.LotSizeAcres.HasValue
                        ? $"{details.LotSizeAcres.Value:N2} acres"
                        : "—",
                IconClass = "fa-ruler-combined"
            },
            new()
            {
                Label = "Last sale",
                Value = details.LastSalePrice.HasValue || details.LastSaleDate.HasValue
                    ? $"{MyHomeDisplayService.FormatCurrency(details.LastSalePrice)} • {(details.LastSaleDate.HasValue ? details.LastSaleDate.Value.ToString("MMM d, yyyy") : "—")}"
                    : "—",
                IconClass = "fa-hand-holding-dollar"
            },
            new()
            {
                Label = "Estimated value",
                Value = MyHomeDisplayService.FormatCurrency(details.EstimatedValue),
                IconClass = "fa-chart-line"
            }
        };

        return rows;
    }

    private static string NormalizeQuietHour(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return TimeOnly.TryParse(value, out var parsed)
            ? parsed.ToString("HH:mm")
            : fallback;
    }

    private static string FormatQuietHoursLabel(string start, string end)
    {
        static string Format(string value) =>
            TimeOnly.TryParse(value, out var time)
                ? time.ToString("h:mm tt")
                : value;

        return $"{Format(start)} - {Format(end)}";
    }

    private static int ParsePropertyCountRange(string? range) => range switch
    {
        "2-5" => 4,
        "6-10" => 8,
        "11-25" => 12,
        "25+" => 25,
        _ => 1
    };
}
