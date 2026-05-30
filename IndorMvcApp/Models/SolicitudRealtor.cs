using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesRealtor")]
public class SolicitudRealtor
{
    public int Id { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string NeedType { get; set; } = "Buy";

    [MaxLength(120)]
    public string? PreferredArea { get; set; }

    [Required, MaxLength(20)]
    public string Timeframe { get; set; } = "ASAP";

    [MaxLength(80)]
    public string? PriceRange { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "MatchingInProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? FechaActualizacion { get; set; }
}
