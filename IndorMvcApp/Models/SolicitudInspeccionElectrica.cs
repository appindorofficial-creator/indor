using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccionElectrica")]
public class SolicitudInspeccionElectrica
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

    /// <summary>BuyingHome | SafetyCheck | IssueAtHome | InspectionFollowUp</summary>
    [Required, MaxLength(40)]
    public string MotivoRevision { get; set; } = "BuyingHome";

    /// <summary>BreakerTrips | LightsFlicker | OutletsNotWorking | OldPanel | BurningSmell | GeneralReview</summary>
    [Required, MaxLength(40)]
    public string PreocupacionPrincipal { get; set; } = "GeneralReview";

    /// <summary>Yes | No | Sometimes</summary>
    [Required, MaxLength(20)]
    public string OcurreAhora { get; set; } = "No";

    /// <summary>Normal | Priority | Emergency</summary>
    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Normal";

    [MaxLength(1000)]
    public string? ComentariosProveedor { get; set; }

    /// <summary>InProgress | DetailsCompleted | FilesUploaded | Completed | Skipped</summary>
    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCitaProgramada { get; set; }

    public TimeSpan? HoraCitaProgramada { get; set; }

    public ICollection<ArchivoInspeccionElectrica> Archivos { get; set; } = new List<ArchivoInspeccionElectrica>();
}
