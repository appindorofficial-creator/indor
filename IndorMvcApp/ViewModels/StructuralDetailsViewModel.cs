using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class StructuralDetailsViewModel
{
    public int InspeccionId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property address is required.")]
    [MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? TiposPreocupacion { get; set; }

    [Required]
    public string TipoPreocupacion { get; set; } = "FoundationCrack";

    [Required]
    public string AreaPreocupacion { get; set; } = "Foundation";

    [Required]
    public string Urgencia { get; set; } = "Normal";

    [Required]
    public string DanoVisible { get; set; } = "Yes";
}
