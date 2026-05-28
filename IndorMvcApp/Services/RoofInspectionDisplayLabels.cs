namespace IndorMvcApp.Services;

public static class RoofInspectionPricingService
{
    public const decimal StartingPrice = 99m;

    public static decimal GetEstimatedPrice(string? tipoServicio) =>
        string.Equals(tipoServicio, "BookInspection", StringComparison.OrdinalIgnoreCase)
            ? StartingPrice
            : 0m;
}

public static class RoofInspectionDisplayLabels
{
    public static string FormatReason(string? code) => code switch
    {
        "AfterStorm" => "After a storm",
        "RoofLeakSigns" => "Roof leak signs",
        "MissingShingles" => "Missing shingles",
        "FlashingConcern" => "Flashing / sealant concern",
        _ => "Routine inspection"
    };

    public static string FormatFocus(string? code) => code switch
    {
        "AfterStorm" => "storm damage and shingles",
        "RoofLeakSigns" => "leak signs and flashing",
        "MissingShingles" => "shingles and sealant",
        "FlashingConcern" => "flashing, shingles, and sealant",
        _ => "flashing, shingles, and sealant"
    };

    public static string FormatRoofType(string? code) => code switch
    {
        "Metal" => "Metal",
        "Tile" => "Tile",
        "Flat" => "Flat",
        "NotSure" => "Not sure",
        _ => "Asphalt shingle"
    };

    public static string FormatRoofAge(string? code) => code switch
    {
        "ZeroToTen" => "0–10 yrs",
        "ElevenToTwenty" => "11–20 yrs",
        "TwentyPlus" => "20+ yrs",
        _ => "Not sure"
    };

    public static string FormatLastInspection(string? code) => code switch
    {
        "ThisYear" => "This year",
        "OneToTwoYears" => "1–2 years ago",
        "ThreePlusYears" => "3+ years ago",
        _ => "I don't know"
    };

    public static string FormatServiceType(string? code) =>
        string.Equals(code, "BookInspection", StringComparison.OrdinalIgnoreCase)
            ? "Professional inspection booked"
            : "Reminder only";

    public static string FormatFrequency(string? code) => code switch
    {
        "Every2Years" => "Every 2 years",
        "AfterStorms" => "After major storms",
        "Custom" => "Custom",
        _ => "Yearly"
    };

    public static string FormatTiming(string? code) => code switch
    {
        "Fall" => "Fall",
        "ThisMonth" => "This month",
        "Flexible" => "Flexible",
        _ => "Spring"
    };
}
