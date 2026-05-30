namespace IndorMvcApp.ViewModels;

public class RiskScoreIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "overview";
    public string PageTitle { get; set; } = "Risk Score";
    public string PageSubtitle { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public int OverallScore { get; set; }
    public int MaxScore { get; set; } = 100;
    public string OverallLevel { get; set; } = "Medium";
    public string OverallLevelTone { get; set; } = "orange";
    public string OverallSummary { get; set; } = string.Empty;
    public int FieldCount { get; set; }
    public List<RiskScoreTabViewModel> Tabs { get; set; } = new();
    public List<RiskCategoryViewModel> Categories { get; set; } = new();
    public List<RiskFactorViewModel> Factors { get; set; } = new();
    public List<RiskFindingViewModel> Findings { get; set; } = new();
    public List<RiskHistoryItemViewModel> HistoryItems { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string CategoriesAlert { get; set; } = string.Empty;
    public string PrimaryActionLabel { get; set; } = string.Empty;
    public string SecondaryActionLabel { get; set; } = string.Empty;
    public string? PrimaryActionTab { get; set; }
    public string? SecondaryActionTab { get; set; }
}

public class RiskScoreChecklistViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveFilter { get; set; } = "all";
    public bool HasData { get; set; }
    public int FieldCount { get; set; }
    public int CompletionPercent { get; set; }
    public string ConfidenceLabel { get; set; } = "Estimated";
    public string ConfidenceTone { get; set; } = "green";
    public List<RiskChecklistFilterViewModel> Filters { get; set; } = new();
    public List<RiskChecklistItemViewModel> Items { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
}

public class RiskScoreTabViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class RiskCategoryViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Level { get; set; } = "Unknown";
    public string LevelTone { get; set; } = "gray";
    public string ConfidenceNote { get; set; } = string.Empty;
    public string? ScoreDisplay { get; set; }
    public int? Score { get; set; }
    public string ScoreTone { get; set; } = "gray";
}

public class RiskFactorViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "orange";
}

public class RiskFindingViewModel
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Badge { get; set; } = string.Empty;
    public string BadgeTone { get; set; } = "orange";
    public string ActionLabel { get; set; } = "Resolve";
    public string? ActionTab { get; set; }
    public string? ChecklistFilter { get; set; }
}

public class RiskHistoryItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string When { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-clock";
}

public class RiskChecklistFilterViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? DotTone { get; set; }
    public string? Icon { get; set; }
}

public class RiskChecklistItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string ImpactTone { get; set; } = "orange";
    public string Status { get; set; } = "Needed";
    public string StatusTone { get; set; } = "orange";
    public string Icon { get; set; } = "fa-circle-info";
    public string FilterGroup { get; set; } = "all";
}
