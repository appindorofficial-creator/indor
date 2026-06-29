namespace IndorMvcApp.ViewModels;

public class HomeDashboardViewModel
{
    public string UserFirstName { get; set; } = "there";
    public string Greeting { get; set; } = "Hello";
    public bool HasProperty { get; set; }
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = "/welcome-house.png";
    public string HomeValue { get; set; } = "—";
    public string? BedsLabel { get; set; }
    public string? BathsLabel { get; set; }
    public string? SqftLabel { get; set; }
    public string HouseFactsUrl { get; set; } = "#";
    public List<HomeQuickActionViewModel> QuickActions { get; set; } = new();
    public List<HomeTodayTaskViewModel> TodayTasks { get; set; } = new();
    public List<HomeScheduleItemViewModel> UpcomingSchedule { get; set; } = new();
    public List<HomeDocumentItemViewModel> RecentDocuments { get; set; } = new();
    public List<HomeActivityItemViewModel> RecentActivity { get; set; } = new();
    public int NotificationCount { get; set; }
    public PropertyMaintenanceSectionViewModel? MaintenanceSection { get; set; }
}

public class HomeQuickActionViewModel
{
    public string Icon { get; set; } = "fa-circle";
    public string Label { get; set; } = string.Empty;
    public string TargetSection { get; set; } = "section-services";
    public string? ScrollTarget { get; set; }
    public string? Url { get; set; }
    public string Tone { get; set; } = "blue";
}

public class HomeTodayTaskViewModel
{
    public string Icon { get; set; } = "fa-circle";
    public string? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? Badge { get; set; }
    public string Url { get; set; } = "#";
}

public class HomeScheduleItemViewModel
{
    public string Icon { get; set; } = "fa-calendar";
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DueLabel { get; set; } = string.Empty;
}

public class HomeDocumentItemViewModel
{
    public string Icon { get; set; } = "fa-file-lines";
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string Url { get; set; } = "#";
}

public class HomeActivityItemViewModel
{
    public string Icon { get; set; } = "fa-circle";
    public string Title { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
