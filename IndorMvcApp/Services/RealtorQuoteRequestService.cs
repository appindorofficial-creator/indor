using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorQuoteRequestService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IRealtorRegistrationService registration,
    IRealtorProviderBridgeService providerBridge) : IRealtorQuoteRequestService
{
    private const string DraftIdSessionKey = "RealtorQuoteRequestDraftId";

    private const string DefaultOptionalMessage =
        "Please review the attached repairs and send your best quote.";

    public async Task<IndorRealtorQuoteRequestDraft> EnsureDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var draftId = session.GetInt32(DraftIdSessionKey);
        if (draftId is > 0)
        {
            var existing = await db.IndorRealtorQuoteRequestDrafts
                .Include(d => d.SelectedProviders)
                .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                          d.Status == RealtorQuoteRequestDraftStatuses.Draft, cancellationToken);
            if (existing != null)
            {
                return existing;
            }
        }

        var entity = new IndorRealtorQuoteRequestDraft
        {
            RealtorId = realtor.Id,
            Status = RealtorQuoteRequestDraftStatuses.Draft,
            CurrentStep = 1,
            OptionalMessage = DefaultOptionalMessage,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorQuoteRequestDrafts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(DraftIdSessionKey, entity.Id);
        return entity;
    }

    public async Task<IndorRealtorQuoteRequestDraft?> GetDraftAsync(CancellationToken cancellationToken = default)
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

        return await db.IndorRealtorQuoteRequestDrafts
            .Include(d => d.SelectedProviders)
            .ThenInclude(p => p.Provider)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                        d.Status == RealtorQuoteRequestDraftStatuses.Draft, cancellationToken);
    }

    public async Task CancelDraftAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null)
        {
            httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
            return;
        }

        db.IndorRealtorQuoteRequestDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
    }

    public string ResolveResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Property",
        2 => "RequestDetails",
        3 => "Providers",
        4 => "Review",
        _ => "Property"
    };

    public async Task<RealtorQuoteRequestPropertyViewModel> BuildPropertyAsync(string? search, CancellationToken cancellationToken = default)
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
                (p.ClientName != null && p.ClientName.Contains(term)) ||
                (p.CityRegion != null && p.CityRegion.Contains(term)));
        }

        var properties = await query
            .OrderByDescending(p => p.UpdatedUtc ?? p.FechaCreacion)
            .Take(20)
            .ToListAsync(cancellationToken);

        return new RealtorQuoteRequestPropertyViewModel
        {
            DisplayStep = 1,
            Title = "Request Quote",
            Subtitle = "Which property needs a quote?",
            SearchQuery = search,
            SelectedPropertyFileId = draft.PropertyFileId,
            Properties = properties.Select(p => new RealtorQuotePropertyOptionViewModel
            {
                Id = p.Id,
                Address = p.Address,
                ClientName = p.ClientName ?? "",
                PhotoUrl = p.PhotoUrl ?? "/welcome-house.png",
                FilePhase = p.FilePhase ?? RealtorPropertyFilePhases.General,
                FilePhaseCss = ToPhaseCss(p.FilePhase)
            }).ToList()
        };
    }

    public async Task SavePropertyAsync(int propertyFileId, CancellationToken cancellationToken = default)
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
        draft.FilePhase = property.FilePhase;
        draft.ServiceType = InferServiceType(property);
        draft.CurrentStep = 2;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorQuoteRequestDetailsViewModel> BuildRequestDetailsAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete property selection first.");

        return new RealtorQuoteRequestDetailsViewModel
        {
            DisplayStep = 2,
            Title = "Request Details",
            Subtitle = "Choose how you want to ask for quotes.",
            RequestType = draft.RequestType,
            SharePhotosVideos = draft.SharePhotosVideos,
            ShareInspectionReport = draft.ShareInspectionReport,
            ShareRepairItems = draft.ShareRepairItems,
            ShareNotes = draft.ShareNotes,
            ResponseDeadlineHours = draft.ResponseDeadlineHours,
            RequestTypeOptions = RealtorQuoteRequestTypes.Options
        };
    }

    public async Task SaveRequestDetailsAsync(
        string requestType,
        bool sharePhotosVideos,
        bool shareInspectionReport,
        bool shareRepairItems,
        bool shareNotes,
        int responseDeadlineHours,
        CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete property selection first.");

        if (!RealtorQuoteRequestTypes.Options.Any(o => o.Value == requestType))
        {
            throw new InvalidOperationException("Invalid request type.");
        }

        if (responseDeadlineHours is not (24 or 48 or 72))
        {
            responseDeadlineHours = 48;
        }

        draft.RequestType = requestType;
        draft.SharePhotosVideos = sharePhotosVideos;
        draft.ShareInspectionReport = shareInspectionReport;
        draft.ShareRepairItems = shareRepairItems;
        draft.ShareNotes = shareNotes;
        draft.ResponseDeadlineHours = responseDeadlineHours;
        draft.CurrentStep = 3;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorQuoteRequestProvidersViewModel> BuildProvidersAsync(
        string? search, string? filter, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete previous steps first.");

        var activeFilter = string.IsNullOrWhiteSpace(filter) ? "Recommended" : filter.Trim();
        var providersQuery = db.IndorRealtorQuoteProviders.AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            providersQuery = providersQuery.Where(p =>
                p.CompanyName.Contains(term) || p.Categories.Contains(term));
        }

        providersQuery = activeFilter switch
        {
            "Verified" => providersQuery.Where(p => p.IsVerified),
            "Nearby" => providersQuery.OrderBy(p => p.DistanceMiles),
            _ => providersQuery.Where(p => p.IsRecommended).OrderByDescending(p => p.Rating)
        };

        var allProviders = await providersQuery.Take(20).ToListAsync(cancellationToken);
        var selectedIds = draft.SelectedProviders.Select(p => p.ProviderId).ToHashSet();

        if (draft.ProviderSelectionMode == RealtorQuoteProviderSelectionModes.IndorRecommended &&
            selectedIds.Count == 0)
        {
            var recommended = GetRecommendedProviders(allProviders, draft);
            selectedIds = recommended.Select(p => p.Id).ToHashSet();
        }

        return new RealtorQuoteRequestProvidersViewModel
        {
            DisplayStep = 3,
            Title = draft.ProviderSelectionMode == RealtorQuoteProviderSelectionModes.IndorRecommended
                ? "INDOR Recommended Providers"
                : "Providers",
            Subtitle = "Who should choose the companies?",
            ProviderSelectionMode = draft.ProviderSelectionMode,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            ServiceType = draft.ServiceType ?? "HVAC Repair",
            ProviderCountTarget = draft.ProviderCountTarget,
            VerifiedOnly = draft.VerifiedOnly,
            Priority = draft.Priority,
            CoverageMiles = draft.CoverageMiles,
            Providers = allProviders.Select(p => new RealtorQuoteProviderCardViewModel
            {
                Id = p.Id,
                CompanyName = p.CompanyName,
                Categories = p.Categories,
                Rating = p.Rating,
                DistanceMiles = p.DistanceMiles,
                BadgeLabel = p.BadgeLabel,
                IsVerified = p.IsVerified,
                Selected = selectedIds.Contains(p.Id)
            }).ToList(),
            SelectedCount = selectedIds.Count,
            ServiceTypes = RealtorQuoteServiceTypes.All,
            SelectionModeOptions = RealtorQuoteProviderSelectionModes.Options,
            PriorityOptions = RealtorQuotePriorities.Options,
            ProviderFilters = ["Recommended", "Verified", "Nearby"]
        };
    }

    public async Task SaveProvidersAsync(
        string providerSelectionMode,
        int[]? providerIds,
        string serviceType,
        int providerCountTarget,
        bool verifiedOnly,
        string priority,
        int coverageMiles,
        CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete previous steps first.");

        if (!RealtorQuoteProviderSelectionModes.Options.Any(o => o.Value == providerSelectionMode))
        {
            providerSelectionMode = RealtorQuoteProviderSelectionModes.Manual;
        }

        draft.ProviderSelectionMode = providerSelectionMode;
        draft.ServiceType = string.IsNullOrWhiteSpace(serviceType) ? draft.ServiceType ?? "HVAC Repair" : serviceType.Trim();
        draft.ProviderCountTarget = providerCountTarget is < 2 or > 5 ? 3 : providerCountTarget;
        draft.VerifiedOnly = verifiedOnly;
        draft.Priority = RealtorQuotePriorities.Options.Any(o => o.Value == priority)
            ? priority
            : RealtorQuotePriorities.FastResponse;
        draft.CoverageMiles = coverageMiles is < 5 or > 50 ? 10 : coverageMiles;

        var existingSelections = await db.IndorRealtorQuoteRequestDraftProviders
            .Where(p => p.DraftId == draft.Id)
            .ToListAsync(cancellationToken);
        db.IndorRealtorQuoteRequestDraftProviders.RemoveRange(existingSelections);

        List<int> finalProviderIds;
        if (providerSelectionMode == RealtorQuoteProviderSelectionModes.IndorRecommended)
        {
            var pool = await db.IndorRealtorQuoteProviders.AsNoTracking()
                .Where(p => p.IsActive && (!verifiedOnly || p.IsVerified) && p.DistanceMiles <= draft.CoverageMiles)
                .ToListAsync(cancellationToken);
            finalProviderIds = GetRecommendedProviders(pool, draft).Select(p => p.Id).ToList();
        }
        else
        {
            finalProviderIds = (providerIds ?? []).Distinct().Take(5).ToList();
            if (finalProviderIds.Count == 0)
            {
                throw new InvalidOperationException("Select at least one provider.");
            }
        }

        foreach (var providerId in finalProviderIds)
        {
            db.IndorRealtorQuoteRequestDraftProviders.Add(new IndorRealtorQuoteRequestDraftProvider
            {
                DraftId = draft.Id,
                ProviderId = providerId
            });
        }

        draft.CurrentStep = 4;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorQuoteRequestReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete previous steps first.");

        await db.Entry(draft).Collection(d => d.SelectedProviders).Query()
            .Include(p => p.Provider)
            .LoadAsync(cancellationToken);

        var providerNames = draft.SelectedProviders
            .Select(p => p.Provider?.CompanyName ?? "")
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        return new RealtorQuoteRequestReviewViewModel
        {
            DisplayStep = 4,
            Title = "Review & Send",
            Subtitle = "Confirm your quote request before sending.",
            PropertyDisplay = FormatPropertyDisplay(draft),
            RequestTypeLabel = FormatRequestTypeLabel(draft.RequestType),
            SharedSummary = BuildSharedSummary(draft),
            ProvidersSummary = providerNames.Count == 0 ? "No providers selected" : string.Join(", ", providerNames),
            ProviderSelectionLabel = draft.ProviderSelectionMode == RealtorQuoteProviderSelectionModes.IndorRecommended
                ? "INDOR Recommended"
                : "Chosen by Realtor",
            SendNow = draft.SendNow,
            ScheduledSendUtc = draft.ScheduledSendUtc,
            ResponseDeadlineHours = draft.ResponseDeadlineHours,
            AllowProviderQuestions = draft.AllowProviderQuestions,
            AllowFullProjectQuote = draft.AllowFullProjectQuote,
            AllowItemizedQuote = draft.AllowItemizedQuote,
            OptionalMessage = draft.OptionalMessage ?? DefaultOptionalMessage
        };
    }

    public async Task<int> SendAsync(
        bool sendNow,
        DateTime? scheduledSendUtc,
        int responseDeadlineHours,
        bool allowProviderQuestions,
        bool allowFullProjectQuote,
        bool allowItemizedQuote,
        string? optionalMessage,
        CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete previous steps first.");

        await db.Entry(draft).Collection(d => d.SelectedProviders).Query()
            .Include(p => p.Provider)
            .LoadAsync(cancellationToken);

        if (draft.PropertyFileId is not > 0 || draft.SelectedProviders.Count == 0)
        {
            throw new InvalidOperationException("Quote request is incomplete.");
        }

        if (responseDeadlineHours is not (24 or 48 or 72))
        {
            responseDeadlineHours = draft.ResponseDeadlineHours;
        }

        var sentUtc = sendNow ? DateTime.UtcNow : scheduledSendUtc ?? DateTime.UtcNow;
        var quoteCode = await GenerateQuoteCodeAsync(realtor.Id, cancellationToken);
        var providerCount = draft.SelectedProviders.Count;

        var quote = new IndorRealtorQuote
        {
            RealtorId = realtor.Id,
            QuoteCode = quoteCode,
            Address = draft.Address ?? "",
            ServiceType = draft.ServiceType ?? "General Repair",
            Status = "Pending",
            ClientName = draft.ClientName,
            PhotoUrl = draft.PhotoUrl,
            ProviderQuotesReceived = 0,
            FooterNote = $"Waiting on {providerCount} provider{(providerCount == 1 ? "" : "s")}",
            RequestedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            PropertyFileId = draft.PropertyFileId,
            RequestType = draft.RequestType,
            ResponseDeadlineHours = responseDeadlineHours,
            ProviderSelectionMode = draft.ProviderSelectionMode,
            OptionalMessage = string.IsNullOrWhiteSpace(optionalMessage)
                ? DefaultOptionalMessage
                : optionalMessage.Trim()[..Math.Min(optionalMessage.Trim().Length, 500)],
            SentUtc = sentUtc
        };

        db.IndorRealtorQuotes.Add(quote);
        await db.SaveChangesAsync(cancellationToken);

        var proveedores = await providerBridge.MatchProveedoresForTradeAsync(
            draft.ServiceType ?? quote.ServiceType,
            cancellationToken);

        if (proveedores.Count == 0)
        {
            proveedores = await db.IndorProveedores.AsNoTracking()
                .OrderByDescending(p => p.FechaCreacion)
                .Take(draft.SelectedProviders.Count > 0 ? draft.SelectedProviders.Count : 3)
                .ToListAsync(cancellationToken);
        }
        else
        {
            proveedores = proveedores
                .Take(Math.Max(draft.SelectedProviders.Count, draft.ProviderCountTarget))
                .ToList();
        }

        foreach (var proveedor in proveedores)
        {
            var lead = await providerBridge.CreateLeadFromRealtorQuoteAsync(
                quote,
                proveedor,
                [],
                null,
                cancellationToken);

            db.IndorRealtorQuoteSentProviders.Add(new IndorRealtorQuoteSentProvider
            {
                QuoteId = quote.Id,
                ProviderId = proveedor.Id,
                ProveedorId = proveedor.Id,
                LeadId = lead.Id,
                ProviderName = ResolveProveedorName(proveedor)
            });
        }

        foreach (var sel in draft.SelectedProviders)
        {
            var alreadySent = proveedores.Any(p => p.Id == sel.ProviderId);
            if (alreadySent)
            {
                continue;
            }

            db.IndorRealtorQuoteSentProviders.Add(new IndorRealtorQuoteSentProvider
            {
                QuoteId = quote.Id,
                ProviderId = sel.ProviderId,
                ProviderName = sel.Provider?.CompanyName ?? "Provider"
            });
        }

        var property = await db.IndorRealtorPropertyFiles
            .FirstOrDefaultAsync(p => p.Id == draft.PropertyFileId && p.RealtorId == realtor.Id, cancellationToken);
        if (property != null)
        {
            property.UpdatedUtc = DateTime.UtcNow;
        }

        var providerNames = string.Join(", ", draft.SelectedProviders.Select(p => p.Provider?.CompanyName).Where(n => n != null));
        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "quote",
            Description = $"Quote request {quoteCode} sent to {providerCount} provider{(providerCount == 1 ? "" : "s")} for {draft.Address}",
            CategoryTag = "Quotes",
            OccurredUtc = DateTime.UtcNow
        });

        draft.SendNow = sendNow;
        draft.ScheduledSendUtc = scheduledSendUtc;
        draft.ResponseDeadlineHours = responseDeadlineHours;
        draft.AllowProviderQuestions = allowProviderQuestions;
        draft.AllowFullProjectQuote = allowFullProjectQuote;
        draft.AllowItemizedQuote = allowItemizedQuote;
        draft.OptionalMessage = quote.OptionalMessage;
        draft.Status = RealtorQuoteRequestDraftStatuses.Sent;
        draft.CurrentStep = 5;
        draft.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        db.IndorRealtorQuoteRequestDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);

        return quote.Id;
    }

    public async Task<RealtorQuoteRequestSuccessViewModel> BuildSuccessAsync(int quoteId, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var quote = await db.IndorRealtorQuotes.AsNoTracking()
            .Include(q => q.SentProviders)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Quote not found.");

        var itemCount = 0;
        if (quote.PropertyFileId is > 0)
        {
            itemCount = await db.IndorRealtorPropertyFileItems.AsNoTracking()
                .CountAsync(i => i.PropertyFileId == quote.PropertyFileId &&
                                 i.CategoryType == RealtorPropertyFileCategoryTypes.RepairItems, cancellationToken);
        }

        var sentLocal = (quote.SentUtc ?? quote.RequestedUtc).ToLocalTime();
        return new RealtorQuoteRequestSuccessViewModel
        {
            QuoteId = quote.Id,
            QuoteCode = FormatQuoteCode(quote.QuoteCode),
            PropertyAddress = quote.Address,
            RequestTypeLabel = FormatRequestTypeLabel(quote.RequestType ?? RealtorQuoteRequestTypes.EntireFile),
            ProvidersSummary = string.Join(", ", quote.SentProviders.Select(p => p.ProviderName)),
            SentWhenLabel = $"Today, {sentLocal:h:mm tt}",
            ResponseDeadlineHours = quote.ResponseDeadlineHours ?? 48,
            ProviderCount = quote.SentProviders.Count,
            PhotoUrl = quote.PhotoUrl ?? "/welcome-house.png",
            ItemCount = Math.Max(itemCount, quote.RequestType == RealtorQuoteRequestTypes.ByItem ? 1 : 0)
        };
    }

    private static List<IndorRealtorQuoteProvider> GetRecommendedProviders(
        IEnumerable<IndorRealtorQuoteProvider> pool,
        IndorRealtorQuoteRequestDraft draft)
    {
        var filtered = pool
            .Where(p => p.DistanceMiles <= draft.CoverageMiles)
            .Where(p => !draft.VerifiedOnly || p.IsVerified);

        filtered = draft.Priority switch
        {
            RealtorQuotePriorities.Price => filtered.OrderBy(p => p.Rating).ThenBy(p => p.DistanceMiles),
            RealtorQuotePriorities.TopRated => filtered.OrderByDescending(p => p.Rating).ThenBy(p => p.DistanceMiles),
            _ => filtered.OrderBy(p => p.DistanceMiles).ThenByDescending(p => p.Rating)
        };

        return filtered.Take(draft.ProviderCountTarget).ToList();
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

    private static string InferServiceType(IndorRealtorPropertyFile property) =>
        property.FilePhase switch
        {
            RealtorPropertyFilePhases.RepairReview => "HVAC Repair",
            RealtorPropertyFilePhases.PreClosing => "Home Inspection",
            RealtorPropertyFilePhases.Transfer => "General Repair",
            _ => property.RepairItemsCount > 0 ? "HVAC Repair" : "General Repair"
        };

    private static string FormatPropertyDisplay(IndorRealtorQuoteRequestDraft draft)
    {
        var address = draft.Address ?? "Property";
        return string.IsNullOrWhiteSpace(draft.ClientName)
            ? address
            : $"{address} — {draft.ClientName}";
    }

    private static string BuildSharedSummary(IndorRealtorQuoteRequestDraft draft)
    {
        var parts = new List<string>();
        if (draft.SharePhotosVideos) parts.Add("Photos & Videos");
        if (draft.ShareInspectionReport) parts.Add("Inspection Report");
        if (draft.ShareRepairItems) parts.Add("Repair Items");
        if (draft.ShareNotes) parts.Add("Notes & Documents");
        return parts.Count == 0 ? "Nothing shared" : string.Join(", ", parts);
    }

    private static string FormatRequestTypeLabel(string requestType) =>
        requestType == RealtorQuoteRequestTypes.ByItem ? "Request by Item" : "Send Entire File";

    private static string FormatQuoteCode(string code) =>
        code.StartsWith("Quote #", StringComparison.OrdinalIgnoreCase) ? code : $"Quote #{code}";

    private static string ToPhaseCss(string? phase) =>
        (phase ?? "").Replace(" ", "-").ToLowerInvariant();

    private static string ResolveProveedorName(IndorProveedor proveedor) =>
        !string.IsNullOrWhiteSpace(proveedor.DbaName) ? proveedor.DbaName
        : !string.IsNullOrWhiteSpace(proveedor.BusinessName) ? proveedor.BusinessName
        : "INDOR Provider";
}
