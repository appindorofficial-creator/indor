using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PlumbingDetailsViewModel
{
    public int InspeccionId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property address is required.")]
    [MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string TipoProblema { get; set; } = "KitchenIssue";

    [Required]
    public string UbicacionProblema { get; set; } = "Kitchen";

    [Required]
    public string Urgencia { get; set; } = "Normal";

    [Required]
    public string FugaAguaAhora { get; set; } = "No";
}
