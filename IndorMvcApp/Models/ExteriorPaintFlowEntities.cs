using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("ExteriorPaintServicioLanding")]
public class ExteriorPaintServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Exterior Paint Review";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [MaxLength(400)]
    public string? LandingSubtitulo { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 0;

    [MaxLength(120)]
    public string? PrecioTexto { get; set; }

    [MaxLength(400)]
    public string? InfoBoxTexto { get; set; }

    [MaxLength(500)]
    public string? WhyItMattersItems { get; set; }

    [MaxLength(300)]
    public string? WhyItMattersIconos { get; set; }

    [MaxLength(500)]
    public string? NextStepsItems { get; set; }

    [MaxLength(300)]
    public string? NextStepsIconos { get; set; }

    [MaxLength(300)]
    public string? ReminderTexto { get; set; }

    [MaxLength(300)]
    public string? ResumenServicioTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Continue";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesExteriorPaint")]
public class SolicitudExteriorPaint
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
    public string? UltimaPintura { get; set; }

    [MaxLength(30)]
    public string? TipoSuperficie { get; set; }

    [MaxLength(10)]
    public string? MantenerMismoColor { get; set; }

    [MaxLength(200)]
    public string? ProblemasSeleccionados { get; set; }

    [MaxLength(200)]
    public string? AreasSeleccionadas { get; set; }

    [MaxLength(10)]
    public string? ActualizacionColor { get; set; }

    [MaxLength(10)]
    public string? LavadoPresionReciente { get; set; }

    [MaxLength(10)]
    public string? NumeroPisos { get; set; }

    [MaxLength(20)]
    public string? TimingPreferido { get; set; }

    public bool RecordatorioAnual { get; set; }

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

    public ICollection<ArchivoExteriorPaint> Archivos { get; set; } = new List<ArchivoExteriorPaint>();
}

[Table("ArchivosExteriorPaint")]
public class ArchivoExteriorPaint
{
    public int Id { get; set; }
    public int SolicitudExteriorPaintId { get; set; }

    [ForeignKey(nameof(SolicitudExteriorPaintId))]
    public SolicitudExteriorPaint? Solicitud { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CategoriaFoto { get; set; }

    [Required, MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TipoContenido { get; set; }

    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
