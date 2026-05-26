using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class AddPropertyViewModel
{
    [Required(ErrorMessage = "Address is required")]
    [Display(Name = "Property address")]
    public string Address { get; set; } = string.Empty;
}
