namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorSmokeDetectorFeaturedViewModel
{
    public string Title { get; set; } = "Smoke Detector Check";
    public string Subtitle { get; set; } = "Fast safety check for occupied homes and Airbnb stays.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorSmokeDetectorFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ServiceType { get; set; } = "";
    public string DetectorCount { get; set; } = "";
    public string UrgentSafetyIssue { get; set; } = "";
    public string DetectorType { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Details { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$69–$119";
    public string ProEtaLabel { get; set; } = "Nearest homecare pro available tomorrow 1–3 PM";
}

public class PropertyAdministratorSmokeDetectorSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "";
    public string DetectorCount { get; set; } = "";
    public string UrgentSafetyIssue { get; set; } = "";
    public string DetectorType { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string? Details { get; set; }
}

public class PropertyAdministratorSmokeDetectorTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorSmokeDetectorConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorSmokeDetectorTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Testing smoke detectors every few months helps keep guests safer and reduces false alarms.";
}
