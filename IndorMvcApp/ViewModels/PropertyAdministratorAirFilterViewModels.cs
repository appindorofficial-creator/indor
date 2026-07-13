namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorAirFilterFeaturedViewModel
{
    public string Title { get; set; } = "Air Filter Change";
    public string Subtitle { get; set; } = "Fast scheduled filter replacement for occupied homes and Airbnb stays.";
    public string StartUrl { get; set; } = "#";
    public IReadOnlyList<PropertyAdministratorAirFilterBenefitViewModel> Benefits { get; set; } = [];
}

public class PropertyAdministratorAirFilterBenefitViewModel
{
    public string Label { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorAirFilterFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ServiceType { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string FilterSize { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Details { get; set; } = "";
    public string EstimatedPrice { get; set; } = "$59–$89";
    public string ProEtaLabel { get; set; } = "Nearest homecare pro available tomorrow 10–12 AM";
}

public class PropertyAdministratorAirFilterSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "";
    public string IsOccupied { get; set; } = "";
    public string GuestsInside { get; set; } = "";
    public string FilterSize { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string? Details { get; set; }
}

public class PropertyAdministratorAirFilterTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorAirFilterConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorAirFilterTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Replacing filters every 2–3 months helps protect airflow and air quality.";
}
