namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorFurnitureHaulAwayFeaturedViewModel
{
    public string Title { get; set; } = "Furniture Haul Away";
    public string Subtitle { get; set; } = "Couches, mattresses, and large items — matched with the right removal crew.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorFurnitureHaulAwayStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string FurnitureItems { get; set; } = "Couch,Mattress";
    public string ItemCount { get; set; } = "TwoThree";
    public string PickupSize { get; set; } = "HalfLoad";
    public string IsOccupied { get; set; } = "No";
    public string GuestsInside { get; set; } = "No";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorFurnitureHaulAwayStep1Input
{
    public int PropertyId { get; set; }
    public List<string> FurnitureItemsList { get; set; } = ["Couch", "Mattress"];
    public string ItemCount { get; set; } = "TwoThree";
    public string PickupSize { get; set; } = "HalfLoad";
    public string IsOccupied { get; set; } = "No";
    public string GuestsInside { get; set; } = "No";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorFurnitureHaulAwayStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string FurnitureItems { get; set; } = "";
    public string ItemCount { get; set; } = "TwoThree";
    public string PickupSize { get; set; } = "HalfLoad";
    public string IsOccupied { get; set; } = "No";
    public string GuestsInside { get; set; } = "No";
    public string QuickDetails { get; set; } = "";
    public string PickupWhen { get; set; } = "TodayAfterCheckout";
    public string TimeWindow { get; set; } = "Afternoon";
    public string EntryAccess { get; set; } = "SmartLock";
    public string EntryCode { get; set; } = "";
    public string ItemLocations { get; set; } = "LivingRoom";
    public string AccessLevel { get; set; } = "GroundFloor";
    public string UpdateRecipient { get; set; } = "Me";
    public string ContactPhone { get; set; } = "";
    public string CrewEtaLabel { get; set; } = "Earliest available haul-away crew: 3:10 PM today";
}

public class PropertyAdministratorFurnitureHaulAwaySubmitInput : PropertyAdministratorFurnitureHaulAwayStep1Input
{
    public string PickupWhen { get; set; } = "TodayAfterCheckout";
    public string TimeWindow { get; set; } = "Afternoon";
    public string EntryAccess { get; set; } = "SmartLock";
    public string? EntryCode { get; set; }
    public List<string> ItemLocationsList { get; set; } = ["LivingRoom"];
    public string AccessLevel { get; set; } = "GroundFloor";
    public List<string> UpdateRecipientsList { get; set; } = ["Me"];
    public string? ContactPhone { get; set; }
}

public class PropertyAdministratorFurnitureHaulAwaySummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorFurnitureHaulAwayTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorFurnitureHaulAwayConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorFurnitureHaulAwaySummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorFurnitureHaulAwayTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Move small valuables aside before the crew arrives.";
}
