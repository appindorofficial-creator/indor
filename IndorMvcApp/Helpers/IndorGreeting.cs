namespace IndorMvcApp.Helpers;

public static class IndorGreeting
{
    public static string ForHour(int hour) =>
        hour switch
        {
            >= 5 and < 12 => "Good morning",
            >= 12 and < 17 => "Good afternoon",
            >= 17 and < 22 => "Good evening",
            _ => "Good evening"
        };

    public static string ForNow() => ForHour(DateTime.Now.Hour);
}
