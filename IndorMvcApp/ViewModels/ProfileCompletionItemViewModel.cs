namespace IndorMvcApp.ViewModels;

public class ProfileCompletionItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public string? ActionUrl { get; set; }
}
