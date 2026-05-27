using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencyElectricalLocationViewModel
{
    public int SolicitudId { get; set; }

    public int ServicioEmergenciaId { get; set; }

    public string NombreServicio { get; set; } = string.Empty;

    public string TituloServicio { get; set; } = string.Empty;

    public string DireccionPropiedad { get; set; } = string.Empty;

    public string ProblemaResumen { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string UbicacionProblema { get; set; } = "Garage";

    [Required, MaxLength(300)]
    public string SintomasNotados { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string EnergiaEncendida { get; set; } = "Yes";

    [Required, MaxLength(20)]
    public string PuedeAlejarse { get; set; } = "Yes";
}
