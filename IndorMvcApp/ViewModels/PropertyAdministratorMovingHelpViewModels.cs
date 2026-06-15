namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorMovingHelpFeaturedViewModel
{
    public string Title { get; set; } = "Moving Help";
    public string Subtitle { get; set; } = "Turnover setup, furniture resets, and rental move support.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorMovingHelpFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceType { get; set; } = "TurnoverSetup";
    public string ItemsToMove { get; set; } = "SmallFurniture,BeddingLinens";
    public string HelperCount { get; set; } = "Two";
    public string ScheduleWhen { get; set; } = "Tomorrow";
    public string ScheduleTimeWindow { get; set; } = "10:00 AM – 1:00 PM";
    public string EntryAccess { get; set; } = "SmartLock";
    public string Details { get; set; } = "";
}

public class PropertyAdministratorMovingHelpSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "TurnoverSetup";
    public List<string> ItemsToMoveList { get; set; } = ["SmallFurniture", "BeddingLinens"];
    public string HelperCount { get; set; } = "Two";
    public string ScheduleWhen { get; set; } = "Tomorrow";
    public string ScheduleTimeWindow { get; set; } = "10:00 AM – 1:00 PM";
    public string EntryAccess { get; set; } = "SmartLock";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "CoHost"];
    public string? Details { get; set; }
}

public class PropertyAdministratorMovingHelpReviewViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorMovingHelpSubmitInput Input { get; set; } = new();
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<PropertyAdministratorMovingHelpReviewRowViewModel> SummaryRows { get; set; } = [];
    public string TeamEtaLabel { get; set; } = "Nearest moving team available in 23 minutes";
    public string EstimatedPrice { get; set; } = "$95–$145";
}

public class PropertyAdministratorMovingHelpReviewRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorMovingHelpTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorMovingHelpConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string TechnicianRole { get; set; } = "";
    public string ScheduleLabel { get; set; } = "";
    public string TimeWindowLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorMovingHelpReviewRowViewModel> BookingDetails { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorMovingHelpTimelineItemViewModel> Timeline { get; set; } = [];
    public string InfoBanner { get; set; } = "Ideal for Airbnb turnovers and rental property move support between guest stays. Trusted teams. On-time. Background checked.";
}
