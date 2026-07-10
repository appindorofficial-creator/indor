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
        (new Regex(@"^([\d.]+)\s+miles away$", RegexOptions.IgnoreCase), "{0} miles away"),
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
            return localizer.T("Requested {0}", text["Requested ".Length..].Trim());
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

        var hrAgoMatch = Regex.Match(text, @"^(\d+) hr ago$", RegexOptions.IgnoreCase);
        if (hrAgoMatch.Success && int.TryParse(hrAgoMatch.Groups[1].Value, out var hrsAgo))
        {
            return localizer.T("{0} hr ago", hrsAgo);
        }

        if (string.Equals(text, "just now", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("just now");
        }

        if (text.EndsWith(" Package", StringComparison.OrdinalIgnoreCase))
        {
            return localizer.T("{0} Package", text[..^" Package".Length].Trim());
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

        var atPropertyMatch = Regex.Match(text, @"^(.+) at (.+)$");
        if (atPropertyMatch.Success)
        {
            return localizer.T(
                "{0} at {1}",
                Localize(localizer, atPropertyMatch.Groups[1].Value.Trim()),
                atPropertyMatch.Groups[2].Value.Trim());
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
