namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorJunkRemovalFeaturedViewModel
{
    public string Title { get; set; } = "Junk Removal";
    public string Subtitle { get; set; } = "Furniture, boxes, and rental cleanouts — matched with the right haul-away crew.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorJunkRemovalStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string RemovalItems { get; set; } = "";
    public string LoadSize { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string PickupType { get; set; } = "";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorJunkRemovalStep1Input
{
    public int PropertyId { get; set; }
    public List<string> RemovalItemsList { get; set; } = [];
    public string LoadSize { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string PickupType { get; set; } = "";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorJunkRemovalStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string RemovalItems { get; set; } = "";
    public string LoadSize { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string PickupType { get; set; } = "";
    public string QuickDetails { get; set; } = "";
    public string PickupWhen { get; set; } = "";
    public string TimeWindow { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string EntryCode { get; set; } = "";
    public string ItemLocations { get; set; } = "";
    public string UpdateRecipient { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string CrewEtaLabel { get; set; } = "Earliest available junk crew: 2:30 PM today";
}

public class PropertyAdministratorJunkRemovalSubmitInput : PropertyAdministratorJunkRemovalStep1Input
{
    public string PickupWhen { get; set; } = "";
    public string TimeWindow { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string? EntryCode { get; set; }
    public List<string> ItemLocationsList { get; set; } = [];
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string? ContactPhone { get; set; }
}

public class PropertyAdministratorJunkRemovalSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorJunkRemovalTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorJunkRemovalConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string TechnicianRole { get; set; } = "";
    public string EtaLabel { get; set; } = "";
    public string VehicleLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorJunkRemovalSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorJunkRemovalTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Move small valuables aside before the crew arrives.";
}
