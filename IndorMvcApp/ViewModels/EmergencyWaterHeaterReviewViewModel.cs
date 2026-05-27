namespace IndorMvcApp.ViewModels;

public class EmergencyWaterHeaterReviewViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    public string TipoUnidadResumen { get; set; } = string.Empty;

    public string UbicacionResumen { get; set; } = string.Empty;

    public string UrgenciaResumen { get; set; } = string.Empty;

    public string UnidadFuncionandoResumen { get; set; } = string.Empty;

    public string ArchivosResumen { get; set; } = string.Empty;

    public string DetallesAcceso { get; set; } = string.Empty;
}
