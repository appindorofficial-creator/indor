namespace IndorMvcApp.ViewModels;

public class ProviderProFlowStepViewModel
{
    public string Label { get; set; } = "";
    public string? IconClass { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsLink { get; set; }
    public string? Url { get; set; }
}

public class ProviderProJobsScheduleViewModel : ProviderProPageBaseViewModel
{
    public string ActiveView { get; set; } = "today";
    public string DateLabel { get; set; } = "";
    public int JobsTodayCount { get; set; }
    public List<ProviderProScheduleJobItemViewModel> Jobs { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProScheduleJobItemViewModel
{
    public int Id { get; set; }
    public string TimeLabel { get; set; } = "";
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "scheduled";
}

public class ProviderProCreateJobCategoryOptionViewModel
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Description { get; set; } = "";
    public string IconClass { get; set; } = "fa-wrench";
}

public class ProviderProCreateJobCategoriesViewModel : ProviderProPageBaseViewModel
{
    public List<ProviderProCreateJobCategoryOptionViewModel> Categories { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateJobCustomerOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Address { get; set; }
}

public class ProviderProCreateJobDetailsViewModel : ProviderProPageBaseViewModel
{
    public string ServiceCategoryId { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 3;
    public string Title { get; set; } = "";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Notes { get; set; } = "";
    public List<ProviderProCreateJobCustomerOptionViewModel> Customers { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateJobScheduleViewModel : ProviderProPageBaseViewModel
{
    public string ServiceCategoryLabel { get; set; } = "";
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 3;
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "9:00 AM";
    public string EndTimeLabel { get; set; } = "11:00 AM";
    public bool AddToCalendar { get; set; } = true;
    public string Reminder { get; set; } = "30 minutes before";
    public string AssignedTechnician { get; set; } = "";
    public List<string> TimeOptions { get; set; } = [];
    public List<string> ReminderOptions { get; set; } = [];
    public List<string> TechnicianOptions { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateJobReviewViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 3;
    public int TotalSteps { get; set; } = 3;
    public string Title { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "";
    public string PriorityClass { get; set; } = "medium";
    public string ServiceCategoryLabel { get; set; } = "";
    public string ScheduleDateLabel { get; set; } = "";
    public string ScheduleTimeLabel { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
    public string Reminder { get; set; } = "";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateJobSuccessViewModel : ProviderProPageBaseViewModel
{
    public int JobId { get; set; }
    public string JobCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string ScheduleLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "Scheduled";
    public string StatusClass { get; set; } = "scheduled";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateJobDraft
{
    public string ServiceCategoryId { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public string Title { get; set; } = "";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Notes { get; set; } = "";
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "";
    public string EndTimeLabel { get; set; } = "";
    public bool AddToCalendar { get; set; } = true;
    public string Reminder { get; set; } = "30 minutes before";
    public string AssignedTechnician { get; set; } = "";
}

public class ProviderProCreateJobViewModel : ProviderProPageBaseViewModel
{
    public int? JobId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceCategory { get; set; } = "";
    public string Title { get; set; } = "";
    public string VisitDate { get; set; } = "";
    public string ScheduleDateLabel { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Notes { get; set; } = "";
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProJobDetailsViewModel : ProviderProPageBaseViewModel
{
    public int JobId { get; set; }
    public string JobCode { get; set; } = "";
    public string? EstimateCode { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "scheduled";
    public string? DistanceLabel { get; set; }
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public string AppointmentLabel { get; set; } = "";
    public string InvoiceStatus { get; set; } = "Pending";
    public string? PaymentLabel { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerInitials { get; set; } = "";
    public bool IsHomeownerVerified { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? ScopeOfWork { get; set; }
    public string? MaterialsNeeded { get; set; }
    public string? AccessInstructions { get; set; }
    public string? JobNotes { get; set; }
    public List<ProviderProJobChecklistItemViewModel> Checklist { get; set; } = [];
    public int ChecklistCompleted { get; set; }
    public int ChecklistTotal { get; set; }
    public bool CanStart { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProActiveJobViewModel : ProviderProPageBaseViewModel
{
    public int JobId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string? DistanceLabel { get; set; }
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public string StartedLabel { get; set; } = "";
    public string ElapsedLabel { get; set; } = "";
    public List<ProviderProJobChecklistItemViewModel> Checklist { get; set; } = [];
    public List<string> PhotoLabels { get; set; } = [];
    public List<ProviderProJobMaterialViewModel> Materials { get; set; } = [];
    public string? JobNotes { get; set; }
    public string? HomeownerSignature { get; set; }
    public string? SignatureLabel { get; set; }
    public bool HasSignature { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProJobCompletionReportViewModel : ProviderProPageBaseViewModel
{
    public int JobId { get; set; }
    public string ReportCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CompletedLabel { get; set; } = "";
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public List<ProviderProJobPhotoLabelViewModel> Photos { get; set; } = [];
    public string? WorkPerformed { get; set; }
    public List<ProviderProJobMaterialViewModel> Materials { get; set; } = [];
    public string? LaborWarranty { get; set; }
    public string? FinalNotes { get; set; }
    public string? HomeownerSignature { get; set; }
    public string? SignedLabel { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProJobChecklistItemViewModel
{
    public string Label { get; set; } = "";
    public bool IsCompleted { get; set; }
    public bool IsInProgress { get; set; }
    public string? CompletedLabel { get; set; }
}

public class ProviderProJobMaterialViewModel
{
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
}

public class ProviderProJobPhotoLabelViewModel
{
    public string Url { get; set; } = "";
    public string Label { get; set; } = "";
}

public class ProviderProCreateJobDetailsInput
{
    public string ServiceCategoryId { get; set; } = "";
    public string Title { get; set; } = "";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Notes { get; set; } = "";
}

public class ProviderProCreateJobScheduleInput
{
    public string VisitDate { get; set; } = "";
    public string StartTimeLabel { get; set; } = "";
    public string EndTimeLabel { get; set; } = "";
    public bool AddToCalendar { get; set; } = true;
    public string Reminder { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
}

public class ProviderProCreateJobInput
{
    public string ServiceCategoryId { get; set; } = "";
    public string ServiceCategory { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public int? ClienteId { get; set; }
    public string Address { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? VisitDate { get; set; }
    public string StartTimeLabel { get; set; } = "";
    public string EndTimeLabel { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public string AssignedTechnician { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Notes { get; set; } = "";
    public string Reminder { get; set; } = "";
    public bool AddToCalendar { get; set; } = true;
    public bool SaveAsDraft { get; set; }
}
