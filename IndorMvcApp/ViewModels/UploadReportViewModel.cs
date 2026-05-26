using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class UploadReportViewModel
{
    public int SolicitudId { get; set; }

    public int InspeccionId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [Display(Name = "What would you like us to review first?")]
    [MaxLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
    public string? NotasRevision { get; set; }

    public List<ExistingReportFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingReportFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}
