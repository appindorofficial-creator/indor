using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("PackingServicioLanding")]
public class PackingServicioLanding
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
    public decimal PrecioDesde { get; set; } = 89;

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
    public string EstimatedTimeValue { get; set; } = "2-5 hrs";

    [Required, MaxLength(60)]
    public string BestTimingLabel { get; set; } = "Best timing";

    [Required, MaxLength(120)]
    public string BestTimingValue { get; set; } = "1-3 days before moving";

    [Required, MaxLength(40)]
    public string CtaContinueTexto { get; set; } = "Continue";

    [Required, MaxLength(40)]
    public string CtaUploadTexto { get; set; } = "Upload photos or list";

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioBaseEstimado { get; set; } = 89;

    [MaxLength(300)]
    public string? DisclaimerTexto { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesPacking")]
public class SolicitudPacking
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
    public string? TipoEmpaque { get; set; }

    [MaxLength(30)]
    public string? CuandoMudanza { get; set; }

    [MaxLength(30)]
    public string? TipoPropiedad { get; set; }

    [MaxLength(30)]
    public string? TamanoHogar { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaServicio { get; set; }

    [MaxLength(60)]
    public string? VentanaHorario { get; set; }

    [MaxLength(400)]
    public string? HabitacionesEmpacar { get; set; }

    [MaxLength(400)]
    public string? ItemsEspeciales { get; set; }

    [MaxLength(300)]
    public string? SuministrosNecesarios { get; set; }

    [MaxLength(300)]
    public string? DetallesAcceso { get; set; }

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoPacking> Archivos { get; set; } = new List<ArchivoPacking>();
}

[Table("ArchivosPacking")]
public class ArchivoPacking
{
    public int Id { get; set; }

    public int SolicitudPackingId { get; set; }

    [ForeignKey(nameof(SolicitudPackingId))]
    public SolicitudPacking? Solicitud { get; set; }

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
