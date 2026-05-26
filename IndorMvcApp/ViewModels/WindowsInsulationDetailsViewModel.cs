namespace IndorMvcApp.ViewModels;

public class WindowsInsulationDetailsViewModel
{
    public int InspeccionId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TiposProblema { get; set; } = "DraftAir";
    public string TipoProblema { get; set; } = "DraftAir";
    public string AreasAtencion { get; set; } = "LivingRoom";
    public string UbicacionProblema { get; set; } = "LivingRoom";
    public string Urgencia { get; set; } = "Normal";
    public string DanoHumedadVisible { get; set; } = "No";
}
