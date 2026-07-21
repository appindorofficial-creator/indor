namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyFloodFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ProblemType { get; set; } = "";
    public string WaterActivelyComingIn { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string WaterLocation { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Notes { get; set; } = "";
    public string ProcessThroughInsurance { get; set; } = "";
    public string InsuranceCarrier { get; set; } = "";
    public string ClaimOpened { get; set; } = "";
    public string? ClaimNumber { get; set; }
    public string? MediaAttachmentsJson { get; set; }
    public string? PolicyDocsJson { get; set; }
    public string EstimatedPrice { get; set; } = "$149–$199";
    public string ProEtaLabel { get; set; } = "Nearest water mitigation pro available in 19 minutes";
}

public class PropertyAdministratorEmergencyFloodSubmitInput
{
    public int PropertyId { get; set; }
    public string ProblemType { get; set; } = "";
    public string WaterActivelyComingIn { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string WaterLocation { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string? Notes { get; set; }
    public string ProcessThroughInsurance { get; set; } = "";
    public string? InsuranceCarrier { get; set; }
    public string ClaimOpened { get; set; } = "";
    public string? ClaimNumber { get; set; }
    public string? MediaAttachmentsJson { get; set; }
    public string? PolicyDocsJson { get; set; }
}

public class PropertyAdministratorEmergencyFloodReviewRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "fa-circle";
    public bool Highlight { get; set; }
}

public class PropertyAdministratorEmergencyFloodReviewViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorEmergencyFloodSubmitInput Input { get; set; } = new();
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<PropertyAdministratorEmergencyFloodReviewRowViewModel> SummaryRows { get; set; } = [];
    public string ProEtaLabel { get; set; } = "Nearest water mitigation pro available in 19 minutes";
    public string EstimatedPrice { get; set; } = "$149–$199";
}

public class PropertyAdministratorEmergencyFloodConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public bool ShowInsuranceBanner { get; set; }
    public IReadOnlyList<PropertyAdministratorEmergencyAcTimelineItemViewModel> Timeline { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorEmergencyElectricalSummaryItemViewModel> Summary { get; set; } = [];
    public string Tip { get; set; } = "Move valuables away from standing water and, if safe, shut off the water source.";
}
