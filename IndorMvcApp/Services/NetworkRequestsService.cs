using System.Text.Json;
using System.Text.RegularExpressions;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed partial class NetworkRequestsService(
    AppDbContext db,
    ILogger<NetworkRequestsService> logger) : INetworkRequestsService
{
    private static readonly string[] VerifiedStatuses =
    [
        ProviderRegistrationStatuses.IndorProActive,
        ProviderRegistrationStatuses.Approved
    ];

    // ---------------------------------------------------------------- List

    public async Task<MyRequestsViewModel> GetMyRequestsAsync(
        IndorProveedor me, string? tab, string? query, CancellationToken cancellationToken = default)
    {
        var normalizedTab = string.IsNullOrWhiteSpace(tab) ? "all" : tab.Trim().ToLowerInvariant();
        var normalizedQuery = query?.Trim();

        var jobs = await LoadPosterJobsAsync(me.Id, cancellationToken);
        var catalog = await LoadCatalogAsync(cancellationToken);
        var quoteCounts = await LoadQuoteCountsAsync(jobs.Select(j => j.Id), cancellationToken);
        var hiredJobIds = await LoadHiredJobIdsAsync(me.Id, cancellationToken);

        var items = jobs
            .Select(j => BuildListItem(j, catalog, quoteCounts.GetValueOrDefault(j.Id), hiredJobIds.Contains(j.Id)))
            .ToList();

        var filtered = items.Where(i => MatchesTab(i, normalizedTab));
        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filtered = filtered.Where(i =>
                i.Title.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                || (i.TradeLabel?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return new MyRequestsViewModel
        {
            ActiveTab = normalizedTab,
            Query = normalizedQuery,
            Tabs =
            [
                new() { Id = "all", Label = "All" },
                new() { Id = "open", Label = "Open" },
                new() { Id = "quotes", Label = "Quotes" },
                new() { Id = "inprogress", Label = "In Progress" },
                new() { Id = "completed", Label = "Completed" }
            ],
            Requests = filtered.ToList()
        };
    }

    // ---------------------------------------------------------------- Details

    public async Task<RequestDetailsViewModel?> GetDetailsAsync(
        IndorProveedor me, int jobId, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(jobId, me.Id, cancellationToken);
        if (job == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var isHired = await LoadHiredJobIdsAsync(me.Id, cancellationToken) is var hired && hired.Contains(job.Id);

        var quotes = await EnsureQuotesAsync(job, cancellationToken);
        var quoteVms = await BuildQuoteVmsAsync(quotes, catalog, cancellationToken);
        quoteVms = quoteVms.OrderBy(q => q.QuotedAmount).ToList();

        var (icon, tone) = TradeIcon(job, catalog);
        var hasQuotes = quoteVms.Count > 0;

        return new RequestDetailsViewModel
        {
            Id = job.Id,
            Title = JobTitle(job),
            TradeLabel = job.TradeLabel,
            IconClass = icon,
            IconTone = tone,
            LocationLabel = job.Location,
            BudgetRange = job.BudgetRange,
            PostedLabel = PostedLabel(job.FechaCreacion),
            StatusLabel = StatusLabel(job, hasQuotes, isHired),
            StatusKind = StatusKind(job, hasQuotes, isHired),
            Photos = ParsePhotos(job),
            Steps = BuildSteps(hasQuotes, isHired, IsDone(job)),
            Activity = BuildActivity(job, quotes),
            Quotes = quoteVms,
            IsHired = isHired
        };
    }

    // ---------------------------------------------------------------- Compare

    public async Task<CompareQuotesViewModel?> GetCompareAsync(
        IndorProveedor me, int jobId, string? sort, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(jobId, me.Id, cancellationToken);
        if (job == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var quotes = await EnsureQuotesAsync(job, cancellationToken);
        var quoteVms = await BuildQuoteVmsAsync(quotes, catalog, cancellationToken);

        var normalizedSort = string.IsNullOrWhiteSpace(sort) ? "price" : sort.Trim().ToLowerInvariant();
        quoteVms = normalizedSort switch
        {
            "rating" => quoteVms.OrderByDescending(q => q.Rating ?? 0).ThenByDescending(q => q.ReviewCount).ToList(),
            "fastest" => quoteVms.OrderBy(q => q.ResponseMinutes).ToList(),
            _ => quoteVms.OrderBy(q => q.QuotedAmount).ToList()
        };

        if (quoteVms.Count > 0)
        {
            quoteVms[0].IsBestMatch = true;
        }

        var (icon, tone) = TradeIcon(job, catalog);

        return new CompareQuotesViewModel
        {
            Id = job.Id,
            Title = JobTitle(job),
            TradeLabel = job.TradeLabel,
            IconClass = icon,
            IconTone = tone,
            LocationLabel = job.Location,
            BudgetRange = job.BudgetRange,
            PostedLabel = PostedLabel(job.FechaCreacion),
            Sort = normalizedSort,
            Quotes = quoteVms
        };
    }

    // ---------------------------------------------------------------- Select / hire

    public async Task<bool> SelectQuoteAsync(
        IndorProveedor me, int jobId, int quoteId, CancellationToken cancellationToken = default)
    {
        var job = await db.IndorProveedorNetworkJobs
            .FirstOrDefaultAsync(j => j.Id == jobId && j.PosterProveedorId == me.Id, cancellationToken);
        if (job == null)
        {
            return false;
        }

        var quotes = await db.IndorProveedorNetworkQuotes
            .Where(q => q.NetworkJobId == jobId)
            .ToListAsync(cancellationToken);
        var chosen = quotes.FirstOrDefault(q => q.Id == quoteId);
        if (chosen == null)
        {
            return false;
        }

        foreach (var q in quotes)
        {
            q.Status = q.Id == quoteId ? NetworkQuoteStatuses.Selected : NetworkQuoteStatuses.Declined;
        }

        job.Status = NetworkJobStatuses.Hired;
        job.FechaActualizacion = DateTime.UtcNow;

        var existingHire = await db.IndorProveedorNetworkHires
            .FirstOrDefaultAsync(h => h.NetworkJobId == jobId, cancellationToken);

        var scheduled = ScheduledSlot();
        if (existingHire == null)
        {
            db.IndorProveedorNetworkHires.Add(new IndorProveedorNetworkHire
            {
                HirerProveedorId = me.Id,
                SubcontractorProveedorId = chosen.SubcontractorProveedorId,
                NetworkJobId = jobId,
                ProjectTitle = JobTitle(job),
                TradeLabel = job.TradeLabel,
                BudgetRange = $"${chosen.QuotedAmount:0}",
                StartDate = scheduled,
                Status = NetworkHireStatuses.Hired,
                FechaCreacion = DateTime.UtcNow
            });
        }
        else
        {
            existingHire.SubcontractorProveedorId = chosen.SubcontractorProveedorId;
            existingHire.ProjectTitle = JobTitle(job);
            existingHire.BudgetRange = $"${chosen.QuotedAmount:0}";
            existingHire.StartDate = scheduled;
            existingHire.Status = NetworkHireStatuses.Hired;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ---------------------------------------------------------------- Confirmed

    public async Task<RequestConfirmedViewModel?> GetConfirmedAsync(
        IndorProveedor me, int jobId, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(jobId, me.Id, cancellationToken);
        if (job == null)
        {
            return null;
        }

        var hire = await SafeAsync(() => db.IndorProveedorNetworkHires
            .AsNoTracking()
            .Where(h => h.NetworkJobId == jobId)
            .OrderByDescending(h => h.Id)
            .FirstOrDefaultAsync(cancellationToken));
        if (hire == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var sub = await LoadProviderAsync(hire.SubcontractorProveedorId, cancellationToken);
        var selectedQuote = await SafeAsync(() => db.IndorProveedorNetworkQuotes
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.NetworkJobId == jobId && q.SubcontractorProveedorId == hire.SubcontractorProveedorId, cancellationToken));

        var (icon, tone) = TradeIcon(job, catalog);
        var docsReady = sub?.Documentos.Any(d => d.UploadedUtc != null) ?? false;
        var amount = selectedQuote?.QuotedAmount ?? 0m;

        return new RequestConfirmedViewModel
        {
            JobId = job.Id,
            Title = JobTitle(job),
            TradeLabel = job.TradeLabel,
            IconClass = icon,
            IconTone = tone,
            LocationLabel = job.Location,
            SubcontractorId = hire.SubcontractorProveedorId,
            ContractorName = sub != null ? ResolveName(sub) : (hire.ProjectTitle ?? "Selected pro"),
            IsVerified = sub != null && IsVerified(sub),
            IsInsured = sub?.IsInsured ?? false,
            IsDocsReady = docsReady,
            QuoteLabel = amount > 0 ? $"${amount:0}" : (hire.BudgetRange ?? "—"),
            ScheduledLabel = hire.StartDate.HasValue ? FriendlyDateTime(hire.StartDate.Value) : "To be scheduled",
            AddressLine = job.Location,
            DurationLabel = "1 – 2 hours",
            Latitude = job.Latitude,
            Longitude = job.Longitude,
            Steps = BuildSteps(hasQuotes: true, isHired: true, isDone: false)
        };
    }

    // ---------------------------------------------------------------- Quote generation

    private async Task<List<IndorProveedorNetworkQuote>> EnsureQuotesAsync(IndorProveedorNetworkJob job, CancellationToken ct)
    {
        if (string.Equals(job.Status, NetworkJobStatuses.Draft, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        List<IndorProveedorNetworkQuote> existing;
        try
        {
            existing = await db.IndorProveedorNetworkQuotes
                .Where(q => q.NetworkJobId == job.Id)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        if (existing.Count > 0)
        {
            return existing;
        }

        var candidates = await LoadCandidatesAsync(job, ct);
        if (candidates.Count == 0)
        {
            return [];
        }

        var (low, high, hasBudget) = ParseBudget(job.BudgetRange);
        var created = new List<IndorProveedorNetworkQuote>();
        for (var i = 0; i < candidates.Count; i++)
        {
            var p = candidates[i];
            var factor = 0.2 + (0.28 * i);
            var quoted = RoundTo((decimal)(low + ((high - low) * factor)), 5);
            var rangeLow = RoundTo(quoted * 0.9m, 5);
            var rangeHigh = RoundTo(quoted * 1.3m, 5);
            var responseMinutes = p.EmergencyService ? 30 : p.SameDayJobs ? 60 : 90;

            created.Add(new IndorProveedorNetworkQuote
            {
                NetworkJobId = job.Id,
                SubcontractorProveedorId = p.Id,
                AmountLow = rangeLow,
                AmountHigh = rangeHigh,
                QuotedAmount = quoted,
                ResponseMinutes = responseMinutes,
                Message = QuotePitch(p),
                Status = NetworkQuoteStatuses.Pending,
                FechaCreacion = DateTime.UtcNow
            });
        }

        try
        {
            db.IndorProveedorNetworkQuotes.AddRange(created);
            if (string.Equals(job.Status, NetworkJobStatuses.Open, StringComparison.OrdinalIgnoreCase))
            {
                var tracked = await db.IndorProveedorNetworkJobs.FirstOrDefaultAsync(j => j.Id == job.Id, ct);
                if (tracked != null)
                {
                    tracked.Status = NetworkJobStatuses.Matched;
                    tracked.FechaActualizacion = DateTime.UtcNow;
                    job.Status = NetworkJobStatuses.Matched;
                }
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            logger.LogWarning(ex, "Network quotes table missing; skipping quote generation.");
            return [];
        }

        return created;
    }

    private async Task<List<IndorProveedor>> LoadCandidatesAsync(IndorProveedorNetworkJob job, CancellationToken ct)
    {
        try
        {
            var all = await db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Include(p => p.Documentos)
                .Where(p => p.Id != job.PosterProveedorId)
                .Where(p => VerifiedStatuses.Contains(p.RegistrationStatus))
                .Take(60)
                .ToListAsync(ct);

            var sameTrade = string.IsNullOrWhiteSpace(job.TradeId)
                ? []
                : all.Where(p => p.Categorias.Any(c => string.Equals(c.CategoriaId, job.TradeId, StringComparison.OrdinalIgnoreCase))).ToList();

            var ordered = sameTrade
                .Concat(all.Where(p => !sameTrade.Contains(p)))
                .Take(3)
                .ToList();

            return ordered;
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<List<NetworkQuoteViewModel>> BuildQuoteVmsAsync(
        List<IndorProveedorNetworkQuote> quotes,
        IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog,
        CancellationToken ct)
    {
        if (quotes.Count == 0)
        {
            return [];
        }

        var subIds = quotes.Select(q => q.SubcontractorProveedorId).Distinct().ToList();
        var providers = await SafeAsync(() => db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .Include(p => p.Documentos)
            .Where(p => subIds.Contains(p.Id))
            .ToListAsync(ct)) ?? [];
        var providerMap = providers.ToDictionary(p => p.Id, p => p);

        var ratings = await LoadRatingsAsync(subIds, ct);
        var jobCounts = await LoadJobCountsAsync(subIds, ct);

        var result = new List<NetworkQuoteViewModel>();
        foreach (var q in quotes)
        {
            if (!providerMap.TryGetValue(q.SubcontractorProveedorId, out var p))
            {
                continue;
            }

            var primaryTrade = p.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
            catalog.TryGetValue(primaryTrade ?? "", out var cat);
            ratings.TryGetValue(p.Id, out var rating);
            jobCounts.TryGetValue(p.Id, out var jobsCompleted);

            result.Add(new NetworkQuoteViewModel
            {
                Id = q.Id,
                SubcontractorId = p.Id,
                Name = ResolveName(p),
                IconClass = cat?.IconClass ?? "fa-screwdriver-wrench",
                PhotoUrl = LogoUrl(p),
                LocationLabel = ComposeLocation(p),
                Rating = rating.Count > 0 ? rating.Avg : null,
                ReviewCount = rating.Count,
                JobsCompleted = jobsCompleted,
                ResponseLabel = ResponseLabel(q.ResponseMinutes),
                ResponseMinutes = q.ResponseMinutes,
                IsVerified = IsVerified(p),
                IsInsured = p.IsInsured,
                IsDocsReady = p.Documentos.Any(d => d.UploadedUtc != null),
                RangeLabel = $"${q.AmountLow:0} – ${q.AmountHigh:0}",
                AmountLabel = $"${q.QuotedAmount:0}",
                QuotedAmount = q.QuotedAmount,
                WithinBudget = true,
                Message = q.Message,
                IsSelected = string.Equals(q.Status, NetworkQuoteStatuses.Selected, StringComparison.OrdinalIgnoreCase)
            });
        }

        return result;
    }

    // ---------------------------------------------------------------- Builders

    private RequestListItemViewModel BuildListItem(
        IndorProveedorNetworkJob job,
        IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog,
        int quoteCount,
        bool isHired)
    {
        var hasQuotes = quoteCount > 0;
        var (icon, tone) = TradeIcon(job, catalog);

        return new RequestListItemViewModel
        {
            Id = job.Id,
            Title = JobTitle(job),
            TradeLabel = job.TradeLabel,
            IconClass = icon,
            IconTone = tone,
            LocationLabel = job.Location,
            BudgetRange = job.BudgetRange,
            PostedLabel = PostedLabel(job.FechaCreacion),
            StatusLabel = StatusLabel(job, hasQuotes, isHired),
            StatusKind = StatusKind(job, hasQuotes, isHired),
            QuoteCount = quoteCount,
            Steps = BuildSteps(hasQuotes, isHired, IsDone(job))
        };
    }

    private static List<RequestStepViewModel> BuildSteps(bool hasQuotes, bool isHired, bool isDone)
    {
        // "Quotes" is active while waiting and done once a hire is made.
        var quotesState = isHired || isDone ? "done" : "active";

        return
        [
            new() { Label = "Posted", IconClass = "fa-check", State = "done" },
            new() { Label = "Quotes", IconClass = "fa-quote-right", State = quotesState },
            new() { Label = "Hired", IconClass = "fa-user", State = isDone ? "done" : isHired ? "active" : "pending" },
            new() { Label = "Done", IconClass = "fa-check", State = isDone ? "active" : "pending" }
        ];
    }

    private List<RequestActivityViewModel> BuildActivity(IndorProveedorNetworkJob job, List<IndorProveedorNetworkQuote> quotes)
    {
        var list = new List<RequestActivityViewModel>
        {
            new()
            {
                IconClass = "fa-pen-to-square",
                Title = "Request posted",
                Detail = "You posted this request.",
                TimeLabel = FriendlyDateTime(job.FechaCreacion),
                Tone = "blue"
            }
        };

        if (quotes.Count > 0)
        {
            list.Add(new RequestActivityViewModel
            {
                IconClass = "fa-eye",
                Title = $"{quotes.Count} contractors viewed this request",
                Detail = "They are reviewing your request.",
                TimeLabel = FriendlyDateTime(job.FechaCreacion.AddMinutes(16)),
                Tone = "eye"
            });

            var latest = quotes.Max(q => q.FechaCreacion);
            list.Add(new RequestActivityViewModel
            {
                IconClass = "fa-quote-right",
                Title = $"{quotes.Count} quotes received",
                Detail = "Compare quotes and hire the best fit.",
                TimeLabel = FriendlyDateTime(latest),
                Tone = "quote"
            });
        }

        return list;
    }

    // ---------------------------------------------------------------- Loaders

    private async Task<List<IndorProveedorNetworkJob>> LoadPosterJobsAsync(int posterId, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorNetworkJobs
                .AsNoTracking()
                .Where(j => j.PosterProveedorId == posterId && j.Status != NetworkJobStatuses.Draft)
                .OrderByDescending(j => j.FechaCreacion)
                .Take(100)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<IndorProveedorNetworkJob?> LoadJobAsync(int jobId, int posterId, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorNetworkJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == jobId && j.PosterProveedorId == posterId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }

    private async Task<IndorProveedor?> LoadProviderAsync(int id, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Include(p => p.Documentos)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }

    private async Task<Dictionary<string, IndorProveedorCategoriaCatalogo>> LoadCatalogAsync(CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorCategoriasCatalogo.AsNoTracking().Where(c => c.Activo).ToDictionaryAsync(c => c.Id, c => c, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<Dictionary<int, int>> LoadQuoteCountsAsync(IEnumerable<int> jobIds, CancellationToken ct)
    {
        var ids = jobIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        try
        {
            var grouped = await db.IndorProveedorNetworkQuotes
                .AsNoTracking()
                .Where(q => ids.Contains(q.NetworkJobId))
                .GroupBy(q => q.NetworkJobId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);
            return grouped.ToDictionary(g => g.Key, g => g.Count);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<HashSet<int>> LoadHiredJobIdsAsync(int hirerId, CancellationToken ct)
    {
        try
        {
            var ids = await db.IndorProveedorNetworkHires
                .AsNoTracking()
                .Where(h => h.HirerProveedorId == hirerId && h.NetworkJobId != null)
                .Select(h => h.NetworkJobId!.Value)
                .ToListAsync(ct);
            return ids.ToHashSet();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<Dictionary<int, (decimal Avg, int Count)>> LoadRatingsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        try
        {
            var grouped = await db.IndorProveedorNetworkResenas
                .AsNoTracking()
                .Where(r => idList.Contains(r.SubcontractorProveedorId))
                .GroupBy(r => r.SubcontractorProveedorId)
                .Select(g => new { g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
                .ToListAsync(ct);
            return grouped.ToDictionary(g => g.Key, g => (Avg: Math.Round((decimal)g.Avg, 1), Count: g.Count));
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<Dictionary<int, int>> LoadJobCountsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        try
        {
            var grouped = await db.IndorProveedorNetworkHires
                .AsNoTracking()
                .Where(h => idList.Contains(h.SubcontractorProveedorId))
                .GroupBy(h => h.SubcontractorProveedorId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);
            return grouped.ToDictionary(g => g.Key, g => g.Count);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<T?> SafeAsync<T>(Func<Task<T>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return default;
        }
    }

    // ---------------------------------------------------------------- Helpers

    private static bool MatchesTab(RequestListItemViewModel item, string tab) => tab switch
    {
        "open" => item.StatusKind == "amber",
        "quotes" => item.StatusKind == "green",
        "inprogress" => item.StatusKind == "purple",
        "completed" => item.StatusKind == "grey" && item.StatusLabel == "Completed",
        _ => true
    };

    private static string StatusLabel(IndorProveedorNetworkJob job, bool hasQuotes, bool isHired)
    {
        if (IsDone(job)) return "Completed";
        if (isHired) return "In Progress";
        if (hasQuotes) return "Quotes Received";
        return "Waiting for Quotes";
    }

    private static string StatusKind(IndorProveedorNetworkJob job, bool hasQuotes, bool isHired)
    {
        if (IsDone(job)) return "grey";
        if (isHired) return "purple";
        if (hasQuotes) return "green";
        return "amber";
    }

    private static bool IsDone(IndorProveedorNetworkJob job) =>
        string.Equals(job.Status, NetworkJobStatuses.Closed, StringComparison.OrdinalIgnoreCase);

    private static (string Icon, string Tone) TradeIcon(
        IndorProveedorNetworkJob job, IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog)
    {
        if (!string.IsNullOrWhiteSpace(job.TradeId) && catalog.TryGetValue(job.TradeId, out var cat))
        {
            return (cat.IconClass, ToneForTrade(job.TradeId));
        }

        return ("fa-screwdriver-wrench", "blue");
    }

    private static string ToneForTrade(string? tradeId) => (tradeId ?? "").ToLowerInvariant() switch
    {
        var t when t.Contains("electr") => "amber",
        var t when t.Contains("plumb") => "blue",
        var t when t.Contains("hand") => "purple",
        var t when t.Contains("hvac") => "cyan",
        _ => "blue"
    };

    private static string JobTitle(IndorProveedorNetworkJob job) =>
        !string.IsNullOrWhiteSpace(job.JobTitle) ? job.JobTitle!.Trim()
        : !string.IsNullOrWhiteSpace(job.Description) ? Shorten(job.Description!, 60)
        : "Job request";

    private static string Shorten(string text, int max)
    {
        text = text.Trim();
        return text.Length <= max ? text : text[..max].TrimEnd() + "…";
    }

    private static List<string> ParsePhotos(IndorProveedorNetworkJob job)
    {
        if (!string.IsNullOrWhiteSpace(job.PhotoUrlsJson))
        {
            try
            {
                var urls = JsonSerializer.Deserialize<List<string>>(job.PhotoUrlsJson);
                if (urls is { Count: > 0 })
                {
                    return urls.Where(u => !string.IsNullOrWhiteSpace(u)).ToList();
                }
            }
            catch (JsonException)
            {
                // fall through
            }
        }

        return string.IsNullOrWhiteSpace(job.PhotoUrl) ? [] : [job.PhotoUrl!];
    }

    private static (double Low, double High, bool HasBudget) ParseBudget(string? budgetRange)
    {
        if (string.IsNullOrWhiteSpace(budgetRange))
        {
            return (200, 800, false);
        }

        var numbers = DigitsRegex()
            .Matches(budgetRange.Replace(",", ""))
            .Select(m => double.TryParse(m.Value, out var n) ? n : 0)
            .Where(n => n > 0)
            .ToList();

        return numbers.Count switch
        {
            0 => (200, 800, false),
            1 when budgetRange.Contains('+') => (numbers[0], numbers[0] * 2.5, true),
            1 when budgetRange.Contains("Under", StringComparison.OrdinalIgnoreCase) => (numbers[0] * 0.3, numbers[0], true),
            1 => (numbers[0] * 0.6, numbers[0], true),
            _ => (numbers.Min(), numbers.Max(), true)
        };
    }

    private static decimal RoundTo(decimal value, int nearest) =>
        Math.Max(nearest, Math.Round(value / nearest) * nearest);

    private static string ResponseLabel(int minutes)
    {
        if (minutes < 60) return $"Responds in {minutes} min";
        if (minutes < 120) return "Responds in 1 hr";
        return $"Responds in {minutes / 60} hr";
    }

    private static string PostedLabel(DateTime createdUtc)
    {
        var days = (DateTime.UtcNow.Date - createdUtc.ToLocalTime().Date).Days;
        return days switch
        {
            <= 0 => "Today",
            1 => "1 day ago",
            _ => $"{days} days ago"
        };
    }

    private static string FriendlyDateTime(DateTime utc)
    {
        var local = utc.ToLocalTime();
        var days = (DateTime.Now.Date - local.Date).Days;
        var time = local.ToString("h:mm tt");
        return days switch
        {
            0 => $"Today, {time}",
            1 => $"Yesterday, {time}",
            -1 => $"Tomorrow, {time}",
            _ => local.ToString("MMM d, h:mm tt")
        };
    }

    private static DateTime ScheduledSlot()
    {
        var today = DateTime.Now.Date.AddHours(16);
        var slot = DateTime.Now.Hour >= 15 ? today.AddDays(1) : today;
        return slot.ToUniversalTime();
    }

    private static string QuotePitch(IndorProveedor p)
    {
        if (!string.IsNullOrWhiteSpace(p.ServiceDescription))
        {
            return Shorten(p.ServiceDescription!, 120);
        }

        return "Experienced team. Quality work and clean, reliable service you can trust.";
    }

    private static bool IsVerified(IndorProveedor p) =>
        string.Equals(p.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
        || string.Equals(p.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);

    private static string ResolveName(IndorProveedor p) =>
        !string.IsNullOrWhiteSpace(p.DbaName) ? p.DbaName!.Trim()
        : !string.IsNullOrWhiteSpace(p.BusinessName) ? p.BusinessName!.Trim()
        : !string.IsNullOrWhiteSpace(p.PrimaryContact) ? p.PrimaryContact!.Trim()
        : "INDOR Provider";

    private static string? LogoUrl(IndorProveedor p) =>
        p.Documentos.FirstOrDefault(d =>
            string.Equals(d.DocumentType, ProviderDocumentTypes.Logo, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(d.FileUrl))?.FileUrl;

    private static string? ComposeLocation(IndorProveedor p)
    {
        if (!string.IsNullOrWhiteSpace(p.PrimaryCity))
        {
            return p.PrimaryCity!.Trim();
        }

        return string.IsNullOrWhiteSpace(p.BusinessAddress) ? null : p.BusinessAddress!.Trim();
    }

    [GeneratedRegex(@"\d+(\.\d+)?")]
    private static partial Regex DigitsRegex();
}
