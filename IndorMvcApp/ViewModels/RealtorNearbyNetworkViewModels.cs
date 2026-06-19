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
    public string TopPercent { get; set; } = "50%";
    public string LeftPercent { get; set; } = "50%";
}

public class RealtorNetworkListingFormViewModel : RealtorPortalShellViewModel
{
    public int? ItemId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [Required, MaxLength(300)]
    public string Address { get; set; } = "";
    public decimal? Price { get; set; }
    public decimal? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int? SquareFeet { get; set; }
    public string? ImageUrl { get; set; }
    public string StatusBadge { get; set; } = "ACTIVE";
    public bool IsOpenHouse { get; set; }
    public string? OpenHouseMeta { get; set; }
    public string? ReturnFilter { get; set; }
}
