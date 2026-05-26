using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PlumbingProblemDetailsViewModel
{
    public int SolicitudId { get; set; }

    public int InspeccionId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string TipoProblema { get; set; } = string.Empty;

    public string UbicacionProblema { get; set; } = string.Empty;

    public string ResumenProblema { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? SituacionesActuales { get; set; }

    [MaxLength(20)]
    public string? CuandoEmpezo { get; set; }

    [MaxLength(20)]
    public string? AguaCerrada { get; set; }

    [MaxLength(500)]
    [Display(Name = "Describe the problem")]
    public string? DescripcionProblema { get; set; }

    [MaxLength(200)]
    public string? NotasAdicionales { get; set; }
}
