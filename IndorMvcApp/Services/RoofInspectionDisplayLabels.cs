namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

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
        "AfterStorm" => DisplayLabelsLocalization.L("After a storm"),
        "RoofLeakSigns" => DisplayLabelsLocalization.L("Roof leak signs"),
        "MissingShingles" => DisplayLabelsLocalization.L("Missing shingles"),
        "FlashingConcern" => DisplayLabelsLocalization.L("Flashing / sealant concern"),
        _ => "Routine inspection"
    };

    public static string FormatFocus(string? code) => code switch
    {
        "AfterStorm" => DisplayLabelsLocalization.L("storm damage and shingles"),
        "RoofLeakSigns" => DisplayLabelsLocalization.L("leak signs and flashing"),
        "MissingShingles" => DisplayLabelsLocalization.L("shingles and sealant"),
        "FlashingConcern" => DisplayLabelsLocalization.L("flashing, shingles, and sealant"),
        _ => "flashing, shingles, and sealant"
    };

    public static string FormatRoofType(string? code) => code switch
    {
        "Metal" => DisplayLabelsLocalization.L("Metal"),
        "Tile" => DisplayLabelsLocalization.L("Tile"),
        "Flat" => DisplayLabelsLocalization.L("Flat"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => "Asphalt shingle"
    };

    public static string FormatRoofAge(string? code) => code switch
    {
        "ZeroToTen" => DisplayLabelsLocalization.L("0-10 yrs"),
        "ElevenToTwenty" => DisplayLabelsLocalization.L("11-20 yrs"),
        "TwentyPlus" => DisplayLabelsLocalization.L("20+ yrs"),
        _ => "Not sure"
    };

    public static string FormatLastInspection(string? code) => code switch
    {
        "ThisYear" => DisplayLabelsLocalization.L("This year"),
        "OneToTwoYears" => DisplayLabelsLocalization.L("1-2 years ago"),
        "ThreePlusYears" => DisplayLabelsLocalization.L("3+ years ago"),
        _ => "I don't know"
    };

    public static string FormatServiceType(string? code) =>
        string.Equals(code, "BookInspection", StringComparison.OrdinalIgnoreCase)
            ? "Professional inspection booked"
            : "Reminder only";

    public static string FormatFrequency(string? code) => code switch
    {
        "Every2Years" => DisplayLabelsLocalization.L("Every 2 years"),
        "AfterStorms" => DisplayLabelsLocalization.L("After major storms"),
        "Custom" => DisplayLabelsLocalization.L("Custom"),
        _ => "Yearly"
    };

    public static string FormatTiming(string? code) => code switch
    {
        "Fall" => DisplayLabelsLocalization.L("Fall"),
        "ThisMonth" => DisplayLabelsLocalization.L("This month"),
        "Flexible" => DisplayLabelsLocalization.L("Flexible"),
        _ => "Spring"
    };
}
