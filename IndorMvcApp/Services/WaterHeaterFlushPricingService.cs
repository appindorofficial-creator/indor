namespace IndorMvcApp.Services;

public static class WaterHeaterFlushPricingService
{
    public const decimal StartingPrice = 79m;

    public static decimal GetEstimatedPrice() => StartingPrice;
}

public static class WaterHeaterFlushDisplayLabels
{
    public static string FormatHeaterType(string? code) => code switch
    {
        "Tankless" => "Tankless",
        _ => "Tank"
    };

    public static string FormatPowerSource(string? code) => code switch
    {
        "Gas" => "Gas",
        _ => "Electric"
    };

    public static string FormatLocation(string? code) => code switch
    {
        "Basement" => "Basement",
        "Closet" => "Closet",
        "Attic" => "Attic",
        "Other" => "Other",
        _ => "Garage"
    };

    public static string FormatSerial(string? serial, bool desconocido) =>
        desconocido || string.IsNullOrWhiteSpace(serial) ? "Not provided" : serial.Trim();

    public static string FormatLastFlush(string? code) => code switch
    {
        "Within1Year" => "Within 1 year",
        "OneToTwoYears" => "1–2 years ago",
        "MoreThan2Years" => "More than 2 years",
        _ => "Not sure"
    };

    public static string FormatSymptom(string code) => code switch
    {
        "RumblingNoise" => "Rumbling noise",
        "RustyWater" => "Rusty / cloudy water",
        "SlowHotWater" => "Slow hot water",
        "TempChanges" => "Temperature changes",
        "NoIssues" => "No issues — just maintenance",
        _ => code
    };

    public static string FormatSymptomsList(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe)
            ? "No issues — just maintenance"
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatSymptom));

    public static string FormatServiceType(string? code, bool recordatorioAnual) =>
        recordatorioAnual || string.Equals(code, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? "Yearly reminder set"
            : "One-time flush";

    public static string FormatPreferredTime(string? code, DateTime? date) => code switch
    {
        "ThisWeek" => "This week",
        "ChooseDate" when date.HasValue => date.Value.ToString("MMM d, yyyy"),
        "ChooseDate" => "Choose date",
        _ => "Next available"
    };

    public static string FormatPrice(decimal amount) => $"from ${amount:0}";
}
