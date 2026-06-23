using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PestControlFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class PestControlServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Pest Control Check";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingSubtitulo { get; set; }
    public string? ImagenUrl { get; set; }
    public List<PestControlFeatureItemViewModel> WhyItMattersItems { get; set; } = new();
    public string? BestForTexto { get; set; }
    public string CtaTexto { get; set; } = "Continue";

    [Required]
    public string TipoAccionInicial { get; set; } = "Reminder";
}

public class PestControlSetupViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Pest Control Check";

    [Required(ErrorMessage = "Select when your last pest service was.")]
    public string UltimoServicio { get; set; } = string.Empty;

    public string SignosSeleccionados { get; set; } = string.Empty;
    public string AreasPreocupacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select whether you have pets or children at home.")]
    public string MascotasONinos { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Notas { get; set; }
}

public class PestControlPlanViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Pest Control Check";
    public string? InfoPlanTexto { get; set; }

    [Required]
    public string TipoServicio { get; set; } = "ReminderOnly";

    [Required]
    public string TimingPreferido { get; set; } = "ThisMonth";
}

public class PestControlConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Pest Control Check";
    public string ServiceLabel { get; set; } = string.Empty;
    public string TimingLabel { get; set; } = string.Empty;
    public string ConcernsLabel { get; set; } = string.Empty;
    public string PetsChildrenLabel { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = "Reminder and service saved";
    public List<PestControlFeatureItemViewModel> WhyYearlyItems { get; set; } = new();
}
