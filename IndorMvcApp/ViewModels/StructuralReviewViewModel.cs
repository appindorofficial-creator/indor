namespace IndorMvcApp.ViewModels;

public class StructuralReviewViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string SubtituloInspeccion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = "USD";
    public string DireccionPropiedad { get; set; } = string.Empty;

    public List<string> Preocupaciones { get; set; } = new();
    public string CuandoNotadoResumen { get; set; } = string.Empty;
    public string SeveridadResumen { get; set; } = string.Empty;
    public string UrgenciaResumen { get; set; } = string.Empty;
    public string TipoFundacionResumen { get; set; } = string.Empty;
    public string ReparacionesPreviasResumen { get; set; } = string.Empty;
    public string MotivoResumen { get; set; } = string.Empty;
    public string TipoPropiedadResumen { get; set; } = string.Empty;
    public string ReporteResumen { get; set; } = string.Empty;
    public string AccesoResumen { get; set; } = string.Empty;
    public List<ExistingStructuralFileViewModel> Archivos { get; set; } = new();
    public string? ComentariosProveedor { get; set; }
}
