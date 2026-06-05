namespace IndorMvcApp.ViewModels;

public class EmergencyServicesGuideViewModel
{
    public string Title { get; set; } = "Emergency Services";
    public string Subtitle { get; set; } = "Fast help for urgent home problems.";
    public List<EmergencyGuideCardViewModel> Items { get; set; } = new();
}

public class EmergencyGuideCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string IconoClase { get; set; } = "fa-truck-medical";
    public string? Url { get; set; }
}
