using System.Globalization;

namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class CrawlspaceCheckPricingService
{
    public const decimal StartingPrice = 89m;

    public static decimal GetEstimatedPrice() => StartingPrice;
}

public static class CrawlspaceCheckDisplayLabels
{
    public static string FormatYesNoNotSure(string? code) => code switch
    {
        "Yes" => DisplayLabelsLocalization.L("Yes"),
        "No" => DisplayLabelsLocalization.L("No"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string FormatAccessType(string? code) => code switch
    {
        "ExteriorDoor" => DisplayLabelsLocalization.L("Exterior door"),
        "NotSure" => DisplayLabelsLocalization.L("Not sure"),
        _ => DisplayLabelsLocalization.L("Interior hatch")
    };

    public static string FormatLastCheck(string? code) => code switch
    {
        "Within1Year" => DisplayLabelsLocalization.L("Within 1 year"),
        "OneToTwoYears" => DisplayLabelsLocalization.L("1–2 years"),
        "TwoPlusYears" => DisplayLabelsLocalization.L("2+ years"),
        _ => DisplayLabelsLocalization.L("Not sure")
    };

    public static string FormatConcern(string code) => code switch
    {
        "Moisture" => DisplayLabelsLocalization.L("Moisture"),
        "Encapsulation" => DisplayLabelsLocalization.L("Encapsulation"),
        "Insulation" => DisplayLabelsLocalization.L("Insulation"),
        "Pests" => DisplayLabelsLocalization.L("Pests"),
        "StandingWater" => DisplayLabelsLocalization.L("Standing water"),
        "MustyOdor" => DisplayLabelsLocalization.L("Musty odor"),
        "MoldMildew" => DisplayLabelsLocalization.L("Mold / mildew"),
        "AirLeaks" => DisplayLabelsLocalization.L("Air leaks"),
        "PestSigns" => DisplayLabelsLocalization.L("Pest signs"),
        "Cracks" => DisplayLabelsLocalization.L("Cracks"),
        "PipeLeaks" => DisplayLabelsLocalization.L("Pipe leaks"),
        "DamagedDucts" => DisplayLabelsLocalization.L("Damaged ducts"),
        _ => DisplayLabelsLocalization.L(code)
    };

    public static string FormatConcernsList(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe)
            ? DisplayLabelsLocalization.L("General inspection")
            : string.Join(", ", pipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(FormatConcern));

    public static string FormatTiming(string? code, bool recordatorioAnual) =>
        recordatorioAnual || string.Equals(code, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? DisplayLabelsLocalization.L("Yearly reminder")
            : code switch
            {
                "ThisMonth" => DisplayLabelsLocalization.L("This month"),
                _ => DisplayLabelsLocalization.L("As soon as possible")
            };

    public static string FormatReminder(string? timing, bool recordatorioAnual, DateTime? fecha)
    {
        if (recordatorioAnual || string.Equals(timing, "YearlyReminder", StringComparison.OrdinalIgnoreCase))
        {
            if (!fecha.HasValue)
            {
                return DisplayLabelsLocalization.L("Yearly follow-up on");
            }

            var culture = CultureInfo.CurrentUICulture;
            var pattern = culture.TwoLetterISOLanguageName == "es" ? "d MMM yyyy" : "MMM d, yyyy";
            var dateLabel = fecha.Value.ToString(pattern, culture);
            var template = DisplayLabelsLocalization.L("Yearly follow-up on {0}");
            return string.Format(
                CultureInfo.InvariantCulture,
                template.Contains("{0}", StringComparison.Ordinal) ? template : "Yearly follow-up on {0}",
                dateLabel);
        }

        return DisplayLabelsLocalization.L("No yearly reminder");
    }
}
