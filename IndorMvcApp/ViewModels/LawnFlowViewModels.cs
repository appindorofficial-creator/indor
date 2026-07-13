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
    public List<LawnFeatureItemViewModel> IncludedItems { get; set; } = [];
    public string? InfoBoxTexto { get; set; }
    public string CtaTexto { get; set; } = "Customize service";
    public string ReminderBannerTitulo { get; set; } = "Automatic reminder";
    public string ReminderBannerTexto { get; set; } = string.Empty;
    public string RemindOnlyCtaTexto { get; set; } = "Only remind me";
    public bool RecordatorioActivo { get; set; }
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
    public string PageTitle { get; set; } = "Configure your service";
    public bool IsReminderOnly { get; set; }

    [Required]
    public string Frecuencia { get; set; } = string.Empty;

    [Required]
    public string AreaServicio { get; set; } = string.Empty;

    public string AddonsSeleccionados { get; set; } = string.Empty;
    public decimal EstimatedTotal { get; set; }
    public List<LawnOptionCardViewModel> FrequencyOptions { get; set; } = [];
    public List<LawnAreaCardViewModel> AreaOptions { get; set; } = [];
    public List<LawnAddonCardViewModel> AddonOptions { get; set; } = [];
}

public class LawnOptionCardViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-circle";
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

public class LawnAddonCardViewModel
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Icon { get; set; } = "fa-plus";
    public bool Selected { get; set; }
}

public class LawnScheduleViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Schedule your service";
    public bool IsReminderOnly { get; set; }
    public string? ImagenUrl { get; set; }
    public string FrequencyLabel { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public decimal PrecioTotal { get; set; }

    [Required]
    public DateTime FechaPreferida { get; set; }

    [Required]
    public string VentanaHorario { get; set; } = string.Empty;

    public bool RecordatorioActivo { get; set; }
    public string Frecuencia { get; set; } = string.Empty;
    public int RecordatorioAvisoDias { get; set; }
    public string RecordatorioCanales { get; set; } = string.Empty;
    public List<LawnDateOptionViewModel> DateOptions { get; set; } = [];
    public List<LawnOptionCardViewModel> TimeWindowOptions { get; set; } = [];
    public List<LawnOptionCardViewModel> ReminderLeadOptions { get; set; } = [];
    public List<LawnOptionCardViewModel> ReminderChannelOptions { get; set; } = [];
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
    public string PageTitle { get; set; } = "Review and confirm";
    public bool IsReminderOnly { get; set; }
    public string ServiceName { get; set; } = "Always Perfect Lawn";
    public string FrequencyLabel { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public string AddonsLabel { get; set; } = string.Empty;
    public string ScheduledLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal PrecioAddons { get; set; }
    public decimal DescuentoSuscripcion { get; set; }
    public decimal PrecioTotal { get; set; }
    public List<LawnLineItemViewModel> AddonLines { get; set; } = [];
}

public class LawnDateOptionViewModel
{
    public DateTime Date { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public string DateLabel { get; set; } = string.Empty;
    public string MonthLabel { get; set; } = string.Empty;
    public bool Selected { get; set; }
}

public class LawnConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public bool IsReminderOnly { get; set; }
    public string NombreServicio { get; set; } = "Always Perfect Lawn";
    public string FrequencyLabel { get; set; } = string.Empty;
    public string AreaLabel { get; set; } = string.Empty;
    public string AddonsLabel { get; set; } = string.Empty;
    public string ScheduledLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public string NextReminderLabel { get; set; } = string.Empty;
    public string NotificationMethodLabel { get; set; } = string.Empty;
    public bool RecordatorioActivo { get; set; }
    public decimal PrecioTotal { get; set; }
    public string Moneda { get; set; } = "USD";
}
