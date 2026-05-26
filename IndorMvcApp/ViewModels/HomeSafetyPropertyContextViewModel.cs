namespace IndorMvcApp.ViewModels;

public class HomeSafetyPropertyContextViewModel
{
    public int SolicitudId { get; set; }
    public int InspeccionId { get; set; }
    public string NombreInspeccion { get; set; } = string.Empty;
    public string DireccionPropiedad { get; set; } = string.Empty;
    public string MotivosRevision { get; set; } = "AnnualReview";
    public string MotivoRevision { get; set; } = "AnnualReview";
    public string TipoPropiedad { get; set; } = "SingleFamily";
    public string NumeroPisos { get; set; } = "TwoStory";
    public string AreasEnfoque { get; set; } = "SmokeCoDetectors";
    public string AccesoPreferido { get; set; } = "SomeoneHome";
    public string OcupantesHogar { get; set; } = "Children";
}
