using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed class ProviderNetworkService(
    AppDbContext db,
    ILogger<ProviderNetworkService> logger) : IProviderNetworkService
{
    private static readonly string[] ActiveStatuses =
    [
        ProviderRegistrationStatuses.IndorProActive,
        ProviderRegistrationStatuses.Approved
    ];

    // ---------------------------------------------------------------- Home

    public async Task<NetworkHomeViewModel> GetHomeAsync(IndorProveedor me, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var providers = await LoadActiveProvidersAsync(me.Id, cancellationToken);
        var ratings = await LoadRatingsAsync(providers.Select(p => p.Id), cancellationToken);
        var jobCounts = await LoadJobCountsAsync(providers.Select(p => p.Id), cancellationToken);

        var cards = providers
            .Select(p => BuildCard(p, catalog, ratings, jobCounts, me))
            .OrderByDescending(c => c.IsRecommended)
            .ThenByDescending(c => c.Rating ?? 0)
            .ThenByDescending(c => c.ReviewCount)
            .ToList();

        var myRequests = await SafeCountAsync(
            () => db.IndorProveedorNetworkJobs.CountAsync(j => j.PosterProveedorId == me.Id, cancellationToken));
        var activeHires = await SafeCountAsync(
            () => db.IndorProveedorNetworkHires.CountAsync(h => h.HirerProveedorId == me.Id, cancellationToken));

        return new NetworkHomeViewModel
        {
            CompanyName = ResolveMyName(me),
            VerifiedSubcontractorsCount = cards.Count(c => c.IsVerified),
            InsuredCount = cards.Count(c => c.IsInsured),
            MyRequestsCount = myRequests,
            ActiveHiresCount = activeHires,
            TradeChips = BuildTradeChips(catalog, providers),
            FeaturedSubcontractors = cards.Take(6).ToList()
        };
    }

    // ---------------------------------------------------------------- Find

    public async Task<FindSubcontractorsViewModel> GetFindAsync(
        IndorProveedor me,
        string? query,
        string? trade,
        string? view,
        bool nearby,
        bool insuredOnly,
        bool availableNow,
        bool docsReady,
        string? mode = null,
        CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var providers = await LoadActiveProvidersAsync(me.Id, cancellationToken);
        var ratings = await LoadRatingsAsync(providers.Select(p => p.Id), cancellationToken);
        var jobCounts = await LoadJobCountsAsync(providers.Select(p => p.Id), cancellationToken);

        trade = string.IsNullOrWhiteSpace(trade) ? "all" : trade.Trim();
        view = string.Equals(view, "map", StringComparison.OrdinalIgnoreCase) ? "map" : "list";
        var normalizedMode = NormalizeFindMode(mode);
        var normalizedQuery = query?.Trim();

        var filtered = providers.AsEnumerable();

        if (!string.Equals(trade, "all", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p => p.Categorias.Any(c =>
                string.Equals(c.CategoriaId, trade, StringComparison.OrdinalIgnoreCase)));
        }

        if (insuredOnly)
        {
            filtered = filtered.Where(p => p.IsInsured);
        }

        if (availableNow)
        {
            filtered = filtered.Where(p => p.SameDayJobs);
        }

        if (docsReady)
        {
            filtered = filtered.Where(p => p.Documentos.Any(d => d.UploadedUtc != null));
        }

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filtered = filtered.Where(p =>
                Contains(p.BusinessName, normalizedQuery) ||
                Contains(p.DbaName, normalizedQuery) ||
                Contains(p.ServiceDescription, normalizedQuery) ||
                Contains(p.PrimaryCity, normalizedQuery) ||
                p.Categorias.Any(c => catalog.TryGetValue(c.CategoriaId, out var cat) && Contains(cat.LabelEn, normalizedQuery)));
        }

        var cards = filtered.Select(p => BuildCard(p, catalog, ratings, jobCounts, me)).ToList();

        var hasLocation = me.Latitude.HasValue && me.Longitude.HasValue;
        if (nearby && hasLocation)
        {
            cards = cards.OrderBy(c => c.DistanceLabel == null ? double.MaxValue : ParseMiles(c.DistanceLabel)).ToList();
        }
        else if (normalizedMode == "services")
        {
            // Trade/service browse: group by primary trade label, then rating.
            cards = cards
                .OrderBy(c => c.TradeLabel ?? "zzz", StringComparer.OrdinalIgnoreCase)
                .ThenByDescending(c => c.Rating ?? 0)
                .ThenByDescending(c => c.ReviewCount)
                .ToList();
        }
        else
        {
            cards = cards
                .OrderByDescending(c => c.IsRecommended)
                .ThenByDescending(c => c.Rating ?? 0)
                .ThenByDescending(c => c.ReviewCount)
                .ToList();
        }

        return new FindSubcontractorsViewModel
        {
            CompanyName = ResolveMyName(me),
            Mode = normalizedMode,
            Query = normalizedQuery,
            ActiveTrade = trade,
            ActiveView = view,
            FilterNearby = nearby,
            FilterInsuredOnly = insuredOnly,
            FilterAvailableNow = availableNow,
            FilterDocsReady = docsReady,
            TradeChips = BuildTradeChips(catalog, providers),
            Results = cards,
            SortLabel = nearby && hasLocation
                ? "Nearest"
                : normalizedMode == "services" ? "By Trade" : "Best Match",
            HasLocation = hasLocation
        };
    }

    private static string NormalizeFindMode(string? mode)
    {
        if (string.Equals(mode, "verified", StringComparison.OrdinalIgnoreCase))
        {
            return "verified";
        }

        if (string.Equals(mode, "services", StringComparison.OrdinalIgnoreCase))
        {
            return "services";
        }

        return "find";
    }

    // ---------------------------------------------------------------- Profile

    public async Task<SubcontractorProfileViewModel?> GetProfileAsync(
        IndorProveedor me,
        int subcontractorId,
        CancellationToken cancellationToken = default)
    {
        var provider = await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .Include(p => p.Documentos)
            .FirstOrDefaultAsync(p => p.Id == subcontractorId, cancellationToken);

        if (provider == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var offerCatalog = await LoadOfferCatalogAsync(cancellationToken);
        var ratings = await LoadRatingsAsync([provider.Id], cancellationToken);
        var reviews = await LoadReviewsAsync(provider.Id, cancellationToken);

        var primaryTrade = provider.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        catalog.TryGetValue(primaryTrade ?? "", out var primaryCat);

        var services = provider.Categorias
            .Select(c => catalog.TryGetValue(c.CategoriaId, out var cat) ? cat.LabelEn : null)
            .Concat(provider.Ofertas.Select(o => offerCatalog.TryGetValue(o.OfertaId, out var of) ? of.LabelEn : null))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!)
            .Distinct()
            .ToList();

        if (services.Count == 0 && !string.IsNullOrWhiteSpace(provider.ServiceDescription))
        {
            services.Add(provider.ServiceDescription!);
        }

        ratings.TryGetValue(provider.Id, out var rating);
        var docsReady = provider.Documentos.Any(d => d.UploadedUtc != null);
        var (avgRating, reviewCount) = rating;

        var isSaved = await SafeAnyAsync(() => db.IndorProveedorNetworkGuardados
            .AnyAsync(g => g.OwnerProveedorId == me.Id && g.SubcontractorProveedorId == provider.Id, cancellationToken));

        return new SubcontractorProfileViewModel
        {
            Id = provider.Id,
            Name = ResolveName(provider),
            TradeLabel = primaryCat?.LabelEn ?? provider.ServiceDescription,
            AvatarInitial = InitialOf(ResolveName(provider)),
            PhotoUrl = LogoUrl(provider),
            IconClass = primaryCat?.IconClass ?? "fa-screwdriver-wrench",
            Rating = reviewCount > 0 ? avgRating : null,
            ReviewCount = reviewCount,
            LocationLabel = ComposeLocation(provider),
            DistanceLabel = DistanceLabel(me, provider),
            IsVerified = IsVerified(provider),
            IsInsured = provider.IsInsured,
            IsDocsReady = docsReady,
            InsuranceStatusLabel = provider.IsInsured ? "General Liability Insurance" : "Insurance not on file",
            InsuranceActive = provider.IsInsured,
            LicenseNumber = provider.LicenseNumber,
            LicenseVerified = provider.IsLicensed && !string.IsNullOrWhiteSpace(provider.LicenseNumber),
            Services = services,
            AvailabilityLabel = AvailabilityLabel(provider),
            IsAvailableNow = provider.SameDayJobs,
            ResponseTimeLabel = ResponseTimeLabel(provider),
            RecentJobsCount = await SafeCountAsync(() => db.IndorProveedorNetworkHires
                .CountAsync(h => h.SubcontractorProveedorId == provider.Id, cancellationToken)),
            Reviews = reviews,
            RatingBreakdown = BuildRatingBreakdown(reviews),
            Phone = provider.Phone,
            Email = provider.Email,
            IsSaved = isSaved
        };
    }

    // ---------------------------------------------------------------- Post job (wizard)

    public async Task<PostJobDetailsViewModel> GetDetailsAsync(IndorProveedor me, int? draftId, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var options = catalog.Values
            .OrderBy(c => c.SortOrder)
            .Select(c => new NetworkTradeChipViewModel
            {
                Id = c.Id,
                Label = c.LabelEn,
                LabelEs = c.LabelEs,
                IconClass = c.IconClass
            })
            .ToList();

        var draft = draftId.HasValue ? await LoadDraftAsync(me.Id, draftId.Value, cancellationToken) : null;

        return new PostJobDetailsViewModel
        {
            DraftId = draft?.Id,
            TradeOptions = options,
            SelectedTradeId = draft?.TradeId,
            JobTitle = draft?.JobTitle,
            Description = draft?.Description,
            Urgency = draft?.Urgency,
            Photos = ParsePhotos(draft?.PhotoUrlsJson)
        };
    }

    public async Task<int> SaveDetailsAsync(
        int posterProveedorId,
        PostJobDetailsInput input,
        List<string> newPhotoUrls,
        CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        string? tradeLabel = null;
        if (!string.IsNullOrWhiteSpace(input.TradeId) && catalog.TryGetValue(input.TradeId, out var cat))
        {
            tradeLabel = cat.LabelEn;
        }

        var draft = input.DraftId.HasValue
            ? await db.IndorProveedorNetworkJobs.FirstOrDefaultAsync(
                j => j.Id == input.DraftId.Value && j.PosterProveedorId == posterProveedorId, cancellationToken)
            : null;

        var isNew = draft == null;
        draft ??= new IndorProveedorNetworkJob
        {
            PosterProveedorId = posterProveedorId,
            Status = NetworkJobStatuses.Draft,
            FechaCreacion = DateTime.UtcNow
        };

        draft.TradeId = input.TradeId;
        draft.TradeLabel = tradeLabel;
        draft.JobTitle = Trim(input.JobTitle, 160);
        draft.Description = Trim(input.Description, 600);
        draft.Urgency = NormalizeUrgency(input.Urgency);

        var photos = (input.ExistingPhotos ?? [])
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Concat(newPhotoUrls)
            .Distinct()
            .Take(6)
            .ToList();
        draft.PhotoUrlsJson = photos.Count == 0 ? null : JsonSerializer.Serialize(photos);
        draft.PhotoUrl = photos.FirstOrDefault();
        draft.FechaActualizacion = DateTime.UtcNow;

        if (isNew)
        {
            db.IndorProveedorNetworkJobs.Add(draft);
        }

        await db.SaveChangesAsync(cancellationToken);
        return draft.Id;
    }

    public async Task<PostJobLocationViewModel?> GetLocationAsync(IndorProveedor me, int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await LoadDraftAsync(me.Id, draftId, cancellationToken);
        if (draft == null)
        {
            return null;
        }

        return new PostJobLocationViewModel
        {
            DraftId = draft.Id,
            JobTitle = draft.JobTitle,
            Location = string.IsNullOrWhiteSpace(draft.Location) ? ComposeLocation(me) : draft.Location,
            Latitude = draft.Latitude ?? me.Latitude,
            Longitude = draft.Longitude ?? me.Longitude,
            PropertyType = draft.PropertyType,
            WhoMeets = draft.WhoMeets,
            BudgetRange = draft.BudgetRange,
            QuoteType = draft.QuoteType,
            AccessNotes = draft.AccessNotes
        };
    }

    public async Task<bool> SaveLocationAsync(IndorProveedor me, PostJobLocationInput input, CancellationToken cancellationToken = default)
    {
        var draft = await db.IndorProveedorNetworkJobs
            .FirstOrDefaultAsync(j => j.Id == input.DraftId && j.PosterProveedorId == me.Id, cancellationToken);
        if (draft == null)
        {
            return false;
        }

        draft.Location = Trim(input.Location, 200);
        draft.Latitude = input.Latitude;
        draft.Longitude = input.Longitude;
        draft.PropertyType = Trim(input.PropertyType, 30);
        draft.WhoMeets = Trim(input.WhoMeets, 30);
        draft.BudgetRange = Trim(input.BudgetRange, 40);
        draft.QuoteType = Trim(input.QuoteType, 20);
        draft.AccessNotes = Trim(input.AccessNotes, 300);
        draft.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PostJobReviewViewModel?> GetReviewAsync(IndorProveedor me, int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await LoadDraftAsync(me.Id, draftId, cancellationToken);
        if (draft == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        catalog.TryGetValue(draft.TradeId ?? "", out var cat);

        return new PostJobReviewViewModel
        {
            DraftId = draft.Id,
            TradeLabel = draft.TradeLabel ?? cat?.LabelEn,
            TradeIconClass = cat?.IconClass ?? "fa-screwdriver-wrench",
            JobTitle = draft.JobTitle,
            Description = draft.Description,
            Photos = ParsePhotos(draft.PhotoUrlsJson),
            Location = draft.Location,
            PropertyTypeLabel = draft.PropertyType,
            BudgetRange = draft.BudgetRange,
            QuoteTypeLabel = QuoteTypeLabel(draft.QuoteType),
            UrgencyLabel = UrgencyLabel(draft.Urgency),
            AccessNotes = draft.AccessNotes
        };
    }

    public async Task<int?> PublishJobAsync(IndorProveedor me, int draftId, CancellationToken cancellationToken = default)
    {
        var draft = await db.IndorProveedorNetworkJobs
            .FirstOrDefaultAsync(j => j.Id == draftId && j.PosterProveedorId == me.Id, cancellationToken);
        if (draft == null || string.IsNullOrWhiteSpace(draft.TradeId) || string.IsNullOrWhiteSpace(draft.JobTitle))
        {
            return null;
        }

        draft.Status = NetworkJobStatuses.Open;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return draft.Id;
    }

    private async Task<IndorProveedorNetworkJob?> LoadDraftAsync(int posterId, int draftId, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorNetworkJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == draftId && j.PosterProveedorId == posterId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }

    private static List<string> ParsePhotos(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? NormalizeUrgency(string? value) => value switch
    {
        NetworkJobUrgencies.Asap => NetworkJobUrgencies.Asap,
        NetworkJobUrgencies.ThisWeek => NetworkJobUrgencies.ThisWeek,
        NetworkJobUrgencies.Flexible => NetworkJobUrgencies.Flexible,
        _ => null
    };

    private static string? UrgencyLabel(string? value) => value switch
    {
        NetworkJobUrgencies.Asap => "ASAP",
        NetworkJobUrgencies.ThisWeek => "This week",
        NetworkJobUrgencies.Flexible => "Flexible",
        _ => null
    };

    private static string? QuoteTypeLabel(string? value) => value switch
    {
        NetworkJobQuoteTypes.Fixed => "Fixed Price",
        NetworkJobQuoteTypes.Hourly => "Hourly Estimate",
        _ => null
    };

    public async Task<NetworkJobPostedViewModel?> GetJobPostedAsync(
        IndorProveedor me,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await db.IndorProveedorNetworkJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId && j.PosterProveedorId == me.Id, cancellationToken);

        if (job == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var providers = await LoadActiveProvidersAsync(me.Id, cancellationToken);
        var ratings = await LoadRatingsAsync(providers.Select(p => p.Id), cancellationToken);
        var jobCounts = await LoadJobCountsAsync(providers.Select(p => p.Id), cancellationToken);

        var matches = providers
            .Where(p => string.IsNullOrWhiteSpace(job.TradeId)
                || p.Categorias.Any(c => string.Equals(c.CategoriaId, job.TradeId, StringComparison.OrdinalIgnoreCase)))
            .Select(p => BuildCard(p, catalog, ratings, jobCounts, me))
            .OrderByDescending(c => c.IsRecommended)
            .ThenByDescending(c => c.Rating ?? 0)
            .Take(4)
            .ToList();

        return new NetworkJobPostedViewModel
        {
            JobId = job.Id,
            TradeLabel = job.TradeLabel,
            MatchedCount = matches.Count,
            Matches = matches
        };
    }

    // ---------------------------------------------------------------- Hire

    public async Task<HireSubcontractorViewModel?> GetHireAsync(
        IndorProveedor me,
        int subcontractorId,
        int? jobId,
        CancellationToken cancellationToken = default)
    {
        var provider = await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .Include(p => p.Documentos)
            .FirstOrDefaultAsync(p => p.Id == subcontractorId, cancellationToken);

        if (provider == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var ratings = await LoadRatingsAsync([provider.Id], cancellationToken);
        ratings.TryGetValue(provider.Id, out var rating);
        var (avgRating, reviewCount) = rating;

        var primaryTrade = provider.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        catalog.TryGetValue(primaryTrade ?? "", out var primaryCat);
        var docsReady = provider.Documentos.Any(d => d.UploadedUtc != null);

        IndorProveedorNetworkJob? job = null;
        if (jobId.HasValue)
        {
            job = await db.IndorProveedorNetworkJobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == jobId.Value && j.PosterProveedorId == me.Id, cancellationToken);
        }

        var startDate = job?.DateNeeded ?? DateTime.UtcNow.Date.AddDays(3);
        var tradeLabel = job?.TradeLabel ?? primaryCat?.LabelEn ?? provider.ServiceDescription ?? "General";

        return new HireSubcontractorViewModel
        {
            SubcontractorId = provider.Id,
            Name = ResolveName(provider),
            TradeLabel = tradeLabel,
            AvatarInitial = InitialOf(ResolveName(provider)),
            PhotoUrl = LogoUrl(provider),
            IconClass = primaryCat?.IconClass ?? "fa-screwdriver-wrench",
            Rating = reviewCount > 0 ? avgRating : null,
            ReviewCount = reviewCount,
            LocationLabel = ComposeLocation(provider),
            IsVerified = IsVerified(provider),
            IsInsured = provider.IsInsured,
            IsDocsReady = docsReady,
            NetworkJobId = job?.Id,
            ProjectTitle = !string.IsNullOrWhiteSpace(job?.JobTitle)
                ? Shorten(job!.JobTitle!, 60)
                : string.IsNullOrWhiteSpace(job?.Description)
                    ? $"{tradeLabel} project"
                    : Shorten(job!.Description!, 60),
            TradeSummary = tradeLabel,
            BudgetRange = job?.BudgetRange ?? "$1,000 – $5,000",
            StartDateLabel = startDate.ToString("MMM d, yyyy"),
            StartDateIso = startDate.ToString("yyyy-MM-dd"),
            ProfileReviewed = true,
            InsuranceVerified = provider.IsInsured,
            DocumentsReady = docsReady,
            AvailabilityConfirmed = provider.SameDayJobs || provider.EmergencyService
        };
    }

    public async Task<int?> ConfirmHireAsync(
        int hirerProveedorId,
        ConfirmHireInput input,
        CancellationToken cancellationToken = default)
    {
        var exists = await db.IndorProveedores
            .AsNoTracking()
            .AnyAsync(p => p.Id == input.SubcontractorId, cancellationToken);
        if (!exists)
        {
            return null;
        }

        DateTime? startDate = null;
        if (DateTime.TryParse(input.StartDate, out var parsed))
        {
            startDate = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        var status = input.Mode switch
        {
            "start" => NetworkHireStatuses.Started,
            "agreement" => NetworkHireStatuses.AgreementSent,
            _ => NetworkHireStatuses.Hired
        };

        var hire = new IndorProveedorNetworkHire
        {
            HirerProveedorId = hirerProveedorId,
            SubcontractorProveedorId = input.SubcontractorId,
            NetworkJobId = input.NetworkJobId,
            ProjectTitle = Trim(input.ProjectTitle, 160),
            TradeLabel = Trim(input.TradeLabel, 120),
            BudgetRange = Trim(input.BudgetRange, 40),
            StartDate = startDate,
            Status = status,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorNetworkHires.Add(hire);

        if (input.NetworkJobId.HasValue)
        {
            var job = await db.IndorProveedorNetworkJobs
                .FirstOrDefaultAsync(j => j.Id == input.NetworkJobId.Value && j.PosterProveedorId == hirerProveedorId, cancellationToken);
            if (job != null)
            {
                job.Status = NetworkJobStatuses.Hired;
                job.FechaActualizacion = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return hire.Id;
    }

    public async Task<NetworkHireConfirmedViewModel?> GetHireConfirmedAsync(
        IndorProveedor me,
        int hireId,
        CancellationToken cancellationToken = default)
    {
        var hire = await db.IndorProveedorNetworkHires
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == hireId && h.HirerProveedorId == me.Id, cancellationToken);

        if (hire == null)
        {
            return null;
        }

        var provider = await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .Include(p => p.Documentos)
            .FirstOrDefaultAsync(p => p.Id == hire.SubcontractorProveedorId, cancellationToken);

        var catalog = await LoadCatalogAsync(cancellationToken);
        var primaryTrade = provider?.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        catalog.TryGetValue(primaryTrade ?? "", out var primaryCat);

        return new NetworkHireConfirmedViewModel
        {
            HireId = hire.Id,
            SubcontractorName = provider != null ? ResolveName(provider) : "Subcontractor",
            TradeLabel = hire.TradeLabel ?? primaryCat?.LabelEn,
            AvatarInitial = InitialOf(provider != null ? ResolveName(provider) : "P"),
            IconClass = primaryCat?.IconClass ?? "fa-screwdriver-wrench",
            ProjectTitle = hire.ProjectTitle,
            BudgetRange = hire.BudgetRange,
            StartDateLabel = hire.StartDate?.ToString("MMM d, yyyy") ?? "To be scheduled",
            StatusLabel = hire.Status switch
            {
                NetworkHireStatuses.Started => "Job started",
                NetworkHireStatuses.AgreementSent => "Agreement sent",
                _ => "Hired"
            },
            IsVerified = provider != null && IsVerified(provider),
            IsInsured = provider?.IsInsured ?? false,
            IsDocsReady = provider?.Documentos.Any(d => d.UploadedUtc != null) ?? false
        };
    }

    public async Task<bool> ToggleSaveAsync(int ownerProveedorId, int subcontractorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await db.IndorProveedorNetworkGuardados
                .FirstOrDefaultAsync(g => g.OwnerProveedorId == ownerProveedorId && g.SubcontractorProveedorId == subcontractorId, cancellationToken);

            if (existing != null)
            {
                db.IndorProveedorNetworkGuardados.Remove(existing);
                await db.SaveChangesAsync(cancellationToken);
                return false;
            }

            db.IndorProveedorNetworkGuardados.Add(new IndorProveedorNetworkGuardado
            {
                OwnerProveedorId = ownerProveedorId,
                SubcontractorProveedorId = subcontractorId,
                FechaCreacion = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            logger.LogWarning(ex, "Network saved-pros table missing.");
            return false;
        }
    }

    public async Task<MessageSubcontractorViewModel?> GetMessageComposeAsync(
        IndorProveedor me,
        int subcontractorId,
        CancellationToken cancellationToken = default)
    {
        var provider = await db.IndorProveedores
            .AsNoTracking()
            .Include(p => p.Categorias)
            .FirstOrDefaultAsync(p => p.Id == subcontractorId, cancellationToken);

        if (provider == null || provider.Id == me.Id)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var primaryTrade = provider.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        catalog.TryGetValue(primaryTrade ?? "", out var primaryCat);

        return new MessageSubcontractorViewModel
        {
            SubcontractorId = provider.Id,
            Name = ResolveName(provider),
            TradeLabel = primaryCat?.LabelEn ?? provider.ServiceDescription,
            PhotoUrl = LogoUrl(provider),
            IconClass = primaryCat?.IconClass ?? "fa-screwdriver-wrench"
        };
    }

    public async Task<MessageSubcontractorSentViewModel?> SendMessageAsync(
        IndorProveedor me,
        MessageSubcontractorInput input,
        CancellationToken cancellationToken = default)
    {
        var body = (input.Body ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(body) || input.SubcontractorId <= 0 || input.SubcontractorId == me.Id)
        {
            return null;
        }

        if (body.Length > 600)
        {
            body = body[..600];
        }

        var provider = await db.IndorProveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == input.SubcontractorId, cancellationToken);

        if (provider == null)
        {
            return null;
        }

        try
        {
            db.IndorProveedorNetworkInvitaciones.Add(new IndorProveedorNetworkInvitacion
            {
                InviterProveedorId = me.Id,
                SubcontractorProveedorId = provider.Id,
                JobTitle = "Direct message",
                Description = body,
                TimingPreference = NetworkInvitationTimings.Flexible,
                Status = NetworkInvitationStatuses.Sent,
                FechaCreacion = DateTime.UtcNow
            });
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            logger.LogWarning(ex, "Network invitations table missing; message not saved.");
            return null;
        }

        return new MessageSubcontractorSentViewModel
        {
            SubcontractorId = provider.Id,
            Name = ResolveName(provider)
        };
    }

    // ---------------------------------------------------------------- Loaders

    private async Task<Dictionary<string, IndorProveedorCategoriaCatalogo>> LoadCatalogAsync(CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorCategoriasCatalogo
                .AsNoTracking()
                .Where(c => c.Activo)
                .ToDictionaryAsync(c => c.Id, c => c, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return new Dictionary<string, IndorProveedorCategoriaCatalogo>();
        }
    }

    private async Task<Dictionary<string, IndorProveedorOfertaCatalogo>> LoadOfferCatalogAsync(CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorOfertasCatalogo
                .AsNoTracking()
                .ToDictionaryAsync(o => o.Id, o => o, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return new Dictionary<string, IndorProveedorOfertaCatalogo>();
        }
    }

    private async Task<List<IndorProveedor>> LoadActiveProvidersAsync(int excludeId, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Include(p => p.Documentos)
                .Where(p => ActiveStatuses.Contains(p.RegistrationStatus))
                .Where(p => p.Id != excludeId)
                .Where(p => p.BusinessName != null || p.DbaName != null)
                .OrderByDescending(p => p.FechaActualizacion)
                .Take(200)
                .ToListAsync(ct);
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
            return new Dictionary<int, (decimal, int)>();
        }

        try
        {
            var grouped = await db.IndorProveedorNetworkResenas
                .AsNoTracking()
                .Where(r => idList.Contains(r.SubcontractorProveedorId))
                .GroupBy(r => r.SubcontractorProveedorId)
                .Select(g => new { g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
                .ToListAsync(ct);

            return grouped.ToDictionary(
                g => g.Key,
                g => (Avg: Math.Round((decimal)g.Avg, 1), Count: g.Count));
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return new Dictionary<int, (decimal, int)>();
        }
    }

    private async Task<Dictionary<int, int>> LoadJobCountsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<int, int>();
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
            return new Dictionary<int, int>();
        }
    }

    private async Task<List<NetworkReviewViewModel>> LoadReviewsAsync(int subId, CancellationToken ct)
    {
        try
        {
            var rows = await db.IndorProveedorNetworkResenas
                .AsNoTracking()
                .Where(r => r.SubcontractorProveedorId == subId)
                .OrderByDescending(r => r.FechaCreacion)
                .Take(10)
                .ToListAsync(ct);

            return rows.Select(r => new NetworkReviewViewModel
            {
                AuthorName = string.IsNullOrWhiteSpace(r.AuthorName) ? "INDOR contractor" : r.AuthorName!,
                Rating = r.Rating,
                Comment = r.Comment,
                DateLabel = r.FechaCreacion.ToLocalTime().ToString("MMM d, yyyy")
            }).ToList();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<int> SafeCountAsync(Func<Task<int>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return 0;
        }
    }

    private async Task<bool> SafeAnyAsync(Func<Task<bool>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return false;
        }
    }

    // ---------------------------------------------------------------- Builders

    private NetworkSubcontractorCardViewModel BuildCard(
        IndorProveedor p,
        IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog,
        IReadOnlyDictionary<int, (decimal Avg, int Count)> ratings,
        IReadOnlyDictionary<int, int> jobCounts,
        IndorProveedor me)
    {
        var name = ResolveName(p);
        var primaryTrade = p.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        catalog.TryGetValue(primaryTrade ?? "", out var primaryCat);

        ratings.TryGetValue(p.Id, out var rating);
        var (avg, count) = rating;
        jobCounts.TryGetValue(p.Id, out var jobsCompleted);
        var docsReady = p.Documentos.Any(d => d.UploadedUtc != null);
        var recommended = count > 0 && avg >= 4.8m;

        return new NetworkSubcontractorCardViewModel
        {
            Id = p.Id,
            Name = name,
            TradeLabel = primaryCat?.LabelEn ?? p.ServiceDescription,
            AvatarInitial = InitialOf(name),
            PhotoUrl = LogoUrl(p),
            IconClass = primaryCat?.IconClass ?? "fa-screwdriver-wrench",
            Rating = count > 0 ? avg : null,
            ReviewCount = count,
            JobsCompletedCount = jobsCompleted,
            ResponseLabel = ShortResponseLabel(p),
            DistanceLabel = DistanceLabel(me, p),
            LocationLabel = ComposeLocation(p),
            IsVerified = IsVerified(p),
            IsInsured = p.IsInsured,
            IsDocsReady = docsReady,
            IsRecommended = recommended,
            IsAvailableNow = p.SameDayJobs,
            AvailabilityLabel = AvailabilityLabel(p)
        };
    }

    private static List<NetworkTradeChipViewModel> BuildTradeChips(
        IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog,
        List<IndorProveedor> providers)
    {
        var usedTradeIds = providers
            .SelectMany(p => p.Categorias.Select(c => c.CategoriaId))
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return catalog.Values
            .Where(c => usedTradeIds.Count == 0 || usedTradeIds.Contains(c.Id))
            .OrderBy(c => c.SortOrder)
            .Select(c => new NetworkTradeChipViewModel
            {
                Id = c.Id,
                Label = c.LabelEn,
                LabelEs = c.LabelEs,
                IconClass = c.IconClass
            })
            .ToList();
    }

    private static List<NetworkRatingBarViewModel> BuildRatingBreakdown(List<NetworkReviewViewModel> reviews)
    {
        var total = reviews.Count;
        var bars = new List<NetworkRatingBarViewModel>();
        for (var stars = 5; stars >= 1; stars--)
        {
            var n = reviews.Count(r => r.Rating == stars);
            bars.Add(new NetworkRatingBarViewModel
            {
                Stars = stars,
                Percent = total == 0 ? 0 : (int)Math.Round(100.0 * n / total)
            });
        }

        return bars;
    }

    // ---------------------------------------------------------------- Helpers

    private static bool IsVerified(IndorProveedor p) =>
        string.Equals(p.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
        || string.Equals(p.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);

    private static string ResolveName(IndorProveedor p) =>
        !string.IsNullOrWhiteSpace(p.DbaName) ? p.DbaName!.Trim()
        : !string.IsNullOrWhiteSpace(p.BusinessName) ? p.BusinessName!.Trim()
        : !string.IsNullOrWhiteSpace(p.PrimaryContact) ? p.PrimaryContact!.Trim()
        : "INDOR Provider";

    private static string ResolveMyName(IndorProveedor p) => ResolveName(p);

    private static string InitialOf(string name) =>
        string.IsNullOrWhiteSpace(name) ? "P" : name.Trim()[0].ToString().ToUpperInvariant();

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

    private static string AvailabilityLabel(IndorProveedor p)
    {
        if (p.SameDayJobs)
        {
            return "Available now";
        }

        return p.EmergencyService ? "Responds fast" : "Available this week";
    }

    private static string ResponseTimeLabel(IndorProveedor p)
    {
        if (p.SameDayJobs)
        {
            return "Typically replies within 1 hour";
        }

        return p.EmergencyService ? "Typically replies within a few hours" : "Typically replies within a day";
    }

    /// <summary>Compact response label shown on cards, derived from real availability flags.</summary>
    private static string ShortResponseLabel(IndorProveedor p)
    {
        if (p.EmergencyService)
        {
            return "Responds in 30 min";
        }

        if (p.SameDayJobs)
        {
            return "Responds in 1 hr";
        }

        return "Responds within a day";
    }

    private static string? DistanceLabel(IndorProveedor me, IndorProveedor other)
    {
        if (!me.Latitude.HasValue || !me.Longitude.HasValue
            || !other.Latitude.HasValue || !other.Longitude.HasValue)
        {
            return null;
        }

        var miles = HaversineMiles(
            (double)me.Latitude.Value, (double)me.Longitude.Value,
            (double)other.Latitude.Value, (double)other.Longitude.Value);

        return $"{miles:0.0} mi";
    }

    private static double ParseMiles(string label)
    {
        var digits = new string(label.Where(ch => char.IsDigit(ch) || ch == '.').ToArray());
        return double.TryParse(digits, out var v) ? v : double.MaxValue;
    }

    private static double HaversineMiles(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMiles = 3958.8;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMiles * c;
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;

    private static bool Contains(string? value, string term) =>
        !string.IsNullOrWhiteSpace(value) && value.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        return value.Length > max ? value[..max] : value;
    }

    private static string Shorten(string value, int max)
    {
        value = value.Trim();
        return value.Length <= max ? value : value[..max].TrimEnd() + "…";
    }
}
