using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

public class Pago
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    [Required, StringLength(200)]
    public string Concepto { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    [StringLength(10)]
    public string Moneda { get; set; } = "USD";

    [Required, StringLength(30)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Completado | Financiado | Vencido

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }

    public int? MetodoPagoId { get; set; }
    [ForeignKey(nameof(MetodoPagoId))]
    public MetodoPago? MetodoPago { get; set; }

    public int? Cuotas { get; set; }
    public int? CuotasPagadas { get; set; }
}
