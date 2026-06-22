namespace IndorMvcApp.ViewModels;

public class RealtorPortalShellViewModel
{
    public string DisplayName { get; set; } = "";
    public string FullDisplayName { get; set; } = "";
    public string? ProfilePhotoUrl { get; set; }
    public string BadgeLabel { get; set; } = "Realtor Basic";
    public bool IsVerified { get; set; }
    public bool HasNotifications { get; set; }
}

public class RealtorStatItemViewModel
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
    public string Icon { get; set; } = "";
    public string ColorClass { get; set; } = "blue";
    public string? DetailUrl { get; set; }
}

public class RealtorInsightViewModel
{
    public string Text { get; set; } = "";
    public string Icon { get; set; } = "fa-lightbulb";
    public string ColorClass { get; set; } = "teal";
    public string? TargetUrl { get; set; }
}

public class RealtorNextStepViewModel
{
    public string Text { get; set; } = "";
    public string Icon { get; set; } = "";
    public string ColorClass { get; set; } = "blue";
    public string? Url { get; set; }
}

public class RealtorHomeViewModel : RealtorPortalShellViewModel
{
    public List<RealtorQuickActionViewModel> QuickActions { get; set; } = [];
    public List<RealtorStatItemViewModel> Stats { get; set; } = [];
    public List<RealtorPropertyFileCardViewModel> PropertyFiles { get; set; } = [];
    public List<RealtorQuoteCardViewModel> PendingQuotes { get; set; } = [];
    public List<RealtorSharedPackageCardViewModel> SharedPackages { get; set; } = [];
    public List<RealtorInsightViewModel> Insights { get; set; } = [];
}

public class RealtorQuickActionViewModel
{
    public string Label { get; set; } = "";
    public string? Subtitle { get; set; }
    public string Icon { get; set; } = "";
    public string Url { get; set; } = "#";
}

public class RealtorClientsViewModel : RealtorPortalShellViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "All";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Buyers", "Sellers", "Homeowners", "Invited"];
    public List<RealtorStatItemViewModel> Stats { get; set; } = [];
    public List<RealtorClientCardViewModel> ActiveClients { get; set; } = [];
    public List<RealtorInvitationCardViewModel> PendingInvitations { get; set; } = [];
    public List<RealtorActivityItemViewModel> RecentActivity { get; set; } = [];
    public List<RealtorNextStepViewModel> NextSteps { get; set; } = [];
}

public class RealtorClientCardViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Initials { get; set; } = "";
    public string ClientRole { get; set; } = "";
    public string? ProfileImageUrl { get; set; }
    public string? PropertyAddress { get; set; }
    public string StatusSummary { get; set; } = "";
    public string StatusBadge { get; set; } = "Connected";
    public string StatusCss { get; set; } = "connected";
    public string LastActiveLabel { get; set; } = "";
    public int FilesCount { get; set; }
    public int QuotesCount { get; set; }
    public string ActionLabel { get; set; } = "Open Client";
    public string ActionUrl { get; set; } = "#";
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
    public string ActiveFilter { get; set; } = "All";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Active", "Inspection", "Quotes", "Shared", "Closed"];
    public List<RealtorStatItemViewModel> Stats { get; set; } = [];
    public List<RealtorFileCardViewModel> ActiveFiles { get; set; } = [];
    public List<RealtorActivityItemViewModel> RecentActivity { get; set; } = [];
    public List<RealtorInsightViewModel> Insights { get; set; } = [];
}

public class RealtorFileCardViewModel
{
    public int Id { get; set; }
    public string FileCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string FilePhase { get; set; } = "";
    public string StatusBadge { get; set; } = "";
    public string StatusCss { get; set; } = "active";
    public string ClientName { get; set; } = "";
    public int RepairItemsCount { get; set; }
    public int QuotesReceivedCount { get; set; }
    public string UpdatedLabel { get; set; } = "";
    public string ActionLabel { get; set; } = "Open File";
    public string ActionUrl { get; set; } = "#";
    public string DetailNote { get; set; } = "";
}

public class RealtorQuotesViewModel : RealtorPortalShellViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "All";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Requested", "Received", "Compare", "Selected", "Urgent"];
    public List<RealtorStatItemViewModel> Stats { get; set; } = [];
    public List<RealtorOpenQuoteCardViewModel> OpenQuotes { get; set; } = [];
    public RealtorCompareQuotesViewModel? CompareQuotes { get; set; }
    public List<RealtorActivityItemViewModel> RecentActivity { get; set; } = [];
    public List<RealtorNextStepViewModel> Alerts { get; set; } = [];
}

