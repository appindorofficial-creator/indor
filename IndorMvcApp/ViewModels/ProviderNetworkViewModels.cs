using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.ViewModels;

/// <summary>A trade/service chip used to filter the contractor network.</summary>
public sealed class NetworkTradeChipViewModel
{
    public required string Id { get; init; }
    public required string Label { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
}

/// <summary>
/// Card shown in the "Find Subcontractors" list and the network home. All data
/// is read from real registered providers in <c>IndorProveedores</c>.
/// </summary>
public sealed class NetworkSubcontractorCardViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string AvatarInitial { get; init; } = "P";
    public string? PhotoUrl { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";

    public decimal? Rating { get; init; }
    public int ReviewCount { get; init; }
    public bool HasReviews => ReviewCount > 0 && Rating.HasValue;

    public int JobsCompletedCount { get; init; }
    public bool HasJobsCompleted => JobsCompletedCount > 0;

    /// <summary>Short response label e.g. "Responds in 30 min". Null when unknown.</summary>
    public string? ResponseLabel { get; init; }

    public string? DistanceLabel { get; init; }
    public string? LocationLabel { get; init; }

    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }
    public bool IsRecommended { get; init; }

    public bool IsAvailableNow { get; init; }
    public string AvailabilityLabel { get; init; } = "";
}

/// <summary>Screen 1 — Contractor Network Home.</summary>
public sealed class NetworkHomeViewModel
{
    public required string CompanyName { get; init; }
    public int VerifiedSubcontractorsCount { get; init; }
    public int InsuredCount { get; init; }
    public int MyRequestsCount { get; init; }
    public int ActiveHiresCount { get; init; }
    public List<NetworkTradeChipViewModel> TradeChips { get; init; } = [];
    public List<NetworkSubcontractorCardViewModel> FeaturedSubcontractors { get; init; } = [];
}

/// <summary>Screen 2 — Find Subcontractors (list / map + filters).</summary>
public sealed class FindSubcontractorsViewModel
{
    public required string CompanyName { get; init; }
    public string? Query { get; init; }
    public string ActiveTrade { get; init; } = "all";
    public string ActiveView { get; init; } = "list";
    public bool FilterNearby { get; init; }
    public bool FilterInsuredOnly { get; init; }
    public bool FilterAvailableNow { get; init; }
    public bool FilterDocsReady { get; init; }
    public List<NetworkTradeChipViewModel> TradeChips { get; init; } = [];
    public List<NetworkSubcontractorCardViewModel> Results { get; init; } = [];
    public int ResultCount => Results.Count;
    public string SortLabel { get; init; } = "Best Match";
    public bool HasLocation { get; init; }
}

public sealed class NetworkReviewViewModel
{
    public required string AuthorName { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public string DateLabel { get; init; } = "";
}

public sealed class NetworkRatingBarViewModel
{
    public int Stars { get; init; }
    public int Percent { get; init; }
}

/// <summary>Screen 3 — Verified Pro Profile.</summary>
public sealed class SubcontractorProfileViewModel
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string AvatarInitial { get; init; } = "P";
    public string? PhotoUrl { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";

    public decimal? Rating { get; init; }
    public int ReviewCount { get; init; }
    public bool HasReviews => ReviewCount > 0 && Rating.HasValue;

    public string? LocationLabel { get; init; }
    public string? DistanceLabel { get; init; }

    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }

    public string InsuranceStatusLabel { get; init; } = "General Liability Insurance";
    public bool InsuranceActive { get; init; }

    public string? LicenseNumber { get; init; }
    public bool LicenseVerified { get; init; }

    public List<string> Services { get; init; } = [];

    public string AvailabilityLabel { get; init; } = "Contact for availability";
    public bool IsAvailableNow { get; init; }
    public string ResponseTimeLabel { get; init; } = "Typically replies within a day";

    public int RecentJobsCount { get; init; }

    public List<NetworkReviewViewModel> Reviews { get; init; } = [];
    public List<NetworkRatingBarViewModel> RatingBreakdown { get; init; } = [];

    public string? Phone { get; init; }
    public string? Email { get; init; }

    public bool IsSaved { get; init; }
}

/// <summary>Budget options shown as chips in the Post a Job wizard.</summary>
public static class PostJobOptions
{
    public static readonly string[] Budgets =
    [
        "Under $250",
        "$250 – $500",
        "$500 – $1,500",
        "$1,500+",
        "Not Sure"
    ];
}

