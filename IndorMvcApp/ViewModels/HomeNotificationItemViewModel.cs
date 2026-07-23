namespace IndorMvcApp.ViewModels;

public class HomeNotificationItemViewModel
{
    public string Icon { get; set; } = "fa-bell";
    public string Severity { get; set; } = "upcoming";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    /// <summary>Optional deep-link (persisted notifications only).</summary>
    public string? Url { get; set; }

    /// <summary>Optional pre-formatted time label; overrides the default date rendering.</summary>
    public string? TimeLabel { get; set; }

    /// <summary>Skips localization of already-localized persisted titles/subtitles.</summary>
    public bool IsPreLocalized { get; set; }
}
