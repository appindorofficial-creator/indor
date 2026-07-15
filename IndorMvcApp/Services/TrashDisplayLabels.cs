namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class TrashDisplayLabels
{
    public static string FormatBinsList(string? pipeValue)
    {
        var bins = TrashPricingService.ParseBins(pipeValue).Select(b => b.Label).ToList();
        return bins.Count == 0 ? "—" : string.Join(" + ", bins);
    }

    public static string FormatBinCount(string? value) => value switch
    {
        "One" => DisplayLabelsLocalization.L("1 bin"),
        "Two" => DisplayLabelsLocalization.L("2 bins"),
        "Three" => DisplayLabelsLocalization.L("3 bins"),
        _ => value ?? "—"
    };

    public static string FormatFrequency(string? value) => value switch
    {
        "OneTime" => DisplayLabelsLocalization.L("One-time"),
        "Weekly" => DisplayLabelsLocalization.L("Weekly"),
        "ReminderOnly" => DisplayLabelsLocalization.L("Reminder only"),
        _ => value ?? "—"
    };

    public static string FormatPickupDay(string? value) => value switch
    {
        "Sun" => DisplayLabelsLocalization.L("Sunday"),
        "Mon" => DisplayLabelsLocalization.L("Monday"),
        "Tue" => DisplayLabelsLocalization.L("Tuesday"),
        "Wed" => DisplayLabelsLocalization.L("Wednesday"),
        "Thu" => DisplayLabelsLocalization.L("Thursday"),
        "Fri" => DisplayLabelsLocalization.L("Friday"),
        "Sat" => DisplayLabelsLocalization.L("Saturday"),
        _ => value ?? "—"
    };

    public static string FormatPickupDayShort(string? value) => value switch
    {
        "Sun" => DisplayLabelsLocalization.L("Sun"),
        "Mon" => DisplayLabelsLocalization.L("Mon"),
        "Tue" => DisplayLabelsLocalization.L("Tue"),
        "Wed" => DisplayLabelsLocalization.L("Wed"),
        "Thu" => DisplayLabelsLocalization.L("Thu"),
        "Fri" => DisplayLabelsLocalization.L("Fri"),
        "Sat" => DisplayLabelsLocalization.L("Sat"),
        _ => value ?? "—"
    };

    public static string FormatHelpType(string? value) => value switch
    {
        "ReminderOnly" => DisplayLabelsLocalization.L("Reminder only"),
        "TakeOut" => DisplayLabelsLocalization.L("Take bins out"),
        "TakeOutReturn" => DisplayLabelsLocalization.L("Take out + bring back"),
        _ => value ?? "—"
    };

    public static string FormatReminderWhen(string? value) => value switch
    {
        "OneDayBefore" => DisplayLabelsLocalization.L("1 day before"),
        "EveningBefore" => DisplayLabelsLocalization.L("Evening before"),
        "MorningOf" => DisplayLabelsLocalization.L("Morning of"),
        _ => value ?? "—"
    };

    public static string FormatPickupWindow(string? value) => value switch
    {
        "Morning7_12" => DisplayLabelsLocalization.L("07:00 – 12:00"),
        "Afternoon12_5" => DisplayLabelsLocalization.L("12:00 – 17:00"),
        "Flexible" => DisplayLabelsLocalization.L("Flexible"),
        _ => value ?? "07:00 – 12:00"
    };
}
