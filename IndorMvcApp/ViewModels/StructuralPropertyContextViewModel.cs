namespace IndorMvcApp.ViewModels;

public class StructuralPropertyContextViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;

    public string MotivoRevision { get; set; } = "BeforePurchase";
    public string? TipoPropiedad { get; set; } = "SingleFamily";
    public string? EdadPropiedad { get; set; }
    public string? TipoFundacion { get; set; } = "CrawlSpace";
    public string? TieneReporte { get; set; } = "No";
    public string? CambiosRecientes { get; set; }
    public string? AccesoPreferido { get; set; } = "SomeoneHome";
    public string? AreasEnfoque { get; set; }
}
