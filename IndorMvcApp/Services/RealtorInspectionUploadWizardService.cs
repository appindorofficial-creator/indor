using System.Collections.Concurrent;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorInspectionUploadWizardService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IRealtorRegistrationService registration,
    IWebHostEnvironment env,
    IRealtorProviderBridgeService providerBridge,
    IServiceScopeFactory scopeFactory) : IRealtorInspectionUploadWizardService
{
    private const string DraftIdSessionKey = "RealtorInspectionUploadDraftId";
    private static readonly ConcurrentDictionary<int, byte> ActiveAnalyses = new();
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileBytes = RealtorInspectionUploadLimits.MaxFileBytes;

    public async Task<IndorRealtorInspectionUploadDraft> EnsureDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var draftId = session.GetInt32(DraftIdSessionKey);
        if (draftId is > 0)
        {
            var existing = await db.IndorRealtorInspectionUploadDrafts
                .Include(d => d.Findings)
                .Include(d => d.TradeProviders)
                .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                          d.Status == RealtorInspectionUploadDraftStatuses.Draft, cancellationToken);
            if (existing != null)
            {
                return existing;
            }
        }

        var entity = new IndorRealtorInspectionUploadDraft
        {
            RealtorId = realtor.Id,
            Status = RealtorInspectionUploadDraftStatuses.Draft,
            CurrentStep = 1,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorInspectionUploadDrafts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(DraftIdSessionKey, entity.Id);
        return entity;
    }

    public async Task<IndorRealtorInspectionUploadDraft?> GetDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return null;
        }

        var session = httpContextAccessor.HttpContext?.Session;
        var draftId = session?.GetInt32(DraftIdSessionKey);
        if (draftId is not > 0)
        {
            return null;
        }

        return await db.IndorRealtorInspectionUploadDrafts
            .Include(d => d.Findings)
            .Include(d => d.TradeProviders)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                        d.Status == RealtorInspectionUploadDraftStatuses.Draft, cancellationToken);
    }

    public async Task CancelDraftAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null)
        {
            httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
            return;
        }

        db.IndorRealtorInspectionUploadDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
    }

    public async Task ResetToUploadAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draftId = httpContextAccessor.HttpContext?.Session.GetInt32(DraftIdSessionKey) ?? 0;
        if (draftId <= 0)
        {
            throw new InvalidOperationException("Draft not found.");
        }

        var draft = await db.IndorRealtorInspectionUploadDrafts
            .Include(d => d.Findings)
            .Include(d => d.TradeProviders)
            .FirstOrDefaultAsync(d => d.Id == draftId
                && d.RealtorId == realtor.Id
                && d.Status == RealtorInspectionUploadDraftStatuses.Draft, cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        ActiveAnalyses.TryRemove(draft.Id, out _);

        if (!string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            var fullPath = ResolveReportFullPath(draft.ReportFileUrl);
            if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException)
                {
                    // Another process may still be reading the file during analysis.
                }
            }
        }

        if (draft.Findings.Count > 0)
        {
            db.IndorRealtorInspectionUploadFindings.RemoveRange(draft.Findings);
        }

        if (draft.TradeProviders.Count > 0)
        {
            db.IndorRealtorInspectionDraftProviders.RemoveRange(draft.TradeProviders);
        }

        draft.CurrentStep = 1;
        draft.ReportFileUrl = null;
        draft.ReportFileName = null;
        draft.ReportPageCount = 0;
        draft.AnalysisProgress = 0;
        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.Pending;
        draft.AnalysisSummary = null;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public string ResolveResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Upload",
        2 => "Analyze",
        3 => "Priorities",
        4 => "Providers",
        5 => "Review",
        _ => "Upload"
    };

    public async Task<RealtorInspectionUploadViewModel> BuildUploadAsync(string? search, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await EnsureDraftAsync(cancellationToken);

        await EnsurePropertyFilesFromPortfolioAsync(realtor.Id, cancellationToken);

        var query = db.IndorRealtorPropertyFiles.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id && p.Status == "Active");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Address.Contains(term) ||
                p.Title.Contains(term) ||
                (p.CityRegion != null && p.CityRegion.Contains(term)));
        }

        var properties = await query
            .OrderByDescending(p => p.UpdatedUtc ?? p.FechaCreacion)
            .Take(20)
            .ToListAsync(cancellationToken);

        return new RealtorInspectionUploadViewModel
        {
            DisplayStep = 1,
            Title = "Upload Inspection Report",
            Subtitle = "Choose the property and upload the inspection report to start AI analysis.",
            SearchQuery = search,
            SelectedPropertyFileId = draft.PropertyFileId,
            UploadMethod = draft.UploadMethod,
            Properties = properties.Select(p => new RealtorInspectionPropertyOptionViewModel
            {
                Id = p.Id,
                Address = p.Address,
                CityRegion = p.CityRegion ?? "",
                DisplayAddress = FormatDisplayAddress(p.Address, p.CityRegion),
                SpecsLabel = FormatSpecs(p.Beds, p.Baths, p.SqFt),
                PhotoUrl = p.PhotoUrl ?? "/welcome-house.png"
            }).ToList()
        };
    }

    public async Task SaveUploadAsync(
        int propertyFileId,
        string uploadMethod,
        IFormFile? reportFile,
        string? newPropertyAddress = null,
        string? newPropertyClientName = null,
        string? newPropertyCityRegion = null,
        CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await EnsureDraftAsync(cancellationToken);

        var resolvedPropertyId = propertyFileId;
        if (resolvedPropertyId <= 0)
        {
            resolvedPropertyId = await ResolveOrCreatePropertyFileAsync(
                realtor.Id,
                newPropertyAddress,
                newPropertyClientName,
                newPropertyCityRegion,
                cancellationToken);
        }

        var property = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == resolvedPropertyId && p.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Select a property or enter the property address to continue.");

        draft.PropertyFileId = property.Id;
        draft.Address = property.Address;
        draft.CityRegion = property.CityRegion;
        draft.ClientName = property.ClientName;
        draft.PhotoUrl = property.PhotoUrl;
        draft.UploadMethod = string.IsNullOrWhiteSpace(uploadMethod) ? "Pdf" : uploadMethod;

        if (reportFile != null && reportFile.Length > 0)
        {
            var ext = Path.GetExtension(reportFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                throw new InvalidOperationException("Upload a PDF or image file (JPG, PNG, WEBP).");
            }

            if (reportFile.Length > MaxFileBytes)
            {
                throw new InvalidOperationException($"The file is too large. Maximum size is {RealtorInspectionUploadLimits.MaxFileSizeLabel}.");
            }

            var folder = Path.Combine(env.WebRootPath, "uploads", "realtor-inspection-reports", draft.Id.ToString());
            Directory.CreateDirectory(folder);
            var fileName = $"report{ext}";
            var fullPath = Path.Combine(folder, fileName);
            await using (var stream = File.Create(fullPath))
            {
                await reportFile.CopyToAsync(stream, cancellationToken);
            }

            draft.ReportFileUrl = $"/uploads/realtor-inspection-reports/{draft.Id}/{fileName}";
            draft.ReportFileName = reportFile.FileName;
            if (ext == ".pdf")
            {
                var (_, pages) = InspectionReportTextExtractor.ExtractFromFile(fullPath);
                draft.ReportPageCount = pages > 0 ? pages : 1;
            }
            else
            {
                draft.ReportPageCount = 1;
            }
        }
        else if (string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            throw new InvalidOperationException("Add an inspection report PDF or image to continue.");
        }

        var staleFindings = await db.IndorRealtorInspectionUploadFindings
            .Where(f => f.DraftId == draft.Id)
            .ToListAsync(cancellationToken);
        if (staleFindings.Count > 0)
        {
            db.IndorRealtorInspectionUploadFindings.RemoveRange(staleFindings);
        }

        draft.AnalysisSummary = null;
        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.InProgress;
        draft.AnalysisProgress = 8;
        draft.CurrentStep = 2;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            await SyncInspectionReportToPropertyFileAsync(
                property.Id,
                draft.ReportFileUrl,
                draft.ReportFileName ?? "Inspection Report",
                cancellationToken);
        }
    }

    public async Task<bool> TryResumeDraftForPropertyAsync(int propertyFileId, CancellationToken cancellationToken = default)
    {
        if (propertyFileId <= 0)
        {
            return false;
        }

        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken);
        if (realtor == null)
        {
            return false;
        }

        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return false;
        }

        var draft = await db.IndorRealtorInspectionUploadDrafts
            .Where(d => d.RealtorId == realtor.Id &&
                        d.PropertyFileId == propertyFileId &&
                        d.Status == RealtorInspectionUploadDraftStatuses.Draft)
            .OrderByDescending(d => d.FechaActualizacion ?? d.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (draft == null)
        {
            return false;
        }

        session.SetInt32(DraftIdSessionKey, draft.Id);
        return true;
    }

    private async Task SyncInspectionReportToPropertyFileAsync(
        int propertyFileId,
        string reportUrl,
        string reportFileName,
        CancellationToken cancellationToken)
    {
        var property = await db.IndorRealtorPropertyFiles
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == propertyFileId, cancellationToken);

        if (property == null)
        {
            return;
        }

        var existing = property.Items.FirstOrDefault(i =>
            string.Equals(i.CategoryType, RealtorPropertyFileCategoryTypes.InspectionReports, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.ItemLabel = reportFileName;
            existing.FileUrl = reportUrl;
            existing.UploadedUtc = DateTime.UtcNow;
        }
        else
        {
            RealtorPropertyFileInspectionSync.UpsertInspectionReport(db, property, reportUrl, reportFileName);
        }

        RealtorPropertyFileInspectionSync.DeduplicateItemsWithFileUrl(db, property);

        RealtorPropertyFileInspectionSync.NormalizeEmptyRepairReviewPhase(property);

        property.UpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorInspectionAnalyzeViewModel> BuildAnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete upload first.");

        var progress = draft.AnalysisProgress;
        var status = draft.AnalysisStatus;

        var findingCount = await db.IndorRealtorInspectionUploadFindings
            .CountAsync(f => f.DraftId == draft.Id, cancellationToken);

        return new RealtorInspectionAnalyzeViewModel
        {
            DisplayStep = 2,
            Title = "AI Analysis",
            Subtitle = "INDOR AI scans the inspection report to find, organize, and prioritize findings automatically.",
            PropertyDisplay = FormatDisplayAddress(draft.Address ?? "", draft.CityRegion),
            ReportFileName = draft.ReportFileName ?? "Home Inspection Report",
            ReportPageCount = draft.ReportPageCount > 0 ? draft.ReportPageCount : 1,
            UploadedLabel = $"Uploaded {draft.FechaCreacion.ToLocalTime():MMM d, yyyy}",
            UploadMethod = string.IsNullOrWhiteSpace(draft.UploadMethod) ? "Pdf" : draft.UploadMethod,
            AnalysisProgress = progress,
            AnalysisStatus = status,
            AnalysisSummary = draft.AnalysisSummary,
            Tasks = BuildAnalysisTasks(progress, status, draft.ReportPageCount, findingCount, draft.UploadMethod),
            DetectedCategories = await BuildDetectedCategoriesAsync(progress, draft.Id, cancellationToken)
        };
    }

    public async Task RunAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.Complete)
        {
            return;
        }

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.Failed)
        {
            return;
        }

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.InProgress
            && draft.AnalysisProgress >= 30
            && draft.AnalysisProgress < 100)
        {
            return;
        }

        if (!ActiveAnalyses.TryAdd(draft.Id, 0))
        {
            return;
        }

        draft.AnalysisProgress = 20;
        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.InProgress;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        StartAnalysisWorker(draft.Id);
    }

    public async Task RetryAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.Complete)
        {
            return;
        }

        ActiveAnalyses.TryRemove(draft.Id, out _);

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.Failed)
        {
            var staleFindings = await db.IndorRealtorInspectionUploadFindings
                .Where(f => f.DraftId == draft.Id)
                .ToListAsync(cancellationToken);
            if (staleFindings.Count > 0)
            {
                db.IndorRealtorInspectionUploadFindings.RemoveRange(staleFindings);
            }

            draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.InProgress;
            draft.AnalysisProgress = 10;
            draft.AnalysisSummary = null;
            draft.FechaActualizacion = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.InProgress
            && draft.AnalysisProgress >= 30
            && draft.AnalysisProgress < 100)
        {
            return;
        }

        if (!ActiveAnalyses.TryAdd(draft.Id, 0))
        {
            return;
        }

        draft.AnalysisProgress = 20;
        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.InProgress;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        StartAnalysisWorker(draft.Id);
    }

    private void StartAnalysisWorker(int draftId)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var scopedDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var scopedAnalysis = scope.ServiceProvider.GetRequiredService<IOpenAiInspectionAnalysisService>();
                var scopedBridge = scope.ServiceProvider.GetRequiredService<IRealtorProviderBridgeService>();
                var scopedEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                await ExecuteAnalysisInBackgroundAsync(
                    draftId, scopedDb, scopedAnalysis, scopedBridge, scopedEnv);
            }
            finally
            {
                ActiveAnalyses.TryRemove(draftId, out _);
            }
        });
    }

    private static async Task ExecuteAnalysisInBackgroundAsync(
        int draftId,
        AppDbContext scopedDb,
        IOpenAiInspectionAnalysisService scopedAnalysis,
        IRealtorProviderBridgeService scopedBridge,
        IWebHostEnvironment scopedEnv)
    {
        var draft = await scopedDb.IndorRealtorInspectionUploadDrafts
            .FirstOrDefaultAsync(d => d.Id == draftId);
        if (draft == null || draft.CurrentStep < 2 || string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            return;
        }

        draft.AnalysisProgress = 30;
        draft.FechaActualizacion = DateTime.UtcNow;
        await scopedDb.SaveChangesAsync();

        var reportPath = ResolveReportFullPath(scopedEnv, draft.ReportFileUrl);
        var address = FormatDisplayAddress(draft.Address ?? "", draft.CityRegion);
        var result = await scopedAnalysis.AnalyzeReportAsync(address, reportPath, CancellationToken.None);

        draft = await scopedDb.IndorRealtorInspectionUploadDrafts.FirstAsync(d => d.Id == draftId);
        if (draft.CurrentStep < 2 || string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            return;
        }

        draft.AnalysisProgress = 75;
        await scopedDb.SaveChangesAsync();

        if (result.PageCount > 0)
        {
            draft.ReportPageCount = result.PageCount;
        }

        if (!result.Success)
        {
            draft.AnalysisSummary = result.ErrorMessage;
            draft.AnalysisProgress = 0;
            draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.Failed;
            draft.FechaActualizacion = DateTime.UtcNow;
            await scopedDb.SaveChangesAsync();
            return;
        }

        draft.AnalysisSummary = result.Summary;

        var existing = await scopedDb.IndorRealtorInspectionUploadFindings
            .Where(f => f.DraftId == draftId)
            .ToListAsync();
        scopedDb.IndorRealtorInspectionUploadFindings.RemoveRange(existing);

        var sort = 0;
        foreach (var finding in result.Findings)
        {
            var tradeMeta = RealtorInspectionTrades.All.FirstOrDefault(t => t.Value == finding.Trade);
            scopedDb.IndorRealtorInspectionUploadFindings.Add(new IndorRealtorInspectionUploadFinding
            {
                DraftId = draftId,
                Title = finding.Title,
                Description = finding.Description,
                SourceExcerpt = finding.SourceExcerpt,
                SourceSection = finding.SourceSection,
                SourceSectionNumber = finding.SourceSectionNumber,
                SourcePage = finding.SourcePage,
                Priority = finding.Priority,
                Trade = finding.Trade,
                TradeLabel = tradeMeta.Label,
                AiScore = finding.AiScore,
                ImageUrl = null,
                SortOrder = ++sort,
                IsSelected = false
            });
        }

        draft.AnalysisProgress = 100;
        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.Complete;
        draft.FechaActualizacion = DateTime.UtcNow;
        await scopedDb.SaveChangesAsync();
        await EnsureDefaultProvidersAsync(draftId, scopedDb, scopedBridge);
    }

    public async Task AdvanceAnalysisAsync(CancellationToken cancellationToken = default)
    {
        await RunAnalysisAsync(cancellationToken);
    }

    public async Task CompleteAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        if (draft.AnalysisStatus != RealtorInspectionAnalysisStatuses.Complete)
        {
            await RunAnalysisAsync(cancellationToken);
        }

        draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        if (draft.AnalysisStatus == RealtorInspectionAnalysisStatuses.Failed)
        {
            throw new InvalidOperationException(
                draft.AnalysisSummary ?? "AI analysis failed. Retry before viewing findings.");
        }

        await db.Entry(draft).Collection(d => d.Findings).LoadAsync(cancellationToken);
        if (draft.Findings.Count == 0)
        {
            throw new InvalidOperationException(
                "No AI findings are available yet. Wait for analysis to finish or retry.");
        }

        foreach (var finding in draft.Findings)
        {
            finding.IsSelected = false;
        }

        draft.CurrentStep = 3;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorInspectionPrioritiesViewModel> BuildPrioritiesAsync(
        string? filter, string? sort, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete analysis first.");

        await db.Entry(draft).Collection(d => d.Findings).LoadAsync(cancellationToken);
        var activeFilter = string.IsNullOrWhiteSpace(filter) ? "All" : filter.Trim();
        var findings = draft.Findings.AsEnumerable();

        if (activeFilter != "All")
        {
            findings = findings.Where(f => f.Priority == activeFilter);
        }

        findings = (sort ?? "Trade") switch
        {
            "Priority" => findings.OrderByDescending(f => PriorityWeight(f.Priority)).ThenBy(f => f.SortOrder),
            "Score" => findings.OrderByDescending(f => f.AiScore),
            _ => findings.OrderBy(f => f.Trade).ThenByDescending(f => PriorityWeight(f.Priority))
        };

        var all = draft.Findings.ToList();
        return new RealtorInspectionPrioritiesViewModel
        {
            DisplayStep = 3,
            Title = "AI Inspection Findings",
            Subtitle = "",
            PropertyDisplay = FormatDisplayAddress(draft.Address ?? "", draft.CityRegion),
            ReportFileName = draft.ReportFileName ?? "Inspection Report",
            ReportPdfUrl = draft.ReportFileUrl,
            AnalysisSummary = draft.AnalysisSummary,
            ReportDateLabel = draft.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy"),
            InspectorLabel = FormatInspectorLabel(draft.ReportFileName),
            AnalyzedLabel = FormatAnalyzedLabel(draft.FechaActualizacion ?? draft.FechaCreacion),
            TotalFindings = all.Count,
            UrgentCount = all.Count(f => f.Priority == RealtorInspectionFindingPriorities.Urgent),
            HighCount = all.Count(f => f.Priority == RealtorInspectionFindingPriorities.High),
            ModerateCount = all.Count(f => f.Priority == RealtorInspectionFindingPriorities.Moderate),
            ActiveFilter = activeFilter,
            SortBy = sort ?? "Trade",
            Findings = findings.Select(MapFindingCard).ToList()
        };
    }

    public async Task SavePrioritiesAsync(int[]? selectedFindingIds, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        await db.Entry(draft).Collection(d => d.Findings).LoadAsync(cancellationToken);
        var selected = (selectedFindingIds ?? []).ToHashSet();
        foreach (var finding in draft.Findings)
        {
            finding.IsSelected = selected.Contains(finding.Id);
        }

        draft.CurrentStep = 4;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await EnsureDefaultProvidersAsync(draft.Id, cancellationToken);
    }

    public async Task<RealtorInspectionProvidersViewModel> BuildProvidersAsync(
        string? tradeFilter, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete priorities first.");

        await db.Entry(draft).Collection(d => d.Findings).LoadAsync(cancellationToken);
        await db.Entry(draft).Collection(d => d.TradeProviders).LoadAsync(cancellationToken);

        var selectedFindings = draft.Findings.Where(f => f.IsSelected).ToList();
        var trades = selectedFindings.Select(f => f.Trade).Distinct().OrderBy(t => t).ToList();

        var selectedProviderIds = draft.TradeProviders
            .GroupBy(p => p.Trade)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProviderId).ToHashSet());

        var activeFilter = string.IsNullOrWhiteSpace(tradeFilter) ? "All" : tradeFilter.Trim();
        var groups = new List<RealtorInspectionTradeProviderGroupViewModel>();

        foreach (var trade in trades)
        {
            if (activeFilter != "All" && trade != activeFilter)
            {
                continue;
            }

            var tradeMeta = RealtorInspectionTrades.All.FirstOrDefault(t => t.Value == trade);
            var tradeFindings = selectedFindings.Where(f => f.Trade == trade).ToList();
            var topPriority = tradeFindings.OrderByDescending(f => PriorityWeight(f.Priority)).First().Priority;
            var providers = await providerBridge.MatchProveedoresForTradeAsync(trade, cancellationToken);
            var selectedForTrade = selectedProviderIds.GetValueOrDefault(trade) ?? [];

            if (selectedForTrade.Count == 0 && providers.Count > 0)
            {
                selectedForTrade = [providers[0].Id];
                if (providers.Count > 1 && trade is RealtorInspectionTrades.Electrical or RealtorInspectionTrades.Hvac)
                {
                    selectedForTrade.Add(providers[1].Id);
                }
            }

            groups.Add(new RealtorInspectionTradeProviderGroupViewModel
            {
                Trade = trade,
                TradeLabel = $"{tradeMeta.Label} needed",
                PriorityNote = FormatTradePriorityNote(topPriority, tradeFindings.Count),
                Providers = providers.Select(p => MapProveedorCard(p, selectedForTrade)).ToList()
            });
        }

        return new RealtorInspectionProvidersViewModel
        {
            DisplayStep = 4,
            Title = "Recommended Providers",
            Subtitle = "INDOR groups findings by trade and recommends trusted providers for each.",
            ActiveTradeFilter = activeFilter,
            TradeGroups = groups,
            TradesReadyCount = groups.Count,
            ProvidersSelectedCount = groups.SelectMany(g => g.Providers).Count(p => p.Selected)
        };
    }

    public async Task SaveProvidersAsync(
        Dictionary<string, int[]>? providersByTrade, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        var existing = await db.IndorRealtorInspectionDraftProviders
            .Where(p => p.DraftId == draft.Id)
            .ToListAsync(cancellationToken);
        db.IndorRealtorInspectionDraftProviders.RemoveRange(existing);

        if (providersByTrade != null)
        {
            foreach (var (trade, ids) in providersByTrade)
            {
                foreach (var providerId in ids.Distinct())
                {
                    db.IndorRealtorInspectionDraftProviders.Add(new IndorRealtorInspectionDraftProvider
                    {
                        DraftId = draft.Id,
                        Trade = trade,
                        ProviderId = providerId
                    });
                }
            }
        }

        draft.CurrentStep = 5;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorInspectionReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete providers first.");

        await db.Entry(draft).Collection(d => d.Findings).LoadAsync(cancellationToken);
        await db.Entry(draft).Collection(d => d.TradeProviders).LoadAsync(cancellationToken);

        var selectedFindings = draft.Findings.Where(f => f.IsSelected).ToList();
        var trades = selectedFindings.Select(f => f.Trade).Distinct().OrderBy(t => t).ToList();
        var providersByTrade = draft.TradeProviders.GroupBy(p => p.Trade)
            .ToDictionary(g => g.Key, g => g.Count());

        var requests = new List<RealtorInspectionReviewRequestViewModel>();
        foreach (var trade in trades)
        {
            var tradeMeta = RealtorInspectionTrades.All.FirstOrDefault(t => t.Value == trade);
            var tradeFindings = selectedFindings.Where(f => f.Trade == trade).ToList();
            var topPriority = tradeFindings.OrderByDescending(f => PriorityWeight(f.Priority)).First().Priority;
            var providerCount = providersByTrade.GetValueOrDefault(trade);
            if (providerCount == 0)
            {
                providerCount = (await providerBridge.MatchProveedoresForTradeAsync(trade, cancellationToken)).Take(2).Count();
            }

            requests.Add(new RealtorInspectionReviewRequestViewModel
            {
                TradeLabel = $"{tradeMeta.Label} request",
                PriorityTag = FormatPriorityTag(topPriority),
                PriorityCss = topPriority.ToLowerInvariant(),
                ProviderCount = providerCount
            });
        }

        return new RealtorInspectionReviewViewModel
        {
            DisplayStep = 5,
            Title = "Review & Create Requests",
            Subtitle = "Review which findings will be sent and how INDOR will create separate quote requests by trade.",
            PropertyAddress = draft.Address ?? "",
            ClientName = draft.ClientName ?? "",
            CityRegion = draft.CityRegion ?? "",
            PhotoUrl = draft.PhotoUrl ?? "/welcome-house.png",
            FindingsSelected = selectedFindings.Count,
            UrgentItems = selectedFindings.Count(f => f.Priority == RealtorInspectionFindingPriorities.Urgent),
            TradesIncluded = string.Join(", ", trades.Select(t => RealtorInspectionTrades.All.FirstOrDefault(x => x.Value == t).Label.Split(' ')[0])),
            ProvidersSelected = draft.TradeProviders.Count,
            ResponseDeadlineHours = draft.ResponseDeadlineHours,
            RequestsToCreate = requests
        };
    }

    public async Task<RealtorInspectionSuccessViewModel> CreateQuoteRequestsAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        await db.Entry(draft).Collection(d => d.Findings).LoadAsync(cancellationToken);
        await db.Entry(draft).Collection(d => d.TradeProviders).LoadAsync(cancellationToken);

        if (draft.PropertyFileId is not > 0)
        {
            throw new InvalidOperationException("Property is required.");
        }

        var selectedFindings = draft.Findings.Where(f => f.IsSelected).ToList();
        var trades = selectedFindings.Select(f => f.Trade).Distinct().ToList();

        var property = await db.IndorRealtorPropertyFiles
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == draft.PropertyFileId && p.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Property not found.");

        if (!string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            RealtorPropertyFileInspectionSync.UpsertInspectionReport(
                db,
                property,
                draft.ReportFileUrl,
                draft.ReportFileName ?? "Inspection Report");
        }

        foreach (var finding in selectedFindings)
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

        property.RepairItemsCount += selectedFindings.Count;
        property.FilePhase = RealtorPropertyFilePhases.RepairReview;
        property.UpdatedUtc = DateTime.UtcNow;

        var quoteCodes = new List<string>();
        var providersByTrade = draft.TradeProviders.GroupBy(p => p.Trade)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProviderId).ToList());

        foreach (var trade in trades)
        {
            var tradeMeta = RealtorInspectionTrades.All.FirstOrDefault(t => t.Value == trade);
            if (tradeMeta.Value != trade)
            {
                tradeMeta = (trade, trade, "fa-wrench", "handyman");
            }
            var tradeFindings = selectedFindings.Where(f => f.Trade == trade).ToList();
            var providerIds = providersByTrade.GetValueOrDefault(trade) ?? [];
            if (providerIds.Count == 0)
            {
                providerIds = (await providerBridge.MatchProveedoresForTradeAsync(trade, cancellationToken))
                    .Take(2).Select(p => p.Id).ToList();
            }

            var quoteCode = await GenerateQuoteCodeAsync(realtor.Id, cancellationToken);
            var quote = new IndorRealtorQuote
            {
                RealtorId = realtor.Id,
                QuoteCode = quoteCode,
                Address = draft.Address ?? "",
                ServiceType = tradeMeta.Label,
                Status = "Pending",
                ClientName = draft.ClientName,
                PhotoUrl = draft.PhotoUrl,
                ProviderQuotesReceived = 0,
                FooterNote = $"Waiting on {providerIds.Count} provider{(providerIds.Count == 1 ? "" : "s")}",
                RequestedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                PropertyFileId = draft.PropertyFileId,
                RequestType = RealtorQuoteRequestTypes.ByItem,
                ResponseDeadlineHours = draft.ResponseDeadlineHours,
                ProviderSelectionMode = RealtorQuoteProviderSelectionModes.IndorRecommended,
                SentUtc = DateTime.UtcNow,
                OptionalMessage = TruncateText(draft.AnalysisSummary, 500)
            };

            db.IndorRealtorQuotes.Add(quote);
            await db.SaveChangesAsync(cancellationToken);

            var proveedores = await db.IndorProveedores
                .AsNoTracking()
                .Where(p => providerIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            if (proveedores.Count == 0)
            {
                proveedores = (await providerBridge.MatchProveedoresForTradeAsync(trade, cancellationToken))
                    .Take(2)
                    .ToList();
            }

            foreach (var proveedor in proveedores)
            {
                var lead = await providerBridge.CreateLeadFromRealtorQuoteAsync(
                    quote,
                    proveedor,
                    tradeFindings,
                    draft.ReportFileUrl,
                    cancellationToken);

                db.IndorRealtorQuoteSentProviders.Add(new IndorRealtorQuoteSentProvider
                {
                    QuoteId = quote.Id,
                    ProviderId = proveedor.Id,
                    ProveedorId = proveedor.Id,
                    LeadId = lead.Id,
                    ProviderName = TruncateText(ResolveProveedorName(proveedor), 120) ?? "INDOR Provider"
                });
            }

            quoteCodes.Add(quoteCode);
        }

        property.QuotesReceivedCount += quoteCodes.Count;

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "upload",
            Description = TruncateText(
                $"Inspection analyzed — {quoteCodes.Count} quote requests for {draft.Address}",
                300) ?? "Inspection analyzed",
            CategoryTag = "Files",
            OccurredUtc = DateTime.UtcNow
        });

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "quote",
            Description = TruncateText(
                $"{selectedFindings.Count} findings sent for {draft.Address}",
                300) ?? "Findings sent to providers",
            CategoryTag = "Quotes",
            OccurredUtc = DateTime.UtcNow
        });

        draft.Status = RealtorInspectionUploadDraftStatuses.Completed;
        draft.CurrentStep = 6;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var result = new RealtorInspectionSuccessViewModel
        {
            PropertyAddress = draft.Address ?? "",
            ClientName = draft.ClientName ?? "",
            QuotesCreated = quoteCodes.Count,
            FindingsAdded = selectedFindings.Count,
            TradesSummary = string.Join(", ", trades),
            QuoteCodes = quoteCodes.Select(c => $"Quote #{c}").ToList()
        };

        db.IndorRealtorInspectionUploadDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);

        return result;
    }

    private Task EnsureDefaultProvidersAsync(int draftId, CancellationToken cancellationToken) =>
        EnsureDefaultProvidersAsync(draftId, db, providerBridge, cancellationToken);

    private static async Task EnsureDefaultProvidersAsync(
        int draftId,
        AppDbContext scopedDb,
        IRealtorProviderBridgeService scopedBridge,
        CancellationToken cancellationToken = default)
    {
        var hasProviders = await scopedDb.IndorRealtorInspectionDraftProviders
            .AnyAsync(p => p.DraftId == draftId, cancellationToken);
        if (hasProviders)
        {
            return;
        }

        var draft = await scopedDb.IndorRealtorInspectionUploadDrafts
            .Include(d => d.Findings)
            .FirstAsync(d => d.Id == draftId, cancellationToken);

        var trades = draft.Findings.Where(f => f.IsSelected).Select(f => f.Trade).Distinct();
        foreach (var trade in trades)
        {
            var matched = await scopedBridge.MatchProveedoresForTradeAsync(trade, cancellationToken);
            var take = trade is RealtorInspectionTrades.Electrical or RealtorInspectionTrades.Hvac ? 2 : 1;
            foreach (var prov in matched.Take(take))
            {
                scopedDb.IndorRealtorInspectionDraftProviders.Add(new IndorRealtorInspectionDraftProvider
                {
                    DraftId = draftId,
                    Trade = trade,
                    ProviderId = prov.Id
                });
            }
        }

        await scopedDb.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsurePropertyFilesFromPortfolioAsync(int realtorId, CancellationToken cancellationToken)
    {
        var hasFiles = await db.IndorRealtorPropertyFiles
            .AnyAsync(p => p.RealtorId == realtorId && p.Status == "Active", cancellationToken);
        if (hasFiles)
        {
            return;
        }

        var quoteRows = await db.IndorRealtorQuotes.AsNoTracking()
            .Where(q => q.RealtorId == realtorId && q.Address != "")
            .OrderByDescending(q => q.RequestedUtc)
            .Select(q => new { q.Address, q.ClientName, q.PhotoUrl })
            .ToListAsync(cancellationToken);

        foreach (var quote in quoteRows
                     .GroupBy(q => q.Address.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Select(g => g.First()))
        {
            await CreatePropertyFileIfMissingAsync(
                realtorId,
                quote.Address.Trim(),
                quote.ClientName,
                null,
                quote.PhotoUrl,
                cancellationToken);
        }

        var clientRows = await db.IndorRealtorClients.AsNoTracking()
            .Where(c => c.RealtorId == realtorId && c.PropertyAddress != null && c.PropertyAddress != "")
            .Select(c => new { c.PropertyAddress, c.FullName, c.ProfileImageUrl })
            .ToListAsync(cancellationToken);

        foreach (var client in clientRows
                     .GroupBy(c => c.PropertyAddress!.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Select(g => g.First()))
        {
            await CreatePropertyFileIfMissingAsync(
                realtorId,
                client.PropertyAddress!.Trim(),
                client.FullName,
                null,
                client.ProfileImageUrl,
                cancellationToken);
        }
    }

    private async Task<int> ResolveOrCreatePropertyFileAsync(
        int realtorId,
        string? address,
        string? clientName,
        string? cityRegion,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException("Select a property or enter the property address to continue.");
        }

        var normalizedAddress = address.Trim();
        var existing = await db.IndorRealtorPropertyFiles
            .FirstOrDefaultAsync(
                p => p.RealtorId == realtorId &&
                     p.Status == "Active" &&
                     p.Address == normalizedAddress,
                cancellationToken);

        if (existing != null)
        {
            return existing.Id;
        }

        return await CreatePropertyFileIfMissingAsync(
            realtorId,
            normalizedAddress,
            clientName,
            cityRegion,
            null,
            cancellationToken);
    }

    private async Task<int> CreatePropertyFileIfMissingAsync(
        int realtorId,
        string address,
        string? clientName,
        string? cityRegion,
        string? photoUrl,
        CancellationToken cancellationToken)
    {
        var existing = await db.IndorRealtorPropertyFiles
            .FirstOrDefaultAsync(
                p => p.RealtorId == realtorId &&
                     p.Status == "Active" &&
                     p.Address == address,
                cancellationToken);
        if (existing != null)
        {
            return existing.Id;
        }

        var file = new IndorRealtorPropertyFile
        {
            RealtorId = realtorId,
            Title = address,
            Address = address,
            CityRegion = string.IsNullOrWhiteSpace(cityRegion) ? null : cityRegion.Trim(),
            ClientName = string.IsNullOrWhiteSpace(clientName) ? null : clientName.Trim(),
            PhotoUrl = photoUrl ?? "/welcome-house.png",
            Status = "Active",
            FilePhase = RealtorPropertyFilePhases.PreClosing,
            UpdatedUtc = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorPropertyFiles.Add(file);
        await db.SaveChangesAsync(cancellationToken);
        return file.Id;
    }

    private string ResolveReportFullPath(string? reportFileUrl) =>
        ResolveReportFullPath(env, reportFileUrl);

    private static string ResolveReportFullPath(IWebHostEnvironment hostEnv, string? reportFileUrl)
    {
        if (string.IsNullOrWhiteSpace(reportFileUrl))
        {
            return string.Empty;
        }

        var relative = reportFileUrl.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(hostEnv.WebRootPath, relative);
    }

    private static RealtorQuoteProviderCardViewModel MapProveedorCard(
        IndorProveedor proveedor, HashSet<int> selectedForTrade) =>
        new()
        {
            Id = proveedor.Id,
            CompanyName = ResolveProveedorName(proveedor),
            Categories = proveedor.ServiceDescription ?? "INDOR PRO",
            Rating = 0m,
            DistanceMiles = proveedor.TravelRadiusMiles > 0 ? proveedor.TravelRadiusMiles : 5m,
            BadgeLabel = string.Equals(proveedor.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
                ? "INDOR PRO" : "Verified",
            IsVerified = true,
            Selected = selectedForTrade.Contains(proveedor.Id)
        };

    private static string ResolveProveedorName(IndorProveedor proveedor) =>
        !string.IsNullOrWhiteSpace(proveedor.DbaName) ? proveedor.DbaName!
        : !string.IsNullOrWhiteSpace(proveedor.BusinessName) ? proveedor.BusinessName!
        : "INDOR Provider";

    private async Task<string> GenerateQuoteCodeAsync(int realtorId, CancellationToken cancellationToken)
    {
        var codes = await db.IndorRealtorQuotes.AsNoTracking()
            .Where(q => q.RealtorId == realtorId && q.QuoteCode.StartsWith("Q-"))
            .Select(q => q.QuoteCode)
            .ToListAsync(cancellationToken);

        var max = 3800;
        foreach (var code in codes)
        {
            if (code.Length > 2 && int.TryParse(code[2..], out var num) && num > max)
            {
                max = num;
            }
        }

        return $"Q-{max + 1}";
    }

    private static RealtorInspectionFindingCardViewModel MapFindingCard(IndorRealtorInspectionUploadFinding f)
    {
        var tradeMeta = RealtorInspectionTrades.All.FirstOrDefault(t => t.Value == f.Trade);
        return new RealtorInspectionFindingCardViewModel
        {
            Id = f.Id,
            Title = f.Title,
            Description = f.Description,
            SourceExcerpt = f.SourceExcerpt,
            SourceSection = f.SourceSection,
            SourceSectionNumber = f.SourceSectionNumber,
            SourcePage = f.SourcePage,
            ReportReference = FormatReportReference(f.SourceSectionNumber, f.SourceSection, f.SourcePage),
            SourceLineItem = FormatSourceLineItem(f.Description, f.Title),
            Priority = f.Priority,
            PriorityCss = f.Priority.ToLowerInvariant(),
            Trade = f.Trade,
            TradeLabel = f.TradeLabel,
            TradeIcon = tradeMeta.Icon,
            TradeCss = tradeMeta.Css,
            AiScore = f.AiScore,
            ImageUrl = f.ImageUrl,
            IsSelected = false
        };
    }

    private static List<RealtorInspectionAnalyzeTaskViewModel> BuildAnalysisTasks(
        int progress, string status, int pageCount, int findingCount, string? uploadMethod)
    {
        var pages = pageCount > 0 ? pageCount : 1;
        var pagesRead = progress >= 20 ? pages : Math.Max(1, (int)(pages * progress / 20.0));
        var method = string.IsNullOrWhiteSpace(uploadMethod) ? "Pdf" : uploadMethod;
        var (readLabel, unit) = method switch
        {
            "Scan" => ("Reading scanned pages", pages == 1 ? "page" : "pages"),
            "Photos" => ("Reading report photos", pages == 1 ? "photo" : "photos"),
            _ => ("Reading report pages", pages == 1 ? "page" : "pages")
        };
        return
        [
            new()
            {
                Label = readLabel,
                Detail = $"{pagesRead} / {pages} {unit}",
                Status = progress >= 20 ? "Done" : progress >= 8 ? "InProgress" : "Pending"
            },
            new()
            {
                Label = "Extracting repair findings",
                Detail = findingCount > 0 ? $"{findingCount} findings" : "Analyzing with OpenAI",
                Status = progress >= 50 ? "Done" : progress >= 20 ? "InProgress" : "Pending"
            },
            new()
            {
                Label = "Classifying by trade",
                Detail = status == RealtorInspectionAnalysisStatuses.Complete
                    ? "Trades assigned from AI"
                    : progress >= 30 ? "OpenAI classifying trades" : "Waiting for AI",
                Status = progress >= 75 ? "Done" : progress >= 30 ? "InProgress" : "Pending"
            },
            new()
            {
                Label = "Scoring urgency",
                Detail = status == RealtorInspectionAnalysisStatuses.Complete ? "Complete" : "In progress",
                Status = status == RealtorInspectionAnalysisStatuses.Complete ? "Done" : progress >= 75 ? "InProgress" : "Pending"
            }
        ];
    }

    private async Task<List<RealtorInspectionCategoryChipViewModel>> BuildDetectedCategoriesAsync(
        int progress, int draftId, CancellationToken cancellationToken)
    {
        if (progress < 50)
        {
            return [];
        }

        var trades = await db.IndorRealtorInspectionUploadFindings
            .AsNoTracking()
            .Where(f => f.DraftId == draftId)
            .Select(f => f.Trade)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (trades.Count == 0)
        {
            return [];
        }

        return trades.Select(t =>
        {
            var meta = RealtorInspectionTrades.All.FirstOrDefault(x => x.Value == t);
            return new RealtorInspectionCategoryChipViewModel
            {
                Label = meta.Value == t ? meta.Label.Split(' ')[0] : t,
                Css = meta.Css,
                Icon = meta.Icon
            };
        }).ToList();
    }

    private static string? TruncateText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        value = value.Trim();
        return value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
    }

    private static string? FormatReportReference(string? sectionNumber, string? section, int? page)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(sectionNumber))
        {
            parts.Add(sectionNumber.Trim());
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            parts.Add(section.Trim());
        }

        if (page is > 0)
        {
            parts.Add($"Page {page}");
        }

        return parts.Count == 0 ? null : string.Join(" · ", parts);
    }

    private static string FormatInspectorLabel(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "Residential Home Inspection";
        }

        var name = Path.GetFileNameWithoutExtension(fileName)
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Trim();

        if (name.Contains("Residential Home Inspection", StringComparison.OrdinalIgnoreCase))
        {
            return "Residential Home Inspection";
        }

        return TruncateText(name, 60) ?? "Residential Home Inspection";
    }

    private static string FormatAnalyzedLabel(DateTime timestampUtc)
    {
        var local = timestampUtc.ToLocalTime();
        return local.Date == DateTime.Today ? "Today" : local.ToString("MMM d, yyyy");
    }

    private static string? FormatSourceLineItem(string? description, string title)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            return TruncateText(description.Trim(), 80);
        }

        return string.IsNullOrWhiteSpace(title) ? null : title;
    }

    private static int PriorityWeight(string priority) => priority switch
    {
        RealtorInspectionFindingPriorities.Urgent => 3,
        RealtorInspectionFindingPriorities.High => 2,
        _ => 1
    };

    private static string FormatTradePriorityNote(string priority, int count) => priority switch
    {
        RealtorInspectionFindingPriorities.Urgent => $"{count} urgent item{(count == 1 ? "" : "s")}",
        RealtorInspectionFindingPriorities.High => $"{count} high-priority item{(count == 1 ? "" : "s")}",
        _ => $"{count} moderate item{(count == 1 ? "" : "s")}"
    };

    private static string FormatPriorityTag(string priority) => priority switch
    {
        RealtorInspectionFindingPriorities.Urgent => "Urgent",
        RealtorInspectionFindingPriorities.High => "High priority",
        _ => "Moderate priority"
    };

    private static string FormatDisplayAddress(string address, string? cityRegion) =>
        string.IsNullOrWhiteSpace(cityRegion) ? address : $"{address}, {cityRegion}";

    private static string FormatSpecs(int? beds, decimal? baths, int? sqFt)
    {
        var parts = new List<string>();
        if (beds is > 0) parts.Add($"{beds} beds");
        if (baths is > 0) parts.Add($"{baths:0.#} baths");
        if (sqFt is > 0) parts.Add($"{sqFt:N0} sq ft");
        return string.Join(", ", parts);
    }
}
