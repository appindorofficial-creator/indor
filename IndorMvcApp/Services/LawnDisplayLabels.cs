namespace IndorMvcApp.Services;

public static class LawnDisplayLabels
{
    public static string FormatServiceType(string? value) => value switch
    {
        "OneTime" => "One-time",
        "Subscription" => "Subscription",
        _ => value ?? "—"
    };

    public static string FormatFrequency(string? value) => value switch
    {
        "Weekly" => "Weekly",
        "Biweekly" => "Biweekly",
        "Monthly" => "Monthly",
        _ => value ?? "—"
    };

    public static string FormatSubscriptionLabel(string? tipoServicio, string? frecuencia)
    {
        if (string.Equals(tipoServicio, "Subscription", StringComparison.OrdinalIgnoreCase))
        {
            return $"{FormatFrequency(frecuencia)} subscription";
        }

        return "One-time service";
    }

    public static string FormatArea(string? value)
    {
        var area = LawnPricingService.AreaOptions.FirstOrDefault(a => a.Code == value);
        return area?.Label ?? value ?? "—";
    }

    public static string FormatAddonsList(string? pipeValue)
    {
        var addons = LawnPricingService.ParseAddons(pipeValue).Select(a => a.Label).ToList();
        return addons.Count == 0 ? "None" : string.Join(", ", addons);
    }

    public static string FormatPreference(string? value) => value switch
    {
        "FrontOnly" => "Front only",
        "BackOnly" => "Back only",
        "ExtraCleanup" => "Extra cleanup",
        "NoThanks" => "No thanks",
        _ => value ?? "—"
    };

    public static string FormatTimeWindow(string? value) => value switch
    {
        "Morning8_11" => "8–11 AM",
        "Midday11_2" => "11 AM–2 PM",
        "Afternoon2_5" => "2–5 PM",
        "Evening5_8" => "5–8 PM",
        _ => value ?? "—"
    };

    public static string FormatScheduledLabel(DateTime? fecha, string? ventana)
    {
        if (!fecha.HasValue)
        {
            return "To be confirmed";
        }

        var day = fecha.Value.ToString("dddd");
        var window = FormatTimeWindow(ventana);
        return string.IsNullOrWhiteSpace(window) || window == "—"
            ? day
            : $"{day}, {window}";
    }

    public static string FormatPreferredDay(DateTime? fecha) =>
        fecha.HasValue ? fecha.Value.ToString("dddd") + " morning" : "Flexible";
}
