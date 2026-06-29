namespace IndorMvcApp.ViewModels;

public class ProviderProDashboardViewModel
{
    public string CompanyName { get; set; } = "Your company";

    public string CompanyInitial { get; set; } = "P";

    public string Greeting { get; set; } = "Good morning";

    public bool IsVerified { get; set; }

    public bool IsProActive { get; set; }

    public bool ActivationPending { get; set; }

    public bool IsInsured { get; set; }

    public int ProviderScore { get; set; }

    public string ScoreLabel { get; set; } = "Great Work!";

    public string ScoreSubtext { get; set; } = "Keep building your INDOR profile";

    public ProviderProMetricsViewModel Metrics { get; set; } = new();

    public List<ProviderProJobItemViewModel> TodaysJobs { get; set; } = [];

    public List<ProviderProLeadItemViewModel> NewLeads { get; set; } = [];

    public List<ProviderProEstimateItemViewModel> PendingEstimates { get; set; } = [];

    public List<ProviderProApprovalItemViewModel> PendingApprovals { get; set; } = [];

    public ProviderProPaymentsSummaryViewModel Payments { get; set; } = new();

    public int HomeRecordsThisMonth { get; set; }

    public int HomeRecordsDelta { get; set; }

    public int HomesProtected { get; set; }

    public List<string> AiSuggestions { get; set; } = [];

    public List<ProviderProCalendarDayViewModel> UpcomingCalendar { get; set; } = [];

    public int UnreadMessages { get; set; }

    public bool ShowEstimateTutorial { get; set; }

    public List<ProviderProWizardStepViewModel> EstimateWizardSteps { get; set; } = [];
}

public class ProviderProMetricsViewModel
{
    public int JobsToday { get; set; }
    public int NewLeads { get; set; }
    public int PendingEstimates { get; set; }
    public decimal PaymentsDue { get; set; }
}

public class ProviderProJobItemViewModel
{
    public int Id { get; set; }
    public string TimeLabel { get; set; } = "";
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string IconClass { get; set; } = "fa-wrench";
    public string IconTone { get; set; } = "blue";
    public string Status { get; set; } = "";
    public string StatusClass { get; set; } = "scheduled";
}

public class ProviderProLeadItemViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public bool IsHighUrgency { get; set; }
}

