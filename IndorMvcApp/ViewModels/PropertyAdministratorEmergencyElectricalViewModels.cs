namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorEmergencyFeaturedViewModel
{
    public string Title { get; set; } = "Emergency Electrical";
    public string Subtitle { get; set; } = "Fast electrical help for occupied homes and Airbnb stays";
    public string StartUrl { get; set; } = "#";
    public string IconClass { get; set; } = "fa-bolt";
    public string PriorityBadge { get; set; } = "24/7 Priority";
}

public class PropertyAdministratorNearestProViewModel
{
    public string ProTypeLabel { get; set; } = "electrician";
    public string EtaMinutes { get; set; } = "24";
    public string TrustLabel { get; set; } = "Licensed • Insured • Background checked";
}

public class PropertyAdministratorRecentRequestViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string PropertyName { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusCss { get; set; } = "inprogress";
    public string Url { get; set; } = "#";
}

public class PropertyAdministratorEmergencyElectricalFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string IssueType { get; set; } = "PowerOutage";
    public string PowerFullyOut { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string ProblemLocation { get; set; } = "LivingRoom";
    public string EntryAccess { get; set; } = "SmartLock";
    public string ContactPhone { get; set; } = "";
    public string Notes { get; set; } = "";
    public string ProEtaLabel { get; set; } = "Nearest electrician pro available in 24 minutes";
}

public class PropertyAdministratorEmergencyElectricalSubmitInput
{
    public int PropertyId { get; set; }
    public string IssueType { get; set; } = "PowerOutage";
    public string PowerFullyOut { get; set; } = "Yes";
    public string GuestsInside { get; set; } = "Yes";
    public string Urgency { get; set; } = "Emergency";
    public string ProblemLocation { get; set; } = "LivingRoom";
    public string EntryAccess { get; set; } = "SmartLock";
    public string ContactPhone { get; set; } = "";
    public string? Notes { get; set; }
}

public class PropertyAdministratorEmergencyElectricalSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
    public bool Highlight { get; set; }
}

public class PropertyAdministratorEmergencyElectricalConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public string Tip { get; set; } = "Ask guests to avoid touching affected outlets or switches until help arrives.";
}
