using System.Net;
using System.Net.Mail;
using IndorMvcApp.Models;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class SmtpTransactionalEmailSender(
    IOptions<SmtpSettings> options,
    ILogger<SmtpTransactionalEmailSender> logger) : ITransactionalEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task<bool> SendAsync(TransactionalEmailModel model, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.ToEmail))
        {
            logger.LogWarning("Transactional email skipped: empty recipient. Subject: {Subject}", model.Subject);
            return false;
        }

        if (!_settings.IsConfigured)
        {
            logger.LogWarning(
                "SMTP is not configured. Transactional email to {Email} was not sent. Subject: {Subject}",
                model.ToEmail, model.Subject);
            return false;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = model.Subject,
                Body = model.HtmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(model.ToEmail,
                string.IsNullOrWhiteSpace(model.ToName) ? model.ToEmail : model.ToName));

            if (!string.IsNullOrWhiteSpace(model.ReplyToEmail))
            {
                message.ReplyToList.Add(new MailAddress(model.ReplyToEmail));
            }

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            }

            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Transactional email sent to {Email}. Subject: {Subject}", model.ToEmail, model.Subject);
            return true;
        }
        catch (Exception ex)
        {
            // Never break the caller flow because of email issues.
            logger.LogError(ex, "Failed to send transactional email to {Email}. Subject: {Subject}",
                model.ToEmail, model.Subject);
            return false;
        }
    }
}
