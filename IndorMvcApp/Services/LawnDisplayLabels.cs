namespace IndorMvcApp.Services;

public static class LawnDisplayLabels
{
    public static string FormatFrequency(string? value) => value switch
    {
        "Once" => "Once",
        "Every15Days" => "Every 15 days",
        "Biweekly" => "Every 15 days",
        "Weekly" => "Weekly",
        "Monthly" => "Monthly",
        "Flexible" => "Flexible",
        _ => value ?? "—"
    };

    public static string FormatFrequencyLabel(string? frequencyCode, string? serviceType = null)
    {
        if (string.Equals(frequencyCode, "Once", StringComparison.OrdinalIgnoreCase)
            || string.Equals(serviceType, "OneTime", StringComparison.OrdinalIgnoreCase))
        {
            return "One-time service";
        }

        return FormatFrequency(frequencyCode);
    }

    public static string FormatArea(string? value, string? labelFromCatalog = null) =>
        labelFromCatalog ?? value switch
        {
            "FrontYard" => "Front",
            "BackYard" => "Backyard",
            "FrontBack" => "Front + Backyard",
            "SideYard" => "Side yard",
            "FullProperty" => "Full property",
            _ => value ?? "—"
        };

    public static string FormatAddonsList(IEnumerable<string> labels)
    {
        var list = labels.ToList();
        return list.Count == 0 ? "None" : string.Join(", ", list);
    }

    public static string FormatTimeWindow(string? value, string? labelFromCatalog = null) =>
        labelFromCatalog ?? value switch
        {
            "Morning8_11" => "8–11 AM",
            "Midday11_2" => "11 AM–2 PM",
            "Afternoon2_5" => "2–5 PM",
            "Evening5_8" => "5–8 PM",
            _ => value ?? "—"
        };

    public static string FormatScheduledLabel(DateTime? fecha, string? ventana, string? ventanaLabel = null)
    {
        if (!fecha.HasValue)
        {
            return "To be confirmed";
        }

        var day = fecha.Value.ToString("dddd");
        var window = FormatTimeWindow(ventana, ventanaLabel);
        return string.IsNullOrWhiteSpace(window) || window == "—"
            ? day
            : $"{day}, {window}";
    }

    public static string FormatReminderLabel(
        bool active,
        string? frequencyCode,
        int leadDays,
        IEnumerable<string>? channels = null)
    {
        if (!active)
        {
            return "Off";
        }

        var freq = FormatFrequency(frequencyCode);
        var lead = leadDays == 1 ? "1 day before" : $"{leadDays} days before";
        var channelText = channels == null ? string.Empty : string.Join(", ", channels);
        return string.IsNullOrWhiteSpace(channelText)
            ? $"Active · {freq} · {lead}"
            : $"Active · {freq} · {lead} · {channelText}";
    }

    public static string FormatNextReminderLabel(DateTime? nextUtc, string? frequencyCode)
    {
        if (nextUtc.HasValue)
        {
            var local = nextUtc.Value.ToLocalTime();
            return local.ToString("MMM d, yyyy");
        }

        var days = LawnCatalogService.GetFrequencyIntervalDays(frequencyCode);
        return days > 0 ? $"In {days} days" : "—";
    }

    public static string FormatNotificationChannels(string? pipeValue, IReadOnlyDictionary<string, string>? labels = null)
    {
        if (string.IsNullOrWhiteSpace(pipeValue))
        {
            return "Push";
        }

        return string.Join(", ", pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(code => labels != null && labels.TryGetValue(code, out var label) ? label : code));
    }
}
