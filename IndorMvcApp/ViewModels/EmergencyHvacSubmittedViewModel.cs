namespace IndorMvcApp.ViewModels;

public class EmergencyHvacSubmittedViewModel
{
    public int SolicitudId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public int TiempoLlegadaMinutos { get; set; } = 45;

    public string TelefonoContacto { get; set; } = string.Empty;

    public string EstadoResumen { get; set; } = "Searching for provider";
}
