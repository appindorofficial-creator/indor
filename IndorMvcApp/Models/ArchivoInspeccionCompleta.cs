using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("ArchivosInspeccionCompleta")]
public class ArchivoInspeccionCompleta
{
    public int Id { get; set; }

    public int SolicitudInspeccionCompletaId { get; set; }

    [ForeignKey(nameof(SolicitudInspeccionCompletaId))]
    public SolicitudInspeccionCompleta? Solicitud { get; set; }

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
