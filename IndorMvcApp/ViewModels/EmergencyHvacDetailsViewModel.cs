using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyHvacDetailsViewModel
{
    public int ServicioEmergenciaId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "NotCooling";

    [Required, MaxLength(20)]
    public string SucedeAhora { get; set; } = "Yes";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Emergency";
}
