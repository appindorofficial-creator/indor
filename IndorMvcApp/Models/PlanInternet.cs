using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.Models;

public class PlanInternet
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Proveedor { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public int VelocidadDescargaMbps { get; set; }
    public int VelocidadSubidaMbps { get; set; }

    public decimal PrecioMensual { get; set; }

    [StringLength(10)]
    public string Moneda { get; set; } = "USD";

    [StringLength(500)]
    public string? Caracteristicas { get; set; } // separadas por |

    public bool EsPlanActual { get; set; }
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}
