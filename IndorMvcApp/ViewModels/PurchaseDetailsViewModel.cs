using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PurchaseDetailsViewModel
{
    public int InspeccionId { get; set; }

    public int? SolicitudId { get; set; }

    public string NombreInspeccion { get; set; } = string.Empty;

    public string? SubtituloInspeccion { get; set; }

    [Required(ErrorMessage = "Property address is required.")]
    [Display(Name = "Property address")]
    [MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Display(Name = "Are you under contract?")]
    public bool BajoContrato { get; set; } = true;

    [Display(Name = "Estimated closing date")]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? FechaCierreEstimada { get; set; }

    [Display(Name = "Do you already have a home inspection report?")]
    public bool TieneReporteExistente { get; set; }

    [Required(ErrorMessage = "Please select who you are.")]
    [Display(Name = "Who are you?")]
    public string RolComprador { get; set; } = "Buyer";

    [Required(ErrorMessage = "Please select your main goal.")]
    [Display(Name = "Main goal")]
    public string ObjetivoPrincipal { get; set; } = "UnderstandRepairRisks";
}
