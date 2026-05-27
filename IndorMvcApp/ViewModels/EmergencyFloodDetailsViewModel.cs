using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyFloodDetailsViewModel
{
    public int ServicioEmergenciaId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string CausaAgua { get; set; } = "UnknownSource";

    [Required, MaxLength(30)]
    public string UbicacionAgua { get; set; } = "FirstFloor";

    [Required, MaxLength(20)]
    public string AguaActiva { get; set; } = "Yes";
}
