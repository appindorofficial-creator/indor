namespace IndorMvcApp.Services;

public static class SafeAirDisplayLabels
{
    public static string FormatNeedType(string? value) => value switch
    {
        "IndorReplaces" => "INDOR replaces it",
        "ChangedMyself" => "I changed it myself",
        "RemindOnly" => "Just remind me",
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
        "Ceiling" => "Ceiling",
        "WallReturn" => "Wall return",
        "HvacUnit" => "HVAC unit",
        "Attic" => "Attic",
        "NotSure" => "Not sure",
        _ => value ?? "—"
    };

    public static string FormatProvider(string? value) => value switch
    {
        "IndorBrings" => "Yes",
        "IHaveFilter" => "No",
        _ => value ?? "—"
    };

    public static string FormatProviderLong(string? value) => value switch
    {
        "IndorBrings" => "INDOR brings it",
        "IHaveFilter" => "I have the filter",
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "NextAvailable" => "Next available",
        "Morning" => "Morning",
        "Afternoon" => "Afternoon",
        "Flexible" => "Flexible",
        _ => value ?? "—"
    };

    public static string FormatVisitLabel(string? needType, string? timeWindow)
    {
        if (string.Equals(needType, "RemindOnly", StringComparison.OrdinalIgnoreCase)
            || string.Equals(needType, "ChangedMyself", StringComparison.OrdinalIgnoreCase))
        {
            return "No visit scheduled";
        }

        return timeWindow switch
        {
            "NextAvailable" => "Tomorrow, 10:00–12:00",
            "Morning" => "Tomorrow, 8:00–12:00",
            "Afternoon" => "Tomorrow, 12:00–4:00 PM",
            "Flexible" => "Flexible window",
            _ => "To be confirmed"
        };
    }

    public static string FormatAccess(string? value) => value switch
    {
        "House" => "House",
        "Apartment" => "Apartment",
        "AtticAccess" => "Attic access",
        "GateCode" => "Gate code",
        _ => value ?? "—"
    };

    public static string FormatFilterSize(decimal? width, decimal? height, decimal? depth, bool unknown)
    {
        if (unknown)
        {
            return "To be verified from photo";
        }

        if (width.HasValue && height.HasValue && depth.HasValue)
        {
            return $"{FormatDimension(width)} x {FormatDimension(height)} x {FormatDimension(depth)}";
        }

        return "I don't know";
    }

    public static string FormatFilterSizeSummary(decimal? width, decimal? height, decimal? depth, bool unknown)
    {
        var size = FormatFilterSize(width, height, depth, unknown);
        return unknown || size == "I don't know"
            ? size
            : $"{size} or To be verified from photo";
    }

    public static string FormatReminder(bool active) =>
        active ? "Every 3 months" : "Off";

    private static string FormatDimension(decimal? value) =>
        value.HasValue ? value.Value.ToString("0.#") : "—";
}
