namespace IndorMvcApp.Services;

public static class PowerWashPricingService
{
    public const decimal StartingPrice = 149m;

    public static decimal GetEstimatedPrice() => StartingPrice;
}

public static class PowerWashDisplayLabels
{
    public static string FormatArea(string code) => code switch
    {
        "FrontOnly" => "Front only",
        "BackOnly" => "Back only",
        "Driveway" => "Driveway",
        "PatioDeck" => "Patio / deck",
        "Fence" => "Fence",
        _ => "Full exterior"
    };

    public static string FormatMaterial(string? code) => code switch
    {
        "Brick" => "Brick",
        "Stucco" => "Stucco",
        "FiberCement" => "Fiber cement",
        "PaintedWood" => "Painted wood",
        "NotSure" => "Not sure",
        _ => "Vinyl siding"
    };

    public static string FormatStories(string? code) => code switch
    {
        "One" => "1",
        "ThreePlus" => "3+",
        _ => "2"
    };

    public static string FormatIssue(string code) => code switch
    {
        "LightDirt" => "Light dirt",
        "HeavyDirt" => "Heavy dirt",
        "MoldAlgae" => "Mold / algae",
        "Pollen" => "Pollen",
        "RustStains" => "Rust stains",
        _ => code
    };

    public static string FormatDelicateArea(string code) => code switch
    {
        "LoosePaint" => "Loose paint",
        "CrackedSiding" => "Cracked siding",
        "OldCaulking" => "Old caulking",
        "None" => "None",
        _ => code
    };

    public static string FormatYesNo(string? code) =>
        string.Equals(code, "Yes", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No";

    public static string FormatTiming(string? code) => code switch
    {
        "ThisMonth" => "This month",
        "Flexible" => "Flexible",
        _ => "Next week"
    };

    public static string FormatTimeWindow(string? code) => code switch
    {
        "Midday" => "Midday",
        "Afternoon" => "Afternoon",
        _ => "Morning"
    };

    public static string FormatPreferredTime(string? timing, string? window) =>
        $"{FormatTiming(timing)} / {FormatTimeWindow(window)}";

    public static string FormatPipeList(string? pipe, Func<string, string> formatter) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "General wash"
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
