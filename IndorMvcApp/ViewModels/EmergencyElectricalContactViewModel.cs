using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyElectricalContactViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? NotaCorta { get; set; }

    [Required, MaxLength(30)]
    public string TelefonoContacto { get; set; } = string.Empty;

    [MaxLength(10)]
    public string AceptaTextos { get; set; } = "Yes";

    public List<ExistingEmergencyElectricalFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingEmergencyElectricalFileViewModel
{
    public int Id { get; set; }

    public string NombreArchivo { get; set; } = string.Empty;

    public string RutaArchivo { get; set; } = string.Empty;

    public string? CategoriaArchivo { get; set; }
}
