namespace IndorMvcApp.Services;

public static class SmokeDetectorDisplayLabels
{
    public static string FormatAlarmCount(string? code) => code switch
    {
        "One" => "1 alarm",
        "Two" => "2 alarms",
        "Three" => "3 alarms",
        "Four" => "4 alarms",
        "FivePlus" => "5+ alarms",
        _ => "Unknown count"
    };

    public static string FormatAlarmCountShort(string? code) => code switch
    {
        "One" => "1",
        "Two" => "2",
        "Three" => "3",
        "Four" => "4",
        "FivePlus" => "5+",
        _ => "?"
    };

    public static string FormatLocation(string code) => code switch
    {
        "Bedrooms" => "Bedrooms",
        "Hallway" => "Hallway",
        "LivingRoom" => "Living room",
        "Basement" => "Basement",
        "UpstairsLanding" => "Upstairs landing",
        "DontKnow" => "Unknown location",
        _ => code
    };

    public static string FormatAlarmType(string code) => code switch
    {
        "Battery" => "Battery",
        "Hardwired" => "Hardwired",
        "TenYearSealed" => "10-year sealed",
        "SmokeCoCombo" => "Smoke / CO combo",
        "DontKnow" => "Unknown type",
        _ => code
    };

    public static string FormatPrimaryAlarmType(string? pipe)
    {
        if (string.IsNullOrWhiteSpace(pipe)) return "Smoke alarm";
        var items = pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (items.Contains("SmokeCoCombo")) return "Smoke / CO combo";
        if (items.Length == 1) return FormatAlarmType(items[0]);
        return string.Join(", ", items.Select(FormatAlarmType));
    }

    public static string FormatLastTest(string? code) => code switch
    {
        "WithinLastMonth" => "Within last month",
        "OneToSixMonths" => "1–6 months ago",
        "MoreThanSixMonths" => "More than 6 months ago",
        _ => "Unknown"
    };

    public static string FormatLastBatteryChange(string? code) => code switch
    {
        "WithinLast6Months" => "Within last 6 months",
        "SixToTwelveMonths" => "6–12 months ago",
        "MoreThan12Months" => "More than 12 months ago",
        _ => "Unknown"
    };

    public static string FormatIssue(string code) => code switch
    {
        "Chirping" => "Chirping",
        "MissingBattery" => "Missing battery",
        "NotSure" => "Not sure",
        "AllWorking" => "All working",
        "DontKnow" => "Unknown",
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
