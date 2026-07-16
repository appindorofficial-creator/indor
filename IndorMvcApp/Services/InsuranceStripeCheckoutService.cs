using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace IndorMvcApp.Services;

public class InsuranceStripeCheckoutService(
    AppDbContext db,
    IProviderProDataService proData,
    IOptions<StripeSettings> stripeOptions,
    IOptions<InsuranceSettings> insuranceOptions,
    IInsuranceCarrierEmailSender insuranceCarrierEmail,
    ILogger<InsuranceStripeCheckoutService> logger) : IInsuranceStripeCheckoutService
{
    public const string MetadataFlowKey = "indor_flow";
    public const string MetadataFlowValue = "provider_insurance";
    public const string MetadataQuoteIdKey = "quote_id";
    public const string MetadataProveedorIdKey = "proveedor_id";

    private readonly StripeSettings _stripe = stripeOptions.Value;

    public bool IsConfigured => _stripe.IsConfigured;

    public async Task<InsuranceCheckoutSessionResult> CreateCheckoutSessionAsync(
        IndorProviderInsuranceQuote quote,
        string successUrl,
        string cancelUrl,
        string? preferredPaymentMethod,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var amount = quote.PayTodayAmount ?? 0m;
        if (amount <= 0m)
        {
            throw new InvalidOperationException("Insurance pay-today amount must be greater than zero.");
        }

        var amountCents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        var plan = quote.Plan ?? "Basic";
        var productName = $"INDOR Insurance — {plan} Plan (down payment)";

        StripeConfiguration.ApiKey = _stripe.SecretKey;

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            CustomerEmail = string.IsNullOrWhiteSpace(quote.OwnerEmail) ? null : quote.OwnerEmail,
            ClientReferenceId = quote.Id.ToString(),
            PaymentMethodTypes = ResolvePaymentMethodTypes(preferredPaymentMethod),
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = amountCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = productName,
                            Description = "General Liability insurance application down payment"
                        }
                    }
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                [MetadataFlowKey] = MetadataFlowValue,
                [MetadataQuoteIdKey] = quote.Id.ToString(),
                [MetadataProveedorIdKey] = quote.ProveedorId.ToString(),
                ["plan"] = plan,
                ["receipt"] = quote.ReceiptNumber ?? string.Empty
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [MetadataFlowKey] = MetadataFlowValue,
                    [MetadataQuoteIdKey] = quote.Id.ToString(),
                    [MetadataProveedorIdKey] = quote.ProveedorId.ToString()
                }
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(session.Url))
        {
            throw new InvalidOperationException("Stripe Checkout did not return a session URL.");
        }

        var tracked = await db.IndorProviderInsuranceQuotes
            .FirstOrDefaultAsync(q => q.Id == quote.Id, cancellationToken);
        if (tracked is not null)
        {
            tracked.StripeCheckoutSessionId = session.Id;
            await db.SaveChangesAsync(cancellationToken);
        }

        return new InsuranceCheckoutSessionResult(session.Id, session.Url);
    }

    public async Task<InsuranceCheckoutCompletion?> CompleteCheckoutSessionAsync(
        string checkoutSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(checkoutSessionId) || !IsConfigured)
        {
            return null;
        }

        StripeConfiguration.ApiKey = _stripe.SecretKey;
        var sessionService = new SessionService();
        var session = await sessionService.GetAsync(checkoutSessionId, cancellationToken: cancellationToken);

        if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "Stripe insurance checkout session {SessionId} is not paid (status={Status}, payment={Payment}).",
                checkoutSessionId,
                session.Status,
                session.PaymentStatus);
            return null;
        }

        return await FinalizePaidSessionAsync(session, cancellationToken);
    }

    public async Task<bool> TryHandleWebhookAsync(
        string json,
        string? stripeSignatureHeader,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(_stripe.WebhookSecret))
        {
            logger.LogWarning("Stripe webhook received but webhook secret is not configured.");
            return false;
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignatureHeader,
                _stripe.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return false;
        }

        if (stripeEvent.Type is not "checkout.session.completed"
            and not "checkout.session.async_payment_succeeded")
        {
            return true;
        }

        if (stripeEvent.Data.Object is not Session session)
        {
            return true;
        }

        if (!IsInsuranceSession(session))
        {
            return true;
        }

        await FinalizePaidSessionAsync(session, cancellationToken);
        return true;
    }

    private async Task<InsuranceCheckoutCompletion?> FinalizePaidSessionAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        var quote = await ResolveQuoteAsync(session, cancellationToken);
        if (quote is null)
        {
            logger.LogWarning(
                "No insurance quote found for Stripe session {SessionId}.",
                session.Id);
            return null;
        }

        var alreadyPaid = string.Equals(quote.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
            && quote.PaymentAuthorized;

        string carrierEmailStatus;
        if (alreadyPaid)
        {
            carrierEmailStatus = "AlreadyCompleted";
        }
        else
        {
            var now = DateTime.UtcNow;
            quote.PaymentStatus = "Paid";
            quote.PaymentAuthorized = true;
            quote.PaidUtc = now;
            quote.Status = "Pending Carrier Approval";
            quote.SubmittedUtc ??= now;
            quote.StripeCheckoutSessionId = session.Id;
            quote.StripePaymentIntentId = session.PaymentIntentId
                ?? (session.PaymentIntent as PaymentIntent)?.Id
                ?? quote.StripePaymentIntentId;
            quote.PaymentMethod = MapStripePaymentMethod(session.PaymentMethodTypes);

            if (string.IsNullOrWhiteSpace(quote.ReceiptNumber))
            {
                quote.ReceiptNumber = $"IND-GL-{now:yyyy}-{quote.Id:00000}";
            }

            await db.SaveChangesAsync(cancellationToken);
            carrierEmailStatus = await SendCarrierIssuanceAsync(quote, cancellationToken);
        }

        var (payToday, monthly) = InsuranceCatalog.Pricing(quote.Plan);
        return new InsuranceCheckoutCompletion(
            QuoteId: quote.Id,
            Plan: quote.Plan ?? "Basic",
            PaidToday: quote.PayTodayAmount ?? payToday,
            Monthly: quote.MonthlyAmount ?? monthly,
            ReceiptNumber: quote.ReceiptNumber ?? string.Empty,
            CarrierEmailStatus: carrierEmailStatus,
            AlreadyCompleted: alreadyPaid);
    }

    private async Task<string> SendCarrierIssuanceAsync(
        IndorProviderInsuranceQuote quote,
        CancellationToken cancellationToken)
    {
        var draft = ToDraft(quote);
        var receiptNumber = quote.ReceiptNumber
            ?? $"IND-GL-{DateTime.UtcNow:yyyy}-{quote.Id:00000}";

        var issuanceId = await proData.SaveInsuranceIssuanceRequestAsync(
            quote.ProveedorId,
            draft,
            receiptNumber,
            insuranceOptions.Value.CarrierEmail,
            cancellationToken);

        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var emailModel = new InsuranceIssuanceEmailModel(
            RequestCode: receiptNumber,
            Plan: draft.Plan,
            BusinessName: draft.BusinessName,
            BusinessAddress: draft.FullAddress,
            WorkersComp: draft.Coverages.Any(c => c.Contains("Workers", StringComparison.OrdinalIgnoreCase)),
            GeneralLiability: draft.Coverages.Any(c => c.Contains("General Liability", StringComparison.OrdinalIgnoreCase)),
            OwnerName: draft.OwnerName,
            OwnerDateOfBirth: draft.OwnerDateOfBirth,
            OwnerPhone: draft.OwnerPhone,
            OwnerEmail: draft.OwnerEmail,
            TypeOfBusiness: draft.Trade,
            NumberOfEmployees: draft.NumberOfEmployees,
            EmployeePayroll: draft.EmployeePayroll?.ToString("0.##", ci),
            CompanyGross: draft.CompanyGrossRevenue?.ToString("0.##", ci),
            Notes: null,
            ProviderContactEmail: draft.OwnerEmail);

        var emailResult = await insuranceCarrierEmail.SendIssuanceRequestAsync(emailModel, cancellationToken);
        await proData.MarkInsuranceIssuanceEmailAsync(
            issuanceId,
            emailResult.ToString(),
            emailResult == InsuranceEmailResult.Sent ? DateTime.UtcNow : null,
            cancellationToken);

        return emailResult.ToString();
    }

    private async Task<IndorProviderInsuranceQuote?> ResolveQuoteAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(session.Id))
        {
            var bySession = await db.IndorProviderInsuranceQuotes
                .FirstOrDefaultAsync(q => q.StripeCheckoutSessionId == session.Id, cancellationToken);
            if (bySession is not null)
            {
                return bySession;
            }
        }

        if (session.Metadata is not null
            && session.Metadata.TryGetValue(MetadataQuoteIdKey, out var quoteIdRaw)
            && int.TryParse(quoteIdRaw, out var quoteId))
        {
            return await db.IndorProviderInsuranceQuotes
                .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);
        }

        if (int.TryParse(session.ClientReferenceId, out var clientRefId))
        {
            return await db.IndorProviderInsuranceQuotes
                .FirstOrDefaultAsync(q => q.Id == clientRefId, cancellationToken);
        }

        return null;
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Stripe is not configured. Set Stripe:SecretKey and Stripe:PublishableKey.");
        }
    }

    private static bool IsInsuranceSession(Session session) =>
        session.Metadata is not null
        && session.Metadata.TryGetValue(MetadataFlowKey, out var flow)
        && string.Equals(flow, MetadataFlowValue, StringComparison.OrdinalIgnoreCase);

    private static List<string> ResolvePaymentMethodTypes(string? preferred) =>
        preferred?.Trim().ToLowerInvariant() switch
        {
            "bankaccount" => ["us_bank_account", "card"],
            _ => ["card"]
        };

    private static string MapStripePaymentMethod(List<string>? types)
    {
        if (types is null || types.Count == 0) return "Card";
        if (types.Contains("us_bank_account", StringComparer.OrdinalIgnoreCase)) return "BankAccount";
        return "Card";
    }

    private static ProviderProInsuranceQuoteDraft ToDraft(IndorProviderInsuranceQuote quote)
    {
        var coverages = string.IsNullOrWhiteSpace(quote.Coverages)
            ? new List<string> { "General Liability" }
            : quote.Coverages.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .ToList();

        return new ProviderProInsuranceQuoteDraft
        {
            Plan = quote.Plan ?? "Basic",
            Coverages = coverages,
            Trade = quote.Trade ?? string.Empty,
            BusinessName = quote.BusinessName ?? string.Empty,
            StreetAddress = quote.BusinessAddress ?? string.Empty,
            City = quote.City ?? string.Empty,
            State = quote.State ?? "NC",
            ZipCode = quote.ZipCode ?? string.Empty,
            OwnerName = quote.OwnerName ?? string.Empty,
            OwnerDateOfBirth = quote.OwnerDateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
            OwnerPhone = quote.OwnerPhone ?? string.Empty,
            OwnerEmail = quote.OwnerEmail ?? string.Empty,
            NumberOfEmployees = quote.NumberOfEmployees?.ToString() ?? string.Empty,
            EmployeePayroll = quote.EmployeePayroll,
            CompanyGrossRevenue = quote.CompanyGrossRevenue,
            YearsInBusiness = quote.YearsInBusiness ?? string.Empty,
            WorksAtCustomerHomes = quote.WorksAtCustomerHomes,
            UsesSubcontractors = quote.UsesSubcontractors,
            NeedsCOI = quote.NeedsCOI,
            AutoPayMonthly = quote.AutoPayMonthly ?? true,
            CardLast4 = quote.CardLast4 ?? string.Empty,
            PaymentMethod = quote.PaymentMethod ?? "Card"
        };
    }
}
