namespace IndorMvcApp.ViewModels;

public class MoldMoisturePropertyContextViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string MotivoRevision { get; set; } = "MustySmellConcern";
    public string TipoPropiedad { get; set; } = "SingleFamily";
    public string UbicacionPrincipal { get; set; } = "Bathroom";
    public string IntrusionAguaReciente { get; set; } = "Yes";
    public string AccesoPreferido { get; set; } = "SomeoneHome";
    public string AreasEnfoque { get; set; } = string.Empty;
}
