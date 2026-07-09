using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Localization;
using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [PersonName]
    [Display(Name = "Full name")]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [ValidEmail]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone")]
    [UsPhoneOptional]
    [StringLength(10, ErrorMessage = "Phone must be 10 digits.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least {2} characters.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    public string UiCulture { get; set; } = Localization.UiCulture.English;
}
