using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public static class RealtorPropertyFileInspectionSync
{
    public static bool HasInspectionReport(IndorRealtorPropertyFile file) =>
        file.Items.Any(i =>
            string.Equals(i.CategoryType, RealtorPropertyFileCategoryTypes.InspectionReports, StringComparison.OrdinalIgnoreCase));

    public static void UpsertInspectionReport(
        AppDbContext db,
        IndorRealtorPropertyFile property,
        string reportUrl,
        string reportFileName)
    {
        var existing = property.Items.FirstOrDefault(i =>
            string.Equals(i.CategoryType, RealtorPropertyFileCategoryTypes.InspectionReports, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.ItemLabel = reportFileName;
            existing.FileUrl = reportUrl;
            existing.UploadedUtc = DateTime.UtcNow;
            return;
        }

        db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
        {
            PropertyFileId = property.Id,
            CategoryType = RealtorPropertyFileCategoryTypes.InspectionReports,
            ItemLabel = reportFileName,
            FileUrl = reportUrl,
            UploadedUtc = DateTime.UtcNow
        });
    }

    public static int UpsertRepairItemsFromFindings(
        AppDbContext db,
        IndorRealtorPropertyFile property,
        IEnumerable<IndorRealtorInspectionUploadFinding> findings)
    {
        var selected = findings.Where(f => f.IsSelected).ToList();
        var source = selected.Count > 0 ? selected : findings.ToList();
        if (source.Count == 0)
        {
            return 0;
        }

        var existingRepairItems = property.Items
            .Where(i => string.Equals(i.CategoryType, RealtorPropertyFileCategoryTypes.RepairItems, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (existingRepairItems.Count > 0)
        {
            return property.RepairItemsCount;
        }

        foreach (var finding in source)
        {
            db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
            {
                PropertyFileId = property.Id,
                CategoryType = RealtorPropertyFileCategoryTypes.RepairItems,
                ItemLabel = finding.Title,
                NoteText = $"{finding.Priority} · {finding.TradeLabel} · AI score {finding.AiScore}",
                UploadedUtc = DateTime.UtcNow
            });
        }

        property.RepairItemsCount = source.Count;
        property.FilePhase = RealtorPropertyFilePhases.RepairReview;
        return source.Count;
    }

    public static void NormalizeEmptyRepairReviewPhase(IndorRealtorPropertyFile property)
    {
        if (property.RepairItemsCount > 0 || HasInspectionReport(property))
        {
            return;
        }

        if (string.Equals(property.FilePhase, RealtorPropertyFilePhases.RepairReview, StringComparison.Ordinal))
        {
            property.FilePhase = RealtorPropertyFilePhases.PreClosing;
        }
    }
}
