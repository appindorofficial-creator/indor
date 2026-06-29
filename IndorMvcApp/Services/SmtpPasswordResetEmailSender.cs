using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using IndorMvcApp.Models;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class SmtpPasswordResetEmailSender(
    IOptions<SmtpSettings> options,
    ILogger<SmtpPasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendPasswordResetEmailAsync(PasswordResetEmailModel model, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsConfigured)
        {
            logger.LogWarning(
                "SMTP is not configured. Password reset email for {Email} was not sent. Code: {Code} | Link: {ResetUrl}",
                model.ToEmail, model.Code, model.ResetUrl);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = "Reset your INDOR password",
                Body = BuildHtmlBody(model),
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(model.ToEmail,
                string.IsNullOrWhiteSpace(model.Name) ? model.ToEmail : model.Name));

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
            logger.LogInformation("Password reset email sent to {Email}.", model.ToEmail);
        }
        catch (Exception ex)
        {
            // Never break the reset flow because of email issues.
            logger.LogError(ex, "Failed to send password reset email to {Email}. Code: {Code} | Link: {ResetUrl}",
                model.ToEmail, model.Code, model.ResetUrl);
        }
    }

    private static string BuildHtmlBody(PasswordResetEmailModel model)
    {
        var name = string.IsNullOrWhiteSpace(model.Name)
            ? "there"
            : HtmlEncoder.Default.Encode(model.Name);
        var code = HtmlEncoder.Default.Encode(model.Code);
        var resetUrl = HtmlEncoder.Default.Encode(model.ResetUrl);
        var hours = model.ValidHours;

        var codeBoxes = string.Concat(model.Code.Select(c =>
            $@"<td style=""padding:0 5px;""><div style=""width:42px;height:52px;line-height:52px;text-align:center;background:#fff;border:1px solid #DBE7F5;border-radius:12px;font-size:24px;font-weight:800;color:#0A2540;font-family:Consolas,Menlo,monospace;"">{HtmlEncoder.Default.Encode(c.ToString())}</div></td>"));

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /><meta name=""viewport"" content=""width=device-width,initial-scale=1"" /></head>
<body style=""margin:0;padding:0;background:#F8FAFC;font-family:Segoe UI,Arial,sans-serif;color:#0A2540;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F8FAFC;""><tr><td align=""center"" style=""padding:24px 12px;"">
    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;"">

      <tr><td style=""padding:0 8px 16px;font-weight:800;color:#0066CC;font-size:20px;"">&#127968; INDOR</td></tr>

      <tr><td style=""padding:0 8px;"">
        <h1 style=""font-size:24px;line-height:1.3;margin:0 0 12px;color:#0A2540;"">Reset your password</h1>
        <p style=""margin:0 0 18px;color:#475569;font-size:15px;line-height:1.55;"">
          Hi {name}, we received a request to reset the password for your INDOR account.
          Use the verification code below or tap the button to continue.
        </p>
      </td></tr>

      <tr><td style=""padding:8px 8px 0;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
          <td style=""background:#EEF4FF;border-radius:16px;padding:22px;text-align:center;"">
            <div style=""color:#64748B;font-size:13px;font-weight:600;text-transform:uppercase;letter-spacing:.06em;margin-bottom:14px;"">Your verification code</div>
            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" align=""center""><tr>{codeBoxes}</tr></table>
            <div style=""color:#94A3B8;font-size:12.5px;margin-top:14px;"">This code expires in {hours} hours.</div>
          </td>
        </tr></table>
      </td></tr>

      <tr><td style=""padding:26px 8px 0;"">
        <a href=""{resetUrl}"" style=""display:block;text-align:center;background:#0066CC;color:#fff;text-decoration:none;font-weight:700;font-size:17px;padding:16px 20px;border-radius:12px;"">Reset Password &#8594;</a>
      </td></tr>

      <tr><td style=""padding:14px 8px 0;"">
        <p style=""margin:0;color:#94A3B8;font-size:12px;line-height:1.5;text-align:center;"">
          If the button doesn't work, copy and paste this link into your browser:<br />
          <a href=""{resetUrl}"" style=""color:#0066CC;word-break:break-all;"">{resetUrl}</a>
        </p>
      </td></tr>

      <tr><td style=""padding:24px 8px 0;text-align:center;color:#94A3B8;font-size:12px;line-height:1.5;"">
        If you didn't request a password reset, you can safely ignore this email — your password won't change.
      </td></tr>

    </table>
  </td></tr></table>
</body>
</html>";
    }
}
