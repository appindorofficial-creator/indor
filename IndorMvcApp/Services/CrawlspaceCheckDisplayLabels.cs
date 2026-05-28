namespace IndorMvcApp.Services;

public static class CrawlspaceCheckPricingService
{
    public const decimal StartingPrice = 89m;

    public static decimal GetEstimatedPrice() => StartingPrice;
}

public static class CrawlspaceCheckDisplayLabels
{
    public static string FormatYesNoNotSure(string? code) => code switch
    {
        "Yes" => "Yes",
        "No" => "No",
        _ => "Not sure"
    };

    public static string FormatAccessType(string? code) => code switch
    {
        "ExteriorDoor" => "Exterior door",
        "NotSure" => "Not sure",
        _ => "Interior hatch"
    };

    public static string FormatLastCheck(string? code) => code switch
    {
        "Within1Year" => "Within 1 year",
        "OneToTwoYears" => "1–2 years",
        "TwoPlusYears" => "2+ years",
        _ => "Not sure"
    };

    public static string FormatConcern(string code) => code switch
    {
        "StandingWater" => "Standing water",
        "MustyOdor" => "Musty odor",
        "MoldMildew" => "Mold / mildew",
        "AirLeaks" => "Air leaks",
        "PestSigns" => "Pest signs",
        "Cracks" => "Cracks",
        "PipeLeaks" => "Pipe leaks",
        "DamagedDucts" => "Damaged ducts",
        _ => code
    };

    public static string FormatConcernsList(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General inspection"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatConcern));

    public static string FormatTiming(string? code, bool recordatorioAnual) =>
        recordatorioAnual || string.Equals(code, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? "Yearly reminder"
            : code switch
            {
                "ThisMonth" => "This month",
                _ => "As soon as possible"
            };

    public static string FormatReminder(string? timing, bool recordatorioAnual, DateTime? fecha) =>
        recordatorioAnual || string.Equals(timing, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? fecha.HasValue
                ? $"Yearly follow-up on {fecha.Value:MMM d, yyyy}"
                : "Yearly follow-up on"
            : "No yearly reminder";
}
