using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Http;

namespace IndorMvcApp.Services;

public interface IRealtorInspectionUploadWizardService
{
    Task<IndorRealtorInspectionUploadDraft> EnsureDraftAsync(CancellationToken cancellationToken = default);
    Task<IndorRealtorInspectionUploadDraft?> GetDraftAsync(CancellationToken cancellationToken = default);
    Task CancelDraftAsync(CancellationToken cancellationToken = default);
    Task ResetToUploadAsync(CancellationToken cancellationToken = default);
    string ResolveResumeAction(int currentStep);

    Task<RealtorInspectionUploadViewModel> BuildUploadAsync(string? search, CancellationToken cancellationToken = default);
    Task SaveUploadAsync(
        int propertyFileId,
        string uploadMethod,
        IFormFile? reportFile,
        string? newPropertyAddress = null,
        string? newPropertyClientName = null,
        string? newPropertyCityRegion = null,
        CancellationToken cancellationToken = default);

    Task<RealtorInspectionAnalyzeViewModel> BuildAnalyzeAsync(CancellationToken cancellationToken = default);
    Task RunAnalysisAsync(CancellationToken cancellationToken = default);
    Task AdvanceAnalysisAsync(CancellationToken cancellationToken = default);
    Task CompleteAnalysisAsync(CancellationToken cancellationToken = default);

    Task<RealtorInspectionPrioritiesViewModel> BuildPrioritiesAsync(string? filter, string? sort, CancellationToken cancellationToken = default);
    Task SavePrioritiesAsync(int[]? selectedFindingIds, CancellationToken cancellationToken = default);

    Task<RealtorInspectionProvidersViewModel> BuildProvidersAsync(string? tradeFilter, CancellationToken cancellationToken = default);
    Task SaveProvidersAsync(Dictionary<string, int[]>? providersByTrade, CancellationToken cancellationToken = default);

    Task<RealtorInspectionReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default);
    Task<RealtorInspectionSuccessViewModel> CreateQuoteRequestsAsync(CancellationToken cancellationToken = default);
}
