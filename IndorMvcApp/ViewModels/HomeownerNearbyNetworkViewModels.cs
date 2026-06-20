namespace IndorMvcApp.ViewModels;

public class HomeownerNearbyNetworkViewModel
{
    public string PageTitle { get; set; } = "Nearby Network";
    public string RadiusLabel { get; set; } = "3 miles around your home";
    public string ActiveView { get; set; } = "feed";
    public string ActiveFilter { get; set; } = "All";
    public string? SearchQuery { get; set; }
    public int PropiedadId { get; set; }
    public bool HasProperty { get; set; }
    public int NotificationCount { get; set; }
    public IReadOnlyList<RealtorNetworkFilterChipViewModel> Filters { get; set; } = [];
    public List<RealtorQuickActionViewModel> QuickActions { get; set; } = [];
    public List<RealtorNetworkFeedCardViewModel> FeedCards { get; set; } = [];
    public List<RealtorNetworkMapPinViewModel> MapPins { get; set; } = [];
    public List<RealtorNetworkMapProviderViewModel> MapProviders { get; set; } = [];
    public string MapCenterLabel { get; set; } = "Your home";
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public double RadiusMiles { get; set; } = 3;
    public string? GoogleMapsApiKey { get; set; }
    public bool HasGoogleMaps => !string.IsNullOrWhiteSpace(GoogleMapsApiKey);
    public int MapNearbyCount { get; set; }
    public List<HomeownerMapCarouselItemViewModel> MapCarouselItems { get; set; } = [];
    public string? PropertyAddress { get; set; }
}

public class HomeownerMapCarouselItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ItemType { get; set; } = "provider";
    public string BadgeLabel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? IconClass { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? DistanceMiles { get; set; }
    public string? MetaLabel { get; set; }
    public List<string> Tags { get; set; } = [];
    public bool IsVerified { get; set; }
    public string PrimaryActionLabel { get; set; } = "View";
    public string PrimaryActionUrl { get; set; } = "#";
    public string? SecondaryActionLabel { get; set; }
    public string? SecondaryActionUrl { get; set; }
}
