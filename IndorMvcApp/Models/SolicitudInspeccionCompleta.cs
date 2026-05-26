using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccionCompleta")]
public class SolicitudInspeccionCompleta
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

    /// <summary>BuyingHome | AnnualReview | SellingHome | InspectionFollowUp</summary>
    [Required, MaxLength(40)]
    public string MotivoInspeccion { get; set; } = "BuyingHome";

    /// <summary>Pipe-separated: Electrical|HVAC|GeneralStructure|...</summary>
    [Required, MaxLength(200)]
    public string AreasEnfoque { get; set; } = "Electrical|HVAC|GeneralStructure";

    [MaxLength(50)]
    public string? TamanoPropiedad { get; set; }

    /// <summary>Yes | No | NotSure</summary>
    [Required, MaxLength(20)]
    public string EsUrgente { get; set; } = "No";

    [MaxLength(1000)]
    public string? ComentariosProveedor { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCitaProgramada { get; set; }

    public TimeSpan? HoraCitaProgramada { get; set; }

    /// <summary>InProgress | DetailsCompleted | FilesUploaded | Confirmed | Skipped</summary>
    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoInspeccionCompleta> Archivos { get; set; } = new List<ArchivoInspeccionCompleta>();
}
