using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccionMoldMoisture")]
public class SolicitudInspeccionMoldMoisture
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

    [MaxLength(300)]
    public string? TiposProblema { get; set; }

    [Required, MaxLength(40)]
    public string TipoProblema { get; set; } = "VisibleMold";

    [Required, MaxLength(30)]
    public string UbicacionProblema { get; set; } = "Bathroom";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Normal";

    [Required, MaxLength(20)]
    public string HumedadActiva { get; set; } = "Yes";

    [MaxLength(30)]
    public string? MotivoRevision { get; set; }

    [MaxLength(20)]
    public string? TipoPropiedad { get; set; }

    [MaxLength(30)]
    public string? UbicacionPrincipal { get; set; }

    [MaxLength(20)]
    public string? IntrusionAguaReciente { get; set; }

    [MaxLength(20)]
    public string? AccesoPreferido { get; set; }

    [MaxLength(200)]
    public string? AreasEnfoque { get; set; }

    [MaxLength(500)]
    public string? ComentariosProveedor { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCitaProgramada { get; set; }

    public TimeSpan? HoraCitaProgramada { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoInspeccionMoldMoisture> Archivos { get; set; } = new List<ArchivoInspeccionMoldMoisture>();
}
