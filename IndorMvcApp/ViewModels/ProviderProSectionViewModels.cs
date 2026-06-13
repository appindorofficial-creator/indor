namespace IndorMvcApp.ViewModels;

public class ProviderProPageBaseViewModel
{
    public string CompanyName { get; set; } = "Your company";
}

public class ProviderProJobsPageViewModel : ProviderProPageBaseViewModel
{
    public string ActiveTab { get; set; } = "active";
    public string? SearchQuery { get; set; }
    public int TodayCount { get; set; }
    public int ActiveCount { get; set; }
    public int NewLeadsCount { get; set; }
    public int EstimatesCount { get; set; }
    public int CompletedCount { get; set; }
    public int NeedsReportCount { get; set; }
    public decimal PaymentsDue { get; set; }
    public List<ProviderProJobsWorkItemViewModel> Items { get; set; } = [];
    public List<ProviderProSmartSuggestionViewModel> SmartSuggestions { get; set; } = [];
}

public class ProviderProJobsWorkItemViewModel
{
    public int? ItemId { get; set; }
    public int? ClienteId { get; set; }
    public string ItemKind { get; set; } = "Job";
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public string ScheduleTimeShort { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "scheduled";
    public string? SecondaryBadge { get; set; }
    public string SecondaryBadgeClass { get; set; } = "urgency";
    public string IconClass { get; set; } = "fa-wrench";
    public string IconTone { get; set; } = "blue";
    public decimal? EstimateAmount { get; set; }
    public List<ProviderProJobMetaLineViewModel> MetaLines { get; set; } = [];
    public bool ShowEstimateLink { get; set; }
    public bool ShowPhotosLink { get; set; }
    public bool ShowChecklistLink { get; set; }
    public bool ShowHouseFactsLink { get; set; }
    public string PrimaryAction { get; set; } = "View Details";
    public string PrimaryActionClass { get; set; } = "primary";
    public string? SecondaryAction { get; set; }
    public bool CanConvertToJob { get; set; }
    public int? LeadId { get; set; }
}

public class ProviderProJobMetaLineViewModel
{
    public string Text { get; set; } = "";
    public string IconClass { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "neutral";
}

public class ProviderProSmartSuggestionViewModel
{
    public string Text { get; set; } = "";
    public string IconClass { get; set; } = "fa-lightbulb";
    public string Tone { get; set; } = "blue";
    public string? Url { get; set; }
}

public class ProviderProCustomersPageViewModel : ProviderProPageBaseViewModel
{
    public string ActiveTab { get; set; } = "connected";
    public string? SearchQuery { get; set; }
    public int TotalCustomers { get; set; }
    public int ConnectedCount { get; set; }
    public int ActiveJobsCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int PropertiesCount { get; set; }
    public List<ProviderProCustomerCardViewModel> Customers { get; set; } = [];
    public List<ProviderProSmartSuggestionViewModel> ConnectionInsights { get; set; } = [];
}

public class ProviderProCustomerCardViewModel
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string ConnectionLabel { get; set; } = "Connected";
    public string ConnectionClass { get; set; } = "connected";
    public bool IsPropertyVerified { get; set; }
    public bool IsAppConnected { get; set; }
    public int PropertiesCount { get; set; } = 1;
    public bool ShowJobSection { get; set; }
    public string JobStatusLabel { get; set; } = "";
    public string JobStatusClass { get; set; } = "scheduled";
    public string JobTitle { get; set; } = "";
    public List<ProviderProJobMetaLineViewModel> JobMetaLines { get; set; } = [];
    public string? ActivityNote { get; set; }
    public bool ShowPhotosLink { get; set; }
    public string PrimaryAction { get; set; } = "View Customer";
    public string PrimaryActionClass { get; set; } = "primary";
    public string? SecondaryAction { get; set; }
}

public class ProviderProReportsPageViewModel : ProviderProPageBaseViewModel
{
    public string ActiveTab { get; set; } = "all";
    public string? SearchQuery { get; set; }
    public int DraftCount { get; set; }
    public int ReadyCount { get; set; }
    public int ApprovalCount { get; set; }
    public int ApprovedCount { get; set; }
    public int HouseFactsCount { get; set; }
    public List<ProviderProReportCardViewModel> Reports { get; set; } = [];
    public List<ProviderProSmartSuggestionViewModel> ReportInsights { get; set; } = [];
}

public class ProviderProReportCardViewModel
{
    public string Title { get; set; } = "";
    public string Address { get; set; } = "";
    public string CustomerJobLabel { get; set; } = "";
    public string CompletedLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "draft";
    public string IconClass { get; set; } = "fa-wrench";
    public int PhotosCount { get; set; }
    public bool HasChecklist { get; set; }
    public bool HasWarranty { get; set; }
    public bool HasDocuments { get; set; }
    public string ActionLabel { get; set; } = "View Record";
    public string ActionClass { get; set; } = "ghost";
}

public class ProviderProProfilePageViewModel : ProviderProPageBaseViewModel
{
    public string? LogoUrl { get; set; }
    public string CompanyInitial { get; set; } = "P";
    public string BusinessName { get; set; } = "";
    public string DbaName { get; set; } = "";
    public string PrimaryContact { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string BusinessAddress { get; set; } = "";
    public string PrimaryCity { get; set; } = "";
    public string RegistrationStatus { get; set; } = "";
    public bool IsApproved { get; set; }
    public bool IsVerified { get; set; }
    public bool IsTopRated { get; set; }
    public string SpecialtiesSummary { get; set; } = "";
    public string ServiceAreaLabel { get; set; } = "";
    public int ProviderScore { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public int JobsCompletedThisYear { get; set; }
    public int HomeRecordsCreated { get; set; }
    public string BusinessHours { get; set; } = "";
    public string Website { get; set; } = "";
    public int TravelRadiusMiles { get; set; }
    public bool EmergencyService { get; set; }
    public bool SameDayJobs { get; set; }
    public List<string> Categories { get; set; } = [];
    public List<string> Services { get; set; } = [];
    public List<string> ServiceAreas { get; set; } = [];
    public List<ProviderProProfileVerificationItemViewModel> VerificationItems { get; set; } = [];
    public bool PayoutConnected { get; set; }
    public bool PaymentProcessingActive { get; set; }
    public decimal NextPayoutAmount { get; set; }
    public string? NextPayoutDateLabel { get; set; }
    public List<ProviderProProfileTeamMemberViewModel> TeamMembers { get; set; } = [];
    public bool AutoRemindersOn { get; set; }
    public int ReportTemplatesCount { get; set; }
    public bool FollowUpCampaignsActive { get; set; }
    public bool AiAssistantEnabled { get; set; }
    public ProviderProProfilePerformanceViewModel Performance { get; set; } = new();
    public List<ProviderProProfileReviewSnippetViewModel> Reviews { get; set; } = [];
    public string SubscriptionPlan { get; set; } = "Pro Plan";
    public int DocumentsUploaded { get; set; }
    public int DocumentsRequired { get; set; }
}

public class ProviderProProfileVerificationItemViewModel
{
    public string Label { get; set; } = "";
    public bool IsComplete { get; set; }
}

public class ProviderProProfileTeamMemberViewModel
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string RoleClass { get; set; } = "owner";
}

public class ProviderProProfileReviewSnippetViewModel
{
    public string Author { get; set; } = "";
    public decimal Rating { get; set; }
    public string Comment { get; set; } = "";
}

public class ProviderProProfilePerformanceViewModel
{
    public string AvgResponseTime { get; set; } = "—";
    public string CompletionRate { get; set; } = "—";
    public string HomeownerApproval { get; set; } = "—";
    public int HouseFactsAdded { get; set; }
}

public class ProviderProEditProfileViewModel : ProviderProPageBaseViewModel
{
    public string? LogoUrl { get; set; }
    public string CompanyInitial { get; set; } = "P";
    public string BusinessName { get; set; } = "";
    public string DbaName { get; set; } = "";
    public string PrimaryContact { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string BusinessAddress { get; set; } = "";
    public string PrimaryCity { get; set; } = "";
    public string PreferredHours { get; set; } = "";
    public string ServiceDescription { get; set; } = "";
    public int TravelRadiusMiles { get; set; } = 25;
    public bool EmergencyService { get; set; } = true;
    public bool SameDayJobs { get; set; } = true;
}

public class ProviderProEditProfileInput
{
    public string BusinessName { get; set; } = "";
    public string DbaName { get; set; } = "";
    public string PrimaryContact { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string BusinessAddress { get; set; } = "";
    public string PrimaryCity { get; set; } = "";
    public string PreferredHours { get; set; } = "";
    public string ServiceDescription { get; set; } = "";
    public int TravelRadiusMiles { get; set; } = 25;
    public bool EmergencyService { get; set; } = true;
    public bool SameDayJobs { get; set; } = true;
}
