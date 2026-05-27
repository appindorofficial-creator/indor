using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesEmergenciaRoofLeak")]
public class SolicitudEmergenciaRoofLeak
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int ServicioEmergenciaId { get; set; }

    [ForeignKey(nameof(ServicioEmergenciaId))]
    public ServicioEmergencia? ServicioEmergencia { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "ActiveDripping";

    [Required, MaxLength(30)]
    public string UbicacionFuga { get; set; } = "Ceiling";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Emergency";

    [MaxLength(20)]
    public string? PuedeColocarCubeta { get; set; } = "Yes";

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoEmergenciaRoofLeak> Archivos { get; set; } = new List<ArchivoEmergenciaRoofLeak>();
}
