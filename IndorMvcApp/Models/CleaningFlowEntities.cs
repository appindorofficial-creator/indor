using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("CleaningServicioLanding")]
public class CleaningServicioLanding
{
    public int Id { get; set; }

    public int MovingSetupServicioId { get; set; }

    [ForeignKey(nameof(MovingSetupServicioId))]
    public MovingSetupServicio? MovingSetupServicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Cleaning Service";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(300)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 149;

    [MaxLength(500)]
    public string? IncluyeItems { get; set; }

    [MaxLength(300)]
    public string? IncluyeIconos { get; set; }

    [Required, MaxLength(60)]
    public string BestForLabel { get; set; } = "Best for";

    [Required, MaxLength(200)]
    public string BestForOptions { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? BestForIcons { get; set; }

    [Required, MaxLength(200)]
    public string BestForValues { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? InfoBoxTitulo { get; set; }

    [MaxLength(300)]
    public string? InfoBoxTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaContinueTexto { get; set; } = "Continue";

    [Required, MaxLength(40)]
    public string CtaUploadTexto { get; set; } = "Upload photos";

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioBaseEstimado { get; set; } = 149;

    [MaxLength(300)]
    public string? DisclaimerTexto { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesCleaning")]
public class SolicitudCleaning
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

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [Required, MaxLength(30)]
    public string TipoLimpieza { get; set; } = "MoveIn";

    [MaxLength(30)]
    public string? TipoPropiedad { get; set; }

    [MaxLength(20)]
    public string? NumeroHabitaciones { get; set; }

    [MaxLength(20)]
    public string? NumeroBanos { get; set; }

    [MaxLength(30)]
    public string? CondicionActual { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaServicio { get; set; }

    [MaxLength(60)]
    public string? VentanaHorario { get; set; }

    [MaxLength(400)]
    public string? AreasPrioridad { get; set; }

    [MaxLength(500)]
    public string? TareasExtra { get; set; }

    [MaxLength(20)]
    public string? SuministrosNecesarios { get; set; }

    [MaxLength(30)]
    public string? MetodoAcceso { get; set; }

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoCleaning> Archivos { get; set; } = new List<ArchivoCleaning>();
}

[Table("ArchivosCleaning")]
public class ArchivoCleaning
{
    public int Id { get; set; }

    public int SolicitudCleaningId { get; set; }

    [ForeignKey(nameof(SolicitudCleaningId))]
    public SolicitudCleaning? Solicitud { get; set; }

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
