using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("TvWallMountingServicioLanding")]
public class TvWallMountingServicioLanding
{
    public int Id { get; set; }
    public int MovingSetupServicioId { get; set; }

    [ForeignKey(nameof(MovingSetupServicioId))]
    public MovingSetupServicio? MovingSetupServicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Service Detail";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(300)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 129;

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

    [MaxLength(300)]
    public string? InfoBoxTexto { get; set; }

    [Required, MaxLength(60)]
    public string EstimatedTimeLabel { get; set; } = "Estimated time";

    [Required, MaxLength(60)]
    public string EstimatedTimeValue { get; set; } = "60-90 min";

    [Required, MaxLength(60)]
    public string BestTimingLabel { get; set; } = "Best recommendation";

    [Required, MaxLength(120)]
    public string BestTimingValue { get; set; } = "After move-in or room setup";

    [Required, MaxLength(40)]
    public string CtaContinueTexto { get; set; } = "Continue";

    [Required, MaxLength(40)]
    public string CtaUploadTexto { get; set; } = "Upload photos first";

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioBaseEstimado { get; set; } = 129;

    [MaxLength(300)]
    public string? DisclaimerTexto { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesTvWallMounting")]
public class SolicitudTvWallMounting
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

    [MaxLength(30)]
    public string? TipoSolicitud { get; set; }

    [MaxLength(30)]
    public string? TamanoTv { get; set; }

    [MaxLength(20)]
    public string? CantidadTvs { get; set; }

    [MaxLength(30)]
    public string? Habitacion { get; set; }

    [MaxLength(30)]
    public string? TipoPared { get; set; }

    [MaxLength(30)]
    public string? TieneSoporte { get; set; }

    [MaxLength(30)]
    public string? ConfiguracionCables { get; set; }

    [MaxLength(20)]
    public string? TomaCercana { get; set; }

    [MaxLength(20)]
    public string? MontajePrevio { get; set; }

    [MaxLength(30)]
    public string? DetallesAcceso { get; set; }

    [MaxLength(30)]
    public string? VentanaHorario { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaServicio { get; set; }

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoTvWallMounting> Archivos { get; set; } = new List<ArchivoTvWallMounting>();
}

[Table("ArchivosTvWallMounting")]
public class ArchivoTvWallMounting
{
    public int Id { get; set; }
    public int SolicitudTvWallMountingId { get; set; }

    [ForeignKey(nameof(SolicitudTvWallMountingId))]
    public SolicitudTvWallMounting? Solicitud { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TipoContenido { get; set; }

    [MaxLength(40)]
    public string? CategoriaArchivo { get; set; }

    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
