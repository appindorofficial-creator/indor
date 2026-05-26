using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class SelectRoleViewModel
{
    [Required(ErrorMessage = "You must select a role")]
    public string SelectedRole { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;
}
