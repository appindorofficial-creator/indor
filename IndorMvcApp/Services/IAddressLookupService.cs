using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IAddressLookupService
{
    Task<PropertyInfoViewModel?> GetPropertyInfoAsync(string address);
}
