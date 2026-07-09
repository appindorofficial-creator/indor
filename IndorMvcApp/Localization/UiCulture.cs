using System.Globalization;

namespace IndorMvcApp.Localization;

public static class UiCulture
{
    public const string English = "en-US";
    public const string Spanish = "es-US";
    public const string CookieName = ".Indor.UiCulture";

    public static readonly string[] Supported =
    [
        English,
        Spanish
    ];

    public static bool IsSupported(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return false;
        }

        return Supported.Contains(Normalize(culture), StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return English;
        }

        var trimmed = culture.Trim();
        if (trimmed.Equals("en", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("en-us", StringComparison.OrdinalIgnoreCase))
        {
            return English;
        }

        if (trimmed.Equals("es", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("es-us", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("es-mx", StringComparison.OrdinalIgnoreCase))
        {
            return Spanish;
        }

        return Supported.Contains(trimmed, StringComparer.OrdinalIgnoreCase) ? trimmed : English;
    }

    public static CultureInfo ToCultureInfo(string? culture)
        => CultureInfo.GetCultureInfo(Normalize(culture));

    public static bool IsSpanish(string? culture)
        => Normalize(culture).Equals(Spanish, StringComparison.OrdinalIgnoreCase);

    public static string HtmlLang(string? culture)
        => IsSpanish(culture) ? "es" : "en";
}
