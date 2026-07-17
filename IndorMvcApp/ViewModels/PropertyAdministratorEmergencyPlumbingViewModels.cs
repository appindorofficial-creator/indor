namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyPlumbingStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string IssueType { get; set; } = "";
    public string ActivelyLeaking { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string ProblemLocation { get; set; } = "";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorEmergencyPlumbingStep1Input
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "";
    public string ActivelyLeaking { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string ProblemLocation { get; set; } = "";
    public string? QuickDetails { get; set; }
    public string? MediaAttachmentsJson { get; set; }
}

public class PropertyAdministratorEmergencyPlumbingStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? GuestsOnSiteLabel { get; set; }
    public string IssueType { get; set; } = "";
    public string ActivelyLeaking { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string ProblemLocation { get; set; } = "";
    public string QuickDetails { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string EntryCode { get; set; } = "";
    public string WaterShutoffAccess { get; set; } = "";
    public string AccessNotes { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string PermissionToEnter { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$129–$169";
    public string ProEtaLabel { get; set; } = "Nearest plumbing pro available in 22 minutes";
    public string? MediaAttachmentsJson { get; set; }
}

public class PropertyAdministratorEmergencyPlumbingSubmitInput
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "";
    public string ActivelyLeaking { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string ProblemLocation { get; set; } = "";
    public string? QuickDetails { get; set; }
    public string EntryAccess { get; set; } = "";
    public string? EntryCode { get; set; }
    public string? WaterShutoffAccess { get; set; }
    public string? AccessNotes { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string PermissionToEnter { get; set; } = "";
    public string? MediaAttachmentsJson { get; set; }
}

public class PropertyAdministratorEmergencyPlumbingConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> Summary { get; set; } = [];
    public string Tip { get; set; } = "Ask guests to avoid using the affected bathroom or fixture until the plumber arrives.";
}
