namespace IndorMvcApp.ViewModels;

public class EmergencyElectricalSubmittedViewModel
{
    public int SolicitudId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public string AreaResumen { get; set; } = string.Empty;

    public string EnergiaEncendidaResumen { get; set; } = string.Empty;

    public string PuedeApagarBreakerResumen { get; set; } = string.Empty;

    public string EstadoResumen { get; set; } = string.Empty;

    public string CallbackRango { get; set; } = "5–10 min";

    public string TiempoLlegadaRango { get; set; } = "45–60 min";
}
