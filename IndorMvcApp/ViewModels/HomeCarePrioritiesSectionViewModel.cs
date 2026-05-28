namespace IndorMvcApp.ViewModels;

public class HomeCarePrioritiesSectionViewModel
{
    public int? PropiedadId { get; set; }
    public string Title { get; set; } = "This Year Priorities";
    public string Subtitle { get; set; } = "Stay ahead of important home maintenance.";
    public string IconClass { get; set; } = "fa-shield-halved";
    public string ViewAllText { get; set; } = "View all tasks";
    public string? ViewAllUrl { get; set; }
    public List<HomeCarePriorityCardViewModel> Items { get; set; } = new();
}

public class HomeCarePriorityCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-wrench";
    public string? Url { get; set; }
}
