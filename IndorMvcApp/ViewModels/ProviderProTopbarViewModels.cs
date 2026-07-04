namespace IndorMvcApp.ViewModels;

public class ProviderProTopbarViewModel
{
    public string CompanyName { get; set; } = "INDOR PRO";
    public string CompanyInitial { get; set; } = "P";
    public bool ShowNotifications { get; set; }
    public bool HasNotifications { get; set; }
    public List<ProviderProNotificationItemViewModel> RecentNotifications { get; set; } = [];
}

public class ProviderProNotificationItemViewModel
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public string OccurredLabel { get; set; } = "";
    public string CategoryTag { get; set; } = "";
    public string? TargetUrl { get; set; }
    public string IconClass { get; set; } = "fa-circle-info";
    public string TagCssClass { get; set; } = "";
    public DateTime OccurredUtc { get; set; }
}
