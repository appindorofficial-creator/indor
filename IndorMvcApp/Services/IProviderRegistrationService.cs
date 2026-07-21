using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public interface IProviderRegistrationService
{
    Task<ProviderRegistrationState> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ProviderRegistrationState state, int currentStep, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OnboardingOption>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OnboardingOption>> GetServiceOfferingsAsync(CancellationToken cancellationToken = default);
    Task<int> GetExamPassingPercentAsync(CancellationToken cancellationToken = default);
    Task<int> GetExamQuestionCountAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<int> GetExamTotalPagesAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamQuestion>> GetExamPageQuestionsAsync(int page, string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task SaveExamPageAnswersAsync(int page, IReadOnlyDictionary<int, string> pageAnswers, string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<(bool Passed, int ScorePercent)> FinalizeExamAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<ViewModels.ProviderExamResultViewModel> GetExamResultAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetScopeAllowedAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetScopeDisallowedAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task CompleteRegistrationAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default);
    Task LinkCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<bool> RequiresTradeExamAsync(ProviderRegistrationState? state = null, CancellationToken cancellationToken = default);
    Task<string> ResolveTradeCodeAsync(ProviderRegistrationState? state = null, CancellationToken cancellationToken = default);
    Task<string?> GetPrimaryTradeLabelAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OnboardingOption>> GetServiceOfferingsForTradeAsync(CancellationToken cancellationToken = default);
    Task SubmitApplicationAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default);
    Task EnsureDocumentSlotsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProviderDocumentSlot>> GetDocumentSlotsAsync(CancellationToken cancellationToken = default);
    Task RegisterDocumentUploadAsync(
        string documentType,
        string relativeUrl,
        int? proveedorId = null,
        CancellationToken cancellationToken = default);
    Task<bool> HasRequiredDocumentsAsync(CancellationToken cancellationToken = default);
    Task<IndorProveedor?> GetProveedorForCurrentUserAsync(CancellationToken cancellationToken = default);
    Task ActivateIndorProAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default);
    string ResolveWizardResumeAction(int currentStep);
    string ResolveWizardResumeAction(IndorProveedor proveedor);
}
