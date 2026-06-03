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
    Task<IReadOnlyList<string>> GetScopeAllowedAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetScopeDisallowedAsync(string tradeCode = "electrical", CancellationToken cancellationToken = default);
    Task CompleteRegistrationAsync(ProviderRegistrationState state, CancellationToken cancellationToken = default);
    Task LinkCurrentUserAsync(CancellationToken cancellationToken = default);
}
