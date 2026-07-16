using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public class CleaningIncludedItemViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class CleaningBestForOptionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-house";
}

public class CleaningServiceViewModel
{
    public int MovingSetupServicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Cleaning Service";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public List<CleaningIncludedItemViewModel> IncludedItems { get; set; } = new();
    public string BestForLabel { get; set; } = "Best for";
    public List<CleaningBestForOptionViewModel> BestForOptions { get; set; } = new();
    public string? InfoBoxTitulo { get; set; }
    public string? InfoBoxTexto { get; set; }
    public string CtaContinueTexto { get; set; } = "Continue";
    public string CtaUploadTexto { get; set; } = "Upload photos";

    [Required]
    public string BestForSelection { get; set; } = string.Empty;
}

public class CleaningDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Cleaning Details";

    [Required, MaxLength(300)]
    [ValidStreetAddress(RequireCityOrZip = true)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select a cleaning type.")]
    public string TipoLimpieza { get; set; } = "";

    [Required(ErrorMessage = "Select a property type.")]
    public string TipoPropiedad { get; set; } = "";

    [Required(ErrorMessage = "Select the number of bedrooms.")]
    public string NumeroHabitaciones { get; set; } = "";

    [Required(ErrorMessage = "Select the number of bathrooms.")]
    public string NumeroBanos { get; set; } = "";

    [Required(ErrorMessage = "Select the current condition.")]
    public string CondicionActual { get; set; } = "";

    public DateTime? FechaServicio { get; set; }

    public string? VentanaHorario { get; set; }
}

public class CleaningTasksViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Cleaning Details";
    public string DireccionPropiedad { get; set; } = string.Empty;

    public string AreasPrioridad { get; set; } = string.Empty;
    public string TareasExtra { get; set; } = string.Empty;

    [Required]
    public string SuministrosNecesarios { get; set; } = "Yes";

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    public List<ExistingCleaningFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingCleaningFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class CleaningReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Review & Request";
    public string TipoLimpiezaLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TamanoHogarLabel { get; set; } = string.Empty;
    public string CondicionLabel { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string AreasResumen { get; set; } = string.Empty;
    public string TareasResumen { get; set; } = string.Empty;
    public string MetodoAccesoLabel { get; set; } = string.Empty;
    public string SuministrosLabel { get; set; } = string.Empty;
    public string? NotaCorta { get; set; }
    public decimal PrecioEstimado { get; set; }
    public string? DisclaimerTexto { get; set; }
}

public class CleaningConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string TipoLimpiezaLabel { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string AreasResumen { get; set; } = string.Empty;
    public string TareasResumen { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Confirmed";
}
