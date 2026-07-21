using System.Globalization;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Localization;

public static class CatalogText
{
    private static readonly Regex DaysBeforePattern =
        new(@"^(\d+)\s+days?\s+before$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string Pick(string? english, string? spanish, bool isSpanish)
    {
        if (isSpanish && !string.IsNullOrWhiteSpace(spanish))
        {
            return spanish;
        }

        return english ?? string.Empty;
    }

    public static string PickPipeList(string? english, string? spanish, bool isSpanish)
        => Pick(english, spanish, isSpanish);

    public static string PickWithUiFallback(string? english, string? spanish, bool isSpanish)
    {
        if (!isSpanish)
        {
            return english?.Trim() ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(spanish))
        {
            var spanishKey = spanish.Trim();
            // Fix imperfect *Es seeds (e.g. mixed EN/ES) via UI dictionary keys.
            if (UiTranslations.Spanish.TryGetValue(spanishKey, out var fromSpanishKey))
            {
                return fromSpanishKey;
            }

            return spanishKey;
        }

        var key = english?.Trim() ?? string.Empty;
        if (UiTranslations.Spanish.TryGetValue(key, out var translated))
        {
            return translated;
        }

        return TryFormatDaysBefore(key) ?? key;
    }

    /// <summary>Shared "N day(s) before" → Spanish via "{0} days before" / "1 day before".</summary>
    public static string? TryFormatDaysBefore(string? english)
    {
        if (string.IsNullOrWhiteSpace(english))
        {
            return null;
        }

        var match = DaysBeforePattern.Match(english.Trim());
        if (!match.Success || !int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days))
        {
            return null;
        }

        if (days == 1)
        {
            return UiTranslations.Spanish.TryGetValue("1 day before", out var oneDay)
                ? oneDay
                : null;
        }

        if (UiTranslations.Spanish.TryGetValue("{0} days before", out var template))
        {
            return string.Format(CultureInfo.CurrentCulture, template, days);
        }

        return null;
    }

    public static string PickPipeListWithUiFallback(string? english, string? spanish, bool isSpanish)
    {
        if (!isSpanish)
        {
            return english ?? string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(spanish))
        {
            return spanish;
        }

        if (string.IsNullOrWhiteSpace(english))
        {
            return string.Empty;
        }

        return string.Join("|",
            english.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(part => PickWithUiFallback(part, null, true)));
    }
}
