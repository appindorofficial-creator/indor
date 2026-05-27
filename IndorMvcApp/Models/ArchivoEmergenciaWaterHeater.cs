using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndorMvcApp.Models;

[Table("ArchivosEmergenciaWaterHeater")]
public class ArchivoEmergenciaWaterHeater
{
    public int Id { get; set; }

    public int SolicitudEmergenciaWaterHeaterId { get; set; }

    [ForeignKey(nameof(SolicitudEmergenciaWaterHeaterId))]
    public SolicitudEmergenciaWaterHeater? Solicitud { get; set; }

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
