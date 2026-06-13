namespace IndorMvcApp.ViewModels;

public class MovingSetupSectionViewModel
{
    public string Titulo { get; set; } = "Moving Setup";
    public string Subtitulo { get; set; } = string.Empty;
    public string IconoClase { get; set; } = "fa-box-open";
    public string ViewAllTexto { get; set; } = "View all";
    public string? ViewAllUrl { get; set; }
    public string FeaturedEtiqueta { get; set; } = "FEATURED";
    public string FeaturedTitulo { get; set; } = "Moving Assistant";
    public string FeaturedDescripcion { get; set; } = string.Empty;
    public string? FeaturedImagenUrl { get; set; }
    public string FeaturedCtaTexto { get; set; } = "Start moving setup";
    public string? FeaturedCtaUrl { get; set; }
    public List<MovingSetupFeatureViewModel> FeaturedCaracteristicas { get; set; } = new();
    public List<MovingSetupServiceItemViewModel> Servicios { get; set; } = new();
    public List<MovingSetupQuickLinkViewModel> EnlacesRapidos { get; set; } = new();
}

public class MovingSetupServiceItemViewModel
{
    public int Orden { get; set; }
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string IconoClase { get; set; } = "fa-house";
    public string? Url { get; set; }
}

public class MovingSetupQuickLinkViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string IconoClase { get; set; } = "fa-clipboard-list";
    public string? Url { get; set; }
}

public class MovingSetupFeatureViewModel
{
    public string Icon { get; set; } = "fa-bolt";
    public string Text { get; set; } = string.Empty;
}
