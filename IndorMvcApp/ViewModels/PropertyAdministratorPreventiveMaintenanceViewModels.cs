namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorPreventiveFeaturedViewModel
{
    public string Title { get; set; } = "Start preventive maintenance";
    public string Subtitle { get; set; } = "Keep your property systems in good shape with scheduled upkeep.";
    public string StartUrl { get; set; } = "#";
    public string BadgeLabel { get; set; } = "Recommended";
}

public class PropertyAdministratorPreventiveServiceItemViewModel
{
    public string ServiceKey { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string DefaultFrequency { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string ToneClass { get; set; } = "";
    public bool IsSelected { get; set; }
}

public class PropertyAdministratorPreventivePlanTierViewModel
{
    public string TierKey { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string PriceLabel { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public decimal BundlePrice { get; set; }
    public bool IsSelected { get; set; }
}

public class PropertyAdministratorPreventiveServicesStepViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 4;
    public int? PlanId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string PlanTier { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorPreventiveServiceItemViewModel> Services { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPreventivePlanTierViewModel> PlanTiers { get; set; } = [];
}

public class PropertyAdministratorPreventiveServicesStepInput
{
    public int PropertyId { get; set; }
    public int? PlanId { get; set; }
    public string PlanTier { get; set; } = "";
    public List<string> SelectedServices { get; set; } = [];
}

public class PropertyAdministratorPreventiveScheduleStepViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 4;
    public int PlanId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<string> SelectedServiceLabels { get; set; } = [];
    public string Frequency { get; set; } = "";
    public string PreferredTiming { get; set; } = "";
    public string PreferredDay { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string Notes { get; set; } = "";
    public bool AutoReminders { get; set; } = true;
    public string FrequencyHint { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$149–$229";
}

public class PropertyAdministratorPreventiveScheduleStepInput
{
    public int PlanId { get; set; }
    public string Frequency { get; set; } = "";
    public string PreferredTiming { get; set; } = "";
    public string PreferredDay { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string? Notes { get; set; }
    public bool AutoReminders { get; set; } = true;
}

public class PropertyAdministratorPreventiveReviewStepViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 4;
    public int TotalSteps { get; set; } = 4;
    public int PlanId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string PlanTierLabel { get; set; } = "";
    public string NextVisitLabel { get; set; } = "";
    public string BundlePriceLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorPreventiveServiceItemViewModel> SelectedServices { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPreventivePreferenceItemViewModel> Preferences { get; set; } = [];
    public IReadOnlyList<string> PreventionBenefits { get; set; } = [];
}

public class PropertyAdministratorPreventivePreferenceItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}
