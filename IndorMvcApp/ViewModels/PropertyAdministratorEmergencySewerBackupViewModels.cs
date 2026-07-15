namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencySewerBackupStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string IssueType { get; set; } = "";
    public string SewageBackingUp { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public List<string> LocationsList { get; set; } = [];
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorEmergencySewerBackupStep1Input
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "";
    public string SewageBackingUp { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public List<string> LocationsList { get; set; } = [];
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorEmergencySewerBackupSubmitInput
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "";
    public string SewageBackingUp { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public List<string> LocationsList { get; set; } = [];
    public string? QuickDetails { get; set; }
    public string EntryAccess { get; set; } = "";
    public string? EntryCode { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
}

public class PropertyAdministratorEmergencySewerBackupReviewViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorEmergencySewerBackupSubmitInput Input { get; set; } = new();
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<PropertyAdministratorEmergencySewerBackupReviewRowViewModel> SummaryRows { get; set; } = [];
    public string ProEtaLabel { get; set; } = "Nearest sewer / drain pro available in 24 minutes";
    public string EstimatedPrice { get; set; } = "$149–$199";
}

public class PropertyAdministratorEmergencySewerBackupReviewRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "fa-circle";
    public bool Highlight { get; set; }
}

public class PropertyAdministratorEmergencySewerBackupConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string EtaLabel { get; set; } = "";
    public string VehicleLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Keep guests away from affected drains and avoid using water until the technician arrives.";
}
