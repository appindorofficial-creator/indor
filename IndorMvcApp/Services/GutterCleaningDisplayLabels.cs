namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class GutterCleaningDisplayLabels
{
    public static string FormatInitialAction(string? code) => code switch
    {
        "ScheduleService" => DisplayLabelsLocalization.L("Schedule service"),
        "AlreadyDone" => DisplayLabelsLocalization.L("Already done"),
        _ => "Reminder"
    };

    public static string FormatStories(string? code) => code switch
    {
        "One" => DisplayLabelsLocalization.L("1 story"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+ stories"),
        _ => "2 stories"
    };

    public static string FormatGutterType(string? code) => code switch
    {
        "Vinyl" => DisplayLabelsLocalization.L("Vinyl"),
        "Copper" => DisplayLabelsLocalization.L("Copper"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => "Aluminum"
    };

    public static string FormatYesNoNotSure(string? code) => code switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        _ => "Not sure"
    };

    public static string FormatLastCleaned(string? code) => code switch
    {
        "LessThan6Months" => DisplayLabelsLocalization.L("< 6 months"),
        "SixToTwelveMonths" => DisplayLabelsLocalization.L("6â€“12 months"),
        "OnePlusYear" => DisplayLabelsLocalization.L("1+ year"),
        _ => "Not sure"
    };

    public static string FormatIssue(string code) => code switch
    {
        "LeavesDebris" => DisplayLabelsLocalization.L("Leaves / debris"),
        "OverflowingWater" => DisplayLabelsLocalization.L("Overflowing water"),
        "PlantsGrowing" => DisplayLabelsLocalization.L("Plants growing"),
        "SaggingSections" => DisplayLabelsLocalization.L("Sagging sections"),
        "DownspoutClogged" => DisplayLabelsLocalization.L("Downspout clogged"),
        "NoVisibleIssues" => DisplayLabelsLocalization.L("No visible issues"),
        _ => code
    };

    public static string FormatProblemArea(string? code) => code switch
    {
        "FrontOnly" => DisplayLabelsLocalization.L("Front only"),
        "BackOnly" => DisplayLabelsLocalization.L("Back only"),
        "OneSideOnly" => DisplayLabelsLocalization.L("One side only"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => "Whole house"
    };

    public static string FormatTodayGoal(string? code) => code switch
    {
        "ReminderOnly" => DisplayLabelsLocalization.L("Reminder only"),
        "CleaningEstimate" => DisplayLabelsLocalization.L("Cleaning estimate"),
        _ => "Schedule service"
    };

    public static string FormatReminderPreference(string? code, bool springFall) =>
        springFall || string.Equals(code, "SpringFall", StringComparison.OrdinalIgnoreCase)
            ? "Spring & Fall"
            : "Custom date";

    public static string FormatFrequency(bool springFall) =>
        springFall ? "Twice a year" : "Custom";

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General check"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(formatter));

    public static DateTime GetNextSpringFallReminderDate(DateTime reference)
    {
        var spring = new DateTime(reference.Year, 4, 1);
        var fall = new DateTime(reference.Year, 9, 1);

        if (reference <= spring) return spring;
        if (reference <= fall) return fall;
        return new DateTime(reference.Year + 1, 4, 1);
    }

    public static DateTime GetDefaultVisitDate()
    {
        var date = DateTime.Today.AddDays(7);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }
}
