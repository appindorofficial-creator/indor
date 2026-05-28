using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("HvacMaintenanceServicioLanding")]
public class HvacMaintenanceServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "HVAC Tune-Up";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(400)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 89;

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

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Start tune-up request";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesHvacMaintenance")]
public class SolicitudHvacMaintenance
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

    [MaxLength(80)]
    public string? NumeroSerieAc { get; set; }

    public bool SerialDesconocido { get; set; }

    [MaxLength(80)]
    public string? UltimoMantenimiento { get; set; }

    public bool UltimoMantenimientoDesconocido { get; set; }

    [MaxLength(40)]
    public string? TamanioFiltro { get; set; }

    [MaxLength(500)]
    public string? NotasTecnico { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaVisita { get; set; }

    [MaxLength(20)]
    public string? VentanaHorario { get; set; }

    [MaxLength(20)]
    public string? TipoServicio { get; set; }

    public bool RecordatorioAnual { get; set; }

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

    public ICollection<ArchivoHvacMaintenance> Archivos { get; set; } = new List<ArchivoHvacMaintenance>();
}

[Table("ArchivosHvacMaintenance")]
public class ArchivoHvacMaintenance
{
    public int Id { get; set; }
    public int SolicitudHvacMaintenanceId { get; set; }

    [ForeignKey(nameof(SolicitudHvacMaintenanceId))]
    public SolicitudHvacMaintenance? Solicitud { get; set; }

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
