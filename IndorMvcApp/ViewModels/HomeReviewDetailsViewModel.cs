using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class HomeReviewDetailsViewModel
{
    public int InspeccionId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property address is required.")]
    [MaxLength(300)]
    [Display(Name = "Property address")]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Why do you need this inspection?")]
    public string MotivoInspeccion { get; set; } = "BuyingHome";

    [Required(ErrorMessage = "Select at least one focus area.")]
    [Display(Name = "What would you like us to focus on?")]
    public string AreasEnfoque { get; set; } = "Electrical|HVAC|GeneralStructure";

    [MaxLength(50)]
    [Display(Name = "Property size")]
    public string? TamanoPropiedad { get; set; }

    [Required]
    [Display(Name = "Is anything urgent right now?")]
    public string EsUrgente { get; set; } = "No";
}
