namespace IndorMvcApp.ViewModels;



public class ProviderProEstimateLineItemViewModel
{
    public string Label { get; set; } = "";
    public string Category { get; set; } = "labor";
    public string Description { get; set; } = "";
    public decimal Qty { get; set; } = 1;
    public string Unit { get; set; } = "ls";
    public decimal UnitPrice { get; set; }
    public decimal LaborAmount { get; set; }
    public decimal MaterialAmount { get; set; }
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; } = true;
}

public class ProviderProCreateEstimateOptionViewModel
{
    public int Id { get; set; }
    public string Label { get; set; } = "";
    public string? SubLabel { get; set; }
}

public class ProviderProCreateEstimateSetupViewModel : ProviderProPageBaseViewModel
{
    public string EstimateType { get; set; } = "new";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceCategoryId { get; set; } = "";
    public int? LeadId { get; set; }
    public int? JobId { get; set; }
    public List<ProviderProCreateEstimateOptionViewModel> Customers { get; set; } = [];
    public List<ProviderProCreateEstimateOptionViewModel> Addresses { get; set; } = [];
    public List<ProviderProCreateJobCategoryOptionViewModel> Categories { get; set; } = [];
    public List<ProviderProCreateEstimateOptionViewModel> Leads { get; set; } = [];
    public List<ProviderProCreateEstimateOptionViewModel> Jobs { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateEstimateDetailsViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 4;
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string EstimatedStartDate { get; set; } = "";
    public string EstimatedEndDate { get; set; } = "";
    public string Warranty { get; set; } = "1 Year Parts & Labor";
    public string Notes { get; set; } = "";
    public List<string> WarrantyOptions { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateEstimatePricingViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 3;
    public int TotalSteps { get; set; } = 4;
    public decimal TaxRate { get; set; } = 0.0825m;
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ProviderProEstimateLineItemViewModel> LineItems { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateEstimateReviewViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 4;
    public int TotalSteps { get; set; } = 4;
    public int? EstimateId { get; set; }
    public string Title { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Warranty { get; set; } = "";
    public string TimelineLabel { get; set; } = "";
    public string DeliveryMethod { get; set; } = "Email";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateEstimateDraft
{
    public string EstimateType { get; set; } = "new";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceCategoryId { get; set; } = "";
    public string ServiceCategoryLabel { get; set; } = "";
    public int? LeadId { get; set; }
    public int? JobId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string EstimatedStartDate { get; set; } = "";
    public string EstimatedEndDate { get; set; } = "";
    public string EstimatedDuration { get; set; } = "";
    public string Warranty { get; set; } = "1 Year Parts & Labor";
    public string Notes { get; set; } = "";
    public decimal TaxRate { get; set; } = 0.0825m;
    public List<ProviderProEstimateLineItemViewModel> LineItems { get; set; } = [];
    public string DeliveryMethod { get; set; } = "Email";
    public int? EstimateId { get; set; }
}



public class ProviderProQuickEstimateViewModel : ProviderProPageBaseViewModel

{

    public int LeadId { get; set; }

    public int? EstimateId { get; set; }

    public string PageTitle { get; set; } = "Quick Estimate";

    public string EstimateCode { get; set; } = "";

    public string StatusLabel { get; set; } = "";

    public string? CreatedLabel { get; set; }

    public string? PropertyMeta { get; set; }

    public string Address { get; set; } = "";

    public string ServiceType { get; set; } = "";

    public string Urgency { get; set; } = "";

    public bool IsHighUrgency { get; set; }

    public string? DistanceLabel { get; set; }

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string CustomerName { get; set; } = "";

    public string CustomerInitials { get; set; } = "";

    public bool IsHomeownerVerified { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public List<ProviderProEstimateLineItemViewModel> ScopeItems { get; set; } = [];

    public decimal LaborAmount { get; set; }

    public decimal MaterialsAmount { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Timeline { get; set; } = "";

    public string EstimatedStartDate { get; set; } = "";

    public string EstimatedDuration { get; set; } = "";

    public string Warranty { get; set; } = "";

    public string LaborWarranty { get; set; } = "";

    public string PartsWarranty { get; set; } = "";

    public string HomeownerNotes { get; set; } = "";

    public string ActiveStep { get; set; } = "scope";

    public string BackUrl { get; set; } = "";

    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];

    public int StepNumber { get; set; } = 4;

    public int TotalSteps { get; set; } = 5;

    public string StatusClass { get; set; } = "needs-review";

    public string ServiceIcon { get; set; } = "fa-wrench";

    public string ServiceTone { get; set; } = "blue";

    public string Title { get; set; } = "";

    public string DateLabel { get; set; } = "";

    public int PhotoCount { get; set; }

    public int VoiceTranscriptCount { get; set; }

    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];

    public string LeadCode { get; set; } = "";

    public string? ProblemDescription { get; set; }

    public string? InspectionReportUrl { get; set; }

    public string? SourceBadge { get; set; }

    public List<ProviderInspectionFindingItemViewModel> InspectionFindings { get; set; } = [];

    public List<string> PhotoUrls { get; set; } = [];

}



public class ProviderProReviewEstimateViewModel : ProviderProPageBaseViewModel

{

    public int LeadId { get; set; }

    public int EstimateId { get; set; }

    public string EstimateCode { get; set; } = "";

    public string? CreatedLabel { get; set; }

    public string? ValidForLabel { get; set; }

    public string? PropertyMeta { get; set; }

    public string Address { get; set; } = "";

    public string ServiceType { get; set; } = "";

    public string Urgency { get; set; } = "";

    public bool IsHighUrgency { get; set; }

    public string? DistanceLabel { get; set; }

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string CustomerName { get; set; } = "";

    public string CustomerInitials { get; set; } = "";

    public bool IsHomeownerVerified { get; set; }

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public List<ProviderProEstimateLineItemViewModel> ScopeItems { get; set; } = [];

    public decimal LaborAmount { get; set; }

    public decimal MaterialsAmount { get; set; }

    public decimal SubtotalAmount { get; set; }

    public decimal TaxRate { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Timeline { get; set; } = "";

    public string? EstimatedStartLabel { get; set; }

    public string? EstimatedDuration { get; set; }

    public string Warranty { get; set; } = "";

    public string LaborWarranty { get; set; } = "";

    public string PartsWarranty { get; set; } = "";

    public string HomeownerNotes { get; set; } = "";

    public bool NotifyHomeowner { get; set; } = true;

    public bool SaveCopyToLeads { get; set; } = true;

    public bool CanSend { get; set; } = true;

    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];

    public int StepNumber { get; set; } = 3;

    public int TotalSteps { get; set; } = 5;

    public string Title { get; set; } = "";

    public string StatusLabel { get; set; } = "";

    public string StatusClass { get; set; } = "needs-review";

    public string DateLabel { get; set; } = "";

    public string ServiceIcon { get; set; } = "fa-wrench";

    public string ServiceTone { get; set; } = "blue";

    public List<string> ScopeSummaryLines { get; set; } = [];

    public List<string> AiRecommendations { get; set; } = [];

    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];

}



public class ProviderProPendingEstimatesPageViewModel : ProviderProPageBaseViewModel

{

    public string ActiveTab { get; set; } = "all";

    public string? SearchQuery { get; set; }

    public int DraftCount { get; set; }

    public int ReadyCount { get; set; }

    public int SentCount { get; set; }

    public int PendingCount { get; set; }

    public int AiDraftCount { get; set; }

    public int NeedsReviewCount { get; set; }

    public int StepNumber { get; set; } = 2;

    public int TotalSteps { get; set; } = 5;

    public List<ProviderProPendingEstimateCardViewModel> Estimates { get; set; } = [];

    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];

    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];

}



public class ProviderProPendingEstimateCardViewModel

{

    public int Id { get; set; }

    public string EstimateCode { get; set; } = "";

    public string Title { get; set; } = "";

    public string CustomerName { get; set; } = "";

    public string Address { get; set; } = "";

    public string? ServiceType { get; set; }

    public string Status { get; set; } = "";

    public string StatusClass { get; set; } = "draft";

    public string FilterKey { get; set; } = "";

    public string ServiceIcon { get; set; } = "fa-wrench";

    public string ServiceTone { get; set; } = "blue";

    public string DateLabel { get; set; } = "";

    public decimal Amount { get; set; }

    public bool CanEdit { get; set; }

    public bool CanReview { get; set; }

    public bool CanSend { get; set; }

    public bool CanView { get; set; }

}



public class ProviderProEstimateSentViewModel : ProviderProPageBaseViewModel

{

    public int EstimateId { get; set; }

    public int? LeadId { get; set; }

    public string EstimateCode { get; set; } = "";

    public string Address { get; set; } = "";

    public string ServiceType { get; set; } = "";

    public decimal TotalAmount { get; set; }

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string StatusLabel { get; set; } = "Sent";

    public bool IsApproved { get; set; }

    public bool CanConvertToJob { get; set; }

    public int? ConvertedJobId { get; set; }

    public List<ProviderProEstimateTrackingStepViewModel> TrackingSteps { get; set; } = [];

    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];

}



public class ProviderProEstimateTrackingStepViewModel

{

