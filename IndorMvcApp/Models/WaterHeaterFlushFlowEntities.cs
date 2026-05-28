using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("WaterHeaterFlushServicioLanding")]
public class WaterHeaterFlushServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Water Heater Flush";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(400)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 79;

    [MaxLength(120)]
    public string? PrecioTexto { get; set; }

    [MaxLength(500)]
    public string? IncluyeItems { get; set; }

    [MaxLength(300)]
    public string? IncluyeIconos { get; set; }

    [MaxLength(500)]
    public string? PreviewItems { get; set; }

    [MaxLength(300)]
    public string? PreviewIconos { get; set; }

    [MaxLength(400)]
    public string? InfoBoxTexto { get; set; }

    [MaxLength(200)]
    public string? ResumenServicioTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Continue";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesWaterHeaterFlush")]
public class SolicitudWaterHeaterFlush
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [MaxLength(15)]
    public string? TipoCalentador { get; set; }

    [MaxLength(15)]
    public string? FuenteEnergia { get; set; }

    [MaxLength(80)]
    public string? NumeroSerie { get; set; }

    public bool SerialDesconocido { get; set; }

    [MaxLength(80)]
    public string? MarcaModelo { get; set; }

    [MaxLength(20)]
    public string? Ubicacion { get; set; }

    [MaxLength(20)]
    public string? UltimoFlush { get; set; }

    [MaxLength(200)]
    public string? SintomasSeleccionados { get; set; }

    [MaxLength(20)]
    public string? TipoServicio { get; set; }

    public bool RecordatorioAnual { get; set; }

    [MaxLength(20)]
    public string? PreferenciaTiempo { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaVisita { get; set; }

    [MaxLength(200)]
    public string? NotasAdicionales { get; set; }

    [MaxLength(30)]
    public string? TelefonoContacto { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoWaterHeaterFlush> Archivos { get; set; } = new List<ArchivoWaterHeaterFlush>();
}

[Table("ArchivosWaterHeaterFlush")]
public class ArchivoWaterHeaterFlush
{
    public int Id { get; set; }
    public int SolicitudWaterHeaterFlushId { get; set; }

    [ForeignKey(nameof(SolicitudWaterHeaterFlushId))]
    public SolicitudWaterHeaterFlush? Solicitud { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TipoContenido { get; set; }

    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
