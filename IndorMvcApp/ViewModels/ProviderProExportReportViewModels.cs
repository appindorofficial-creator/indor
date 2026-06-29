namespace IndorMvcApp.ViewModels;

public class ProviderProExportReportDraft
{
    public int? JobId { get; set; }
    public string? ReportName { get; set; }
    public string? ReportDate { get; set; }
    public string? PreparedBy { get; set; }
    public string? Category { get; set; }
    public string? Location { get; set; }
    public string? Priority { get; set; }
    public string? Weather { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<ProviderProUploadReportFileSlot> Photos { get; set; } = [];
}

public class ProviderProExportReportViewModel
{
    public ProviderProUploadPhotosJobSummary Job { get; set; } = new();
    public ProviderProExportReportDraft Draft { get; set; } = new();
}
