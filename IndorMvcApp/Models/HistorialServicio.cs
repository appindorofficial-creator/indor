using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

public class HistorialServicio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    [Required, StringLength(30)]
    public string Tipo { get; set; } = "Microservicio"; // Microservicio | Inspeccion | Servicio

    public int? ItemId { get; set; }

    [Required, StringLength(200)]
    public string NombreItem { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string? Estado { get; set; } // Completado | En curso | Cancelado

    public decimal? Monto { get; set; }

    [StringLength(10)]
    public string? Moneda { get; set; }

    [StringLength(500)]
    public string? Notas { get; set; }
}
