using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("ArchivosInspeccionRoof")]
public class ArchivoInspeccionRoof
{
    public int Id { get; set; }

    public int SolicitudInspeccionRoofId { get; set; }

    [ForeignKey(nameof(SolicitudInspeccionRoofId))]
    public SolicitudInspeccionRoof? Solicitud { get; set; }

    [Required, MaxLength(260)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CategoriaArchivo { get; set; }

    [MaxLength(20)]
    public string? TipoArchivo { get; set; }

    public long TamanioBytes { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.Now;
}
