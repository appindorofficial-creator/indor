namespace IndorMvcApp.ViewModels;

public class RoofPropertyContextViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string MotivoRevision { get; set; } = "LeakConcern";
    public string TipoPropiedad { get; set; } = "SingleFamily";
    public string NumeroPisos { get; set; } = "Two";
    public string MaterialTecho { get; set; } = "AsphaltShingles";
    public string AccesoPreferido { get; set; } = "SomeoneHome";
    public string AreasEnfoque { get; set; } = string.Empty;
}
