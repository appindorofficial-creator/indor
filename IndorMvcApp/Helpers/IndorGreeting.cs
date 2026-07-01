namespace IndorMvcApp.Helpers;

public static class IndorGreeting
{
    private static readonly string[] PreferredTimeZoneIds =
    [
        "America/Toronto",
        "America/New_York",
        "Eastern Standard Time",
        "America/Chicago",
        "Central Standard Time",
        "America/Bogota",
        "America/Mexico_City"
    ];

    public static string ForHour(int hour) =>
        hour switch
        {
            >= 5 and < 12 => "Good morning",
            >= 12 and < 17 => "Good afternoon",
            >= 17 and < 22 => "Good evening",
            _ => "Good evening"
        };

    public static string ForNow() => ForHour(ResolveLocalHour());

    private static int ResolveLocalHour()
    {
        foreach (var timeZoneId in PreferredTimeZoneIds)
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZone))
            {
                continue;
            }

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).Hour;
        }

        return DateTime.Now.Hour;
    }
}
