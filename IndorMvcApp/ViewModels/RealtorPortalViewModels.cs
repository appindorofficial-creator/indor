namespace IndorMvcApp.ViewModels;

public class RealtorPortalShellViewModel
{
    public string DisplayName { get; set; } = "";
    public string? ProfilePhotoUrl { get; set; }
    public string BadgeLabel { get; set; } = "Realtor Basic";
    public bool IsVerified { get; set; }
    public bool HasNotifications { get; set; }
}

public class RealtorHomeViewModel : RealtorPortalShellViewModel
{
    public List<RealtorQuickActionViewModel> QuickActions { get; set; } = [];
    public List<RealtorPropertyFileCardViewModel> PropertyFiles { get; set; } = [];
    public List<RealtorQuoteCardViewModel> PendingQuotes { get; set; } = [];
    public List<RealtorSharedPackageCardViewModel> SharedPackages { get; set; } = [];
}

public class RealtorQuickActionViewModel
{
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Url { get; set; } = "#";
}

public class RealtorClientsViewModel : RealtorPortalShellViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "All";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Buyers", "Sellers", "Homeowners", "Pending"];
    public List<RealtorClientCardViewModel> ActiveClients { get; set; } = [];
    public List<RealtorInvitationCardViewModel> PendingInvitations { get; set; } = [];
    public List<RealtorActivityItemViewModel> RecentActivity { get; set; } = [];
}

public class RealtorClientCardViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string ClientRole { get; set; } = "";
    public string? ProfileImageUrl { get; set; }
    public string? PropertyAddress { get; set; }
    public string StatusSummary { get; set; } = "";
    public string LastActiveLabel { get; set; } = "";
}

public class RealtorInvitationCardViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Initials { get; set; } = "";
    public string SentLabel { get; set; } = "";
}

public class RealtorFilesViewModel : RealtorPortalShellViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "Active";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Active", "Pre-Closing", "Repair Review", "Transfer", "Archived"];
    public List<RealtorFileCardViewModel> ActiveFiles { get; set; } = [];
    public List<RealtorActivityItemViewModel> RecentActivity { get; set; } = [];
}

public class RealtorFileCardViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string FilePhase { get; set; } = "";
    public string ClientName { get; set; } = "";
    public int RepairItemsCount { get; set; }
    public int QuotesReceivedCount { get; set; }
    public string UpdatedLabel { get; set; } = "";
}

public class RealtorQuotesViewModel : RealtorPortalShellViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "Pending";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Pending", "Received", "Compare", "Accepted", "Expired"];
    public List<RealtorOpenQuoteCardViewModel> OpenQuotes { get; set; } = [];
    public RealtorCompareQuotesViewModel? CompareQuotes { get; set; }
    public List<RealtorActivityItemViewModel> RecentActivity { get; set; } = [];
}

public class RealtorOpenQuoteCardViewModel
{
    public int Id { get; set; }
    public string QuoteCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string FooterNote { get; set; } = "";
    public string UpdatedLabel { get; set; } = "";
}

public class RealtorCompareQuotesViewModel
{
    public int QuoteId { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public List<RealtorQuoteBidViewModel> Bids { get; set; } = [];
}

public class RealtorQuoteBidViewModel
{
    public string ProviderName { get; set; } = "";
    public string AmountLabel { get; set; } = "";
    public decimal Rating { get; set; }
}

public class RealtorProfileViewModel : RealtorPortalShellViewModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string BrokerageName { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public string LicenseState { get; set; } = "";
    public string ServiceAreas { get; set; } = "";
    public bool CanUpgradeToVerified { get; set; }
    public List<RealtorProfileDocumentViewModel> Documents { get; set; } = [];
}

public class RealtorProfileDocumentViewModel
{
    public string DocumentType { get; set; } = "";
    public string Label { get; set; } = "";
    public bool Uploaded { get; set; }
    public bool Optional { get; set; }
}

public class RealtorActivityItemViewModel
{
    public int Id { get; set; }
    public string ActivityType { get; set; } = "";
    public string Description { get; set; } = "";
    public string OccurredLabel { get; set; } = "";
    public string CategoryTag { get; set; } = "";
}

// Shared card types (used on Home)
public class RealtorPropertyFileCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string SpecsLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string StatusLabel { get; set; } = "Active";
}

public class RealtorQuoteCardViewModel
{
    public int Id { get; set; }
    public string QuoteCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string RequestedLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "Pending";
}

public class RealtorSharedPackageCardViewModel
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string Address { get; set; } = "";
    public string SharedLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "";
}
