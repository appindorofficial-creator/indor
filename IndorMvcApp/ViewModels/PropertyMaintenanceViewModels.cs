namespace IndorMvcApp.ViewModels;

public class PropertyMaintenancePlanViewModel
{
    public string Summary { get; set; } = string.Empty;
    public string? DataSource { get; set; }
    public bool IsAiGenerated { get; set; }
    public DateTime? GeneratedUtc { get; set; }
    public List<PropertyMaintenanceItemViewModel> Items { get; set; } = [];
}

public class PropertyMaintenanceItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Priority { get; set; } = "Routine";
    public string Frequency { get; set; } = "As needed";
    public string Icon { get; set; } = "fa-screwdriver-wrench";
    public string? Reason { get; set; }
    public int SortOrder { get; set; }
    public string? ScheduleUrl { get; set; }
    public string ScheduleActionLabel { get; set; } = "Schedule now";
    public bool HasScheduleLink => !string.IsNullOrWhiteSpace(ScheduleUrl);
}

public class PropertyMaintenanceSectionViewModel
{
    public string Title { get; set; } = "AI Maintenance Plan";
    public string Subtitle { get; set; } = "Personalized upkeep recommendations for your home.";
    public string? Summary { get; set; }
    public string? DataSource { get; set; }
    public bool IsAiGenerated { get; set; }
    public bool IsUnavailable { get; set; }
    public List<PropertyMaintenanceItemViewModel> Items { get; set; } = [];
    public int UrgentCount { get; set; }
    public int HighCount { get; set; }
    public bool ShowAlerts { get; set; }
}
