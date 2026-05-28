using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("CrawlspaceCheckServicioLanding")]
public class CrawlspaceCheckServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Crawlspace Check";

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
    public string? PreocupacionItems { get; set; }

    [MaxLength(300)]
    public string? PreocupacionIconos { get; set; }

    [MaxLength(400)]
    public string? InfoBoxTexto { get; set; }

    [MaxLength(300)]
    public string? ResumenServicioTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Start crawlspace check";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesCrawlspaceCheck")]
public class SolicitudCrawlspaceCheck
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

    [MaxLength(10)]
    public string? Encapsulacion { get; set; }

    [MaxLength(10)]
    public string? Aislamiento { get; set; }

    [MaxLength(10)]
    public string? BarreraVapor { get; set; }

    [MaxLength(20)]
    public string? TipoAcceso { get; set; }

    [MaxLength(20)]
    public string? UltimaRevision { get; set; }

    [MaxLength(200)]
    public string? PreocupacionesSeleccionadas { get; set; }

    [MaxLength(20)]
    public string? TimingPreferido { get; set; }

    public bool RecordatorioAnual { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaPreferida { get; set; }

    [MaxLength(300)]
    public string? Notas { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
}
