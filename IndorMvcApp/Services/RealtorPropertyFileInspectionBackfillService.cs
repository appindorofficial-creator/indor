using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed class RealtorPropertyFileInspectionBackfillService(AppDbContext db)
{
    public sealed record BackfillResult(int DraftsProcessed, int ReportsSynced, int RepairItemsSynced, int PhasesFixed);

    public async Task<BackfillResult> BackfillAsync(int? realtorId = null, CancellationToken cancellationToken = default)
    {
        var reportsSynced = 0;
        var repairItemsSynced = 0;
        var phasesFixed = 0;

        var draftQuery = db.IndorRealtorInspectionUploadDrafts
            .Include(d => d.Findings)
            .Where(d => d.PropertyFileId != null && d.ReportFileUrl != null && d.ReportFileUrl != "");

        if (realtorId is > 0)
        {
            draftQuery = draftQuery.Where(d => d.RealtorId == realtorId);
        }

        var drafts = await draftQuery
            .OrderByDescending(d => d.FechaActualizacion ?? d.FechaCreacion)
            .ToListAsync(cancellationToken);

        var processedPropertyIds = new HashSet<int>();

        foreach (var draft in drafts)
        {
            var propertyId = draft.PropertyFileId!.Value;
            if (!processedPropertyIds.Add(propertyId))
            {
                continue;
            }

            await SyncDraftToPropertyAsync(draft, propertyId, cancellationToken,
                () => reportsSynced++, () => repairItemsSynced++, () => phasesFixed++);
        }

        var addressDraftQuery = db.IndorRealtorInspectionUploadDrafts
            .Include(d => d.Findings)
            .Where(d => d.PropertyFileId == null &&
                        d.ReportFileUrl != null && d.ReportFileUrl != "" &&
                        d.Address != null && d.Address != "");

        if (realtorId is > 0)
        {
            addressDraftQuery = addressDraftQuery.Where(d => d.RealtorId == realtorId);
        }

        var addressDrafts = await addressDraftQuery
            .OrderByDescending(d => d.FechaActualizacion ?? d.FechaCreacion)
            .ToListAsync(cancellationToken);

        foreach (var draft in addressDrafts)
        {
            var property = await db.IndorRealtorPropertyFiles
                .Include(p => p.Items)
                .FirstOrDefaultAsync(
                    p => p.Status == "Active" && p.Address == draft.Address,
                    cancellationToken);

            if (property == null || !processedPropertyIds.Add(property.Id))
            {
                continue;
            }

            draft.PropertyFileId = property.Id;
            await SyncDraftToPropertyAsync(draft, property.Id, cancellationToken,
                () => reportsSynced++, () => repairItemsSynced++, () => phasesFixed++);
        }

        var propertyQuery = db.IndorRealtorPropertyFiles
            .Include(p => p.Items)
            .Where(p => p.Status == "Active");

        if (realtorId is > 0)
        {
            propertyQuery = propertyQuery.Where(p => p.RealtorId == realtorId);
        }

        var propertiesMissingReports = await propertyQuery
            .Where(p => !p.Items.Any(i => i.CategoryType == RealtorPropertyFileCategoryTypes.InspectionReports))
            .ToListAsync(cancellationToken);

        foreach (var property in propertiesMissingReports)
        {
            if (processedPropertyIds.Contains(property.Id))
            {
                continue;
            }

            var draft = await db.IndorRealtorInspectionUploadDrafts
                .Include(d => d.Findings)
                .Where(d => d.ReportFileUrl != null && d.ReportFileUrl != "" &&
                            (d.PropertyFileId == property.Id ||
                             (d.Address != null && d.Address == property.Address)))
                .OrderByDescending(d => d.FechaActualizacion ?? d.FechaCreacion)
                .FirstOrDefaultAsync(cancellationToken);

            if (draft == null)
            {
                continue;
            }

            draft.PropertyFileId = property.Id;
            processedPropertyIds.Add(property.Id);
            await SyncDraftToPropertyAsync(draft, property.Id, cancellationToken,
                () => reportsSynced++, () => repairItemsSynced++, () => phasesFixed++);
        }

        var orphanQuery = db.IndorRealtorPropertyFiles
            .Include(p => p.Items)
            .Where(p => p.FilePhase == RealtorPropertyFilePhases.RepairReview && p.RepairItemsCount == 0);

        if (realtorId is > 0)
        {
            orphanQuery = orphanQuery.Where(p => p.RealtorId == realtorId);
        }

        var orphanRepairReview = await orphanQuery.ToListAsync(cancellationToken);

        foreach (var property in orphanRepairReview)
        {
            if (processedPropertyIds.Contains(property.Id))
            {
                continue;
            }

            if (!RealtorPropertyFileInspectionSync.HasInspectionReport(property))
            {
                var beforePhase = property.FilePhase;
                RealtorPropertyFileInspectionSync.NormalizeEmptyRepairReviewPhase(property);
                if (!string.Equals(beforePhase, property.FilePhase, StringComparison.Ordinal))
                {
                    phasesFixed++;
                    property.UpdatedUtc = DateTime.UtcNow;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new BackfillResult(drafts.Count + addressDrafts.Count, reportsSynced, repairItemsSynced, phasesFixed);
    }

    private async Task SyncDraftToPropertyAsync(
        IndorRealtorInspectionUploadDraft draft,
        int propertyId,
        CancellationToken cancellationToken,
        Action onReportSynced,
        Action onRepairItemsSynced,
        Action onPhaseFixed)
    {
        var property = await db.IndorRealtorPropertyFiles
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (property == null)
        {
            return;
        }

        if (!RealtorPropertyFileInspectionSync.HasInspectionReport(property))
        {
            RealtorPropertyFileInspectionSync.UpsertInspectionReport(
                db,
                property,
                draft.ReportFileUrl!,
                draft.ReportFileName ?? "Inspection Report");
            onReportSynced();
        }

        if (string.Equals(draft.AnalysisStatus, RealtorInspectionAnalysisStatuses.Complete, StringComparison.Ordinal) &&
            draft.Findings.Count > 0 &&
            property.Items.All(i => !string.Equals(i.CategoryType, RealtorPropertyFileCategoryTypes.RepairItems, StringComparison.OrdinalIgnoreCase)))
        {
            var added = RealtorPropertyFileInspectionSync.UpsertRepairItemsFromFindings(
                db,
                property,
                draft.Findings);
            if (added > 0)
            {
                onRepairItemsSynced();
            }
        }

        var beforePhase = property.FilePhase;
        RealtorPropertyFileInspectionSync.NormalizeEmptyRepairReviewPhase(property);
        if (!string.Equals(beforePhase, property.FilePhase, StringComparison.Ordinal))
        {
            onPhaseFixed();
        }

        property.UpdatedUtc = DateTime.UtcNow;
    }
}