/// <summary>Screen 4a — Post a Job, step 1 (Details).</summary>
public sealed class PostJobDetailsViewModel
{
    public int? DraftId { get; set; }
    public List<NetworkTradeChipViewModel> TradeOptions { get; init; } = [];
    public string? SelectedTradeId { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? Urgency { get; set; }
    public List<string> Photos { get; init; } = [];
    public string? ErrorMessage { get; set; }
}

public sealed class PostJobDetailsInput
{
    public int? DraftId { get; set; }
    public string? TradeId { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? Urgency { get; set; }
    public List<IFormFile>? Photos { get; set; }
    public List<string>? ExistingPhotos { get; set; }
}

/// <summary>Screen 4b — Post a Job, step 2 (Location &amp; Budget).</summary>
public sealed class PostJobLocationViewModel
{
    public int DraftId { get; set; }
    public string? JobTitle { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PropertyType { get; set; }
    public string? WhoMeets { get; set; }
    public string? BudgetRange { get; set; }
    public IReadOnlyList<string> BudgetOptions => PostJobOptions.Budgets;
    public string? QuoteType { get; set; }
    public string? AccessNotes { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class PostJobLocationInput
{
    public int DraftId { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? PropertyType { get; set; }
    public string? WhoMeets { get; set; }
    public string? BudgetRange { get; set; }
    public string? QuoteType { get; set; }
    public string? AccessNotes { get; set; }
    /// <summary>"draft" to save and exit, otherwise continue to Review.</summary>
    public string? Mode { get; set; }
}

/// <summary>Screen 4c — Post a Job, step 3 (Review).</summary>
public sealed class PostJobReviewViewModel
{
    public int DraftId { get; set; }
    public string? TradeLabel { get; set; }
    public string TradeIconClass { get; init; } = "fa-screwdriver-wrench";
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public List<string> Photos { get; init; } = [];
    public string? Location { get; set; }
    public string? PropertyTypeLabel { get; set; }
    public string? BudgetRange { get; set; }
    public string? QuoteTypeLabel { get; set; }
    public string? UrgencyLabel { get; set; }
    public string? AccessNotes { get; set; }
}

/// <summary>Success screen after posting a job.</summary>
public sealed class NetworkJobPostedViewModel
{
    public int JobId { get; init; }
    public string? TradeLabel { get; init; }
    public int MatchedCount { get; init; }
    public List<NetworkSubcontractorCardViewModel> Matches { get; init; } = [];
}

/// <summary>Screen 5 — Hire With Confidence.</summary>
public sealed class HireSubcontractorViewModel
{
    public int SubcontractorId { get; init; }
    public required string Name { get; init; }
    public string? TradeLabel { get; init; }
    public string AvatarInitial { get; init; } = "P";
    public string? PhotoUrl { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";

    public decimal? Rating { get; init; }
    public int ReviewCount { get; init; }
    public bool HasReviews => ReviewCount > 0 && Rating.HasValue;
    public string? LocationLabel { get; init; }

    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }

    public int? NetworkJobId { get; init; }
    public string ProjectTitle { get; set; } = "";
    public string TradeSummary { get; init; } = "";
    public string BudgetRange { get; set; } = "";
    public string StartDateLabel { get; init; } = "";
    public string? StartDateIso { get; init; }

    public bool ProfileReviewed { get; init; } = true;
    public bool InsuranceVerified { get; init; }
    public bool DocumentsReady { get; init; }
    public bool AvailabilityConfirmed { get; init; } = true;
}

public sealed class ConfirmHireInput
{
    public int SubcontractorId { get; set; }
    public int? NetworkJobId { get; set; }
    public string? ProjectTitle { get; set; }
    public string? TradeLabel { get; set; }
    public string? BudgetRange { get; set; }
    public string? StartDate { get; set; }
    public string? Mode { get; set; }
}

/// <summary>Success screen after confirming a hire.</summary>
public sealed class NetworkHireConfirmedViewModel
{
    public int HireId { get; init; }
    public required string SubcontractorName { get; init; }
    public string? TradeLabel { get; init; }
    public string AvatarInitial { get; init; } = "P";
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string? ProjectTitle { get; init; }
    public string? BudgetRange { get; init; }
    public string StartDateLabel { get; init; } = "";
    public string StatusLabel { get; init; } = "Hired";
    public bool IsVerified { get; init; }
    public bool IsInsured { get; init; }
    public bool IsDocsReady { get; init; }
}
