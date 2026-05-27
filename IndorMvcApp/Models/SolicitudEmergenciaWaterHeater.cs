using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesEmergenciaWaterHeater")]
public class SolicitudEmergenciaWaterHeater
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

    [MaxLength(300)]
    public string? TiposProblema { get; set; }

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "NoHotWater";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Emergency";

    [Required, MaxLength(20)]
    public string UnidadFuncionando { get; set; } = "No";

    [MaxLength(30)]
    public string? UbicacionProblema { get; set; } = "Garage";

    [MaxLength(20)]
    public string? TipoUnidad { get; set; } = "Gas";

    [MaxLength(300)]
    public string? SintomasVisibles { get; set; }

    [MaxLength(250)]
    public string? NotaCorta { get; set; }

    [MaxLength(500)]
    public string? DetallesAcceso { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoEmergenciaWaterHeater> Archivos { get; set; } = new List<ArchivoEmergenciaWaterHeater>();
}
