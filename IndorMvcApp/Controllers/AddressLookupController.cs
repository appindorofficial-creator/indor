using IndorMvcApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Controllers;

[Authorize]
[Route("[controller]/[action]")]
public class AddressLookupController(IAddressLookupService addressLookup) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Zip(string city, string state, string? street, CancellationToken cancellationToken)
    {
        city = city?.Trim() ?? string.Empty;
        state = state?.Trim() ?? string.Empty;
        street = street?.Trim();

        if (string.IsNullOrWhiteSpace(city) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest(new { message = "City and state are required." });
        }

        string? zip = null;

        if (!string.IsNullOrWhiteSpace(street))
        {
            var fullAddress = $"{street}, {city}, {state}, USA";
            var geocoded = await addressLookup.GetGeocodedPropertyAsync(fullAddress, cancellationToken);
            zip = geocoded?.PostalCode?.Trim();
        }

        if (string.IsNullOrWhiteSpace(zip))
        {
            zip = await addressLookup.LookupPrimaryZipForCityAsync(city, state, cancellationToken);
        }

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

    [HttpGet]
    public async Task<IActionResult> Resolve(string address, CancellationToken cancellationToken)
    {
        address = address?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest(new { message = "Address is required." });
        }

        var lookupAddress = address.Contains("USA", StringComparison.OrdinalIgnoreCase)
            ? address
            : $"{address}, USA";

        var resolved = await addressLookup.GetGeocodedPropertyAsync(lookupAddress, cancellationToken);
        if (resolved == null)
        {
            return NotFound(new { message = "Address could not be resolved." });
        }

        return Json(new
        {
            street = resolved.Street,
            city = resolved.City,
            state = resolved.State,
            zip = resolved.PostalCode
        });
    }
}
