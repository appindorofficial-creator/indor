using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccionPlomeria")]
public class SolicitudInspeccionPlomeria
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int InspeccionId { get; set; }

    [ForeignKey(nameof(InspeccionId))]
    public Inspeccion? Inspeccion { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "KitchenIssue";

    [Required, MaxLength(30)]
    public string UbicacionProblema { get; set; } = "Kitchen";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Normal";

    [Required, MaxLength(20)]
    public string FugaAguaAhora { get; set; } = "No";

    [MaxLength(300)]
    public string? SituacionesActuales { get; set; }

    [MaxLength(20)]
    public string? CuandoEmpezo { get; set; }

    [MaxLength(20)]
    public string? AguaCerrada { get; set; }

    [MaxLength(500)]
    public string? DescripcionProblema { get; set; }

    [MaxLength(200)]
    public string? NotasAdicionales { get; set; }

    [MaxLength(1000)]
    public string? ComentariosProveedor { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCitaProgramada { get; set; }

    public TimeSpan? HoraCitaProgramada { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoInspeccionPlomeria> Archivos { get; set; } = new List<ArchivoInspeccionPlomeria>();
}
