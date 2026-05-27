namespace IndorMvcApp.ViewModels;

public class EmergencyWaterHeaterSubmittedViewModel
{
    public int SolicitudId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public DateTime FechaServicio { get; set; }

    public string HoraServicio { get; set; } = "ASAP";

    public string ProblemaResumen { get; set; } = string.Empty;

    public string UrgenciaResumen { get; set; } = string.Empty;

    public string EstadoResumen { get; set; } = "Confirmed";
}
