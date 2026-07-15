using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using IndorMvcApp.Localization;

namespace IndorMvcApp.Models;

[Table("Microservicios")]
public class Microservicio
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? NombreEs { get; set; }

    [MaxLength(250)]
    public string? Subtitulo { get; set; }

    [MaxLength(250)]
    public string? SubtituloEs { get; set; }

    [Required, MaxLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? DescripcionEs { get; set; }

    public string? DescripcionCompleta { get; set; }

    public string? DescripcionCompletaEs { get; set; }

    /// <summary>
    /// Lista de viñetas separadas por '|' (ej: "Revisión básica|Cambio de filtro|Instalación profesional")
    /// </summary>
    public string? Incluye { get; set; }

    public string? IncluyeEs { get; set; }

    [Required, MaxLength(100)]
    public string Frecuencia { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FrecuenciaEs { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; }

    [MaxLength(10)]
    public string Moneda { get; set; } = "USD";

    [MaxLength(50)]
    public string? PrecioPrefijo { get; set; }  // "Desde", "Mensual", etc.

    [MaxLength(50)]
    public string? PrecioPrefijoEs { get; set; }

    [MaxLength(80)]
    public string? CtaTexto { get; set; }       // "Agendar servicio", "Reservar mantenimiento"...

    [MaxLength(80)]
    public string? CtaTextoEs { get; set; }

    /// <summary>
    /// Ruta relativa a /wwwroot (ej: "aire.jpeg"). Si está vacío, se usa <see cref="ImagenBase64"/>.
    /// </summary>
    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    public string ImagenBase64 { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public string LocalizedNombre(bool isSpanish) => CatalogText.PickWithUiFallback(Nombre, NombreEs, isSpanish);
    public string? LocalizedSubtitulo(bool isSpanish) => CatalogText.PickWithUiFallback(Subtitulo, SubtituloEs, isSpanish);
    public string LocalizedDescripcion(bool isSpanish) => CatalogText.PickWithUiFallback(Descripcion, DescripcionEs, isSpanish);
    public string? LocalizedCtaTexto(bool isSpanish) => CatalogText.PickWithUiFallback(CtaTexto, CtaTextoEs, isSpanish);
}
