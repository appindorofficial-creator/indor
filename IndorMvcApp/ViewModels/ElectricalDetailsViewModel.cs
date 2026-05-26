using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class ElectricalDetailsViewModel
{
    public int InspeccionId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property address is required.")]
    [MaxLength(300)]
    [Display(Name = "Property address")]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Why do you need this review?")]
    public string MotivoRevision { get; set; } = "BuyingHome";

    [Required]
    [Display(Name = "What is the main concern?")]
    public string PreocupacionPrincipal { get; set; } = "GeneralReview";

    [Required]
    [Display(Name = "Is this happening right now?")]
    public string OcurreAhora { get; set; } = "No";

    [Required]
    [Display(Name = "How urgent is it?")]
    public string Urgencia { get; set; } = "Normal";
}
