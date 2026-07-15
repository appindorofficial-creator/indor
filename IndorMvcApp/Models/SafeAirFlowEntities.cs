using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SafeAirServicioLanding")]
public class SafeAirServicioLanding
{
    public int Id { get; set; }
    public int MicroservicioId { get; set; }

    [ForeignKey(nameof(MicroservicioId))]
    public Microservicio? Microservicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Safe Air 365";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(400)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 49;

    [MaxLength(120)]
    public string? PrecioTexto { get; set; }

    [MaxLength(500)]
    public string? IncluyeItems { get; set; }

    [MaxLength(300)]
    public string? IncluyeIconos { get; set; }

    [MaxLength(400)]
    public string? InfoBoxTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaScheduleTexto { get; set; } = "Schedule with INDOR";

    [Required, MaxLength(40)]
    public string CtaChangedTexto { get; set; } = "I changed it myself";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesSafeAir")]
public class SolicitudSafeAir
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int MicroservicioId { get; set; }

    [ForeignKey(nameof(MicroservicioId))]
    public Microservicio? Microservicio { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(30)]
    public string TipoNecesidad { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? CantidadFiltros { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? FiltroAncho { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? FiltroAlto { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? FiltroProfundidad { get; set; }

    public bool FiltroTamanioDesconocido { get; set; }

    [MaxLength(30)]
    public string? UbicacionFiltro { get; set; }

    [MaxLength(30)]
    public string? ProveedorFiltro { get; set; }

    public bool RecordatorioActivo { get; set; }

    [MaxLength(30)]
    public string? VentanaTiempo { get; set; }

    [MaxLength(120)]
    public string? DetallesAcceso { get; set; }

    [MaxLength(500)]
    public string? NotasAcceso { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaProximoRecordatorio { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoSafeAir> Archivos { get; set; } = new List<ArchivoSafeAir>();
}

[Table("ArchivosSafeAir")]
public class ArchivoSafeAir
{
    public int Id { get; set; }
    public int SolicitudSafeAirId { get; set; }

    [ForeignKey(nameof(SolicitudSafeAirId))]
    public SolicitudSafeAir? Solicitud { get; set; }

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
