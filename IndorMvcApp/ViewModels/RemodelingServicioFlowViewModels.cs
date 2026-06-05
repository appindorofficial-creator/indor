using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class RemodelingServiceViewModel
{
    public int SolicitudId { get; set; }
    public int ServicioId { get; set; }
    public string PageTitle { get; set; } = "Remodeling";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingSubtitulo { get; set; }
    public string? ImagenUrl { get; set; }
    public IReadOnlyList<string> IncludedItems { get; set; } = Array.Empty<string>();
    public string CtaTexto { get; set; } = "Start my project";
}

public class RemodelingDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int ServicioId { get; set; }
    public string PageTitle { get; set; } = "Project details";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string AlcanceProyecto { get; set; } = "FullRemodel";

    [Required]
    public string VentanaTiempo { get; set; } = "Flexible";

    [Required]
    public string PresupuestoEstimado { get; set; } = "NotSure";

    [Required, MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public string ContactoPreferido { get; set; } = "Text";

    public List<ExistingRemodelingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingRemodelingFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class RemodelingReviewViewModel
{
    public int SolicitudId { get; set; }
    public int ServicioId { get; set; }
    public string PageTitle { get; set; } = "Review request";
    public string NombreServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string AlcanceLabel { get; set; } = string.Empty;
    public string VentanaTiempoLabel { get; set; } = string.Empty;
    public string PresupuestoLabel { get; set; } = string.Empty;
    public string ContactoPreferidoLabel { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public List<ExistingRemodelingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class RemodelingSentViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string VentanaTiempoLabel { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Pending quote";
}
