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

        if (localizer.IsSpanish)
        {
            var direct = localizer[text];
            if (!string.Equals(direct, text, StringComparison.Ordinal))
            {
                return direct;
            }
        }

        if (text.StartsWith("Today, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today, {0}", text["Today, ".Length..].Trim());
        }

        if (text.StartsWith("Yesterday, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Yesterday, {0}", text["Yesterday, ".Length..].Trim());
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

        if (string.Equals(text, "1 invited client awaiting response", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("1 invited client awaiting response");
        }

        var invitedClientsMatch = Regex.Match(text, @"^(\d+) invited clients awaiting response$", RegexOptions.IgnoreCase);
        if (invitedClientsMatch.Success && int.TryParse(invitedClientsMatch.Groups[1].Value, out var invitedCount))
        {
            return localizer.T("{0} invited clients awaiting response", invitedCount);
        }

        var pendingQuotesMatch = Regex.Match(text, @"^(\d+) clients? have pending quotes$", RegexOptions.IgnoreCase);
        if (pendingQuotesMatch.Success && int.TryParse(pendingQuotesMatch.Groups[1].Value, out var pendingQuotesCount))
        {
            var key = pendingQuotesCount == 1
                ? "{0} client has pending quotes"
                : "{0} clients have pending quotes";
            return localizer.T(key, pendingQuotesCount);
        }

        var clientFileMatch = Regex.Match(text, @"^(.+) File$", RegexOptions.IgnoreCase);
        if (clientFileMatch.Success)
        {
            return localizer.T("{0} File", clientFileMatch.Groups[1].Value.Trim());
        }

        if (text.StartsWith("Last updated ", StringComparison.OrdinalIgnoreCase))
        {
            var datePart = LocalizeRelativeTimestamp(localizer, text["Last updated ".Length..].Trim());
            return localizer.T("Last updated {0}", datePart);
        }

        var repairItemsMatch = Regex.Match(text, @"^Repair items: (\d+)$", RegexOptions.IgnoreCase);
        if (repairItemsMatch.Success && int.TryParse(repairItemsMatch.Groups[1].Value, out var repairCount))
        {
            return localizer.T("Repair items: {0}", repairCount);
        }

        var quotesReceivedMatch = Regex.Match(text, @"^Quotes received: (\d+)$", RegexOptions.IgnoreCase);
        if (quotesReceivedMatch.Success && int.TryParse(quotesReceivedMatch.Groups[1].Value, out var quotesCount))
        {
            return localizer.T("Quotes received: {0}", quotesCount);
        }

        var parsingReadyMatch = Regex.Match(text, @"^(\d+) reports? ready for parsing$", RegexOptions.IgnoreCase);
        if (parsingReadyMatch.Success && int.TryParse(parsingReadyMatch.Groups[1].Value, out var parsingCount))
        {
            var key = parsingCount == 1
                ? "{0} report ready for parsing"
                : "{0} reports ready for parsing";
            return localizer.T(key, parsingCount);
        }

        var contractorSelectionMatch = Regex.Match(text, @"^(\d+) files? need contractor selection$", RegexOptions.IgnoreCase);
        if (contractorSelectionMatch.Success && int.TryParse(contractorSelectionMatch.Groups[1].Value, out var contractorCount))
        {
            var key = contractorCount == 1
                ? "{0} file needs contractor selection"
                : "{0} files need contractor selection";
            return localizer.T(key, contractorCount);
        }

        var miMatch = Regex.Match(text, @"^([\d.]+) mi$", RegexOptions.IgnoreCase);
        if (miMatch.Success)
        {
            return localizer.T("{0} mi", miMatch.Groups[1].Value);
        }

        var thisMonthMatch = Regex.Match(text, @"^\+(\d+) this month$", RegexOptions.IgnoreCase);
        if (thisMonthMatch.Success && int.TryParse(thisMonthMatch.Groups[1].Value, out var thisMonthCount))
        {
            return localizer.T("+{0} this month", thisMonthCount);
        }

        var vsLastMonthMatch = Regex.Match(text, @"^\+(\d+)% vs last month$", RegexOptions.IgnoreCase);
        if (vsLastMonthMatch.Success && int.TryParse(vsLastMonthMatch.Groups[1].Value, out var pctChange))
        {
            return localizer.T("+{0}% vs last month", pctChange);
        }

        if (string.Equals(text, "+100% vs last month", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("+100% vs last month");
        }

        var attachDocMatch = Regex.Match(
            text,
            @"^Please attach your (.+) before continuing, or choose Skip for now\.$",
            RegexOptions.IgnoreCase);
        if (attachDocMatch.Success)
        {
            var docLabel = Localize(localizer, attachDocMatch.Groups[1].Value.Trim());
            return localizer.T("Please attach your {0} before continuing, or choose Skip for now.", docLabel);
        }

        if (text.StartsWith("Requested ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Requested {0}", text["Requested ".Length..].Trim());
        }

        if (text.StartsWith("Total ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Total {0}", text["Total ".Length..].Trim());
        }

        if (text.StartsWith("Starting at ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Starting at {0}", text["Starting at ".Length..].Trim());
        }

        var quotesReceivedPluralMatch = Regex.Match(text, @"^(\d+) Quotes Received$", RegexOptions.IgnoreCase);
        if (quotesReceivedPluralMatch.Success && int.TryParse(quotesReceivedPluralMatch.Groups[1].Value, out var quotesReceivedCount))
        {
            return localizer.T("{0} Quotes Received", quotesReceivedCount);
        }

        var quoteReceivedSingularMatch = Regex.Match(text, @"^(\d+) Quote Received$", RegexOptions.IgnoreCase);
        if (quoteReceivedSingularMatch.Success && int.TryParse(quoteReceivedSingularMatch.Groups[1].Value, out var quoteReceivedCount))
        {
            return localizer.T("{0} Quote Received", quoteReceivedCount);
        }

        var providerQuotesPluralMatch = Regex.Match(text, @"^(\d+) provider quotes received$", RegexOptions.IgnoreCase);
        if (providerQuotesPluralMatch.Success && int.TryParse(providerQuotesPluralMatch.Groups[1].Value, out var providerQuotesCount))
        {
            return localizer.T("{0} provider quotes received", providerQuotesCount);
        }

        var providerQuoteSingularMatch = Regex.Match(text, @"^(\d+) provider quote received$", RegexOptions.IgnoreCase);
        if (providerQuoteSingularMatch.Success && int.TryParse(providerQuoteSingularMatch.Groups[1].Value, out var providerQuoteCount))
        {
            return localizer.T("{0} provider quote received", providerQuoteCount);
        }

        var quotesReceivedFooterMatch = Regex.Match(text, @"^(\d+) quotes received$", RegexOptions.IgnoreCase);
        if (quotesReceivedFooterMatch.Success && int.TryParse(quotesReceivedFooterMatch.Groups[1].Value, out var footerQuotesCount))
        {
            return localizer.T("{0} quotes received", footerQuotesCount);
        }

        var reviewTodayMatch = Regex.Match(text, @"^(\d+) quotes? need review today$", RegexOptions.IgnoreCase);
        if (reviewTodayMatch.Success && int.TryParse(reviewTodayMatch.Groups[1].Value, out var reviewCount))
        {
            var key = reviewCount == 1 ? "{0} quote needs review today" : "{0} quotes need review today";
            return localizer.T(key, reviewCount);
        }

        var urgentProviderMatch = Regex.Match(text, @"^(\d+) urgent requests? need provider$", RegexOptions.IgnoreCase);
        if (urgentProviderMatch.Success && int.TryParse(urgentProviderMatch.Groups[1].Value, out var urgentCount))
        {
            var key = urgentCount == 1 ? "{0} urgent request needs provider" : "{0} urgent requests need provider";
            return localizer.T(key, urgentCount);
        }

        var selectedShareMatch = Regex.Match(text, @"^(\d+) selected quotes? ready to share$", RegexOptions.IgnoreCase);
        if (selectedShareMatch.Success && int.TryParse(selectedShareMatch.Groups[1].Value, out var selectedCount))
        {
            var key = selectedCount == 1 ? "{0} selected quote ready to share" : "{0} selected quotes ready to share";
            return localizer.T(key, selectedCount);
        }

        if (text.StartsWith("Serving ", StringComparison.OrdinalIgnoreCase)
            && text.EndsWith(" and surrounding areas", StringComparison.OrdinalIgnoreCase))
        {
            var cityPart = text["Serving ".Length..^" and surrounding areas".Length].Trim();
            return localizer.T("Serving {0} and surrounding areas", cityPart);
        }

        return localizer[text];
    }

    public static string LocalizeCommaList(IIndorLocalizer localizer, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        return string.Join(", ",
            text.Split(", ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(part => Localize(localizer, part)));
    }

    private static string LocalizeRelativeTimestamp(IIndorLocalizer localizer, string value)
    {
        if (value.StartsWith("Today, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today, {0}", value["Today, ".Length..].Trim());
        }

        if (value.StartsWith("Yesterday, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Yesterday, {0}", value["Yesterday, ".Length..].Trim());
        }

        return value;
    }
}
