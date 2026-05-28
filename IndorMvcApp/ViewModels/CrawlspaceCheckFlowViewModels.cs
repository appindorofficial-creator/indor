using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class CrawlspaceCheckServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Crawlspace Check";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public List<CrawlspaceCheckFeatureItemViewModel> CheckItems { get; set; } = new();
    public string? BestPracticeTexto { get; set; }
    public string CtaTexto { get; set; } = "Start crawlspace check";
}

public class CrawlspaceCheckFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class CrawlspaceCheckSetupViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Crawlspace Check";

    [Required]
    public string Encapsulacion { get; set; } = "NotSure";

    [Required]
    public string Aislamiento { get; set; } = "Yes";

    [Required]
    public string BarreraVapor { get; set; } = "Yes";

    [Required]
    public string TipoAcceso { get; set; } = "InteriorHatch";

    [Required]
    public string UltimaRevision { get; set; } = "NotSure";
}

public class CrawlspaceCheckScheduleViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Crawlspace Check";

    public string PreocupacionesSeleccionadas { get; set; } = string.Empty;

    [Required]
    public string TimingPreferido { get; set; } = "AsSoonAsPossible";

    [Required]
    public DateTime FechaPreferida { get; set; }

    [MaxLength(300)]
    public string? Notas { get; set; }

    public List<CrawlspaceCheckFeatureItemViewModel> ConcernOptions { get; set; } = new();
    public string? TipTexto { get; set; }
    public string? ResumenServicioTexto { get; set; }
}

public class CrawlspaceCheckConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Crawlspace Check";
    public string EncapsulacionLabel { get; set; } = string.Empty;
    public string AislamientoLabel { get; set; } = string.Empty;
    public string BarreraVaporLabel { get; set; } = string.Empty;
    public string ConcernsLabel { get; set; } = string.Empty;
    public string TimingLabel { get; set; } = string.Empty;
    public string ReminderLabel { get; set; } = string.Empty;
    public string? FrequencyTexto { get; set; }
    public string? ResumenServicioTexto { get; set; }
}
