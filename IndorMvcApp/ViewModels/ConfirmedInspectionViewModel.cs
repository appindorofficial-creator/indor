namespace IndorMvcApp.ViewModels;

public class ConfirmedInspectionViewModel
{
    public string FlowType { get; set; } = string.Empty;

    public int SolicitudId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public DateTime FechaCita { get; set; }

    public string HoraCita { get; set; } = string.Empty;

    public string ResumenPreocupacion { get; set; } = string.Empty;
}
