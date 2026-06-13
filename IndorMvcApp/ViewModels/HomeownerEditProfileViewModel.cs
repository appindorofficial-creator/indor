namespace IndorMvcApp.ViewModels;

public class HomeownerEditProfileViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string DisplayInitial { get; set; } = "?";
}
