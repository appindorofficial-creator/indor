using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Helpers;

public static class RealtorWizardReturnNavigation
{
    public const string ReturnToQueryKey = "returnTo";

    public const string Dashboard = "dashboard";
    public const string Files = "files";
    public const string Clients = "clients";
    public const string Quotes = "quotes";

    public const string InviteClientSessionKey = "RealtorInviteClientReturnTo";
    public const string PropertyFileSessionKey = "RealtorPropertyFileReturnTo";
    public const string QuoteRequestSessionKey = "RealtorQuoteRequestReturnTo";

    private static readonly HashSet<string> ValidTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        Dashboard,
        Files,
        Clients,
        Quotes
    };

    public static void CaptureReturnTo(
        ISession session,
        string? returnTo,
        string sessionKey,
        string defaultToken)
    {
        var token = NormalizeToken(returnTo) ?? defaultToken;
        session.SetString(sessionKey, token);
    }

    public static void CaptureReturnToIfMissing(
        ISession session,
        string sessionKey,
        string defaultToken)
    {
        if (string.IsNullOrWhiteSpace(session.GetString(sessionKey)))
        {
            session.SetString(sessionKey, defaultToken);
        }
    }

    public static string GetReturnToken(ISession session, string sessionKey, string defaultToken) =>
        NormalizeToken(session.GetString(sessionKey)) ?? defaultToken;

    public static void ClearReturnTo(ISession session, string sessionKey) =>
        session.Remove(sessionKey);

    public static string AppendReturnTo(string url, string token)
    {
        var normalized = NormalizeToken(token);
        if (normalized == null)
        {
            return url;
        }

        var separator = url.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return $"{url}{separator}{ReturnToQueryKey}={normalized}";
    }

    public static IActionResult RedirectTo(Controller controller, string token)
    {
        return NormalizeToken(token) switch
        {
            Files => controller.RedirectToAction("Files", "Realtor"),
            Clients => controller.RedirectToAction("Clients", "Realtor"),
            Quotes => controller.RedirectToAction("Quotes", "Realtor"),
            _ => controller.RedirectToAction("Dashboard", "Realtor"),
        };
    }

    private static string? NormalizeToken(string? returnTo)
    {
        if (string.IsNullOrWhiteSpace(returnTo))
        {
            return null;
        }

        var token = returnTo.Trim().ToLowerInvariant();
        return ValidTokens.Contains(token) ? token : null;
    }
}
