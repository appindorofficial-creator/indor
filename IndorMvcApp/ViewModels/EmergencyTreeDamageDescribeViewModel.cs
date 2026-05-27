using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyTreeDamageDescribeViewModel
{
    public int ServicioEmergenciaId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "FallenBranch";

    [Required, MaxLength(30)]
    public string UbicacionDanio { get; set; } = "FrontYard";

    [Required, MaxLength(20)]
    public string PeligroInmediato { get; set; } = "NotSure";

    [Required, MaxLength(30)]
    public string RiesgoUtilidad { get; set; } = "NotSure";
}
