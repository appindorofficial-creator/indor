using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesGeneralHelp")]
public class SolicitudGeneralHelp
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int MovingSetupServicioId { get; set; }

    [ForeignKey(nameof(MovingSetupServicioId))]
    public MovingSetupServicio? MovingSetupServicio { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [MaxLength(30)]
    public string? TipoAyuda { get; set; }

    [MaxLength(30)]
    public string? VentanaTiempo { get; set; }

    [MaxLength(20)]
    public string? Urgencia { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string? ContactoPreferido { get; set; }

    [MaxLength(120)]
    public string? NotasAcceso { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoGeneralHelp> Archivos { get; set; } = new List<ArchivoGeneralHelp>();
}

[Table("ArchivosGeneralHelp")]
public class ArchivoGeneralHelp
{
    public int Id { get; set; }
    public int SolicitudGeneralHelpId { get; set; }

    [ForeignKey(nameof(SolicitudGeneralHelpId))]
    public SolicitudGeneralHelp? Solicitud { get; set; }

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
