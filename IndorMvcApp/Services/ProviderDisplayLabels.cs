namespace IndorMvcApp.Services;

public static class ProviderDisplayLabels
{
    public static string FormatRegistrationStatus(string? status) => status switch
    {
        "Draft" => "In progress",
        "IndorProActive" => "INDOR Pro Active",
        "Submitted" => "Submitted",
        "PendingReview" => "Pending INDOR review",
        "Approved" => "Approved",
        "Rejected" => "Not approved — contact support",
        _ => "Unknown"
    };

    public static string StatusBadgeClass(string? status) => status switch
    {
        "Approved" or "IndorProActive" => "prv-status--approved",
        "Rejected" => "prv-status--rejected",
        "PendingReview" or "Submitted" => "prv-status--pending",
        _ => "prv-status--draft",
    };
}
