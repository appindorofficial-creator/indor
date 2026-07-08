namespace IndorMvcApp.ViewModels;

/// <summary>A node in the Posted → Quotes → Hired → Done progress stepper.</summary>
public sealed class RequestStepViewModel
{
    public required string Label { get; init; }
    public required string IconClass { get; init; }
    /// <summary>done | active | pending</summary>
    public string State { get; init; } = "pending";
}

/// <summary>An entry in a request's activity timeline.</summary>
public sealed class RequestActivityViewModel
{
    public required string IconClass { get; init; }
    public required string Title { get; init; }
    public string? Detail { get; init; }
    public string TimeLabel { get; init; } = "";
    /// <summary>blue | eye | quote</summary>
    public string Tone { get; init; } = "blue";
}

/// <summary>A subcontractor's quote card (Request Details + Compare Quotes).</summary>
public sealed class NetworkQuoteViewModel
{
    public int Id { get; init; }
    public int SubcontractorId { get; init; }
    public required string Name { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string? PhotoUrl { get; init; }
    public string? LocationLabel { get; init; }
    public decimal? Rating { get; init; }
    public int ReviewCount { get; init; }
    public int JobsCompleted { get; init; }
    public string ResponseLabel { get; init; } = "Responds in 1 hr";
    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }

    public string RangeLabel { get; init; } = "";
    public string AmountLabel { get; init; } = "";
    public decimal QuotedAmount { get; init; }
    public int ResponseMinutes { get; init; }
    public bool WithinBudget { get; init; }
    public string? Message { get; init; }
    public bool IsBestMatch { get; set; }
    public bool IsSelected { get; init; }
}

public sealed class RequestTabViewModel
{
    public required string Id { get; init; }
    public required string Label { get; init; }
}

/// <summary>A row in the My Requests list.</summary>
public sealed class RequestListItemViewModel
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? TradeLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string IconTone { get; init; } = "blue";
    public string? LocationLabel { get; init; }
    public string? BudgetRange { get; init; }
    public string PostedLabel { get; init; } = "";
    public string StatusLabel { get; init; } = "";
    /// <summary>green | amber | purple | grey</summary>
    public string StatusKind { get; init; } = "grey";
    public int QuoteCount { get; init; }
    public List<RequestStepViewModel> Steps { get; init; } = [];
}

/// <summary>Screen — My Requests list.</summary>
public sealed class MyRequestsViewModel
{
    public string ActiveTab { get; init; } = "all";
    public string? Query { get; init; }
    public List<RequestTabViewModel> Tabs { get; init; } = [];
    public List<RequestListItemViewModel> Requests { get; init; } = [];
}

/// <summary>Screen — Request Details (timeline, activity, quotes).</summary>
public sealed class RequestDetailsViewModel
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? TradeLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string IconTone { get; init; } = "blue";
    public string? LocationLabel { get; init; }
    public string? BudgetRange { get; init; }
    public string PostedLabel { get; init; } = "";
    public string StatusLabel { get; init; } = "";
    public string StatusKind { get; init; } = "grey";

    public List<string> Photos { get; init; } = [];
    public List<RequestStepViewModel> Steps { get; init; } = [];
    public List<RequestActivityViewModel> Activity { get; init; } = [];
    public List<NetworkQuoteViewModel> Quotes { get; init; } = [];
    public int QuoteCount => Quotes.Count;

    public bool IsHired { get; init; }
}

/// <summary>Screen — Compare Quotes.</summary>
public sealed class CompareQuotesViewModel
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public string? TradeLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string IconTone { get; init; } = "blue";
    public string? LocationLabel { get; init; }
    public string? BudgetRange { get; init; }
    public string PostedLabel { get; init; } = "";

    public string Sort { get; init; } = "price";
    public List<NetworkQuoteViewModel> Quotes { get; init; } = [];
}

/// <summary>Screen — Request Confirmed (after hiring a pro).</summary>
public sealed class RequestConfirmedViewModel
{
    public int JobId { get; init; }
    public required string Title { get; init; }
    public string? TradeLabel { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string IconTone { get; init; } = "blue";
    public string? LocationLabel { get; init; }

    public int SubcontractorId { get; init; }
    public required string ContractorName { get; init; }
    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }

    public string QuoteLabel { get; init; } = "";
    public string ScheduledLabel { get; init; } = "";
    public string? AddressLine { get; init; }
    public string DurationLabel { get; init; } = "1 – 2 hours";
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    public List<RequestStepViewModel> Steps { get; init; } = [];
}
