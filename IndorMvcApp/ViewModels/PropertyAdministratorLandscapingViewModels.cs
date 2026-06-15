namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorLandscapingFeaturedViewModel
{
    public string Title { get; set; } = "Landscaping";
    public string Subtitle { get; set; } = "Garden refresh, plant replacement, and yard upgrades for rentals.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorLandscapingStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceType { get; set; } = "GardenRefresh";
    public string WorkArea { get; set; } = "FrontYard";
    public string ServiceReason { get; set; } = "CarDroveOverPlants";
    public string Timeline { get; set; } = "ThisWeek";
    public string IsOccupied { get; set; } = "No";
    public string QuickNotes { get; set; } = "";
}

public class PropertyAdministratorLandscapingStep1Input
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "GardenRefresh";
    public string WorkArea { get; set; } = "FrontYard";
    public string ServiceReason { get; set; } = "CarDroveOverPlants";
    public string Timeline { get; set; } = "ThisWeek";
    public string IsOccupied { get; set; } = "No";
    public string? QuickNotes { get; set; }
}

public class PropertyAdministratorLandscapingStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceType { get; set; } = "GardenRefresh";
    public string WorkArea { get; set; } = "FrontYard";
    public string ServiceReason { get; set; } = "CarDroveOverPlants";
    public string Timeline { get; set; } = "ThisWeek";
    public string IsOccupied { get; set; } = "No";
    public string QuickNotes { get; set; } = "";
    public string VisitType { get; set; } = "QuoteWalkthrough";
    public string ProvideMaterials { get; set; } = "Yes";
    public string HaulAwayType { get; set; } = "RemoveDamagedPlants";
    public string HasIrrigation { get; set; } = "No";
    public string YardAccess { get; set; } = "UnlockedAccess";
    public string PreferredDate { get; set; } = "Tomorrow";
    public string TimeWindow { get; set; } = "Morning";
    public string ProjectNotes { get; set; } = "";
    public string UpdateRecipients { get; set; } = "Me,CoHost";
}

public class PropertyAdministratorLandscapingSubmitInput : PropertyAdministratorLandscapingStep1Input
{
    public string VisitType { get; set; } = "QuoteWalkthrough";
    public string ProvideMaterials { get; set; } = "Yes";
    public string HaulAwayType { get; set; } = "RemoveDamagedPlants";
    public string HasIrrigation { get; set; } = "No";
    public string YardAccess { get; set; } = "UnlockedAccess";
    public string PreferredDate { get; set; } = "Tomorrow";
    public string TimeWindow { get; set; } = "Morning";
    public string? ProjectNotes { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "CoHost"];
}

public class PropertyAdministratorLandscapingSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorLandscapingTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorLandscapingConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public int TechnicianReviewCount { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string ConsultationLabel { get; set; } = "";
    public string VehicleLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorLandscapingSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorLandscapingTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Send photos of the damaged area to help the pro prepare an accurate estimate.";
}
