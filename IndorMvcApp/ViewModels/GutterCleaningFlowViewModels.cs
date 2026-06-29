using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class GutterCleaningFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
    public string? Subtext { get; set; }
}

public class GutterCleaningServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Gutter Cleaning";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string? InfoBoxTexto { get; set; }
    public string? ImagenUrl { get; set; }
    public List<GutterCleaningFeatureItemViewModel> WhyItMattersItems { get; set; } = new();
    public string CtaTexto { get; set; } = "Continue";

    [Required]
    public string TipoAccionInicial { get; set; } = "Reminder";
}

public class GutterCleaningSetupViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Gutter Cleaning";

    [Required(ErrorMessage = "Select your home size.")]
    public string NumeroPisos { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select your gutter type.")]
    public string TipoCanaletas { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select whether you have gutter guards.")]
    public string ProtectorCanaletas { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select when gutters were last cleaned.")]
    public string UltimaLimpieza { get; set; } = string.Empty;

    [Range(0, 50)]
    public int? CantidadBajantes { get; set; }
}

public class ExistingGutterCleaningFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class GutterCleaningPreferencesViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Gutter Cleaning";

    [Required(ErrorMessage = "Select at least one issue you've noticed.")]
    public string ProblemasSeleccionados { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select a problem area.")]
    public string AreaProblema { get; set; } = "WholeHouse";

    [Required(ErrorMessage = "Select what you want today.")]
    public string ObjetivoHoy { get; set; } = "ScheduleService";

    [Required(ErrorMessage = "Select a reminder preference.")]
    public string PreferenciaRecordatorio { get; set; } = "SpringFall";

    public DateTime? FechaRecordatorioPersonalizada { get; set; }

    [MaxLength(300)]
    public string? Notas { get; set; }

    public List<ExistingGutterCleaningFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class GutterCleaningConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Gutter Cleaning";
    public bool AlreadyCompleted { get; set; }
    public string FrequencyLabel { get; set; } = string.Empty;
    public string HomeLabel { get; set; } = string.Empty;
    public string GutterGuardsLabel { get; set; } = string.Empty;
    public string LastCleanedLabel { get; set; } = string.Empty;
    public string NeedLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public string? NextReminderLabel { get; set; }
    public string? PreferredVisitLabel { get; set; }
    public bool ShowServiceRequested { get; set; }
    public List<GutterCleaningFeatureItemViewModel> NextStepsItems { get; set; } = new();
    public List<GutterCleaningFeatureItemViewModel> RecommendedTimingItems { get; set; } = new();
    public string? InfoConfirmacionTexto { get; set; }
}
