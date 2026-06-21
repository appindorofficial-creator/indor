namespace IndorMvcApp.ViewModels;

public class ScheduleSectionViewModel
{
    public bool HasProperty { get; set; }

    public int? PropiedadId { get; set; }

    public List<ScheduleQuickAddItemViewModel> QuickAddItems { get; set; } = [];

    public List<ScheduleReminderItemViewModel> ComingUpItems { get; set; } = [];

    public string? CreateReminderUrl { get; set; }

    public string? BookServiceUrl { get; set; }
}

public class ScheduleQuickAddItemViewModel
{
    public string Label { get; set; } = string.Empty;

    public string IconClass { get; set; } = "fa-calendar";

    public string? ImageUrl { get; set; }

    public string ToneClass { get; set; } = "sch-tone-general";

    public string Url { get; set; } = "#";
}

public class ScheduleReminderItemViewModel
{
    public string SourceKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string DateLabel { get; set; } = string.Empty;

    public DateTime SortDate { get; set; }

    public string IconClass { get; set; } = "fa-calendar-check";

    public string ToneClass { get; set; } = "sch-tone-general";

    public string EditUrl { get; set; } = "#";

    public string EditLabel { get; set; } = "Edit";
}
