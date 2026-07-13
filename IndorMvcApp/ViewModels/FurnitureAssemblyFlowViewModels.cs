using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public class FurnitureAssemblyBadgeViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class FurnitureAssemblyIncludedItemViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-check";
}

public class FurnitureAssemblyServiceViewModel
{
    public int MovingSetupServicioId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Furniture & Assembly";
    public string LandingTitulo { get; set; } = string.Empty;
    public string LandingSubtitulo { get; set; } = string.Empty;
    public string? ImagenUrl { get; set; }
    public decimal PrecioDesde { get; set; }
    public List<FurnitureAssemblyBadgeViewModel> Badges { get; set; } = new();
    public List<FurnitureAssemblyIncludedItemViewModel> IncludedItems { get; set; } = new();
    public string EstimatedTimeLabel { get; set; } = "Estimated time";
    public string EstimatedTimeValue { get; set; } = string.Empty;
    public string? EstimatedTimeNote { get; set; }
    public string BestForLabel { get; set; } = "Best for";
    public string BestForValue { get; set; } = string.Empty;
    public string? BestForNote { get; set; }
    public string CtaContinueTexto { get; set; } = "Continue";
    public string CtaUploadTexto { get; set; } = "Upload photos or manuals";
}

public class FurnitureAssemblyItemsViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Furniture & Assembly Details";

    [Required, MaxLength(300)]
    [ValidStreetAddress(RequireCityOrZip = true)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    public string TiposMueble { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select how many items need assembly.")]
    public string CantidadItems { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select the condition of your items.")]
    public string CondicionItems { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select whether any item needs wall anchoring.")]
    public string AnclajePared { get; set; } = string.Empty;
}

public class FurnitureAssemblyPreferencesViewModel : IValidatableObject
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Furniture & Assembly Details";

    [Required, MaxLength(300)]
    public string DireccionPropiedad { get; set; } = string.Empty;

    [Required]
    public string Habitacion { get; set; } = string.Empty;

    [Required]
    public string DetallesAcceso { get; set; } = string.Empty;

    [Required]
    public string AyudaMover { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select a preferred date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Preferred date")]
    public DateTime? FechaServicio { get; set; }

    [Required]
    public string VentanaHorario { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? NotaCorta { get; set; }

    public string MinServiceDateIso { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaServicio is null || FechaServicio.Value.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "Please select today or a future date.",
                [nameof(FechaServicio)]);
        }
    }
}

public class ExistingFurnitureAssemblyFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? CategoriaArchivo { get; set; }
}

public class FurnitureAssemblyReviewViewModel
{
    public int SolicitudId { get; set; }
    public int MovingSetupServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string PageTitle { get; set; } = "Review & Confirm";
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string ItemsResumen { get; set; } = string.Empty;
    public string FechaHorarioLabel { get; set; } = string.Empty;
    public string HabitacionLabel { get; set; } = string.Empty;
    public string AccesoLabel { get; set; } = string.Empty;
    public string? NotaCorta { get; set; }
    public string? DisclaimerTexto { get; set; }
    public List<ExistingFurnitureAssemblyFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class FurnitureAssemblyConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string FechaServicioLabel { get; set; } = string.Empty;
    public string VentanaHorarioLabel { get; set; } = string.Empty;
    public string ItemsResumen { get; set; } = string.Empty;
    public string HabitacionLabel { get; set; } = string.Empty;
    public string AccesoLabel { get; set; } = string.Empty;
    public string EstadoLabel { get; set; } = "Confirmed";
}
