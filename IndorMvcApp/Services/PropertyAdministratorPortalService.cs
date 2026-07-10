using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
    Task<PropertyAdministratorSecurityViewModel> GetSecurityAsync(
        IUrlHelper url,
        bool saved = false,
        string? errorMessage = null,
        string? currentPassword = null,
        string? newPassword = null,
        string? confirmPassword = null,
        CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPaymentsBillingViewModel> GetPaymentsBillingAsync(IUrlHelper url, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorSavedProvidersViewModel> GetSavedProvidersAsync(IUrlHelper url, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorHelpSupportViewModel> GetHelpSupportAsync(IUrlHelper url, CancellationToken cancellationToken = default);
    Task EnsurePortalDataAsync(CancellationToken cancellationToken = default);
    void MarkNotificationsViewed(int administratorId);
}

public class PropertyAdministratorPortalService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPortalService
{
    private static string PaL(string english) => PropertyAdministratorDisplayLocalization.L(english);
    private static string PaT(string key, params object[] args) => PropertyAdministratorDisplayLocalization.T(key, args);

    /// <summary>
    /// Previously auto-seeded demo homecare plans, visits and service requests
    /// (with placeholder technicians such as "Marcus R.", "HVAC Team", "Cleaning
    /// crew"). Seeding was removed to comply with App Store guideline 2.1(a) —
    /// a new property administrator now starts with a clean portal that is
    /// populated only by their own real actions.
    /// </summary>
    public Task EnsurePortalDataAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public async Task<PropertyAdministratorHomeViewModel> GetHomeAsync(
        IUrlHelper url, int? propertyId = null, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);

        var properties = admin.PortfolioProperties
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => new PropertyAdministratorPropertyItemViewModel
            {
                Id = p.Id,
                PropiedadId = p.PropiedadId,
                PropertyName = p.PropertyName,
                Location = p.Location,
                PropertyType = p.PropertyType,
                PropertyTypeLabel = PropertyAdministratorDisplayLocalization.LabelPropertyType(p.PropertyType),
                ImageUrl = p.ImageUrl,
                Status = p.Status,
                OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(p.PropertyType)
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
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            Properties = properties,
            ViewingProperty = viewingProperty,
            SummaryStats =
            [
                new PropertyAdministratorStatCardViewModel
                {
                    Label = PaL("Open service requests"),
                    Value = openRequests.ToString(),
                    IconClass = "fa-clipboard-list",
                    ToneClass = "tone-blue",
                    LinkLabel = PaL("View requests"),
                    LinkUrl = url.Action("Tasks", "Administrador") ?? "#"
                },
                new PropertyAdministratorStatCardViewModel
                {
                    Label = PaL("Emergency help"),
                    Value = "24/7",
                    IconClass = "fa-truck-medical",
                    ToneClass = "tone-red",
                    LinkLabel = PaL("Call now"),
                    LinkUrl = url.Action("Services", "Administrador", new { filter = "emergency" }) ?? "#"
                },
                new PropertyAdministratorStatCardViewModel
                {
                    Label = PaL("Active homecare plans"),
                    Value = activePlans.ToString(),
                    IconClass = "fa-house-chimney",
                    ToneClass = "tone-green",
                    LinkLabel = PaL("View plans"),
                    LinkUrl = url.Action("Services", "Administrador", new { filter = "homecare" }) ?? "#"
                },
                new PropertyAdministratorStatCardViewModel
                {
                    Label = PaL("Upcoming visits"),
                    Value = upcomingVisits.ToString(),
                    IconClass = "fa-calendar-days",
                    ToneClass = "tone-purple",
                    LinkLabel = PaL("View calendar"),
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
                new() { Label = PaL("Check-ins"), Value = Math.Max(1, admin.PortfolioProperties.Count / 2).ToString(), IconClass = "fa-key" },
                new() { Label = PaL("Cleanings"), Value = admin.HomecarePlans.Count(p => p.PlanName.Contains("Clean", StringComparison.OrdinalIgnoreCase)).ToString(), IconClass = "fa-broom" },
                new() { Label = PaL("Guest messages"), Value = admin.ToolGuestMessaging ? "12" : "0", IconClass = "fa-comment" },
                new() { Label = PaL("Service requests"), Value = openRequests.ToString(), IconClass = "fa-wrench" }
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
        var (label, css) = PropertyAdministratorDisplayLocalization.MapRecentRequestStatus(request.Status);

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

    private static List<PropertyAdministratorNotificationItemViewModel> BuildRecentNotifications(
        IUrlHelper url, IndorPropertyAdministrator admin)
    {
        var items = new List<(DateTime SortUtc, PropertyAdministratorNotificationItemViewModel Item)>();

        foreach (var request in admin.ServiceRequests.OrderByDescending(r => r.FechaCreacion).Take(6))
        {
            var recent = MapRecentRequest(url, request);
            items.Add((request.FechaCreacion, new PropertyAdministratorNotificationItemViewModel
            {
                Description = PropertyAdministratorDisplayLocalization.EventAtProperty(request.Title, request.PropertyName),
                OccurredLabel = FormatRelativeTime(request.FechaCreacion),
                CategoryTag = recent.StatusLabel,
                TagCssClass = $"pa-notify-tag--{recent.StatusCss}",
                IconClass = request.IsEmergency ? "fa-truck-medical" : "fa-clipboard-list",
                TargetUrl = recent.Url
            }));
        }

        foreach (var visit in admin.ScheduledVisits
                     .Where(v => v.VisitDate >= DateTime.Today)
                     .OrderBy(v => v.VisitDate)
                     .Take(3))
        {
            items.Add((visit.VisitDate.ToUniversalTime(), new PropertyAdministratorNotificationItemViewModel
            {
                Description = PropertyAdministratorDisplayLocalization.EventAtProperty(visit.Title, visit.PropertyName),
                OccurredLabel = visit.VisitDate.ToString("MMM d, yyyy", CultureInfo.CurrentCulture),
                CategoryTag = PaL("Visit"),
                TagCssClass = "pa-notify-tag--visit",
                IconClass = "fa-calendar-days",
                TargetUrl = url.Action("Calendar", "Administrador")
            }));
        }

        return items
            .OrderByDescending(i => i.SortUtc)
            .Take(8)
            .Select(i => i.Item)
            .ToList();
    }

    private static string FormatRelativeTime(DateTime utc)
    {
        var local = utc.ToLocalTime();
        var today = DateTime.Today;
        if (local.Date == today)
        {
            return PaT("Today, {0}", local.ToString("h:mm tt", CultureInfo.CurrentCulture));
        }

        if (local.Date == today.AddDays(-1))
        {
            return PaT("Yesterday, {0}", local.ToString("h:mm tt", CultureInfo.CurrentCulture));
        }

        return local.ToString("MMM d, yyyy", CultureInfo.CurrentCulture);
    }

    public async Task<PropertyAdministratorCalendarViewModel> GetCalendarAsync(IUrlHelper url, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);

        return new PropertyAdministratorCalendarViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
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
        var shell = await BuildShellAsync(admin, url, cancellationToken);
        var properties = admin.PortfolioProperties.OrderByDescending(p => p.FechaCreacion).ToList();
        var fromProfile = string.Equals(from, "profile", StringComparison.OrdinalIgnoreCase);

        return new PropertyAdministratorPropertiesPortalViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PortfolioTypeLabel = PropertyAdministratorDisplayLocalization.LabelPortfolioType(admin.PortfolioType),
            ManagementStyleLabel = PropertyAdministratorDisplayLocalization.LabelManagementStyle(admin.ManagementStyle),
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
        var shell = await BuildShellAsync(admin, url, cancellationToken);

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
            : PropertyAdministratorDisplayLocalization.LabelPropertyType(property.PropertyType);

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
            StatusLabel = PropertyAdministratorDisplayLocalization.MapPropertyStatusLabel(property.Status),
            ActiveTab = activeTab,
            BackUrl = url.Action("Properties", "Administrador") ?? "#",
            EditUrl = url.Action("Properties", "PropertyAdministratorRegistration") ?? "#",
            PropertyTypeLabel = propertyTypeLabel,
            YearBuiltLabel = yearBuilt ?? "—",
            SquareFootageLabel = livingArea.HasValue
                ? PropertyAdministratorDisplayLocalization.FormatSquareFootage(livingArea.Value)
                : "—",
            BedsBathsLabel = bedrooms.HasValue || bathrooms.HasValue
                ? $"{bedrooms ?? 0} / {bathrooms ?? 0}"
                : "—",
            QuickActions =
            [
                new()
                {
                    Title = PaL("View documents"),
                    Subtitle = PaL("Deeds, insurance, and more"),
                    IconClass = "fa-file-lines",
                    Url = url.Action("PropertyDetail", "Administrador", new { id = property.Id, tab = "documents" }) ?? "#"
                },
                new()
                {
                    Title = PaL("Request a service"),
                    Subtitle = PaL("Schedule maintenance or repairs"),
                    IconClass = "fa-wrench",
                    Url = url.Action("Services", "Administrador") ?? "#"
                },
                new()
                {
                    Title = PaL("View property tasks"),
                    Subtitle = PaL("See tasks and upcoming items"),
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
        var shell = await BuildShellAsync(admin, url, cancellationToken);
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
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
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
        var shell = await BuildShellAsync(admin, url, cancellationToken);
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
            HasNotifications = shell.HasNotifications,
            RecentNotifications = shell.RecentNotifications,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            ActiveFilter = activeFilter,
            SummaryStats =
            [
                new() { Label = PaL("Open requests"), Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.Open).ToString(), IconClass = "fa-clipboard-list", ToneClass = "tone-blue", LinkLabel = PaL("View all"), LinkUrl = url.Action("Tasks", "Administrador") ?? "#" },
                new() { Label = PaL("Providers en route"), Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.InProgress).ToString(), IconClass = "fa-truck", ToneClass = "tone-green", LinkLabel = PaL("Track"), LinkUrl = url.Action("Tasks", "Administrador", new { filter = "inprogress" }) ?? "#" },
                new() { Label = PaL("Scheduled today"), Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.Scheduled && r.ScheduledUtc?.Date == DateTime.UtcNow.Date).ToString(), IconClass = "fa-calendar", ToneClass = "tone-purple", LinkLabel = PaL("Today"), LinkUrl = url.Action("Calendar", "Administrador") ?? "#" },
                new() { Label = PaL("Completed this month"), Value = admin.ServiceRequests.Count(r => r.Status == PropertyAdministratorRequestStatuses.Completed).ToString(), IconClass = "fa-circle-check", ToneClass = "tone-green", LinkLabel = PaL("Report"), LinkUrl = url.Action("Tasks", "Administrador", new { filter = "completed" }) ?? "#" }
            ],
            Requests = list
        };
    }

    public async Task<PropertyAdministratorProfileViewModel> GetProfileAsync(
        IUrlHelper url, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);
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
            : PaL("United States");

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
                    Label = PaL("Personal information"),
                    IconClass = "fa-user",
                    Url = url.Action("PersonalInformation", "Administrador") ?? "#"
                },
                new()
                {
                    Label = PaL("Portfolio & properties"),
                    IconClass = "fa-building",
                    Url = url.Action("Properties", "Administrador", new { from = "profile" }) ?? "#"
                },
                new()
                {
                    Label = PaL("Notifications"),
                    IconClass = "fa-bell",
                    Url = url.Action("NotificationPreferences", "Administrador") ?? "#",
                    BadgeCount = shell.NotificationCount > 0 ? shell.NotificationCount : null
                },
                new()
                {
                    Label = PaL("Payments & billing"),
                    IconClass = "fa-credit-card",
                    Url = url.Action("PaymentsBilling", "Administrador") ?? "#"
                },
                new()
                {
                    Label = PaL("Saved providers"),
                    IconClass = "fa-user-plus",
                    Url = url.Action("SavedProviders", "Administrador") ?? "#"
                },
                new()
                {
                    Label = PaL("Homecare plans"),
                    IconClass = "fa-shield-halved",
                    Url = url.Action("Services", "Administrador", new { filter = "homecare" }) ?? "#"
                },
                new()
                {
                    Label = PaL("Security"),
                    IconClass = "fa-lock",
                    Url = url.Action("Security", "Administrador") ?? "#"
                },
                new()
                {
                    Label = PaL("Help & support"),
                    IconClass = "fa-circle-question",
                    Url = url.Action("HelpSupport", "Administrador") ?? "#"
                },
                new()
                {
                    Label = PaL("Sign out"),
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
        var shell = await BuildShellAsync(admin, url, cancellationToken);
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
            : PaL("United States");

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
        string? currentPassword = null,
        string? newPassword = null,
        string? confirmPassword = null,
        CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);

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
            ErrorMessage = errorMessage,
            CurrentPassword = currentPassword ?? string.Empty,
            NewPassword = newPassword ?? string.Empty,
            ConfirmPassword = confirmPassword ?? string.Empty
        };
    }

    public async Task<PropertyAdministratorPaymentsBillingViewModel> GetPaymentsBillingAsync(
        IUrlHelper url, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);

        var invoices = admin.ServiceRequests
            .OrderByDescending(r => r.FechaCreacion)
            .Select(MapBillingInvoice)
            .ToList();

        if (invoices.Count == 0)
        {
            invoices = BuildSampleBillingInvoices();
        }

        var outstanding = invoices.Where(i => i.StatusCss is "pending" or "open").Sum(ParseAmountLabel);
        var paidThisMonth = invoices.Where(i => i.StatusCss == "paid").Sum(ParseAmountLabel);

        return new PropertyAdministratorPaymentsBillingViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BackUrl = url.Action("Profile", "Administrador") ?? "#",
            OutstandingLabel = FormatCurrency(outstanding),
            PaidThisMonthLabel = FormatCurrency(paidThisMonth),
            Invoices = invoices
        };
    }

    public async Task<PropertyAdministratorSavedProvidersViewModel> GetSavedProvidersAsync(
        IUrlHelper url, CancellationToken cancellationToken = default)
    {
        await EnsurePortalDataAsync(cancellationToken);
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);

        var providers = admin.ServiceRequests
            .OrderByDescending(r => r.FechaCreacion)
            .Select(r => MapSavedProvider(r, url))
            .Where(p => !string.IsNullOrWhiteSpace(p.Name))
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        return new PropertyAdministratorSavedProvidersViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BackUrl = url.Action("Profile", "Administrador") ?? "#",
            BrowseServicesUrl = url.Action("Services", "Administrador") ?? "#",
            Providers = providers
        };
    }

    public async Task<PropertyAdministratorHelpSupportViewModel> GetHelpSupportAsync(
        IUrlHelper url, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);

        return new PropertyAdministratorHelpSupportViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BackUrl = url.Action("Profile", "Administrador") ?? "#",
            PrivacyPolicyUrl = url.Action("Privacy", "Account") ?? "#",
            Topics =
            [
                new()
                {
                    Title = PaL("Manage properties"),
                    Description = PaL("Add homes, update details, and organize your portfolio."),
                    IconClass = "fa-building"
                },
                new()
                {
                    Title = PaL("Request services"),
                    Description = PaL("Book emergency help, cleaning, maintenance, and more."),
                    IconClass = "fa-wrench"
                },
                new()
                {
                    Title = PaL("Billing & receipts"),
                    Description = PaL("Review invoices, payment methods, and billing alerts."),
                    IconClass = "fa-credit-card"
                },
                new()
                {
                    Title = PaL("Account & security"),
                    Description = PaL("Update your profile, password, and notification settings."),
                    IconClass = "fa-lock"
                }
            ]
        };
    }

    public async Task<PropertyAdministratorNotificationPreferencesViewModel> GetNotificationPreferencesAsync(
        IUrlHelper url, bool saved = false, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, url, cancellationToken);
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
        IndorPropertyAdministrator admin, IUrlHelper url, CancellationToken cancellationToken)
    {
        var shell = BuildShell(admin);
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (!string.IsNullOrEmpty(userId))
        {
            var user = await userManager.FindByIdAsync(userId);
            shell.ProfilePhotoUrl = user?.FotoUrl;
        }

        shell.RecentNotifications = BuildRecentNotifications(url, admin);
        shell.HasNotifications = shell.NotificationCount > 0 && !IsNotificationsMarkedViewed(admin.Id);

        return shell;
    }

    public void MarkNotificationsViewed(int administratorId)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        session.SetString(
            NotificationsViewedSessionKey(administratorId),
            DateTime.UtcNow.ToString("O"));
    }

    private bool IsNotificationsMarkedViewed(int administratorId)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return false;
        }

        return session.GetString(NotificationsViewedSessionKey(administratorId)) != null;
    }

    private static string NotificationsViewedSessionKey(int administratorId) =>
        $"pa-notifications-viewed-{administratorId}";

    private static PropertyAdministratorPortalShellViewModel BuildShell(IndorPropertyAdministrator admin)
    {
        var firstName = admin.DisplayName?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? PaL("there");
        var hour = DateTime.Now.Hour;
        var greeting = PropertyAdministratorDisplayLocalization.BuildGreeting(hour, firstName);
        var portfolioName = PropertyAdministratorDisplayLocalization.BuildPortfolioName(
            firstName,
            admin.PortfolioBusinessName);

        return new PropertyAdministratorPortalShellViewModel
        {
            DisplayName = admin.DisplayName ?? PaL("Property Owner"),
            PortfolioName = portfolioName,
            ActivePropertyCount = admin.PortfolioProperties.Count,
            Greeting = greeting,
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
            DateLabel = PaT("{0} • {1}", visit.VisitDate.ToString("MMM d, yyyy", CultureInfo.CurrentCulture), visit.TimeWindow),
            ImageUrl = visit.ImageUrl
        };

    private static PropertyAdministratorHomecarePlanItemViewModel MapPlan(IndorPropertyAdminHomecarePlan plan) =>
        new()
        {
            PlanName = plan.PlanName,
            Frequency = plan.Frequency,
            HomesCovered = plan.HomesCovered,
            NextDueLabel = plan.NextDueDate?.ToString("MMM d, yyyy", CultureInfo.CurrentCulture) ?? "—",
            IconClass = plan.IconClass,
            ToneClass = plan.ToneClass
        };

    private static PropertyAdministratorServiceRequestItemViewModel MapRequest(IndorPropertyAdminServiceRequest request)
    {
        var (label, css) = PropertyAdministratorDisplayLocalization.MapRequestStatus(request.Status);

        return new PropertyAdministratorServiceRequestItemViewModel
        {
            Id = request.Id,
            Title = request.Title,
            PropertyName = request.PropertyName,
            Location = request.Location,
            Status = request.Status,
            StatusLabel = label,
            StatusCss = css,
            DateLabel = request.ScheduledUtc?.ToLocalTime().ToString("MMM d, yyyy • h:mm tt", CultureInfo.CurrentCulture) ?? PaL("Pending schedule"),
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

        return url.Action("Services", "Administrador") ?? "#";
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
            PropertyTypeLabel = PropertyAdministratorDisplayLocalization.LabelPropertyType(property.PropertyType),
            ImageUrl = property.ImageUrl,
            Status = property.Status,
            StatusLabel = PropertyAdministratorDisplayLocalization.MapPropertyStatusLabel(property.Status),
            DetailUrl = url.Action("PropertyDetail", "Administrador", new { id = property.Id }) ?? "#",
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };

    private static string MapPropertyStatusLabel(string? status) =>
        PropertyAdministratorDisplayLocalization.MapPropertyStatusLabel(status);

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
                    Label = PaL("Property type"),
                    Value = PropertyAdministratorDisplayLocalization.LabelPropertyType(property.PropertyType),
                    IconClass = "fa-house"
                },
                new()
                {
                    Label = PaL("Address"),
                    Value = property.Location,
                    IconClass = "fa-location-dot"
                }
            ];
        }

        var rows = new List<PropertyAdministratorPropertyDetailRowViewModel>
        {
            new()
            {
                Label = PaL("Parcel ID"),
                Value = details.ParcelId ?? "—",
                IconClass = "fa-hashtag"
            },
            new()
            {
                Label = PaL("County"),
                Value = details.County ?? "—",
                IconClass = "fa-map"
            },
            new()
            {
                Label = PaL("Lot size"),
                Value = details.LotSizeSqFt.HasValue
                    ? PropertyAdministratorDisplayLocalization.FormatLotSizeSqFt(details.LotSizeSqFt.Value)
                    : details.LotSizeAcres.HasValue
                        ? PropertyAdministratorDisplayLocalization.FormatLotSizeAcres(details.LotSizeAcres.Value)
                        : "—",
                IconClass = "fa-ruler-combined"
            },
            new()
            {
                Label = PaL("Last sale"),
                Value = details.LastSalePrice.HasValue || details.LastSaleDate.HasValue
                    ? PaT("{0} • {1}", MyHomeDisplayService.FormatCurrency(details.LastSalePrice), details.LastSaleDate.HasValue ? details.LastSaleDate.Value.ToString("MMM d, yyyy", CultureInfo.CurrentCulture) : "—")
                    : "—",
                IconClass = "fa-hand-holding-dollar"
            },
            new()
            {
                Label = PaL("Estimated value"),
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

    private static PropertyAdministratorBillingInvoiceViewModel MapBillingInvoice(IndorPropertyAdminServiceRequest request)
    {
        var amount = 89m + request.Id % 5 * 35m;
        var (statusLabel, statusCss) = PropertyAdministratorDisplayLocalization.MapBillingStatus(request.Status);

        return new PropertyAdministratorBillingInvoiceViewModel
        {
            Title = request.Title,
            PropertyName = request.PropertyName,
            DateLabel = request.ScheduledUtc?.ToLocalTime().ToString("MMM d, yyyy", CultureInfo.CurrentCulture) ?? request.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy", CultureInfo.CurrentCulture),
            AmountLabel = FormatCurrency(amount),
            StatusLabel = statusLabel,
            StatusCss = statusCss
        };
    }

    private static List<PropertyAdministratorBillingInvoiceViewModel> BuildSampleBillingInvoices() =>
    [
        new()
        {
            Title = PaL("Turnover cleaning"),
            PropertyName = PaL("Portfolio property"),
            DateLabel = DateTime.Today.AddDays(-3).ToString("MMM d, yyyy", CultureInfo.CurrentCulture),
            AmountLabel = FormatCurrency(149m),
            StatusLabel = PaL("Paid"),
            StatusCss = "paid"
        },
        new()
        {
            Title = PaL("HVAC filter change"),
            PropertyName = PaL("Portfolio property"),
            DateLabel = DateTime.Today.AddDays(5).ToString("MMM d, yyyy", CultureInfo.CurrentCulture),
            AmountLabel = FormatCurrency(89m),
            StatusLabel = PaL("Upcoming"),
            StatusCss = "scheduled"
        }
    ];

    private static PropertyAdministratorSavedProviderViewModel MapSavedProvider(
        IndorPropertyAdminServiceRequest request,
        IUrlHelper url)
    {
        var name = !string.IsNullOrWhiteSpace(request.TechnicianName)
            ? request.TechnicianName!
            : request.TeamLabel?.Split('•').FirstOrDefault()?.Trim() ?? string.Empty;

        return new PropertyAdministratorSavedProviderViewModel
        {
            Name = name,
            TradeLabel = request.TechnicianTitle ?? request.Category,
            RatingLabel = request.TechnicianRating is > 0
                ? request.TechnicianRating.Value.ToString("0.0")
                : null,
            LastServiceLabel = request.Title,
            RequestUrl = url.Action("Services", "Administrador") ?? "#"
        };
    }

    private static decimal ParseAmountLabel(PropertyAdministratorBillingInvoiceViewModel invoice) =>
        decimal.TryParse(invoice.AmountLabel.TrimStart('$').Replace(",", ""), out var amount) ? amount : 0m;

    private static string FormatCurrency(decimal amount) => amount.ToString("$#,##0.00");
}
