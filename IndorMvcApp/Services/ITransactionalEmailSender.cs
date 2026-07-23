namespace IndorMvcApp.Services;

/// <summary>
/// Generic branded transactional email. Body must already be a full HTML document.
/// Use <see cref="IndorEmailTemplates"/> to build consistent INDOR-branded HTML.
/// </summary>
public sealed record TransactionalEmailModel(
    string ToEmail,
    string? ToName,
    string Subject,
    string HtmlBody,
    string? ReplyToEmail = null);

public interface ITransactionalEmailSender
{
    /// <summary>Returns true when the email was accepted by the SMTP server.</summary>
    Task<bool> SendAsync(TransactionalEmailModel model, CancellationToken cancellationToken = default);
}
