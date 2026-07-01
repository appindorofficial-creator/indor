using IndorMvcApp.Models;

namespace IndorMvcApp.Services;

public sealed record RealtorPropertyFileActionContext(
    IndorRealtorPropertyFile File,
    IndorRealtorInspectionUploadDraft? PendingInspectionDraft = null,
    bool OnViewPage = false);

public static class RealtorPropertyFileActions
{
    public static bool HasInspectionReport(IndorRealtorPropertyFile file) =>
        file.Items.Any(i =>
            string.Equals(i.CategoryType, RealtorPropertyFileCategoryTypes.InspectionReports, StringComparison.OrdinalIgnoreCase));

    public static bool HasPendingInspection(RealtorPropertyFileActionContext context) =>
        context.PendingInspectionDraft is { Status: RealtorInspectionUploadDraftStatuses.Draft } draft &&
        !string.IsNullOrWhiteSpace(draft.ReportFileUrl);

    public static string ContinueInspectionUrl(int propertyFileId) =>
        $"/RealtorInspectionUpload/Continue?propertyFileId={propertyFileId}";

    public static (string Badge, string Css) DeriveStatus(RealtorPropertyFileActionContext context)
    {
        var file = context.File;

        if (string.Equals(file.FilePhase, RealtorPropertyFilePhases.Transfer, StringComparison.Ordinal))
        {
            return ("Shared Package", "shared");
        }

        if (file.QuotesReceivedCount > 0)
        {
            return ("Quotes Pending", "quotes");
        }

        if (file.RepairItemsCount > 0 || HasInspectionReport(file))
        {
            return ("Inspection Uploaded", "inspection");
        }

        if (HasPendingInspection(context))
        {
            return ("Analysis In Progress", "inspection");
        }

        if (string.Equals(file.Status, "Archived", StringComparison.OrdinalIgnoreCase))
        {
            return ("Closed", "closed");
        }

        return ("Active", "active");
    }

    public static (string Label, string Url, string? Detail) DeriveListAction(RealtorPropertyFileActionContext context)
    {
        var file = context.File;
        var id = file.Id;
        var viewUrl = $"/RealtorPropertyFile/View?id={id}";

        if (string.Equals(file.FilePhase, RealtorPropertyFilePhases.Transfer, StringComparison.Ordinal))
        {
            return ("View Package", viewUrl, "Shared with client");
        }

        if (file.RepairItemsCount > 0 && file.QuotesReceivedCount == 0)
        {
            return ("Request Quotes", $"/RealtorQuoteRequest/Start?propertyFileId={id}", $"Repair items: {file.RepairItemsCount}");
        }

        if (HasPendingInspection(context))
        {
            return ("Continue Inspection", ContinueInspectionUrl(id), "Finish AI analysis and review findings");
        }

        if (!HasInspectionReport(file) && IsAwaitingInspectionReport(file))
        {
            return ("Upload Report", $"/RealtorInspectionUpload/Upload?propertyFileId={id}", "Awaiting inspection report");
        }

        if (file.QuotesReceivedCount > 0)
        {
            return ("View File", viewUrl, $"Quotes received: {file.QuotesReceivedCount}");
        }

        return ("View File", viewUrl, file.RepairItemsCount > 0 ? $"Repair items: {file.RepairItemsCount}" : null);
    }

    public static (string Label, string Url, string? SecondaryLabel, string? SecondaryUrl) DeriveViewActions(
        RealtorPropertyFileActionContext context)
    {
        var (label, url, _) = DeriveListAction(context);
        var viewUrl = $"/RealtorPropertyFile/View?id={context.File.Id}";

        if (!string.Equals(url, viewUrl, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(label, "Request Quotes", StringComparison.Ordinal))
            {
                return (label, url, "View File", viewUrl);
            }

            return (label, url, null, null);
        }

        return (string.Empty, string.Empty, null, null);
    }

    private static bool IsAwaitingInspectionReport(IndorRealtorPropertyFile file) =>
        string.IsNullOrWhiteSpace(file.FilePhase) ||
        string.Equals(file.FilePhase, RealtorPropertyFilePhases.PreClosing, StringComparison.Ordinal) ||
        string.Equals(file.FilePhase, RealtorPropertyFilePhases.General, StringComparison.Ordinal);
}
