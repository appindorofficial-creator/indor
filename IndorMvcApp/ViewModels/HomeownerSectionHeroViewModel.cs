namespace IndorMvcApp.ViewModels;

public class HomeownerSectionHeroViewModel
{
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string? BadgeLabel { get; set; }
    public string? BadgeIconClass { get; set; } = "fa-gift";
    public string CssClass { get; set; } = "";
}
