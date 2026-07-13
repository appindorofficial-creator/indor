namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorTrashOutFeaturedViewModel
{
    public string Title { get; set; } = "Trash Out Service";
    public string Subtitle { get; set; } = "Curbside bin take-out and return for rentals and Airbnb turnovers.";
    public string StartUrl { get; set; } = "#";
}

public class PropertyAdministratorTrashOutStep1ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceNeed { get; set; } = "";
    public string Bins { get; set; } = "";
    public string BinCount { get; set; } = "";
    public string BinLocation { get; set; } = "";
    public string PickupDay { get; set; } = "";
    public string QuickNotes { get; set; } = "";
    public string FlatRateLabel { get; set; } = "$30";
}

public class PropertyAdministratorTrashOutStep1Input
{
    public int PropertyId { get; set; }
    public string ServiceNeed { get; set; } = "";
    public List<string> BinsList { get; set; } = [];
    public string BinCount { get; set; } = "";
    public string BinLocation { get; set; } = "";
    public string PickupDay { get; set; } = "";
    public string? QuickNotes { get; set; }
}

public class PropertyAdministratorTrashOutStep2ViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int PropertyId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public string? PropertyStatusLabel { get; set; }
    public string ServiceNeed { get; set; } = "";
    public string Bins { get; set; } = "";
    public string BinCount { get; set; } = "";
    public string BinLocation { get; set; } = "";
    public string PickupDay { get; set; } = "";
    public string QuickNotes { get; set; } = "";
    public string TakeOutTiming { get; set; } = "";
    public string BringInTiming { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string AccessMethod { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string AccessNotes { get; set; } = "";
    public string UpdateRecipients { get; set; } = "";
    public string AvailabilityLabel { get; set; } = "Available tomorrow evening";
    public string ServiceTotalLabel { get; set; } = "$30";
    public string ServiceTotalDescription { get; set; } = "Take bins out + bring back in";
}

public class PropertyAdministratorTrashOutSubmitInput : PropertyAdministratorTrashOutStep1Input
{
    public string TakeOutTiming { get; set; } = "";
    public string BringInTiming { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string AccessMethod { get; set; } = "";
    public string? ContactPhone { get; set; }
    public string? AccessNotes { get; set; }
    public List<string> UpdateRecipientsList { get; set; } = [];
}

public class PropertyAdministratorTrashOutSummaryItemViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string IconClass { get; set; } = "";
}

public class PropertyAdministratorTrashOutTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string State { get; set; } = "pending";
}

public class PropertyAdministratorTrashOutConfirmedViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
    public IReadOnlyList<PropertyAdministratorTrashOutSummaryItemViewModel> Summary { get; set; } = [];
    public IReadOnlyList<PropertyAdministratorTrashOutTimelineItemViewModel> Timeline { get; set; } = [];
    public string RunnerLabel { get; set; } = "Homecare runner assigned";
    public string ArrivalWindow { get; set; } = "";
    public string Tip { get; set; } = "Leave gate access clear so the bins can be rolled out quickly.";
}
