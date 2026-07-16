namespace IndorMvcApp.Models;

public class StripeSettings
{
    public const string SectionName = "Stripe";

    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string? WebhookSecret { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SecretKey)
        && !string.IsNullOrWhiteSpace(PublishableKey);
}
