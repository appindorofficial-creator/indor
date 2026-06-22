using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using IndorMvcApp.Models;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class SmtpInvitationEmailSender(
    IOptions<SmtpSettings> options,
    ILogger<SmtpInvitationEmailSender> logger) : IInvitationEmailSender
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task SendInvitationEmailAsync(InvitationEmailModel model, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsConfigured)
        {
            logger.LogWarning(
                "SMTP is not configured. Invitation email for {Email} was not sent. Accept link: {AcceptUrl}",
                model.ToEmail, model.AcceptUrl);
            return;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = $"You've been invited to view your home in INDOR",
                Body = BuildHtmlBody(model),
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(model.ToEmail, model.ClientName));

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
            logger.LogInformation("Invitation email sent to {Email}.", model.ToEmail);
        }
        catch (Exception ex)
        {
            // Never break the invitation flow because of email issues.
            logger.LogError(ex, "Failed to send invitation email to {Email}. Accept link: {AcceptUrl}",
                model.ToEmail, model.AcceptUrl);
        }
    }

    private static string BuildHtmlBody(InvitationEmailModel model)
    {
        var realtor = HtmlEncoder.Default.Encode(model.RealtorName);
        var property = HtmlEncoder.Default.Encode(model.PropertyDisplay);
        var acceptUrl = HtmlEncoder.Default.Encode(model.AcceptUrl);
        var welcome = string.IsNullOrWhiteSpace(model.WelcomeMessage)
            ? string.Empty
            : $@"<tr><td style=""padding:6px 8px 0;""><p style=""margin:0;padding:14px 16px;background:#F1F5F9;border-radius:12px;color:#334155;font-size:14px;line-height:1.5;"">{HtmlEncoder.Default.Encode(model.WelcomeMessage)}</p></td></tr>";

        var propertyBlock = string.IsNullOrWhiteSpace(model.PropertyDisplay)
            ? string.Empty
            : $@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin:0 0 18px;""><tr>
                  <td style=""background:#fff;border:1px solid #E2E8F0;border-radius:14px;padding:16px;"">
                    <table role=""presentation"" cellpadding=""0"" cellspacing=""0""><tr>
                      <td width=""48"" valign=""middle"" style=""width:48px;""><div style=""width:48px;height:48px;border-radius:50%;background:#E8F1FF;text-align:center;line-height:48px;font-size:22px;"">&#127968;</div></td>
                      <td valign=""middle"" style=""padding-left:14px;font-size:16px;font-weight:700;color:#0A2540;"">{property}</td>
                    </tr></table>
                  </td></tr></table>";

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /><meta name=""viewport"" content=""width=device-width,initial-scale=1"" /></head>
<body style=""margin:0;padding:0;background:#F8FAFC;font-family:Segoe UI,Arial,sans-serif;color:#0A2540;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F8FAFC;""><tr><td align=""center"" style=""padding:24px 12px;"">
    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;"">

      <tr><td style=""padding:0 8px 16px;font-weight:800;color:#0066CC;font-size:20px;"">&#127968; INDOR</td></tr>

      <tr><td style=""padding:0 8px;"">
        <h1 style=""font-size:26px;line-height:1.25;margin:0 0 12px;color:#0A2540;"">You've been invited to view your home in Home INDOR</h1>
        <p style=""margin:0 0 18px;color:#475569;font-size:15px;line-height:1.55;"">
          <strong style=""color:#0066CC;"">{realtor}</strong> has invited you to access your property in Home INDOR — the home operating system that helps you stay organized, informed, and connected in one place.
        </p>
      </td></tr>

      <tr><td style=""padding:0 8px;"">{propertyBlock}</td></tr>

      {welcome}

      <tr><td style=""padding:6px 8px 0;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
          <td style=""background:#EEF4FF;border-radius:16px;padding:20px;"">
            <h2 style=""margin:0 0 8px;font-size:18px;color:#0A2540;"">What is Home INDOR?</h2>
            <p style=""margin:0;color:#475569;font-size:14px;line-height:1.55;"">Home INDOR is a digital home hub where you can view your home profile, inspection reports, quotes, maintenance reminders, and updates from your realtor or service providers.</p>
          </td>
        </tr></table>
      </td></tr>

      <tr><td style=""padding:26px 8px 14px;text-align:center;font-weight:700;font-size:17px;color:#0A2540;"">With Home INDOR, you can:</td></tr>
      <tr><td style=""padding:0 8px;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
          <tr>
            {FeatureCell("&#127968;", "View your home profile", "See key details and property information in one place.")}
            {FeatureCell("&#128196;", "See inspection reports", "Access reports and findings shared by your provider.")}
          </tr>
          <tr><td colspan=""2"" style=""height:12px;line-height:12px;"">&nbsp;</td></tr>
          <tr>
            {FeatureCell("&#128178;", "Track quotes and repairs", "Stay up to date on quotes, tasks, and repair progress.")}
            {FeatureCell("&#128197;", "Maintenance reminders", "Get reminders and keep your home running smoothly.")}
          </tr>
          <tr><td colspan=""2"" style=""height:12px;line-height:12px;"">&nbsp;</td></tr>
          <tr>
            {FeatureCell("&#128101;", "Collaborate with your realtor", "Communicate and share updates seamlessly.")}
            {FeatureCell("&#128193;", "Keep everything in one place", "All your important home info organized and secure.")}
          </tr>
        </table>
      </td></tr>

      <tr><td style=""padding:28px 8px 0;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
          <td style=""background:#EEF4FF;border-radius:16px;padding:20px;"">
            <div style=""text-align:center;font-weight:700;font-size:16px;margin-bottom:16px;color:#0A2540;"">How it works</div>
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
              {StepCell("1", "Accept invitation", "Open this email and accept your invitation to get started.")}
              {StepCell("2", "Download or open the app", "Download the Home INDOR app or open it if you already have it.")}
              {StepCell("3", "View your home", "Access your home information and stay in the loop.")}
            </tr></table>
          </td>
        </tr></table>
      </td></tr>

      <tr><td style=""padding:28px 8px 0;"">
        <a href=""{acceptUrl}"" style=""display:block;text-align:center;background:#0066CC;color:#fff;text-decoration:none;font-weight:700;font-size:17px;padding:16px 20px;border-radius:12px;"">Accept Invitation &#8594;</a>
      </td></tr>

      <tr><td style=""padding:14px 8px 0;"">
        <p style=""margin:0;color:#94A3B8;font-size:12px;line-height:1.5;text-align:center;"">
          If the button doesn't work, copy and paste this link into your browser:<br />
          <a href=""{acceptUrl}"" style=""color:#0066CC;word-break:break-all;"">{acceptUrl}</a>
        </p>
      </td></tr>

      <tr><td style=""padding:24px 8px 0;text-align:center;color:#94A3B8;font-size:12px;"">This invitation was sent by {realtor} through INDOR.</td></tr>

    </table>
  </td></tr></table>
</body>
</html>";
    }

    private static string FeatureCell(string icon, string title, string text) =>
        $@"<td width=""50%"" valign=""top"" style=""width:50%;padding:0 6px;"">
            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""height:100%;""><tr>
              <td style=""background:#fff;border:1px solid #E2E8F0;border-radius:14px;padding:16px;"">
                <div style=""font-size:20px;line-height:1;margin-bottom:8px;"">{icon}</div>
                <div style=""font-weight:700;font-size:14px;margin-bottom:5px;color:#0A2540;"">{title}</div>
                <div style=""color:#64748B;font-size:12.5px;line-height:1.45;"">{text}</div>
              </td>
            </tr></table>
          </td>";

    private static string StepCell(string number, string title, string text) =>
        $@"<td width=""33%"" valign=""top"" style=""width:33%;padding:0 6px;text-align:center;"">
            <div style=""width:28px;height:28px;border-radius:50%;background:#0066CC;color:#fff;font-weight:700;font-size:13px;line-height:28px;margin:0 auto 8px;"">{number}</div>
            <div style=""font-weight:700;font-size:13px;margin-bottom:4px;color:#0A2540;"">{title}</div>
            <div style=""color:#64748B;font-size:11.5px;line-height:1.4;"">{text}</div>
          </td>";
}
