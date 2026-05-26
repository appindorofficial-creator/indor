using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesInspeccionStructural")]
public class SolicitudInspeccionStructural
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
    public string MotivoRevision { get; set; } = "BuyingHome";

    [Required, MaxLength(40)]
    public string TipoPreocupacion { get; set; } = "FoundationCrack";

    [MaxLength(300)]
    public string? TiposPreocupacion { get; set; }

    [Required, MaxLength(30)]
    public string AreaPreocupacion { get; set; } = "Foundation";

    [Required, MaxLength(20)]
    public string Urgencia { get; set; } = "Normal";

    [Required, MaxLength(20)]
    public string DanoVisible { get; set; } = "Yes";

    [MaxLength(300)]
    public string? SignosVisibles { get; set; }

    [MaxLength(20)]
    public string? SeveridadApariencia { get; set; }

    [MaxLength(200)]
    public string? UbicacionEspecifica { get; set; }

    [MaxLength(50)]
    public string? CuandoNotadoTexto { get; set; }

    [MaxLength(20)]
    public string? DuracionProblema { get; set; }

    [MaxLength(20)]
    public string? Severidad { get; set; }

    [MaxLength(20)]
    public string? ReparacionesPrevias { get; set; }

    [MaxLength(200)]
    public string? CondicionesInseguras { get; set; }

    [MaxLength(20)]
    public string? MejorHorarioVisita { get; set; }

    [MaxLength(30)]
    public string? TipoPropiedad { get; set; }

    [MaxLength(20)]
    public string? TipoFundacion { get; set; }

    [MaxLength(10)]
    public string? TieneReporte { get; set; }

    [MaxLength(200)]
    public string? CambiosRecientes { get; set; }

    [MaxLength(30)]
    public string? AccesoPreferido { get; set; }

    [MaxLength(200)]
    public string? AreasEnfoque { get; set; }

    [MaxLength(20)]
    public string? CuandoNotado { get; set; }

    [MaxLength(20)]
    public string? EdadPropiedad { get; set; }

    [MaxLength(20)]
    public string? RemodelReciente { get; set; }

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

    public ICollection<ArchivoInspeccionStructural> Archivos { get; set; } = new List<ArchivoInspeccionStructural>();
}
