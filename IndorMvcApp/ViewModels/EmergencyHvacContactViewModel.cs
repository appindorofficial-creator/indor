using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class ExistingEmergencyHvacFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class EmergencyHvacContactViewModel
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
    public string PuedeLlamarYa { get; set; } = "Yes";

    [Required, MaxLength(20)]
    public string EnCasaAhora { get; set; } = "Yes";

    public List<ExistingEmergencyHvacFileViewModel> ArchivosExistentes { get; set; } = new();
}
