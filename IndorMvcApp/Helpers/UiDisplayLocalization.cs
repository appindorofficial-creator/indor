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
        (new Regex(@"^([\d.]+) mi away$", RegexOptions.IgnoreCase), "{0} mi away"),
        (new Regex(@"^([\d.]+) miles around you$", RegexOptions.IgnoreCase), "{0} miles around you"),
        (new Regex(@"^([\d.]+) miles around (.+)$", RegexOptions.IgnoreCase), "{0} miles around {1}"),
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

            if (key.Contains("{0} miles around {1}", StringComparison.Ordinal) && match.Groups.Count > 2)
            {
                return localizer.T(key, raw, match.Groups[2].Value.Trim());
            }

            if (key.Contains("{0} mi away", StringComparison.Ordinal) || key.Contains("{0} miles around you", StringComparison.Ordinal))
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

        if (text.StartsWith("Shared with ", StringComparison.OrdinalIgnoreCase))
        {
            var namePart = text["Shared with ".Length..].Trim();
            return localizer.T("Shared with {0}", namePart);
        }

        var inspectionReadyMatch = Regex.Match(text, @"^(\d+) inspection reports? ready for review$", RegexOptions.IgnoreCase);
        if (inspectionReadyMatch.Success && int.TryParse(inspectionReadyMatch.Groups[1].Value, out var inspectionCount))
        {
            var key = inspectionCount == 1
                ? "{0} inspection report ready for review"
                : "{0} inspection reports ready for review";
            return localizer.T(key, inspectionCount);
        }

        var followUpMatch = Regex.Match(text, @"^(\d+) files? need client follow-up$", RegexOptions.IgnoreCase);
        if (followUpMatch.Success && int.TryParse(followUpMatch.Groups[1].Value, out var followUpCount))
        {
            var key = followUpCount == 1
                ? "{0} file needs client follow-up"
                : "{0} files need client follow-up";
            return localizer.T(key, followUpCount);
        }

        var providerSelectionMatch = Regex.Match(text, @"^(\d+) quote requests? need provider selection$", RegexOptions.IgnoreCase);
        if (providerSelectionMatch.Success && int.TryParse(providerSelectionMatch.Groups[1].Value, out var selectionCount))
        {
            var key = selectionCount == 1
                ? "{0} quote request needs provider selection"
                : "{0} quote requests need provider selection";
            return localizer.T(key, selectionCount);
        }

        return localizer[text];
    }
}
