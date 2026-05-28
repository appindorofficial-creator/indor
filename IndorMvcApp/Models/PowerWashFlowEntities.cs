using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("PowerWashServicioLanding")]
public class PowerWashServicioLanding
{
    public int Id { get; set; }
    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    [Required, MaxLength(80)]
    public string PageTitle { get; set; } = "Power Wash Exterior";

    [Required, MaxLength(120)]
    public string LandingTitulo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LandingTagline { get; set; }

    [MaxLength(500)]
    public string? InfoBoxTexto { get; set; }

    [MaxLength(300)]
    public string? ImagenUrl { get; set; }

    [MaxLength(500)]
    public string? BestForItems { get; set; }

    [MaxLength(300)]
    public string? BestForIconos { get; set; }

    [MaxLength(400)]
    public string? PreviewTexto { get; set; }

    [MaxLength(300)]
    public string? TipConfirmacionTexto { get; set; }

    [MaxLength(400)]
    public string? InfoCondicionTexto { get; set; }

    [Required, MaxLength(40)]
    public string CtaTexto { get; set; } = "Start exterior check";

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

[Table("SolicitudesPowerWash")]
public class SolicitudPowerWash
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int HomeCarePriorityId { get; set; }

    [ForeignKey(nameof(HomeCarePriorityId))]
    public HomeCarePriority? HomeCarePriority { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [MaxLength(200)]
    public string? AreasSeleccionadas { get; set; }

    [MaxLength(30)]
    public string? MaterialExterior { get; set; }

    [MaxLength(10)]
    public string? NumeroPisos { get; set; }

    [MaxLength(200)]
    public string? ProblemasSeleccionados { get; set; }

    [MaxLength(200)]
    public string? AreasDelicadas { get; set; }

    [MaxLength(10)]
    public string? AccesoGrifo { get; set; }

    [MaxLength(20)]
    public string? TimingPreferido { get; set; }

    [MaxLength(20)]
    public string? VentanaHorario { get; set; }

    [Column(TypeName = "date")]
    public DateTime? FechaPreferida { get; set; }

    public bool RecordatorioAnual { get; set; }

    [MaxLength(300)]
    public string? Notas { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioEstimado { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoPowerWash> Archivos { get; set; } = new List<ArchivoPowerWash>();
}

[Table("ArchivosPowerWash")]
public class ArchivoPowerWash
{
    public int Id { get; set; }
    public int SolicitudPowerWashId { get; set; }

    [ForeignKey(nameof(SolicitudPowerWashId))]
    public SolicitudPowerWash? Solicitud { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TipoContenido { get; set; }

    public long TamanoBytes { get; set; }
    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
