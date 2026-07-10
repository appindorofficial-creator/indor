using System.Globalization;
using IndorMvcApp.Localization;

namespace IndorMvcApp.Services;

public static class SafeAirDisplayLabels
{
    private static bool IsSpanish => UiCulture.IsSpanish(CultureInfo.CurrentUICulture.Name);

    private static string L(string english) => CatalogText.PickWithUiFallback(english, null, IsSpanish);

    public static string FormatNeedType(string? value) => value switch
    {
        "IndorReplaces" => L("INDOR replaces it"),
        "ChangedMyself" => L("I changed it myself"),
        "RemindOnly" => L("Just remind me"),
        _ => value ?? "—"
    };

    public static string FormatFilterCount(string? value) => value switch
    {
        "One" => "1",
        "Two" => "2",
        "ThreePlus" => "3+",
        _ => value ?? "—"
    };

    public static string FormatLocation(string? value) => value switch
    {
        "Ceiling" => L("Ceiling"),
        "WallReturn" => L("Wall return"),
        "HvacUnit" => L("HVAC unit"),
        "Attic" => L("Attic"),
        "NotSure" => L("Not sure"),
        _ => value ?? "—"
    };

    public static string FormatProvider(string? value) => value switch
    {
        "IndorBrings" => L("Yes"),
        "IHaveFilter" => L("No"),
        _ => value ?? "—"
    };

    public static string FormatProviderLong(string? value) => value switch
    {
        "IndorBrings" => L("INDOR brings it"),
        "IHaveFilter" => L("I have the filter"),
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "NextAvailable" => L("Next available"),
        "Morning" => L("Morning"),
        "Afternoon" => L("Afternoon"),
        "Flexible" => L("Flexible"),
        _ => value ?? "—"
    };

    public static string FormatVisitLabel(string? needType, string? timeWindow)
    {
        if (string.Equals(needType, "RemindOnly", StringComparison.OrdinalIgnoreCase)
            || string.Equals(needType, "ChangedMyself", StringComparison.OrdinalIgnoreCase))
        {
            return L("No visit scheduled");
        }

        return timeWindow switch
        {
            "NextAvailable" => L("Tomorrow, 10:00–12:00"),
            "Morning" => L("Tomorrow, 8:00–12:00"),
            "Afternoon" => L("Tomorrow, 12:00–4:00 PM"),
            "Flexible" => L("Flexible window"),
            _ => L("To be confirmed")
        };
    }

    public static string FormatAccess(string? value) => value switch
    {
        "House" => L("House"),
        "Apartment" => L("Apartment"),
        "AtticAccess" => L("Attic access"),
        "GateCode" => L("Gate code"),
        _ => value ?? "—"
    };

    public static string FormatFilterSize(decimal? width, decimal? height, decimal? depth, bool unknown)
    {
        if (unknown)
        {
            return L("To be verified from photo");
        }

        if (width.HasValue && height.HasValue && depth.HasValue)
        {
            return $"{FormatDimension(width)} x {FormatDimension(height)} x {FormatDimension(depth)}";
        }

        return L("I don't know");
    }

    public static string FormatFilterSizeSummary(decimal? width, decimal? height, decimal? depth, bool unknown)
    {
        var size = FormatFilterSize(width, height, depth, unknown);
        var unknownLabel = L("I don't know");
        return unknown || size == unknownLabel
            ? size
            : $"{size} {L("or")} {L("To be verified from photo")}";
    }

    public static string FormatReminder(bool active) =>
        active ? L("Every 3 months") : L("Off");

    private static string FormatDimension(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.#") : "—";
}
