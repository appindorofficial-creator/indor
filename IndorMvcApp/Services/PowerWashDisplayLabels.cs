namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class PowerWashPricingService
{
    public const decimal StartingPrice = 149m;

    public static decimal GetEstimatedPrice() => StartingPrice;
}

public static class PowerWashDisplayLabels
{
    public static string FormatArea(string code) => code switch
    {
        "FrontOnly" => DisplayLabelsLocalization.L("Front only"),
        "BackOnly" => DisplayLabelsLocalization.L("Back only"),
        "Driveway" => DisplayLabelsLocalization.L("Driveway"),
        "PatioDeck" => DisplayLabelsLocalization.L("Patio / deck"),
        "Fence" => DisplayLabelsLocalization.L("Fence"),
        _ => "Full exterior"
    };

    public static string FormatMaterial(string? code) => code switch
    {
        "Brick" => DisplayLabelsLocalization.L("Brick"),
        "Stucco" => DisplayLabelsLocalization.L("Stucco"),
        "FiberCement" => DisplayLabelsLocalization.L("Fiber cement"),
        "PaintedWood" => DisplayLabelsLocalization.L("Painted wood"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => "Vinyl siding"
    };

    public static string FormatStories(string? code) => code switch
    {
        "One" => DisplayLabelsLocalization.L("1"),
        "ThreePlus" => DisplayLabelsLocalization.L("3+"),
        _ => "2"
    };

    public static string FormatIssue(string code) => code switch
    {
        "LightDirt" => DisplayLabelsLocalization.L("Light dirt"),
        "HeavyDirt" => DisplayLabelsLocalization.L("Heavy dirt"),
        "MoldAlgae" => DisplayLabelsLocalization.L("Mold / algae"),
        "Pollen" => DisplayLabelsLocalization.L("Pollen"),
        "RustStains" => DisplayLabelsLocalization.L("Rust stains"),
        _ => code
    };

    public static string FormatDelicateArea(string code) => code switch
    {
        "LoosePaint" => DisplayLabelsLocalization.L("Loose paint"),
        "CrackedSiding" => DisplayLabelsLocalization.L("Cracked siding"),
        "OldCaulking" => DisplayLabelsLocalization.L("Old caulking"),
        "None" => DisplayLabelsLocalization.L("None"),
        _ => code
    };

    public static string FormatYesNo(string? code) =>
        string.Equals(code, "Yes", StringComparison.OrdinalIgnoreCase)
            ? DisplayLabelsLocalization.L("Yes")
            : string.Equals(code, "No", StringComparison.OrdinalIgnoreCase)
                ? DisplayLabelsLocalization.L("No")
                : "—";

    public static string FormatTiming(string? code) => code switch
    {
        "NextWeek" => DisplayLabelsLocalization.L("Next week"),
        "ThisMonth" => DisplayLabelsLocalization.L("This month"),
        "Flexible" => DisplayLabelsLocalization.L("Flexible"),
        _ => string.IsNullOrWhiteSpace(code) ? "—" : DisplayLabelsLocalization.L(code)
    };

    public static string FormatTimeWindow(string? code) => code switch
    {
        "Morning" => DisplayLabelsLocalization.L("Morning"),
        "Midday" => DisplayLabelsLocalization.L("Midday"),
        "Afternoon" => DisplayLabelsLocalization.L("Afternoon"),
        _ => string.IsNullOrWhiteSpace(code) ? "—" : DisplayLabelsLocalization.L(code)
    };

    public static string FormatPreferredTime(string? timing, string? window) =>
        $"{FormatTiming(timing)} / {FormatTimeWindow(window)}";

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? DisplayLabelsLocalization.L("General wash")
            : string.Join(" + ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(formatter));

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
