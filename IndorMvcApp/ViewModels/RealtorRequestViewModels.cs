using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class RealtorRequestFormViewModel
{
    public int? PropiedadId { get; set; }

    [Required(ErrorMessage = "Select what you need.")]
    public string NeedType { get; set; } = "Buy";

    [MaxLength(120)]
    public string? PreferredArea { get; set; }

    [Required]
    public string Timeframe { get; set; } = "ASAP";

    [MaxLength(80)]
    public string? PriceRange { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class RealtorRequestSentViewModel
{
    public int SolicitudId { get; set; }

    public string NeedLabel { get; set; } = string.Empty;

    public string AreaLabel { get; set; } = "—";

    public string TimeframeLabel { get; set; } = string.Empty;

    public string StatusLabel { get; set; } = "Matching in progress";
}
