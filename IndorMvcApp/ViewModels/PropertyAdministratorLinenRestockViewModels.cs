namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorLinenRestockFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ServiceType { get; set; } = "";
    public string ScheduleWhen { get; set; } = "";
    public string ScheduleTimeWindow { get; set; } = "";
    public string IncludedItems { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Details { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$59–$119";
    public string ProEtaLabel { get; set; } = "Nearest restock crew available tomorrow 10 AM–1 PM";
}

public class PropertyAdministratorLinenRestockSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "";
    public string ScheduleWhen { get; set; } = "";
    public string ScheduleTimeWindow { get; set; } = "";
    public List<string> IncludedItemsList { get; set; } = [];
    public string EntryAccess { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string? Details { get; set; }
    public string? MediaAttachmentsJson { get; set; }
}

public class PropertyAdministratorLinenRestockTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorLinenRestockConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorLinenRestockTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Restocking linens and supplies between stays keeps your rental guest-ready for the next check-in.";
}
