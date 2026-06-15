namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyTreeBranchStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string IssueType { get; set; } = "LargeHangingBranch";
    public string ImmediateDanger { get; set; } = "No";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public List<string> DamageAreasList { get; set; } = ["Roof", "FrontYard"];
    public string TarpNeeded { get; set; } = "Yes";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorEmergencyTreeBranchStep1Input
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "LargeHangingBranch";
    public string ImmediateDanger { get; set; } = "No";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public List<string> DamageAreasList { get; set; } = ["Roof", "FrontYard"];
    public string TarpNeeded { get; set; } = "Yes";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorEmergencyTreeBranchSubmitInput
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "LargeHangingBranch";
    public string ImmediateDanger { get; set; } = "No";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public List<string> DamageAreasList { get; set; } = ["Roof", "FrontYard"];
    public string TarpNeeded { get; set; } = "Yes";
    public string? QuickDetails { get; set; }
    public string EntryAccess { get; set; } = "ExteriorOnly";
    public string GateParkingNotes { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string ContactPhone { get; set; } = "";
    public string InsuranceHelp { get; set; } = "NeedDocumentation";
}

public class PropertyAdministratorEmergencyTreeBranchReviewViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorEmergencyTreeBranchSubmitInput Input { get; set; } = new();
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? GuestsOnSiteLabel { get; set; }
    public IReadOnlyList<PropertyAdministratorEmergencyTreeBranchReviewRowViewModel> RequestSummaryRows { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorEmergencyTreeBranchReviewRowViewModel> AccessContactRows { get; set; } = [];
    public string ProEtaLabel { get; set; } = "Nearest tree service pro available in 28 minutes";
    public string EstimatedPrice { get; set; } = "$149–$249";
}

public class PropertyAdministratorEmergencyTreeBranchReviewRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "fa-circle";
    public bool IsDangerBadge { get; set; }
}

public class PropertyAdministratorEmergencyTreeBranchConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "Keep guests away from the affected area until the crew arrives.";
}
