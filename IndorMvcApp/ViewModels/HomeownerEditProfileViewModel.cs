namespace IndorMvcApp.ViewModels;

public class HomeownerEditProfileViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string DisplayInitial { get; set; } = "?";

    public bool HasHome { get; set; }
    public int? PropiedadId { get; set; }
    public string? HomeAddress { get; set; }
    public bool HasEnrichedData { get; set; }
    public string? DataSource { get; set; }
    public int? YearBuilt { get; set; }
    public int? LivingArea { get; set; }
    public int? Bedrooms { get; set; }
    public decimal? Bathrooms { get; set; }
    public int HouseFactFieldCount { get; set; }

    public AddPropertyViewModel AddressForm { get; set; } = new();
    public HouseFactProfileViewModel? HouseFactPreview { get; set; }
}
