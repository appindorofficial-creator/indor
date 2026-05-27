namespace IndorMvcApp.ViewModels;

public class EmergencyTreeDamageReviewViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public string UbicacionResumen { get; set; } = string.Empty;

    public string PeligroInmediatoResumen { get; set; } = string.Empty;

    public string RiesgoUtilidadResumen { get; set; } = string.Empty;

    public string AccesoCasaResumen { get; set; } = string.Empty;

    public string ArchivosResumen { get; set; } = string.Empty;

    public string TelefonoContacto { get; set; } = string.Empty;

    public string TiempoLlegadaRango { get; set; } = "45–90 min";

    public string? NotaCorta { get; set; }
}
