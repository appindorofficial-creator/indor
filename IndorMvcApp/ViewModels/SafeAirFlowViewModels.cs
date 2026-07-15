using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class SafeAirServiceViewModel
{
    public int MicroservicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Safe Air 365";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? PrecioTexto { get; set; }
    public List<SafeAirFeatureItemViewModel> IncludedItems { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string CtaScheduleTexto { get; set; } = "Schedule with INDOR";
    public string CtaChangedTexto { get; set; } = "I changed it myself";
}

public class SafeAirFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class SafeAirDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Safe Air 365";

    [Required(ErrorMessage = "Select what you need today.")]
    public string TipoNecesidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select how many filters.")]
    public string CantidadFiltros { get; set; } = string.Empty;

    public decimal? FiltroAncho { get; set; }
    public decimal? FiltroAlto { get; set; }
    public decimal? FiltroProfundidad { get; set; }
    public bool FiltroTamanioDesconocido { get; set; }

    [Required(ErrorMessage = "Select where the filter is located.")]
    public string UbicacionFiltro { get; set; } = string.Empty;

    public string? ProveedorFiltro { get; set; }
    public bool RecordatorioActivo { get; set; }
}

public class SafeAirScheduleViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string PageTitle { get; set; } = "Safe Air 365";
    public string TipoNecesidad { get; set; } = string.Empty;

    public string VentanaTiempo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select access details.")]
    public string DetallesAcceso { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotasAcceso { get; set; }

    public string NombreServicio { get; set; } = "Safe Air 365";
    public string CantidadFiltrosLabel { get; set; } = string.Empty;
    public string FiltroTamanioLabel { get; set; } = string.Empty;
    public string ProveedorFiltroLabel { get; set; } = string.Empty;
    public string RecordatorioLabel { get; set; } = string.Empty;
    public bool ShowScheduleOptions { get; set; } = true;
    public List<ExistingSafeAirFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingSafeAirFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class SafeAirConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int MicroservicioId { get; set; }
    public string NombreServicio { get; set; } = "Safe Air 365";
    public string TipoNecesidad { get; set; } = "IndorReplaces";
    public string VisitaLabel { get; set; } = string.Empty;
    public string FiltroTamanioLabel { get; set; } = string.Empty;
    public string ProveedorLabel { get; set; } = "INDOR partner";
    public string RecordatorioLabel { get; set; } = string.Empty;
    public DateTime? FechaProximoRecordatorio { get; set; }
    public bool TieneFotos { get; set; }
    public bool RecordatorioActivo { get; set; }
}
