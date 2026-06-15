namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyWaterHeaterStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ProblemType { get; set; } = "NoHotWater";
    public string ActivelyLeaking { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string HeaterType { get; set; } = "Gas";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorEmergencyWaterHeaterStep1Input
{
    public int PropertyId { get; set; }
    public string ProblemType { get; set; } = "NoHotWater";
    public string ActivelyLeaking { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string HeaterType { get; set; } = "Gas";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorEmergencyWaterHeaterSubmitInput
{
    public int PropertyId { get; set; }
    public string ProblemType { get; set; } = "NoHotWater";
    public string ActivelyLeaking { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string HeaterType { get; set; } = "Gas";
    public string? QuickDetails { get; set; }
    public string EntryAccess { get; set; } = "GarageSide";
    public string? AccessNotes { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string ContactPhone { get; set; } = "";
}

public class PropertyAdministratorEmergencyWaterHeaterReviewViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorEmergencyWaterHeaterSubmitInput Input { get; set; } = new();
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<PropertyAdministratorEmergencyWaterHeaterReviewRowViewModel> SummaryRows { get; set; } = [];
    public string ProEtaLabel { get; set; } = "Nearest water heater pro available in 24 minutes";
    public string EstimatedPrice { get; set; } = "$129–$169";
}

public class PropertyAdministratorEmergencyWaterHeaterReviewRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "fa-circle";
    public bool Highlight { get; set; }
}

public class PropertyAdministratorEmergencyWaterHeaterConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "If the unit is leaking, keep the area clear and locate the water shutoff if safe to do so.";
}
