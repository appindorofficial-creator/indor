using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class LawnServiceViewModel
{
    public int MicroservicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Always Perfect Lawn";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? PrecioTexto { get; set; }
    public List<LawnFeatureItemViewModel> IncludedItems { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string CtaTexto { get; set; } = "Customize service";
}

public class LawnFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class LawnSetupViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Lawn Service Setup";

    [Required]
    public string TipoServicio { get; set; } = "Subscription";

    [Required]
    public string Frecuencia { get; set; } = "Biweekly";

    [Required]
    public string AreaServicio { get; set; } = "FrontBack";

    public decimal EstimatedTotal { get; set; }
    public List<LawnAreaCardViewModel> AreaOptions { get; set; } = new();
}

public class LawnAreaCardViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Icon { get; set; } = "fa-house";
    public bool IsCustomQuote { get; set; }
    public string PriceLabel => IsCustomQuote ? "custom quote" : $"from ${Price:0}";
}

public class LawnAddonsViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Customize add-ons";
    public string TipoServicio { get; set; } = "Subscription";
    public string AreaServicio { get; set; } = "FrontBack";

    public string AddonsSeleccionados { get; set; } = string.Empty;

    public string PreferenciaExtra { get; set; } = "NoThanks";

    public decimal PrecioBase { get; set; }
    public decimal PrecioAddons { get; set; }
    public decimal DescuentoSuscripcion { get; set; }
    public decimal PrecioTotal { get; set; }
    public string AreaLabel { get; set; } = string.Empty;
    public List<LawnAddonCardViewModel> AddonOptions { get; set; } = new();
    public List<LawnLineItemViewModel> SelectedAddonLines { get; set; } = new();
}

public class LawnAddonCardViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Icon { get; set; } = "fa-plus";
    public bool Selected { get; set; }
}

public class LawnLineItemViewModel
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Icon { get; set; } = "fa-check";
}

public class LawnReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Review your booking";
    public string? ImagenUrl { get; set; }
    public string SubscriptionLabel { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public string AddonsLabel { get; set; } = string.Empty;
    public string PreferredDayLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public DateTime FechaPreferida { get; set; }

    [Required]
    public string VentanaHorario { get; set; } = "Morning8_11";

    public decimal PrecioBase { get; set; }
    public decimal PrecioAddons { get; set; }
    public decimal DescuentoSuscripcion { get; set; }
    public decimal PrecioTotal { get; set; }
    public List<LawnLineItemViewModel> AddonLines { get; set; } = new();
    public List<LawnDateOptionViewModel> DateOptions { get; set; } = new();
}

public class LawnDateOptionViewModel
{
    public DateTime Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public bool Selected { get; set; }
}

public class LawnConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string NombreServicio { get; set; } = "Always Perfect Lawn";
    public string SubscriptionLabel { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public string AddonsLabel { get; set; } = string.Empty;
    public string ScheduledLabel { get; set; } = string.Empty;
    public decimal PrecioTotal { get; set; }
    public string Moneda { get; set; } = "USD";
}
