using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IRealtorPropertyFileWizardService
{
    Task<IndorRealtorPropertyFileDraft> EnsureDraftAsync(CancellationToken cancellationToken = default);
    Task<IndorRealtorPropertyFileDraft?> GetDraftAsync(CancellationToken cancellationToken = default);
    Task CancelDraftAsync(CancellationToken cancellationToken = default);
    string ResolveResumeAction(int currentStep);

    Task<RealtorPropertyFileDetailsViewModel> BuildDetailsAsync(string? search, CancellationToken cancellationToken = default);
    Task SaveDetailsAsync(int sourcePropertyId, string filePhase, CancellationToken cancellationToken = default);

    Task<RealtorPropertyFileAddItemsViewModel> BuildAddItemsAsync(CancellationToken cancellationToken = default);
    Task SaveAddItemsAsync(IEnumerable<string> categoryTypes, CancellationToken cancellationToken = default);

    Task<RealtorPropertyFileContentViewModel> BuildAddContentAsync(CancellationToken cancellationToken = default);
    Task SaveDraftItemAsync(string categoryType, string itemLabel, string? fileUrl, long? fileSizeBytes, string? noteText, DateTime? expirationUtc, CancellationToken cancellationToken = default);
    Task CompleteAddContentAsync(string? noteText, CancellationToken cancellationToken = default);

    Task<RealtorPropertyFileReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default);
    Task<int> CreateFileAsync(bool createAndContinueLater, CancellationToken cancellationToken = default);
    Task<RealtorPropertyFileSuccessViewModel> BuildSuccessAsync(int propertyFileId, CancellationToken cancellationToken = default);
    Task<RealtorPropertyFileViewViewModel> BuildViewAsync(int propertyFileId, CancellationToken cancellationToken = default);
}
