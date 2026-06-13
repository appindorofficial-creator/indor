namespace IndorMvcApp.Services;

public static class CleaningProDisplayLabels
{
    public static string FormatFrequency(string? value) => value switch
    {
        "OneTime" => "One-time",
        "Weekly" => "Weekly",
        "Biweekly" => "Biweekly",
        "Monthly" => "Monthly",
        _ => value ?? "—"
    };

    public static string FormatCrew(string? value) => value switch
    {
        "One" => "1 cleaner",
        "Two" => "2 cleaners",
        "Three" => "3 cleaners",
        _ => value ?? "—"
    };

    public static string FormatCrewShort(string? value) => value switch
    {
        "One" => "1 Cleaner",
        "Two" => "2 Cleaners",
        "Three" => "3 Cleaners",
        _ => value ?? "—"
    };

    public static string FormatAreasList(string? pipeValue)
    {
        var areas = CleaningProPricingService.ParseAreas(pipeValue).Select(a => a.Label).ToList();
        return areas.Count == 0 ? "—" : string.Join(", ", areas);
    }

    public static string FormatHours(decimal? hours) =>
        hours.HasValue ? $"{hours.Value:0.#} hour{(hours.Value == 1 ? "" : "s")}" : "—";

    public static string FormatHoursShort(decimal? hours) =>
        hours.HasValue ? $"{hours.Value:0.#} hr" : "—";

    public static string FormatDateTime(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue) return "To be confirmed";

        var day = fecha.Value.ToString("ddd, MMM d, yyyy");
        var time = FormatTimeWindow(ventana);
        return string.IsNullOrWhiteSpace(time) || time == "—" ? day : $"{day} at {time}";
    }

    public static string FormatScheduledRange(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue) return "To be confirmed";

        var day = fecha.Value.ToString("ddd, MMM d, yyyy");
        var range = ventana switch
        {
            "Morning9_12" => "09:00 – 12:00",
            "Morning10" => "10:00",
            _ => FormatTimeWindow(ventana)
        };

        return $"{day}\n{range}";
    }

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning10" => "10:00",
        "Morning9_12" => "09:00 – 12:00",
        "Afternoon1_5" => "13:00 – 17:00",
        _ => value ?? "—"
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

        return string.Join(" • ", parts.Where(p => p != "—"));
    }
}
