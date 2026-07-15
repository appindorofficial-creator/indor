namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorBrokenWindowBoardUpStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string HelpType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string DamageLocation { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string ExposedToRisk { get; set; } = "";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorBrokenWindowBoardUpStep1Input
{
    public int PropertyId { get; set; }
    public string HelpType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string DamageLocation { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string ExposedToRisk { get; set; } = "";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorBrokenWindowBoardUpStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? GuestsOnSiteLabel { get; set; }
    public string HelpType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string DamageLocation { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string ExposedToRisk { get; set; } = "";
    public string QuickDetails { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string EntryInstructions { get; set; } = "";
    public List<string> SecurityConcernsList { get; set; } = [];
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string EmergencyBoardUpTonight { get; set; } = "";
    public string ProEtaLabel { get; set; } = "Earliest available board-up pro: 26 min";
}

public class PropertyAdministratorBrokenWindowBoardUpSubmitInput
{
    public int PropertyId { get; set; }
    public string HelpType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string DamageLocation { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string ExposedToRisk { get; set; } = "";
    public string? QuickDetails { get; set; }
    public string EntryAccess { get; set; } = "";
    public string? EntryInstructions { get; set; }
    public List<string> SecurityConcernsList { get; set; } = [];
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string EmergencyBoardUpTonight { get; set; } = "";
}

public class PropertyAdministratorBrokenWindowBoardUpConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "Keep guests away from the broken glass area until help arrives.";
}
