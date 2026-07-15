namespace IndorMvcApp.Services;

// Localized via DisplayLabelsLocalization.L

public static class ProviderDisplayLabels
{
    public static string FormatRegistrationStatus(string? status) => status switch
    {
        "Draft" => DisplayLabelsLocalization.L("In progress"),
        "IndorProActive" => DisplayLabelsLocalization.L("INDOR Pro Active"),
        "Submitted" => DisplayLabelsLocalization.L("Submitted"),
        "PendingReview" => DisplayLabelsLocalization.L("Pending INDOR review"),
        "Approved" => DisplayLabelsLocalization.L("Approved"),
        "Rejected" => DisplayLabelsLocalization.L("Not approved — contact support"),
        _ => DisplayLabelsLocalization.L("Unknown")
    };

    public static string StatusBadgeClass(string? status) => status switch
    {
        "Approved" or "IndorProActive" => DisplayLabelsLocalization.L("prv-status--approved"),
        "Rejected" => DisplayLabelsLocalization.L("prv-status--rejected"),
        "PendingReview" or "Submitted" => DisplayLabelsLocalization.L("prv-status--pending"),
        _ => DisplayLabelsLocalization.L("prv-status--draft"),
    };
}
