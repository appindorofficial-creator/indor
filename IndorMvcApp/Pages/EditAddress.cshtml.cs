using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IndorMvcApp.ViewModels;
using System.Text.Json;

namespace IndorMvcApp.Pages;

public class EditAddressModel : PageModel
{
    [BindProperty]
    public PropertyInfoViewModel PropertyInfo { get; set; } = new();

    public IActionResult OnGet()
    {
        if (TempData["PropertyInfoJson"] is string json && !string.IsNullOrEmpty(json))
        {
            PropertyInfo = JsonSerializer.Deserialize<PropertyInfoViewModel>(json) ?? new PropertyInfoViewModel();
            // Keep TempData for POST
            TempData.Keep("PropertyInfoJson");
        }
        else
        {
            // If no data, redirect back to AddProperty
            return RedirectToAction("AddProperty", "Propietario");
        }
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        // Save or process the edited address here
        // Redirect to confirmation or next step
        return RedirectToPage("/Success");
    }
}
