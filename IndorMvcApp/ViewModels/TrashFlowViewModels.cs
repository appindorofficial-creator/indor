using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class TrashServiceViewModel
{
    public int MicroservicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Trash Day Assistant";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? PrecioTexto { get; set; }
    public List<TrashFeatureItemViewModel> IncludedItems { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string CtaTexto { get; set; } = "Activate service";
}

public class TrashFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class TrashSetupViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Trash Day Assistant";

    [Required]
    public string BinsSeleccionados { get; set; } = string.Empty;

    [Required]
    public string CantidadBins { get; set; } = string.Empty;

    [Required]
    public string Frecuencia { get; set; } = string.Empty;

    [Required]
    public string DiaRecoleccion { get; set; } = string.Empty;

    public decimal PrecioMensual { get; set; }
}

public class TrashHelpViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Trash Day Assistant";

    [Required]
    public string TipoAyuda { get; set; } = string.Empty;

    [Required]
    public string RecordatorioCuando { get; set; } = string.Empty;

    [Required]
    public string VentanaRecoleccion { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotasEspeciales { get; set; }
}

public class TrashReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Trash Day Assistant";
    public string BinsLabel { get; set; } = string.Empty;
    public string ServiceTypeLabel { get; set; } = string.Empty;
    public string FrequencyLabel { get; set; } = string.Empty;
    public string PickupDayLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public string PickupWindowLabel { get; set; } = string.Empty;
    public string? NotasEspeciales { get; set; }
    public decimal PrecioMensual { get; set; }
    public string Moneda { get; set; } = "USD";
    public string? InfoBoxTexto { get; set; }
}

public class TrashConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Trash Day Assistant";
    public string BinsLabel { get; set; } = string.Empty;
    public string ServiceTypeLabel { get; set; } = string.Empty;
    public string FrequencyLabel { get; set; } = string.Empty;
    public string PickupDayLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public decimal PrecioMensual { get; set; }
    public string Moneda { get; set; } = "USD";
}
