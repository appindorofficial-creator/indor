namespace IndorMvcApp.ViewModels;

public class EmergencyHvacReviewViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public string EstadoResumen { get; set; } = string.Empty;

    public string UrgenciaResumen { get; set; } = string.Empty;

    public string TelefonoContacto { get; set; } = string.Empty;

    public string ArchivosResumen { get; set; } = string.Empty;

    public int TiempoLlegadaMinutos { get; set; } = 45;
}
