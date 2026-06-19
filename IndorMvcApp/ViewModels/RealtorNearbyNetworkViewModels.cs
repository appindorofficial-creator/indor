using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class RealtorNearbyNetworkViewModel : RealtorPortalShellViewModel
{
    public string PageTitle { get; set; } = "Nearby Network";
    public string RadiusLabel { get; set; } = "3 miles around your home";
    public string ActiveView { get; set; } = "feed";
    public string ActiveFilter { get; set; } = "All";
    public string? SearchQuery { get; set; }
    public IReadOnlyList<RealtorNetworkFilterChipViewModel> Filters { get; set; } = [];
    public List<RealtorQuickActionViewModel> QuickActions { get; set; } = [];
    public List<RealtorNetworkFeedCardViewModel> FeedCards { get; set; } = [];
    public List<RealtorNetworkMapPinViewModel> MapPins { get; set; } = [];
    public string MapCenterLabel { get; set; } = "Your service area";
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusMiles { get; set; } = 3;
    public string? GoogleMapsApiKey { get; set; }
    public bool HasGoogleMaps => !string.IsNullOrWhiteSpace(GoogleMapsApiKey);
    public int MapNearbyCount { get; set; }
    public bool UseDeviceLocation { get; set; } = true;
    public List<RealtorNetworkMapProviderViewModel> MapProviders { get; set; } = [];
}

public class RealtorNetworkMapProviderViewModel
{
    public int ProviderId { get; set; }
    public string Name { get; set; } = "";
    public string? Category { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DistanceMiles { get; set; }
    public bool IsVerified { get; set; }
}

public class RealtorNetworkMapDataViewModel
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string CenterLabel { get; set; } = "Your location";
    public double RadiusMiles { get; set; } = 3;
    public string ActiveFilter { get; set; } = "All";
    public List<RealtorNetworkMapProviderViewModel> Providers { get; set; } = [];
    public List<RealtorNetworkMapPinViewModel> Listings { get; set; } = [];
    public int ProviderCount { get; set; }
    public int ListingCount { get; set; }
    public int TotalCount { get; set; }
}

public class RealtorNetworkFilterChipViewModel
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class RealtorNetworkFeedCardViewModel
{
    public int? ItemId { get; set; }
    public string CardType { get; set; } = "";
    public string BadgeLabel { get; set; } = "";
    public string BadgeCss { get; set; } = "default";
    public string? ImageUrl { get; set; }
    public string? IconClass { get; set; }
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string? PriceLabel { get; set; }
    public string? SpecsLabel { get; set; }
    public string? MetaLabel { get; set; }
    public string DistanceLabel { get; set; } = "";
    public string StatusBadge { get; set; } = "";
    public string StatusCss { get; set; } = "active";
    public string PrimaryActionLabel { get; set; } = "View";
    public string PrimaryActionUrl { get; set; } = "#";
    public string? SecondaryActionLabel { get; set; }
    public string? SecondaryActionUrl { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class RealtorNetworkMapPinViewModel
{
    public string Label { get; set; } = "";
    public string PinType { get; set; } = "home";
    public string IconClass { get; set; } = "fa-house";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string TopPercent { get; set; } = "50%";
    public string LeftPercent { get; set; } = "50%";
}

public class RealtorNetworkListingWizardViewModel : RealtorPortalShellViewModel
{
    public string Title { get; set; } = "Post Listing";
    public string? Subtitle { get; set; }
    public int DisplayStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public bool ShowStepper { get; set; } = true;
}

public class RealtorNetworkListingFormViewModel : RealtorPortalShellViewModel
{
    public int? ItemId { get; set; }
    public int WizardStep { get; set; } = 2;
    public int WizardTotalSteps { get; set; } = 4;
    public bool IsEdit { get; set; }

    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required, MaxLength(300)]
    public string Address { get; set; } = "";

    [Required]
    public decimal? Price { get; set; }

    public decimal? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? SquareFeet { get; set; }
    public int? YearBuilt { get; set; }
    public string ListingType { get; set; } = "sale";
    public string? PropertySubtype { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
    public string? AdditionalPhotoUrls { get; set; }

    [MaxLength(500)]
    public string? PhotoGalleryLink { get; set; }

    public string? PhotoPdfUrl { get; set; }
    public string? PhotoPdfFileName { get; set; }

    public string StatusBadge { get; set; } = "ACTIVE";
    public bool IsOpenHouse { get; set; }
    public string? OpenHouseMeta { get; set; }
    public decimal VisibilityRadiusMiles { get; set; } = 3;
    public bool PromoteInNearbyFeed { get; set; } = true;
    public bool PromoteOpenHouseProgram { get; set; }
    public bool FeaturedListing { get; set; }
    public double? AddressLatitude { get; set; }
    public double? AddressLongitude { get; set; }
    public bool SaveAsDraft { get; set; }
    public string? ReturnFilter { get; set; }

    public IReadOnlyList<(string Value, string Label)> PropertySubtypes { get; set; } =
    [
        ("single-family", "Single Family"),
        ("townhouse", "Townhouse"),
        ("condo", "Condo"),
        ("multi-family", "Multi-Family"),
        ("land", "Land / Lot"),
        ("other", "Other")
    ];

    public IReadOnlyList<(decimal Value, string Label)> VisibilityRadiusOptions { get; set; } =
    [
        (1m, "1 mile"),
        (3m, "3 miles"),
        (5m, "5 miles"),
        (10m, "10 miles")
    ];
}
