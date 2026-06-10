using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorInspectionUploadWizardService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IRealtorRegistrationService registration,
    IWebHostEnvironment env) : IRealtorInspectionUploadWizardService
{
    private const string DraftIdSessionKey = "RealtorInspectionUploadDraftId";
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileBytes = 15_000_000;

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
            Subtitle = "Select a property and add your inspection report.",
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
        int propertyFileId, string uploadMethod, IFormFile? reportFile, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await EnsureDraftAsync(cancellationToken);

        var property = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyFileId && p.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Property not found.");

        draft.PropertyFileId = property.Id;
        draft.Address = property.Address;
        draft.CityRegion = property.CityRegion;
        draft.ClientName = property.ClientName;
        draft.PhotoUrl = property.PhotoUrl;
        draft.UploadMethod = string.IsNullOrWhiteSpace(uploadMethod) ? "Pdf" : uploadMethod;

        if (reportFile != null && reportFile.Length > 0)
        {
            var ext = Path.GetExtension(reportFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext) || reportFile.Length > MaxFileBytes)
            {
                throw new InvalidOperationException("Invalid file.");
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
            draft.ReportPageCount = ext == ".pdf" ? 42 : 1;
        }
        else if (string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            draft.ReportFileName = "Home Inspection Report";
            draft.ReportFileUrl = "/welcome-house.png";
            draft.ReportPageCount = 42;
        }

        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.InProgress;
        draft.AnalysisProgress = 72;
        draft.CurrentStep = 2;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await SeedDemoFindingsIfNeededAsync(draft.Id, cancellationToken);
    }

    public async Task<RealtorInspectionAnalyzeViewModel> BuildAnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete upload first.");

        var progress = draft.AnalysisProgress;
        var status = draft.AnalysisStatus;

        return new RealtorInspectionAnalyzeViewModel
        {
            DisplayStep = 2,
            Title = "AI Analysis",
            Subtitle = "INDOR AI scans the inspection report to find, organize, and prioritize findings automatically.",
            PropertyDisplay = FormatDisplayAddress(draft.Address ?? "", draft.CityRegion),
            ReportFileName = draft.ReportFileName ?? "Home Inspection Report",
            ReportPageCount = draft.ReportPageCount > 0 ? draft.ReportPageCount : 42,
            UploadedLabel = $"Uploaded {draft.FechaCreacion.ToLocalTime():MMM d, yyyy}",
            AnalysisProgress = progress,
            AnalysisStatus = status,
            Tasks = BuildAnalysisTasks(progress, status),
            DetectedCategories = BuildDetectedCategories(progress)
        };
    }

    public async Task AdvanceAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        draft.AnalysisProgress = Math.Min(100, draft.AnalysisProgress + 28);
        if (draft.AnalysisProgress >= 100)
        {
            draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.Complete;
        }

        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await SeedDemoFindingsIfNeededAsync(draft.Id, cancellationToken);
    }

    public async Task CompleteAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        draft.AnalysisProgress = 100;
        draft.AnalysisStatus = RealtorInspectionAnalysisStatuses.Complete;
        draft.CurrentStep = 3;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        await SeedDemoFindingsIfNeededAsync(draft.Id, cancellationToken);
        await EnsureDefaultProvidersAsync(draft.Id, cancellationToken);
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
            Title = "Prioritized Findings",
            Subtitle = "INDOR AI has ordered the findings by urgency and assigned the recommended trade for each issue.",
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
            finding.IsSelected = selected.Count == 0 || selected.Contains(finding.Id);
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
        var allProviders = await db.IndorRealtorQuoteProviders.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

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
            var providers = MatchProvidersForTrade(trade, allProviders);
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
                Providers = providers.Select(p => new RealtorQuoteProviderCardViewModel
                {
                    Id = p.Id,
                    CompanyName = p.CompanyName,
                    Categories = p.Categories,
                    Rating = p.Rating,
                    DistanceMiles = p.DistanceMiles,
                    BadgeLabel = p.BadgeLabel,
                    IsVerified = p.IsVerified,
                    Selected = selectedForTrade.Contains(p.Id)
                }).ToList()
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

        var providerLookup = await db.IndorRealtorQuoteProviders.AsNoTracking()
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var requests = new List<RealtorInspectionReviewRequestViewModel>();
        foreach (var trade in trades)
        {
            var tradeMeta = RealtorInspectionTrades.All.FirstOrDefault(t => t.Value == trade);
            var tradeFindings = selectedFindings.Where(f => f.Trade == trade).ToList();
            var topPriority = tradeFindings.OrderByDescending(f => PriorityWeight(f.Priority)).First().Priority;
            var providerCount = providersByTrade.GetValueOrDefault(trade);
            if (providerCount == 0)
            {
                providerCount = MatchProvidersForTrade(trade, providerLookup.Values).Take(2).Count();
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
        var providerLookup = await db.IndorRealtorQuoteProviders.AsNoTracking()
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var property = await db.IndorRealtorPropertyFiles
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == draft.PropertyFileId && p.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Property not found.");

        if (!string.IsNullOrWhiteSpace(draft.ReportFileUrl))
        {
            db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
            {
                PropertyFileId = property.Id,
                CategoryType = RealtorPropertyFileCategoryTypes.InspectionReports,
                ItemLabel = draft.ReportFileName ?? "Inspection Report",
                FileUrl = draft.ReportFileUrl,
                UploadedUtc = DateTime.UtcNow
            });
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
            var tradeMeta = RealtorInspectionTrades.All.First(t => t.Value == trade);
            var tradeFindings = selectedFindings.Where(f => f.Trade == trade).ToList();
            var providerIds = providersByTrade.GetValueOrDefault(trade) ?? [];
            if (providerIds.Count == 0)
            {
                providerIds = MatchProvidersForTrade(trade, providerLookup.Values).Take(2).Select(p => p.Id).ToList();
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
                SentUtc = DateTime.UtcNow
            };

            db.IndorRealtorQuotes.Add(quote);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var providerId in providerIds)
            {
                if (providerLookup.TryGetValue(providerId, out var prov))
                {
                    db.IndorRealtorQuoteSentProviders.Add(new IndorRealtorQuoteSentProvider
                    {
                        QuoteId = quote.Id,
                        ProviderId = providerId,
                        ProviderName = prov.CompanyName
                    });
                }
            }

            quoteCodes.Add(quoteCode);
        }

        property.QuotesReceivedCount += quoteCodes.Count;

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "upload",
            Description = $"Inspection report analyzed for {draft.Address} — {quoteCodes.Count} quote requests created",
            CategoryTag = "Files",
            OccurredUtc = DateTime.UtcNow
        });

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "quote",
            Description = $"{selectedFindings.Count} findings sent to providers for {draft.Address}",
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

    private async Task SeedDemoFindingsIfNeededAsync(int draftId, CancellationToken cancellationToken)
    {
        var exists = await db.IndorRealtorInspectionUploadFindings
            .AnyAsync(f => f.DraftId == draftId, cancellationToken);
        if (exists)
        {
            return;
        }

        var demo = new (string Title, string Priority, string Trade, int Score, int Sort)[]
        {
            ("Exposed electrical wiring", RealtorInspectionFindingPriorities.Urgent, RealtorInspectionTrades.Electrical, 95, 1),
            ("Panel grounding issue", RealtorInspectionFindingPriorities.Urgent, RealtorInspectionTrades.Electrical, 92, 2),
            ("Missing GFCI outlet", RealtorInspectionFindingPriorities.Urgent, RealtorInspectionTrades.Electrical, 88, 3),
            ("A/C not cooling properly", RealtorInspectionFindingPriorities.High, RealtorInspectionTrades.Hvac, 87, 4),
            ("Dirty condenser coils", RealtorInspectionFindingPriorities.High, RealtorInspectionTrades.Hvac, 84, 5),
            ("Leaky pipe under sink", RealtorInspectionFindingPriorities.High, RealtorInspectionTrades.Plumbing, 83, 6),
            ("Water heater sediment buildup", RealtorInspectionFindingPriorities.High, RealtorInspectionTrades.Plumbing, 81, 7),
            ("Missing shingles on roof", RealtorInspectionFindingPriorities.High, RealtorInspectionTrades.Roof, 80, 8),
            ("Peeling exterior paint", RealtorInspectionFindingPriorities.Moderate, RealtorInspectionTrades.Paint, 72, 9),
            ("Loose handrail", RealtorInspectionFindingPriorities.Moderate, RealtorInspectionTrades.Electrical, 70, 10),
            ("Minor grout cracking", RealtorInspectionFindingPriorities.Moderate, RealtorInspectionTrades.Plumbing, 68, 11),
            ("Attic insulation gaps", RealtorInspectionFindingPriorities.Moderate, RealtorInspectionTrades.Hvac, 65, 12)
        };

        foreach (var item in demo)
        {
            var tradeMeta = RealtorInspectionTrades.All.First(t => t.Value == item.Trade);
            db.IndorRealtorInspectionUploadFindings.Add(new IndorRealtorInspectionUploadFinding
            {
                DraftId = draftId,
                Title = item.Title,
                Priority = item.Priority,
                Trade = item.Trade,
                TradeLabel = tradeMeta.Label,
                AiScore = item.Score,
                ImageUrl = "/welcome-house.png",
                SortOrder = item.Sort,
                IsSelected = item.Priority != RealtorInspectionFindingPriorities.Moderate || item.Sort <= 11
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultProvidersAsync(int draftId, CancellationToken cancellationToken)
    {
        var hasProviders = await db.IndorRealtorInspectionDraftProviders
            .AnyAsync(p => p.DraftId == draftId, cancellationToken);
        if (hasProviders)
        {
            return;
        }

        var draft = await db.IndorRealtorInspectionUploadDrafts
            .Include(d => d.Findings)
            .FirstAsync(d => d.Id == draftId, cancellationToken);

        var allProviders = await db.IndorRealtorQuoteProviders.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var trades = draft.Findings.Where(f => f.IsSelected).Select(f => f.Trade).Distinct();
        foreach (var trade in trades)
        {
            var matched = MatchProvidersForTrade(trade, allProviders);
            var take = trade is RealtorInspectionTrades.Electrical or RealtorInspectionTrades.Hvac ? 2 : 1;
            foreach (var prov in matched.Take(take))
            {
                db.IndorRealtorInspectionDraftProviders.Add(new IndorRealtorInspectionDraftProvider
                {
                    DraftId = draftId,
                    Trade = trade,
                    ProviderId = prov.Id
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static List<IndorRealtorQuoteProvider> MatchProvidersForTrade(
        string trade, IEnumerable<IndorRealtorQuoteProvider> providers)
    {
        var keywords = trade switch
        {
            RealtorInspectionTrades.Electrical => new[] { "electric", "electrical" },
            RealtorInspectionTrades.Hvac => new[] { "hvac", "cool", "mechanical" },
            RealtorInspectionTrades.Plumbing => new[] { "plumb" },
            RealtorInspectionTrades.Roof => new[] { "roof", "gutter" },
            RealtorInspectionTrades.Paint => new[] { "paint", "exterior" },
            _ => Array.Empty<string>()
        };

        var matched = providers
            .Where(p => keywords.Any(k =>
                p.Categories.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                p.CompanyName.Contains(k, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(p => p.IsRecommended)
            .ThenByDescending(p => p.Rating)
            .ToList();

        if (matched.Count == 0)
        {
            matched = providers.OrderByDescending(p => p.Rating).Take(2).ToList();
        }

        return matched;
    }

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
            Priority = f.Priority,
            PriorityCss = f.Priority.ToLowerInvariant(),
            TradeLabel = f.TradeLabel,
            TradeIcon = tradeMeta.Icon,
            AiScore = f.AiScore,
            ImageUrl = f.ImageUrl,
            IsSelected = f.IsSelected
        };
    }

    private static List<RealtorInspectionAnalyzeTaskViewModel> BuildAnalysisTasks(int progress, string status) =>
    [
        new() { Label = "Reading report pages", Detail = "42 / 42 pages", Status = progress >= 20 ? "Done" : "Pending" },
        new() { Label = "Extracting repair findings", Detail = "188 findings", Status = progress >= 50 ? "Done" : progress >= 20 ? "InProgress" : "Pending" },
        new() { Label = "Classifying by trade", Detail = "In progress", Status = progress >= 72 ? "InProgress" : "Pending" },
        new() { Label = "Scoring urgency", Detail = "Pending", Status = status == RealtorInspectionAnalysisStatuses.Complete ? "Done" : "Pending" }
    ];

    private static List<RealtorInspectionCategoryChipViewModel> BuildDetectedCategories(int progress)
    {
        if (progress < 50)
        {
            return [];
        }

        return
        [
            new() { Label = "Electrical", Css = "electrical", Icon = "fa-bolt" },
            new() { Label = "HVAC", Css = "hvac", Icon = "fa-fan" },
            new() { Label = "Plumbing", Css = "plumbing", Icon = "fa-faucet-drip" },
            new() { Label = "Roof", Css = "roof", Icon = "fa-house-chimney" },
            new() { Label = "Paint", Css = "paint", Icon = "fa-paint-roller" }
        ];
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
