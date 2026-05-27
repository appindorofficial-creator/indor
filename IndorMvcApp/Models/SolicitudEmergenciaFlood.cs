using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesEmergenciaFlood")]
public class SolicitudEmergenciaFlood
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
    public string CausaAgua { get; set; } = "UnknownSource";

    [Required, MaxLength(30)]
    public string UbicacionAgua { get; set; } = "FirstFloor";

    [Required, MaxLength(20)]
    public string AguaActiva { get; set; } = "Yes";

    [MaxLength(20)]
    public string? PuedeCerrarAgua { get; set; } = "NotSure";

    [MaxLength(20)]
    public string? UbicacionCierreAgua { get; set; } = "DontKnow";

    [MaxLength(20)]
    public string? PuedeApagarElectricidad { get; set; } = "NotSure";

    [MaxLength(20)]
    public string? CantidadAgua { get; set; } = "OneRoom";

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoEmergenciaFlood> Archivos { get; set; } = new List<ArchivoEmergenciaFlood>();
}
