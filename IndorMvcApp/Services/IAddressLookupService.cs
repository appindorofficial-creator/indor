using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IAddressLookupService
{
    Task<PropertyInfoViewModel?> GetPropertyInfoAsync(string address);
    Task<(decimal Latitude, decimal Longitude)?> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default);
}
