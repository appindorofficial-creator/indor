using System.Globalization;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Localization;

public static class CatalogText
{
    private static readonly Regex DaysBeforePattern =
        new(@"^(\d+)\s+days?\s+before$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Case-insensitive UI dictionary (same idea as <c>UiDisplayLocalization</c>).
    /// Skips identity translations (e.g. Drywall → Drywall) when a real Spanish value exists.
    /// </summary>
    private static readonly Lazy<IReadOnlyDictionary<string, string>> UiSpanishIgnoreCase = new(() =>
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in UiTranslations.Spanish)
        {
            if (!map.TryGetValue(pair.Key, out var existing)
                || string.Equals(existing, pair.Key, StringComparison.Ordinal))
            {
                map[pair.Key] = pair.Value;
            }
        }

        return map;
    });

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

        var en = english?.Trim() ?? string.Empty;
        var es = string.IsNullOrWhiteSpace(spanish) ? null : spanish.Trim();

        if (es is not null)
        {
            // LabelEs may itself be an English catalog key (or casing variant).
            if (TryResolveUiSpanish(es, out var fromSpanishKey))
            {
                return fromSpanishKey;
            }

            // Duplicate English in LabelEs (common seed mistake) must not block UI dictionary.
            if (!string.Equals(es, en, StringComparison.OrdinalIgnoreCase))
            {
                return es;
            }
        }

        if (TryResolveUiSpanish(en, out var translated))
        {
            return translated;
        }

        return TryFormatDaysBefore(en) ?? en;
    }

    /// <summary>
    /// Resolve English (or English-as-LabelEs) catalog text via UI Spanish dictionary.
    /// Identity translations (value == key) are treated as misses so incomplete merges
    /// like Drywall → Drywall do not block a later real translation.
    /// </summary>
    internal static bool TryResolveUiSpanish(string? key, out string translated)
    {
        translated = string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        key = key.Trim();

        if (UiTranslations.Spanish.TryGetValue(key, out var ordinal)
            && !string.Equals(ordinal, key, StringComparison.Ordinal))
        {
            translated = ordinal;
            return true;
        }

        if (UiSpanishIgnoreCase.Value.TryGetValue(key, out var ignoreCase)
            && !string.Equals(ignoreCase, key, StringComparison.OrdinalIgnoreCase))
        {
            translated = ignoreCase;
            return true;
        }

        return false;
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
