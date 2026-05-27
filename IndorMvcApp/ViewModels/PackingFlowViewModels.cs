using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PackingIncludedItemViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class PackingBestForOptionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-house";
}

public class PackingServiceViewModel
{
    public int MovingSetupServicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Service Detail";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public List<PackingIncludedItemViewModel> IncludedItems { get; set; } = new();
    public string BestForLabel { get; set; } = "Best for";
    public List<PackingBestForOptionViewModel> BestForOptions { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string EstimatedTimeLabel { get; set; } = "Estimated time";
    public string EstimatedTimeValue { get; set; } = string.Empty;
    public string BestTimingLabel { get; set; } = "Best timing";
    public string BestTimingValue { get; set; } = string.Empty;
    public string CtaContinueTexto { get; set; } = "Continue";
    public string CtaUploadTexto { get; set; } = "Upload photos or list";

    public string BestForSelection { get; set; } = "MoveOut";
}

public class PackingAboutViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Packing Help";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string TipoEmpaque { get; set; } = "PartialPacking";

    [Required]
    public string CuandoMudanza { get; set; } = "ThisWeek";

    [Required]
    public string TipoPropiedad { get; set; } = "Apartment";

    [Required]
    public string TamanoHogar { get; set; } = "ThreeFourRooms";

    public DateTime? FechaServicio { get; set; }
}

public class PackingDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Packing Help";
    public string DireccionPropiedad { get; set; } = string.Empty;

    public string HabitacionesEmpacar { get; set; } = string.Empty;
    public string ItemsEspeciales { get; set; } = string.Empty;
    public string SuministrosNecesarios { get; set; } = string.Empty;
    public string DetallesAcceso { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NotaCorta { get; set; }

    public List<ExistingPackingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingPackingFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class PackingReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Packing Help";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TipoEmpaqueLabel { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string TipoPropiedadLabel { get; set; } = string.Empty;
    public string AlcanceLabel { get; set; } = string.Empty;
    public string ItemsEspecialesLabel { get; set; } = string.Empty;
    public string SuministrosLabel { get; set; } = string.Empty;
    public string AccesoLabel { get; set; } = string.Empty;
    public string? NotaCorta { get; set; }
    public string? DisclaimerTexto { get; set; }
}

public class PackingConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string AlcanceLabel { get; set; } = string.Empty;
    public string SuministrosLabel { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Confirmed";
}
