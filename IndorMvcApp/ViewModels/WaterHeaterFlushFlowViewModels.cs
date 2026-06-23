using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Validation;

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

    [Required(ErrorMessage = "Select a heater type.")]
    public string TipoCalentador { get; set; } = "";

    [Required(ErrorMessage = "Select a power source.")]
    public string FuenteEnergia { get; set; } = "";

    [MaxLength(80)]
    [EquipmentSerialNumber]
    public string? NumeroSerie { get; set; }

    public bool SerialDesconocido { get; set; }

    [MaxLength(80)]
    public string? MarcaModelo { get; set; }

    [Required(ErrorMessage = "Select a location.")]
    public string Ubicacion { get; set; } = "";

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

    [Required(ErrorMessage = "Select when the last flush was.")]
    public string UltimoFlush { get; set; } = "";

    public string SintomasSeleccionados { get; set; } = "";

    [Required(ErrorMessage = "Select a service preference.")]
    public string TipoServicio { get; set; } = "";

    [Required(ErrorMessage = "Select when works best for you.")]
    public string PreferenciaTiempo { get; set; } = "";

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
