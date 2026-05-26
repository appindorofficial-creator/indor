using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccionHvac")]
public class SolicitudInspeccionHvac
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
    public string TipoProblema { get; set; } = "NotCooling";

    [Required, MaxLength(30)]
    public string ParteAtencion { get; set; } = "WholeSystem";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Normal";

    [Required, MaxLength(20)]
    public string SistemaFuncionando { get; set; } = "Yes";

    [MaxLength(30)]
    public string? TipoEquipo { get; set; }

    [MaxLength(10)]
    public string? CantidadSistemas { get; set; }

    [MaxLength(200)]
    public string? ComponentesRevision { get; set; }

    [MaxLength(20)]
    public string? EdadSistema { get; set; }

    [MaxLength(20)]
    public string? FiltroCambiado { get; set; }

    [MaxLength(20)]
    public string? TipoTermostato { get; set; }

    [MaxLength(500)]
    public string? DescripcionProblema { get; set; }

    [MaxLength(200)]
    public string? NotasOpcionales { get; set; }

    [MaxLength(1000)]
    public string? ComentariosProveedor { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaCitaProgramada { get; set; }

    public TimeSpan? HoraCitaProgramada { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    public ICollection<ArchivoInspeccionHvac> Archivos { get; set; } = new List<ArchivoInspeccionHvac>();
}
