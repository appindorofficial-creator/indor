namespace IndorMvcApp.ViewModels;

public class EmergencyRoofLeakSubmittedViewModel
{
    public int SolicitudId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public string AreaResumen { get; set; } = string.Empty;

    public string UrgenciaResumen { get; set; } = string.Empty;

    public string ArchivosResumen { get; set; } = string.Empty;

    public string TiempoLlegadaRango { get; set; } = "45–60 min";

    public string EstadoResumen { get; set; } = string.Empty;
}
