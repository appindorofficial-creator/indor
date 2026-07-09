using Microsoft.AspNetCore.Identity;

namespace IndorMvcApp.Models;

public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public string? RolUsuario { get; set; }
    public string? FotoUrl { get; set; }

    /// <summary>UI language preference: en-US or es-US.</summary>
    public string? PreferredUiCulture { get; set; }
}
