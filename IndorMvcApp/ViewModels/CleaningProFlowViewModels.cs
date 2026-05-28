using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class CleaningProServiceViewModel
{
    public int MicroservicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Cleaning Pro";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? PrecioTexto { get; set; }
    public List<CleaningProFeatureItemViewModel> IncludedItems { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string CtaTexto { get; set; } = "Customize cleaning";
}

public class CleaningProFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class CleaningProSetupViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Cleaning Pro";

    [Required]
    public string Frecuencia { get; set; } = "OneTime";

    [Required]
    public string CantidadLimpiadores { get; set; } = "One";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;
}

public class CleaningProCustomizeViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Customize your cleaning";

    [Required]
    public string Frecuencia { get; set; } = "OneTime";

    [Required]
    public string CantidadLimpiadores { get; set; } = "Two";

    [Required]
    public string AreasLimpieza { get; set; } = "Bathrooms|Kitchen|LivingRoom|Baseboards|Floors|InsideFridge|Windows|Dusting";

    [Required]
    public decimal HorasEstimadas { get; set; } = 3m;

    public string AddonsSeleccionados { get; set; } = string.Empty;
    public string SummaryLine { get; set; } = string.Empty;
    public decimal FromTotal { get; set; }
}

public class CleaningProReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Review and pricing summary";
    public string NombreServicio { get; set; } = "Cleaning Pro";
    public string FrequencyLabel { get; set; } = string.Empty;
    public string AreasLabel { get; set; } = string.Empty;
    public string CrewLabel { get; set; } = string.Empty;
    public string HoursLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public DateTime FechaServicio { get; set; }

    [Required]
    public string VentanaHorario { get; set; } = "Morning10";

    [MaxLength(500)]
    public string? NotasLimpiador { get; set; }

    public string AddonsSeleccionados { get; set; } = string.Empty;
    public decimal TarifaHoraria { get; set; }
    public decimal HorasEstimadas { get; set; }
    public decimal ServiceSubtotal { get; set; }
    public decimal AddonsTotal { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ImpuestoVenta { get; set; }
    public decimal PrecioTotal { get; set; }
    public List<CleaningProAddonLineViewModel> AddonLines { get; set; } = new();
}

public class CleaningProAddonLineViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class CleaningProConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string NombreServicio { get; set; } = "Cleaning Pro";
    public string CrewSummary { get; set; } = string.Empty;
    public string HoursLabel { get; set; } = string.Empty;
    public decimal ServiceEstimate { get; set; }
    public string FrequencyLabel { get; set; } = string.Empty;
    public string ScheduledLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public decimal PrecioTotal { get; set; }
    public string Moneda { get; set; } = "USD";
}
