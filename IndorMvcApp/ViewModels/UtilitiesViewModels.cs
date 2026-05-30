namespace IndorMvcApp.ViewModels;

public class UtilitiesIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "overview";
    public int CurrentStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string StepLabel { get; set; } = "Overview";
    public string PageTitle { get; set; } = "Utilities";
    public string PageSubtitle { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public int ProviderCount { get; set; }
    public bool SavedToHouseFacts { get; set; }
    public List<UtilitiesTabViewModel> Tabs { get; set; } = new();
    public List<UtilityItemViewModel> Utilities { get; set; } = new();
    public List<UtilityContactRowViewModel> ContactRows { get; set; } = new();
    public List<UtilitySetupItemViewModel> SetupItems { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string PrimaryActionLabel { get; set; } = string.Empty;
    public string SecondaryActionLabel { get; set; } = string.Empty;
    public string PrimaryActionIcon { get; set; } = "fa-address-book";
    public string SecondaryActionIcon { get; set; } = "fa-arrow-left";
    public string? PrimaryActionTab { get; set; }
    public string? SecondaryActionTab { get; set; }
}

public class UtilitiesDetailViewModel
{
    public int PropiedadId { get; set; }
    public string UtilityId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string PageSubtitle { get; set; } = "Provider information for this property.";
    public int ProviderCount { get; set; }
    public string UtilityType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-plug";
    public string Status { get; set; } = "Active";
    public string StatusTone { get; set; } = "green";
    public string ServiceBadge { get; set; } = "Service available";
    public string ServiceBadgeTone { get; set; } = "green";
    public List<UtilityDetailRowViewModel> Rows { get; set; } = new();
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string InfoBanner { get; set; } = string.Empty;
}

public class UtilitiesTabViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Step { get; set; }
}

public class UtilityItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-plug";
    public string Status { get; set; } = "Available";
    public string StatusTone { get; set; } = "green";
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ServiceType { get; set; }
    public string? Notes { get; set; }
    public string? Coverage { get; set; }
}

public class UtilityContactRowViewModel
{
    public string UtilityId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-plug";
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string Status { get; set; } = "Active";
    public string StatusTone { get; set; } = "green";
}

public class UtilitySetupItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-check";
    public bool Completed { get; set; }
}

public class UtilityDetailRowViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public bool IsLink { get; set; }
    public string? LinkHref { get; set; }
    public bool ShowChevron { get; set; } = true;
}
