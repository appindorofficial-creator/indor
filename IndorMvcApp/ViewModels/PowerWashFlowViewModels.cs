using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class PowerWashFeatureItemViewModel
{
    public string Icon { get; set; } = "fa-check";
    public string Text { get; set; } = string.Empty;
}

public class PowerWashServiceViewModel
{
    public int HomeCarePriorityId { get; set; }
    public int? SolicitudId { get; set; }
    public string PageTitle { get; set; } = "Power Wash Exterior";
    public string LandingTitulo { get; set; } = string.Empty;
    public string? LandingTagline { get; set; }
    public string? InfoBoxTexto { get; set; }
    public string? ImagenUrl { get; set; }
    public List<PowerWashFeatureItemViewModel> BestForItems { get; set; } = new();
    public string? PreviewTexto { get; set; }
    public string CtaTexto { get; set; } = "Start exterior check";
}

public class PowerWashDetailsViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Power Wash Exterior";

    public string AreasSeleccionadas { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select the main exterior material.")]
    public string MaterialExterior { get; set; } = string.Empty;

    [Required(ErrorMessage = "Select your home size.")]
    public string NumeroPisos { get; set; } = string.Empty;
}

public class ExistingPowerWashFileViewModel
{
    public int Id { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
}

public class PowerWashConditionViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public string PageTitle { get; set; } = "Power Wash Exterior";
    public string? InfoCondicionTexto { get; set; }

    public string ProblemasSeleccionados { get; set; } = string.Empty;
    public string AreasDelicadas { get; set; } = string.Empty;

    [Required]
    public string AccesoGrifo { get; set; } = "";

    [Required]
    public string TimingPreferido { get; set; } = "";

    [Required]
    public string VentanaHorario { get; set; } = "";

    [MaxLength(300)]
    public string? Notas { get; set; }

    public List<ExistingPowerWashFileViewModel> ArchivosExistentes { get; set; } = new();
}

public class PowerWashConfirmedViewModel
{
    public int SolicitudId { get; set; }
    public int HomeCarePriorityId { get; set; }
    public int? PropiedadId { get; set; }
    public string NombreServicio { get; set; } = "Power Wash Exterior";
    public string AreaLabel { get; set; } = string.Empty;
    public string MaterialLabel { get; set; } = string.Empty;
    public string StoriesLabel { get; set; } = string.Empty;
    public string ConditionLabel { get; set; } = string.Empty;
    public string WaterAccessLabel { get; set; } = string.Empty;
    public string PreferredTimeLabel { get; set; } = string.Empty;
    public string? TipConfirmacionTexto { get; set; }
}
