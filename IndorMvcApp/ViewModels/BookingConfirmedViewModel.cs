namespace IndorMvcApp.ViewModels;

public class BookingConfirmedViewModel
{
    public int SolicitudId { get; set; }

    /// <summary>purchase | electrical | complete</summary>
    public string FlowType { get; set; } = string.Empty;

    public string NombreServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public DateTime FechaCita { get; set; }

    public string HoraCita { get; set; } = string.Empty;

    public string ResumenEtiqueta { get; set; } = "Concern";

    public string ResumenPreocupacion { get; set; } = string.Empty;

    public string InfoMensaje { get; set; } =
        "You'll receive updates at every step. Your provider will review your photos and comments before arriving.";

    public string Estado { get; set; } = "Confirmed";
}
