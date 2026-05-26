namespace IndorMvcApp.ViewModels;

public class ExistingWindowsInsulationFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class WindowsInsulationUploadViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string? ComentariosProveedor { get; set; }
    public List<ExistingWindowsInsulationFileViewModel> ArchivosExistentes { get; set; } = new();
}
