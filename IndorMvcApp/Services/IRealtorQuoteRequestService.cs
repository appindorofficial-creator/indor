using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public interface IRealtorQuoteRequestService
{
    Task<IndorRealtorQuoteRequestDraft> EnsureDraftAsync(CancellationToken cancellationToken = default);
    Task<IndorRealtorQuoteRequestDraft?> GetDraftAsync(CancellationToken cancellationToken = default);
    Task CancelDraftAsync(CancellationToken cancellationToken = default);
    string ResolveResumeAction(int currentStep);
    Task PrepareBackToPropertyAsync(CancellationToken cancellationToken = default);
    Task PrepareBackToRequestDetailsAsync(CancellationToken cancellationToken = default);
    Task PrepareBackToProvidersAsync(CancellationToken cancellationToken = default);

    Task<RealtorQuoteRequestPropertyViewModel> BuildPropertyAsync(string? search, CancellationToken cancellationToken = default);
    Task SavePropertyAsync(int propertyFileId, CancellationToken cancellationToken = default);

    Task<RealtorQuoteRequestDetailsViewModel> BuildRequestDetailsAsync(CancellationToken cancellationToken = default);
    Task SaveRequestDetailsAsync(
        string requestType,
        bool sharePhotosVideos,
        bool shareInspectionReport,
        bool shareRepairItems,
        bool shareNotes,
        int responseDeadlineHours,
        CancellationToken cancellationToken = default);

    Task<RealtorQuoteRequestProvidersViewModel> BuildProvidersAsync(string? search, string? filter, CancellationToken cancellationToken = default);
    Task SaveProvidersAsync(
        string providerSelectionMode,
        int[]? providerIds,
        string serviceType,
        int providerCountTarget,
        bool verifiedOnly,
        string priority,
        int coverageMiles,
        CancellationToken cancellationToken = default);

    Task<RealtorQuoteRequestReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default);
    Task<int> SendAsync(
        bool sendNow,
        DateTime? scheduledSendUtc,
        int responseDeadlineHours,
        bool allowProviderQuestions,
        bool allowFullProjectQuote,
        bool allowItemizedQuote,
        string? optionalMessage,
        CancellationToken cancellationToken = default);

    Task<RealtorQuoteRequestSuccessViewModel> BuildSuccessAsync(int quoteId, CancellationToken cancellationToken = default);
}
