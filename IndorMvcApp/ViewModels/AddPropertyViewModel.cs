using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.ViewModels;

public class AddPropertyViewModel
{
    [Required(ErrorMessage = "Address is required")]
    [Display(Name = "Property address")]
    public string Address { get; set; } = string.Empty;

    [Display(Name = "Apt, Suite, Unit")]
    [StringLength(50)]
    public string? Unit { get; set; }
}
