namespace IndorMvcApp.ViewModels;

public class HoaCommunityViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "overview";
    public int CurrentStep { get; set; } = 1;
    public int TotalSteps { get; set; } = 5;
    public string PageTitle { get; set; } = "HOA & Community";
    public string PageSubtitle { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public string HoaStatus { get; set; } = "Active";
    public string HoaStatusTone { get; set; } = "green";
    public string HoaName { get; set; } = string.Empty;
    public string EstimatedFee { get; set; } = string.Empty;
    public string Confidence { get; set; } = "Estimated";
    public string ConfidenceTone { get; set; } = "green";
    public List<HoaTabViewModel> Tabs { get; set; } = new();
    public List<HoaRowViewModel> Rows { get; set; } = new();
    public List<HoaAmenityViewModel> Amenities { get; set; } = new();
    public HoaManagementViewModel? Management { get; set; }
    public List<HoaDocumentViewModel> Documents { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string PrimaryActionLabel { get; set; } = string.Empty;
    public string SecondaryActionLabel { get; set; } = string.Empty;
    public string PrimaryActionIcon { get; set; } = "fa-file-lines";
    public string SecondaryActionIcon { get; set; } = "fa-phone";
}

public class HoaTabViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Step { get; set; }
}

public class HoaRowViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string? Badge { get; set; }
    public string BadgeTone { get; set; } = "green";
    public bool ShowChevron { get; set; } = true;
}

public class HoaAmenityViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-check";
}

public class HoaManagementViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Badge { get; set; } = "Estimated";
    public string BadgeTone { get; set; } = "blue";
    public List<HoaContactRowViewModel> Contacts { get; set; } = new();
}

public class HoaContactRowViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string? Badge { get; set; }
    public string BadgeTone { get; set; } = "gray";
    public bool IsExternalLink { get; set; }
}

public class HoaDocumentViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-file-lines";
}
