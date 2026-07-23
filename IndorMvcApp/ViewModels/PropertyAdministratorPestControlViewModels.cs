namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorPestControlFeaturedViewModel
{
    public string Title { get; set; } = "Pest Control";
    public string Subtitle { get; set; } = "Fast dispatch for ants, roaches, rodents, and rental property pest issues.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorPestControlStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string PestType { get; set; } = "";
    public string IssueLocation { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string GuestsStaying { get; set; } = "";
    public string LivePestsToday { get; set; } = "";
    public string QuickDetails { get; set; } = "";
}

public class PropertyAdministratorPestControlStep1Input
{
    public int PropertyId { get; set; }
    public string PestType { get; set; } = "";
    public string IssueLocation { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string GuestsStaying { get; set; } = "";
    public string LivePestsToday { get; set; } = "";
    public string? QuickDetails { get; set; }
}

public class PropertyAdministratorPestControlStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string PestType { get; set; } = "";
    public string IssueLocation { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string GuestsStaying { get; set; } = "";
    public string LivePestsToday { get; set; } = "";
    public string QuickDetails { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string PreferredArrival { get; set; } = "";
    public string PreferredTime { get; set; } = "11:00 AM";
    public string EntryAccess { get; set; } = "";
    public string HasPets { get; set; } = "";
    public string TreatAreas { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string AccessNotes { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string ProEtaLabel { get; set; } = "Earliest available pest control pro: 38 min";
}

public class PropertyAdministratorPestControlSubmitInput : PropertyAdministratorPestControlStep1Input
{
    public string ServiceType { get; set; } = "";
    public string PreferredArrival { get; set; } = "";
    public string PreferredTime { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string HasPets { get; set; } = "";
    public List<string> TreatAreasList { get; set; } = [];
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string? AccessNotes { get; set; }
    public string? ContactPhone { get; set; }
}

public class PropertyAdministratorPestControlSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorPestControlTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorPestControlConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorPestControlSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPestControlTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Ask guests to keep food sealed and pets away from the treatment area until the technician arrives.";
}
