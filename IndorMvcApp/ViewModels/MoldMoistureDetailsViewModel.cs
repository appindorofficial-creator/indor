namespace IndorMvcApp.ViewModels;

public class MoldMoistureDetailsViewModel
{
    public int InspeccionId { get; set; }
    public int? SolicitudId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TiposProblema { get; set; } = "VisibleMold";
    public string TipoProblema { get; set; } = "VisibleMold";
    public string UbicacionProblema { get; set; } = "Bathroom";
    public string Urgencia { get; set; } = "Normal";
    public string HumedadActiva { get; set; } = "Yes";
}
