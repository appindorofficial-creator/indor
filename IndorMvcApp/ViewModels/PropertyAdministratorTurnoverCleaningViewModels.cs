namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorTurnoverCleaningFeaturedViewModel
{
    public string Title { get; set; } = "Turnover Cleaning";
    public string Subtitle { get; set; } = "Guest-ready cleaning between stays with linens, restock, and setup.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorTurnoverCleaningFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string ServiceType { get; set; } = "";
    public string GuestArrival { get; set; } = "";
    /// <summary>Labeled hourly slot (e.g. 4:00 PM) or legacy HH:mm.</summary>
    public string GuestArrivalTime { get; set; } = "4:00 PM";
    public string IncludedTasks { get; set; } = "";
    public string UrgentIssue { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Details { get; set; } = "";
    public string? MediaAttachmentsJson { get; set; }
    public string? FormError { get; set; }
    public string EstimatedPrice { get; set; } = "$149–$249";
    public string ProEtaLabel { get; set; } = "Nearest turnover crew available today {0}";
    public string ProEtaTimeRange { get; set; } = "11 AM–2 PM";
    public string DurationLabel { get; set; } = "Usually 2-3 hours";
    public string CrewLabel { get; set; } = "2-4 cleaners";
}

public class PropertyAdministratorTurnoverCleaningSubmitInput
{
    public int PropertyId { get; set; }
    public string ServiceType { get; set; } = "";
    public string GuestArrival { get; set; } = "";
    /// <summary>Guest arrival time; accepted as labeled slot or HH:mm. Normalized on submit.</summary>
    public string GuestArrivalTime { get; set; } = "4:00 PM";
    public List<string> IncludedTasksList { get; set; } = [];
    public string UrgentIssue { get; set; } = "";
    public string EntryAccess { get; set; } = "";
    public List<string> UpdateRecipientsList { get; set; } = [];
    public string ContactPhone { get; set; } = "";
    public string? Details { get; set; }
    public string? MediaAttachmentsJson { get; set; }
}

public class PropertyAdministratorTurnoverCleaningTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorTurnoverCleaningConfirmedViewModel : PropertyAdministratorPortalShellViewModel
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
    public IReadOnlyList<PropertyAdministratorTurnoverCleaningTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Turnover service can include linens, restocking, and guest-ready setup before check-in.";
}
