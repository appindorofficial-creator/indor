using System.Text.RegularExpressions;
using IndorMvcApp.Services;

namespace IndorMvcApp.Helpers;

public static class UiDisplayLocalization
{
    private static readonly (Regex Regex, string Key)[] CountPatterns =
    [
        (new Regex(@"^(\d+)\s+nearby$", RegexOptions.IgnoreCase), "{0} nearby"),
        (new Regex(@"^(\d+)\s+airports?$", RegexOptions.IgnoreCase), "{0} airports"),
        (new Regex(@"^(\d+)\s+schools$", RegexOptions.IgnoreCase), "{0} schools"),
        (new Regex(@"^(\d+)\s+items$", RegexOptions.IgnoreCase), "{0} items"),
        (new Regex(@"^(\d+)\s+documents$", RegexOptions.IgnoreCase), "{0} documents"),
        (new Regex(@"^(\d+)\s+systems$", RegexOptions.IgnoreCase), "{0} systems"),
        (new Regex(@"^(\d+)\s+providers$", RegexOptions.IgnoreCase), "{0} providers"),
        (new Regex(@"^(\d+)\s+facts$", RegexOptions.IgnoreCase), "{0} facts"),
        (new Regex(@"^(\d+)\s+permit types$", RegexOptions.IgnoreCase), "{0} permit types"),
        (new Regex(@"^(\d+)\s+tasks?$", RegexOptions.IgnoreCase), "{0} tasks"),
        (new Regex(@"^(\d+)\s+bed$", RegexOptions.IgnoreCase), "{0} bed"),
        (new Regex(@"^(\d+)\s+beds$", RegexOptions.IgnoreCase), "{0} beds"),
        (new Regex(@"^(\d+(?:\.\d+)?)\s+baths?$", RegexOptions.IgnoreCase), "{0} baths"),
        (new Regex(@"^(\d{1,3}(?:,\d{3})*)\s+sqft$", RegexOptions.IgnoreCase), "{0} sqft"),
        (new Regex(@"^(\d{1,3}(?:,\d{3})*)\s+sq ft$", RegexOptions.IgnoreCase), "{0} sq ft"),
        (new Regex(@"^(\d+)\s+Quotes Received$", RegexOptions.IgnoreCase), "{0} Quotes Received"),
        (new Regex(@"^(\d+)\s+Quote Received$", RegexOptions.IgnoreCase), "{0} Quote Received"),
        (new Regex(@"^(\d+)\s+Quotes$", RegexOptions.IgnoreCase), "{0} Quotes"),
    ];

    public static string Localize(IIndorLocalizer localizer, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        foreach (var (regex, key) in CountPatterns)
        {
            var match = regex.Match(text);
            if (!match.Success)
            {
                continue;
            }

            var raw = match.Groups[1].Value;
            if (key.Contains("{0} baths", StringComparison.Ordinal))
            {
                return localizer.T(key, raw);
            }

            if (int.TryParse(raw.Replace(",", string.Empty), out var count))
            {
                return localizer.T(key, count);
            }

            return localizer.T(key, raw);
        }

        if (text.Contains(" · ", StringComparison.Ordinal))
        {
            return string.Join(" · ",
                text.Split(" · ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => Localize(localizer, part)));
        }

        if (text.Contains(" • ", StringComparison.Ordinal))
        {
            return string.Join(" • ",
                text.Split(" • ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => Localize(localizer, part)));
        }

        if (text.StartsWith("Uploaded ", StringComparison.OrdinalIgnoreCase))
        {
            var datePart = text["Uploaded ".Length..].Trim();
            return $"{localizer.T("Uploaded")} {datePart}";
        }

        if (text.StartsWith("Requested: ", StringComparison.OrdinalIgnoreCase))
        {
            var datePart = text["Requested: ".Length..].Trim();
            return localizer.T("Requested: {0}", datePart);
        }

        if (text.StartsWith("Due: ", StringComparison.OrdinalIgnoreCase))
        {
            var datePart = text["Due: ".Length..].Trim();
            return localizer.T("Due: {0}", datePart);
        }

        if (text.StartsWith("Shared ", StringComparison.OrdinalIgnoreCase))
        {
            var datePart = text["Shared ".Length..].Trim();
            return localizer.T("Shared {0}", datePart);
        }

        const string inspectionPackageSuffix = " - Inspection Package";
        if (text.EndsWith(inspectionPackageSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var address = text[..^inspectionPackageSuffix.Length].Trim();
            return localizer.T("{0} - Inspection Package", address);
        }

        return localizer[text];
    }
}
