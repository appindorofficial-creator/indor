namespace IndorMvcApp.ViewModels;

public class HomeSafetyDetailsViewModel
{
    public int InspeccionId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TiposProblema { get; set; } = "SmokeDetectorConcern";
    public string TipoProblema { get; set; } = "SmokeDetectorConcern";
    public string AreasAtencion { get; set; } = "Hallway";
    public string UbicacionProblema { get; set; } = "Hallway";
    public string Urgencia { get; set; } = "Normal";
    public string RiesgoActivo { get; set; } = "No";
}
