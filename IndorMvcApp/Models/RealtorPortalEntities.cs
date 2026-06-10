using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("IndorRealtors")]
public class IndorRealtor
{
    public int Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    public Guid RegistrationToken { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string RegistrationStatus { get; set; } = RealtorRegistrationStatuses.Draft;

    public int CurrentStep { get; set; } = 1;

    [MaxLength(120)]
    public string? DisplayName { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? BrokerageName { get; set; }

    [MaxLength(80)]
    public string? LicenseNumber { get; set; }

    [MaxLength(10)]
    public string? LicenseState { get; set; }

    [MaxLength(500)]
    public string? ServiceAreas { get; set; }

    public bool ProfessionalTermsAccepted { get; set; }

    public DateTime? TermsAcceptedUtc { get; set; }

    public bool VerificationSkipped { get; set; }

    public DateTime? ProfileCompletedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    [MaxLength(500)]
    public string? ProfilePhotoUrl { get; set; }

    public ICollection<IndorRealtorDocumento> Documentos { get; set; } = [];

    public ICollection<IndorRealtorPropertyFile> PropertyFiles { get; set; } = [];

    public ICollection<IndorRealtorQuote> Quotes { get; set; } = [];

    public ICollection<IndorRealtorSharedPackage> SharedPackages { get; set; } = [];

    public ICollection<IndorRealtorClient> Clients { get; set; } = [];

    public ICollection<IndorRealtorInvitation> Invitations { get; set; } = [];

    public ICollection<IndorRealtorActivity> Activities { get; set; } = [];
}

public static class RealtorRegistrationStatuses
{
    public const string Draft = "Draft";
    public const string Basic = "Basic";
    public const string Verified = "Verified";
}

public static class RealtorDocumentTypes
{
    public const string LicensePhoto = "license_photo";
    public const string GovernmentId = "government_id";
    public const string BusinessCard = "business_card";

    public static IReadOnlyList<(string Type, string Label, bool Required)> Slots =>
    [
        (LicensePhoto, "License photo", true),
        (GovernmentId, "Government ID", true),
        (BusinessCard, "Business card", false)
    ];
}

[Table("IndorRealtorDocumentos")]
public class IndorRealtorDocumento
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [Required, MaxLength(40)]
    public string DocumentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    public DateTime? UploadedUtc { get; set; }
}

[Table("IndorRealtorPropertyFiles")]
public class IndorRealtorPropertyFile
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? CityRegion { get; set; }

