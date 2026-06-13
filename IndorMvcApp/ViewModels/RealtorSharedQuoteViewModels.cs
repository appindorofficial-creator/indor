namespace IndorMvcApp.ViewModels;

public class RealtorEditSharedQuoteViewModel : RealtorPortalShellViewModel
{
    public int SharedQuoteId { get; set; }
    public int QuoteId { get; set; }
    public int BidId { get; set; }
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string StatusLabel { get; set; } = "1 Quote Received";
    public string HomeownerName { get; set; } = "";
    public string HomeownerEmail { get; set; } = "";
    public string HomeownerPhone { get; set; } = "";
    public string HomeownerInitials { get; set; } = "";
    public bool ShareProviderInfo { get; set; } = true;
    public bool ShareFullPriceBreakdown { get; set; }
    public bool ShareScopeOfWork { get; set; } = true;
    public bool ShareWarranty { get; set; } = true;
    public bool ShareIncludedRepairs { get; set; } = true;
    public bool ShareTimeline { get; set; } = true;
    public string PricingDisplayMode { get; set; } = "TotalOnly";
    public string MessageToHomeowner { get; set; } = "";
    public string InternalNotes { get; set; } = "";
}

public class RealtorPreviewSharedQuoteViewModel : RealtorPortalShellViewModel
{
    public int SharedQuoteId { get; set; }
    public int QuoteId { get; set; }
    public int BidId { get; set; }
    public string HomeownerName { get; set; } = "";
    public string HomeownerRole { get; set; } = "Homeowner";
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string ProviderName { get; set; } = "";
    public string ProviderInitials { get; set; } = "";
    public bool ShowProviderInfo { get; set; } = true;
    public string TotalLabel { get; set; } = "";
    public string TimelineLabel { get; set; } = "";
    public string WarrantyLabel { get; set; } = "";
    public string ReviewStatusLabel { get; set; } = "Ready to review";
    public string ScopeOfWork { get; set; } = "";
    public List<string> IncludedRepairs { get; set; } = [];
    public List<RealtorQuotePriceLineViewModel> PriceLines { get; set; } = [];
    public string TotalAmountLabel { get; set; } = "";
    public bool ShowFullPriceBreakdown { get; set; }
    public string MessageToHomeowner { get; set; } = "";
    public string DeliveryMethod { get; set; } = "InApp";
    public string ShareLink { get; set; } = "";
}

public class RealtorSharedQuoteTrackingViewModel : RealtorPortalShellViewModel
{
    public int SharedQuoteId { get; set; }
    public int QuoteId { get; set; }
    public int BidId { get; set; }
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string HomeownerName { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public string TotalAmountLabel { get; set; } = "";
    public string StatusBadge { get; set; } = "Waiting";
    public string RecentActivityLabel { get; set; } = "";
    public string RecentActivityTime { get; set; } = "";
    public List<RealtorSharedQuoteTimelineItemViewModel> Timeline { get; set; } = [];
    public string ViewSharedQuoteUrl { get; set; } = "#";
    public string ShareLink { get; set; } = "";
}

public class RealtorSharedQuoteTimelineItemViewModel
{
    public string Label { get; set; } = "";
    public string? TimestampLabel { get; set; }
    public string? SubLabel { get; set; }
    public string Icon { get; set; } = "fa-circle-check";
    public string State { get; set; } = "done";
    public string? Badge { get; set; }
}

public class HomeownerSharedQuoteViewModel
{
    public string Address { get; set; } = "";
    public string? PhotoUrl { get; set; }
    public string RealtorName { get; set; } = "";
    public string ProviderName { get; set; } = "";
    public string ProviderInitials { get; set; } = "";
    public bool ShowProviderInfo { get; set; } = true;
    public string StatusLabel { get; set; } = "1 Quote Shared";
    public string TotalLabel { get; set; } = "";
    public string TimelineLabel { get; set; } = "";
    public string WarrantyLabel { get; set; } = "";
    public string ScopeOfWork { get; set; } = "";
    public List<string> IncludedRepairs { get; set; } = [];
    public List<RealtorQuotePriceLineViewModel> PriceLines { get; set; } = [];
    public string TotalAmountLabel { get; set; } = "";
    public bool ShowFullPriceBreakdown { get; set; }
    public string RealtorMessage { get; set; } = "";
    public string AcceptUrl { get; set; } = "#";
    public Guid ShareToken { get; set; }
}
