namespace IndorMvcApp.ViewModels;

public class RoofExteriorIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "roof";
    public bool HasData { get; set; }
    public List<RoofExteriorTabViewModel> Tabs { get; set; } = new();
    public List<RoofExteriorSummaryCardViewModel> SummaryCards { get; set; } = new();
    public List<RoofExteriorListItemViewModel> Items { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
}

public class RoofExteriorTabViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-house-chimney";
}

public class RoofExteriorSummaryCardViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";
    public string StatusTone { get; set; } = "green";
    public string Icon { get; set; } = "fa-circle-check";
}

public class RoofExteriorListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "green";
    public string? SectionId { get; set; }
}

public class RoofExteriorSectionViewModel
{
    public int PropiedadId { get; set; }
    public string SectionId { get; set; } = "roof";
    public string Name { get; set; } = string.Empty;
    public string PageSubtitle { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "roof";
    public List<RoofExteriorTabViewModel> Tabs { get; set; } = new();
    public List<RoofExteriorSummaryCardViewModel> HealthBadges { get; set; } = new();
    public List<RoofExteriorSummaryCardViewModel> SummaryCards { get; set; } = new();
    public List<RoofExteriorDetailRowViewModel> Rows { get; set; } = new();
    public List<string> Reminders { get; set; } = new();
    public List<RoofExteriorActionViewModel> SuggestedActions { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string WhyItMattersTitle { get; set; } = string.Empty;
    public string WhyItMattersText { get; set; } = string.Empty;
}

public class RoofExteriorDetailRowViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "green";
}

public class RoofExteriorActionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string? Url { get; set; }
}

public class RoofExteriorCarePlanViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public int CurrentStep { get; set; } = 4;
    public List<RoofExteriorCareActionViewModel> RecommendedActions { get; set; } = new();
    public List<RoofExteriorVerificationItemViewModel> VerificationItems { get; set; } = new();
    public int VerifiedCount { get; set; }
    public int TotalCount { get; set; }
    public int ProgressPercent { get; set; }
    public List<RoofExteriorServiceOptionViewModel> ServiceOptions { get; set; } = new();
    public List<string> WhatHappensNext { get; set; } = new();
}

public class RoofExteriorCareActionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-calendar";
    public string Status { get; set; } = "Suggested";
    public string StatusTone { get; set; } = "blue";
    public string? Url { get; set; }
}

public class RoofExteriorVerificationItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-file-arrow-up";
    public string Status { get; set; } = "Needs upload";
    public string StatusTone { get; set; } = "orange";
}

public class RoofExteriorServiceOptionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-hard-hat";
    public string? Url { get; set; }
}
