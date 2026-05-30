namespace IndorMvcApp.ViewModels;

public class PermitsImprovementsIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "permits";
    public string PageSubtitle { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public List<PermitsStatViewModel> Stats { get; set; } = new();
    public List<PermitsListItemViewModel> Items { get; set; } = new();
    public List<PermitsListItemViewModel> NeededItems { get; set; } = new();
    public List<PermitsListItemViewModel> HelpfulItems { get; set; } = new();
    public List<PermitsNoteCardViewModel> RecordedNotes { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string? MissingDocsBadge { get; set; }
}

public class PermitsStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class PermitsListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "orange";
    public bool LinkToDetail { get; set; }
}

public class PermitsNoteCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class PermitsDetailViewModel
{
    public int PropiedadId { get; set; }
    public string PermitId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PageSubtitle { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "overview";
    public List<PermitsSummaryCardViewModel> SummaryCards { get; set; } = new();
    public List<PermitsDetailRowViewModel> VerificationItems { get; set; } = new();
    public List<PermitsDetailRowViewModel> RecordItems { get; set; } = new();
    public string InfoBanner { get; set; } = string.Empty;
    public string? HistoryNote { get; set; }
    public string? DocumentsNote { get; set; }
}

public class PermitsSummaryCardViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string StatusTone { get; set; } = "default";
}

public class PermitsDetailRowViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "orange";
}
