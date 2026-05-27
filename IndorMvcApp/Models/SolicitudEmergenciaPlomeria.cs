using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesEmergenciaPlomeria")]
public class SolicitudEmergenciaPlomeria
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
    public string TipoProblema { get; set; } = "BurstPipe";

    [Required, MaxLength(20)]
    public string AguaFluyendo { get; set; } = "Yes";

    [Required, MaxLength(20)]
    public string PuedeCerrarAgua { get; set; } = "Yes";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Emergency";

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [MaxLength(30)]
    public string? TelefonoContacto { get; set; }

    [MaxLength(20)]
    public string? AccesoSiAusente { get; set; } = "Yes";

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoEmergenciaPlomeria> Archivos { get; set; } = new List<ArchivoEmergenciaPlomeria>();
}
