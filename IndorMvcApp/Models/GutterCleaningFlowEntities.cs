using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("GutterCleaningServicioLanding")]
public class GutterCleaningServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Gutter Cleaning";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [MaxLength(500)]
    public string? InfoBoxTexto { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(500)]
    public string? WhyItMattersItems { get; set; }

    [MaxLength(300)]
    public string? WhyItMattersIconos { get; set; }

    [MaxLength(500)]
    public string? NextStepsItems { get; set; }

    [MaxLength(300)]
    public string? NextStepsIconos { get; set; }

    [MaxLength(500)]
    public string? RecommendedTimingItems { get; set; }

    [MaxLength(300)]
    public string? RecommendedTimingIconos { get; set; }

    [MaxLength(300)]
    public string? InfoConfirmacionTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Continue";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesGutterCleaning")]
public class SolicitudGutterCleaning
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

    [MaxLength(20)]
    public string? TipoAccionInicial { get; set; }

    [MaxLength(10)]
    public string? NumeroPisos { get; set; }

    [MaxLength(20)]
    public string? TipoCanaletas { get; set; }

    [MaxLength(10)]
    public string? ProtectorCanaletas { get; set; }

    [MaxLength(20)]
    public string? UltimaLimpieza { get; set; }

    public int? CantidadBajantes { get; set; }

    [MaxLength(200)]
    public string? ProblemasSeleccionados { get; set; }

    [MaxLength(20)]
    public string? AreaProblema { get; set; }

    [MaxLength(20)]
    public string? ObjetivoHoy { get; set; }

    [MaxLength(20)]
    public string? PreferenciaRecordatorio { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaRecordatorioPersonalizada { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaVisitaPreferida { get; set; }

    [MaxLength(300)]
    public string? Notas { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    public bool RecordatorioPrimaveraOtono { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoGutterCleaning> Archivos { get; set; } = new List<ArchivoGutterCleaning>();
}

[Table("ArchivosGutterCleaning")]
public class ArchivoGutterCleaning
{
    public int Id { get; set; }
    public int SolicitudGutterCleaningId { get; set; }

    [ForeignKey(nameof(SolicitudGutterCleaningId))]
    public SolicitudGutterCleaning? Solicitud { get; set; }

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
