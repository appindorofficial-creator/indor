using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

// Propiedad asociada a un usuario. Guardamos el JSON crudo de la API
// para poder consultar cualquier atributo más adelante.
public class Propiedad
{
    [Key]
    public int Id { get; set; }

    [StringLength(500)]
    public string? Direccion { get; set; }

    // JSON crudo devuelto por la API de búsqueda de direcciones
    public string DatosJson { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public bool Activo { get; set; } = true;

    public long? AttomPropertyId { get; set; }

    public string? AttomRawJson { get; set; }

    public DateTime? AttomLastSyncUtc { get; set; }

    [MaxLength(30)]
    public string? AttomSyncStatus { get; set; }

    [MaxLength(500)]
    public string? AttomSyncError { get; set; }

    public string? MantenimientoRecomendadoJson { get; set; }

    public DateTime? MantenimientoRecomendadoUtc { get; set; }

    // Owner relationship
    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }
}
