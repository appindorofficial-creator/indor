using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("Microservicios")]
public class Microservicio
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Subtitulo { get; set; }

    [Required, MaxLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    public string? DescripcionCompleta { get; set; }

    /// <summary>
    /// Lista de viñetas separadas por '|' (ej: "Revisión básica|Cambio de filtro|Instalación profesional")
    /// </summary>
    public string? Incluye { get; set; }

    [Required, MaxLength(100)]
    public string Frecuencia { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [MaxLength(10)]
    public string Moneda { get; set; } = "USD";

    [MaxLength(50)]
    public string? PrecioPrefijo { get; set; }  // "Desde", "Mensual", etc.

    [MaxLength(80)]
    public string? CtaTexto { get; set; }       // "Agendar servicio", "Reservar mantenimiento"...

    /// <summary>
    /// Ruta relativa a /wwwroot (ej: "aire.jpeg"). Si está vacío, se usa <see cref="ImagenBase64"/>.
    /// </summary>
    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    public string ImagenBase64 { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
