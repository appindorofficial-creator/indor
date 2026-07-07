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

/// <summary>Screen 4 — Post a Job.</summary>
public sealed class PostNetworkJobViewModel
{
    public required string CompanyName { get; init; }
    public List<NetworkTradeChipViewModel> TradeOptions { get; init; } = [];
    public List<string> BudgetOptions { get; init; } =
    [
        "Under $500",
        "$500 – $1,000",
        "$1,000 – $5,000",
        "$5,000 – $10,000",
        "$10,000 – $25,000",
        "$25,000+"
    ];

    // Preserved values on validation errors.
    public string? SelectedTradeId { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? DateNeeded { get; set; }
    public string? BudgetRange { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class PostNetworkJobInput
{
    public string? TradeId { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? DateNeeded { get; set; }
    public string? BudgetRange { get; set; }
    public IFormFile? Photo { get; set; }
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
