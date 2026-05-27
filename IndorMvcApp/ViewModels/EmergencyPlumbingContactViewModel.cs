using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class ExistingEmergencyPlumbingFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class EmergencyPlumbingContactViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    [Required, MaxLength(30)]
    public string TelefonoContacto { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string AccesoSiAusente { get; set; } = "Yes";

    public List<ExistingEmergencyPlumbingFileViewModel> ArchivosExistentes { get; set; } = new();
}
