namespace IndorMvcApp.ViewModels;

public class DocumentsIndexViewModel
{
    public int PropiedadId { get; set; }
    public string Address { get; set; } = string.Empty;
    public int TotalDocuments { get; set; }
    public int PendingCount { get; set; }
    public int SharedCount { get; set; }
    public string ActiveTab { get; set; } = "all";
    public List<DocumentTabViewModel> Tabs { get; set; } = new();
    public List<DocumentListItemViewModel> Documents { get; set; } = new();
}

public class DocumentDetailViewModel
{
    public int PropiedadId { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string SizeLabel { get; set; } = string.Empty;
    public string PageCountLabel { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string UpdatedLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "green";
    public string Icon { get; set; } = "fa-file-lines";
    public string IconTone { get; set; } = "red";
    public string? StoragePath { get; set; }
    public List<DocumentDetailRowViewModel> Details { get; set; } = new();
    public List<string> AiSummary { get; set; } = new();
    public string PrimaryActionLabel { get; set; } = "Mark as reviewed";
}

public class DocumentAddViewModel
{
    public int PropiedadId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = "Report";
    public string RelatedSection { get; set; } = "Systems";
    public DateTime? DocumentDate { get; set; }
    public string? Notes { get; set; }
    public List<string> CategoryOptions { get; set; } = new();
    public List<string> SectionOptions { get; set; } = new();
}

public class DocumentRequestsViewModel
{
    public int PropiedadId { get; set; }
    public int PendingCount { get; set; }
    public int SharedCount { get; set; }
    public int RemindersSent { get; set; }
    public List<DocumentRequestItemViewModel> PendingItems { get; set; } = new();
    public List<DocumentRequestItemViewModel> SharedItems { get; set; } = new();
}

public class DocumentTabViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class DocumentListItemViewModel
{
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string CategoryLabel { get; set; } = string.Empty;
    public string UpdatedLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "green";
    public string Icon { get; set; } = "fa-file-lines";
    public string IconTone { get; set; } = "blue";
    public string TabGroup { get; set; } = "reports";
}

public class DocumentDetailRowViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle-info";
}

public class DocumentRequestItemViewModel
{
    public string RequestId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusTone { get; set; } = "orange";
    public string Icon { get; set; } = "fa-file-lines";
    public string IconTone { get; set; } = "orange";
    public string ActionLabel { get; set; } = string.Empty;
    public string ActionTone { get; set; } = "outline";
}
