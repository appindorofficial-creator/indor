using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IOpenAiMaintenanceRecommendationService
{
    Task<PropertyMaintenancePlanViewModel> GenerateAsync(
        PropertyInfoViewModel propertyInfo,
        CancellationToken cancellationToken = default);
}
