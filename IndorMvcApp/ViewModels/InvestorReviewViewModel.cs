namespace IndorMvcApp.ViewModels;

public class InvestorReviewViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string SubtituloInspeccion { get; set; } = string.Empty;
    public string? FrecuenciaInspeccion { get; set; }
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "USD";
    public string PrecioPrefijo { get; set; } = "From";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string ObjetivoInversion { get; set; } = string.Empty;
    public string PropiedadResumen { get; set; } = string.Empty;
    public string NivelRehabResumen { get; set; } = string.Empty;
    public string AreasRevisionResumen { get; set; } = string.Empty;
    public string AccesoResumen { get; set; } = string.Empty;
    public string ArchivosResumen { get; set; } = string.Empty;
    public List<ExistingInvestorFileViewModel> Archivos { get; set; } = new();
    public string? ComentariosProveedor { get; set; }
}
