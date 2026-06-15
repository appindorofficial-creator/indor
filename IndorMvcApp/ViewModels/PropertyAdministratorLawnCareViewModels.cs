namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorLawnCareFeaturedViewModel
{
    public string Title { get; set; } = "Lawn Care / Grass Cutting";
    public string Subtitle { get; set; } = "Grass cutting, edging, and yard refresh for rentals and Airbnb turnovers.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorLawnCareStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceType { get; set; } = "GrassCutting";
    public string YardArea { get; set; } = "Both";
    public string YardSize { get; set; } = "Medium";
    public string Frequency { get; set; } = "OneTime";
    public string IsOccupied { get; set; } = "Yes";
    public string AccessDetails { get; set; } = "";
    public string QuickNotes { get; set; } = "";
    public string AvailabilityLabel { get; set; } = "Available tomorrow morning";
    public string AvailabilityWindow { get; set; } = "8:00 AM – 12:00 PM";
}

public class PropertyAdministratorLawnCareStep1Input
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "GrassCutting";
    public string YardArea { get; set; } = "Both";
    public string YardSize { get; set; } = "Medium";
    public string Frequency { get; set; } = "OneTime";
    public string IsOccupied { get; set; } = "Yes";
    public string? AccessDetails { get; set; }
    public string? QuickNotes { get; set; }
}

public class PropertyAdministratorLawnCareStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceType { get; set; } = "GrassCutting";
    public string YardArea { get; set; } = "Both";
    public string YardSize { get; set; } = "Medium";
    public string Frequency { get; set; } = "OneTime";
    public string IsOccupied { get; set; } = "Yes";
    public string AccessDetails { get; set; } = "";
    public string QuickNotes { get; set; } = "";
    public string ScheduleWhen { get; set; } = "Asap";
    public string ArrivalWindow { get; set; } = "Morning";
    public string AddOns { get; set; } = "Edging,BlowDebris";
    public string BagClippings { get; set; } = "Yes";
    public string UpdateRecipients { get; set; } = "Me,Guest";
    public string AccessNotes { get; set; } = "Park in driveway. Enter through left gate.";
    public string AvoidNotes { get; set; } = "Please avoid flower bed near the front porch.";
}

public class PropertyAdministratorLawnCareSubmitInput : PropertyAdministratorLawnCareStep1Input
{
    public string ScheduleWhen { get; set; } = "Asap";
    public string ArrivalWindow { get; set; } = "Morning";
    public List<string> AddOnsList { get; set; } = ["Edging", "BlowDebris"];
    public string BagClippings { get; set; } = "Yes";
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "Guest"];
    public string? AccessNotes { get; set; }
    public string? AvoidNotes { get; set; }
}

public class PropertyAdministratorLawnCareSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorLawnCareTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
    public int StepNumber { get; set; }
}

public class PropertyAdministratorLawnCareConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string TechnicianBadges { get; set; } = "Insured • Background checked";
    public string EtaLabel { get; set; } = "";
    public string VehicleLabel { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorLawnCareSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorLawnCareTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Lawn service is best completed before guest check-in. We'll send photo confirmation when finished.";
}
