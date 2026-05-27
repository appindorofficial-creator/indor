using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("FurnitureAssemblyServicioLanding")]
public class FurnitureAssemblyServicioLanding
{
    public int Id { get; set; }

    public int MovingSetupServicioId { get; set; }

    [ForeignKey(nameof(MovingSetupServicioId))]
    public MovingSetupServicio? MovingSetupServicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Furniture & Assembly";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

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

    [MaxLength(300)]
    public string? BadgesTexto { get; set; }

    [MaxLength(200)]
    public string? BadgesIconos { get; set; }

    [Required, MaxLength(60)]
    public string EstimatedTimeLabel { get; set; } = "Estimated time";

    [Required, MaxLength(60)]
    public string EstimatedTimeValue { get; set; } = "1-3 hours";

    [MaxLength(120)]
    public string? EstimatedTimeNote { get; set; }

    [Required, MaxLength(60)]
    public string BestForLabel { get; set; } = "Best for";

    [Required, MaxLength(120)]
    public string BestForValue { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? BestForNote { get; set; }

    [Required, MaxLength(40)]
    public string CtaContinueTexto { get; set; } = "Continue";

    [Required, MaxLength(40)]
    public string CtaUploadTexto { get; set; } = "Upload photos or manuals";

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioBaseEstimado { get; set; } = 89;

    [MaxLength(300)]
    public string? DisclaimerTexto { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesFurnitureAssembly")]
public class SolicitudFurnitureAssembly
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

    [MaxLength(400)]
    public string? TiposMueble { get; set; }

    [MaxLength(20)]
    public string? CantidadItems { get; set; }

    [MaxLength(30)]
    public string? CondicionItems { get; set; }

    [MaxLength(20)]
    public string? AnclajePared { get; set; }

    [MaxLength(30)]
    public string? Habitacion { get; set; }

    [MaxLength(300)]
    public string? DetallesAcceso { get; set; }

    [MaxLength(20)]
    public string? AyudaMover { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaServicio { get; set; }

    [MaxLength(30)]
    public string? VentanaHorario { get; set; }

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoFurnitureAssembly> Archivos { get; set; } = new List<ArchivoFurnitureAssembly>();
}

[Table("ArchivosFurnitureAssembly")]
public class ArchivoFurnitureAssembly
{
    public int Id { get; set; }

    public int SolicitudFurnitureAssemblyId { get; set; }

    [ForeignKey(nameof(SolicitudFurnitureAssemblyId))]
    public SolicitudFurnitureAssembly? Solicitud { get; set; }

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
