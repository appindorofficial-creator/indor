using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class HvacDetailsViewModel
{
    public int InspeccionId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property address is required.")]
    [MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string TipoProblema { get; set; } = "NotCooling";

    [Required]
    public string ParteAtencion { get; set; } = "WholeSystem";

    [Required]
    public string Urgencia { get; set; } = "Normal";

    [Required]
    public string SistemaFuncionando { get; set; } = "Yes";
}
