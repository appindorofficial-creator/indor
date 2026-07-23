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
    /// <summary>Legacy Bug 30 shared default — replaced by per-service notes below.</summary>
    internal const string LegacySharedScheduleNotesKey =
        "Please service the HVAC and flush the water heater between guest stays if possible.";

    /// <summary>English UI keys for schedule-step default notes, keyed by preventive catalog ServiceKey.</summary>
    private static readonly Dictionary<string, string> DefaultNotesByServiceKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["HvacTuneUp"] = "Please service the HVAC between guest stays if possible.",
        ["HvacFilterChange"] = "Please change the HVAC filter between guest stays if possible.",
        ["WaterHeaterFlush"] = "Please flush the water heater between guest stays if possible.",
        ["SmokeDetectorCheck"] = "Please test all smoke detectors and replace batteries as needed.",
        ["DryerVentCleaning"] = "Please clean the dryer vent thoroughly for fire safety.",
        ["GutterCleaning"] = "Please clear gutters and downspouts of debris.",
        ["PlumbingCheck"] = "Please inspect plumbing for leaks and slow drains.",
        ["ElectricalSafety"] = "Please check outlets, breakers, and detector wiring for safety.",
        ["ApplianceCheck"] = "Please inspect major appliances for safe operation.",
        ["PestPrevention"] = "Please treat common pest entry points around the property."
    };

    private const string MultiServiceDefaultNotesKey =
        "Please complete the selected preventive maintenance between guest stays if possible.";

    private static readonly Dictionary<string, (string Title, string Description, decimal Monthly, decimal Bundle)> TierCatalog = new()
    {
        ["Basic"] = ("Basic Homecare", "Essential upkeep for core home systems.", 29m, 149m),
        ["Seasonal"] = ("Seasonal Care", "Seasonal maintenance for year-round comfort.", 49m, 189m),
        ["Full"] = ("Full Preventive Plan", "Comprehensive protection for your entire home.", 79m, 229m)
    };

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

        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tier = "";
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
        var services = input.SelectedServices;
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

        // Schedule step must start blank — user chooses frequency/timing/access explicitly.
        ClearScheduleSelections(plan);

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
        var scheduleConfigured = IsScheduleConfigured(plan);
        var useFilterHints = UsesFilterFrequencyHints(selectedKeys);

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
            UsesFilterFrequencyHints = useFilterHints,
            Frequency = scheduleConfigured ? plan.Frequency : "",
            PreferredTiming = scheduleConfigured ? plan.PreferredTiming : "",
            PreferredDay = scheduleConfigured ? plan.PreferredDay : "",
            EntryAccess = scheduleConfigured ? plan.EntryAccess : "",
            UpdateRecipients = scheduleConfigured ? plan.UpdateRecipients : "",
            // Only show notes the user saved; do not prefill service templates.
            Notes = scheduleConfigured ? (plan.Notes ?? "") : "",
            AutoReminders = scheduleConfigured && plan.AutoReminders,
            FrequencyHint = scheduleConfigured ? BuildFrequencyHint(plan.Frequency, useFilterHints) : "",
            EstimatedPrice = $"${plan.BundlePrice:0}–${plan.BundlePrice + 80:0}"
        };
    }

    public async Task SaveScheduleStepAsync(
        PropertyAdministratorPreventiveScheduleStepInput input, CancellationToken cancellationToken = default)
    {
        var plan = await LoadPlanAsync(input.PlanId, trackChanges: true, cancellationToken)
            ?? throw new InvalidOperationException("Plan not found.");

        if (string.IsNullOrWhiteSpace(input.Frequency)
            || string.IsNullOrWhiteSpace(input.PreferredTiming)
            || string.IsNullOrWhiteSpace(input.PreferredDay)
            || string.IsNullOrWhiteSpace(input.EntryAccess)
            || string.IsNullOrWhiteSpace(input.UpdateRecipients))
        {
            throw new InvalidOperationException("Complete all required schedule selections.");
        }

        plan.Frequency = input.Frequency;
        plan.PreferredTiming = input.PreferredTiming;
        plan.PreferredDay = input.PreferredDay;
        plan.EntryAccess = input.EntryAccess;
        plan.UpdateRecipients = input.UpdateRecipients;
        // Empty string (not null) marks the schedule step as configured for draft plans.
        plan.Notes = input.Notes ?? "";
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
            PlanTierLabel = tierLabel,
            NextVisitLabel = "Next visit in 14 days",
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
            // English catalog keys — PreventiveMaintenanceReview localizes via UiDisplayLocalization.
            Preferences =
            [
                new() { Label = "Preferred timing", Value = LabelTiming(plan.PreferredTiming), IconClass = "fa-calendar" },
                new() { Label = "Access method", Value = LabelAccess(plan.EntryAccess), IconClass = "fa-key" },
                new() { Label = "Updates to", Value = LabelUpdates(plan.UpdateRecipients), IconClass = "fa-users" },
                new() { Label = "Auto-reminders", Value = plan.AutoReminders ? "Enabled" : "Disabled", IconClass = "fa-bell" }
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

    /// <summary>
    /// Prefills notes from the selected preventive service key(s). Replaces the legacy shared HVAC/water-heater
    /// default when the plan still has that placeholder so unrelated services (e.g. smoke detector) get their own text.
    /// </summary>
    internal static string ResolveScheduleNotes(string? storedNotes, IReadOnlyList<string> selectedKeys)
    {
        if (!string.IsNullOrWhiteSpace(storedNotes) && !IsLegacySharedDefaultNotes(storedNotes))
        {
            return storedNotes;
        }

        return BuildDefaultScheduleNotes(selectedKeys);
    }

    internal static string BuildDefaultScheduleNotes(IReadOnlyList<string> selectedKeys)
    {
        var keys = selectedKeys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (keys.Count == 0)
        {
            return string.Empty;
        }

        if (keys.Count == 1 && DefaultNotesByServiceKey.TryGetValue(keys[0], out var single))
        {
            return single;
        }

        // Classic HVAC + water heater combo keeps the original combined suggestion.
        var hasHvac = keys.Any(k =>
            k.Equals("HvacTuneUp", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("HvacFilterChange", StringComparison.OrdinalIgnoreCase));
        var hasWaterHeater = keys.Any(k => k.Equals("WaterHeaterFlush", StringComparison.OrdinalIgnoreCase));
        if (keys.Count == 2 && hasHvac && hasWaterHeater)
        {
            return LegacySharedScheduleNotesKey;
        }

        var mapped = keys
            .Select(k => DefaultNotesByServiceKey.TryGetValue(k, out var note) ? note : null)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (mapped.Count == 1)
        {
            return mapped[0];
        }

        if (mapped.Count == 0)
        {
            return string.Empty;
        }

        return MultiServiceDefaultNotesKey;
    }

    private static bool IsLegacySharedDefaultNotes(string notes)
    {
        var trimmed = notes.Trim();
        return trimmed.Equals(LegacySharedScheduleNotesKey, StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(
                "Por favor, da servicio al HVAC y vacía/enjuaga el calentador de agua entre estadías de huéspedes si es posible.",
                StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(
                "Por favor, da servicio al HVAC y lava el calentador de agua entre estadías de huéspedes si es posible.",
                StringComparison.OrdinalIgnoreCase);
    }

    private static string MapServiceFrequency(IndorPropertyAdminPreventiveServiceCatalogItem svc, string planFrequency) =>
        planFrequency switch
        {
            "Monthly" => PropertyAdministratorDisplayLocalization.L("Monthly"),
            "TwiceAYear" => svc.ServiceKey == "HvacFilterChange"
                ? PropertyAdministratorDisplayLocalization.L("Every 3 months")
                : PropertyAdministratorDisplayLocalization.L("Twice a year"),
            "Yearly" => PropertyAdministratorDisplayLocalization.L("Annual"),
            _ => PropertyAdministratorDisplayLocalization.L(svc.DefaultFrequency)
        };

    private static bool UsesFilterFrequencyHints(IReadOnlyList<string> selectedKeys) =>
        selectedKeys.Count > 0
        && selectedKeys.All(k => k.Equals("HvacFilterChange", StringComparison.OrdinalIgnoreCase));

    private static string BuildFrequencyHint(string frequency, bool useFilterHints) => frequency switch
    {
        "" => "",
        "Monthly" => useFilterHints
            ? PropertyAdministratorDisplayLocalization.L("We'll change the filter every month.")
            : PropertyAdministratorDisplayLocalization.L("We'll schedule visits every month."),
        "Every3Months" => useFilterHints
            ? PropertyAdministratorDisplayLocalization.L("We'll change the filter every 3 months.")
            : PropertyAdministratorDisplayLocalization.L("We'll schedule visits every 3 months."),
        "TwiceAYear" => useFilterHints
            ? PropertyAdministratorDisplayLocalization.L("We'll change the filter every 6 months. Full service visits twice a year.")
            : PropertyAdministratorDisplayLocalization.L("We'll schedule visits twice a year."),
        "Yearly" => useFilterHints
            ? PropertyAdministratorDisplayLocalization.L("We'll change the filter once a year.")
            : PropertyAdministratorDisplayLocalization.L("We'll schedule annual preventive visits."),
        _ => ""
    };

    /// <summary>
    /// Draft plans created before schedule defaults were removed still carry factory values.
    /// Notes == null means the schedule step was never POSTed (SaveSchedule writes Notes as "").
    /// </summary>
    private static bool IsScheduleConfigured(IndorPropertyAdminPreventivePlan plan)
    {
        if (string.IsNullOrWhiteSpace(plan.Frequency))
        {
            return false;
        }

        if (plan.Status == PropertyAdministratorPreventivePlanStatuses.Draft
            && plan.ActivatedUtc == null
            && plan.Notes == null)
        {
            return false;
        }

        return true;
    }

    private static void ClearScheduleSelections(IndorPropertyAdminPreventivePlan plan)
    {
        plan.Frequency = "";
        plan.PreferredTiming = "";
        plan.PreferredDay = "";
        plan.EntryAccess = "";
        plan.UpdateRecipients = "";
        plan.Notes = null;
        plan.AutoReminders = false;
    }

    // Keep English catalog keys so views localize via IIndorLocalizer / UiDisplayLocalization.
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
            ImageUrl = PropertyAdministratorCatalog.ResolvePortfolioImageUrl(property.ImageUrl, property.PropertyType),
            OccupancyLabel = PropertyAdministratorDisplayLocalization.OccupancyLabel(property.PropertyType)
        };
    }
}