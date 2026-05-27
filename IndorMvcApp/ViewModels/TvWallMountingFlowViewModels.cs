using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class TvWallMountingIncludedItemViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class TvWallMountingBestForOptionViewModel
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-house";
}

public class TvWallMountingServiceViewModel
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
    public List<TvWallMountingIncludedItemViewModel> IncludedItems { get; set; } = new();
    public string BestForLabel { get; set; } = "Best for";
    public List<TvWallMountingBestForOptionViewModel> BestForOptions { get; set; } = new();
    public string? InfoBoxTexto { get; set; }
    public string EstimatedTimeLabel { get; set; } = "Estimated time";
    public string EstimatedTimeValue { get; set; } = string.Empty;
    public string BestTimingLabel { get; set; } = "Best recommendation";
    public string BestTimingValue { get; set; } = string.Empty;
    public string CtaContinueTexto { get; set; } = "Continue";
    public string CtaUploadTexto { get; set; } = "Upload photos first";
}

public class TvWallMountingProjectViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "TV Wall Mounting";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string TipoSolicitud { get; set; } = "MountTv";

    [Required]
    public string TamanoTv { get; set; } = "Size43_55";

    [Required]
    public string CantidadTvs { get; set; } = "One";

    [Required]
    public string Habitacion { get; set; } = "LivingRoom";

    [Required]
    public string TipoPared { get; set; } = "Drywall";

    [Required]
    public string TieneSoporte { get; set; } = "YesHaveIt";
}

public class TvWallMountingPrepareViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "TV Wall Mounting";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string HabitacionLabel { get; set; } = string.Empty;

    [Required]
    public string ConfiguracionCables { get; set; } = "BasicVisible";

    [Required]
    public string TomaCercana { get; set; } = "Yes";

    [Required]
    public string MontajePrevio { get; set; } = "No";

    [Required]
    public string DetallesAcceso { get; set; } = "GroundFloor";

    [Required]
    public string VentanaHorario { get; set; } = "Afternoon";

    public DateTime? FechaServicio { get; set; }

    [MaxLength(250)]
    public string? NotaCorta { get; set; }

    public List<ExistingTvWallMountingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class ExistingTvWallMountingFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class TvWallMountingReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Review & Confirm";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TipoSolicitudLabel { get; set; } = string.Empty;
    public string TamanoTvLabel { get; set; } = string.Empty;
    public string CantidadTvsLabel { get; set; } = string.Empty;
    public string HabitacionLabel { get; set; } = string.Empty;
    public string TipoParedLabel { get; set; } = string.Empty;
    public string TieneSoporteLabel { get; set; } = string.Empty;
    public string ConfiguracionCablesLabel { get; set; } = string.Empty;
    public string TomaCercanaLabel { get; set; } = string.Empty;
    public string MontajePrevioLabel { get; set; } = string.Empty;
    public string AccesoLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string TiempoEstimadoLabel { get; set; } = "60-90 min";
    public decimal PrecioEstimado { get; set; }
    public string? NotaCorta { get; set; }
    public string? DisclaimerTexto { get; set; }

    [Required]
    public bool AceptaDisclaimer { get; set; }

    public List<ExistingTvWallMountingFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class TvWallMountingConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string TamanoTvLabel { get; set; } = string.Empty;
    public string HabitacionLabel { get; set; } = string.Empty;
    public string TipoParedLabel { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Confirmed";
}
