namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorLockoutAccessStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string IssueType { get; set; } = "GuestLockedOut";
    public string SomeoneOutside { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string WhoNeedsAccess { get; set; } = "Guest";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorLockoutAccessStep1Input
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "GuestLockedOut";
    public string SomeoneOutside { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string WhoNeedsAccess { get; set; } = "Guest";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorLockoutAccessStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? GuestsWaitingLabel { get; set; }
    public string IssueType { get; set; } = "GuestLockedOut";
    public string SomeoneOutside { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string WhoNeedsAccess { get; set; } = "Guest";
    public string QuickDetails { get; set; } = "";
    public string SmartLockCodeWorks { get; set; } = "Yes";
    public string BackupAccess { get; set; } = "LockboxKey";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string ContactPhone { get; set; } = "";
    public string ProEnterImmediately { get; set; } = "Yes";
    public string EntryNotes { get; set; } = "";
    public string ProEtaLabel { get; set; } = "Earliest available access pro: 18 min";
}

public class PropertyAdministratorLockoutAccessSubmitInput
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "GuestLockedOut";
    public string SomeoneOutside { get; set; } = "Yes";
    public string HomeOccupied { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string WhoNeedsAccess { get; set; } = "Guest";
    public string? QuickDetails { get; set; }
    public string SmartLockCodeWorks { get; set; } = "Yes";
    public string BackupAccess { get; set; } = "LockboxKey";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string ContactPhone { get; set; } = "";
    public string ProEnterImmediately { get; set; } = "Yes";
    public string? EntryNotes { get; set; }
}

public class PropertyAdministratorLockoutAccessConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "Keep the guest near the entry and have ID ready if needed.";
}
