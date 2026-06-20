using IndorMvcApp.Models;

namespace IndorMvcApp.ViewModels;

public class RealtorInspectionUploadStepViewModel
{
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string Title { get; set; } = "Upload Inspection Report";
    public string Subtitle { get; set; } = "";
}

public class RealtorInspectionPropertyOptionViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string DisplayAddress { get; set; } = "";
    public string SpecsLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
}

public class RealtorInspectionUploadViewModel : RealtorInspectionUploadStepViewModel
{
    public string? SearchQuery { get; set; }
    public int? SelectedPropertyFileId { get; set; }
    public string UploadMethod { get; set; } = "Pdf";
    public List<RealtorInspectionPropertyOptionViewModel> Properties { get; set; } = [];
    public string? NewPropertyAddress { get; set; }
    public string? NewPropertyClientName { get; set; }
    public string? NewPropertyCityRegion { get; set; }
}

public class RealtorInspectionAnalyzeViewModel : RealtorInspectionUploadStepViewModel
{
    public string PropertyDisplay { get; set; } = "";
    public string ReportFileName { get; set; } = "Home Inspection Report";
    public int ReportPageCount { get; set; }
    public string UploadedLabel { get; set; } = "";
    public int AnalysisProgress { get; set; }
    public string AnalysisStatus { get; set; } = "";
    public string? AnalysisSummary { get; set; }
    public List<RealtorInspectionAnalyzeTaskViewModel> Tasks { get; set; } = [];
    public List<RealtorInspectionCategoryChipViewModel> DetectedCategories { get; set; } = [];
}

public class RealtorInspectionAnalyzeTaskViewModel
{
    public string Label { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Status { get; set; } = "Pending";
}

public class RealtorInspectionCategoryChipViewModel
{
    public string Label { get; set; } = "";
    public string Css { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class RealtorInspectionPrioritiesViewModel : RealtorInspectionUploadStepViewModel
{
    public string PropertyDisplay { get; set; } = "";
    public string ReportFileName { get; set; } = "";
    public string? ReportPdfUrl { get; set; }
    public string? AnalysisSummary { get; set; }
    public string ReportDateLabel { get; set; } = "";
    public string InspectorLabel { get; set; } = "Residential Home Inspection";
    public string AnalyzedLabel { get; set; } = "Today";
    public int TotalFindings { get; set; }
    public int UrgentCount { get; set; }
    public int HighCount { get; set; }
    public int ModerateCount { get; set; }
    public string ActiveFilter { get; set; } = "All";
    public string SortBy { get; set; } = "Trade";
    public IReadOnlyList<string> Filters { get; set; } = ["All", "Urgent", "High", "Moderate"];
    public List<RealtorInspectionFindingCardViewModel> Findings { get; set; } = [];
}

public class RealtorInspectionFindingCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? SourceExcerpt { get; set; }
    public string? SourceSection { get; set; }
    public string? SourceSectionNumber { get; set; }
    public int? SourcePage { get; set; }
    public string? ReportReference { get; set; }
    public string? SourceLineItem { get; set; }
    public string Priority { get; set; } = "";
    public string PriorityCss { get; set; } = "";
    public string Trade { get; set; } = "";
    public string TradeLabel { get; set; } = "";
    public string TradeIcon { get; set; } = "";
    public string TradeCss { get; set; } = "";
    public int AiScore { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsSelected { get; set; }
}

public class RealtorInspectionTradeProviderGroupViewModel
{
    public string Trade { get; set; } = "";
    public string TradeLabel { get; set; } = "";
    public string PriorityNote { get; set; } = "";
    public List<RealtorQuoteProviderCardViewModel> Providers { get; set; } = [];
}

public class RealtorInspectionProvidersViewModel : RealtorInspectionUploadStepViewModel
{
    public string ActiveTradeFilter { get; set; } = "All";
    public IReadOnlyList<string> TradeFilters { get; set; } = ["All", "Electrical", "HVAC", "Plumbing", "Roof"];
    public List<RealtorInspectionTradeProviderGroupViewModel> TradeGroups { get; set; } = [];
    public int TradesReadyCount { get; set; }
    public int ProvidersSelectedCount { get; set; }
}

public class RealtorInspectionReviewRequestViewModel
{
    public string TradeLabel { get; set; } = "";
    public string PriorityTag { get; set; } = "";
    public string PriorityCss { get; set; } = "";
    public int ProviderCount { get; set; }
}

public class RealtorInspectionReviewViewModel : RealtorInspectionUploadStepViewModel
{
    public string PropertyAddress { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string CityRegion { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public int FindingsSelected { get; set; }
    public int UrgentItems { get; set; }
    public string TradesIncluded { get; set; } = "";
    public int ProvidersSelected { get; set; }
    public int ResponseDeadlineHours { get; set; } = 48;
    public List<RealtorInspectionReviewRequestViewModel> RequestsToCreate { get; set; } = [];
}

public class RealtorInspectionSuccessViewModel
{
    public string PropertyAddress { get; set; } = "";
    public string ClientName { get; set; } = "";
    public int QuotesCreated { get; set; }
    public int FindingsAdded { get; set; }
    public string TradesSummary { get; set; } = "";
    public List<string> QuoteCodes { get; set; } = [];
}
