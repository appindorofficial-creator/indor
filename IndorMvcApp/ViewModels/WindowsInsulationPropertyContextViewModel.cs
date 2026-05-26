namespace IndorMvcApp.ViewModels;

public class WindowsInsulationPropertyContextViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string MotivosRevision { get; set; } = "HighUtilityBill";
    public string MotivoRevision { get; set; } = "HighUtilityBill";
    public string TipoPropiedad { get; set; } = "SingleFamily";
    public string NumeroPisos { get; set; } = "TwoStory";
    public string AreasEnfoque { get; set; } = "Windows";
    public string AccesoPreferido { get; set; } = "SomeoneHome";
    public string TipoVentana { get; set; } = "DoublePane";
    public string AccesoAtticCrawlSpace { get; set; } = "Yes";
}
