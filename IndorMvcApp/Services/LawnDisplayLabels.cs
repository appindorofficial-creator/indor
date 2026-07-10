namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class LawnDisplayLabels
{
    public static string FormatFrequency(string? value) => value switch
    {
        "Once" => DisplayLabelsLocalization.L("Once"),
        "Every15Days" => DisplayLabelsLocalization.L("Every 15 days"),
        "Biweekly" => DisplayLabelsLocalization.L("Every 15 days"),
        "Weekly" => DisplayLabelsLocalization.L("Weekly"),
        "Monthly" => DisplayLabelsLocalization.L("Monthly"),
        "Flexible" => DisplayLabelsLocalization.L("Flexible"),
        _ => value ?? "â€”"
    };

    public static string FormatFrequencyLabel(string? frequencyCode, string? serviceType = null)
    {
        if (string.Equals(frequencyCode, "Once", StringComparison.OrdinalIgnoreCase)
            || string.Equals(serviceType, "OneTime", StringComparison.OrdinalIgnoreCase))
        {
            return DisplayLabelsLocalization.L("One-time service");
        }

        return FormatFrequency(frequencyCode);
    }

    public static string FormatArea(string? value, string? labelFromCatalog = null) =>
        labelFromCatalog ?? value switch
        {
            "FrontYard" => DisplayLabelsLocalization.L("Front"),
            "BackYard" => DisplayLabelsLocalization.L("Backyard"),
            "FrontBack" => DisplayLabelsLocalization.L("Front + Backyard"),
            "SideYard" => DisplayLabelsLocalization.L("Side yard"),
            "FullProperty" => DisplayLabelsLocalization.L("Full property"),
            _ => value ?? "â€”"
        };

    public static string FormatAddonsList(IEnumerable<string> labels)
    {
        var list = labels.ToList();
        return list.Count == 0 ? DisplayLabelsLocalization.L("None") : string.Join(", ", list);
    }

    public static string FormatTimeWindow(string? value, string? labelFromCatalog = null) =>
        labelFromCatalog ?? value switch
        {
            "Morning8_11" => DisplayLabelsLocalization.L("8â€“11 AM"),
            "Midday11_2" => DisplayLabelsLocalization.L("11 AMâ€“2 PM"),
            "Afternoon2_5" => DisplayLabelsLocalization.L("2â€“5 PM"),
            "Evening5_8" => DisplayLabelsLocalization.L("5â€“8 PM"),
            _ => value ?? "â€”"
        };

    public static string FormatScheduledLabel(DateTime? fecha, string? ventana, string? ventanaLabel = null)
    {
        if (!fecha.HasValue)
        {
            return DisplayLabelsLocalization.L("To be confirmed");
        }

        var day = fecha.Value.ToString("dddd");
        var window = FormatTimeWindow(ventana, ventanaLabel);
        return string.IsNullOrWhiteSpace(window) || window == "â€”"
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
            return DisplayLabelsLocalization.L("Off");
        }

        var freq = FormatFrequency(frequencyCode);
        var lead = leadDays == 1 ? "1 day before" : $"{leadDays} days before";
        var channelText = channels == null ? string.Empty : string.Join(", ", channels);
        return string.IsNullOrWhiteSpace(channelText)
            ? $"Active Â· {freq} Â· {lead}"
            : $"Active Â· {freq} Â· {lead} Â· {channelText}";
    }

    public static string FormatNextReminderLabel(DateTime? nextUtc, string? frequencyCode)
    {
        if (nextUtc.HasValue)
        {
            var local = nextUtc.Value.ToLocalTime();
            return local.ToString("MMM d, yyyy");
        }

        var days = LawnCatalogService.GetFrequencyIntervalDays(frequencyCode);
        return days > 0 ? $"In {days} days" : "â€”";
    }

    public static string FormatNotificationChannels(string? pipeValue, IReadOnlyDictionary<string, string>? labels = null)
    {
        if (string.IsNullOrWhiteSpace(pipeValue))
        {
            return DisplayLabelsLocalization.L("Push");
        }

        return string.Join(", ", pipeValue.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(code => labels != null && labels.TryGetValue(code, out var label) ? label : code));
    }
}
