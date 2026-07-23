namespace IndorMvcApp.ViewModels;

public sealed class ServiceRequestCategoryOption
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
    public string? Description { get; init; }
}

public sealed class ServiceRequestPropertyOption
{
    public int Id { get; init; }
    public string Address { get; init; } = string.Empty;
}

/// <summary>Homeowner: the "Request a service" form.</summary>
public sealed class ServiceRequestCreateViewModel
{
    public List<ServiceRequestCategoryOption> Categories { get; set; } = new();
    public List<ServiceRequestPropertyOption> Properties { get; set; } = new();

    // Posted / prefilled values
    public string? CategoryId { get; set; }
    public int? PropiedadId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? ContactPhone { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string? PreferredTime { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string Urgency { get; set; } = "Standard";
    public string? ErrorMessage { get; set; }
}

/// <summary>A row in a homeowner or provider request list.</summary>
public sealed class ServiceRequestListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CategoryLabel { get; init; } = string.Empty;
    public string CategoryIcon { get; init; } = "fa-screwdriver-wrench";
    public string? Location { get; init; }
    public string? WhenLabel { get; init; }
    public string? BudgetLabel { get; init; }
    public string Status { get; init; } = "Open";
    public string Urgency { get; init; } = "Standard";
    public DateTime CreatedUtc { get; init; }
    public string? ClaimedProviderName { get; init; }
    public string? Description { get; init; }
}

public sealed class HomeownerRequestsViewModel
{
    public List<ServiceRequestListItemViewModel> Open { get; set; } = new();
    public List<ServiceRequestListItemViewModel> Claimed { get; set; } = new();
    public List<ServiceRequestListItemViewModel> Closed { get; set; } = new();
    public int TotalCount => Open.Count + Claimed.Count + Closed.Count;
}

public sealed class ClaimedProviderContactViewModel
{
    public string Name { get; init; } = string.Empty;
    public string? Contact { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public bool IsInsured { get; init; }
    public bool IsLicensed { get; init; }
    public string? YearsExperience { get; init; }
}

public sealed class HomeownerRequestDetailViewModel
{
    public ServiceRequestListItemViewModel Request { get; init; } = new();
    public string? ContactPhone { get; init; }
    public DateTime? ClaimedUtc { get; init; }
    public ClaimedProviderContactViewModel? Provider { get; init; }
}

public sealed class ProviderAvailableRequestsViewModel
{
    public string CompanyName { get; set; } = string.Empty;
    public List<ServiceRequestListItemViewModel> Requests { get; set; } = new();
    public bool HasMatchingCategories { get; set; }
}

public sealed class ProviderRequestDetailViewModel
{
    public string CompanyName { get; set; } = string.Empty;
    public ServiceRequestListItemViewModel Request { get; init; } = new();
    public string? Description { get; init; }
    public string? ContactPhone { get; init; }
    public string? HomeownerName { get; init; }
    public bool CanTake { get; init; }
    public bool AlreadyTaken { get; init; }
    public bool TakenByMe { get; init; }
}

public enum ClaimServiceRequestResult
{
    Success,
    AlreadyTaken,
    NotFound,
    NotEligible
}
