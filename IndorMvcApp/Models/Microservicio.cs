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

    /// <summary>
    /// Spanish display names for Home Care Essentials brands
    /// (QA: show Spanish names like "Aire Seguro 365").
    /// </summary>
    private static readonly Dictionary<int, string> BrandNamesEsById = new()
    {
        [1] = "Aire Seguro 365",
        [2] = "Césped Siempre Perfecto",
        [3] = "Basura Sin Estrés",
        [4] = "Limpieza Pro",
    };

    private static readonly Dictionary<string, string> BrandNamesEsByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Safe Air 365"] = "Aire Seguro 365",
        ["Aire Seguro 365"] = "Aire Seguro 365",
        ["Always Perfect Lawn"] = "Césped Siempre Perfecto",
        ["Césped Siempre Perfecto"] = "Césped Siempre Perfecto",
        ["Jardín Siempre Perfecto"] = "Césped Siempre Perfecto",
        ["Jardin Siempre Perfecto"] = "Césped Siempre Perfecto",
        ["Stress-Free Trash"] = "Basura Sin Estrés",
        ["Basura Sin Estrés"] = "Basura Sin Estrés",
        ["Basura Sin Estres"] = "Basura Sin Estrés",
        ["Cleaning Pro"] = "Limpieza Pro",
        ["Limpieza Pro"] = "Limpieza Pro",
    };

    /// <summary>Resolve display name for a Home Care Essentials brand.</summary>
    public static string ResolveBrandNombre(int id, string? nombre, string? nombreEs = null, bool isSpanish = false)
    {
        if (!isSpanish)
        {
            // Prefer canonical English keys when known.
            if (BrandNamesEsById.ContainsKey(id))
            {
                return id switch
                {
                    1 => "Safe Air 365",
                    2 => "Always Perfect Lawn",
                    3 => "Stress-Free Trash",
                    4 => "Cleaning Pro",
                    _ => nombre?.Trim() ?? string.Empty
                };
            }

            return nombre?.Trim() ?? string.Empty;
        }

        if (BrandNamesEsById.TryGetValue(id, out var byId))
        {
            return byId;
        }

        if (!string.IsNullOrWhiteSpace(nombreEs) && BrandNamesEsByKey.TryGetValue(nombreEs.Trim(), out var fromEs))
        {
            return fromEs;
        }

        if (!string.IsNullOrWhiteSpace(nombre) && BrandNamesEsByKey.TryGetValue(nombre.Trim(), out var fromEn))
        {
            return fromEn;
        }

        return CatalogText.PickWithUiFallback(nombre, nombreEs, true);
    }

    public string LocalizedNombre(bool isSpanish) =>
        ResolveBrandNombre(Id, Nombre, NombreEs, isSpanish);

    public string? LocalizedSubtitulo(bool isSpanish) => CatalogText.PickWithUiFallback(Subtitulo, SubtituloEs, isSpanish);
    public string LocalizedDescripcion(bool isSpanish) => CatalogText.PickWithUiFallback(Descripcion, DescripcionEs, isSpanish);
    public string? LocalizedCtaTexto(bool isSpanish) => CatalogText.PickWithUiFallback(CtaTexto, CtaTextoEs, isSpanish);
}
