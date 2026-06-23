using IndorMvcApp.Models;

namespace IndorMvcApp.ViewModels;

public class RealtorUrgentQuoteStepViewModel
{
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string Title { get; set; } = "Urgente Quote";
    public string Subtitle { get; set; } = "";
}

public class RealtorUrgentQuotePropertyOptionViewModel
{
    public int Id { get; set; }
    public string DisplayAddress { get; set; } = "";
    public string SpecsLabel { get; set; } = "";
    public string LocationLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
}

public class RealtorUrgentQuotePropertyViewModel : RealtorUrgentQuoteStepViewModel
{
    public string? SearchQuery { get; set; }
    public int? SelectedPropertyFileId { get; set; }
    public string RequestCategory { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string UrgencyLevel { get; set; } = "";
    public List<RealtorUrgentQuotePropertyOptionViewModel> Properties { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Icon)> CategoryOptions { get; set; } = [];
    public IReadOnlyList<string> ServiceTypes { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Icon)> UrgencyOptions { get; set; } = [];

    public IReadOnlyList<string> States { get; set; } = [];
    public bool QuickAddOpen { get; set; }
    public string QuickAddAddress { get; set; } = "";
    public string QuickAddCity { get; set; } = "";
    public string QuickAddState { get; set; } = "NC";
    public string QuickAddZip { get; set; } = "";
    public bool QuickAddUseForQuote { get; set; } = true;
}

public class RealtorUrgentQuoteSummaryViewModel
{
    public string DisplayAddress { get; set; } = "";
    public string SpecsLabel { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string ServiceType { get; set; } = "";
    public string UrgencyLabel { get; set; } = "";
    public string? QuickDescription { get; set; }
}

public class RealtorUrgentQuoteIssueViewModel : RealtorUrgentQuoteStepViewModel
{
    public RealtorUrgentQuoteSummaryViewModel Property { get; set; } = new();
    public string ServiceType { get; set; } = "HVAC";
    public string UrgencyLevel { get; set; } = RealtorUrgentQuoteUrgencyLevels.Today;
    public string QuickDescription { get; set; } = "";
    public string RequestTypeTag { get; set; } = RealtorUrgentQuoteRequestTags.NeedQuote;
    public IReadOnlyList<string> ServiceTypes { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Icon)> UrgencyOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Icon)> RequestTagOptions { get; set; } = [];
}

public class RealtorUrgentQuotePhotoItemViewModel
{
    public int Id { get; set; }
    public string FileUrl { get; set; } = "";
}

public class RealtorUrgentQuotePhotosViewModel : RealtorUrgentQuoteStepViewModel
{
    public RealtorUrgentQuoteSummaryViewModel Property { get; set; } = new();
    public List<RealtorUrgentQuotePhotoItemViewModel> Photos { get; set; } = [];
    public string OptionalNote { get; set; } = "";
}

public class RealtorUrgentQuoteSendViewModel : RealtorUrgentQuoteStepViewModel
{
    public RealtorUrgentQuoteSummaryViewModel Property { get; set; } = new();
    public int PhotoCount { get; set; }
    public string ProviderSelectionMode { get; set; } = RealtorUrgentQuoteProviderModes.IndorAuto;
    public string SendPayload { get; set; } = RealtorUrgentQuoteSendPayloads.IssueOnly;
    public bool NotifyClient { get; set; }
}

public class RealtorUrgentQuoteSuccessViewModel
{
    public string QuoteCode { get; set; } = "";
    public string PropertyAddress { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public string UrgencyLabel { get; set; } = "";
    public string SentWhenLabel { get; set; } = "";
    public int ProviderCount { get; set; }
}
