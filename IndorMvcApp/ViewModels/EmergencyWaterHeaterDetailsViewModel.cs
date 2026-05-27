using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyWaterHeaterDetailsViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string UbicacionProblema { get; set; } = "Garage";

    [Required, MaxLength(20)]
    public string TipoUnidad { get; set; } = "Gas";

    [MaxLength(300)]
    public string SintomasVisibles { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? NotaCorta { get; set; }
}
