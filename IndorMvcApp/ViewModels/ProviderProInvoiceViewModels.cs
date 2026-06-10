namespace IndorMvcApp.ViewModels;

public class ProviderProInvoiceLineItemViewModel
{
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string IconClass { get; set; } = "fa-wrench";
    public decimal Qty { get; set; } = 1;
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
}

public class ProviderProInvoicesPageViewModel : ProviderProPageBaseViewModel
{
    public string ActiveTab { get; set; } = "all";
    public string? SearchQuery { get; set; }
    public string PageTitle { get; set; } = "Payments & Invoices";
    public string SearchPlaceholder { get; set; } = "Search invoices";
    public decimal PaidThisMonth { get; set; }
    public decimal PendingTotal { get; set; }
    public decimal OverdueTotal { get; set; }
    public int OverdueCount { get; set; }
    public int PendingCount { get; set; }
    public bool ShowOverdueSummary { get; set; }
    public bool ShowPendingSummary { get; set; }
    public List<ProviderProInvoiceCardViewModel> Invoices { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProInvoiceCardViewModel
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public string StatusClass { get; set; } = "pending";
    public string StatusIcon { get; set; } = "fa-clock";
    public string? DueDateLabel { get; set; }
    public string? DaysLateLabel { get; set; }
    public bool ShowReminderAction { get; set; }
    public bool ShowMarkPaidAction { get; set; }
}

public class ProviderProInvoicePaymentRecordViewModel
{
    public string TimestampLabel { get; set; } = "";
    public string MethodLabel { get; set; } = "";
    public decimal Amount { get; set; }
}

public class ProviderProInvoiceDetailsViewModel : ProviderProPageBaseViewModel
{
    public int InvoiceId { get; set; }
    public int? JobId { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = "";
    public string StatusClass { get; set; } = "pending";
    public bool IsPaid { get; set; }
    public string PageTitle { get; set; } = "Invoice Details";
    public string AmountLabel { get; set; } = "Amount Due";
    public string DueDateLabel { get; set; } = "";
    public string InvoiceDateLabel { get; set; } = "";
    public string PaymentDateLabel { get; set; } = "";
    public string PaymentMethod { get; set; } = "Unpaid";
    public bool ReceiptAvailable { get; set; }
    public string NotesToCustomer { get; set; } = "";
    public string CustomerNotes { get; set; } = "";
    public string PaymentHistoryLabel { get; set; } = "No payments have been recorded for this invoice.";
    public List<ProviderProInvoicePaymentRecordViewModel> PaymentRecords { get; set; } = [];
    public string JobTitle { get; set; } = "";
    public string JobCode { get; set; } = "";
    public string JobStatus { get; set; } = "";
    public string JobCompletedLabel { get; set; } = "";
    public string TechnicianName { get; set; } = "";
    public string ServicePerformed { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string PropertyAddress { get; set; } = "";
    public string PropertyType { get; set; } = "";
    public string ReturnTab { get; set; } = "all";
    public int LineItemCount { get; set; }
    public List<ProviderProInvoiceLineItemViewModel> LineItems { get; set; } = [];
    public bool ShowReminderAction { get; set; }
    public bool ShowMarkPaidAction { get; set; }
    public bool ShowSendReceiptAction { get; set; }
    public bool ShowViewJobReport { get; set; }
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProSendInvoiceReminderViewModel : ProviderProPageBaseViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public decimal Amount { get; set; }
    public string DueDateLabel { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusClass { get; set; } = "pending";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string PropertyAddress { get; set; } = "";
    public string DefaultMessage { get; set; } = "";
    public string SendVia { get; set; } = "Email";
    public bool AttachInvoicePdf { get; set; } = true;
    public bool SendCopyToTeam { get; set; } = true;
    public string ReminderTiming { get; set; } = "Send now";
    public string ReturnTab { get; set; } = "all";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProSendInvoiceReminderInput
{
    public int InvoiceId { get; set; }
    public string SendVia { get; set; } = "Email";
    public string Message { get; set; } = "";
    public bool AttachInvoicePdf { get; set; } = true;
    public bool SendCopyToTeam { get; set; } = true;
    public string ReminderTiming { get; set; } = "Send now";
    public string? ReturnTab { get; set; }
}

public class ProviderProRecordPaymentViewModel : ProviderProPageBaseViewModel
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public decimal AmountDue { get; set; }
    public decimal PaymentAmount { get; set; }
    public string PaymentDate { get; set; } = "";
    public string PaymentMethod { get; set; } = "Cash";
    public string? PaymentReference { get; set; }
    public string? InternalNotes { get; set; }
    public bool NotifyHomeowner { get; set; } = true;
    public bool SendReceipt { get; set; } = true;
    public string ReturnTab { get; set; } = "pending";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProRecordPaymentInput
{
    public int InvoiceId { get; set; }
    public decimal PaymentAmount { get; set; }
    public string PaymentDate { get; set; } = "";
    public string PaymentMethod { get; set; } = "Cash";
    public string? PaymentReference { get; set; }
    public string? InternalNotes { get; set; }
    public bool NotifyHomeowner { get; set; } = true;
    public bool SendReceipt { get; set; } = true;
    public string? ReturnTab { get; set; }
}
