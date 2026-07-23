using System.Text;
using System.Text.Encodings.Web;

namespace IndorMvcApp.Services;

/// <summary>
/// Branded INDOR HTML email builder. Mirrors the visual language of the existing
/// password-reset / invitation emails (table layout, #0066CC accent, ~600px width).
/// </summary>
public static class IndorEmailTemplates
{
    private static string Enc(string? value) => HtmlEncoder.Default.Encode(value ?? string.Empty);

    /// <summary>Wraps content in the standard INDOR email shell.</summary>
    public static string Layout(string preheader, string bodyInner)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head><meta charset=""utf-8"" /><meta name=""viewport"" content=""width=device-width,initial-scale=1"" /></head>
<body style=""margin:0;padding:0;background:#F1F5F9;font-family:Segoe UI,Arial,sans-serif;color:#0A2540;"">
  <span style=""display:none!important;opacity:0;color:transparent;height:0;width:0;overflow:hidden;"">{Enc(preheader)}</span>
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F1F5F9;""><tr><td align=""center"" style=""padding:24px 12px;"">
    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;background:#FFFFFF;border-radius:20px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);"">

      <tr><td style=""background:linear-gradient(135deg,#0066CC,#0A2540);padding:22px 28px;"">
        <span style=""font-weight:800;color:#FFFFFF;font-size:20px;letter-spacing:.3px;"">&#127968; INDOR</span>
      </td></tr>

      <tr><td style=""padding:30px 28px 8px;"">
        {bodyInner}
      </td></tr>

      <tr><td style=""padding:24px 28px 30px;border-top:1px solid #EEF2F7;color:#94A3B8;font-size:12px;line-height:1.6;"">
        INDOR &middot; Your home, connected.<br />
        This is an automated message — please do not reply directly to this email.
      </td></tr>

    </table>
  </td></tr></table>
</body>
</html>";
    }

    private static string Heading(string text) =>
        $@"<h1 style=""font-size:23px;line-height:1.3;margin:0 0 14px;color:#0A2540;"">{Enc(text)}</h1>";

    private static string Paragraph(string text) =>
        $@"<p style=""margin:0 0 16px;color:#475569;font-size:15px;line-height:1.6;"">{Enc(text)}</p>";

    private static string Button(string label, string url) =>
        $@"<table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:8px 0 20px;""><tr><td style=""border-radius:12px;background:#0066CC;"">
            <a href=""{Enc(url)}"" style=""display:inline-block;padding:15px 30px;color:#FFFFFF;text-decoration:none;font-weight:700;font-size:16px;border-radius:12px;"">{Enc(label)} &#8594;</a>
          </td></tr></table>";

