using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccion")]
public class SolicitudInspeccion
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

    public bool BajoContrato { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCierreEstimada { get; set; }

    public bool TieneReporteExistente { get; set; }

    /// <summary>Buyer | Realtor | Investor</summary>
    [Required, MaxLength(30)]
    public string RolComprador { get; set; } = "Buyer";

    /// <summary>BuyWithConfidence | UnderstandRepairRisks | NegotiateRepairs | SecondOpinion</summary>
    [Required, MaxLength(50)]
    public string ObjetivoPrincipal { get; set; } = "UnderstandRepairRisks";

    [MaxLength(250)]
    public string? NotasRevision { get; set; }

    /// <summary>InProgress | ReportPending | ReportUploaded | Completed | Skipped</summary>
    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCitaProgramada { get; set; }

    public TimeSpan? HoraCitaProgramada { get; set; }

    public ICollection<ArchivoReporteInspeccion> Archivos { get; set; } = new List<ArchivoReporteInspeccion>();
}
