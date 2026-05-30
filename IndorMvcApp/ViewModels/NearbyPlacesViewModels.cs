namespace IndorMvcApp.ViewModels;

public class NearbyPlacesIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string PageSubtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-tree";
    public string Tone { get; set; } = "green";
    public List<NearbyPlaceItemViewModel> Places { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
}

public class NearbyPlaceItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Distance { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string BadgeTone { get; set; } = "green";
    public string Icon { get; set; } = "fa-location-dot";
}

public class NearbyPlaceCardSummaryViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Tone { get; set; } = "blue";
    public string Badge { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}
