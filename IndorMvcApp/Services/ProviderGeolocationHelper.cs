using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public static class ProviderGeolocationHelper
{
    public static async Task ApplyGeocodeAsync(
        IndorProveedor entity,
        IAddressLookupService addressLookup,
        CancellationToken cancellationToken = default)
    {
        var query = BuildGeocodeQuery(entity);
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        var coordinates = await addressLookup.GeocodeAddressAsync(query, cancellationToken);
        if (coordinates is not { } coords)
        {
            return;
        }

        entity.Latitude = coords.Latitude;
        entity.Longitude = coords.Longitude;
    }

    public static string? BuildGeocodeQuery(IndorProveedor entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.BusinessAddress))
        {
            return entity.BusinessAddress.Trim();
        }

        return string.IsNullOrWhiteSpace(entity.PrimaryCity) ? null : entity.PrimaryCity.Trim();
    }
}
