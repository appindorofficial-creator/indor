using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class RoofInspectionServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Roof Inspection";
    public string LandingTitulo { get; set; } = string.Empty;
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public List<RoofInspectionFeatureItemViewModel> RecommendationItems { get; set; } = new();
    public List<RoofInspectionFeatureItemViewModel> CheckItems { get; set; } = new();
    public string? TrustTexto { get; set; }
    public string CtaTexto { get; set; } = "Set roof check";
}

public class RoofInspectionFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class RoofInspectionDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Roof Inspection";

    [Required]
    public string MotivoRevision { get; set; } = "RoutineInspection";

    [Required]
    public string TipoTecho { get; set; } = "AsphaltShingle";

    [Required]
    public string EdadTecho { get; set; } = "NotSure";

    [Required]
    public string UltimaInspeccion { get; set; } = "DontKnow";

    public List<ExistingRoofInspectionFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingRoofInspectionFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class RoofInspectionScheduleViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Roof Inspection";
    public string ScheduleIntro { get; set; } = string.Empty;

    [Required]
    public string TipoServicio { get; set; } = "ReminderOnly";

    [Required]
    public string Frecuencia { get; set; } = "Yearly";

    [Required]
    public string TimingPreferido { get; set; } = "Spring";

    public DateTime? FechaPreferida { get; set; }

    [MaxLength(300)]
    public string? Notas { get; set; }

    public List<RoofInspectionFeatureItemViewModel> CoverageItems { get; set; } = new();
    public string? TrustTexto { get; set; }
}

public class RoofInspectionConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Roof Inspection";
    public string FrequencyLabel { get; set; } = string.Empty;
    public string TimingLabel { get; set; } = string.Empty;
    public string PropertyLabel { get; set; } = string.Empty;
    public string LastInspectionLabel { get; set; } = string.Empty;
    public string FocusLabel { get; set; } = string.Empty;
}
