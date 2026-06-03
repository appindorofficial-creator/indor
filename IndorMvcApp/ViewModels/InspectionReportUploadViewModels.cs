using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class UploadInspectionReportViewModel
{
    public int PropiedadId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/welcome-house.png";

    [Required(ErrorMessage = "Enter a document title.")]
    [MaxLength(200)]
    [Display(Name = "Document title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select an inspection date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Inspection date")]
    public DateTime? InspectionDate { get; set; }

    [Required(ErrorMessage = "Select a category.")]
    [MaxLength(40)]
    [Display(Name = "Document category")]
    public string Category { get; set; } = "Inspections";

    [MaxLength(300)]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    public string? SelectedFileName { get; set; }

    public string? SelectedFileSizeLabel { get; set; }

    public IReadOnlyList<string> CategoryOptions { get; set; } = ["Inspections"];
}

public class ReviewInspectionReportViewModel
{
    public int PropiedadId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "/welcome-house.png";

    public string Title { get; set; } = string.Empty;

    public DateTime? InspectionDate { get; set; }

    public string Category { get; set; } = "Inspections";

    public string CategoryLabel { get; set; } = "Inspection Report";

    public string? Notes { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string FileSizeLabel { get; set; } = string.Empty;

    public string FileTypeLabel { get; set; } = "PDF";
}

public class InspectionReportUploadedViewModel
{
    public int PropiedadId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string UploadedDateLabel { get; set; } = string.Empty;

    public string FileTypeLabel { get; set; } = "PDF";
}

public sealed class PendingInspectionUploadSession
{
    public int PropiedadId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime? InspectionDate { get; set; }

    public string Category { get; set; } = "Inspections";

    public string? Notes { get; set; }

    public string TempRelativePath { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }
}
