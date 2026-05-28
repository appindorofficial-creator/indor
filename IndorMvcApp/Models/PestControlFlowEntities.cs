using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("PestControlServicioLanding")]
public class PestControlServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Pest Control Check";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? LandingSubtitulo { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(500)]
    public string? WhyItMattersItems { get; set; }

    [MaxLength(300)]
    public string? WhyItMattersIconos { get; set; }

    [MaxLength(400)]
    public string? BestForTexto { get; set; }

    [MaxLength(400)]
    public string? InfoPlanTexto { get; set; }

    [MaxLength(500)]
    public string? WhyYearlyItems { get; set; }

    [MaxLength(300)]
    public string? WhyYearlyIconos { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Continue";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesPestControl")]
public class SolicitudPestControl
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

    [MaxLength(20)]
    public string? UltimoServicio { get; set; }

    [MaxLength(200)]
    public string? SignosSeleccionados { get; set; }

    [MaxLength(200)]
    public string? AreasPreocupacion { get; set; }

    [MaxLength(10)]
    public string? MascotasONinos { get; set; }

    [MaxLength(20)]
    public string? TipoServicio { get; set; }

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
}
