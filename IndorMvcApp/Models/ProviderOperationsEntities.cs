using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorProveedorClientes")]
public class IndorProveedorCliente
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [MaxLength(30)]
    public string? CustomerCode { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? CustomerType { get; set; }

    [MaxLength(60)]
    public string? FirstName { get; set; }

    [MaxLength(60)]
    public string? LastName { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? PreferredContactMethod { get; set; }

    [MaxLength(120)]
    public string? CompanyName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(120)]
    public string? CityState { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(200)]
    public string? StreetAddress { get; set; }

    [MaxLength(40)]
    public string? AptUnit { get; set; }

    [MaxLength(80)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? State { get; set; }

    [MaxLength(15)]
    public string? ZipCode { get; set; }

    [MaxLength(30)]
    public string? PropertyType { get; set; }

    public int? Bedrooms { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? Bathrooms { get; set; }

    public bool IsBillingAddressSame { get; set; } = true;

    [MaxLength(500)]
    public string? PropertyPhotoUrl { get; set; }

    [MaxLength(250)]
    public string? AccessNotes { get; set; }

    [MaxLength(20)]
    public string? EstimateDeliveryPref { get; set; }

    [MaxLength(20)]
    public string? InvoiceDeliveryPref { get; set; }

    [MaxLength(20)]
    public string? PreferredLanguage { get; set; }

    [MaxLength(30)]
    public string? CustomerSource { get; set; }

    [MaxLength(500)]
    public string? TagsJson { get; set; }

    [MaxLength(500)]
    public string? InternalNotes { get; set; }

    public bool SendIndorInvite { get; set; } = true;

    public bool AllowServiceUpdates { get; set; } = true;

    [MaxLength(20)]
    public string ConnectionStatus { get; set; } = ProviderCustomerConnectionStatuses.Connected;

    public bool IsPropertyVerified { get; set; }

    public bool IsAppConnected { get; set; }

    public int PropertiesCount { get; set; } = 1;

    public int HouseFactsCount { get; set; }

    [MaxLength(200)]
    public string? LastActivityNote { get; set; }

    public DateTime MemberSince { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("IndorProveedorJobs")]
public class IndorProveedorJob
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    public int? ClienteId { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public IndorProveedorCliente? Cliente { get; set; }

    public int? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public IndorProveedorLead? Lead { get; set; }

    [Required, MaxLength(40)]
    public string JobCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Scheduled";

    [MaxLength(80)]
    public string? ServiceType { get; set; }

    [MaxLength(40)]
    public string? ChecklistStatus { get; set; }

    public int PhotosCount { get; set; }

    [MaxLength(60)]
    public string? HouseFactsStatus { get; set; }

    public bool ViewedByCustomer { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? EstimateAmount { get; set; }

    [MaxLength(30)]
    public string? EstimateCode { get; set; }

    [MaxLength(20)]
    public string? InvoiceStatus { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? PaymentAmount { get; set; }

    [MaxLength(20)]
    public string? PaymentStatus { get; set; }

    [Column(TypeName = "decimal(5,1)")]
    public decimal? DistanceMiles { get; set; }

    [MaxLength(2000)]
    public string? ScopeOfWork { get; set; }

    [MaxLength(1000)]
    public string? MaterialsNeeded { get; set; }

    [MaxLength(500)]
    public string? AccessInstructions { get; set; }

    [MaxLength(2000)]
    public string? JobNotes { get; set; }

    [MaxLength(120)]
    public string? AssignedTechnician { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public string? ChecklistJson { get; set; }

    [MaxLength(2000)]
    public string? MaterialsUsedJson { get; set; }

    [MaxLength(1000)]
    public string? PhotoUrlsJson { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(120)]
    public string? HomeownerSignature { get; set; }

    public DateTime? HomeownerSignedAt { get; set; }

    [MaxLength(30)]
    public string? ReportCode { get; set; }

    [MaxLength(2000)]
    public string? WorkPerformed { get; set; }

    [MaxLength(200)]
    public string? LaborWarranty { get; set; }

    [MaxLength(2000)]
    public string? FinalNotes { get; set; }

    public bool IsDraft { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? ScheduledEndAt { get; set; }

    [MaxLength(60)]
    public string? ReminderSetting { get; set; }

    public bool AddToCalendar { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

[Table("IndorProveedorLeads")]
public class IndorProveedorLead
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string ServiceType { get; set; } = string.Empty;

    [MaxLength(40)]
    public string Urgency { get; set; } = "Standard";

    [Required, MaxLength(20)]
    public string Status { get; set; } = "New";

    [MaxLength(120)]
    public string? CustomerName { get; set; }

    [MaxLength(256)]
    public string? CustomerEmail { get; set; }

    [MaxLength(30)]
    public string? CustomerPhone { get; set; }

    [MaxLength(20)]
    public string LeadCode { get; set; } = "";

    public bool IsHomeownerVerified { get; set; }

    [MaxLength(2000)]
    public string? ProblemDescription { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(1000)]
    public string? PhotosJson { get; set; }

    [Column(TypeName = "decimal(5,1)")]
    public decimal? DistanceMiles { get; set; }

    [MaxLength(120)]
    public string? TimelineNote { get; set; }

    [MaxLength(40)]
    public string? HomeType { get; set; }

    public int? SquareFeet { get; set; }

    public int? YearBuilt { get; set; }

    public int? Stories { get; set; }

    [MaxLength(500)]
    public string? AccessNotes { get; set; }

    public string? SuggestedScopeItemsJson { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? SuggestedLaborAmount { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? SuggestedMaterialsAmount { get; set; }

    [MaxLength(80)]
    public string? SuggestedTimeline { get; set; }

    [MaxLength(200)]
    public string? SuggestedWarranty { get; set; }

    [MaxLength(2000)]
    public string? SuggestedHomeownerNotes { get; set; }

    [MaxLength(40)]
    public string? DefaultVisitType { get; set; }

    [MaxLength(120)]
    public string? DefaultAssignedTechnician { get; set; }

    [MaxLength(2000)]
    public string? DefaultVisitNotes { get; set; }

    public DateTime? DefaultVisitAt { get; set; }

    [MaxLength(20)]
    public string? DefaultVisitTimeLabel { get; set; }

    public string? DefaultChecklistJson { get; set; }

    [MaxLength(2000)]
    public string? DefaultMaterialsUsedJson { get; set; }

    [MaxLength(200)]
    public string? DefaultLaborWarranty { get; set; }

    public int? RealtorQuoteId { get; set; }

    [MaxLength(40)]
    public string? LeadSource { get; set; }

    [MaxLength(500)]
    public string? InspectionReportUrl { get; set; }

    public string? FindingsJson { get; set; }

    [MaxLength(2000)]
    public string? AnalysisSummary { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("IndorProveedorEstimates")]
public class IndorProveedorEstimate
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    public int? JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public IndorProveedorJob? Job { get; set; }

    public int? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public IndorProveedorLead? Lead { get; set; }

    public int? ClienteId { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public IndorProveedorCliente? Cliente { get; set; }

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(20)]
    public string? EstimateType { get; set; }

    [MaxLength(40)]
    public string? ServiceCategoryId { get; set; }

    [MaxLength(20)]
    public string? DeliveryMethod { get; set; }

    public DateTime? EstimatedEndDate { get; set; }

    [Required, MaxLength(30)]
    public string EstimateCode { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? ServiceType { get; set; }

    [MaxLength(120)]
    public string? CustomerName { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal LaborAmount { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal MaterialsAmount { get; set; }

    public string? ScopeItemsJson { get; set; }

    [MaxLength(80)]
    public string? Timeline { get; set; }

    [MaxLength(200)]
    public string? Warranty { get; set; }

    [MaxLength(2000)]
    public string? HomeownerNotes { get; set; }

    public bool NotifyHomeowner { get; set; } = true;

    public bool SaveCopyToLeads { get; set; } = true;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Sent";

    [Column(TypeName = "decimal(12,2)")]
    public decimal? SubtotalAmount { get; set; }

    [Column(TypeName = "decimal(5,4)")]
    public decimal? TaxRate { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? DiscountAmount { get; set; }

    public DateTime? EstimatedStartDate { get; set; }

    [MaxLength(80)]
    public string? EstimatedDuration { get; set; }

    [MaxLength(120)]
    public string? LaborWarranty { get; set; }

    [MaxLength(120)]
    public string? PartsWarranty { get; set; }

    public int ValidDays { get; set; } = 30;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime? SentUtc { get; set; }

    public DateTime? ViewedUtc { get; set; }

    public DateTime? ApprovedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

public static class ProviderEstimateStatuses
{
    public const string Draft = "Draft";
    public const string Ready = "Ready";
    public const string Sent = "Sent";
    public const string Viewed = "Viewed";
    public const string Approved = "Approved";
    public const string Declined = "Declined";
}

[Table("IndorProveedorInvoices")]
public class IndorProveedorInvoice
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    public int? JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public IndorProveedorJob? Job { get; set; }

    public int? EstimateId { get; set; }

    [ForeignKey(nameof(EstimateId))]
    public IndorProveedorEstimate? Estimate { get; set; }

    public int? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public IndorProveedorLead? Lead { get; set; }

    public DateTime? SentUtc { get; set; }

    [MaxLength(20)]
    public string? InvoiceCode { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? ServiceType { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(256)]
    public string? CustomerEmail { get; set; }

    [MaxLength(40)]
    public string? CustomerPhone { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime? DueDate { get; set; }

    public DateTime? PaidDate { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    [MaxLength(40)]
    public string? PaymentMethod { get; set; }

    public string? NotesToCustomer { get; set; }

    public string? CustomerNotes { get; set; }

    [MaxLength(40)]
    public string? PropertyType { get; set; }

    public string? LineItemsJson { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? PaidAmount { get; set; }

    [MaxLength(80)]
    public string? PaymentReference { get; set; }

    [MaxLength(500)]
    public string? InternalNotes { get; set; }

    public DateTime? LastReminderUtc { get; set; }

    [MaxLength(20)]
    public string? LastReminderChannel { get; set; }

    public string? LastReminderMessage { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

[Table("IndorProveedorReports")]
public class IndorProveedorReport
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    public int? JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public IndorProveedorJob? Job { get; set; }

    public int? ClienteId { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public IndorProveedorCliente? Cliente { get; set; }

    [Required, MaxLength(30)]
    public string ReportCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? CustomerName { get; set; }

    [MaxLength(80)]
    public string? ServiceType { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = ProviderReportStatuses.Draft;

    public int PhotosCount { get; set; }

    public bool HasChecklist { get; set; }

    public bool HasWarranty { get; set; }

    public bool HasDocuments { get; set; }

    public bool AddedToHouseFacts { get; set; }

    [MaxLength(40)]
    public string? ReportType { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    [MaxLength(1000)]
    public string? WorkCompleted { get; set; }

    [MaxLength(1000)]
    public string? MaterialsUsed { get; set; }

    [MaxLength(500)]
    public string? WarrantyInfo { get; set; }

    [MaxLength(500)]
    public string? Recommendations { get; set; }

    [MaxLength(500)]
    public string? InternalNotes { get; set; }

    public bool SendToHomeowner { get; set; } = true;

    public bool RequestApproval { get; set; }

    public bool AttachToHouseFacts { get; set; } = true;

    [MaxLength(2000)]
    public string? PhotoUrlsJson { get; set; }

    [MaxLength(2000)]
    public string? DocumentsJson { get; set; }

    public int FilesCount { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

public static class ProviderReportTypes
{
    public const string Completion = "Completion Report";
    public const string Photo = "Photo Report";
    public const string Assessment = "Assessment / Inspection";
    public const string Warranty = "Warranty / Materials";
    public const string BeforeAfter = "Before & After";
}

[Table("IndorProveedorConversations")]
public class IndorProveedorConversation
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    public int? ClienteId { get; set; }

    [ForeignKey(nameof(ClienteId))]
    public IndorProveedorCliente? Cliente { get; set; }

    public int? JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public IndorProveedorJob? Job { get; set; }

    public int? LeadId { get; set; }

    [ForeignKey(nameof(LeadId))]
    public IndorProveedorLead? Lead { get; set; }

    [Required, MaxLength(20)]
    public string Category { get; set; } = ProviderConversationCategories.Job;

    [Required, MaxLength(20)]
    public string Status { get; set; } = ProviderConversationStatuses.New;

    public int UnreadCount { get; set; }

    [MaxLength(250)]
    public string? LastMessagePreview { get; set; }

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public bool IsCustomerOnline { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("IndorProveedorMessages")]
public class IndorProveedorMessage
{
    public int Id { get; set; }

    public int ConversationId { get; set; }

    [ForeignKey(nameof(ConversationId))]
    public IndorProveedorConversation? Conversation { get; set; }

    [Required, MaxLength(20)]
    public string SenderType { get; set; } = ProviderMessageSenderTypes.Customer;

    [Required, MaxLength(2000)]
    public string Body { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; }

    [MaxLength(40)]
    public string? AttachmentType { get; set; }

    [MaxLength(120)]
    public string? AttachmentLabel { get; set; }
}

public static class ProviderConversationCategories
{
    public const string Job = "Job";
    public const string Lead = "Lead";
}

public static class ProviderConversationStatuses
{
    public const string New = "New";
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
}

public static class ProviderMessageSenderTypes
{
    public const string Provider = "Provider";
    public const string Customer = "Customer";
}

public static class ProviderMessageActionTypes
{
    public const string Estimate = "estimate";
    public const string Invoice = "invoice";
    public const string Visit = "visit";
    public const string Report = "report";
    public const string Approval = "approval";
}

[Table("IndorProveedorApprovals")]
public class IndorProveedorApproval
{
    public int Id { get; set; }

    public int ProveedorId { get; set; }

    [ForeignKey(nameof(ProveedorId))]
    public IndorProveedor? Proveedor { get; set; }

    public int? JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public IndorProveedorJob? Job { get; set; }

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public static class ProviderJobStatuses
{
    public const string InProgress = "InProgress";
    public const string Scheduled = "Scheduled";
    public const string Confirmed = "Confirmed";
    public const string Completed = "Completed";
    public const string WaitingOnMaterials = "WaitingOnMaterials";
}

public static class ProviderInvoiceStatuses
{
    public const string Paid = "Paid";
    public const string Pending = "Pending";
    public const string Overdue = "Overdue";
}

public static class ProviderLeadStatuses
{
    public const string New = "New";
    public const string Accepted = "Accepted";
    public const string Declined = "Declined";
}

public static class ProviderCustomerConnectionStatuses
{
    public const string Connected = "Connected";
    public const string NeedsInvite = "NeedsInvite";
}

public static class ProviderReportStatuses
{
    public const string Draft = "Draft";
    public const string Ready = "Ready";
    public const string Approval = "Approval";
    public const string Approved = "Approved";
    public const string HouseFacts = "HouseFacts";
}
