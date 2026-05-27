using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class ExistingEmergencyWaterHeaterFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class EmergencyWaterHeaterUploadViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? DetallesAcceso { get; set; }

    public List<ExistingEmergencyWaterHeaterFileViewModel> ArchivosExistentes { get; set; } = new();
}
