namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorPortalShellViewModel
{
    public string DisplayName { get; set; } = "";
    public string PortfolioName { get; set; } = "";
    public int ActivePropertyCount { get; set; }
    public string Greeting { get; set; } = "Good morning";
    public int NotificationCount { get; set; }
    public string? ProfilePhotoUrl { get; set; }
}

public class PropertyAdministratorStatCardViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string ToneClass { get; set; } = "";
    public string LinkLabel { get; set; } = "";
    public string LinkUrl { get; set; } = "#";
}

public class PropertyAdministratorActivityChipViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorVisitCardViewModel
{
    public string Title { get; set; } = "";
    public string PropertyName { get; set; } = "";
    public string DateLabel { get; set; } = "";
    public string? ImageUrl { get; set; }
}

public class PropertyAdministratorHomeViewModel : PropertyAdministratorPortalShellViewModel
{
    public int HomeFlowStep { get; set; } = 1;
    public int HomeFlowTotalSteps { get; set; } = 3;
    public PropertyAdministratorPoolHotTubFeaturedViewModel? FeaturedPoolHotTub { get; set; }
    public PropertyAdministratorPestControlFeaturedViewModel? FeaturedPestControl { get; set; }
    public PropertyAdministratorPressureWashingFeaturedViewModel? FeaturedPressureWashing { get; set; }
    public PropertyAdministratorLandscapingFeaturedViewModel? FeaturedLandscaping { get; set; }
    public PropertyAdministratorLawnCareFeaturedViewModel? FeaturedLawnCare { get; set; }
    public PropertyAdministratorTrashOutFeaturedViewModel? FeaturedTrashOut { get; set; }
    public PropertyAdministratorFurnitureHaulAwayFeaturedViewModel? FeaturedFurnitureHaulAway { get; set; }
    public PropertyAdministratorJunkRemovalFeaturedViewModel? FeaturedJunkRemoval { get; set; }
    public PropertyAdministratorMovingHelpFeaturedViewModel? FeaturedMovingHelp { get; set; }
    public PropertyAdministratorPetDeepCleanFeaturedViewModel? FeaturedPetDeepClean { get; set; }
    public PropertyAdministratorStandardCleaningFeaturedViewModel? FeaturedStandardCleaning { get; set; }
    public PropertyAdministratorTurnoverCleaningFeaturedViewModel? FeaturedTurnoverCleaning { get; set; }
    public PropertyAdministratorSmokeDetectorFeaturedViewModel? FeaturedSmokeDetector { get; set; }
    public PropertyAdministratorAirFilterFeaturedViewModel? FeaturedAirFilter { get; set; }
    public PropertyAdministratorPreventiveFeaturedViewModel? FeaturedPreventive { get; set; }
    public PropertyAdministratorEmergencyFeaturedViewModel? FeaturedEmergency { get; set; }
    public PropertyAdministratorNearestProViewModel? NearestPro { get; set; }
    public IReadOnlyList<PropertyAdministratorRecentRequestViewModel> RecentRequests { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPropertyItemViewModel> Properties { get; set; } = [];
    public PropertyAdministratorPropertyItemViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<PropertyAdministratorStatCardViewModel> SummaryStats { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorServiceHubItemViewModel> ServiceHub { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorActivityChipViewModel> TodayActivity { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorVisitCardViewModel> UpcomingVisits { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorHomecarePlanItemViewModel> HomecarePlans { get; set; } = [];
}

public class PropertyAdministratorCalendarViewModel : PropertyAdministratorPortalShellViewModel
{
    public IReadOnlyList<PropertyAdministratorVisitCardViewModel> Visits { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorServiceRequestItemViewModel> ScheduledRequests { get; set; } = [];
}

public class PropertyAdministratorPropertiesPortalViewModel : PropertyAdministratorPortalShellViewModel
{
    public IReadOnlyList<PropertyAdministratorPropertyItemViewModel> Properties { get; set; } = [];
    public string PortfolioTypeLabel { get; set; } = "";
    public string ManagementStyleLabel { get; set; } = "";
    public int TotalPropertyCount { get; set; }
    public int ActivePropertiesCount { get; set; }
    public string BackUrl { get; set; } = "#";
    public bool ShowBackHeader { get; set; }
    public string AddPropertyUrl { get; set; } = "#";
}

public class PropertyAdministratorPropertyDetailViewModel : PropertyAdministratorPortalShellViewModel
{
    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = "";
    public string Location { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string StatusLabel { get; set; } = "Active";
    public string ActiveTab { get; set; } = "overview";
    public string BackUrl { get; set; } = "#";
    public string EditUrl { get; set; } = "#";
    public string PropertyTypeLabel { get; set; } = "";
    public string YearBuiltLabel { get; set; } = "—";
    public string SquareFootageLabel { get; set; } = "—";
    public string BedsBathsLabel { get; set; } = "—";
    public IReadOnlyList<PropertyAdministratorPropertyQuickActionViewModel> QuickActions { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPropertyDetailRowViewModel> DetailRows { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorServiceRequestItemViewModel> ActivityItems { get; set; } = [];
}

public class PropertyAdministratorPropertyQuickActionViewModel
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string Url { get; set; } = "#";
}

public class PropertyAdministratorPropertyDetailRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorServiceHubItemViewModel
{
    public string Label { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string ToneClass { get; set; } = "";
    public string Url { get; set; } = "#";
}

public class PropertyAdministratorServiceCategoryViewModel
{
    public string CategoryKey { get; set; } = "";
    public string CategoryTitle { get; set; } = "";
    public int CategoryOrder { get; set; }
    public IReadOnlyList<PropertyAdministratorServiceCatalogItemViewModel> Items { get; set; } = [];
}

public class PropertyAdministratorServiceCatalogItemViewModel
{
    public string ServiceName { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string ToneClass { get; set; } = "";
    public string Url { get; set; } = "#";
}

public class PropertyAdministratorServicesViewModel : PropertyAdministratorPortalShellViewModel
{
    public IReadOnlyList<PropertyAdministratorServiceCategoryViewModel> Categories { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorHomecarePlanItemViewModel> ActivePlans { get; set; } = [];
    public string ActiveFilter { get; set; } = "all";
}

public class PropertyAdministratorHomecarePlanItemViewModel
{
    public string PlanName { get; set; } = "";
    public string Frequency { get; set; } = "";
    public int HomesCovered { get; set; }
    public string NextDueLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string ToneClass { get; set; } = "";
    public string StatusLabel { get; set; } = "Active";
}

public class PropertyAdministratorServiceRequestItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string PropertyName { get; set; } = "";
    public string Location { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusCss { get; set; } = "";
    public string DateLabel { get; set; } = "";
    public string? TeamLabel { get; set; }
    public string? EtaLabel { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsEmergency { get; set; }
}

public class PropertyAdministratorTasksViewModel : PropertyAdministratorPortalShellViewModel
{
    public string ActiveFilter { get; set; } = "all";
    public IReadOnlyList<PropertyAdministratorStatCardViewModel> SummaryStats { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorServiceRequestItemViewModel> Requests { get; set; } = [];
}

public class PropertyAdministratorProfileMenuItemViewModel
{
    public string Label { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string Url { get; set; } = "#";
    public bool IsDanger { get; set; }
    public int? BadgeCount { get; set; }
}

public class PropertyAdministratorProfileViewModel : PropertyAdministratorPortalShellViewModel
{
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Location { get; set; } = "";
    public string RoleBadge { get; set; } = "Multi-Property Owner";
    public IReadOnlyList<PropertyAdministratorProfileMenuItemViewModel> MenuItems { get; set; } = [];
}

public class PropertyAdministratorPersonalInformationViewModel : PropertyAdministratorPortalShellViewModel
{
    public string RoleBadge { get; set; } = "Multi-Property Owner";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string DateOfBirthLabel { get; set; } = "Not set";
    public string PreferredContactMethod { get; set; } = "Email & SMS";
    public bool MarketingEmailsEnabled { get; set; }
    public string ChangePasswordUrl { get; set; } = "#";
    public string PrivacyPolicyUrl { get; set; } = "#";
    public string BackUrl { get; set; } = "#";
}

public class PropertyAdministratorNotificationPreferencesInput
{
    public bool PushEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; }
    public bool PropertyUpdatesEnabled { get; set; } = true;
    public bool ServiceUpdatesEnabled { get; set; } = true;
    public bool TaskRemindersEnabled { get; set; } = true;
    public bool PaymentsBillingEnabled { get; set; } = true;
    public bool PromotionsTipsEnabled { get; set; }
    public string QuietHoursStart { get; set; } = "22:00";
    public string QuietHoursEnd { get; set; } = "07:00";
}

public class PropertyAdministratorNotificationPreferencesViewModel : PropertyAdministratorPortalShellViewModel
{
    public string BackUrl { get; set; } = "#";
    public bool Saved { get; set; }
    public bool PushEnabled { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; }
    public bool PropertyUpdatesEnabled { get; set; } = true;
    public bool ServiceUpdatesEnabled { get; set; } = true;
    public bool TaskRemindersEnabled { get; set; } = true;
    public bool PaymentsBillingEnabled { get; set; } = true;
    public bool PromotionsTipsEnabled { get; set; }
    public string QuietHoursStart { get; set; } = "22:00";
    public string QuietHoursEnd { get; set; } = "07:00";
    public string QuietHoursLabel { get; set; } = "10:00 PM - 7:00 AM";
}
