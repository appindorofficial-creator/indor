using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IProviderProDataService
{
    Task<ProviderProWorkspaceData> GetWorkspaceDataAsync(int proveedorId, bool includeLeads, CancellationToken cancellationToken = default);
    Task<ProviderProJobsPageViewModel> GetJobsPageAsync(IndorProveedor proveedor, string? tab = "active", string? search = null, CancellationToken cancellationToken = default);
    Task<ProviderProCustomersPageViewModel> GetCustomersPageAsync(IndorProveedor proveedor, string? tab = "connected", string? search = null, CancellationToken cancellationToken = default);
    ProviderProAddCustomerInfoViewModel GetAddCustomerInfoViewModel(IndorProveedor proveedor, ProviderProAddCustomerDraft? draft);
    ProviderProAddCustomerPropertyViewModel GetAddCustomerPropertyViewModel(IndorProveedor proveedor, ProviderProAddCustomerDraft draft);
    ProviderProAddCustomerPreferencesViewModel GetAddCustomerPreferencesViewModel(IndorProveedor proveedor, ProviderProAddCustomerDraft draft);
    ProviderProAddCustomerReviewViewModel? GetAddCustomerReviewViewModel(IndorProveedor proveedor, ProviderProAddCustomerDraft draft);
    Task<int?> SaveAddCustomerFromDraftAsync(int proveedorId, ProviderProAddCustomerDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProAddCustomerSuccessViewModel?> GetAddCustomerSuccessAsync(IndorProveedor proveedor, int customerId, CancellationToken cancellationToken = default);
    Task<ProviderProReportsPageViewModel> GetReportsPageAsync(IndorProveedor proveedor, string? tab = "all", string? search = null, CancellationToken cancellationToken = default);
    Task<ProviderProProfilePageViewModel> GetProfilePageAsync(IndorProveedor proveedor, CancellationToken cancellationToken = default);
    Task<ProviderProEditProfileViewModel> GetEditProfileAsync(
        IndorProveedor proveedor,
        ProviderProEditProfileInput? input = null,
        CancellationToken cancellationToken = default);
    Task<bool> SaveEditProfileAsync(int proveedorId, ProviderProEditProfileInput input, CancellationToken cancellationToken = default);
    Task<ProviderProEditProfileServicesViewModel> GetEditProfileServicesAsync(IndorProveedor proveedor, CancellationToken cancellationToken = default);
    Task<bool> SaveEditProfileServicesAsync(int proveedorId, IReadOnlyList<string> selectedIds, CancellationToken cancellationToken = default);
    Task<ProviderProEditProfileVerificationViewModel> GetEditProfileVerificationAsync(IndorProveedor proveedor, CancellationToken cancellationToken = default);
    Task ApplyVerificationDocumentFlagsAsync(int proveedorId, string documentType, CancellationToken cancellationToken = default);
    Task<ProviderProNotificationsViewModel> GetNotificationsPageAsync(IndorProveedor proveedor, CancellationToken cancellationToken = default);
    Task SaveNotificationPreferencesAsync(int proveedorId, ProviderProNotificationsInput input, CancellationToken cancellationToken = default);
    Task<ProviderProNewLeadsPageViewModel> GetNewLeadsPageAsync(IndorProveedor proveedor, string? filter = "all", string? search = null, CancellationToken cancellationToken = default);
    Task<ProviderProLeadDetailsViewModel?> GetLeadDetailsAsync(IndorProveedor proveedor, int leadId, CancellationToken cancellationToken = default);
    Task<ProviderProInspectionFindingsViewModel?> GetInspectionFindingsAsync(IndorProveedor proveedor, int leadId, CancellationToken cancellationToken = default);
    Task SaveLeadFindingSelectionAsync(int leadId, IReadOnlyList<int> selectedIndices);
    Task<ProviderProSelectRepairItemsViewModel?> GetSelectRepairItemsAsync(IndorProveedor proveedor, int leadId, CancellationToken cancellationToken = default);
    Task<ProviderProScheduleVisitViewModel?> GetScheduleVisitAsync(IndorProveedor proveedor, int leadId, string? kind = null, CancellationToken cancellationToken = default);
    Task<int?> ConfirmScheduleVisitAsync(int proveedorId, ProviderProScheduleVisitInput input, CancellationToken cancellationToken = default);
    Task<ProviderProQuickEstimateViewModel?> GetQuickEstimateAsync(IndorProveedor proveedor, int leadId, CancellationToken cancellationToken = default);
    Task<int?> SaveQuickEstimateAsync(int proveedorId, ProviderProQuickEstimateInput input, CancellationToken cancellationToken = default);
    Task<ProviderProReviewEstimateViewModel?> GetReviewEstimateAsync(IndorProveedor proveedor, int estimateId, CancellationToken cancellationToken = default);
    Task<bool> SendEstimateAsync(int proveedorId, ProviderProSendEstimateInput input, CancellationToken cancellationToken = default);
    Task<ProviderProSendEstimatePageViewModel?> GetSendEstimatePageAsync(IndorProveedor proveedor, int estimateId, CancellationToken cancellationToken = default);
    Task<ProviderProPendingEstimatesPageViewModel> GetPendingEstimatesPageAsync(IndorProveedor proveedor, string? tab = "all", string? search = null, CancellationToken cancellationToken = default);
    Task<ProviderProQuickEstimateViewModel?> GetEditEstimateAsync(IndorProveedor proveedor, int estimateId, CancellationToken cancellationToken = default);
    Task<ProviderProCreateEstimateSetupViewModel> GetCreateEstimateSetupAsync(IndorProveedor proveedor, ProviderProCreateEstimateDraft? draft, CancellationToken cancellationToken = default);
    Task<ProviderProCreateEstimateDetailsViewModel?> GetCreateEstimateDetailsAsync(IndorProveedor proveedor, ProviderProCreateEstimateDraft draft);
    Task<ProviderProCreateEstimatePricingViewModel?> GetCreateEstimatePricingAsync(IndorProveedor proveedor, ProviderProCreateEstimateDraft draft);
    Task<ProviderProCreateEstimateReviewViewModel?> GetCreateEstimateReviewAsync(IndorProveedor proveedor, ProviderProCreateEstimateDraft draft);
    Task<int?> SaveCreateEstimateFromDraftAsync(int proveedorId, ProviderProCreateEstimateDraft draft, bool readyForReview, CancellationToken cancellationToken = default);
    Task ApplyCreateEstimateSourcePrefillAsync(int proveedorId, ProviderProCreateEstimateDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProEstimateSentViewModel?> GetEstimateSentAsync(IndorProveedor proveedor, int estimateId, CancellationToken cancellationToken = default);
    Task<ProviderProEstimateAcceptedViewModel?> GetEstimateAcceptedAsync(IndorProveedor proveedor, int estimateId, CancellationToken cancellationToken = default);
    Task<bool> ApproveEstimateAsync(int proveedorId, int estimateId, CancellationToken cancellationToken = default);
    Task<int?> ConvertEstimateToJobAsync(int proveedorId, int estimateId, CancellationToken cancellationToken = default);
    Task<ProviderProCreateInvoiceViewModel?> GetCreateInvoiceAsync(IndorProveedor proveedor, int estimateId, CancellationToken cancellationToken = default);
    Task<int?> SaveCreateInvoiceAsync(int proveedorId, ProviderProCreateInvoiceInput input, CancellationToken cancellationToken = default);
    Task<ProviderProReviewInvoiceViewModel?> GetReviewInvoiceAsync(IndorProveedor proveedor, int invoiceId, CancellationToken cancellationToken = default);
    Task<bool> SendInvoiceAsync(int proveedorId, int invoiceId, CancellationToken cancellationToken = default);
    Task<ProviderProInvoiceSentViewModel?> GetInvoiceSentAsync(IndorProveedor proveedor, int invoiceId, CancellationToken cancellationToken = default);
    Task<bool> AcceptLeadAsync(int proveedorId, int leadId, CancellationToken cancellationToken = default);
    Task<bool> DeclineLeadAsync(int proveedorId, int leadId, CancellationToken cancellationToken = default);
    Task<bool> ApproveHomeownerRequestAsync(int proveedorId, int approvalId, CancellationToken cancellationToken = default);
    Task<ProviderProInvoicesPageViewModel> GetInvoicesPageAsync(IndorProveedor proveedor, string? tab = "all", string? search = null, CancellationToken cancellationToken = default);
    Task<ProviderProInvoiceDetailsViewModel?> GetInvoiceDetailsAsync(IndorProveedor proveedor, int invoiceId, string? fromTab = null, CancellationToken cancellationToken = default);
    Task<ProviderProSendInvoiceReminderViewModel?> GetSendInvoiceReminderAsync(IndorProveedor proveedor, int invoiceId, string? fromTab = null, CancellationToken cancellationToken = default);
    Task<ProviderProRecordPaymentViewModel?> GetRecordPaymentAsync(IndorProveedor proveedor, int invoiceId, string? fromTab = null, CancellationToken cancellationToken = default);
    Task<bool> RecordInvoicePaymentAsync(int proveedorId, ProviderProRecordPaymentInput input, CancellationToken cancellationToken = default);
    Task<bool> SendInvoiceReminderAsync(int proveedorId, ProviderProSendInvoiceReminderInput input, CancellationToken cancellationToken = default);
    Task<bool> SendInvoiceReceiptAsync(int proveedorId, int invoiceId, CancellationToken cancellationToken = default);
    Task<ProviderProUploadReportSelectJobViewModel> GetUploadReportSelectJobAsync(IndorProveedor proveedor, ProviderProUploadReportDraft? draft, string? search = null, string? filter = "all", CancellationToken cancellationToken = default);
    Task<ProviderProUploadReportTypeViewModel?> GetUploadReportTypeAsync(IndorProveedor proveedor, ProviderProUploadReportDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProUploadReportFilesViewModel?> GetUploadReportFilesAsync(IndorProveedor proveedor, ProviderProUploadReportDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProUploadReportDetailsViewModel?> GetUploadReportDetailsAsync(IndorProveedor proveedor, ProviderProUploadReportDraft draft, CancellationToken cancellationToken = default);
    Task<int?> SaveUploadReportFromDraftAsync(int proveedorId, ProviderProUploadReportDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProUploadReportSuccessViewModel?> GetUploadReportSuccessAsync(IndorProveedor proveedor, int reportId, CancellationToken cancellationToken = default);
    Task<ProviderProUploadPhotosSelectJobViewModel> GetUploadPhotosSelectJobAsync(IndorProveedor proveedor, string? search = null, string? filter = "all", CancellationToken cancellationToken = default);
    Task<ProviderProUploadPhotosAddViewModel?> GetUploadPhotosAddAsync(IndorProveedor proveedor, ProviderProUploadPhotosDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProUploadPhotosReviewViewModel?> GetUploadPhotosReviewAsync(IndorProveedor proveedor, ProviderProUploadPhotosDraft draft, CancellationToken cancellationToken = default);
    Task<int?> SaveUploadPhotosFromDraftAsync(int proveedorId, ProviderProUploadPhotosDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProTemplatesPageViewModel> GetReportTemplatesAsync(int proveedorId, CancellationToken cancellationToken = default);
    Task<ReportTemplateView?> GetReportTemplateAsync(int proveedorId, string key, CancellationToken cancellationToken = default);
    Task<ProviderProUploadPhotosJobSummary?> GetExportJobSummaryAsync(IndorProveedor proveedor, int jobId, CancellationToken cancellationToken = default);
    Task<int?> SaveExportReportFromDraftAsync(int proveedorId, ProviderProExportReportDraft draft, bool send, CancellationToken cancellationToken = default);
    Task<ProviderProMessagesInboxViewModel> GetMessagesInboxAsync(IndorProveedor proveedor, string? tab = "all", string? search = null, CancellationToken cancellationToken = default);
    Task<ProviderProConversationViewModel?> GetConversationAsync(IndorProveedor proveedor, int conversationId, CancellationToken cancellationToken = default);
    Task<bool> SendConversationMessageAsync(int proveedorId, ProviderProSendMessageInput input, CancellationToken cancellationToken = default);
    Task<ProviderProMessageQuickActionsViewModel?> GetMessageQuickActionsAsync(IndorProveedor proveedor, int conversationId, string? selectedAction = null, CancellationToken cancellationToken = default);
    Task<bool> SendMessageQuickActionAsync(int proveedorId, ProviderProMessageActionDraft draft, CancellationToken cancellationToken = default);
    Task<ProviderProMessageSentSuccessViewModel?> GetMessageSentSuccessAsync(IndorProveedor proveedor, int conversationId, string actionLabel, CancellationToken cancellationToken = default);
    Task<int> GetUnreadMessageCountAsync(int proveedorId, CancellationToken cancellationToken = default);
    Task<int> SaveInsuranceQuoteAsync(int proveedorId, ProviderProInsuranceQuoteDraft draft, CancellationToken cancellationToken = default);
}

public class ProviderProWorkspaceData
{
    public List<ProviderProJobItemViewModel> TodaysJobs { get; set; } = [];
    public int NewLeadsCount { get; set; }
    public List<ProviderProLeadItemViewModel> NewLeads { get; set; } = [];
    public int PendingEstimatesCount { get; set; }
    public List<ProviderProEstimateItemViewModel> PendingEstimates { get; set; } = [];
    public List<ProviderProApprovalItemViewModel> PendingApprovals { get; set; } = [];
    public ProviderProPaymentsSummaryViewModel Payments { get; set; } = new();
    public List<ProviderProCalendarDayViewModel> UpcomingCalendar { get; set; } = [];
    public int HomeRecordsThisMonth { get; set; }
    public int HomeRecordsDelta { get; set; }
    public int HomesProtected { get; set; }
}
