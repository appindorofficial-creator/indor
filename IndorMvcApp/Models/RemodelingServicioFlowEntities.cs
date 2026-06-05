using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("SolicitudesRemodelingServicio")]
public class SolicitudRemodelingServicio
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? Usuario { get; set; }

    public int ServicioId { get; set; }

    [ForeignKey(nameof(ServicioId))]
    public Servicio? Servicio { get; set; }

    public int? PropiedadId { get; set; }

    [ForeignKey(nameof(PropiedadId))]
    public Propiedad? Propiedad { get; set; }

    [MaxLength(300)]
    public string? DireccionPropiedad { get; set; }

    [MaxLength(40)]
    public string? AlcanceProyecto { get; set; }

    [MaxLength(30)]
    public string? VentanaTiempo { get; set; }

    [MaxLength(30)]
    public string? PresupuestoEstimado { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string? ContactoPreferido { get; set; }

    [Required, MaxLength(30)]
    public string Estado { get; set; } = "InProgress";

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<ArchivoRemodelingServicio> Archivos { get; set; } = new List<ArchivoRemodelingServicio>();
}

[Table("ArchivosRemodelingServicio")]
public class ArchivoRemodelingServicio
{
    public int Id { get; set; }
    public int SolicitudRemodelingServicioId { get; set; }

    [ForeignKey(nameof(SolicitudRemodelingServicioId))]
    public SolicitudRemodelingServicio? Solicitud { get; set; }

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
