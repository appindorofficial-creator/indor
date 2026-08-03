using System.Globalization;

namespace IndorMvcApp.Services;

public static class HvacMaintenancePricingService
{
    public const decimal StartingPrice = 89m;

    public static decimal GetEstimatedPrice(string? tipoServicio) =>
        StartingPrice;
}

public static class HvacMaintenanceDisplayLabels
{
    public static string FormatSerial(string? serial, bool desconocido) =>
        desconocido || string.IsNullOrWhiteSpace(serial)
            ? DisplayLabelsLocalization.L("Not provided")
            : serial.Trim();

    public static string FormatLastMaintenance(string? value, bool desconocido)
    {
        if (desconocido || string.IsNullOrWhiteSpace(value))
        {
            return DisplayLabelsLocalization.L("Not sure");
        }

        if (DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var isoDate))
        {
            return FormatDate(isoDate);
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            || DateTime.TryParse(value, CultureInfo.CurrentUICulture, DateTimeStyles.None, out date))
        {
            return FormatDate(date);
        }

        return value.Trim();
    }

    /// <summary>UI culture date — e.g. "Aug 2, 2026" (en) / "2 ago 2026" (es).</summary>
    public static string FormatDate(DateTime date) =>
        date.ToString(
            DisplayLabelsLocalization.IsSpanishUi ? "d MMM yyyy" : "MMM d, yyyy",
            CultureInfo.CurrentUICulture);

    public static string FormatTimeWindow(string? code)
    {
        var key = (code ?? string.Empty).Trim();
        // Normalize ASCII hyphen so DB/catalog variants still resolve.
        key = key.Replace('-', '–');

        return key switch
        {
            "Morning" or "Morning 8–11" => DisplayLabelsLocalization.L("Morning 8–11"),
            "Midday" or "Midday 11–2" => DisplayLabelsLocalization.L("Midday 11–2"),
            "Afternoon" or "Afternoon 2–5" => DisplayLabelsLocalization.L("Afternoon 2–5"),
            "Evening" or "Evening 5–8" => DisplayLabelsLocalization.L("Evening 5–8"),
            _ => string.IsNullOrWhiteSpace(code) ? "—" : DisplayLabelsLocalization.L(code)
        };
    }

    public static string FormatServiceType(string? code, bool recordatorioAnual) =>
        recordatorioAnual || string.Equals(code, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? DisplayLabelsLocalization.L("Yearly reminder enabled")
            : DisplayLabelsLocalization.L("One-time tune-up");

    public static string FormatScheduledLabel(DateTime? date, string? window) =>
        date.HasValue
            ? $"{FormatDate(date.Value)} • {FormatTimeWindow(window)}"
            : FormatTimeWindow(window);

    public static string FormatPrice(decimal amount) =>
        string.Format(
            CultureInfo.CurrentCulture,
            DisplayLabelsLocalization.L("From ${0}"),
            amount.ToString("0", CultureInfo.InvariantCulture));
}
