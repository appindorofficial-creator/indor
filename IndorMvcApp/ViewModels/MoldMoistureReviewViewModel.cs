namespace IndorMvcApp.ViewModels;

public class MoldMoistureReviewViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string SubtituloInspeccion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "USD";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public List<string> ProblemasPrincipales { get; set; } = new();
    public string PropiedadResumen { get; set; } = string.Empty;
    public string AccesoResumen { get; set; } = string.Empty;
    public string ArchivosResumen { get; set; } = string.Empty;
    public List<ExistingMoldMoistureFileViewModel> Archivos { get; set; } = new();
    public string? ComentariosProveedor { get; set; }
}
