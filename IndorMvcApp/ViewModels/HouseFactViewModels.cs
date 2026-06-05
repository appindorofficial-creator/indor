namespace IndorMvcApp.ViewModels;

public class AttomFieldGroupViewModel
{
    public string SectionId { get; set; } = string.Empty;
    public string CategoryKey { get; set; } = "more";
    public string Title { get; set; } = string.Empty;
    public string DisplayTitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public int Order { get; set; }
    public string SectionKind { get; set; } = "fields";
    public string? Paragraph { get; set; }
    public string? Notes { get; set; }
    public string? Summary { get; set; }
    public int ItemCount { get; set; }
    public bool ExpandByDefault { get; set; }
    public List<AttomFieldItemViewModel> Fields { get; set; } = new();
    public List<HouseFactChecklistItemViewModel> ChecklistItems { get; set; } = new();
    public List<HouseFactSourceViewModel> Sources { get; set; } = new();
    public List<HouseFactSchoolViewModel> Schools { get; set; } = new();
    public List<HouseFactUtilityViewModel> Utilities { get; set; } = new();
}

public class HouseFactSchoolViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Level { get; set; } = "School";
    public string? Distance { get; set; }
    public string? Rating { get; set; }
}

public class HouseFactUtilityViewModel
{
    public string Type { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string Icon { get; set; } = "fa-plug";
}

public class HouseFactChecklistItemViewModel
{
    public string Item { get; set; } = string.Empty;
    public string? Status { get; set; }
}

public class AttomFieldItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool NeedsVerification { get; set; }
}

public class HouseFactSourceViewModel
{
    public string SourceName { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string InformationFound { get; set; } = string.Empty;
    public string? Conflicts { get; set; }
}

public class HouseFactProfileViewModel
{
    public string? DataSource { get; set; }
    public string? FormattedAddress { get; set; }
    public string? Confidence { get; set; }
    public int FieldCount { get; set; }
    public bool HasData { get; set; }
    public string RawJsonPretty { get; set; } = string.Empty;
    public string PropertyImageUrl { get; set; } = "/welcome-house.png";
    public int NeedsReviewCount { get; set; }
    public bool ShowSuccessBadge { get; set; }
    public List<AttomFieldGroupViewModel> Sections { get; set; } = new();
    public HouseFactOverviewViewModel Overview { get; set; } = new();
}

public class HouseFactOverviewViewModel
{
    public string? LocationSummary { get; set; }
    public string YearBuiltDisplay { get; set; } = "— Not confirmed";
    public string LivingAreaDisplay { get; set; } = "— Not confirmed";
    public string ConfidenceDisplay { get; set; } = "Needs verification";
    public List<HouseFactHeroStatViewModel> HeroStats { get; set; } = new();
    public List<HouseFactQuickJumpViewModel> QuickJumps { get; set; } = new();
    public List<HouseFactCategoryCardViewModel> CategoryCards { get; set; } = new();
}

public class HouseFactHeroStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class HouseFactQuickJumpViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class HouseFactCategoryCardViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "blue";
    public string Badge { get; set; } = string.Empty;
    public bool IsWarning { get; set; }
    public int ItemCount { get; set; }
    public List<string> SectionIds { get; set; } = new();
}
