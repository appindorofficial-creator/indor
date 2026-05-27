using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyRoofLeakDescribeViewModel
{
    public int ServicioEmergenciaId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "ActiveDripping";

    [Required, MaxLength(30)]
    public string UbicacionFuga { get; set; } = "Ceiling";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Emergency";
}