    public int? Beds { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? Baths { get; set; }

    public int? SqFt { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";

    [MaxLength(30)]
    public string? FilePhase { get; set; }

    [MaxLength(120)]
    public string? ClientName { get; set; }

    public int RepairItemsCount { get; set; }

    public int QuotesReceivedCount { get; set; }

    public DateTime? UpdatedUtc { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<IndorRealtorPropertyFileItem> Items { get; set; } = [];
}

public static class RealtorPropertyFilePhases
{
    public const string PreClosing = "Pre-Closing";
    public const string RepairReview = "Repair Review";
    public const string Transfer = "Transfer";
    public const string General = "General Property File";

    public static IReadOnlyList<(string Value, string Label, string Icon)> Options =>
    [
        (PreClosing, "Pre-Closing File", "fa-house-circle-check"),
        (RepairReview, "Repair Review File", "fa-wrench"),
        (Transfer, "Transfer File", "fa-right-left"),
        (General, "General Property File", "fa-folder")
    ];
}

public static class RealtorPropertyFileCategoryTypes
{
    public const string PhotosVideos = "photos_videos";
    public const string InspectionReports = "inspection_reports";
    public const string RepairItems = "repair_items";
    public const string QuotesEstimates = "quotes_estimates";
    public const string Warranties = "warranties";
    public const string InvoicesReceipts = "invoices_receipts";
    public const string ManualsSerialNumbers = "manuals_serial_numbers";
    public const string NotesDocuments = "notes_documents";

    public static IReadOnlyList<(string Type, string Label, string Description, string Icon)> All =>
    [
        (PhotosVideos, "Photos & Videos", "Before, during, and after photos", "fa-camera"),
        (InspectionReports, "Inspection Reports", "Upload inspection or review documents", "fa-file-circle-check"),
        (RepairItems, "Repair Items", "Track issues and needed repairs", "fa-wrench"),
        (QuotesEstimates, "Quotes & Estimates", "Attach contractor estimates", "fa-file-invoice-dollar"),
        (Warranties, "Warranties", "Store warranty documents and dates", "fa-shield-halved"),
        (InvoicesReceipts, "Invoices & Receipts", "Save bills and payment records", "fa-receipt"),
        (ManualsSerialNumbers, "Manuals & Serial Numbers", "Appliance and system details", "fa-book"),
        (NotesDocuments, "Notes & Documents", "Add notes, PDFs, and related files", "fa-note-sticky")
    ];
}

public static class RealtorPropertyFileDraftStatuses
{
    public const string Draft = "Draft";
    public const string Created = "Created";
}

[Table("IndorRealtorPropertyFileDrafts")]
public class IndorRealtorPropertyFileDraft
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    public int CurrentStep { get; set; } = 1;

    [Required, MaxLength(20)]
    public string Status { get; set; } = RealtorPropertyFileDraftStatuses.Draft;

    public int? SourcePropertyId { get; set; }

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? CityRegion { get; set; }

    [MaxLength(120)]
    public string? ClientName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(40)]
    public string? FilePhase { get; set; }

    [MaxLength(1000)]
    public string? NoteText { get; set; }

    public bool CreateAndContinueLater { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<IndorRealtorPropertyFileDraftCategory> Categories { get; set; } = [];

    public ICollection<IndorRealtorPropertyFileDraftItem> Items { get; set; } = [];
}

[Table("IndorRealtorPropertyFileDraftCategories")]
public class IndorRealtorPropertyFileDraftCategory
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    [ForeignKey(nameof(DraftId))]
    public IndorRealtorPropertyFileDraft? Draft { get; set; }

    [Required, MaxLength(40)]
    public string CategoryType { get; set; } = string.Empty;
}

[Table("IndorRealtorPropertyFileDraftItems")]
public class IndorRealtorPropertyFileDraftItem
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    [ForeignKey(nameof(DraftId))]
    public IndorRealtorPropertyFileDraft? Draft { get; set; }

