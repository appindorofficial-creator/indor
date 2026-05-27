using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("ServiciosEmergencia")]
public class ServicioEmergencia
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string TituloEmergencia { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    public int TiempoLlegadaMinutos { get; set; } = 45;

    [MaxLength(50)]
    public string IconoClase { get; set; } = "fa-droplet";

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(80)]
    public string? BadgeTexto { get; set; }

    public bool EsPredeterminado { get; set; }

    /// <summary>
    /// Lista separada por '|', ej: Arrives fast|Trusted pros|Upfront pricing
    /// </summary>
    [MaxLength(500)]
    public string? Caracteristicas { get; set; }

    /// <summary>
    /// Iconos Font Awesome separados por '|', alineados con Caracteristicas.
    /// </summary>
    [MaxLength(200)]
    public string? IconosCaracteristicas { get; set; }

    [MaxLength(80)]
    public string CtaTexto { get; set; } = "Request help";

    public bool Activo { get; set; } = true;

    public int Orden { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
