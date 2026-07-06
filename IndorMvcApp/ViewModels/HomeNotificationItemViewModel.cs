namespace IndorMvcApp.ViewModels;

public class HomeNotificationItemViewModel
{
    public string Icon { get; set; } = "fa-bell";
    public string Severity { get; set; } = "upcoming";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
