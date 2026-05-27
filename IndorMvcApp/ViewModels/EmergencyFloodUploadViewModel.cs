using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyFloodUploadViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    public List<ExistingEmergencyFloodFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingEmergencyFloodFileViewModel
{
    public int Id { get; set; }

    public string NombreArchivo { get; set; } = string.Empty;

    public string RutaArchivo { get; set; } = string.Empty;

    public string? CategoriaArchivo { get; set; }
}
