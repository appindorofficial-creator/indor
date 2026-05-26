using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class StructuralUploadViewModel
{
    public int SolicitudId { get; set; }

    public int InspeccionId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [Display(Name = "Comments for the provider")]
    [MaxLength(1000)]
    public string? ComentariosProveedor { get; set; }

    public string? TipoPropiedad { get; set; }

    public List<ExistingStructuralFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingStructuralFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}
