namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyAcFeaturedViewModel
{
    public string Title { get; set; } = "Emergency AC";
    public string Subtitle { get; set; } = "Fast help for occupied homes and Airbnb stays";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorFlowPropertyViewModel
{
    public int Id { get; set; }
    public string PropertyName { get; set; } = "";
    public string PropertyTypeLabel { get; set; } = "";
    public string Location { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? OccupancyLabel { get; set; }
}

public class PropertyAdministratorEmergencyAcFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ProblemType { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string IndoorTemperature { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string EntryCode { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Details { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$129–$169";
    public string ProEtaLabel { get; set; } = "Nearest HVAC pro available in 25 minutes";
}

public class PropertyAdministratorEmergencyAcSubmitInput
{
    public int PropertyId { get; set; }
    public string ProblemType { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string IndoorTemperature { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string? EntryCode { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string? Details { get; set; }
}

public class PropertyAdministratorEmergencyAcTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorEmergencyAcConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "Keep blinds closed and portable fans running while help is on the way.";
}
