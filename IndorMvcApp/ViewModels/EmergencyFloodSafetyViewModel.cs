using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyFloodSafetyViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PuedeCerrarAgua { get; set; } = "NotSure";

    [Required, MaxLength(20)]
    public string UbicacionCierreAgua { get; set; } = "DontKnow";

    [Required, MaxLength(20)]
    public string PuedeApagarElectricidad { get; set; } = "NotSure";

    [Required, MaxLength(20)]
    public string CantidadAgua { get; set; } = "OneRoom";
}
