using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IRealtorRegistrationService
{
    Task<RealtorRegistrationState> GetAsync(CancellationToken cancellationToken = default);
    Task SaveProfileAsync(RealtorRegistrationState state, CancellationToken cancellationToken = default);
    Task LinkCurrentUserAsync(CancellationToken cancellationToken = default);
    Task EnsureDocumentSlotsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RealtorDocumentSlotViewModel>> GetDocumentSlotsAsync(CancellationToken cancellationToken = default);
    Task RegisterDocumentUploadAsync(string documentType, string relativeUrl, CancellationToken cancellationToken = default);
    Task<string?> ClearDocumentAsync(string documentType, CancellationToken cancellationToken = default);
    Task CompleteVerificationAsync(bool skipped, CancellationToken cancellationToken = default);
    Task<RealtorReadyViewModel> GetReadyViewModelAsync(CancellationToken cancellationToken = default);
    Task<IndorRealtor?> GetRealtorForCurrentUserAsync(CancellationToken cancellationToken = default);
    string ResolveWizardResumeAction(int currentStep);
    bool IsRegistrationComplete(IndorRealtor realtor);
    IReadOnlyList<string> GetLicenseStates();
    IReadOnlyList<string> GetSupportedLanguages();
}
