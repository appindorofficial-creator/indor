using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.Services;

public interface IPropertyAdministratorRegistrationService
{
    Task<PropertyAdministratorRegistrationState> GetAsync(CancellationToken cancellationToken = default);
    Task LinkCurrentUserAsync(CancellationToken cancellationToken = default);
    Task SaveProfileAsync(PropertyAdministratorProfileInput input, CancellationToken cancellationToken = default);
    Task SavePortfolioAsync(PropertyAdministratorPortfolioInput input, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PropertyAdministratorPropertyItemViewModel>> GetPortfolioPropertiesAsync(CancellationToken cancellationToken = default);
    Task AddPortfolioPropertyAsync(PropertyAdministratorPropertyInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorPortfolioImportResult> ImportPortfolioFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task UploadPortfolioDocumentAsync(int portfolioPropertyId, IFormFile file, string? title, CancellationToken cancellationToken = default);
    Task RemovePortfolioPropertyAsync(int propertyId, CancellationToken cancellationToken = default);
    Task AdvanceFromPropertiesAsync(CancellationToken cancellationToken = default);
    Task SaveToolsAsync(PropertyAdministratorToolsInput input, CancellationToken cancellationToken = default);
    Task<PropertyAdministratorReviewViewModel> GetReviewViewModelAsync(CancellationToken cancellationToken = default);
    Task CompleteRegistrationAsync(bool platformTermsAccepted, CancellationToken cancellationToken = default);
    Task<IndorPropertyAdministrator?> GetAdministratorForCurrentUserAsync(CancellationToken cancellationToken = default);
    string ResolveWizardResumeAction(int currentStep);
    bool IsRegistrationComplete(IndorPropertyAdministrator administrator);
}
