namespace IndorMvcApp.ViewModels;

public class RoofDetailsViewModel
{
    public int InspeccionId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TiposProblema { get; set; } = "ActiveLeak";
    public string TipoProblema { get; set; } = "ActiveLeak";
    public string UbicacionProblema { get; set; } = "MainRoof";
    public string Urgencia { get; set; } = "Normal";
}
