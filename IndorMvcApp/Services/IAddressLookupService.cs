using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IAddressLookupService
{
    Task<PropertyInfoViewModel?> GetPropertyInfoAsync(string address);
    Task<PropertyInfoViewModel?> GetGeocodedPropertyAsync(string address, CancellationToken cancellationToken = default);
    Task<string?> LookupPrimaryZipForCityAsync(string city, string state, CancellationToken cancellationToken = default);
    Task EnrichPropertyInfoAsync(PropertyInfoViewModel propertyInfo);
    Task<(decimal Latitude, decimal Longitude)?> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default);
}
