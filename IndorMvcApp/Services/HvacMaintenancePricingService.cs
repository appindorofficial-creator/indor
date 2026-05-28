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
        desconocido || string.IsNullOrWhiteSpace(serial) ? "Not provided" : serial.Trim();

    public static string FormatLastMaintenance(string? value, bool desconocido) =>
        desconocido || string.IsNullOrWhiteSpace(value) ? "Not sure" : value.Trim();

    public static string FormatTimeWindow(string? code) => code switch
    {
        "Morning" => "Morning 8–11",
        "Midday" => "Midday 11–2",
        "Afternoon" => "Afternoon 2–5",
        _ => code ?? "—"
    };

    public static string FormatServiceType(string? code, bool recordatorioAnual) =>
        recordatorioAnual || string.Equals(code, "YearlyReminder", StringComparison.OrdinalIgnoreCase)
            ? "Yearly reminder enabled"
            : "One-time tune-up";

    public static string FormatScheduledLabel(DateTime? date, string? window) =>
        date.HasValue
            ? $"{date.Value:MMM d, yyyy} • {FormatTimeWindow(window)}"
            : FormatTimeWindow(window);

    public static string FormatPrice(decimal amount) => $"from ${amount:0}";
}
