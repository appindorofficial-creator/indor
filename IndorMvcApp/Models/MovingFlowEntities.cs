using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("MovingServicioLanding")]
public class MovingServicioLanding
{
    public int Id { get; set; }

    public int MovingSetupServicioId { get; set; }

    [ForeignKey(nameof(MovingSetupServicioId))]
    public MovingSetupServicio? MovingSetupServicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Moving Service";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(500)]
    public string? IncluyeItems { get; set; }

    [MaxLength(300)]
    public string? IncluyeIconos { get; set; }

    [Required, MaxLength(60)]
    public string EstimatedTimeLabel { get; set; } = "Estimated time";

    [Required, MaxLength(60)]
    public string EstimatedTimeValue { get; set; } = "2-6 hours";

    [MaxLength(120)]
    public string? EstimatedTimeNote { get; set; }

    [Required, MaxLength(60)]
    public string BestForLabel { get; set; } = "Best for";

    [Required, MaxLength(120)]
    public string BestForValue { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? BestForNote { get; set; }

    [Required, MaxLength(200)]
    public string MoveTypes { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? MoveTypeIcons { get; set; }

    [Required, MaxLength(200)]
    public string MoveTypeValues { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string CtaContinueTexto { get; set; } = "Continue";

    [Required, MaxLength(40)]
    public string CtaEstimateTexto { get; set; } = "Get estimate";

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioEstimadoMin { get; set; } = 420;

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioEstimadoMax { get; set; } = 620;

    public int DuracionEstimadaMinHoras { get; set; } = 2;

    public int DuracionEstimadaMaxHoras { get; set; } = 6;

    [MaxLength(300)]
    public string? DisclaimerTexto { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesMoving")]
public class SolicitudMoving
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int MovingSetupServicioId { get; set; }

    [ForeignKey(nameof(MovingSetupServicioId))]
    public MovingSetupServicio? MovingSetupServicio { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(30)]
    public string TipoMovimiento { get; set; } = "MoveIn";

    [MaxLength(30)]
    public string? TipoPropiedad { get; set; }

    [MaxLength(30)]
    public string? TamanoHogar { get; set; }

    [MaxLength(300)]
    public string? DireccionOrigen { get; set; }

    [MaxLength(300)]
    public string? DireccionDestino { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaMovimiento { get; set; }

    [MaxLength(60)]
    public string? VentanaHorario { get; set; }

    [MaxLength(30)]
    public string? TipoServicio { get; set; }

    [MaxLength(400)]
    public string? ItemsMover { get; set; }

    [MaxLength(30)]
    public string? TamanoMovimiento { get; set; }

    [MaxLength(300)]
    public string? CondicionesAcceso { get; set; }

    [MaxLength(10)]
    public string? RequiereMontaje { get; set; }

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimadoMin { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimadoMax { get; set; }

    public int? DuracionEstimadaMinHoras { get; set; }

    public int? DuracionEstimadaMaxHoras { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoMoving> Archivos { get; set; } = new List<ArchivoMoving>();
}

[Table("ArchivosMoving")]
public class ArchivoMoving
{
    public int Id { get; set; }

    public int SolicitudMovingId { get; set; }

    [ForeignKey(nameof(SolicitudMovingId))]
    public SolicitudMoving? Solicitud { get; set; }

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
