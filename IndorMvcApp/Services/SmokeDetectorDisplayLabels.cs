namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class SmokeDetectorDisplayLabels
{
    public static string FormatAlarmCount(string? code) => code switch
    {
        "One" => DisplayLabelsLocalization.L("1 alarm"),
        "Two" => DisplayLabelsLocalization.L("2 alarms"),
        "Three" => DisplayLabelsLocalization.L("3 alarms"),
        "Four" => DisplayLabelsLocalization.L("4 alarms"),
        "FivePlus" => DisplayLabelsLocalization.L("5+ alarms"),
        _ => DisplayLabelsLocalization.L("Unknown count")
    };

    public static string FormatAlarmCountShort(string? code) => code switch
    {
        "One" => DisplayLabelsLocalization.L("1"),
        "Two" => DisplayLabelsLocalization.L("2"),
        "Three" => DisplayLabelsLocalization.L("3"),
        "Four" => DisplayLabelsLocalization.L("4"),
        "FivePlus" => DisplayLabelsLocalization.L("5+"),
        _ => DisplayLabelsLocalization.L("?")
    };

    public static string FormatLocation(string code) => code switch
    {
        "Bedrooms" => DisplayLabelsLocalization.L("Bedrooms"),
        "Hallway" => DisplayLabelsLocalization.L("Hallway"),
        "LivingRoom" => DisplayLabelsLocalization.L("Living room"),
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "UpstairsLanding" => DisplayLabelsLocalization.L("Upstairs landing"),
        "DontKnow" => DisplayLabelsLocalization.L("Unknown location"),
        _ => code
    };

    public static string FormatAlarmType(string code) => code switch
    {
        "Battery" => DisplayLabelsLocalization.L("Battery"),
        "Hardwired" => DisplayLabelsLocalization.L("Hardwired"),
        "TenYearSealed" => DisplayLabelsLocalization.L("10-year sealed"),
        "SmokeCoCombo" => DisplayLabelsLocalization.L("Smoke / CO combo"),
        "DontKnow" => DisplayLabelsLocalization.L("Unknown type"),
        _ => code
    };

    public static string FormatPrimaryAlarmType(string? pipe)
    {
        if (string.IsNullOrWhiteSpace(pipe)) return DisplayLabelsLocalization.L("Smoke alarm");
        var items = pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (items.Contains("SmokeCoCombo")) return DisplayLabelsLocalization.L("Smoke / CO combo");
        if (items.Length == 1) return FormatAlarmType(items[0]);
        return string.Join(", ", items.Select(FormatAlarmType));
    }

    public static string FormatLastTest(string? code) => code switch
    {
        "WithinLastMonth" => DisplayLabelsLocalization.L("Within last month"),
        "OneToSixMonths" => DisplayLabelsLocalization.L("1-6 months ago"),
        "MoreThanSixMonths" => DisplayLabelsLocalization.L("More than 6 months ago"),
        _ => DisplayLabelsLocalization.L("Unknown")
    };

    public static string FormatLastBatteryChange(string? code) => code switch
    {
        "WithinLast6Months" => DisplayLabelsLocalization.L("Within last 6 months"),
        "SixToTwelveMonths" => DisplayLabelsLocalization.L("6-12 months ago"),
        "MoreThan12Months" => DisplayLabelsLocalization.L("More than 12 months ago"),
        _ => DisplayLabelsLocalization.L("Unknown")
    };

    public static string FormatIssue(string code) => code switch
    {
        "Chirping" => DisplayLabelsLocalization.L("Chirping"),
        "MissingBattery" => DisplayLabelsLocalization.L("Missing battery"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        "AllWorking" => DisplayLabelsLocalization.L("All working"),
        "DontKnow" => DisplayLabelsLocalization.L("Unknown"),
        _ => code
    };

    public static string FormatHelpAction(string? code) =>
        string.Equals(code, "ScheduleSafetyCheck", StringComparison.OrdinalIgnoreCase)
            ? "Schedule safety check"
            : "Reminder only";

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General areas"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(v => !string.Equals(v, "DontKnow", StringComparison.OrdinalIgnoreCase))
                .Select(formatter));

    public static string FormatInstalledDate(int? year, bool unknown, DateTime? referenceDate)
    {
        if (unknown || !year.HasValue)
        {
            return referenceDate?.ToString("MMM d, yyyy") ?? "Unknown";
        }

        return new DateTime(year.Value, referenceDate?.Month ?? 5, referenceDate?.Day ?? 20).ToString("MMM d, yyyy");
    }

    public static DateTime ResolveInstallReferenceDate(int? year, bool unknown, DateTime confirmDate)
    {
        if (!unknown && year.HasValue)
        {
            return new DateTime(year.Value, confirmDate.Month, Math.Min(confirmDate.Day, DateTime.DaysInMonth(year.Value, confirmDate.Month)));
        }

        return confirmDate.Date;
    }

    public static DateTime GetNextMonthlyTest(DateTime reference, DateTime fromDate) =>
        reference.AddMonths(1) > fromDate ? reference.AddMonths(1) : fromDate.AddMonths(1);

    public static DateTime GetNextBatteryReminder(DateTime reference) => reference.AddYears(1);

    public static DateTime GetReplacementDate(DateTime reference) => reference.AddYears(10);

    public static DateTime GetNextSeasonalReview(DateTime fromDate)
    {
        var autumn = new DateTime(fromDate.Year, 9, 20);
        return fromDate <= autumn ? autumn : new DateTime(fromDate.Year + 1, 9, 20);
    }

    public static string FormatDate(DateTime date) => date.ToString("MMM d, yyyy");
}
