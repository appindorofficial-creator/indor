namespace IndorMvcApp.ViewModels;

public class ExistingHomeSafetyFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class HomeSafetyUploadViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string? ComentariosProveedor { get; set; }
    public List<ExistingHomeSafetyFileViewModel> ArchivosExistentes { get; set; } = new();
}
