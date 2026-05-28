using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("LawnServicioLanding")]
public class LawnServicioLanding
{
    public int Id { get; set; }
    public int MicroservicioId { get; set; }

    [ForeignKey(nameof(MicroservicioId))]
    public Microservicio? Microservicio { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Always Perfect Lawn";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [Required, MaxLength(400)]
    public string LandingSubtitulo { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; } = 45;

    [MaxLength(120)]
    public string? PrecioTexto { get; set; }

    [MaxLength(500)]
    public string? IncluyeItems { get; set; }

    [MaxLength(300)]
    public string? IncluyeIconos { get; set; }

    [MaxLength(400)]
    public string? InfoBoxTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Customize service";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesLawn")]
public class SolicitudLawn
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

    [Required, MaxLength(20)]
    public string TipoServicio { get; set; } = "Subscription";

    [MaxLength(20)]
    public string? Frecuencia { get; set; }

    [MaxLength(30)]
    public string? AreaServicio { get; set; }

    [MaxLength(300)]
    public string? AddonsSeleccionados { get; set; }

    [MaxLength(30)]
    public string? PreferenciaExtra { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaPreferida { get; set; }

    [MaxLength(30)]
    public string? VentanaHorario { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioBase { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioAddons { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? DescuentoSuscripcion { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioTotal { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
}
