namespace IndorMvcApp.Services;

public static class TrashDisplayLabels
{
    public static string FormatBinsList(string? pipeValue)
    {
        var bins = TrashPricingService.ParseBins(pipeValue).Select(b => b.Label).ToList();
        return bins.Count == 0 ? "—" : string.Join(" + ", bins);
    }

    public static string FormatBinCount(string? value) => value switch
    {
        "One" => "1 bin",
        "Two" => "2 bins",
        "Three" => "3 bins",
        _ => value ?? "—"
    };

    public static string FormatFrequency(string? value) => value switch
    {
        "OneTime" => "One-time",
        "Weekly" => "Weekly",
        "ReminderOnly" => "Reminder only",
        _ => value ?? "—"
    };

    public static string FormatPickupDay(string? value) => value switch
    {
        "Sun" => "Sunday",
        "Mon" => "Monday",
        "Tue" => "Tuesday",
        "Wed" => "Wednesday",
        "Thu" => "Thursday",
        "Fri" => "Friday",
        "Sat" => "Saturday",
        _ => value ?? "—"
    };

    public static string FormatPickupDayShort(string? value) => value switch
    {
        "Sun" => "Sun",
        "Mon" => "Mon",
        "Tue" => "Tue",
        "Wed" => "Wed",
        "Thu" => "Thu",
        "Fri" => "Fri",
        "Sat" => "Sat",
        _ => value ?? "—"
    };

    public static string FormatHelpType(string? value) => value switch
    {
        "ReminderOnly" => "Reminder only",
        "TakeOut" => "Take bins out",
        "TakeOutReturn" => "Take out + bring back",
        _ => value ?? "—"
    };

    public static string FormatReminderWhen(string? value) => value switch
    {
        "OneDayBefore" => "1 day before",
        "EveningBefore" => "Evening before",
        "MorningOf" => "Morning of",
        _ => value ?? "—"
    };

    public static string FormatPickupWindow(string? value) => value switch
    {
        "Morning7_12" => "07:00 – 12:00",
        "Afternoon12_5" => "12:00 – 17:00",
        "Flexible" => "Flexible",
        _ => value ?? "07:00 – 12:00"
    };
}
