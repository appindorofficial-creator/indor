using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class SmokeDetectorFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
    public string? Subtext { get; set; }
}

public class SmokeDetectorServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Smoke / CO Check";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingSubtitulo { get; set; }
    public string? ImagenUrl { get; set; }
    public List<SmokeDetectorFeatureItemViewModel> TrackItems { get; set; } = new();
    public List<SmokeDetectorFeatureItemViewModel> WhereTrackItems { get; set; } = new();
    public string? ReminderBannerTexto { get; set; }
    public string CtaTexto { get; set; } = "Start reminder setup";
}

public class SmokeDetectorSetupViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Smoke / CO Check";

    [Required(ErrorMessage = "Please select how many smoke alarms are in your home.")]
    public string CantidadAlarmas { get; set; } = string.Empty;

    public string UbicacionesSeleccionadas { get; set; } = string.Empty;
    public string TiposAlarmas { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please indicate when the alarms were last tested.")]
    public string UltimaPrueba { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please indicate when the battery was last changed.")]
    public string UltimoCambioBateria { get; set; } = string.Empty;

    public int? AnioInstalacion { get; set; }

    public bool AnioInstalacionDesconocido { get; set; }

    public string ProblemasSeleccionados { get; set; } = string.Empty;
}

public class SmokeDetectorRemindersViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Smoke / CO Check";
    public string AlarmCountLabel { get; set; } = string.Empty;
    public string AlarmTypeLabel { get; set; } = string.Empty;
    public string LocationsLabel { get; set; } = string.Empty;
    public string InstalledLabel { get; set; } = string.Empty;
    public string NextMonthlyTestLabel { get; set; } = string.Empty;
    public string NextBatteryLabel { get; set; } = string.Empty;
    public string NextReplacementLabel { get; set; } = string.Empty;
    public string NextSeasonalLabel { get; set; } = string.Empty;

    public bool RecordatorioMensual { get; set; } = true;
    public bool RecordatorioBateriaAnual { get; set; } = true;
    public bool RecordatorioReemplazo10Anos { get; set; } = true;
    public bool RecordatorioRevisionEstacional { get; set; } = true;

    [Required]
    public string TipoAccionAyuda { get; set; } = "ReminderOnly";
}

public class SmokeDetectorConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Smoke / CO Check";
    public string NextMonthlyTestLabel { get; set; } = string.Empty;
    public string NextBatteryLabel { get; set; } = string.Empty;
    public string ReplacementLabel { get; set; } = string.Empty;
    public bool ShowSafetyVisit { get; set; }
}
