using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class HvacSystemDetailsViewModel
{
    public int SolicitudId { get; set; }

    public int InspeccionId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string TipoProblema { get; set; } = string.Empty;

    public string ParteAtencion { get; set; } = string.Empty;

    public string ResumenProblema { get; set; } = string.Empty;

    public string ResumenArea { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? TipoEquipo { get; set; }

    [MaxLength(10)]
    public string? CantidadSistemas { get; set; }

    [MaxLength(200)]
    public string? ComponentesRevision { get; set; }

    [MaxLength(20)]
    public string? EdadSistema { get; set; }

    [MaxLength(20)]
    public string? FiltroCambiado { get; set; }

    [MaxLength(20)]
    public string? TipoTermostato { get; set; }

    [MaxLength(500)]
    [Display(Name = "Describe the issue")]
    public string? DescripcionProblema { get; set; }

    [MaxLength(200)]
    public string? NotasOpcionales { get; set; }
}
