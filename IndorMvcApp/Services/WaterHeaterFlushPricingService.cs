using System.Globalization;
using IndorMvcApp.Localization;

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
        "Tankless" => DisplayLabelsLocalization.L("Tankless"),
        _ => DisplayLabelsLocalization.L("Tank")
    };

    public static string FormatPowerSource(string? code) => code switch
    {
        "Gas" => DisplayLabelsLocalization.L("Gas"),
        _ => DisplayLabelsLocalization.L("Electric")
    };

    public static string FormatLocation(string? code) => code switch
    {
        "Basement" => DisplayLabelsLocalization.L("Basement"),
        "Closet" => DisplayLabelsLocalization.L("Closet"),
        "Attic" => DisplayLabelsLocalization.L("Attic"),
        "Other" => DisplayLabelsLocalization.L("Other"),
        _ => DisplayLabelsLocalization.L("Garage")
    };

    public static string FormatSerial(string? serial, bool desconocido) =>
        desconocido || string.IsNullOrWhiteSpace(serial)
            ? DisplayLabelsLocalization.L("Not provided")
            : serial.Trim();

    public static string FormatLastFlush(string? code) => code switch
    {
        "Within1Year" => DisplayLabelsLocalization.L("Within 1 year"),
        "OneToTwoYears" => DisplayLabelsLocalization.L("1–2 years ago"),
        "MoreThan2Years" => DisplayLabelsLocalization.L("More than 2 years"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string FormatSymptom(string code) => code switch
    {
        "RumblingNoise" => DisplayLabelsLocalization.L("Rumbling noise"),
        "RustyWater" => DisplayLabelsLocalization.L("Rusty / cloudy water"),
        "SlowHotWater" => DisplayLabelsLocalization.L("Slow hot water"),
        "TempChanges" => DisplayLabelsLocalization.L("Temperature changes"),
        "NoIssues" => DisplayLabelsLocalization.L("No issues — just maintenance"),
        _ => DisplayLabelsLocalization.L(code)
    };

    public static string FormatSymptomsList(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe)
            ? DisplayLabelsLocalization.L("No issues — just maintenance")
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatSymptom));

    public static string FormatServiceType(string? code, bool recordatorioAnual) =>
        recordatorioAnual || string.Equals(code, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? DisplayLabelsLocalization.L("Yearly reminder set")
            : DisplayLabelsLocalization.L("One-time flush");

    public static string FormatPreferredTime(string? code, DateTime? date) => code switch
    {
        "ThisWeek" => DisplayLabelsLocalization.L("This week"),
        "ChooseDate" when date.HasValue => date.Value.ToString(
            DisplayLabelsLocalization.IsSpanishUi ? "d MMM yyyy" : "MMM d, yyyy",
            CultureInfo.CurrentUICulture),
        "ChooseDate" => DisplayLabelsLocalization.L("Choose date"),
        _ => DisplayLabelsLocalization.L("Next available")
    };

    public static string FormatPrice(decimal amount) =>
        string.Format(
            CultureInfo.CurrentCulture,
            DisplayLabelsLocalization.L("from ${0}"),
            amount.ToString("0", CultureInfo.InvariantCulture));
}
