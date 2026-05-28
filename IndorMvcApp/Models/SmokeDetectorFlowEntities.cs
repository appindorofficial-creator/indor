using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SmokeDetectorServicioLanding")]
public class SmokeDetectorServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Smoke / CO Check";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(400)]
    public string? LandingSubtitulo { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(500)]
    public string? TrackItems { get; set; }

    [MaxLength(500)]
    public string? TrackDescriptions { get; set; }

    [MaxLength(300)]
    public string? TrackIconos { get; set; }

    [MaxLength(300)]
    public string? WhereTrackItems { get; set; }

    [MaxLength(300)]
    public string? WhereTrackIconos { get; set; }

    [MaxLength(400)]
    public string? ReminderBannerTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Start reminder setup";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesSmokeDetector")]
public class SolicitudSmokeDetector
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
    public string? CantidadAlarmas { get; set; }

    [MaxLength(200)]
    public string? UbicacionesSeleccionadas { get; set; }

    [MaxLength(200)]
    public string? TiposAlarmas { get; set; }

    [MaxLength(20)]
    public string? UltimaPrueba { get; set; }

    [MaxLength(20)]
    public string? UltimoCambioBateria { get; set; }

    public int? AnioInstalacion { get; set; }

    public bool AnioInstalacionDesconocido { get; set; }

    [MaxLength(200)]
    public string? ProblemasSeleccionados { get; set; }

    public bool RecordatorioMensual { get; set; } = true;

    public bool RecordatorioBateriaAnual { get; set; } = true;

    public bool RecordatorioReemplazo10Anos { get; set; } = true;

    public bool RecordatorioRevisionEstacional { get; set; } = true;

    [MaxLength(20)]
    public string? TipoAccionAyuda { get; set; }

    public DateTime? FechaInstalacionReferencia { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
}
