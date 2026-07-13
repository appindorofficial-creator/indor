namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class CleaningProDisplayLabels
{
    public static string FormatFrequency(string? value) => value switch
    {
        "OneTime" => DisplayLabelsLocalization.L("One-time"),
        "Weekly" => DisplayLabelsLocalization.L("Weekly"),
        "Biweekly" => DisplayLabelsLocalization.L("Biweekly"),
        "Monthly" => DisplayLabelsLocalization.L("Monthly"),
        _ => value ?? "—"
    };

    public static string FormatCrew(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 cleaner"),
        "Two" => DisplayLabelsLocalization.L("2 cleaners"),
        "Three" => DisplayLabelsLocalization.L("3 cleaners"),
        _ => value ?? "—"
    };

    public static string FormatCrewShort(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 Cleaner"),
        "Two" => DisplayLabelsLocalization.L("2 Cleaners"),
        "Three" => DisplayLabelsLocalization.L("3 Cleaners"),
        _ => value ?? "—"
    };

    public static string FormatAreasList(string? pipeValue)
    {
        var areas = CleaningProPricingService.ParseAreas(pipeValue)
            .Select(a => DisplayLabelsLocalization.L(a.Label))
            .ToList();
        return areas.Count == 0 ? "—" : string.Join(", ", areas);
    }

    public static string FormatHours(decimal? hours)
    {
        if (!hours.HasValue || hours.Value <= 0)
        {
            return "—";
        }

        var value = hours.Value.ToString("0.#");
        return hours.Value == 1
            ? $"{value} {DisplayLabelsLocalization.L("hour")}"
            : $"{value} {DisplayLabelsLocalization.L("hours")}";
    }

    public static string FormatHoursShort(decimal? hours) =>
        hours.HasValue && hours.Value > 0
            ? $"{hours.Value:0.#} {DisplayLabelsLocalization.L("hr")}"
            : "—";

    public static string FormatDateTime(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue) return DisplayLabelsLocalization.L("To be confirmed");

        var day = fecha.Value.ToString("ddd, MMM d, yyyy");
        var time = FormatTimeWindow(ventana);
        return string.IsNullOrWhiteSpace(time) || time == "—"
            ? day
            : $"{day} {DisplayLabelsLocalization.L("at")} {time}";
    }

    public static string FormatScheduledRange(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue) return DisplayLabelsLocalization.L("To be confirmed");

        var day = fecha.Value.ToString("ddd, MMM d, yyyy");
        var range = ventana switch
        {
            "Morning9_12" => DisplayLabelsLocalization.L("09:00 – 12:00"),
            "Morning10" => DisplayLabelsLocalization.L("10:00"),
            _ => FormatTimeWindow(ventana)
        };

        return $"{day}\n{range}";
    }

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning10" => DisplayLabelsLocalization.L("10:00"),
        "Morning9_12" => DisplayLabelsLocalization.L("09:00 – 12:00"),
        "Afternoon1_5" => DisplayLabelsLocalization.L("13:00 – 17:00"),
        _ => value ?? "—"
    };

    public static string FormatSummaryLine(string? frequency, string? crew, decimal? hours, decimal fromTotal)
    {
        var parts = new List<string>
        {
            FormatFrequency(frequency),
            FormatCrew(crew),
            $"{DisplayLabelsLocalization.L("Est.")} {FormatHoursShort(hours)}",
            $"{DisplayLabelsLocalization.L("From")} ${fromTotal:0}"
        };

        return string.Join(" • ", parts.Where(p => p != "—"));
    }
}
