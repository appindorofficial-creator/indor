namespace IndorMvcApp.ViewModels;

public class StructuralSignsViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string? SignosVisibles { get; set; }
    public string? SeveridadApariencia { get; set; } = "Moderate";
}
