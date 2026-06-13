using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public class HvacMaintenanceServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "HVAC Tune-Up";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? PrecioTexto { get; set; }
    public List<HvacMaintenanceFeatureItemViewModel> IncludedItems { get; set; } = new();
    public List<HvacMaintenanceFeatureItemViewModel> PreviewItems { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string CtaTexto { get; set; } = "Start tune-up request";
}

public class HvacMaintenanceFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class HvacMaintenanceDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "HVAC Tune-Up";

    [MaxLength(EquipmentSerialNumberAttribute.MaxLength)]
    [EquipmentSerialNumber]
    public string? NumeroSerieAc { get; set; }

    public bool SerialDesconocido { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Last maintenance")]
    public DateTime? FechaUltimoMantenimiento { get; set; }

    public bool UltimoMantenimientoDesconocido { get; set; }

    [MaxLength(40)]
    public string? TamanioFiltro { get; set; }

    [MaxLength(500)]
    public string? NotasTecnico { get; set; }

    public List<ExistingHvacMaintenanceFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingHvacMaintenanceFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class HvacMaintenanceScheduleViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "HVAC Tune-Up";

    [Required]
    public DateTime FechaVisita { get; set; }

    [Required]
    public string VentanaHorario { get; set; } = "Morning";

    [Required]
    public string TipoServicio { get; set; } = "OneTime";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string TelefonoContacto { get; set; } = string.Empty;

    public decimal PrecioEstimado { get; set; }
    public string? PrecioTexto { get; set; }
    public string? InfoBoxTexto { get; set; }
}

public class HvacMaintenanceConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string NombreServicio { get; set; } = "HVAC Tune-Up";
    public string SerialLabel { get; set; } = string.Empty;
    public string LastMaintenanceLabel { get; set; } = string.Empty;
    public string VisitWindowLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public decimal PrecioEstimado { get; set; }
    public string Moneda { get; set; } = "USD";
}