public class ProviderProNewLeadsPageViewModel : ProviderProPageBaseViewModel
{
    public string ActiveFilter { get; set; } = "all";
    public string? SearchQuery { get; set; }
    public int NewCount { get; set; }
    public int InProgressCount { get; set; }
    public int RespondedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int HighUrgencyCount { get; set; }
    public List<ProviderProNewLeadCardViewModel> Leads { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProNewLeadCardViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string UrgencyClass { get; set; } = "standard";
    public string UrgencyIcon { get; set; } = "fa-clock";
    public bool IsHighUrgency { get; set; }
    public string? DistanceLabel { get; set; }
    public string StatusLabel { get; set; } = "New";
    public string StatusClass { get; set; } = "new";
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public bool CanAccept { get; set; }
    public bool CanDecline { get; set; } = true;
    public string? SourceBadge { get; set; }
    public string ReceivedLabel { get; set; } = "";
    public int FindingCount { get; set; }
    public string? FindingSummary { get; set; }
}

public class ProviderProLeadDetailsViewModel : ProviderProPageBaseViewModel
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public bool IsHighUrgency { get; set; }
    public string? DistanceLabel { get; set; }
    public string? TimelineNote { get; set; }
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public string CustomerName { get; set; } = "";
    public string CustomerInitials { get; set; } = "";
    public bool IsHomeownerVerified { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? ProblemDescription { get; set; }
    public List<string> PhotoUrls { get; set; } = [];
    public string? HomeType { get; set; }
    public string? SquareFeetLabel { get; set; }
    public string? YearBuiltLabel { get; set; }
    public string? StoriesLabel { get; set; }
    public string? AccessNotes { get; set; }
    public bool CanAccept { get; set; }
    public bool IsAccepted { get; set; }
    public bool CanScheduleVisit { get; set; }
    public bool CanCreateEstimate { get; set; }
    public bool CanDecline { get; set; }
    public string? InspectionReportUrl { get; set; }
    public string? SourceBadge { get; set; }
    public string? AnalysisSummary { get; set; }
    public int FindingCount { get; set; }
    public bool HasInspectionFindings { get; set; }
    public List<ProviderInspectionFindingItemViewModel> InspectionFindings { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProInspectionFindingsViewModel : ProviderProPageBaseViewModel
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string? InspectionReportUrl { get; set; }
    public string? AnalysisSummary { get; set; }
    public List<ProviderInspectionFindingItemViewModel> Findings { get; set; } = [];
    public int SelectedCount { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProSelectRepairItemsViewModel : ProviderProPageBaseViewModel
{
    public int LeadId { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public List<ProviderInspectionFindingItemViewModel> SelectedItems { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProEstimateAcceptedViewModel : ProviderProPageBaseViewModel
{
    public int EstimateId { get; set; }
    public int LeadId { get; set; }
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int ApprovedItemCount { get; set; }
    public string ApprovedLabel { get; set; } = "";
    public string ApprovedByLabel { get; set; } = "";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProCreateInvoiceViewModel : ProviderProPageBaseViewModel
{
    public int EstimateId { get; set; }
    public int LeadId { get; set; }
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public List<ProviderProEstimateLineItemViewModel> LineItems { get; set; } = [];
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentTerms { get; set; } = "Due at completion";
    public bool IncludeServiceCall { get; set; }
    public decimal ServiceCallAmount { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProReviewInvoiceViewModel : ProviderProPageBaseViewModel
{
    public int InvoiceId { get; set; }
    public int EstimateId { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string InvoiceDateLabel { get; set; } = "";
    public string DueDateLabel { get; set; } = "";
    public List<ProviderProEstimateLineItemViewModel> LineItems { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public string PaymentTerms { get; set; } = "";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProInvoiceSentViewModel : ProviderProPageBaseViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string Address { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string StatusLabel { get; set; } = "Sent";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderInspectionFindingItemViewModel
{
    public int Index { get; set; }
    public bool IsSelected { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string Priority { get; set; } = "";
    public string? SourceSection { get; set; }
    public string? SourceSectionNumber { get; set; }
    public int? SourcePage { get; set; }
    public string? SourceExcerpt { get; set; }
    public string? ReportReference { get; set; }
}

public class ProviderProScheduleVisitViewModel : ProviderProPageBaseViewModel
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string Urgency { get; set; } = "";
    public bool IsHighUrgency { get; set; }
    public string? DistanceLabel { get; set; }
    public string? TimelineNote { get; set; }
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public string CustomerName { get; set; } = "";
    public string CustomerInitials { get; set; } = "";
    public bool IsHomeownerVerified { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string PageTitle { get; set; } = "Schedule Visit";
    public string VisitType { get; set; } = "Estimate Visit";
    public bool IsVerificationVisit { get; set; }
    public string? InfoBanner { get; set; }
    public string ScheduleDateLabel { get; set; } = "";
    public string VisitDate { get; set; } = "";
    public string TimeLabel { get; set; } = "10:30 AM";
    public string AssignedTechnician { get; set; } = "";
    public string Priority { get; set; } = "High";
    public string Reminder { get; set; } = "Send 1 hour before";
    public string Notes { get; set; } = "";
    public bool NotifyHomeowner { get; set; } = true;
    public bool AddToCalendar { get; set; } = true;
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProScheduleVisitInput
{
    public int LeadId { get; set; }
    public string VisitType { get; set; } = "Estimate Visit";
    public string? VisitDate { get; set; }
    public string TimeLabel { get; set; } = "10:30 AM";
    public string AssignedTechnician { get; set; } = "";
    public string Priority { get; set; } = "High";
    public string Reminder { get; set; } = "Send 1 hour before";
    public string Notes { get; set; } = "";
    public bool NotifyHomeowner { get; set; } = true;
    public bool AddToCalendar { get; set; } = true;
    public bool SaveAsDraft { get; set; }
}

public class ProviderProEstimateItemViewModel
{
    public int Id { get; set; }
    public string EstimateId { get; set; } = "";
    public decimal Amount { get; set; }
    public string Address { get; set; } = "";
    public string? ServiceType { get; set; }
    public string Status { get; set; } = "Sent";
}

public class ProviderProApprovalItemViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string? ServiceType { get; set; }
    public string ImageUrl { get; set; } = "/welcome-house.png";
}

public class ProviderProPaymentsSummaryViewModel
{
    public decimal Paid { get; set; }
    public decimal Pending { get; set; }
    public decimal Overdue { get; set; }
}

public class ProviderProCalendarDayViewModel
{
    public string DayLabel { get; set; } = "";
    public string DateIso { get; set; } = "";
    public int JobCount { get; set; }
}
