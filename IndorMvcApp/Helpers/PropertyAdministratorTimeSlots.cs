namespace IndorMvcApp.Helpers;

/// <summary>Shared hourly arrival slots for PA service request flows.</summary>
public static class PropertyAdministratorTimeSlots
{
    public static readonly string[] Hourly =
    [
        "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM",
        "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM",
        "4:00 PM", "5:00 PM", "6:00 PM"
    ];

    public const string Default = "11:00 AM";

    public static string Resolve(string? selected, string? fallback = null)
    {
        var candidate = string.IsNullOrWhiteSpace(selected) ? null : selected.Trim();
        if (candidate != null && Hourly.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            return Hourly.First(h => h.Equals(candidate, StringComparison.OrdinalIgnoreCase));
        }

        var fb = string.IsNullOrWhiteSpace(fallback) ? Default : fallback.Trim();
        return Hourly.Contains(fb, StringComparer.OrdinalIgnoreCase) ? fb : Default;
    }

    public static bool LooksLikeClockTime(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && (value.Contains("AM", StringComparison.OrdinalIgnoreCase)
            || value.Contains("PM", StringComparison.OrdinalIgnoreCase));
}
