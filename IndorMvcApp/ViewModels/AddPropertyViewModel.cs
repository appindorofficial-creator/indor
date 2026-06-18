using System.ComponentModel.DataAnnotations;
using IndorMvcApp.Models;
using IndorMvcApp.Validation;

namespace IndorMvcApp.ViewModels;

public class AddPropertyViewModel
{
    [Required(ErrorMessage = "Street address is required")]
    [ValidStreetAddress]
    [Display(Name = "Street address")]
    public string StreetAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [Display(Name = "City")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Select a state")]
    [Display(Name = "State")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "ZIP code is required")]
    [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Enter a valid ZIP code.")]
    [Display(Name = "ZIP Code")]
    public string ZipCode { get; set; } = string.Empty;

    [Display(Name = "Unit / Apt")]
    [StringLength(50)]
    public string? Unit { get; set; }

    public string Address { get; set; } = string.Empty;

    public string BuildLookupAddress() =>
        PropertyAdministratorCatalog.FormatPropertyLocation(City, State, StreetAddress, ZipCode);
}
