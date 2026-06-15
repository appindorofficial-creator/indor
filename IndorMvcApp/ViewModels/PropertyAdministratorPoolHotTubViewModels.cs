namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorPoolHotTubFeaturedViewModel
{
    public string Title { get; set; } = "Pool & Hot Tub Repair";
    public string Subtitle { get; set; } = "Pump, heater, and spa issues — matched with a licensed pool & spa pro.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorPoolHotTubStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ServiceHelpType { get; set; } = "HotTubRepair";
    public string MainIssue { get; set; } = "HeaterIssue";
    public string GuestStayAffected { get; set; } = "Yes";
    public string Urgency { get; set; } = "Urgent";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorPoolHotTubStep1Input
{
    public int PropertyId { get; set; }
    public string ServiceHelpType { get; set; } = "HotTubRepair";
    public string MainIssue { get; set; } = "HeaterIssue";
    public string GuestStayAffected { get; set; } = "Yes";
    public string Urgency { get; set; } = "Urgent";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorPoolHotTubSummaryChipViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorPoolHotTubStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceHelpType { get; set; } = "HotTubRepair";
    public string MainIssue { get; set; } = "HeaterIssue";
    public string GuestStayAffected { get; set; } = "Yes";
    public string Urgency { get; set; } = "Urgent";
    public string QuickDetails { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorPoolHotTubSummaryChipViewModel> Step1Summary { get; set; } = [];
    public string EquipmentLocation { get; set; } = "BackyardSpa";
    public string EntryAccess { get; set; } = "GateCode";
    public string AccessCode { get; set; } = "";
    public string UpdateRecipients { get; set; } = "Me,Guest";
    public string ContactPhone { get; set; } = "";
    public string ProEtaLabel { get; set; } = "Nearest pool & spa pro available in 27 minutes";
    public string DiagnosticEstimate { get; set; } = "$129 – $169";
    public string EmergencyFeeLabel { get; set; } = "Included";
}

public class PropertyAdministratorPoolHotTubSubmitInput : PropertyAdministratorPoolHotTubStep1Input
{
    public string EquipmentLocation { get; set; } = "BackyardSpa";
    public string EntryAccess { get; set; } = "GateCode";
    public string? AccessCode { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string? ContactPhone { get; set; }
}

public class PropertyAdministratorPoolHotTubSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorPoolHotTubTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorPoolHotTubConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string EtaLabel { get; set; } = "";
    public string VehicleLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorPoolHotTubSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPoolHotTubTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Ask guests to avoid using the hot tub until the technician finishes the repair.";
}