    public string Label { get; set; } = "";

    public string? Detail { get; set; }

    public string IconClass { get; set; } = "";

    public string StateClass { get; set; } = "pending";

}



public class ProviderProCreateInvoiceInput
{
    public int EstimateId { get; set; }
    public string PaymentTerms { get; set; } = "Due at completion";
    public bool IncludeServiceCall { get; set; }
    public decimal ServiceCallAmount { get; set; }
    public bool GoToReview { get; set; }
}

public class ProviderProQuickEstimateInput

{

    public int LeadId { get; set; }

    public int? EstimateId { get; set; }

    public string ServiceType { get; set; } = "";

    public decimal LaborAmount { get; set; }

    public decimal MaterialsAmount { get; set; }

    public string Timeline { get; set; } = "";

    public string? EstimatedStartDate { get; set; }

    public string EstimatedDuration { get; set; } = "";

    public string Warranty { get; set; } = "";

    public string LaborWarranty { get; set; } = "";

    public string PartsWarranty { get; set; } = "";

    public string HomeownerNotes { get; set; } = "";

    public decimal TaxRate { get; set; }

    public decimal DiscountAmount { get; set; }

    public List<string> ScopeLabels { get; set; } = [];

    public List<decimal> ScopeAmounts { get; set; } = [];

