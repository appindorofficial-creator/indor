namespace IndorMvcApp.ViewModels;

public class PropertySnapshotViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ActiveTab { get; set; } = "overview";
    public string PageSubtitle { get; set; } = "Your property at a glance.";
    public string ConfidenceBadge { get; set; } = "Mostly estimated";
    public string ConfidenceLevel { get; set; } = "estimated";
    public int FieldCount { get; set; }
    public bool HasData { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public List<SnapshotStatViewModel> CoreStats { get; set; } = new();
    public List<SnapshotQuickLinkViewModel> QuickSections { get; set; } = new();
    public List<SnapshotHighlightViewModel> Highlights { get; set; } = new();
    public List<SnapshotFieldViewModel> DetailFields { get; set; } = new();
    public List<SnapshotFieldViewModel> LotFields { get; set; } = new();
    public SnapshotNotesViewModel Notes { get; set; } = new();
    public string NextTab { get; set; } = "details";
    public string NextTabLabel { get; set; } = "Next: Details";
}

public class SnapshotStatViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class SnapshotQuickLinkViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public string Tab { get; set; } = "overview";
}

public class SnapshotHighlightViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public bool IsEstimated { get; set; }
}

public class SnapshotFieldViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
    public bool IsEstimated { get; set; }
    public bool FullWidth { get; set; }
}

public class SnapshotNotesViewModel
{
    public string ConfidenceSummary { get; set; } = string.Empty;
    public List<SnapshotSourceViewModel> Sources { get; set; } = new();
    public List<SnapshotMissingItemViewModel> MissingItems { get; set; } = new();
}

public class SnapshotSourceViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-globe";
}

public class SnapshotMissingItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-question";
}
