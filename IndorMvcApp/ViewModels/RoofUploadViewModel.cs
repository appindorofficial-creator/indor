namespace IndorMvcApp.ViewModels;

public class ExistingRoofFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class RoofUploadViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string? ComentariosProveedor { get; set; }
    public List<ExistingRoofFileViewModel> ArchivosExistentes { get; set; } = new();
}
