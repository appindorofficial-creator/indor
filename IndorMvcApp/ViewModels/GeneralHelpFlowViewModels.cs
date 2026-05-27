using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class GeneralHelpRequestViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "General Help";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string TipoAyuda { get; set; } = "ExtraHands";

    [Required]
    public string VentanaTiempo { get; set; } = "Tomorrow";

    [Required]
    public string Urgencia { get; set; } = "Normal";
}

public class GeneralHelpDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "General Help";
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public string ContactoPreferido { get; set; } = "Text";

    [Required]
    public string NotasAcceso { get; set; } = "Apartment";

    public List<ExistingGeneralHelpFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingGeneralHelpFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class GeneralHelpReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string PageTitle { get; set; } = "Review Request";
    public string NombreServicio { get; set; } = "General Help";
    public string TipoAyudaLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string VentanaTiempoLabel { get; set; } = string.Empty;
    public string UrgenciaLabel { get; set; } = string.Empty;
    public string ContactoPreferidoLabel { get; set; } = string.Empty;
    public string AccesoLabel { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<ExistingGeneralHelpFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class GeneralHelpSentViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = "General Help";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string VentanaTiempoLabel { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Pending confirmation";
}
