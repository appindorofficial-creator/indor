using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("CleaningProServicioLanding")]
public class CleaningProServicioLanding
{
    public int Id { get; set; }
    public int MicroservicioId { get; set; }

    [ForeignKey(nameof(MicroservicioId))]
    public Microservicio? Microservicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Cleaning Pro";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(400)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 35;

    [MaxLength(120)]
    public string? PrecioTexto { get; set; }

    [MaxLength(500)]
    public string? IncluyeItems { get; set; }

    [MaxLength(300)]
    public string? IncluyeIconos { get; set; }

    [MaxLength(400)]
    public string? InfoBoxTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Customize cleaning";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesCleaningPro")]
public class SolicitudCleaningPro
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int MicroservicioId { get; set; }

    [ForeignKey(nameof(MicroservicioId))]
    public Microservicio? Microservicio { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [MaxLength(20)]
    public string? Frecuencia { get; set; }

    [MaxLength(10)]
    public string? CantidadLimpiadores { get; set; }

    [MaxLength(300)]
    public string? AreasLimpieza { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal? HorasEstimadas { get; set; }

    [MaxLength(120)]
    public string? AddonsSeleccionados { get; set; }

    [MaxLength(500)]
    public string? NotasLimpiador { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaServicio { get; set; }

    [MaxLength(30)]
    public string? VentanaHorario { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? TarifaHoraria { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Subtotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? ImpuestoVenta { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioTotal { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
}
