using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

public class MetodoPago
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    [Required, StringLength(30)]
    public string Tipo { get; set; } = "Tarjeta"; // Tarjeta | Banco | PayPal

    [StringLength(30)]
    public string? Marca { get; set; } // Visa | Mastercard | Amex | Bancolombia | etc.

    [StringLength(10)]
    public string? Ultimos4 { get; set; }

    [StringLength(100)]
    public string? Titular { get; set; }

    [StringLength(7)]
    public string? Expiracion { get; set; } // MM/YY

    public bool EsPredeterminado { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
