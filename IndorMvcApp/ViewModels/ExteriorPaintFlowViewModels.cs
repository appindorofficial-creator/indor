using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class ExteriorPaintFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class ExteriorPaintReviewViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Exterior Paint Review";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string? InfoBoxTexto { get; set; }
    public string? ImagenUrl { get; set; }
    public string CtaTexto { get; set; } = "Continue";

    [Required(ErrorMessage = "Select when the exterior was last painted.")]
    public string UltimaPintura { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select your exterior surface type.")]
    public string TipoSuperficie { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select whether you want to keep the same color.")]
    public string MantenerMismoColor { get; set; } = string.Empty;
}

public class ExteriorPaintConditionViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Exterior Paint Review";
    public string? LandingSubtitulo { get; set; }
    public string? ImagenUrl { get; set; }

    [Required(ErrorMessage = "Select at least one issue you've noticed.")]
    public string ProblemasSeleccionados { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select at least one area that needs attention.")]
    public string AreasSeleccionadas { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select whether you want a color update.")]
    public string ActualizacionColor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select whether the exterior was power washed recently.")]
    public string LavadoPresionReciente { get; set; } = string.Empty;
}

public class ExistingExteriorPaintFileViewModel
{
    public int Id { get; set; }
    public string? CategoriaFoto { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class ExteriorPaintScheduleViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Exterior Paint Review";
    public string? LandingSubtitulo { get; set; }
    public string? ImagenUrl { get; set; }

    [Required]
    public string NumeroPisos { get; set; } = "One";

    [Required]
    public string TimingPreferido { get; set; } = "AsSoonAsPossible";

    [MaxLength(300)]
    public string? Notas { get; set; }

    public string SurfaceLabel { get; set; } = string.Empty;
    public string IssuesLabel { get; set; } = string.Empty;
    public string LastPaintedLabel { get; set; } = string.Empty;
    public string ScopeLabel { get; set; } = string.Empty;

    public List<ExistingExteriorPaintFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExteriorPaintConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Exterior Paint Review";
    public List<ExteriorPaintFeatureItemViewModel> WhyItMattersItems { get; set; } = new();
    public List<ExteriorPaintFeatureItemViewModel> NextStepsItems { get; set; } = new();
    public string? ReminderTexto { get; set; }
}
