using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Models;

public class PlanMembresia
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NombreEs { get; set; }

    [StringLength(250)]
    public string? Subtitulo { get; set; }

    [StringLength(250)]
    public string? SubtituloEs { get; set; }

    [StringLength(1000)]
    public string? Descripcion { get; set; }

    [StringLength(1000)]
    public string? DescripcionEs { get; set; }

    public decimal PrecioMensual { get; set; }

    [StringLength(10)]
    public string Moneda { get; set; } = "USD";

    public string? Caracteristicas { get; set; }

    public string? CaracteristicasEs { get; set; }

    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public bool Recomendado { get; set; }

    public string LocalizedNombre(bool isSpanish) => CatalogText.Pick(Nombre, NombreEs, isSpanish);
}