    [Required, MaxLength(40)]
    public string CategoryType { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ItemLabel { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(1000)]
    public string? NoteText { get; set; }

    public long? FileSizeBytes { get; set; }

    public DateTime? ExpirationUtc { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorRealtorPropertyFileItems")]
public class IndorRealtorPropertyFileItem
{
    public int Id { get; set; }

    public int PropertyFileId { get; set; }

    [ForeignKey(nameof(PropertyFileId))]
    public IndorRealtorPropertyFile? PropertyFile { get; set; }

    [Required, MaxLength(40)]
    public string CategoryType { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ItemLabel { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? FileUrl { get; set; }

    [MaxLength(1000)]
    public string? NoteText { get; set; }

    public long? FileSizeBytes { get; set; }

    public DateTime? ExpirationUtc { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorRealtorQuotes")]
public class IndorRealtorQuote
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [Required, MaxLength(30)]
    public string QuoteCode { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string ServiceType { get; set; } = string.Empty;

    public DateTime RequestedUtc { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(120)]
    public string? ClientName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public int ProviderQuotesReceived { get; set; }

    [MaxLength(120)]
    public string? FooterNote { get; set; }

    public DateTime? UpdatedUtc { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? Amount { get; set; }

    public int? PropertyFileId { get; set; }

    [MaxLength(30)]
    public string? RequestType { get; set; }

    public int? ResponseDeadlineHours { get; set; }

    [MaxLength(30)]
    public string? ProviderSelectionMode { get; set; }

    [MaxLength(500)]
    public string? OptionalMessage { get; set; }

    public DateTime? SentUtc { get; set; }

    public ICollection<IndorRealtorQuoteBid> Bids { get; set; } = [];

    public ICollection<IndorRealtorQuoteSentProvider> SentProviders { get; set; } = [];
}

public static class RealtorQuoteRequestDraftStatuses
{
    public const string Draft = "Draft";
    public const string Sent = "Sent";
}

public static class RealtorQuoteRequestTypes
{
    public const string EntireFile = "EntireFile";
    public const string ByItem = "ByItem";

    public static IReadOnlyList<(string Value, string Label, string Description, string Icon)> Options =>
    [
        (EntireFile, "Send Entire File", "Best when you want one quote for the full project.", "fa-file-lines"),
        (ByItem, "Request by Item", "Best when you want separate quotes for each repair item.", "fa-list-check")
    ];
}

public static class RealtorQuoteProviderSelectionModes
{
    public const string Manual = "Manual";
    public const string IndorRecommended = "IndorRecommended";

    public static IReadOnlyList<(string Value, string Label, string Description)> Options =>
    [
        (Manual, "I want to choose companies", "Search and select who receives the request."),
        (IndorRecommended, "Let INDOR choose for me", "INDOR matches recommended providers based on the repairs.")
    ];
}

public static class RealtorQuotePriorities
{
    public const string FastResponse = "FastResponse";
    public const string Price = "Price";
    public const string TopRated = "TopRated";

    public static IReadOnlyList<(string Value, string Label)> Options =>
    [
        (FastResponse, "Fast response"),
        (Price, "Price"),
        (TopRated, "Top rated")
    ];
}

public static class RealtorQuoteServiceTypes
{
    public static IReadOnlyList<string> All =>
    [
        "HVAC Repair",
        "Roofing",
        "Plumbing",
        "Electrical",
        "Home Inspection"
    ];
}

[Table("IndorRealtorQuoteProviders")]
public class IndorRealtorQuoteProvider
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [Required, MaxLength(200)]
    public string Categories { get; set; } = string.Empty;

    [Column(TypeName = "decimal(2,1)")]
    public decimal Rating { get; set; } = 4.5m;

    [Column(TypeName = "decimal(4,1)")]
    public decimal DistanceMiles { get; set; } = 5.0m;

    [MaxLength(40)]
    public string? BadgeLabel { get; set; }

    public bool IsVerified { get; set; } = true;

    public bool IsRecommended { get; set; }

    public bool IsActive { get; set; } = true;
}

[Table("IndorRealtorQuoteRequestDrafts")]
public class IndorRealtorQuoteRequestDraft
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    public int CurrentStep { get; set; } = 1;

    [Required, MaxLength(20)]
    public string Status { get; set; } = RealtorQuoteRequestDraftStatuses.Draft;

    public int? PropertyFileId { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? CityRegion { get; set; }

    [MaxLength(120)]
    public string? ClientName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(40)]
    public string? FilePhase { get; set; }

    [MaxLength(120)]
    public string? ServiceType { get; set; }

    [Required, MaxLength(30)]
    public string RequestType { get; set; } = RealtorQuoteRequestTypes.EntireFile;

    public bool SharePhotosVideos { get; set; } = true;

    public bool ShareInspectionReport { get; set; } = true;

    public bool ShareRepairItems { get; set; } = true;

    public bool ShareNotes { get; set; } = true;

    public int ResponseDeadlineHours { get; set; } = 48;

    [Required, MaxLength(30)]
    public string ProviderSelectionMode { get; set; } = RealtorQuoteProviderSelectionModes.Manual;

    public int ProviderCountTarget { get; set; } = 3;

    public bool VerifiedOnly { get; set; } = true;

    [Required, MaxLength(30)]
    public string Priority { get; set; } = RealtorQuotePriorities.FastResponse;

    public int CoverageMiles { get; set; } = 10;

    public bool SendNow { get; set; } = true;

    public DateTime? ScheduledSendUtc { get; set; }

    public bool AllowProviderQuestions { get; set; } = true;

    public bool AllowFullProjectQuote { get; set; } = true;

    public bool AllowItemizedQuote { get; set; } = true;

    [MaxLength(500)]
    public string? OptionalMessage { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<IndorRealtorQuoteRequestDraftProvider> SelectedProviders { get; set; } = [];
}

[Table("IndorRealtorQuoteRequestDraftProviders")]
public class IndorRealtorQuoteRequestDraftProvider
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    [ForeignKey(nameof(DraftId))]
    public IndorRealtorQuoteRequestDraft? Draft { get; set; }

    public int ProviderId { get; set; }

    [ForeignKey(nameof(ProviderId))]
    public IndorRealtorQuoteProvider? Provider { get; set; }
}

[Table("IndorRealtorQuoteSentProviders")]
public class IndorRealtorQuoteSentProvider
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    [ForeignKey(nameof(QuoteId))]
    public IndorRealtorQuote? Quote { get; set; }

    public int ProviderId { get; set; }

    [Required, MaxLength(120)]
    public string ProviderName { get; set; } = string.Empty;
}

public static class RealtorInspectionUploadDraftStatuses
{
    public const string Draft = "Draft";
    public const string Completed = "Completed";
}

public static class RealtorInspectionAnalysisStatuses
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Complete = "Complete";
}

public static class RealtorInspectionFindingPriorities
{
    public const string Urgent = "Urgent";
    public const string High = "High";
    public const string Moderate = "Moderate";

    public static IReadOnlyList<string> All => [Urgent, High, Moderate];
}

public static class RealtorInspectionTrades
{
    public const string Electrical = "Electrical";
    public const string Hvac = "HVAC";
    public const string Plumbing = "Plumbing";
    public const string Roof = "Roof";
    public const string Paint = "Paint";

    public static IReadOnlyList<(string Value, string Label, string Icon, string Css)> All =>
    [
        (Electrical, "Electrician", "fa-bolt", "electrical"),
        (Hvac, "HVAC Technician", "fa-fan", "hvac"),
        (Plumbing, "Plumber", "fa-faucet-drip", "plumbing"),
        (Roof, "Roofer", "fa-house-chimney", "roof"),
        (Paint, "Painter", "fa-paint-roller", "paint")
    ];
}

[Table("IndorRealtorInspectionUploadDrafts")]
public class IndorRealtorInspectionUploadDraft
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    public int CurrentStep { get; set; } = 1;

    [Required, MaxLength(20)]
    public string Status { get; set; } = RealtorInspectionUploadDraftStatuses.Draft;

    public int? PropertyFileId { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? CityRegion { get; set; }

    [MaxLength(120)]
    public string? ClientName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(500)]
    public string? ReportFileUrl { get; set; }

    [MaxLength(200)]
    public string? ReportFileName { get; set; }

    public int ReportPageCount { get; set; }

    [Required, MaxLength(20)]
    public string UploadMethod { get; set; } = "Pdf";

    public int AnalysisProgress { get; set; }

    [Required, MaxLength(20)]
    public string AnalysisStatus { get; set; } = RealtorInspectionAnalysisStatuses.Pending;

    public int ResponseDeadlineHours { get; set; } = 48;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<IndorRealtorInspectionUploadFinding> Findings { get; set; } = [];

    public ICollection<IndorRealtorInspectionDraftProvider> TradeProviders { get; set; } = [];
}

[Table("IndorRealtorInspectionUploadFindings")]
public class IndorRealtorInspectionUploadFinding
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    [ForeignKey(nameof(DraftId))]
    public IndorRealtorInspectionUploadDraft? Draft { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Priority { get; set; } = RealtorInspectionFindingPriorities.Moderate;

    [Required, MaxLength(40)]
    public string Trade { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string TradeLabel { get; set; } = string.Empty;

    public int AiScore { get; set; } = 80;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }

    public bool IsSelected { get; set; } = true;
}

[Table("IndorRealtorInspectionDraftProviders")]
public class IndorRealtorInspectionDraftProvider
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    [ForeignKey(nameof(DraftId))]
    public IndorRealtorInspectionUploadDraft? Draft { get; set; }

    [Required, MaxLength(40)]
    public string Trade { get; set; } = string.Empty;

    public int ProviderId { get; set; }
}

public static class RealtorUrgentQuoteDraftStatuses
{
    public const string Draft = "Draft";
    public const string Sent = "Sent";
}

public static class RealtorUrgentQuoteCategories
{
    public const string NeedQuoteToday = "NeedQuoteToday";
    public const string ClosingRepair = "ClosingRepair";
    public const string UrgentIssue = "UrgentIssue";

    public static IReadOnlyList<(string Value, string Label, string Icon)> Options =>
    [
        (NeedQuoteToday, "Need quote today", "fa-bolt"),
        (ClosingRepair, "Closing repair", "fa-wrench"),
        (UrgentIssue, "Urgent issue", "fa-triangle-exclamation")
    ];
}

public static class RealtorUrgentQuoteServiceTypes
{
    public static IReadOnlyList<string> All => ["HVAC", "Plumbing", "Electrical", "Roof", "Other"];
}

public static class RealtorUrgentQuoteUrgencyLevels
{
    public const string Today = "Today";
    public const string ThisWeek = "ThisWeek";
    public const string Emergency = "Emergency";

    public static IReadOnlyList<(string Value, string Label, string Icon)> Options =>
    [
        (Today, "Today", "fa-clock"),
        (ThisWeek, "This week", "fa-calendar"),
        (Emergency, "Emergency", "fa-bell")
    ];
}

public static class RealtorUrgentQuoteRequestTags
{
    public const string NeedQuote = "NeedQuote";
    public const string NeedAvailability = "NeedAvailability";
    public const string NeedSiteVisit = "NeedSiteVisit";

    public static IReadOnlyList<(string Value, string Label, string Icon)> Options =>
    [
        (NeedQuote, "Need quote", "fa-tag"),
        (NeedAvailability, "Need availability", "fa-calendar-check"),
        (NeedSiteVisit, "Need site visit", "fa-user-check")
    ];
}

public static class RealtorUrgentQuoteProviderModes
{
    public const string IndorAuto = "IndorAuto";
    public const string Manual = "Manual";
}

public static class RealtorUrgentQuoteSendPayloads
{
    public const string IssueOnly = "IssueOnly";
    public const string FullPropertyFile = "FullPropertyFile";
}

[Table("IndorRealtorUrgentQuoteDrafts")]
public class IndorRealtorUrgentQuoteDraft
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    public int CurrentStep { get; set; } = 1;

    [Required, MaxLength(20)]
    public string Status { get; set; } = RealtorUrgentQuoteDraftStatuses.Draft;

    public int? PropertyFileId { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? CityRegion { get; set; }

    [MaxLength(120)]
    public string? ClientName { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    public int? Beds { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal? Baths { get; set; }

    public int? SqFt { get; set; }

    [Required, MaxLength(30)]
    public string RequestCategory { get; set; } = RealtorUrgentQuoteCategories.NeedQuoteToday;

    [Required, MaxLength(40)]
    public string ServiceType { get; set; } = "HVAC";

    [Required, MaxLength(20)]
    public string UrgencyLevel { get; set; } = RealtorUrgentQuoteUrgencyLevels.Today;

    [MaxLength(200)]
    public string? QuickDescription { get; set; }

    [Required, MaxLength(30)]
    public string RequestTypeTag { get; set; } = RealtorUrgentQuoteRequestTags.NeedQuote;

    [MaxLength(250)]
    public string? OptionalNote { get; set; }

    [Required, MaxLength(30)]
    public string ProviderSelectionMode { get; set; } = RealtorUrgentQuoteProviderModes.IndorAuto;

    [Required, MaxLength(30)]
    public string SendPayload { get; set; } = RealtorUrgentQuoteSendPayloads.IssueOnly;

    public bool NotifyClient { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<IndorRealtorUrgentQuoteDraftPhoto> Photos { get; set; } = [];
}

[Table("IndorRealtorUrgentQuoteDraftPhotos")]
public class IndorRealtorUrgentQuoteDraftPhoto
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    [ForeignKey(nameof(DraftId))]
    public IndorRealtorUrgentQuoteDraft? Draft { get; set; }

    [Required, MaxLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorRealtorSharedPackages")]
public class IndorRealtorSharedPackage
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [Required, MaxLength(120)]
    public string ClientName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    public DateTime SharedUtc { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(60)]
    public string StatusLabel { get; set; } = "Viewed by client";
}

public static class RealtorClientRoles
{
    public const string Buyer = "Buyer";
    public const string Seller = "Seller";
    public const string Homeowner = "Homeowner";
    public const string Tenant = "Tenant";
    public const string Other = "Other";

    public static IReadOnlyList<string> All => [Buyer, Seller, Homeowner, Tenant, Other];
}

public static class RealtorInvitationStatuses
{
    public const string Draft = "Draft";
    public const string Sent = "Sent";
    public const string Accepted = "Accepted";
    public const string Expired = "Expired";
}

public static class RealtorCollaborationLevels
{
    public const string ViewOnly = "ViewOnly";
    public const string CanComment = "CanComment";
    public const string CanUpload = "CanUpload";

    public static IReadOnlyList<(string Value, string Label, string Icon)> Options =>
    [
        (ViewOnly, "View Only", "fa-eye"),
        (CanComment, "Can Comment", "fa-comment"),
        (CanUpload, "Can Upload / Collaborate", "fa-cloud-arrow-up")
    ];
}

[Table("IndorRealtorClients")]
public class IndorRealtorClient
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? Email { get; set; }

    [Required, MaxLength(20)]
    public string ClientRole { get; set; } = RealtorClientRoles.Buyer;

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    [MaxLength(250)]
    public string? PropertyAddress { get; set; }

    [MaxLength(120)]
    public string? StatusSummary { get; set; }

    public DateTime LastActiveUtc { get; set; } = DateTime.UtcNow;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("IndorRealtorInvitations")]
public class IndorRealtorInvitation
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    public Guid InvitationToken { get; set; } = Guid.NewGuid();

    public int CurrentStep { get; set; } = 1;

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? ClientRole { get; set; }

    [MaxLength(250)]
    public string? QuickNote { get; set; }

    public int? PropertyFileId { get; set; }

    [ForeignKey(nameof(PropertyFileId))]
    public IndorRealtorPropertyFile? PropertyFile { get; set; }

    [MaxLength(250)]
    public string? PropertyAddress { get; set; }

    [MaxLength(80)]
    public string? PropertyLabel { get; set; }

    [MaxLength(120)]
    public string? PropertyCityRegion { get; set; }

    [MaxLength(40)]
    public string? PropertyStatusLabel { get; set; }

    public bool AccessPropertyOverview { get; set; } = true;

    public bool AccessFilesReports { get; set; } = true;

    public bool AccessQuotesEstimates { get; set; } = true;

    public bool AccessMessages { get; set; } = true;

    public bool AccessProjectUpdates { get; set; } = true;

    public bool AccessPayments { get; set; }

    [Required, MaxLength(30)]
    public string CollaborationLevel { get; set; } = RealtorCollaborationLevels.CanComment;

    public bool DeliveryEmail { get; set; } = true;

    public bool DeliveryText { get; set; }

    [MaxLength(250)]
    public string? WelcomeMessage { get; set; }

    public bool SendReminder48h { get; set; } = true;

    [Required, MaxLength(20)]
    public string Status { get; set; } = RealtorInvitationStatuses.Draft;

    public DateTime SentUtc { get; set; } = DateTime.UtcNow;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}

[Table("IndorRealtorActivities")]
public class IndorRealtorActivity
{
    public int Id { get; set; }

    public int RealtorId { get; set; }

    [ForeignKey(nameof(RealtorId))]
    public IndorRealtor? Realtor { get; set; }

    [Required, MaxLength(30)]
    public string ActivityType { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string CategoryTag { get; set; } = string.Empty;

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}

[Table("IndorRealtorQuoteBids")]
public class IndorRealtorQuoteBid
{
    public int Id { get; set; }

    public int QuoteId { get; set; }

    [ForeignKey(nameof(QuoteId))]
    public IndorRealtorQuote? Quote { get; set; }

    [Required, MaxLength(120)]
    public string ProviderName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(2,1)")]
    public decimal Rating { get; set; } = 4.5m;

    public int SortOrder { get; set; }
}
