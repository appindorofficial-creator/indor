using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IHomeownerPropertyService
{
    Task<Propiedad?> GetPrimaryPropertyAsync(string userId, CancellationToken cancellationToken = default);

    Task<PropertyInfoViewModel?> EnrichAddressAsync(
        AddPropertyViewModel model,
        bool requestFullHouseFactResearch = false,
        CancellationToken cancellationToken = default);

    Task<int> SaveOrUpdatePropertyAsync(
        PropertyInfoViewModel propertyInfo,
        string userId,
        int? existingPropertyId = null,
        CancellationToken cancellationToken = default);

    Task<PropertyMaintenancePlanViewModel> TryGenerateMaintenanceAsync(
        PropertyInfoViewModel propertyInfo,
        CancellationToken cancellationToken = default);

    void QueuePropertyEnrichment(int propiedadId, bool fullResearch = false);

    void ApplyAddressFields(PropertyInfoViewModel propertyInfo, AddPropertyViewModel model);
}
