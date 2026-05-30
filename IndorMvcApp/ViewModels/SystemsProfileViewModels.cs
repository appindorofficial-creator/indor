namespace IndorMvcApp.ViewModels;

public class SystemsProfileIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveFilter { get; set; } = "all";
    public int SystemCount { get; set; }
    public int NeedsVerificationCount { get; set; }
    public int AlertCount { get; set; }
    public bool HasData { get; set; }
    public List<SystemProfileItemViewModel> Systems { get; set; } = new();
    public List<SystemFilterChipViewModel> Filters { get; set; } = new();
}

public class SystemFilterChipViewModel
{
    public string Key { get; set; } = "all";
    public string Label { get; set; } = string.Empty;
}

public class SystemProfileItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "Estimated";
    public string StatusTone { get; set; } = "green";
}

public class SystemsVerificationViewModel
{
    public int PropiedadId { get; set; }
    public int VerifiedCount { get; set; }
    public int TotalCount { get; set; }
    public int ProgressPercent { get; set; }
    public List<SystemVerificationItemViewModel> Items { get; set; } = new();
}

public class SystemVerificationItemViewModel
{
    public string SystemId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "orange";
}

public class SystemDetailViewModel
{
    public int PropiedadId { get; set; }
    public string SystemId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PageSubtitle { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "overview";
    public List<SystemStatusBadgeViewModel> StatusBadges { get; set; } = new();
    public List<SystemAttributeViewModel> Attributes { get; set; } = new();
    public List<SystemFactViewModel> KnownFacts { get; set; } = new();
    public List<SystemFactViewModel> MissingItems { get; set; } = new();
    public List<SystemActionViewModel> SuggestedActions { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public List<SystemTabViewModel> Tabs { get; set; } = new();
    public string? ServiceHistoryNote { get; set; }
    public string? WarrantyNote { get; set; }
    public string? DocumentsNote { get; set; }
    public int? HvacMicroservicioId { get; set; }
}

public class SystemStatusBadgeViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tone { get; set; } = "blue";
}

public class SystemAttributeViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Tone { get; set; } = "default";
}

public class SystemFactViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class SystemActionViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string? Url { get; set; }
}

public class SystemTabViewModel
{
    public string Key { get; set; } = "overview";
    public string Label { get; set; } = string.Empty;
}
