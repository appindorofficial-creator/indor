namespace IndorMvcApp.ViewModels;

public class ProviderProCalendarOverviewViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string ActiveView { get; set; } = "week";
    public string ActiveFilter { get; set; } = "all";
    public string WeekRangeLabel { get; set; } = "";
    public string WeekStartIso { get; set; } = "";
    public int WeekEventCount { get; set; }
    public string EstimatedWorkLabel { get; set; } = "";
    public string AvailableTimeLabel { get; set; } = "";
    public int UtilizationPercent { get; set; }
    public List<ProviderProCalendarDayHeaderViewModel> DayHeaders { get; set; } = [];
    public List<int> HourSlots { get; set; } = [];
    public List<ProviderProCalendarGridEventViewModel> GridEvents { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCalendarDayHeaderViewModel
{
    public string DayName { get; set; } = "";
    public int DayNumber { get; set; }
    public string DateIso { get; set; } = "";
    public bool IsToday { get; set; }
    public bool IsUnavailable { get; set; }
}

public class ProviderProCalendarGridEventViewModel
{
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public int DayIndex { get; set; }
    public int StartHour { get; set; }
    public int SpanHours { get; set; } = 1;
    public string ToneClass { get; set; } = "job";
}

public class ProviderProDayScheduleViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 5;
    public string DateIso { get; set; } = "";
    public string DateLabel { get; set; } = "";
    public int EventsTodayCount { get; set; }
    public int InProgressCount { get; set; }
    public string? NextJobTime { get; set; }
    public string? NextJobTitle { get; set; }
    public string EventsDeltaLabel { get; set; } = "";
    public List<ProviderProDayScheduleItemViewModel> Items { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProDayScheduleItemViewModel
{
    public int Id { get; set; }
    public string TimeLabel { get; set; } = "";
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "scheduled";
    public string IconClass { get; set; } = "fa-wrench";
    public bool CanStart { get; set; }
}

public class ProviderProRescheduleJobViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 4;
    public int TotalSteps { get; set; } = 5;
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "";
    public string EndTimeLabel { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
    public string DurationLabel { get; set; } = "2 hours";
    public string Notes { get; set; } = "";
    public bool NotifySms { get; set; } = true;
    public bool NotifyEmail { get; set; } = true;
    public bool IsRecurring { get; set; }
    public List<string> TimeOptions { get; set; } = [];
    public List<string> DurationOptions { get; set; } = [];
    public List<string> TechnicianOptions { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProRescheduleJobInput
{
    public int JobId { get; set; }
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "";
    public string EndTimeLabel { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
    public string DurationLabel { get; set; } = "2 hours";
    public string Notes { get; set; } = "";
    public bool NotifySms { get; set; } = true;
    public bool NotifyEmail { get; set; } = true;
    public bool IsRecurring { get; set; }
}

public class ProviderProCalendarUpdatedViewModel : ProviderProPageBaseViewModel
{
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string UpdatedDateLabel { get; set; } = "";
    public string UpdatedTimeLabel { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
    public string StatusLabel { get; set; } = "Rescheduled";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}