    private static string DetailCard(IEnumerable<(string Label, string Value)> rows)
    {
        var sb = new StringBuilder();
        sb.Append(@"<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F8FAFC;border:1px solid #E8EEF6;border-radius:16px;margin:4px 0 22px;""><tr><td style=""padding:18px 20px;"">");
        var first = true;
        foreach (var (label, value) in rows)
        {
            if (string.IsNullOrWhiteSpace(value)) continue;
            var marginTop = first ? "0" : "12px";
            first = false;
            sb.Append($@"<div style=""margin-top:{marginTop};"">
                <div style=""color:#94A3B8;font-size:11px;font-weight:700;text-transform:uppercase;letter-spacing:.06em;"">{Enc(label)}</div>
                <div style=""color:#0A2540;font-size:15px;font-weight:600;margin-top:2px;"">{Enc(value)}</div>
            </div>");
        }
        sb.Append("</td></tr></table>");
        return sb.ToString();
    }

    /// <summary>Email sent to each matching provider when a homeowner posts a request.</summary>
    public static string ProviderNewRequest(bool isSpanish, ProviderNewRequestEmail m)
    {
        var greetingName = string.IsNullOrWhiteSpace(m.ProviderName) ? (isSpanish ? "Hola" : "Hi") : $"{(isSpanish ? "Hola" : "Hi")} {m.ProviderName}";
        string inner;
        if (isSpanish)
        {
            inner = Heading("Nueva solicitud de servicio cerca de ti")
                + Paragraph($"{greetingName}, un propietario acaba de solicitar un servicio de \u201C{m.CategoryLabel}\u201D. Entra a la app INDOR PRO para revisarla y tomarla antes que otro proveedor.")
                + DetailCard(new[]
                {
                    ("Servicio", m.Title),
                    ("Categoría", m.CategoryLabel),
                    ("Ubicación", m.Location),
                    ("Cuándo", m.WhenLabel),
                    ("Presupuesto", m.BudgetLabel),
                    ("Detalles", m.Description),
                })
                + Button("Ver y tomar solicitud", m.ActionUrl)
                + Paragraph("El primer proveedor que la tome se queda con el trabajo — no te tardes.");
        }
        else
        {
            inner = Heading("New service request near you")
                + Paragraph($"{greetingName}, a homeowner just requested a \u201C{m.CategoryLabel}\u201D service. Open the INDOR PRO app to review it and claim it before another provider does.")
                + DetailCard(new[]
                {
                    ("Service", m.Title),
                    ("Category", m.CategoryLabel),
                    ("Location", m.Location),
                    ("When", m.WhenLabel),
                    ("Budget", m.BudgetLabel),
                    ("Details", m.Description),
                })
                + Button("View & take request", m.ActionUrl)
                + Paragraph("The first provider to take it wins the job — don't wait too long.");
        }

        var pre = isSpanish ? $"Nueva solicitud: {m.Title}" : $"New request: {m.Title}";
        return Layout(pre, inner);
    }

    /// <summary>Email sent to the homeowner when a provider claims their request.</summary>
    public static string HomeownerClaimed(bool isSpanish, HomeownerClaimedEmail m)
    {
        var greetingName = string.IsNullOrWhiteSpace(m.HomeownerName) ? (isSpanish ? "Hola" : "Hi") : $"{(isSpanish ? "Hola" : "Hi")} {m.HomeownerName}";
        string inner;
        if (isSpanish)
        {
            inner = Heading("¡Un proveedor tomó tu solicitud!")
                + Paragraph($"{greetingName}, buenas noticias: {m.ProviderName} aceptó tu solicitud de \u201C{m.Title}\u201D y se pondrá en contacto contigo. Aquí están sus datos:")
                + DetailCard(new[]
                {
                    ("Proveedor", m.ProviderName),
                    ("Contacto", m.ProviderContact),
                    ("Teléfono", m.ProviderPhone),
                    ("Correo", m.ProviderEmail),
                    ("Servicio", m.Title),
                })
                + Button("Ver detalles en INDOR", m.ActionUrl)
                + Paragraph("Puedes coordinar el resto directamente con el proveedor desde la app.");
        }
        else
        {
            inner = Heading("A provider took your request!")
                + Paragraph($"{greetingName}, good news: {m.ProviderName} accepted your \u201C{m.Title}\u201D request and will reach out to you. Here are their details:")
                + DetailCard(new[]
                {
                    ("Provider", m.ProviderName),
                    ("Contact", m.ProviderContact),
                    ("Phone", m.ProviderPhone),
                    ("Email", m.ProviderEmail),
                    ("Service", m.Title),
                })
                + Button("View details in INDOR", m.ActionUrl)
                + Paragraph("You can coordinate the rest directly with the provider from the app.");
        }

        var pre = isSpanish ? $"{m.ProviderName} tomó tu solicitud" : $"{m.ProviderName} took your request";
        return Layout(pre, inner);
    }
}

public sealed record ProviderNewRequestEmail(
    string? ProviderName,
    string Title,
    string CategoryLabel,
    string Location,
    string WhenLabel,
    string BudgetLabel,
    string Description,
    string ActionUrl);

public sealed record HomeownerClaimedEmail(
    string? HomeownerName,
    string ProviderName,
    string ProviderContact,
    string ProviderPhone,
    string ProviderEmail,
    string Title,
    string ActionUrl);
