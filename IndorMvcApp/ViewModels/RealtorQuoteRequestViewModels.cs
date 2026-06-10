using IndorMvcApp.Models;

namespace IndorMvcApp.ViewModels;

public class RealtorQuoteRequestStepViewModel
{
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string Title { get; set; } = "Request Quote";
    public string Subtitle { get; set; } = "";
}

public class RealtorQuoteRequestPropertyViewModel : RealtorQuoteRequestStepViewModel
{
    public string? SearchQuery { get; set; }
    public int? SelectedPropertyFileId { get; set; }
    public List<RealtorQuotePropertyOptionViewModel> Properties { get; set; } = [];
}

public class RealtorQuotePropertyOptionViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string FilePhase { get; set; } = "";
    public string FilePhaseCss { get; set; } = "";
}

public class RealtorQuoteRequestDetailsViewModel : RealtorQuoteRequestStepViewModel
{
    public string RequestType { get; set; } = RealtorQuoteRequestTypes.EntireFile;
    public bool SharePhotosVideos { get; set; } = true;
    public bool ShareInspectionReport { get; set; } = true;
    public bool ShareRepairItems { get; set; } = true;
    public bool ShareNotes { get; set; } = true;
    public int ResponseDeadlineHours { get; set; } = 48;
    public IReadOnlyList<(string Value, string Label, string Description, string Icon)> RequestTypeOptions { get; set; } = [];
    public IReadOnlyList<int> DeadlineOptions { get; set; } = [24, 48, 72];
}

public class RealtorQuoteProviderCardViewModel
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = "";
    public string Categories { get; set; } = "";
    public decimal Rating { get; set; }
    public decimal DistanceMiles { get; set; }
    public string? BadgeLabel { get; set; }
    public bool IsVerified { get; set; }
    public bool Selected { get; set; }
}

public class RealtorQuoteRequestProvidersViewModel : RealtorQuoteRequestStepViewModel
{
    public string ProviderSelectionMode { get; set; } = RealtorQuoteProviderSelectionModes.Manual;
    public string? SearchQuery { get; set; }
    public string ActiveFilter { get; set; } = "Recommended";
    public string ServiceType { get; set; } = "HVAC Repair";
    public int ProviderCountTarget { get; set; } = 3;
    public bool VerifiedOnly { get; set; } = true;
    public string Priority { get; set; } = RealtorQuotePriorities.FastResponse;
    public int CoverageMiles { get; set; } = 10;
    public List<RealtorQuoteProviderCardViewModel> Providers { get; set; } = [];
    public int SelectedCount { get; set; }
    public IReadOnlyList<string> ServiceTypes { get; set; } = [];
    public IReadOnlyList<(string Value, string Label, string Description)> SelectionModeOptions { get; set; } = [];
    public IReadOnlyList<(string Value, string Label)> PriorityOptions { get; set; } = [];
    public IReadOnlyList<string> ProviderFilters { get; set; } = ["Recommended", "Verified", "Nearby"];
}

public class RealtorQuoteRequestReviewViewModel : RealtorQuoteRequestStepViewModel
{
    public string PropertyDisplay { get; set; } = "";
    public string RequestTypeLabel { get; set; } = "";
    public string SharedSummary { get; set; } = "";
    public string ProvidersSummary { get; set; } = "";
    public string ProviderSelectionLabel { get; set; } = "";
    public bool SendNow { get; set; } = true;
    public DateTime? ScheduledSendUtc { get; set; }
    public int ResponseDeadlineHours { get; set; } = 48;
    public bool AllowProviderQuestions { get; set; } = true;
    public bool AllowFullProjectQuote { get; set; } = true;
    public bool AllowItemizedQuote { get; set; } = true;
    public string OptionalMessage { get; set; } = "";
}

public class RealtorQuoteRequestSuccessViewModel
{
    public string QuoteCode { get; set; } = "";
    public int QuoteId { get; set; }
    public string PropertyAddress { get; set; } = "";
    public string RequestTypeLabel { get; set; } = "";
    public string ProvidersSummary { get; set; } = "";
    public string SentWhenLabel { get; set; } = "";
    public int ResponseDeadlineHours { get; set; }
    public int ProviderCount { get; set; }
    public string? PhotoUrl { get; set; }
    public int ItemCount { get; set; }
}
