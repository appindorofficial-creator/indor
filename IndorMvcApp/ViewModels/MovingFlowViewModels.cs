using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public class MovingIncludedItemViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class MovingMoveTypeOptionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-truck";
}

public class MovingServiceViewModel
{
    public int MovingSetupServicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Moving Service";
    public string LandingTitulo { get; set; } = string.Empty;
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public List<MovingIncludedItemViewModel> IncludedItems { get; set; } = new();
    public string EstimatedTimeLabel { get; set; } = "Estimated time";
    public string EstimatedTimeValue { get; set; } = string.Empty;
    public string? EstimatedTimeNote { get; set; }
    public string BestForLabel { get; set; } = "Best for";
    public string BestForValue { get; set; } = string.Empty;
    public string? BestForNote { get; set; }
    public List<MovingMoveTypeOptionViewModel> MoveTypes { get; set; } = new();
    public string CtaContinueTexto { get; set; } = "Continue";
    public string CtaEstimateTexto { get; set; } = "Get estimate";

    [Required]
    public string TipoMovimiento { get; set; } = "MoveIn";
}

public class MovingDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Moving Details";

    [Required]
    public string TipoMovimiento { get; set; } = "MoveIn";

    [Required]
    public string TipoPropiedad { get; set; } = "Apartment";

    [Required]
    public string TamanoHogar { get; set; } = "OneTwoBedrooms";

    [Required(ErrorMessage = "Please enter the pick-up address."), MaxLength(300)]
    [ValidStreetAddress]
    public string DireccionOrigen { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the drop-off address."), MaxLength(300)]
    [ValidStreetAddress]
    public string DireccionDestino { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a move date.")]
    public DateTime? FechaMovimiento { get; set; }

    [Required]
    public string VentanaHorario { get; set; } = "Morning";

    [Required]
    public string TipoServicio { get; set; } = "MoversOnly";
}

public class MovingItemsViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Move Items & Access";

    public string ItemsMover { get; set; } = string.Empty;

    [Required]
    public string TamanoMovimiento { get; set; } = "OneTwoBedroom";

    public string CondicionesAcceso { get; set; } = string.Empty;

    [Required]
    public string RequiereMontaje { get; set; } = "No";

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    public List<ExistingMovingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingMovingFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class MovingReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Review & Confirm";
    public string TipoMovimientoLabel { get; set; } = string.Empty;
    public string TipoPropiedadLabel { get; set; } = string.Empty;
    public string TamanoHogarLabel { get; set; } = string.Empty;
    public string DireccionOrigen { get; set; } = string.Empty;
    public string DireccionDestino { get; set; } = string.Empty;
    public string FechaMovimientoLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string TipoServicioLabel { get; set; } = string.Empty;
    public string ItemsResumen { get; set; } = string.Empty;
    public string TamanoMovimientoLabel { get; set; } = string.Empty;
    public string AccesoResumen { get; set; } = string.Empty;
    public string RequiereMontajeLabel { get; set; } = string.Empty;
    public string? NotaCorta { get; set; }
    public decimal PrecioEstimadoMin { get; set; }
    public decimal PrecioEstimadoMax { get; set; }
    public int DuracionEstimadaMinHoras { get; set; }
    public int DuracionEstimadaMaxHoras { get; set; }
    public string? DisclaimerTexto { get; set; }
}

public class MovingConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string TipoMovimientoLabel { get; set; } = string.Empty;
    public string FechaMovimientoLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string DireccionOrigen { get; set; } = string.Empty;
    public string DireccionDestino { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Confirmed";
}
