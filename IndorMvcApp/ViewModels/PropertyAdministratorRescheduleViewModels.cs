namespace IndorMvcApp.ViewModels;

public class PropertyAdministratorRescheduleFormViewModel : PropertyAdministratorPortalShellViewModel
{
    public int FlowStep { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public int RequestId { get; set; }
    public string FromAction { get; set; } = "Tasks";
    public string ServiceTitle { get; set; } = "";
    public string PropertyName { get; set; } = "";
    public string Location { get; set; } = "";
    public string CurrentScheduleLabel { get; set; } = "";
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "11:00 AM";
    public string EndTimeLabel { get; set; } = "2:00 PM";
    public IReadOnlyList<string> TimeOptions { get; set; } = [];
    public PropertyAdministratorFlowPropertyViewModel? ViewingProperty { get; set; }
}

public class PropertyAdministratorRescheduleSubmitInput
{
    public int RequestId { get; set; }
    public string FromAction { get; set; } = "Tasks";
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "";
    public string EndTimeLabel { get; set; } = "";
}
