using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("ProgramacionesMicroservicio")]
public class ProgramacionMicroservicio
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int MicroservicioId { get; set; }

    [ForeignKey(nameof(MicroservicioId))]
    public Microservicio? Microservicio { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Column(TypeName = "date")]
    public DateTime FechaProgramada { get; set; }

    [StringLength(500)]
    public string? Notas { get; set; }

    /// <summary>Scheduled | Completed | Cancelled</summary>
    [Required, StringLength(30)]
    public string Estado { get; set; } = "Scheduled";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }
}
