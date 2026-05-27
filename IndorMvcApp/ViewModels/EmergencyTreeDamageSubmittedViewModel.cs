namespace IndorMvcApp.ViewModels;

public class EmergencyTreeDamageSubmittedViewModel
{
    public int SolicitudId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public string TelefonoContacto { get; set; } = string.Empty;

    public string TiempoLlegadaRango { get; set; } = "45–90 min";

    public string EstadoResumen { get; set; } = string.Empty;
}
