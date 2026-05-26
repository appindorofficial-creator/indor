using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PlumbingUploadViewModel
{
    public int SolicitudId { get; set; }

    public int InspeccionId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [Display(Name = "Comments for the provider")]
    [MaxLength(1000)]
    public string? ComentariosProveedor { get; set; }

    public List<ExistingPlumbingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingPlumbingFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}
