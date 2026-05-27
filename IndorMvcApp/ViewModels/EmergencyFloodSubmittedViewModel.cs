namespace IndorMvcApp.ViewModels;

public class EmergencyFloodSubmittedViewModel
{
    public int SolicitudId { get; set; }

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string AreaResumen { get; set; } = string.Empty;

    public string CausaAguaResumen { get; set; } = string.Empty;

    public string PuedeCerrarAguaResumen { get; set; } = string.Empty;

    public string EstadoResumen { get; set; } = string.Empty;

    public int TiempoLlegadaMinutos { get; set; } = 45;

    public string TiempoLlegadaRango { get; set; } = "35-50 min";
}
