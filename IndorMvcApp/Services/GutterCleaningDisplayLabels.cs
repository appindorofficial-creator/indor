namespace IndorMvcApp.Services;

public static class GutterCleaningDisplayLabels
{
    public static string FormatInitialAction(string? code) => code switch
    {
        "ScheduleService" => "Schedule service",
        "AlreadyDone" => "Already done",
        _ => "Reminder"
    };

    public static string FormatStories(string? code) => code switch
    {
        "One" => "1 story",
        "ThreePlus" => "3+ stories",
        _ => "2 stories"
    };

    public static string FormatGutterType(string? code) => code switch
    {
        "Vinyl" => "Vinyl",
        "Copper" => "Copper",
        "NotSure" => "Not sure",
        _ => "Aluminum"
    };

    public static string FormatYesNoNotSure(string? code) => code switch
    {
        "Yes" => "Yes",
        "No" => "No",
        _ => "Not sure"
    };

    public static string FormatLastCleaned(string? code) => code switch
    {
        "LessThan6Months" => "< 6 months",
        "SixToTwelveMonths" => "6–12 months",
        "OnePlusYear" => "1+ year",
        _ => "Not sure"
    };

    public static string FormatIssue(string code) => code switch
    {
        "LeavesDebris" => "Leaves / debris",
        "OverflowingWater" => "Overflowing water",
        "PlantsGrowing" => "Plants growing",
        "SaggingSections" => "Sagging sections",
        "DownspoutClogged" => "Downspout clogged",
        "NoVisibleIssues" => "No visible issues",
        _ => code
    };

    public static string FormatProblemArea(string? code) => code switch
    {
        "FrontOnly" => "Front only",
        "BackOnly" => "Back only",
        "OneSideOnly" => "One side only",
        "NotSure" => "Not sure",
        _ => "Whole house"
    };

    public static string FormatTodayGoal(string? code) => code switch
    {
        "ReminderOnly" => "Reminder only",
        "CleaningEstimate" => "Cleaning estimate",
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
