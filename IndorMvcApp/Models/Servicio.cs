using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("Servicios")]
public class Servicio
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
    /// Lista de viñetas separadas por '|'.
    /// </summary>
    public string? Incluye { get; set; }

    [MaxLength(100)]
    public string? Frecuencia { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? Valor { get; set; }

    [MaxLength(10)]
    public string Moneda { get; set; } = "USD";

    [MaxLength(50)]
    public string? PrecioPrefijo { get; set; }   // "Desde", "Personalizado"...

    /// <summary>
    /// Etiqueta libre cuando el precio no es un número (ej: "Personalizado").
    /// </summary>
    [MaxLength(50)]
    public string? PrecioTexto { get; set; }

    [MaxLength(80)]
    public string? CtaTexto { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;

    public int Orden { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
