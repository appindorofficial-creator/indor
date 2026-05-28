using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class WaterHeaterFlushServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Water Heater Flush";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public string? PrecioTexto { get; set; }
    public List<WaterHeaterFlushFeatureItemViewModel> BenefitItems { get; set; } = new();
    public List<WaterHeaterFlushFeatureItemViewModel> PreviewItems { get; set; } = new();
    public string? WhyItMattersTexto { get; set; }
    public string CtaTexto { get; set; } = "Continue";
}

public class WaterHeaterFlushFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class WaterHeaterFlushSetupViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Water Heater Flush";

    [Required]
    public string TipoCalentador { get; set; } = "Tank";

    [Required]
    public string FuenteEnergia { get; set; } = "Electric";

    [MaxLength(80)]
    public string? NumeroSerie { get; set; }

    public bool SerialDesconocido { get; set; }

    [MaxLength(80)]
    public string? MarcaModelo { get; set; }

    [Required]
    public string Ubicacion { get; set; } = "Garage";

    public List<ExistingWaterHeaterFlushFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingWaterHeaterFlushFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class WaterHeaterFlushDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Water Heater Flush";

    [Required]
    public string UltimoFlush { get; set; } = "NotSure";

    public string SintomasSeleccionados { get; set; } = "NoIssues";

    [Required]
    public string TipoServicio { get; set; } = "OneTime";

    [Required]
    public string PreferenciaTiempo { get; set; } = "NextAvailable";

    public DateTime? FechaVisita { get; set; }

    [MaxLength(200)]
    public string? NotasAdicionales { get; set; }

    public string? ResumenServicioTexto { get; set; }
    public decimal PrecioEstimado { get; set; }
    public string? PrecioTexto { get; set; }
}

public class WaterHeaterFlushConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Water Heater Flush";
    public string FrequencyLabel { get; set; } = string.Empty;
    public string PreferredTimeLabel { get; set; } = string.Empty;
    public string UnitInfoLabel { get; set; } = "Complete";
    public decimal PrecioEstimado { get; set; }
    public string Moneda { get; set; } = "USD";
}
