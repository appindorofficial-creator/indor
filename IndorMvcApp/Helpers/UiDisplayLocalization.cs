using System.Text.RegularExpressions;
using IndorMvcApp.Services;

namespace IndorMvcApp.Helpers;

public static class UiDisplayLocalization
{
    private static readonly Lazy<IReadOnlyDictionary<string, string>> SpanishIgnoreCase = new(() =>
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in Localization.UiTranslations.Spanish)
        {
            map[pair.Key] = pair.Value;
        }

        return map;
    });

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
        (new Regex(@"^Built\s+(\d+)$", RegexOptions.IgnoreCase), "Built {0}"),
        (new Regex(@"^(\d{1,3}(?:,\d{3})*)\s+sq\s*ft$", RegexOptions.IgnoreCase), "{0} sq ft"),
        (new Regex(@"^(\d{1,3}(?:,\d{3})*)\s+sqft$", RegexOptions.IgnoreCase), "{0} sqft"),
        (new Regex(@"^([\d.]+)\s+acres$", RegexOptions.IgnoreCase), "{0} acres"),
        (new Regex(@"^([\d.]+) mi away$", RegexOptions.IgnoreCase), "{0} mi away"),
        (new Regex(@"^([\d.]+)\s+miles away$", RegexOptions.IgnoreCase), "{0} miles away"),
        (new Regex(@"^([\d.]+) miles around you$", RegexOptions.IgnoreCase), "{0} miles around you"),
        (new Regex(@"^([\d.]+) miles around (.+)$", RegexOptions.IgnoreCase), "{0} miles around {1}"),
        (new Regex(@"^(\d+)\s+helpers?$", RegexOptions.IgnoreCase), "{0} helpers"),
        (new Regex(@"^Min\.\s*(\d+)\s*hrs?$", RegexOptions.IgnoreCase), "Min. {0} hrs"),
        (new Regex(@"^\$(\d+(?:\.\d+)?)\s*/hr$", RegexOptions.IgnoreCase), "${0}/hr"),
        (new Regex(@"^(\d+)\s+offers received$", RegexOptions.IgnoreCase), "{0} offers received"),
        (new Regex(@"^(\d+)\s+offers$", RegexOptions.IgnoreCase), "{0} offers"),
        (new Regex(@"^(\d+)\s+urgent items?$", RegexOptions.IgnoreCase), "{0} urgent items"),
        (new Regex(@"^(\d+)\s+high-priority items?$", RegexOptions.IgnoreCase), "{0} high-priority items"),
        (new Regex(@"^(\d+)\s+moderate items?$", RegexOptions.IgnoreCase), "{0} moderate items"),
        (new Regex(@"^(\d+)\s+photos uploaded$", RegexOptions.IgnoreCase), "{0} photos uploaded"),
        (new Regex(@"^1 photo uploaded$", RegexOptions.IgnoreCase), "1 photo uploaded"),
        (new Regex(@"^(\d+)\s+photos$", RegexOptions.IgnoreCase), "{0} photos"),
        (new Regex(@"^(\d+)\s+attached$", RegexOptions.IgnoreCase), "{0} attached"),
        (new Regex(@"^(\d+)\s+uploaded$", RegexOptions.IgnoreCase), "{0} uploaded"),
        // Reminder lead chips (Lawn and sibling wizards)
        (new Regex(@"^(\d+)\s+days before$", RegexOptions.IgnoreCase), "{0} days before"),
    ];

    /// <summary>
    /// Shared entry point for portal topbar notification bodies (Realtor, Provider, PA, Homeowner).
    /// Keep English templates at creation time; localize here at display time.
    /// </summary>
    public static string LocalizeNotification(IIndorLocalizer localizer, string? text) =>
        Localize(localizer, text);

    public static string LocalizeNotificationTime(IIndorLocalizer localizer, string? text) =>
        Localize(localizer, text);

    public static string LocalizeNotificationTag(IIndorLocalizer localizer, string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return string.Empty;
        }

        return localizer.T(tag.Trim());
    }

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

            if (text.EndsWith(".", StringComparison.Ordinal) && text.Length > 1)
            {
                var withoutPeriod = text[..^1];
                var trimmed = localizer[withoutPeriod];
                if (!string.Equals(trimmed, withoutPeriod, StringComparison.Ordinal))
                {
                    return trimmed.EndsWith(".", StringComparison.Ordinal) ? trimmed : trimmed + ".";
                }
            }

            // Inspection report headings are often ALL CAPS; match catalog keys case-insensitively.
            if (SpanishIgnoreCase.Value.TryGetValue(text, out var ignoreCaseHit)
                && !string.Equals(ignoreCaseHit, text, StringComparison.Ordinal))
            {
                return ignoreCaseHit;
            }
        }

        var activeRiskMatch = Regex.Match(
            text,
            @"^(.+),\s*Active risk:\s*(.+)$",
            RegexOptions.IgnoreCase);
        if (activeRiskMatch.Success)
        {
            return localizer.T(
                "{0}, Active risk: {1}",
                Localize(localizer, activeRiskMatch.Groups[1].Value.Trim()),
                Localize(localizer, activeRiskMatch.Groups[2].Value.Trim()));
        }

        var visibleMoistureMatch = Regex.Match(
            text,
            @"^(.+),\s*Visible moisture:\s*(.+)$",
            RegexOptions.IgnoreCase);
        if (visibleMoistureMatch.Success)
        {
            return localizer.T(
                "{0}, Visible moisture: {1}",
                Localize(localizer, visibleMoistureMatch.Groups[1].Value.Trim()),
                Localize(localizer, visibleMoistureMatch.Groups[2].Value.Trim()));
        }

        // Card payment lines: "Visa ending in 4242" / "Expires 08/28"
        var endingInMatch = Regex.Match(text, @"^(.+?)\s+ending in\s+(\d{2,4})$", RegexOptions.IgnoreCase);
        if (endingInMatch.Success)
        {
            return localizer.T(
                "{0} ending in {1}",
                Localize(localizer, endingInMatch.Groups[1].Value.Trim()),
                endingInMatch.Groups[2].Value);
        }

        var expiresMatch = Regex.Match(text, @"^Expires\s+(.+)$", RegexOptions.IgnoreCase);
        if (expiresMatch.Success)
        {
            return localizer.T("Expires {0}", expiresMatch.Groups[1].Value.Trim());
        }

        if (text.StartsWith("Today, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today, {0}", Localize(localizer, text["Today, ".Length..].Trim()));
        }

        if (text.StartsWith("Today • ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today • {0}", Localize(localizer, text["Today • ".Length..].Trim()));
        }

        if (text.StartsWith("Yesterday, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Yesterday, {0}", Localize(localizer, text["Yesterday, ".Length..].Trim()));
        }

        if (text.StartsWith("Tomorrow, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Tomorrow, {0}", Localize(localizer, text["Tomorrow, ".Length..].Trim()));
        }

        var tradeNeeded = Regex.Match(text, @"^(.+?)\s+needed$", RegexOptions.IgnoreCase);
        if (tradeNeeded.Success)
        {
            return localizer.T("{0} needed", localizer[tradeNeeded.Groups[1].Value.Trim()]);
        }

        var tradeRequest = Regex.Match(text, @"^(.+?)\s+request$", RegexOptions.IgnoreCase);
        if (tradeRequest.Success)
        {
            return localizer.T("{0} request", localizer[tradeRequest.Groups[1].Value.Trim()]);
        }

        var monthPrice = Regex.Match(text, @"^\$(\d+(?:\.\d+)?)\s*/mo$", RegexOptions.IgnoreCase);
        if (monthPrice.Success)
        {
            return localizer.T("${0} /mo", monthPrice.Groups[1].Value);
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

        if (text.Contains(" | ", StringComparison.Ordinal))
        {
            return string.Join(" | ",
                text.Split(" | ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => Localize(localizer, part)));
        }

        if (text.Contains(" • ", StringComparison.Ordinal))
        {
            return string.Join(" • ",
                text.Split(" • ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => Localize(localizer, part)));
        }

        var summaryLabelMatch = Regex.Match(
            text,
            @"^(Bring|Pets|Stairs|Parking|Gate code|Helpers|Duration|Pay):\s*(.+)$",
            RegexOptions.IgnoreCase);
        if (summaryLabelMatch.Success)
        {
            var prefixKey = summaryLabelMatch.Groups[1].Value.Trim() switch
            {
                var p when p.Equals("Bring", StringComparison.OrdinalIgnoreCase) => "Bring:",
                var p when p.Equals("Pets", StringComparison.OrdinalIgnoreCase) => "Pets:",
                var p when p.Equals("Stairs", StringComparison.OrdinalIgnoreCase) => "Stairs:",
                var p when p.Equals("Parking", StringComparison.OrdinalIgnoreCase) => "Parking:",
                var p when p.Equals("Gate code", StringComparison.OrdinalIgnoreCase) => "Gate code:",
                var p when p.Equals("Helpers", StringComparison.OrdinalIgnoreCase) => "Helpers:",
                var p when p.Equals("Duration", StringComparison.OrdinalIgnoreCase) => "Duration:",
                var p when p.Equals("Pay", StringComparison.OrdinalIgnoreCase) => "Pay:",
                _ => summaryLabelMatch.Groups[1].Value.Trim() + ":"
            };
            var rawValue = summaryLabelMatch.Groups[2].Value.Trim();
            var localizedValue = rawValue.Contains(',', StringComparison.Ordinal)
                ? string.Join(", ",
                    rawValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => Localize(localizer, v)))
                : Localize(localizer, rawValue);
            return $"{localizer.T(prefixKey)} {localizedValue}";
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
            // Avoid mangling UI labels like "Send Entire File" into "Send Entire expediente".
            var prefix = clientFileMatch.Groups[1].Value.Trim();
            var looksLikeUiLabel =
                prefix.Contains(' ', StringComparison.Ordinal) &&
                (prefix.Contains("Entire", StringComparison.OrdinalIgnoreCase) ||
                 prefix.Contains("Property", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Send", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Open", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Upload", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("General", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Active", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Needs", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("New", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("View", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Repair", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Transfer", StringComparison.OrdinalIgnoreCase) ||
                 prefix.StartsWith("Pre-", StringComparison.OrdinalIgnoreCase));
            if (!looksLikeUiLabel)
            {
                return localizer.T("{0} File", prefix);
            }
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

        if (text.StartsWith("Sent ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Sent {0}", text["Sent ".Length..].Trim());
        }

        if (text.StartsWith("Viewed ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Viewed {0}", text["Viewed ".Length..].Trim());
        }

        if (text.StartsWith("Approved ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Approved {0}", text["Approved ".Length..].Trim());
        }

        if (text.StartsWith("Requested ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Requested {0}", Localize(localizer, text["Requested ".Length..].Trim()));
        }

        if (text.StartsWith("Budget ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Budget {0}", Localize(localizer, text["Budget ".Length..].Trim()));
        }

        if (text.StartsWith("Connected ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Connected {0}", text["Connected ".Length..].Trim());
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

        if (text.StartsWith("Due ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Due {0}", text["Due ".Length..].Trim());
        }

        if (text.StartsWith("Submitted ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Submitted {0}", text["Submitted ".Length..].Trim());
        }

        if (string.Equals(text, "Submitted recently", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Submitted recently");
        }

        if (string.Equals(text, "0 responses so far", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("0 responses so far");
        }

        var responsesMatch = Regex.Match(text, @"^(\d+) responses? so far$", RegexOptions.IgnoreCase);
        if (responsesMatch.Success && int.TryParse(responsesMatch.Groups[1].Value, out var responseCount))
        {
            var responseKey = responseCount == 1 ? "{0} response so far" : "{0} responses so far";
            return localizer.T(responseKey, responseCount);
        }

        if (string.Equals(text, "Quote received", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Quote received");
        }

        if (string.Equals(text, "Waiting", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Waiting");
        }

        if (text.StartsWith("Selected: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Selected: {0}", text["Selected: ".Length..].Trim());
        }

        if (text.StartsWith("Shared with ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Shared with {0}", text["Shared with ".Length..].Trim());
        }

        var daysRangeMatch = Regex.Match(text, @"^(\d+)\s*[–-]\s*(\d+)\s+days$", RegexOptions.IgnoreCase);
        if (daysRangeMatch.Success)
        {
            return localizer.T("{0} – {1} days", daysRangeMatch.Groups[1].Value, daysRangeMatch.Groups[2].Value);
        }

        var licenseMatch = Regex.Match(text, @"^License #(.+)$", RegexOptions.IgnoreCase);
        if (licenseMatch.Success)
        {
            return localizer.T("License #{0}", licenseMatch.Groups[1].Value);
        }

        if (text.StartsWith("Serving ", StringComparison.OrdinalIgnoreCase)
            && text.EndsWith(" and surrounding areas", StringComparison.OrdinalIgnoreCase))
        {
            var cityPart = text["Serving ".Length..^" and surrounding areas".Length].Trim();
            return localizer.T("Serving {0} and surrounding areas", cityPart);
        }

        var servingMatch = Regex.Match(text, @"^Serving (.+)$", RegexOptions.IgnoreCase);
        if (servingMatch.Success)
        {
            return localizer.T("Serving {0}", servingMatch.Groups[1].Value);
        }

        if (text.StartsWith("Homeowner viewed the quote ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Homeowner viewed the quote {0}", text["Homeowner viewed the quote ".Length..].Trim());
        }

        if (text.StartsWith("Quote delivered ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Quote delivered {0}", text["Quote delivered ".Length..].Trim());
        }

        var quoteSelectedMatch = Regex.Match(text, @"^Quote selected for (.+) — (.+) \((.+)\)$");
        if (quoteSelectedMatch.Success)
        {
            return localizer.T(
                "Quote selected for {0} — {1} ({2})",
                quoteSelectedMatch.Groups[1].Value,
                quoteSelectedMatch.Groups[2].Value,
                quoteSelectedMatch.Groups[3].Value);
        }

        var minAgoMatch = Regex.Match(text, @"^(\d+) min ago$", RegexOptions.IgnoreCase);
        if (minAgoMatch.Success && int.TryParse(minAgoMatch.Groups[1].Value, out var minsAgo))
        {
            return localizer.T("{0} min ago", minsAgo);
        }

        if (string.Equals(text, "Posted just now", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Posted just now");
        }

        var postedMinsMatch = Regex.Match(text, @"^Posted (\d+) mins? ago$", RegexOptions.IgnoreCase);
        if (postedMinsMatch.Success && int.TryParse(postedMinsMatch.Groups[1].Value, out var postedMins))
        {
            return localizer.T(postedMins == 1 ? "Posted {0} min ago" : "Posted {0} mins ago", postedMins);
        }

        var postedHoursMatch = Regex.Match(text, @"^Posted (\d+) hours? ago$", RegexOptions.IgnoreCase);
        if (postedHoursMatch.Success && int.TryParse(postedHoursMatch.Groups[1].Value, out var postedHours))
        {
            return localizer.T(postedHours == 1 ? "Posted {0} hour ago" : "Posted {0} hours ago", postedHours);
        }

        var postedDaysMatch = Regex.Match(text, @"^Posted (\d+) days? ago$", RegexOptions.IgnoreCase);
        if (postedDaysMatch.Success && int.TryParse(postedDaysMatch.Groups[1].Value, out var postedDays))
        {
            return localizer.T(postedDays == 1 ? "Posted {0} day ago" : "Posted {0} days ago", postedDays);
        }

        var clockMatch = Regex.Match(text, @"^(\d{1,2}:\d{2})\s*(AM|PM)$", RegexOptions.IgnoreCase);
        if (clockMatch.Success)
        {
            var period = clockMatch.Groups[2].Value.Equals("AM", StringComparison.OrdinalIgnoreCase)
                ? localizer.T("a. m.")
                : localizer.T("p. m.");
            return $"{clockMatch.Groups[1].Value} {period}";
        }

        var hrAgoMatch = Regex.Match(text, @"^(\d+) hr ago$", RegexOptions.IgnoreCase);
        if (hrAgoMatch.Success && int.TryParse(hrAgoMatch.Groups[1].Value, out var hrsAgo))
        {
            return localizer.T("{0} hr ago", hrsAgo);
        }

        var hourAgoMatch = Regex.Match(text, @"^(\d+)\s+hours?\s+ago$", RegexOptions.IgnoreCase);
        if (hourAgoMatch.Success && int.TryParse(hourAgoMatch.Groups[1].Value, out var hoursAgo))
        {
            return localizer.T(hoursAgo == 1 ? "{0} hour ago" : "{0} hours ago", hoursAgo);
        }

        var dayAgoMatch = Regex.Match(text, @"^(\d+)\s+days?\s+ago$", RegexOptions.IgnoreCase);
        if (dayAgoMatch.Success && int.TryParse(dayAgoMatch.Groups[1].Value, out var daysAgo))
        {
            return localizer.T(daysAgo == 1 ? "{0} day ago" : "{0} days ago", daysAgo);
        }

        if (string.Equals(text, "just now", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("just now");
        }

        if (text.EndsWith(" Package", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("{0} Package", text[..^" Package".Length].Trim());
        }

        // Time-of-day schedule labels: "Morning 8–11", "Afternoon 2–5", etc.
        var timeOfDayMatch = Regex.Match(
            text,
            @"^(Morning|Midday|Afternoon|Evening)\s+(.+)$",
            RegexOptions.IgnoreCase);
        if (timeOfDayMatch.Success)
        {
            var period = localizer.T(timeOfDayMatch.Groups[1].Value);
            return $"{period} {timeOfDayMatch.Groups[2].Value}";
        }

        if (text.StartsWith("From: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("From: {0}", text["From: ".Length..].Trim());
        }

        if (text.StartsWith("Pressed: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Pressed: {0}", text["Pressed: ".Length..].Trim());
        }

        if (text.StartsWith("Now: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Now: {0}", text["Now: ".Length..].Trim());
        }

        var itemsMatch = Regex.Match(text, @"^(\d+) Items?$", RegexOptions.IgnoreCase);
        if (itemsMatch.Success && int.TryParse(itemsMatch.Groups[1].Value, out var itemsCount))
        {
            return localizer.T(itemsCount == 1 ? "{0} Item" : "{0} Items", itemsCount);
        }

        var jobsTodayMatch = Regex.Match(text, @"^(\d+) Jobs? Today$", RegexOptions.IgnoreCase);
        if (jobsTodayMatch.Success && int.TryParse(jobsTodayMatch.Groups[1].Value, out var jobsTodayCount))
        {
            return localizer.T(jobsTodayCount == 1 ? "{0} Job Today" : "{0} Jobs Today", jobsTodayCount);
        }

        var newLeadsMatch = Regex.Match(text, @"^(\d+) New Leads?$", RegexOptions.IgnoreCase);
        if (newLeadsMatch.Success && int.TryParse(newLeadsMatch.Groups[1].Value, out var newLeadsCount))
        {
            return localizer.T(newLeadsCount == 1 ? "{0} New Lead" : "{0} New Leads", newLeadsCount);
        }

        var estimatesMatch = Regex.Match(text, @"^(\d+) Estimates?$", RegexOptions.IgnoreCase);
        if (estimatesMatch.Success && int.TryParse(estimatesMatch.Groups[1].Value, out var estimatesCount))
        {
            return localizer.T(estimatesCount == 1 ? "{0} Estimate" : "{0} Estimates", estimatesCount);
        }

        var completedJobsMatch = Regex.Match(text, @"^(\d+) Completed Jobs?$", RegexOptions.IgnoreCase);
        if (completedJobsMatch.Success && int.TryParse(completedJobsMatch.Groups[1].Value, out var completedJobsCount))
        {
            return localizer.T(completedJobsCount == 1 ? "{0} Completed Job" : "{0} Completed Jobs", completedJobsCount);
        }

        var activeJobsMatch = Regex.Match(text, @"^(\d+) Active Jobs?$", RegexOptions.IgnoreCase);
        if (activeJobsMatch.Success && int.TryParse(activeJobsMatch.Groups[1].Value, out var activeJobsCount))
        {
            return localizer.T(activeJobsCount == 1 ? "{0} Active Job" : "{0} Active Jobs", activeJobsCount);
        }

        var propertiesMatch = Regex.Match(text, @"^(\d+) propert(?:y|ies)$", RegexOptions.IgnoreCase);
        if (propertiesMatch.Success && int.TryParse(propertiesMatch.Groups[1].Value, out var propertiesCount))
        {
            return localizer.T(propertiesCount == 1 ? "{0} property" : "{0} properties", propertiesCount);
        }

        var reportsApprovalMatch = Regex.Match(text, @"^(\d+) reports? need homeowner approval$", RegexOptions.IgnoreCase);
        if (reportsApprovalMatch.Success && int.TryParse(reportsApprovalMatch.Groups[1].Value, out var reportsApprovalCount))
        {
            return localizer.T(
                reportsApprovalCount == 1 ? "{0} report needs homeowner approval" : "{0} reports need homeowner approval",
                reportsApprovalCount);
        }

        var reportsUploadMatch = Regex.Match(text, @"^(\d+) reports? ready to upload$", RegexOptions.IgnoreCase);
        if (reportsUploadMatch.Success && int.TryParse(reportsUploadMatch.Groups[1].Value, out var reportsUploadCount))
        {
            return localizer.T(
                reportsUploadCount == 1 ? "{0} report ready to upload" : "{0} reports ready to upload",
                reportsUploadCount);
        }

        var invoicesOverdueMatch = Regex.Match(text, @"^(\d+) invoices? (?:is|are) overdue$", RegexOptions.IgnoreCase);
        if (invoicesOverdueMatch.Success && int.TryParse(invoicesOverdueMatch.Groups[1].Value, out var invoicesOverdueCount))
        {
            return localizer.T(
                invoicesOverdueCount == 1 ? "{0} invoice is overdue" : "{0} invoices are overdue",
                invoicesOverdueCount);
        }

        var leadsAreaMatch = Regex.Match(text, @"^(\d+) leads? match your service area$", RegexOptions.IgnoreCase);
        if (leadsAreaMatch.Success && int.TryParse(leadsAreaMatch.Groups[1].Value, out var leadsAreaCount))
        {
            return localizer.T(
                leadsAreaCount == 1 ? "{0} lead matches your service area" : "{0} leads match your service area",
                leadsAreaCount);
        }

        if (text.StartsWith("Scheduled — ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Scheduled — {0}", text["Scheduled — ".Length..].Trim());
        }

        if (text.StartsWith("Today • ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today • {0}", text["Today • ".Length..].Trim());
        }

        if (text.StartsWith("Tomorrow • ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Tomorrow • {0}", text["Tomorrow • ".Length..].Trim());
        }

        var timeBulletMatch = Regex.Match(text, @"^([A-Za-z]{3} \d+) • (.+)$");
        if (timeBulletMatch.Success)
        {
            return localizer.T("{0} • {1}", timeBulletMatch.Groups[1].Value, timeBulletMatch.Groups[2].Value);
        }

        var estimateAmountMatch = Regex.Match(text, @"^Estimate \$(.+)$", RegexOptions.IgnoreCase);
        if (estimateAmountMatch.Success)
        {
            return localizer.T("Estimate ${0}", estimateAmountMatch.Groups[1].Value);
        }

        if (text.EndsWith(" Lead", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("{0} Lead", text[..^" Lead".Length].Trim());
        }

        if (text.StartsWith("Viewing: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Viewing: {0}", text["Viewing: ".Length..].Trim());
        }

        if (text.StartsWith("Next: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Next: {0}", text["Next: ".Length..].Trim());
        }

        // Realtor activity notifications (must run before the generic "at/in/en" matcher).
        // Accept Spanglish "… interest en …" when a prior pass already swapped in→en.
        var expressedInterestMatch = Regex.Match(
            text,
            @"^You expressed interest (?:in|en) (.+)$",
            RegexOptions.IgnoreCase);
        if (expressedInterestMatch.Success)
        {
            return localizer.T(
                "You expressed interest in {0}",
                expressedInterestMatch.Groups[1].Value.Trim());
        }

        var interestedListingMatch = Regex.Match(
            text,
            @"^(.+) is interested (?:in|en) your listing (?:at|in|en) (.+)$",
            RegexOptions.IgnoreCase);
        if (interestedListingMatch.Success)
        {
            return localizer.T(
                "{0} is interested in your listing at {1}",
                Localize(localizer, interestedListingMatch.Groups[1].Value.Trim()),
                interestedListingMatch.Groups[2].Value.Trim());
        }

        var urgentQuoteSentMatch = Regex.Match(
            text,
            @"^Urgent (.+) quote (.+) sent for (.+)$",
            RegexOptions.IgnoreCase);
        if (urgentQuoteSentMatch.Success)
        {
            return localizer.T(
                "Urgent {0} quote {1} sent for {2}",
                Localize(localizer, urgentQuoteSentMatch.Groups[1].Value.Trim()),
                urgentQuoteSentMatch.Groups[2].Value.Trim(),
                urgentQuoteSentMatch.Groups[3].Value.Trim());
        }

        var fileCreatedMatch = Regex.Match(text, @"^(.+) created for (.+)$");
        if (fileCreatedMatch.Success)
        {
            return localizer.T(
                "{0} created for {1}",
                Localize(localizer, fileCreatedMatch.Groups[1].Value.Trim()),
                fileCreatedMatch.Groups[2].Value.Trim());
        }

        if (text.StartsWith("Created property ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Created property {0}", text["Created property ".Length..].Trim());
        }

        var quoteRequestSentMatch = Regex.Match(
            text,
            @"^Quote request (.+) sent to (\d+) providers? for (.+)$",
            RegexOptions.IgnoreCase);
        if (quoteRequestSentMatch.Success
            && int.TryParse(quoteRequestSentMatch.Groups[2].Value, out var quoteProviderCount))
        {
            var quoteRequestKey = quoteProviderCount == 1
                ? "Quote request {0} sent to {1} provider for {2}"
                : "Quote request {0} sent to {1} providers for {2}";
            return localizer.T(
                quoteRequestKey,
                quoteRequestSentMatch.Groups[1].Value.Trim(),
                quoteProviderCount,
                quoteRequestSentMatch.Groups[3].Value.Trim());
        }

        var quoteSharedMatch = Regex.Match(text, @"^Quote shared with (.+) for (.+)$", RegexOptions.IgnoreCase);
        if (quoteSharedMatch.Success)
        {
            return localizer.T(
                "Quote shared with {0} for {1}",
                quoteSharedMatch.Groups[1].Value.Trim(),
                quoteSharedMatch.Groups[2].Value.Trim());
        }

        var submittedQuoteMatch = Regex.Match(
            text,
            @"^(.+) submitted a quote for (.+) \((.+)\) — (.+)$",
            RegexOptions.IgnoreCase);
        if (submittedQuoteMatch.Success)
        {
            return localizer.T(
                "{0} submitted a quote for {1} ({2}) — {3}",
                submittedQuoteMatch.Groups[1].Value.Trim(),
                submittedQuoteMatch.Groups[2].Value.Trim(),
                submittedQuoteMatch.Groups[3].Value.Trim(),
                submittedQuoteMatch.Groups[4].Value.Trim());
        }

        // Provider PRO notification templates
        var newLeadMatch = Regex.Match(text, @"^New lead: (.+) — (.+)$", RegexOptions.IgnoreCase);
        if (newLeadMatch.Success)
        {
            return localizer.T(
                "New lead: {0} — {1}",
                Localize(localizer, newLeadMatch.Groups[1].Value.Trim()),
                Localize(localizer, newLeadMatch.Groups[2].Value.Trim()));
        }

        if (text.StartsWith("Estimate approved for ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T(
                "Estimate approved for {0}",
                text["Estimate approved for ".Length..].Trim());
        }

        if (text.StartsWith("Homeowner viewed your estimate for ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T(
                "Homeowner viewed your estimate for {0}",
                text["Homeowner viewed your estimate for ".Length..].Trim());
        }

        var overdueInvoiceMatch = Regex.Match(text, @"^Overdue invoice (.+) — (.+)$", RegexOptions.IgnoreCase);
        if (overdueInvoiceMatch.Success)
        {
            return localizer.T(
                "Overdue invoice {0} — {1}",
                overdueInvoiceMatch.Groups[1].Value.Trim(),
                overdueInvoiceMatch.Groups[2].Value.Trim());
        }

        var paymentReceivedMatch = Regex.Match(text, @"^Payment received for (.+) — (.+)$", RegexOptions.IgnoreCase);
        if (paymentReceivedMatch.Success)
        {
            return localizer.T(
                "Payment received for {0} — {1}",
                paymentReceivedMatch.Groups[1].Value.Trim(),
                paymentReceivedMatch.Groups[2].Value.Trim());
        }

        if (string.Equals(text, "New message waiting", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("New message waiting");
        }

        if (string.Equals(text, "your service area", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("your service area");
        }

        if (string.Equals(text, "Now", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Now");
        }

        var hoursAgoShortMatch = Regex.Match(text, @"^(\d+)h ago$", RegexOptions.IgnoreCase);
        if (hoursAgoShortMatch.Success && int.TryParse(hoursAgoShortMatch.Groups[1].Value, out var hoursAgoShort))
        {
            return localizer.T("{0}h ago", hoursAgoShort);
        }

        // Remaining realtor activity templates
        if (text.StartsWith("Contact initiated for lead: ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T(
                "Contact initiated for lead: {0}",
                text["Contact initiated for lead: ".Length..].Trim());
        }

        var inspectionAnalyzedMatch = Regex.Match(
            text,
            @"^Inspection analyzed — (\d+) quote requests? for (.+)$",
            RegexOptions.IgnoreCase);
        if (inspectionAnalyzedMatch.Success
            && int.TryParse(inspectionAnalyzedMatch.Groups[1].Value, out var analyzedQuoteCount))
        {
            var analyzedKey = analyzedQuoteCount == 1
                ? "Inspection analyzed — {0} quote request for {1}"
                : "Inspection analyzed — {0} quote requests for {1}";
            return localizer.T(
                analyzedKey,
                analyzedQuoteCount,
                inspectionAnalyzedMatch.Groups[2].Value.Trim());
        }

        if (string.Equals(text, "Inspection analyzed", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Inspection analyzed");
        }

        var findingsSentMatch = Regex.Match(text, @"^(\d+) findings? sent for (.+)$", RegexOptions.IgnoreCase);
        if (findingsSentMatch.Success && int.TryParse(findingsSentMatch.Groups[1].Value, out var findingsSentCount))
        {
            var findingsKey = findingsSentCount == 1
                ? "{0} finding sent for {1}"
                : "{0} findings sent for {1}";
            return localizer.T(
                findingsKey,
                findingsSentCount,
                findingsSentMatch.Groups[2].Value.Trim());
        }

        if (string.Equals(text, "Findings sent to providers", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Findings sent to providers");
        }

        // PA nearest-pro ETA lines (must run before the generic "at/in/en" matcher,
        // which would otherwise split on "available in N minutes").
        var nearestProMatch = Regex.Match(
            text,
            @"^Nearest (.+?) pro available in (\d+) minutes$",
            RegexOptions.IgnoreCase);
        if (nearestProMatch.Success)
        {
            return localizer.T(
                "Nearest {0} pro available in {1} minutes",
                Localize(localizer, nearestProMatch.Groups[1].Value.Trim()),
                nearestProMatch.Groups[2].Value);
        }

        // Match "Name at Address" style labels (also Spanglish "Lawn Care en …" / "… in …").
        // Never rewrite phrases that contain "at least" (e.g. validation: "select at least one…"),
        // or "{0} at {1}" becomes "{0} en {1}" and yields "… en least …".
        // Skip realtor notification bodies — they have dedicated templates above; this matcher
        // would otherwise turn "You expressed interest in …" into Spanglish "… interest en …".
        if (!text.Contains(" at least ", StringComparison.OrdinalIgnoreCase)
            && !text.Contains(" at least.", StringComparison.OrdinalIgnoreCase)
            && !text.StartsWith("You expressed interest ", StringComparison.OrdinalIgnoreCase)
            && !text.StartsWith("Created property ", StringComparison.OrdinalIgnoreCase)
            && !text.Contains(" is interested ", StringComparison.OrdinalIgnoreCase)
            && !text.Contains(" interested in your listing ", StringComparison.OrdinalIgnoreCase)
            && !text.Contains(" interested en your listing ", StringComparison.OrdinalIgnoreCase))
        {
            var atPropertyMatch = Regex.Match(
                text,
                @"^(.+?)\s+(?:at|in|en)\s+(.+)$",
                RegexOptions.IgnoreCase);
            if (atPropertyMatch.Success
                && !atPropertyMatch.Groups[2].Value.StartsWith("least", StringComparison.OrdinalIgnoreCase))
            {
                return localizer.T(
                    "{0} at {1}",
                    Localize(localizer, atPropertyMatch.Groups[1].Value.Trim()),
                    Localize(localizer, atPropertyMatch.Groups[2].Value.Trim()));
            }
        }

        var timestampAtMatch = Regex.Match(text, @"^(.+?) at (\d{1,2}:\d{2} [AP]M)$", RegexOptions.IgnoreCase);
        if (timestampAtMatch.Success)
        {
            return localizer.T("{0} at {1}", timestampAtMatch.Groups[1].Value.Trim(), timestampAtMatch.Groups[2].Value.Trim());
        }

        var greetingMatch = Regex.Match(text, @"^(Good morning|Good afternoon|Good evening), (.+)$", RegexOptions.IgnoreCase);
        if (greetingMatch.Success)
        {
            return localizer.T(
                "{0}, {1}",
                localizer.T(greetingMatch.Groups[1].Value),
                greetingMatch.Groups[2].Value.Trim());
        }

        var portfolioNameMatch = Regex.Match(text, @"^(.+) Portfolio$");
        if (portfolioNameMatch.Success && !text.Contains(" & ", StringComparison.Ordinal))
        {
            return localizer.T("{0} Portfolio", portfolioNameMatch.Groups[1].Value.Trim());
        }

        var activePropertyMatch = Regex.Match(text, @"^(\d+) active (property|properties)$", RegexOptions.IgnoreCase);
        if (activePropertyMatch.Success && int.TryParse(activePropertyMatch.Groups[1].Value, out var activePropertyCount))
        {
            var isSingle = string.Equals(activePropertyMatch.Groups[2].Value, "property", StringComparison.OrdinalIgnoreCase);
            return localizer.T(isSingle ? "{0} active property" : "{0} active properties", activePropertyCount);
        }

        if (string.Equals(text, "AI · Urgent", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("AI · Urgent");
        }

        if (string.Equals(text, "AI · Recommended", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("AI · Recommended");
        }

        if (string.Equals(text, "Pending schedule", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Pending schedule");
        }

        if (string.Equals(text, "Updated in your home profile.", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Updated in your home profile.");
        }

        var workDurationMatch = Regex.Match(text, @"^(\d+)h (\d+)m$");
        if (workDurationMatch.Success)
        {
            return localizer.T("{0}h {1}m", workDurationMatch.Groups[1].Value, workDurationMatch.Groups[2].Value);
        }

        var workHoursOnlyMatch = Regex.Match(text, @"^(\d+)h$");
        if (workHoursOnlyMatch.Success)
        {
            return localizer.T("{0}h", workHoursOnlyMatch.Groups[1].Value);
        }

        var invitationSentMatch = Regex.Match(text, @"^Invitation sent to (.+) for (.+)$");
        if (invitationSentMatch.Success)
        {
            return localizer.T(
                "Invitation sent to {0} for {1}",
                invitationSentMatch.Groups[1].Value.Trim(),
                invitationSentMatch.Groups[2].Value.Trim());
        }

        var invitationAcceptedMatch = Regex.Match(text, @"^(.+) accepted the invitation for (.+)$");
        if (invitationAcceptedMatch.Success)
        {
            return localizer.T(
                "{0} accepted the invitation for {1}",
                invitationAcceptedMatch.Groups[1].Value.Trim(),
                invitationAcceptedMatch.Groups[2].Value.Trim());
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

    /// <summary>
    /// Inspector wording is often a raw English PDF quote. When UI is Spanish, prefer a Spanish
    /// AI summary (description) if the excerpt was not translated and still looks English.
    /// </summary>
    public static string LocalizeInspectorWording(
        IIndorLocalizer localizer,
        string? sourceExcerpt,
        string? spanishFallbackDescription)
    {
        if (!localizer.IsSpanish)
        {
            return string.IsNullOrWhiteSpace(sourceExcerpt)
                ? (spanishFallbackDescription ?? string.Empty)
                : Localize(localizer, sourceExcerpt);
        }

        if (!string.IsNullOrWhiteSpace(sourceExcerpt))
        {
            var localized = Localize(localizer, sourceExcerpt);
            if (!string.Equals(localized, sourceExcerpt, StringComparison.Ordinal)
                || !AppearsPrimarilyEnglish(sourceExcerpt))
            {
                return localized;
            }
        }

        if (!string.IsNullOrWhiteSpace(spanishFallbackDescription))
        {
            return Localize(localizer, spanishFallbackDescription);
        }

        return sourceExcerpt ?? string.Empty;
    }

    public static bool AppearsPrimarilyEnglish(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (text.IndexOfAny(['á', 'é', 'í', 'ó', 'ú', 'ñ', 'ü', 'Á', 'É', 'Í', 'Ó', 'Ú', 'Ñ', 'Ü', '¿', '¡']) >= 0)
        {
            return false;
        }

        return Regex.IsMatch(
            text,
            @"\b(the|are|is|was|were|and|with|from|this|that|than|of|on|in|to|for|not|a|an)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string LocalizeRelativeTimestamp(IIndorLocalizer localizer, string value)
    {
        if (value.StartsWith("Today, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today, {0}", Localize(localizer, value["Today, ".Length..].Trim()));
        }

        if (value.StartsWith("Today • ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Today • {0}", Localize(localizer, value["Today • ".Length..].Trim()));
        }

        if (value.StartsWith("Yesterday, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Yesterday, {0}", Localize(localizer, value["Yesterday, ".Length..].Trim()));
        }

        if (value.StartsWith("Tomorrow, ", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("Tomorrow, {0}", Localize(localizer, value["Tomorrow, ".Length..].Trim()));
        }

        return Localize(localizer, value);
    }
}
