namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyRoofLeakStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string RoofIssue { get; set; } = "ActiveLeak";
    public string WaterEnteringNow { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string LeakLocation { get; set; } = "Bedroom";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorEmergencyRoofLeakStep1Input
{
    public int PropertyId { get; set; }
    public string RoofIssue { get; set; } = "ActiveLeak";
    public string WaterEnteringNow { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string LeakLocation { get; set; } = "Bedroom";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorEmergencyRoofLeakStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? GuestsOnSiteLabel { get; set; }
    public string RoofIssue { get; set; } = "ActiveLeak";
    public string WaterEnteringNow { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string LeakLocation { get; set; } = "Bedroom";
    public string QuickDetails { get; set; } = "";
    public string EntryAccess { get; set; } = "SmartLock";
    public string EntryCode { get; set; } = "";
    public string AreaProtection { get; set; } = "BucketPlaced";
    public string InteriorDamage { get; set; } = "Ceiling,Floor";
    public string InsuranceInfo { get; set; } = "UploadLater";
    public string AccessNotes { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string ContactPhone { get; set; } = "";
    public string ProEtaLabel { get; set; } = "Earliest available roof leak pro: 26 min";
}

public class PropertyAdministratorEmergencyRoofLeakSubmitInput
{
    public int PropertyId { get; set; }
    public string RoofIssue { get; set; } = "ActiveLeak";
    public string WaterEnteringNow { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string LeakLocation { get; set; } = "Bedroom";
    public string? QuickDetails { get; set; }
    public string EntryAccess { get; set; } = "SmartLock";
    public string? EntryCode { get; set; }
    public string AreaProtection { get; set; } = "BucketPlaced";
    public List<string> InteriorDamageList { get; set; } = ["Ceiling", "Floor"];
    public string InsuranceInfo { get; set; } = "UploadLater";
    public string? AccessNotes { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string ContactPhone { get; set; } = "";
}

public class PropertyAdministratorEmergencyRoofLeakConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "Move valuables away from the leak and place a bucket under active drips while help is on the way.";
}
