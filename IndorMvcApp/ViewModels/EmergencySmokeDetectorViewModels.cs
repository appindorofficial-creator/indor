using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class EmergencySmokeDetectorDetailsViewModel
{
    public int ServicioEmergenciaId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string TituloServicio { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string TiposProblema { get; set; } = "SmokeDetectorBeeping";

    [Required, MaxLength(200)]
    public string UbicacionesDetectores { get; set; } = "Hallway";

    [Required, MaxLength(30)]
    public string SituacionActual { get; set; } = "IntermittentChirp";

    [Required, MaxLength(20)]
    public string PuedePermanecerAdentro { get; set; } = "Yes";

    public string Urgencia { get; set; } = "Emergency";
}

public class EmergencySmokeDetectorYourInfoViewModel
{
    public int SolicitudId { get; set; }
    public int ServicioEmergenciaId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string TituloServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string ProblemaResumen { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string AccesoPropiedad { get; set; } = "AdultHomeNow";

    [Required, MaxLength(30)]
    public string TelefonoContacto { get; set; } = string.Empty;
}

public class EmergencySmokeDetectorReviewViewModel
{
    public int SolicitudId { get; set; }
    public int ServicioEmergenciaId { get; set; }
    public string TituloServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string ProblemaResumen { get; set; } = string.Empty;
    public string UbicacionesResumen { get; set; } = string.Empty;
    public string SeguridadResumen { get; set; } = string.Empty;
    public string OlorGasResumen { get; set; } = string.Empty;
    public string AccesoResumen { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotaCorta { get; set; }
}

public class EmergencySmokeDetectorSubmittedViewModel
{
    public int SolicitudId { get; set; }
    public string TituloServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string PreocupacionResumen { get; set; } = string.Empty;
    public string UbicacionesResumen { get; set; } = string.Empty;
    public string UrgenciaResumen { get; set; } = string.Empty;
    public string EstadoResumen { get; set; } = string.Empty;
    public string TiempoCallbackRango { get; set; } = string.Empty;
}
