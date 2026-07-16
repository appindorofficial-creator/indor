using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public sealed record InsuranceCheckoutSessionResult(string SessionId, string Url);

public sealed record InsuranceCheckoutCompletion(
    int QuoteId,
    string Plan,
    decimal PaidToday,
    decimal Monthly,
    string ReceiptNumber,
    string CarrierEmailStatus,
    bool AlreadyCompleted);

public interface IInsuranceStripeCheckoutService
{
    bool IsConfigured { get; }

    Task<InsuranceCheckoutSessionResult> CreateCheckoutSessionAsync(
        IndorProviderInsuranceQuote quote,
        string successUrl,
        string cancelUrl,
        string? preferredPaymentMethod,
        CancellationToken cancellationToken = default);

    Task<InsuranceCheckoutCompletion?> CompleteCheckoutSessionAsync(
        string checkoutSessionId,
        CancellationToken cancellationToken = default);

    Task<bool> TryHandleWebhookAsync(
        string json,
        string? stripeSignatureHeader,
        CancellationToken cancellationToken = default);
}
