using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("UtilitiesSetupProveedorInternet")]
public class UtilitiesSetupProveedorInternet
{
    public int Id { get; set; }

    [Required, MaxLength(40)]
    public string Codigo { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Etiqueta { get; set; }

    [MaxLength(60)]
    public string? Velocidad { get; set; }

    [MaxLength(120)]
    public string? DetalleExtra { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioDesde { get; set; }

    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
}

[Table("SolicitudesUtilitiesSetup")]
public class SolicitudUtilitiesSetup
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

    [MaxLength(120)]
    public string? ServiciosConectar { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaServicio { get; set; }

    [MaxLength(30)]
    public string? PreferenciaContacto { get; set; }

    public int? ProveedorInternetId { get; set; }

    [ForeignKey(nameof(ProveedorInternetId))]
    public UtilitiesSetupProveedorInternet? ProveedorInternet { get; set; }

    [MaxLength(30)]
    public string? OpcionCable { get; set; }

    public bool OmitirInternet { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<UtilitiesSetupContacto> Contactos { get; set; } = new List<UtilitiesSetupContacto>();
}

[Table("UtilitiesSetupContactos")]
public class UtilitiesSetupContacto
{
    public int Id { get; set; }
    public int SolicitudUtilitiesSetupId { get; set; }

    [ForeignKey(nameof(SolicitudUtilitiesSetupId))]
    public SolicitudUtilitiesSetup? Solicitud { get; set; }

    [Required, MaxLength(30)]
    public string TipoUtilidad { get; set; } = string.Empty;

    [Required, MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Telefono { get; set; }

    [MaxLength(300)]
    public string? Website { get; set; }

    [MaxLength(50)]
    public string? IconoClase { get; set; }

    public int Orden { get; set; }
}
