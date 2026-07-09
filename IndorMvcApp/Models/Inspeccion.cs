using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Models;

[Table("Inspecciones")]
public class Inspeccion
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

    public string? Incluye { get; set; }

    public string? IncluyeEs { get; set; }

    [MaxLength(100)]
    public string? Frecuencia { get; set; }

    [MaxLength(100)]
    public string? FrecuenciaEs { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? Valor { get; set; }

    [MaxLength(10)]
    public string Moneda { get; set; } = "USD";

    [MaxLength(50)]
    public string? PrecioPrefijo { get; set; }

    [MaxLength(50)]
    public string? PrecioPrefijoEs { get; set; }

    [MaxLength(50)]
    public string? PrecioTexto { get; set; }

    [MaxLength(50)]
    public string? PrecioTextoEs { get; set; }

    [MaxLength(80)]
    public string? CtaTexto { get; set; }

    [MaxLength(80)]
    public string? CtaTextoEs { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;

    public int Orden { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public string LocalizedNombre(bool isSpanish) => CatalogText.Pick(Nombre, NombreEs, isSpanish);
    public string LocalizedDescripcion(bool isSpanish) => CatalogText.Pick(Descripcion, DescripcionEs, isSpanish);
    public string? LocalizedCtaTexto(bool isSpanish) => CatalogText.Pick(CtaTexto, CtaTextoEs, isSpanish);
}
