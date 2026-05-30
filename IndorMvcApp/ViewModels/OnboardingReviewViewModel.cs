namespace IndorMvcApp.ViewModels;

public class OnboardingReviewViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Unit { get; set; }
}
