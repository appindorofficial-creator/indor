using IndorMvcApp.Services;

namespace IndorMvcApp.ViewModels;

/// <summary>
/// Public, no-login "Explore" page: the shared services catalog plus a
/// read-only directory of real, registered providers. Lets visitors freely
/// browse services and service providers before creating an account
/// (App Store guideline 5.1.1(v)).
/// </summary>
public sealed class ExploreViewModel
{
    public required HomeCatalogSnapshot Catalog { get; init; }
    public List<ExploreProviderCardViewModel> Providers { get; init; } = [];
}

public sealed class ExploreProviderCardViewModel
{
    public required string Name { get; init; }
    public string? CategoryLabel { get; init; }
    public string? Location { get; init; }
    public bool IsVerified { get; init; }
    public string IconClass { get; init; } = "fa-screwdriver-wrench";
}
