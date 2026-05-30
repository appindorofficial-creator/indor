namespace IndorMvcApp.ViewModels;

public class MissingInformationHubViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public int ProfileStrengthPercent { get; set; }
    public int ItemsRemaining { get; set; }
    public int HighPriorityCount { get; set; }
    public int DocumentsNeededCount { get; set; }
    public MissingRecommendedStepViewModel? RecommendedStep { get; set; }
    public List<MissingCategorySummaryViewModel> CategorySummaries { get; set; } = new();
}

public class MissingInformationCategoriesViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public int ProfileStrengthPercent { get; set; }
    public MissingRecommendedStepViewModel? FeaturedStep { get; set; }
    public List<MissingCategoryCardViewModel> Categories { get; set; } = new();
}

public class MissingInformationCategoryViewModel
{
    public int PropiedadId { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "blue";
    public List<MissingItemViewModel> Items { get; set; } = new();
}

public class MissingInformationVerifyViewModel
{
    public int PropiedadId { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string PriorityTone { get; set; } = "orange";
    public string Icon { get; set; } = "fa-circle-info";
    public List<MissingVerifyNeedViewModel> Needs { get; set; } = new();
    public List<MissingVerifyActionViewModel> Actions { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string PrimaryActionLabel { get; set; } = string.Empty;
}

public class MissingInformationUpdatedViewModel
{
    public int PropiedadId { get; set; }
    public int BeforePercent { get; set; }
    public int AfterPercent { get; set; }
    public int ImprovementPercent { get; set; }
    public List<MissingUpdatedItemViewModel> UpdatedItems { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
}

public class MissingRecommendedStepViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-star";
    public string ItemId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = "documents";
}

public class MissingCategorySummaryViewModel
{
    public string CategoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "blue";
    public int ItemCount { get; set; }
}

public class MissingCategoryCardViewModel
{
    public string CategoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "blue";
    public int ItemCount { get; set; }
    public bool IsFeatured { get; set; }
}

public class MissingItemViewModel
{
    public string ItemId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string ImpactTone { get; set; } = "orange";
    public string Status { get; set; } = "Needed";
    public string Icon { get; set; } = "fa-circle-info";
    public bool IsHighPriority { get; set; }
}

public class MissingVerifyNeedViewModel
{
    public string Label { get; set; } = string.Empty;
}

public class MissingVerifyActionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "blue";
}

public class MissingUpdatedItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-check";
}