public class RealtorOpenQuoteCardViewModel
{
    public int Id { get; set; }
    public string QuoteCode { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string Address { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusCss { get; set; } = "pending";
    public string? PhotoUrl { get; set; }
    public string FooterNote { get; set; } = "";
    public string UpdatedLabel { get; set; } = "";
    public string RequestedLabel { get; set; } = "";
    public int ProviderQuotesReceived { get; set; }
    public string ActionLabel { get; set; } = "View Request";
    public string ActionUrl { get; set; } = "";
    public string? SecondaryActionLabel { get; set; }
    public string? SecondaryActionUrl { get; set; }
    public string ProviderSummary { get; set; } = "";
    public string ProviderInitials { get; set; } = "";
    public bool IsUrgent { get; set; }
    public string PriceRangeLabel { get; set; } = "";
}

public class RealtorQuoteDetailViewModel : RealtorPortalShellViewModel
{
    public int QuoteId { get; set; }
    public string QuoteStatus { get; set; } = "Pending";
    public string QuoteCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string StatusLabel { get; set; } = "";
    public string StatusCss { get; set; } = "pending";
    public string RequestedLabel { get; set; } = "";
    public string? DueLabel { get; set; }
    public string FooterNote { get; set; } = "";
    public string? OptionalMessage { get; set; }
    public int ProviderQuotesReceived { get; set; }
    public int ProvidersSentCount { get; set; }
    public int? PropertyFileId { get; set; }
    public string RequestedByLabel { get; set; } = "Realtor";
    public string ProvidersResponseLabel { get; set; } = "";
    public List<RealtorQuoteRequestedServiceViewModel> RequestedServices { get; set; } = [];
    public string InviteProvidersUrl { get; set; } = "#";
    public List<RealtorQuoteDetailBidViewModel> Bids { get; set; } = [];
    public List<RealtorQuoteDetailProviderViewModel> SentProviders { get; set; } = [];
}

public class RealtorQuoteRequestedServiceViewModel
{
    public int SortOrder { get; set; }
    public string Title { get; set; } = "";
    public string Icon { get; set; } = "fa-wrench";
}

public class RealtorQuoteDetailBidViewModel
{
    public int Id { get; set; }
    public string ProviderName { get; set; } = "";
    public string AmountLabel { get; set; } = "";
    public decimal Rating { get; set; }
    public string SubmittedLabel { get; set; } = "";
}

public class RealtorQuoteDetailProviderViewModel
{
    public string ProviderName { get; set; } = "";
    public string StatusLabel { get; set; } = "Waiting";
    public string StatusCss { get; set; } = "pending";
}

public class RealtorCompareQuotesViewModel
{
    public int QuoteId { get; set; }
    public string Address { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public List<RealtorQuoteBidViewModel> Bids { get; set; } = [];
}

public class RealtorCompareQuotesPageViewModel : RealtorPortalShellViewModel
{
    public int QuoteId { get; set; }
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string StatusLabel { get; set; } = "";
    public string RequestedLabel { get; set; } = "";
    public string PriceRangeLabel { get; set; } = "";
    public string TimelineRangeLabel { get; set; } = "";
    public string InviteProvidersUrl { get; set; } = "#";
    public List<RealtorCompareQuoteCardViewModel> Quotes { get; set; } = [];
}

public class RealtorCompareQuoteCardViewModel
{
    public int BidId { get; set; }
    public string ProviderName { get; set; } = "";
    public string ProviderInitials { get; set; } = "";
    public string AmountLabel { get; set; } = "";
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string TimelineLabel { get; set; } = "";
    public string WarrantyLabel { get; set; } = "";
    public bool IsBestValue { get; set; }
    public string ViewDetailsUrl { get; set; } = "#";
}

public class RealtorViewQuoteViewModel : RealtorPortalShellViewModel
{
    public int QuoteId { get; set; }
    public int BidId { get; set; }
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string StatusLabel { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public string ProviderInitials { get; set; } = "";
    public string TotalLabel { get; set; } = "";
    public string TimelineLabel { get; set; } = "";
    public string WarrantyLabel { get; set; } = "";
    public string ReviewStatusLabel { get; set; } = "Ready to review";
    public string ScopeOfWork { get; set; } = "";
    public List<string> IncludedRepairs { get; set; } = [];
    public List<RealtorQuotePriceLineViewModel> PriceLines { get; set; } = [];
    public string TotalAmountLabel { get; set; } = "";
    public string CompareQuotesUrl { get; set; } = "#";
    public string RequestAnotherUrl { get; set; } = "#";
    public string EditSharedQuoteUrl { get; set; } = "#";
}

public class RealtorQuotePriceLineViewModel
{
    public string Label { get; set; } = "";
    public string AmountLabel { get; set; } = "";
}

public class RealtorQuoteSelectedViewModel : RealtorPortalShellViewModel
{
    public int QuoteId { get; set; }
    public int BidId { get; set; }
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string ProviderName { get; set; } = "";
    public string TotalAmountLabel { get; set; } = "";
    public string ApprovedLabel { get; set; } = "";
    public string TimelineLabel { get; set; } = "";
    public string WarrantyLabel { get; set; } = "";
    public string ViewQuoteUrl { get; set; } = "#";
    public List<RealtorQuoteNextStepViewModel> NextSteps { get; set; } = [];
}

public class RealtorQuoteNextStepViewModel
{
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "fa-circle";
}

public class RealtorQuoteBidViewModel
{
    public string ProviderName { get; set; } = "";
    public string AmountLabel { get; set; } = "";
    public decimal Rating { get; set; }
}

public class RealtorBusinessInformationViewModel : RealtorEditProfileWizardViewModel
{
    public int ProfileCompletionPercent { get; set; }
    public List<RealtorBusinessInfoRowViewModel> Rows { get; set; } = [];
}

public class RealtorEditProfileWizardViewModel : RealtorPortalShellViewModel
{
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string HeaderBadge { get; set; } = "Start here";
    public string BackAction { get; set; } = "Profile";
    public string BackController { get; set; } = "Realtor";
}

public class RealtorEditProfileContactViewModel : RealtorEditProfileWizardViewModel
{
    public string BusinessName { get; set; } = "";
    public string PublicDisplayName { get; set; } = "";
    public string BrokerageName { get; set; } = "";
    public string RealtorTitle { get; set; } = "";
    public string Email { get; set; } = "";
    public string Website { get; set; } = "";
    public string OfficeAddress { get; set; } = "";
    public string OfficeCity { get; set; } = "";
    public string OfficeState { get; set; } = "";
    public string OfficeZip { get; set; } = "";
    public List<string> Languages { get; set; } = [];
    public string LanguagesCsv { get; set; } = "";
    public IReadOnlyList<string> LicenseStates { get; set; } = [];
}

public class RealtorEditProfileLicenseViewModel : RealtorEditProfileWizardViewModel
{
    public string LicenseNumber { get; set; } = "";
    public string LicenseState { get; set; } = "";
    public string YearsOfExperience { get; set; } = "";
    public List<string> SelectedSpecialties { get; set; } = [];
    public string TeamName { get; set; } = "";
    public string BrokerInCharge { get; set; } = "";
    public List<RealtorDocumentSlotViewModel> DocumentSlots { get; set; } = [];
    public List<RealtorEditProfileVerificationItemViewModel> VerificationItems { get; set; } = [];
    public IReadOnlyList<string> SpecialtyOptions { get; set; } = [];
    public IReadOnlyList<string> ExperienceOptions { get; set; } = [];
    public IReadOnlyList<string> LicenseStates { get; set; } = [];
}

public class RealtorEditProfileVerificationItemViewModel
{
    public string Label { get; set; } = "";
    public bool IsVerified { get; set; }
}

public class RealtorEditProfileReviewViewModel : RealtorEditProfileWizardViewModel
{
    public List<RealtorEditProfileReviewRowViewModel> SummaryRows { get; set; } = [];
    public RealtorEditProfilePreviewViewModel Preview { get; set; } = new();
}

public class RealtorEditProfileReviewRowViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string? SecondaryValue { get; set; }
    public string Icon { get; set; } = "";
    public string EditAction { get; set; } = "";
    public string? StatusBadge { get; set; }
    public string StatusCss { get; set; } = "";
}

public class RealtorEditProfilePreviewViewModel
{
    public string FullName { get; set; } = "";
    public string BrokerageName { get; set; } = "";
    public string LocationLabel { get; set; } = "";
    public string? ProfilePhotoUrl { get; set; }
}

public class RealtorBusinessInfoRowViewModel
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string? StatusBadge { get; set; }
    public string StatusCss { get; set; } = "";
    public string Icon { get; set; } = "";
    public string? EditUrl { get; set; }
    public bool IsEmpty { get; set; }
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
    public List<RealtorStatItemViewModel> Stats { get; set; } = [];
    public string ResponseTimeLabel { get; set; } = "1.2 hrs";
    public string ResponseTimeStatus { get; set; } = "Excellent";
    public int FilesThisMonth { get; set; }
    public string FilesTrendLabel { get; set; } = "";
    public int ClientConnections { get; set; }
    public string ClientsTrendLabel { get; set; } = "";
    public List<RealtorInsightViewModel> Insights { get; set; } = [];
    public bool EmailAlertsEnabled { get; set; } = true;
    public bool QuoteUpdatesEnabled { get; set; } = true;
    public bool ReportNotificationsEnabled { get; set; } = true;
    public bool PackageViewAlertsEnabled { get; set; }
    public bool NotificationsSaved { get; set; }
}

public class RealtorNotificationPreferencesInput
{
    public bool EmailAlertsEnabled { get; set; } = true;
    public bool QuoteUpdatesEnabled { get; set; } = true;
    public bool ReportNotificationsEnabled { get; set; } = true;
    public bool PackageViewAlertsEnabled { get; set; }
}

public class RealtorProfileDocumentViewModel
{
    public string DocumentType { get; set; } = "";
    public string Label { get; set; } = "";
    public bool Uploaded { get; set; }
    public bool Optional { get; set; }
}

public class RealtorNetworkViewModel : RealtorPortalShellViewModel
{
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "Recommended";
    public List<RealtorQuoteProviderCardViewModel> Providers { get; set; } = [];
    public IReadOnlyList<string> ProviderFilters { get; set; } = ["Recommended", "Verified", "Nearby"];
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
    public string StreetAddress { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string SpecsLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string StatusLabel { get; set; } = "Active";
    public string StatusCss { get; set; } = "active";
    public string ActionLabel { get; set; } = "Open File";
    public string ActionUrl { get; set; } = "#";
}

public class RealtorQuoteCardViewModel
{
    public int Id { get; set; }
    public string QuoteCode { get; set; } = "";
    public string Address { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string RequestedLabel { get; set; } = "";
    public string DueLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "Pending";
    public string StatusCss { get; set; } = "pending";
    public string? PhotoUrl { get; set; }
    public int ProviderQuotesReceived { get; set; }
    public string ActionLabel { get; set; } = "Compare Quotes";
    public List<string> ProviderInitials { get; set; } = [];
}

public class RealtorSharedPackageCardViewModel
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string Address { get; set; } = "";
    public string PackageTitle { get; set; } = "";
    public string SharedLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusCss { get; set; } = "viewed";
    public string IconColor { get; set; } = "teal";
    public string ActionLabel { get; set; } = "Open Package";
}

public class RealtorPublicProfileViewModel : RealtorPortalShellViewModel
{
    public bool IsOwnProfile { get; set; } = true;
    public string FullName { get; set; } = "";
    public string TitleLabel { get; set; } = "Realtor";
    public string? Tagline { get; set; }
    public string? Bio { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? LocationLabel { get; set; }
    public string? BrokerageName { get; set; }
    public string? LicenseLabel { get; set; }
    public List<RealtorPublicVerificationItemViewModel> VerificationItems { get; set; } = [];
    public List<RealtorStatItemViewModel> Stats { get; set; } = [];
    public List<string> ServiceAreaChips { get; set; } = [];
    public List<RealtorPublicListingCardViewModel> ActiveListings { get; set; } = [];
    public List<RealtorPublicOpenHouseViewModel> OpenHouses { get; set; } = [];
    public List<RealtorPublicSharedPackageViewModel> SharedPackages { get; set; } = [];
    public string ShareUrl { get; set; } = "";
    public string AccountStatusLabel { get; set; } = "";
    public string AccountStatusCss { get; set; } = "";
    public string HeroLocationLabel { get; set; } = "";
    public bool ShowVerificationPrompt { get; set; }
    public List<RealtorPublicHeroBadgeViewModel> HeroBadges { get; set; } = [];
    public List<RealtorPublicVerificationStepViewModel> VerificationSteps { get; set; } = [];
}

public class RealtorPublicHeroBadgeViewModel
{
    public string Label { get; set; } = "";
    public bool IsComplete { get; set; }
}

public class RealtorPublicVerificationStepViewModel
{
    public string Label { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public bool IsComplete { get; set; }
}

public class RealtorPublicVerificationItemViewModel
{
    public string Label { get; set; } = "";
    public bool IsComplete { get; set; }
}

public class RealtorPublicListingCardViewModel
{
    public int ItemId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string PriceLabel { get; set; } = "";
    public string? StatusBadge { get; set; }
    public string? ImageUrl { get; set; }
    public string? SpecsLabel { get; set; }
    public string ViewUrl { get; set; } = "#";
}

public class RealtorPublicOpenHouseViewModel
{
    public int ItemId { get; set; }
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string? MetaLabel { get; set; }
    public string? ImageUrl { get; set; }
    public string ViewUrl { get; set; } = "#";
}

public class RealtorPublicSharedPackageViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string SharedLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "";
}
