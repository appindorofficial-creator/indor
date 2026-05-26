namespace IndorMvcApp.ViewModels;

public class OfferingCarouselViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Icon { get; set; } = "fa-layer-group";
    public string? BlockClass { get; set; }
    public string? IconClass { get; set; }
    public string? BadgeClass { get; set; }
    public List<OfferingCardViewModel> Items { get; set; } = new();
}

public class OfferingCardViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Description { get; set; } = string.Empty;
    public string[] Includes { get; set; } = Array.Empty<string>();
    public string? Frequency { get; set; }
    public decimal? Price { get; set; }
    public string? PricePrefix { get; set; }
    public string? PriceText { get; set; }
    public string Currency { get; set; } = "USD";
    public string Cta { get; set; } = "Schedule";
    public string? ImageUrl { get; set; }
    public string? ImageBase64 { get; set; }
    public string FreqIcon { get; set; } = "fa-rotate";
    public string CtaIcon { get; set; } = "fa-calendar-plus";
    public int? SourceId { get; set; }
    public bool EnableSchedule { get; set; }
    public string? LinkController { get; set; }
    public string? LinkAction { get; set; }
    public bool EnableLink { get; set; }
}