    public List<decimal> ScopeQtys { get; set; } = [];

    public List<decimal> ScopeUnitPrices { get; set; } = [];

    public List<decimal> ScopeLaborAmounts { get; set; } = [];

    public List<decimal> ScopeMaterialAmounts { get; set; } = [];

    public decimal ServiceCallAmount { get; set; }

    public bool IncludeServiceCall { get; set; }

    public bool SaveAsDraft { get; set; }

    public bool GoToReview { get; set; }

    public bool GoToSend { get; set; }

}



public class ProviderProCreateEstimateSetupInput
{
    public string EstimateType { get; set; } = "new";
    public int? ClienteId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceCategoryId { get; set; } = "";
    public int? LeadId { get; set; }
    public int? JobId { get; set; }
}

public class ProviderProCreateEstimateDetailsInput
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "Medium";
    public string EstimatedStartDate { get; set; } = "";
    public string EstimatedEndDate { get; set; } = "";
    public string Warranty { get; set; } = "";
    public string Notes { get; set; } = "";
}

public class ProviderProCreateEstimatePricingInput
{
    public decimal TaxRate { get; set; } = 0.0825m;
    public List<string> LineCategories { get; set; } = [];
    public List<string> LineLabels { get; set; } = [];
    public List<string> LineDescriptions { get; set; } = [];
    public List<decimal> LineQtys { get; set; } = [];
    public List<string> LineUnits { get; set; } = [];
    public List<decimal> LineUnitPrices { get; set; } = [];
    public List<decimal> LineAmounts { get; set; } = [];
    public List<string> LineTaxable { get; set; } = [];
}

public class ProviderProSendEstimateInput
{
    public int EstimateId { get; set; }
    public string DeliveryMethod { get; set; } = "indor";
    public string CustomerMessage { get; set; } = "";
    public string PostSendAction { get; set; } = "wait_approval";
    public bool NotifyHomeowner { get; set; } = true;
    public bool SaveCopyToLeads { get; set; } = true;
    public bool SaveAsDraft { get; set; }
}

public class ProviderProSendEstimatePageViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 5;
    public int TotalSteps { get; set; } = 5;
    public int EstimateId { get; set; }
    public string Title { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string DateLabel { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string ServiceIcon { get; set; } = "fa-wrench";
    public string ServiceTone { get; set; } = "blue";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "needs-review";
    public decimal TotalAmount { get; set; }
    public string CustomerMessage { get; set; } = "";
    public string DeliveryMethod { get; set; } = "indor";
    public string PostSendAction { get; set; } = "wait_approval";
    public bool HasEstimatePdf { get; set; } = true;
    public bool HasAiSummary { get; set; } = true;
    public bool HasVoiceTranscript { get; set; }
    public List<ProviderProWizardStepViewModel> WizardSteps { get; set; } = [];
}


