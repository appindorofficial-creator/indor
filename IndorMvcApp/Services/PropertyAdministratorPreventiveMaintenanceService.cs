using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorPreventiveMaintenanceService
{
    PropertyAdministratorPreventiveFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId);
    Task<PropertyAdministratorPreventiveServicesStepViewModel> GetServicesStepAsync(IUrlHelper url, int? propertyId, int? planId, CancellationToken cancellationToken = default);
    Task<int> SaveServicesStepAsync(PropertyAdministratorPreventiveServicesStepInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPreventiveScheduleStepViewModel?> GetScheduleStepAsync(int planId, CancellationToken cancellationToken = default);
    Task SaveScheduleStepAsync(PropertyAdministratorPreventiveScheduleStepInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPreventiveReviewStepViewModel?> GetReviewStepAsync(int planId, CancellationToken cancellationToken = default);
    Task ActivatePlanAsync(int planId, CancellationToken cancellationToken = default);
}

public class PropertyAdministratorPreventiveMaintenanceService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IPropertyAdministratorPreventiveMaintenanceService
{
    private static readonly Dictionary<string, (string Title, string Description, decimal Monthly, decimal Bundle)> TierCatalog = new()
    {
        ["Basic"] = ("Basic Homecare", "Essential upkeep for core home systems.", 29m, 149m),
        ["Seasonal"] = ("Seasonal Care", "Seasonal maintenance for year-round comfort.", 49m, 189m),
        ["Full"] = ("Full Preventive Plan", "Comprehensive protection for your entire home.", 79m, 229m)
    };

    private static readonly string[] DefaultSelectedServices = ["HvacTuneUp", "WaterHeaterFlush", "SmokeDetectorCheck"];

    public PropertyAdministratorPreventiveFeaturedViewModel BuildFeaturedCta(IUrlHelper url, int? propertyId) =>
        new()
        {
            StartUrl = url.Action("PreventiveMaintenanceServices", "Administrador", new { propertyId }) ?? "#"
        };

    public async Task<PropertyAdministratorPreventiveServicesStepViewModel> GetServicesStepAsync(
        IUrlHelper url, int? propertyId, int? planId, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = ResolveProperty(admin, propertyId);
        var catalog = await db.IndorPropertyAdminPreventiveServiceCatalog.AsNoTracking()
            .Where(c => c.Activo).OrderBy(c => c.Orden).ToListAsync(cancellationToken);

        var selected = DefaultSelectedServices.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tier = "Basic";
        if (planId.HasValue)
        {
            var existing = await db.IndorPropertyAdminPreventivePlans
                .FirstOrDefaultAsync(p => p.Id == planId.Value && p.AdministratorId == admin.Id, cancellationToken);
            if (existing != null)
            {
                selected = DeserializeServices(existing.SelectedServicesJson).ToHashSet(StringComparer.OrdinalIgnoreCase);
                tier = existing.PlanTier;
            }
        }

        return new PropertyAdministratorPreventiveServicesStepViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PlanId = planId,
            ViewingProperty = MapProperty(property),
            PropertyStatusLabel = property?.PropertyType == "ShortTermRental" ? "Rental ready" : null,
            PlanTier = tier,
            Services = catalog.Select(c => new PropertyAdministratorPreventiveServiceItemViewModel
            {
                ServiceKey = c.ServiceKey,
                ServiceName = c.ServiceName,
                DefaultFrequency = c.DefaultFrequency,
                IconClass = c.IconClass,
                ToneClass = c.ToneClass,
                IsSelected = selected.Contains(c.ServiceKey)
            }).ToList(),
            PlanTiers = TierCatalog.Select(t => new PropertyAdministratorPreventivePlanTierViewModel
            {
                TierKey = t.Key,
                Title = t.Value.Title,
                Description = t.Value.Description,
                PriceLabel = $"${t.Value.Monthly:0} /mo",
                MonthlyPrice = t.Value.Monthly,
                BundlePrice = t.Value.Bundle,
                IsSelected = tier == t.Key
            }).ToList()
        };
    }

    public async Task<int> SaveServicesStepAsync(
        PropertyAdministratorPreventiveServicesStepInput input, CancellationToken cancellationToken = default)
    {
        var admin = await LoadAdminAsync(trackChanges: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Property administrator not found.");
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == input.PropertyId)
            ?? throw new InvalidOperationException("Property not found.");

        var tier = TierCatalog.TryGetValue(input.PlanTier, out var tierInfo) ? tierInfo : TierCatalog["Basic"];
        var services = input.SelectedServices.Count > 0 ? input.SelectedServices : DefaultSelectedServices.ToList();
        var json = JsonSerializer.Serialize(services);

        IndorPropertyAdminPreventivePlan plan;
        if (input.PlanId.HasValue)
        {
            plan = await db.IndorPropertyAdminPreventivePlans
                .FirstOrDefaultAsync(p => p.Id == input.PlanId.Value && p.AdministratorId == admin.Id, cancellationToken)
                ?? throw new InvalidOperationException("Plan not found.");
            plan.PlanTier = input.PlanTier;
            plan.MonthlyPrice = tier.Monthly;
            plan.BundlePrice = tier.Bundle;
            plan.SelectedServicesJson = json;
        }
        else
        {
            plan = new IndorPropertyAdminPreventivePlan
            {
                AdministratorId = admin.Id,
                PortfolioPropertyId = property.Id,
                Status = PropertyAdministratorPreventivePlanStatuses.Draft,
                PlanTier = input.PlanTier,
                MonthlyPrice = tier.Monthly,
                BundlePrice = tier.Bundle,
                SelectedServicesJson = json
            };
            db.IndorPropertyAdminPreventivePlans.Add(plan);
        }

        await db.SaveChangesAsync(cancellationToken);
        return plan.Id;
    }

    public async Task<PropertyAdministratorPreventiveScheduleStepViewModel?> GetScheduleStepAsync(
        int planId, CancellationToken cancellationToken = default)
    {
        var plan = await LoadPlanAsync(planId, cancellationToken: cancellationToken);
        if (plan == null)
        {
            return null;
        }

        var admin = plan.Administrator!;
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == plan.PortfolioPropertyId);
        var catalog = await db.IndorPropertyAdminPreventiveServiceCatalog.AsNoTracking()
            .Where(c => c.Activo).ToListAsync(cancellationToken);
        var selectedKeys = DeserializeServices(plan.SelectedServicesJson);

        return new PropertyAdministratorPreventiveScheduleStepViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PlanId = plan.Id,
            ViewingProperty = MapProperty(property),
            SelectedServiceLabels = selectedKeys
                .Select(k => catalog.FirstOrDefault(c => c.ServiceKey == k)?.ServiceName ?? k)
                .ToList(),
            Frequency = plan.Frequency,
            PreferredTiming = plan.PreferredTiming,
            PreferredDay = plan.PreferredDay,
            EntryAccess = plan.EntryAccess,
            UpdateRecipients = plan.UpdateRecipients,
            Notes = plan.Notes ?? "Please service the HVAC and flush the water heater between guest stays if possible.",
            AutoReminders = plan.AutoReminders,
            FrequencyHint = BuildFrequencyHint(plan.Frequency),
            EstimatedPrice = $"${plan.BundlePrice:0}–${plan.BundlePrice + 80:0}"
        };
    }

    public async Task SaveScheduleStepAsync(
        PropertyAdministratorPreventiveScheduleStepInput input, CancellationToken cancellationToken = default)
    {
        var plan = await LoadPlanAsync(input.PlanId, trackChanges: true, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");

        plan.Frequency = input.Frequency;
        plan.PreferredTiming = input.PreferredTiming;
        plan.PreferredDay = input.PreferredDay;
        plan.EntryAccess = input.EntryAccess;
        plan.UpdateRecipients = input.UpdateRecipients;
        plan.Notes = input.Notes;
        plan.AutoReminders = input.AutoReminders;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PropertyAdministratorPreventiveReviewStepViewModel?> GetReviewStepAsync(
        int planId, CancellationToken cancellationToken = default)
    {
        var plan = await LoadPlanAsync(planId, cancellationToken: cancellationToken);
        if (plan == null)
        {
            return null;
        }

        var admin = plan.Administrator!;
        var shell = await BuildShellAsync(admin, cancellationToken);
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == plan.PortfolioPropertyId);
        var catalog = await db.IndorPropertyAdminPreventiveServiceCatalog.AsNoTracking()
            .Where(c => c.Activo).OrderBy(c => c.Orden).ToListAsync(cancellationToken);
        var selectedKeys = DeserializeServices(plan.SelectedServicesJson).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tierLabel = TierCatalog.TryGetValue(plan.PlanTier, out var tier) ? tier.Title : plan.PlanTier;

        return new PropertyAdministratorPreventiveReviewStepViewModel
        {
            DisplayName = shell.DisplayName,
            PortfolioName = shell.PortfolioName,
            ActivePropertyCount = shell.ActivePropertyCount,
            Greeting = shell.Greeting,
            NotificationCount = shell.NotificationCount,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            PlanId = plan.Id,
            ViewingProperty = MapProperty(property),
            PlanTierLabel = $"{tierLabel} Plan",
            NextVisitLabel = PropertyAdministratorDisplayLocalization.L("Next visit in 14 days"),
            BundlePriceLabel = $"${plan.BundlePrice:0}",
            SelectedServices = catalog.Where(c => selectedKeys.Contains(c.ServiceKey))
                .Select(c => new PropertyAdministratorPreventiveServiceItemViewModel
                {
                    ServiceKey = c.ServiceKey,
                    ServiceName = c.ServiceName,
                    DefaultFrequency = MapServiceFrequency(c, plan.Frequency),
                    IconClass = c.IconClass,
                    ToneClass = c.ToneClass,
                    IsSelected = true
                }).ToList(),
            Preferences =
            [
                new() { Label = PropertyAdministratorDisplayLocalization.L("Preferred timing"), Value = LabelTiming(plan.PreferredTiming), IconClass = "fa-calendar" },
                new() { Label = PropertyAdministratorDisplayLocalization.L("Access method"), Value = LabelAccess(plan.EntryAccess), IconClass = "fa-key" },
                new() { Label = PropertyAdministratorDisplayLocalization.L("Updates to"), Value = LabelUpdates(plan.UpdateRecipients), IconClass = "fa-users" },
                new() { Label = PropertyAdministratorDisplayLocalization.L("Auto-reminders"), Value = plan.AutoReminders ? "Enabled" : "Disabled", IconClass = "fa-bell" }
            ],
            PreventionBenefits =
            [
                "AC breakdowns", "Poor air quality", "Water heater sediment issues", "Smoke detector neglect"
            ]
        };
    }

    public async Task ActivatePlanAsync(int planId, CancellationToken cancellationToken = default)
    {
        var plan = await LoadPlanAsync(planId, trackChanges: true, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");
        var admin = plan.Administrator!;
        var property = admin.PortfolioProperties.FirstOrDefault(p => p.Id == plan.PortfolioPropertyId);
        var catalog = await db.IndorPropertyAdminPreventiveServiceCatalog.AsNoTracking()
            .Where(c => c.Activo).ToListAsync(cancellationToken);
        var selectedKeys = DeserializeServices(plan.SelectedServicesJson);

        plan.Status = PropertyAdministratorPreventivePlanStatuses.Active;
        plan.ActivatedUtc = DateTime.UtcNow;
        plan.NextVisitDate = DateTime.Today.AddDays(14);

        var orden = await db.IndorPropertyAdminHomecarePlans
            .Where(p => p.AdministratorId == admin.Id).MaxAsync(p => (int?)p.Orden, cancellationToken) ?? 0;

        foreach (var key in selectedKeys)
        {
            var svc = catalog.FirstOrDefault(c => c.ServiceKey == key);
            if (svc == null)
            {
                continue;
            }

            orden++;
            db.IndorPropertyAdminHomecarePlans.Add(new IndorPropertyAdminHomecarePlan
            {
                AdministratorId = admin.Id,
                PlanName = svc.ServiceName,
                Frequency = MapServiceFrequency(svc, plan.Frequency),
                HomesCovered = Math.Max(1, admin.PortfolioProperties.Count),
                NextDueDate = plan.NextVisitDate,
                IconClass = svc.IconClass,
                ToneClass = svc.ToneClass,
                Orden = orden
            });
        }

        db.IndorPropertyAdminScheduledVisits.Add(new IndorPropertyAdminScheduledVisit
        {
            AdministratorId = admin.Id,
            Title = "Preventive maintenance visit",
            PropertyName = property?.PropertyName ?? "Portfolio property",
            VisitDate = plan.NextVisitDate.Value,
            TimeWindow = LabelTiming(plan.PreferredTiming),
            ImageUrl = property?.ImageUrl
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<IndorPropertyAdminPreventivePlan?> LoadPlanAsync(
        int planId, bool trackChanges = false, CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var query = db.IndorPropertyAdminPreventivePlans
            .Include(p => p.Administrator!).ThenInclude(a => a.PortfolioProperties)
            .Where(p => p.Id == planId && p.Administrator!.UserId == userId);

        return trackChanges
            ? await query.FirstOrDefaultAsync(cancellationToken)
            : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    private static List<string> DeserializeServices(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string MapServiceFrequency(IndorPropertyAdminPreventiveServiceCatalogItem svc, string planFrequency) =>
        planFrequency switch
        {
            "Monthly" => "Monthly",
            "TwiceAYear" => svc.ServiceKey == "HvacFilterChange" ? "Every 3 months" : "Twice a year",
            "Yearly" => "Annual",
            _ => svc.DefaultFrequency
        };

    private static string BuildFrequencyHint(string frequency) => frequency switch
    {
        "Monthly" => "We'll schedule visits every month.",
        "TwiceAYear" => "We'll filter change every 3 months. Full service visits twice a year.",
        "Yearly" => "We'll schedule annual preventive visits.",
        _ => "We'll filter change every 3 months. Full service visits twice a year."
    };

    private static string LabelTiming(string value) => value switch
    {
        "Morning" => "Weekdays, 9 AM – 12 PM",
        "Afternoon" => "Weekdays, 12 PM – 5 PM",
        _ => "Weekdays, 9 AM – 5 PM"
    };

    private static string LabelAccess(string value) => value switch
    {
        "SmartLock" => "Lockbox",
        "GuestCoordination" => "Coordinate with guest",
        _ => "Host present"
    };

    private static string LabelUpdates(string value) => value switch
    {
        "Guest" => "Guest",
        "CoHost" => "Co-host",
        "MeGuest" => "Me + Guest",
        _ => "Me"
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
            .Where(a => a.UserId == userId && a.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed);

        return trackChanges
            ? await query.FirstOrDefaultAsync(cancellationToken)
            : await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PropertyAdministratorPortalShellViewModel> BuildShellAsync(
        IndorPropertyAdministrator admin, CancellationToken cancellationToken)
    {
        var shell = PropertyAdministratorFlowServiceSupport.BuildShell(admin);
        shell.NotificationCount = await db.IndorPropertyAdminServiceRequests
            .CountAsync(r => r.AdministratorId == admin.Id &&
                (r.Status == PropertyAdministratorRequestStatuses.Open ||
                 r.Status == PropertyAdministratorRequestStatuses.Emergency ||
                 r.Status == PropertyAdministratorRequestStatuses.InProgress), cancellationToken);

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
