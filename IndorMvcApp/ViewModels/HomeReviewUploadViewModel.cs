using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class HomeReviewUploadViewModel
{
    public int SolicitudId { get; set; }

    public int InspeccionId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [Display(Name = "Comments for the provider")]
    [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters.")]
    public string? ComentariosProveedor { get; set; }

    public List<ExistingHomeReviewFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingHomeReviewFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}
