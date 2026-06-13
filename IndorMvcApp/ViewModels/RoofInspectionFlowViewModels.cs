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
    public string? Subtext { get; set; }
}

public class RoofInspectionDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Roof Inspection";

    [Required(ErrorMessage = "Please select why you need a roof check.")]
    public string MotivoRevision { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your roof type.")]
    public string TipoTecho { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please indicate the approximate age of your roof.")]
    public string EdadTecho { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please indicate when your roof was last inspected.")]
    public string UltimaInspeccion { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "Please choose a service.")]
    public string TipoServicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select how often you'd like a roof check.")]
    public string Frecuencia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your preferred timing.")]
    public string TimingPreferido { get; set; } = string.Empty;

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
