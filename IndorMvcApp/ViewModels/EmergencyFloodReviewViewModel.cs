namespace IndorMvcApp.ViewModels;

public class EmergencyFloodReviewViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string CausaAguaResumen { get; set; } = string.Empty;

    public string UbicacionAguaResumen { get; set; } = string.Empty;

    public string AguaActivaResumen { get; set; } = string.Empty;

    public string PuedeCerrarAguaResumen { get; set; } = string.Empty;

    public string UbicacionCierreAguaResumen { get; set; } = string.Empty;

    public string PuedeApagarElectricidadResumen { get; set; } = string.Empty;

    public string CantidadAguaResumen { get; set; } = string.Empty;

    public string ArchivosResumen { get; set; } = string.Empty;

    public string? NotaCorta { get; set; }
}
