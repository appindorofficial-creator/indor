using System.Net;
using System.Net.Mail;
using System.Text.Encodings.Web;
using IndorMvcApp.Models;
using Microsoft.Extensions.Options;

namespace IndorMvcApp.Services;

public class SmtpInsuranceCarrierEmailSender(
    IOptions<SmtpSettings> smtpOptions,
    IOptions<InsuranceSettings> insuranceOptions,
    ILogger<SmtpInsuranceCarrierEmailSender> logger) : IInsuranceCarrierEmailSender
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;
    private readonly InsuranceSettings _insurance = insuranceOptions.Value;

    public async Task<InsuranceEmailResult> SendIssuanceRequestAsync(
        InsuranceIssuanceEmailModel model, CancellationToken cancellationToken = default)
    {
        if (!_smtp.IsConfigured || !_insurance.IsConfigured)
        {
            logger.LogWarning(
                "Insurance carrier email not sent for {Code}: SMTP configured={Smtp}, carrier configured={Carrier}.",
                model.RequestCode, _smtp.IsConfigured, _insurance.IsConfigured);
            return InsuranceEmailResult.NotConfigured;
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_smtp.FromEmail, _smtp.FromName),
                Subject = $"INDOR — Insurance issuance request {model.RequestCode}",
                Body = BuildHtmlBody(model),
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(_insurance.CarrierEmail, _insurance.CarrierName));

            foreach (var copyEmail in ParseCopyEmails(_insurance.CopyToEmail, _insurance.CarrierEmail))
            {
                message.CC.Add(new MailAddress(copyEmail));
            }

            if (!string.IsNullOrWhiteSpace(model.OwnerEmail))
            {
                message.ReplyToList.Add(new MailAddress(model.OwnerEmail));
            }

            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                EnableSsl = _smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(_smtp.Username))
            {
                client.Credentials = new NetworkCredential(_smtp.Username, _smtp.Password);
            }

            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Insurance issuance email {Code} sent to carrier {Carrier}.",
                model.RequestCode, _insurance.CarrierEmail);
            return InsuranceEmailResult.Sent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send insurance issuance email {Code} to carrier.", model.RequestCode);
            return InsuranceEmailResult.Failed;
        }
    }

    /// <summary>
    /// Splits <paramref name="copyTo"/> on comma/semicolon and skips blanks
    /// or addresses that already match the primary carrier inbox.
    /// </summary>
    internal static IEnumerable<string> ParseCopyEmails(string? copyTo, string? carrierEmail)
    {
        if (string.IsNullOrWhiteSpace(copyTo))
        {
            yield break;
        }

        var carrier = carrierEmail?.Trim();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(carrier))
        {
            seen.Add(carrier);
        }

        foreach (var part in copyTo.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (seen.Add(part))
            {
                yield return part;
            }
        }
    }

    private string BuildHtmlBody(InsuranceIssuanceEmailModel m)
    {
        string E(string? v) => HtmlEncoder.Default.Encode(string.IsNullOrWhiteSpace(v) ? "—" : v.Trim());
        string YN(bool v) => v ? "Y" : "N";

        string Row(string label, string value) =>
            $@"<tr>
                 <td style=""padding:9px 14px;border-bottom:1px solid #EAF0F6;color:#64748B;font-size:13px;width:44%;"">{label}</td>
                 <td style=""padding:9px 14px;border-bottom:1px solid #EAF0F6;color:#0A2540;font-size:13px;font-weight:600;"">{value}</td>
               </tr>";

        var carrier = E(_insurance.CarrierName);
        var rows =
            Row("Business Name", E(m.BusinessName)) +
            Row("Business Address", E(m.BusinessAddress)) +
            Row("Workers Compensation", YN(m.WorkersComp)) +
            Row("General Liability", YN(m.GeneralLiability)) +
            Row("Owner's Name", E(m.OwnerName)) +
            Row("Owner's Date of Birth", E(m.OwnerDateOfBirth)) +
            Row("Owner's Phone", E(m.OwnerPhone)) +
            Row("Owner's Email", E(m.OwnerEmail)) +
            Row("Type of Business", E(m.TypeOfBusiness)) +
            Row("Employees", E(m.NumberOfEmployees)) +
            Row("Employee Payroll", E(m.EmployeePayroll)) +
            Row("Company GROSS", E(m.CompanyGross));

        var planLine = string.IsNullOrWhiteSpace(m.Plan)
            ? string.Empty
            : $@"<p style=""margin:0 0 14px;color:#475569;font-size:14px;"">Requested INDOR plan: <strong style=""color:#0066CC;"">{E(m.Plan)}</strong></p>";

        var notes = string.IsNullOrWhiteSpace(m.Notes)
            ? string.Empty
            : $@"<tr><td colspan=""2"" style=""padding:12px 14px;color:#334155;font-size:13px;background:#F8FAFC;""><strong>Notes:</strong> {E(m.Notes)}</td></tr>";

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /><meta name=""viewport"" content=""width=device-width,initial-scale=1"" /></head>
<body style=""margin:0;padding:0;background:#F1F5F9;font-family:Segoe UI,Arial,sans-serif;color:#0A2540;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0""><tr><td align=""center"" style=""padding:24px 12px;"">
    <table role=""presentation"" width=""620"" cellpadding=""0"" cellspacing=""0"" style=""max-width:620px;width:100%;background:#fff;border-radius:16px;overflow:hidden;border:1px solid #E2E8F0;"">
      <tr><td style=""padding:20px 24px;background:#0066CC;color:#fff;font-weight:800;font-size:18px;"">&#128737;&#65039; INDOR — Insurance Issuance Request</td></tr>
      <tr><td style=""padding:22px 24px 8px;"">
        <p style=""margin:0 0 6px;color:#0A2540;font-size:15px;"">Hello {carrier},</p>
        <p style=""margin:0 0 14px;color:#475569;font-size:14px;line-height:1.55;"">A provider registered in INDOR requests an insurance policy to be issued. Below is the completed Business Quote Sheet. Please issue the policy manually and reply to this email.</p>
        {planLine}
        <p style=""margin:0 0 16px;color:#94A3B8;font-size:12px;"">Request code: <strong style=""color:#0A2540;"">{E(m.RequestCode)}</strong></p>
      </td></tr>
      <tr><td style=""padding:0 24px 24px;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #EAF0F6;border-radius:12px;overflow:hidden;"">
          {rows}
          {notes}
        </table>
      </td></tr>
      <tr><td style=""padding:0 24px 24px;color:#94A3B8;font-size:12px;line-height:1.5;"">This request was generated automatically by INDOR. Reply to this email to reach the provider directly.</td></tr>
    </table>
  </td></tr></table>
</body>
</html>";
    }
}
