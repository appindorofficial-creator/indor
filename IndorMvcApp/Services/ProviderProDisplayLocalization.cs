using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

/// <summary>Localized labels for Provider PRO services and profile flows.</summary>
public static class ProviderProDisplayLocalization
{
    public static string L(string english) => DisplayLabelsLocalization.L(english);

    public static string MapJobStatus(string status) => status switch
    {
        ProviderJobStatuses.InProgress => L("On Site"),
        ProviderJobStatuses.Completed => L("Completed"),
        ProviderJobStatuses.Confirmed => L("Confirmed"),
        ProviderJobStatuses.WaitingOnMaterials => L("Waiting"),
        _ => L("Scheduled")
    };

    public static string DayLabel(int offset, DateTime day) => offset switch
    {
        0 => L("Today"),
        1 => $"{L("Tomorrow")}, {day.ToString("MMM d")}",
        _ => day.ToString("dddd, MMM d")
    };

    public static (string Kind, string Label) SectionStatus(string id, bool complete, bool pendingReview)
    {
        if (complete) return ("complete", L("Complete"));
        if (pendingReview) return ("pending", L("Pending Review"));
        if (id is "insurance" or "w9") return ("missing", L("Missing"));
        return ("incomplete", L("Incomplete"));
    }
}
