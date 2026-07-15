namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorStandardCleaningFeaturedViewModel
{
    public string Title { get; set; } = "Standard Cleaning";
    public string Subtitle { get; set; } = "Routine cleaning to keep your rental guest-ready between stays.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorStandardCleaningFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ServiceType { get; set; } = "";
    public string ScheduleWhen { get; set; } = "";
    public string ScheduleTimeWindow { get; set; } = "";
    public string IncludedTasks { get; set; } = "";
    public string UrgentIssue { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Details { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$89–$149";
    public string ProEtaLabel { get; set; } = "Nearest cleaning crew available tomorrow 11 AM–2 PM";
}

public class PropertyAdministratorStandardCleaningSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "";
    public string ScheduleWhen { get; set; } = "";
    public string ScheduleTimeWindow { get; set; } = "";
    public List<string> IncludedTasksList { get; set; } = [];
    public string UrgentIssue { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string? Details { get; set; }
}

public class PropertyAdministratorStandardCleaningTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorStandardCleaningConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string ScheduleLabel { get; set; } = "";
    public string VehicleLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorStandardCleaningTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Standard cleaning helps keep your rental guest-ready between stays and makes future turnovers easier.";
}
