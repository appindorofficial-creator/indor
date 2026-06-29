namespace IndorMvcApp.ViewModels;

public class ProviderProFlowStepViewModel
{
    public string Label { get; set; } = "";
    public string? IconClass { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsLink { get; set; }
    public string? Url { get; set; }
}

public class ProviderProWizardStepViewModel
{
    public int Number { get; set; }
    public string Label { get; set; } = "";
    public bool IsComplete { get; set; }
    public bool IsCurrent { get; set; }
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
    public string ToneClass { get; set; } = "blue";
    public string SuggestedJobName { get; set; } = "";
}

public class ProviderProCreateJobCategoriesViewModel : ProviderProPageBaseViewModel
{
    public string? SelectedCategoryId { get; set; }
    public string JobTitle { get; set; } = "";
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string StepSubtitle { get; set; } = "Choose the type of work";
    public List<ProviderProCreateJobCategoryOptionViewModel> Categories { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];
}

public class ProviderProCreateJobCustomerOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Initials { get; set; } = "";
    public string ToneClass { get; set; } = "blue";
    public string? Address { get; set; }
    public string PropertyLabel { get; set; } = "Primary Home";
    public bool IsConnected { get; set; }
    public int PropertiesCount { get; set; } = 1;
}

public class ProviderProCreateJobDetailsViewModel : ProviderProPageBaseViewModel
{
    public string ServiceCategoryId { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 5;
    public string StepSubtitle { get; set; } = "Select the customer";
    public string Title { get; set; } = "";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string Notes { get; set; } = "";
    public List<ProviderProCreateJobCustomerOptionViewModel> Customers { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];
}

public class ProviderProCreateJobEstimateLineViewModel
{
    public string Label { get; set; } = "";
    public decimal Amount { get; set; }
    public string AmountLabel { get; set; } = "";
}

public class ProviderProCreateJobAttachmentViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string SizeLabel { get; set; } = "";
    public string Kind { get; set; } = "image";
    public string? ThumbnailUrl { get; set; }
}

public class ProviderProCreateJobQuoteViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 3;
    public int TotalSteps { get; set; } = 5;
    public string StepSubtitle { get; set; } = "Create quote request";
    public string JobTitle { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public string ServiceCategoryIcon { get; set; } = "fa-droplet";
    public string ServiceCategoryTone { get; set; } = "blue";
    public string CustomerName { get; set; } = "";
    public string CustomerInitials { get; set; } = "";
    public string CustomerTone { get; set; } = "green";
    public bool SendQuote { get; set; } = true;
    public string QuoteRequestNotes { get; set; } = "";
    public int MaxCharacters { get; set; } = 1000;
    public List<ProviderProCreateJobAttachmentViewModel> Attachments { get; set; } = [];
    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];
}

public class ProviderProCreateJobAiDraftViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 4;
    public int TotalSteps { get; set; } = 5;
    public string StepSubtitle { get; set; } = "AI estimate assistant";
    public string JobTitle { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public string ServiceCategoryIcon { get; set; } = "fa-droplet";
    public string ServiceCategoryTone { get; set; } = "blue";
    public string CustomerName { get; set; } = "";
    public string CustomerInitials { get; set; } = "";
    public string CustomerTone { get; set; } = "green";
    public string AiCustomerNeeds { get; set; } = "";
    public List<string> AiRecommendedScope { get; set; } = [];
    public List<ProviderProCreateJobEstimateLineViewModel> EstimateLines { get; set; } = [];
    public string EstimateTotalLabel { get; set; } = "";
    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];
}

public class ProviderProCreateJobSendViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 5;
    public int TotalSteps { get; set; } = 5;
    public string StepSubtitle { get; set; } = "Finalize the job and quote";
    public string JobTitle { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public string ServiceCategoryIcon { get; set; } = "fa-droplet";
    public string ServiceCategoryTone { get; set; } = "blue";
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public List<ProviderProCreateJobEstimateLineViewModel> EstimateLines { get; set; } = [];
    public string EstimateTotalLabel { get; set; } = "";
    public string ScopeSummary { get; set; } = "";
    public string DeliveryMethod { get; set; } = "indor";
    public string CustomerMessage { get; set; } = "";
    public bool IncludeAiSummary { get; set; } = true;
    public bool IncludeVoiceTranscript { get; set; }
    public bool SendQuote { get; set; } = true;
    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];
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
    /// <summary>Bottom nav context: "home" when started from Home, otherwise "jobs".</summary>
    public string NavOrigin { get; set; } = "jobs";

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
    public bool SendQuote { get; set; } = true;
    public string QuoteRequestNotes { get; set; } = "";
    public bool HasVoiceRecording { get; set; }
    public bool AiDraftGenerated { get; set; }
    public string AiCustomerNeeds { get; set; } = "";
    public List<string> AiRecommendedScope { get; set; } = [];
    public List<ProviderProCreateJobEstimateLineViewModel> EstimateLines { get; set; } = [];
    public decimal EstimateTotal { get; set; }
    public string ScopeSummary { get; set; } = "";
    public string DeliveryMethod { get; set; } = "indor";
    public string CustomerMessage { get; set; } = "";
    public bool IncludeAiSummary { get; set; } = true;
    public bool IncludeVoiceTranscript { get; set; }
    public List<ProviderProCreateJobAttachmentViewModel> Attachments { get; set; } = [];
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

public class ProviderProCreateJobStep1Input
{
    public string ServiceCategoryId { get; set; } = "";
    public string Title { get; set; } = "";
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

public class ProviderProCreateJobQuoteInput
{
    public bool SendQuote { get; set; } = true;
    public string QuoteRequestNotes { get; set; } = "";
    public string SubmitAction { get; set; } = "ai";
}

public class ProviderProCreateJobAiDraftInput
{
    public string SubmitAction { get; set; } = "continue";
}

public class ProviderProCreateJobSendInput
{
    public string DeliveryMethod { get; set; } = "indor";
    public string CustomerMessage { get; set; } = "";
    public bool IncludeAiSummary { get; set; } = true;
    public bool IncludeVoiceTranscript { get; set; }
    public string SubmitAction { get; set; } = "job_and_quote";
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
    public bool SendQuoteWithJob { get; set; }
    public decimal? EstimateAmount { get; set; }
    public string? EstimateScopeSummary { get; set; }
    public string DeliveryMethod { get; set; } = "indor";
    public string CustomerMessage { get; set; } = "";
}
