namespace IndorMvcApp.ViewModels;

public class MoreProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int ProfileCompletionPercent { get; set; }
    public bool IsProfileComplete => ProfileCompletionPercent >= 100;
    public List<ProfileCompletionItemViewModel> CompletionItems { get; set; } = [];
    public string MembershipLabel { get; set; } = "No Membership";
    public bool HasActiveMembership { get; set; }
    public int HomeCount { get; set; }
    public int DocumentCount { get; set; }
    public int ServiceCount { get; set; }
}
