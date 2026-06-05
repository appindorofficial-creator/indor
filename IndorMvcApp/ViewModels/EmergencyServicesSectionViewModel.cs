namespace IndorMvcApp.ViewModels;

public class EmergencyServicesSectionViewModel
{
    public List<EmergencyServiceCardViewModel> Items { get; set; } = new();
    public int SelectedId { get; set; }
    public string? ViewAllUrl { get; set; }
}

public class EmergencyServiceCardViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TituloEmergencia { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int TiempoLlegadaMinutos { get; set; }
    public string IconoClase { get; set; } = "fa-droplet";
    public string? ImagenUrl { get; set; }
    public string? BadgeTexto { get; set; }
    public string CtaTexto { get; set; } = "Request help";
    public List<EmergencyServiceFeatureViewModel> Caracteristicas { get; set; } = new();
}

public class EmergencyServiceFeatureViewModel
{
    public string Icon { get; set; } = "fa-clock";
    public string Text { get; set; } = string.Empty;
}
