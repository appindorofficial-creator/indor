using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.Services;

public interface IRealtorUrgentQuoteWizardService
{
    Task<IndorRealtorUrgentQuoteDraft> EnsureDraftAsync(CancellationToken cancellationToken = default);
    Task<IndorRealtorUrgentQuoteDraft?> GetDraftAsync(CancellationToken cancellationToken = default);
    Task CancelDraftAsync(CancellationToken cancellationToken = default);
    string ResolveResumeAction(int currentStep);

    Task<RealtorUrgentQuotePropertyViewModel> BuildPropertyAsync(string? search, CancellationToken cancellationToken = default);
    Task SavePropertyAsync(int propertyFileId, string requestCategory, string serviceType, string urgencyLevel, CancellationToken cancellationToken = default);

    Task<RealtorUrgentQuoteIssueViewModel> BuildIssueAsync(CancellationToken cancellationToken = default);
    Task SaveIssueAsync(string serviceType, string urgencyLevel, string quickDescription, string requestTypeTag, CancellationToken cancellationToken = default);

    Task<RealtorUrgentQuotePhotosViewModel> BuildPhotosAsync(CancellationToken cancellationToken = default);
    Task SavePhotosAsync(string? optionalNote, bool skipPhotos, IEnumerable<IFormFile>? photos, CancellationToken cancellationToken = default);
    Task RemovePhotoAsync(int photoId, CancellationToken cancellationToken = default);

    Task<RealtorUrgentQuoteSendViewModel> BuildSendAsync(CancellationToken cancellationToken = default);
    Task<RealtorUrgentQuoteSuccessViewModel> SendAsync(string providerSelectionMode, string sendPayload, bool notifyClient, CancellationToken cancellationToken = default);
}
