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
        _ => value ?? "â€”"
    };

    public static string FormatCrew(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 cleaner"),
        "Two" => DisplayLabelsLocalization.L("2 cleaners"),
        "Three" => DisplayLabelsLocalization.L("3 cleaners"),
        _ => value ?? "â€”"
    };

    public static string FormatCrewShort(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 Cleaner"),
        "Two" => DisplayLabelsLocalization.L("2 Cleaners"),
        "Three" => DisplayLabelsLocalization.L("3 Cleaners"),
        _ => value ?? "â€”"
    };

    public static string FormatAreasList(string? pipeValue)
    {
        var areas = CleaningProPricingService.ParseAreas(pipeValue).Select(a => a.Label).ToList();
        return areas.Count == 0 ? "â€”" : string.Join(", ", areas);
    }

    public static string FormatHours(decimal? hours) =>
        hours.HasValue ? $"{hours.Value:0.#} hour{(hours.Value == 1 ? "" : "s")}" : "â€”";

    public static string FormatHoursShort(decimal? hours) =>
        hours.HasValue ? $"{hours.Value:0.#} hr" : "â€”";

    public static string FormatDateTime(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue) return DisplayLabelsLocalization.L("To be confirmed");

        var day = fecha.Value.ToString("ddd, MMM d, yyyy");
        var time = FormatTimeWindow(ventana);
        return string.IsNullOrWhiteSpace(time) || time == "â€”" ? day : $"{day} at {time}";
    }

    public static string FormatScheduledRange(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue) return DisplayLabelsLocalization.L("To be confirmed");

        var day = fecha.Value.ToString("ddd, MMM d, yyyy");
        var range = ventana switch
        {
            "Morning9_12" => DisplayLabelsLocalization.L("09:00 â€“ 12:00"),
            "Morning10" => DisplayLabelsLocalization.L("10:00"),
            _ => FormatTimeWindow(ventana)
        };

        return $"{day}\n{range}";
    }

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning10" => DisplayLabelsLocalization.L("10:00"),
        "Morning9_12" => DisplayLabelsLocalization.L("09:00 â€“ 12:00"),
        "Afternoon1_5" => DisplayLabelsLocalization.L("13:00 â€“ 17:00"),
        _ => value ?? "â€”"
    };

    public static string FormatSummaryLine(string? frequency, string? crew, decimal? hours, decimal fromTotal)
    {
        var parts = new List<string>
        {
            FormatFrequency(frequency),
            FormatCrew(crew),
            $"Est. {FormatHoursShort(hours)}",
            $"From ${fromTotal:0}"
        };

        return string.Join(" â€¢ ", parts.Where(p => p != "â€”"));
    }
}
