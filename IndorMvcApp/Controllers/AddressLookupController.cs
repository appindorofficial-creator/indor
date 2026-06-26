using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class AddressLookupController(IAddressLookupService addressLookup) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Zip(string city, string state, CancellationToken cancellationToken)
    {
        city = city?.Trim() ?? string.Empty;
        state = state?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest(new { message = "City and state are required." });
        }

        var zip = await addressLookup.LookupPrimaryZipForCityAsync(city, state, cancellationToken);
        if (string.IsNullOrWhiteSpace(zip))
        {
            var propertyInfo = await addressLookup.GetGeocodedPropertyAsync($"{city}, {state}, USA", cancellationToken);
            zip = propertyInfo?.PostalCode?.Trim();
        }

        if (string.IsNullOrWhiteSpace(zip))
        {
            return NotFound(new { message = "ZIP code not found for that city and state." });
        }

        return Json(new { zip });
    }
}
