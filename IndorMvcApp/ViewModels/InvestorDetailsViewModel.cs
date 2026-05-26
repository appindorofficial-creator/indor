namespace IndorMvcApp.ViewModels;

public class InvestorDetailsViewModel
{
    public int InspeccionId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TipoInversion { get; set; } = "Flip";
    public string EnfoquesInversion { get; set; } = "RehabBudget";
    public string Urgencia { get; set; } = "Normal";
}
