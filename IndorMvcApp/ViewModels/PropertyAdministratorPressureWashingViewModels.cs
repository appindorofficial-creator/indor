namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorPressureWashingFeaturedViewModel
{
    public string Title { get; set; } = "Pressure Washing";
    public string Subtitle { get; set; } = "Driveways, patios, walkways, and exterior surfaces for rental turnovers.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorPressureWashingStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string WashAreas { get; set; } = "Walkway,Patio";
    public string AreaSize { get; set; } = "Medium";
    public string ServiceReason { get; set; } = "GuestTurnover";
    public string IsOccupied { get; set; } = "Yes";
    public string GuestNotification { get; set; } = "Both";
    public string QuickNotes { get; set; } = "";
}

public class PropertyAdministratorPressureWashingStep1Input
{
    public int PropertyId { get; set; }
    public List<string> WashAreasList { get; set; } = ["Walkway", "Patio"];
    public string AreaSize { get; set; } = "Medium";
    public string ServiceReason { get; set; } = "GuestTurnover";
    public string IsOccupied { get; set; } = "Yes";
    public string GuestNotification { get; set; } = "Both";
    public string? QuickNotes { get; set; }
}

public class PropertyAdministratorPressureWashingStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string WashAreas { get; set; } = "";
    public string AreaSize { get; set; } = "Medium";
    public string ServiceReason { get; set; } = "GuestTurnover";
    public string IsOccupied { get; set; } = "Yes";
    public string GuestNotification { get; set; } = "Both";
    public string QuickNotes { get; set; } = "";
    public string ServiceTiming { get; set; } = "AfterCheckOut";
    public string ArrivalWindow { get; set; } = "Midday";
    public string HasWaterAccess { get; set; } = "Yes";
    public string HasPower { get; set; } = "Yes";
    public string EntryMethod { get; set; } = "GateCode";
    public string AccessNotes { get; set; } = "";
    public string UpdateRecipients { get; set; } = "Me,CoHost";
    public string CrewEtaLabel { get; set; } = "Earliest available exterior crew: Tomorrow, 11:00 AM";
}

public class PropertyAdministratorPressureWashingSubmitInput : PropertyAdministratorPressureWashingStep1Input
{
    public string ServiceTiming { get; set; } = "AfterCheckOut";
    public string ArrivalWindow { get; set; } = "Midday";
    public string HasWaterAccess { get; set; } = "Yes";
    public string HasPower { get; set; } = "Yes";
    public string EntryMethod { get; set; } = "GateCode";
    public string? AccessNotes { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = ["Me", "CoHost"];
}

public class PropertyAdministratorPressureWashingSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorPressureWashingTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
    public int StepNumber { get; set; }
}

public class PropertyAdministratorPressureWashingConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string TechnicianName { get; set; } = "";
    public decimal TechnicianRating { get; set; }
    public int TechnicianReviewCount { get; set; }
    public string TechnicianTitle { get; set; } = "";
    public string TechnicianSubtitle { get; set; } = "";
    public string EtaLabel { get; set; } = "";
    public string ServiceAreasLabel { get; set; } = "";
    public string EstimatedTotal { get; set; } = "";
    public IReadOnlyList<PropertyAdministratorPressureWashingSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorPressureWashingTimelineItemViewModel> Timeline { get; set; } = [];
    public string Tip { get; set; } = "Move small outdoor items and keep the area clear before the crew arrives.";
}
