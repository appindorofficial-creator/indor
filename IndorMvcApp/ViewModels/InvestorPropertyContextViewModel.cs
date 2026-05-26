namespace IndorMvcApp.ViewModels;

public class InvestorPropertyContextViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string TipoPropiedad { get; set; } = "SingleFamily";
    public string Ocupacion { get; set; } = "TenantOccupied";
    public string NivelRehab { get; set; } = "Light";
    public string AreasRevision { get; set; } = "Roof";
    public string AccesoPreferido { get; set; } = "SomeoneHome";
}
