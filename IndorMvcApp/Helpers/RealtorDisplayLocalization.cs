using System.Globalization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;

namespace IndorMvcApp.Helpers;

public static class RealtorDisplayLocalization
{
    public static string Localize(IIndorLocalizer localizer, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text ?? string.Empty;
        }

        return localizer[text];
    }

    public static string FilePhaseLabel(IIndorLocalizer localizer, string? phase)
    {
        if (string.IsNullOrWhiteSpace(phase))
        {
            return localizer.T("Property File");
        }

        foreach (var option in RealtorPropertyFilePhases.Options)
        {
            if (string.Equals(option.Value, phase, StringComparison.OrdinalIgnoreCase))
            {
                return localizer.T(option.Label);
            }
        }

        return localizer.T("{0} File", phase);
    }

    public static string ClientRole(IIndorLocalizer localizer, string? role) =>
        string.IsNullOrWhiteSpace(role) ? string.Empty : localizer.T(role);

    public static string ItemCountLabel(IIndorLocalizer localizer, int count) =>
        count == 1 ? localizer.T("1 item") : localizer.T("{0} items", count);

    public static string PropertyFileStepLabel(IIndorLocalizer localizer, int step) =>
        step switch
        {
            1 => localizer.T("Details"),
            2 => localizer.T("Add Items"),
            3 => localizer.T("Review"),
            4 => localizer.T("Create"),
            _ => localizer.T("Details")
        };

    public static string InviteClientStepLabel(IIndorLocalizer localizer, int step) =>
        step switch
        {
            1 => localizer.T("Client Info"),
            2 => localizer.T("Property"),
            3 => localizer.T("Access"),
            4 => localizer.T("Review"),
            _ => localizer.T("Client Info")
        };

    public static string CategoryShortLabel(IIndorLocalizer localizer, string fullLabel) =>
        fullLabel switch
        {
            var l when l.StartsWith("Photos", StringComparison.Ordinal) => localizer.T("Photos"),
            var l when l.StartsWith("Inspection", StringComparison.Ordinal) => localizer.T("Inspection"),
            var l when l.StartsWith("Repair", StringComparison.Ordinal) => localizer.T("Repair"),
            var l when l.StartsWith("Quotes", StringComparison.Ordinal) => localizer.T("Quotes"),
            var l when l.StartsWith("Warranties", StringComparison.Ordinal) => localizer.T("Warranties"),
            var l when l.StartsWith("Invoices", StringComparison.Ordinal) => localizer.T("Invoices"),
            var l when l.StartsWith("Manuals", StringComparison.Ordinal) => localizer.T("Manuals"),
            var l when l.StartsWith("Notes", StringComparison.Ordinal) => localizer.T("Notes"),
            _ => Localize(localizer, fullLabel)
        };

    public static string FileFilterLabel(IIndorLocalizer localizer, string filter) => localizer.T(filter);

    public static string NetworkFilterLabel(IIndorLocalizer localizer, string label) => localizer.T(label);

    public static string ProviderFilterLabel(IIndorLocalizer localizer, string filter) => localizer.T(filter);

    public static string UrgentQuoteStepLabel(IIndorLocalizer localizer, string step) => localizer.T(step);

    public static string DistanceMilesAway(IIndorLocalizer localizer, double miles) =>
        localizer.T("{0} mi away", miles.ToString("0.#", CultureInfo.InvariantCulture));

    public static string PropertySpecsSummary(
        IIndorLocalizer localizer,
        decimal? beds,
        decimal? baths,
        int? sqft,
        int? yearBuilt = null)
    {
        var parts = new List<string>();
        if (beds is > 0)
        {
            parts.Add(localizer.T("{0} Beds", beds.Value.ToString("0.#", CultureInfo.InvariantCulture)));
        }

        if (baths is > 0)
        {
            parts.Add(localizer.T("{0} Baths", baths.Value.ToString("0.#", CultureInfo.InvariantCulture)));
        }

        if (sqft is > 0)
        {
            parts.Add(localizer.T("{0} sqft", sqft.Value.ToString("N0", CultureInfo.InvariantCulture)));
        }

        if (yearBuilt is > 0)
        {
            parts.Add(localizer.T("Built {0}", yearBuilt.Value.ToString(CultureInfo.InvariantCulture)));
        }

        return parts.Count == 0 ? string.Empty : string.Join(" · ", parts);
    }

    public static string PropertySubtypeLabel(IIndorLocalizer localizer, string? subtype) =>
        subtype switch
        {
            "single-family" => localizer.T("Single Family"),
            "townhouse" => localizer.T("Townhouse"),
            "condo" => localizer.T("Condo"),
            "multi-family" => localizer.T("Multi-Family"),
            "land" => localizer.T("Land / Lot"),
            "other" => localizer.T("Other"),
            _ => string.Empty
        };

    public static string FileDetailNote(IIndorLocalizer localizer, string? detailNote)
    {
        if (string.IsNullOrWhiteSpace(detailNote))
        {
            return string.Empty;
        }

        if (detailNote.StartsWith("Repair items:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(detailNote["Repair items:".Length..].Trim(), out var repairCount))
        {
            return localizer.T("Repair items: {0}", repairCount);
        }

        if (detailNote.StartsWith("Quotes received:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(detailNote["Quotes received:".Length..].Trim(), out var quoteCount))
        {
            return localizer.T("Quotes received: {0}", quoteCount);
        }

        return localizer.T(detailNote);
    }
}
