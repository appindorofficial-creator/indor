using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorProviderBridgeService(AppDbContext db) : IRealtorProviderBridgeService
{
    private static readonly string[] ActiveProviderStatuses =
    [
        ProviderRegistrationStatuses.Approved,
        ProviderRegistrationStatuses.IndorProActive,
        ProviderRegistrationStatuses.PendingReview,
        ProviderRegistrationStatuses.Submitted
    ];

    public async Task<List<IndorProveedor>> MatchProveedoresForTradeAsync(
        string trade, CancellationToken cancellationToken = default)
    {
        var categoryCode = TradeToCategoryCode(trade);
        var proveedorIds = await db.IndorProveedorCategoriasSel
            .AsNoTracking()
            .Where(s => s.CategoriaId == categoryCode)
            .Select(s => s.ProveedorId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (proveedorIds.Count == 0)
        {
            proveedorIds = await db.IndorProveedorCategoriasSel
                .AsNoTracking()
                .Where(s => s.CategoriaId == "handyman")
                .Select(s => s.ProveedorId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        var proveedores = await db.IndorProveedores
            .AsNoTracking()
            .Where(p => proveedorIds.Contains(p.Id)
                && ActiveProviderStatuses.Contains(p.RegistrationStatus))
            .OrderByDescending(p => p.FechaCreacion)
            .Take(12)
            .ToListAsync(cancellationToken);

        if (proveedores.Count == 0)
        {
            proveedores = await db.IndorProveedores
                .AsNoTracking()
                .Where(p => ActiveProviderStatuses.Contains(p.RegistrationStatus))
                .OrderByDescending(p => p.FechaCreacion)
                .Take(6)
                .ToListAsync(cancellationToken);
        }

        return proveedores;
    }

    public async Task<IndorProveedorLead> CreateLeadFromRealtorQuoteAsync(
        IndorRealtorQuote quote,
        IndorProveedor proveedor,
        IReadOnlyList<IndorRealtorInspectionUploadFinding> tradeFindings,
        string? inspectionReportUrl,
        CancellationToken cancellationToken = default)
    {
        var topPriority = tradeFindings
            .OrderByDescending(f => PriorityWeight(f.Priority))
            .FirstOrDefault()?.Priority ?? RealtorInspectionFindingPriorities.Moderate;

        var urgency = topPriority switch
        {
            RealtorInspectionFindingPriorities.Urgent => "High urgency",
            RealtorInspectionFindingPriorities.High => "High",
            _ => "Standard"
        };

        var findingsPayload = tradeFindings.Select(f => new
        {
            f.Title,
            f.Description,
            f.Priority,
            f.Trade,
            f.AiScore,
            f.SourceSection,
            f.SourcePage,
            f.SourceExcerpt,
            reportReference = BuildReportReference(f.SourceSection, f.SourcePage)
        });

        var problem = tradeFindings.Count > 0
            ? TruncateText(string.Join("\n\n", tradeFindings.Select(FormatFindingForProvider)), 2000)
            : TruncateText(
                quote.OptionalMessage ?? $"Quote request {quote.QuoteCode} for {quote.ServiceType} at {quote.Address}.",
                2000);

        var lead = new IndorProveedorLead
        {
            ProveedorId = proveedor.Id,
            Address = TruncateText(quote.Address, 250) ?? "",
            ServiceType = TruncateText(quote.ServiceType, 120) ?? "General",
            Urgency = urgency,
            Status = ProviderLeadStatuses.New,
            CustomerName = TruncateText(quote.ClientName ?? "Realtor client", 120),
            CustomerEmail = proveedor.Email,
            ProblemDescription = problem,
            ImageUrl = quote.PhotoUrl ?? "/welcome-house.png",
            TimelineNote = quote.ResponseDeadlineHours.HasValue
                ? $"Respond within {quote.ResponseDeadlineHours} hours"
                : "Realtor inspection quote request",
            LeadCode = $"RQ-{quote.QuoteCode}",
            RealtorQuoteId = quote.Id,
            LeadSource = "RealtorInspection",
            InspectionReportUrl = inspectionReportUrl,
            FindingsJson = JsonSerializer.Serialize(findingsPayload),
            SuggestedScopeItemsJson = JsonSerializer.Serialize(tradeFindings.Select(f => new ProviderProEstimateLineItemViewModel
            {
                Label = f.Title,
                Description = f.Description ?? "",
                Amount = 0,
                Qty = 1
            })),
            SuggestedHomeownerNotes = $"Quote request {quote.QuoteCode} from INDOR Realtor portal.",
            AnalysisSummary = $"INDOR found {tradeFindings.Count} {quote.ServiceType.ToLowerInvariant()} repair item{(tradeFindings.Count == 1 ? "" : "s")} from the uploaded inspection report.",
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorProveedorLeads.Add(lead);
        await db.SaveChangesAsync(cancellationToken);
        return lead;
    }

    public async Task SyncBidFromEstimateAsync(
        IndorProveedorEstimate estimate, CancellationToken cancellationToken = default)
    {
        if (estimate.LeadId is not > 0)
        {
            return;
        }

        if (estimate.Status is not ProviderEstimateStatuses.Sent
            and not ProviderEstimateStatuses.Viewed
            and not ProviderEstimateStatuses.Approved)
        {
            return;
        }

        var lead = await db.IndorProveedorLeads
            .FirstOrDefaultAsync(l => l.Id == estimate.LeadId, cancellationToken);

        if (lead == null)
        {
            return;
        }

        var quoteId = await ResolveRealtorQuoteIdAsync(lead, estimate, cancellationToken);
        if (quoteId is not > 0)
        {
            return;
        }

        if (lead.RealtorQuoteId != quoteId)
        {
            lead.RealtorQuoteId = quoteId;
        }

        var quote = await db.IndorRealtorQuotes
            .Include(q => q.Bids)
            .FirstOrDefaultAsync(q => q.Id == quoteId, cancellationToken);

        if (quote == null)
        {
            return;
        }

        var proveedor = await db.IndorProveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == estimate.ProveedorId, cancellationToken);

        var providerName = ResolveCompanyName(proveedor);
        var submittedUtc = estimate.SentUtc ?? estimate.ApprovedUtc ?? DateTime.UtcNow;

        var existing = quote.Bids.FirstOrDefault(b =>
            b.EstimateId == estimate.Id
            || (b.ProveedorId == estimate.ProveedorId && b.LeadId == lead.Id)
            || (b.ProveedorId == estimate.ProveedorId
                && string.Equals(b.ProviderName, providerName, StringComparison.OrdinalIgnoreCase)));

        if (existing != null)
        {
            existing.Amount = estimate.Amount;
            existing.ProviderName = providerName;
            existing.SubmittedUtc = submittedUtc;
            existing.ProveedorId ??= estimate.ProveedorId;
            existing.EstimateId ??= estimate.Id;
            existing.LeadId ??= lead.Id;
        }
        else
        {
            db.IndorRealtorQuoteBids.Add(new IndorRealtorQuoteBid
            {
                QuoteId = quote.Id,
                ProviderName = providerName,
                Amount = estimate.Amount,
                Rating = 4.8m,
                SortOrder = quote.Bids.Count + 1,
                ProveedorId = estimate.ProveedorId,
                EstimateId = estimate.Id,
                LeadId = lead.Id,
                SubmittedUtc = submittedUtc
            });
        }

        var sentProvider = await db.IndorRealtorQuoteSentProviders
            .FirstOrDefaultAsync(sp =>
                sp.QuoteId == quote.Id
                && (sp.ProveedorId == estimate.ProveedorId
                    || sp.LeadId == lead.Id
                    || sp.ProviderId == estimate.ProveedorId), cancellationToken);

        if (sentProvider == null)
        {
            db.IndorRealtorQuoteSentProviders.Add(new IndorRealtorQuoteSentProvider
            {
                QuoteId = quote.Id,
                ProviderId = estimate.ProveedorId,
                ProveedorId = estimate.ProveedorId,
                LeadId = lead.Id,
                ProviderName = providerName
            });
        }
        else if (sentProvider.LeadId is not > 0)
        {
            sentProvider.LeadId = lead.Id;
            sentProvider.ProveedorId = estimate.ProveedorId;
        }

        await db.SaveChangesAsync(cancellationToken);

        quote.ProviderQuotesReceived = await db.IndorRealtorQuoteBids
            .CountAsync(b => b.QuoteId == quote.Id, cancellationToken);

        var bidAmounts = await db.IndorRealtorQuoteBids.AsNoTracking()
            .Where(b => b.QuoteId == quote.Id)
            .Select(b => b.Amount)
            .ToListAsync(cancellationToken);

        if (bidAmounts.Count > 0)
        {
            quote.Amount = bidAmounts.Min();
        }

        quote.Status = quote.ProviderQuotesReceived >= 2 ? "Compare"
            : quote.ProviderQuotesReceived >= 1 ? "Received"
            : quote.Status;

        quote.FooterNote = $"{quote.ProviderQuotesReceived} provider quote{(quote.ProviderQuotesReceived == 1 ? "" : "s")} received";
        quote.UpdatedUtc = DateTime.UtcNow;

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = quote.RealtorId,
            ActivityType = "quote",
            Description = $"{providerName} submitted a quote for {quote.Address} ({quote.QuoteCode}) — {estimate.Amount:C0}",
            CategoryTag = "Quotes",
            OccurredUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int?> ResolveRealtorQuoteIdAsync(
        IndorProveedorLead lead,
        IndorProveedorEstimate estimate,
        CancellationToken cancellationToken)
    {
        if (lead.RealtorQuoteId is > 0)
        {
            return lead.RealtorQuoteId;
        }

        var fromSentProvider = await (
            from sp in db.IndorRealtorQuoteSentProviders.AsNoTracking()
            join q in db.IndorRealtorQuotes.AsNoTracking() on sp.QuoteId equals q.Id
            where sp.LeadId == lead.Id
                || sp.ProveedorId == estimate.ProveedorId
                || sp.ProviderId == estimate.ProveedorId
            orderby sp.Id descending
            select (int?)sp.QuoteId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fromSentProvider is > 0)
        {
            return fromSentProvider;
        }

        if (!string.IsNullOrWhiteSpace(lead.LeadCode)
            && lead.LeadCode.StartsWith("RQ-", StringComparison.OrdinalIgnoreCase))
        {
            var quoteCode = lead.LeadCode["RQ-".Length..];
            var fromCode = await db.IndorRealtorQuotes.AsNoTracking()
                .Where(q => q.QuoteCode == quoteCode)
                .Select(q => (int?)q.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (fromCode is > 0)
            {
                return fromCode;
            }
        }

        var normalizedAddress = lead.Address.Trim();
        var fromAddress = await (
            from q in db.IndorRealtorQuotes.AsNoTracking()
            join sp in db.IndorRealtorQuoteSentProviders.AsNoTracking() on q.Id equals sp.QuoteId
            where q.Address == normalizedAddress
                && (sp.ProveedorId == estimate.ProveedorId || sp.ProviderId == estimate.ProveedorId)
            orderby q.RequestedUtc descending
            select (int?)q.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return fromAddress;
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

    private static string FormatFindingForProvider(IndorRealtorInspectionUploadFinding f)
    {
        var lines = new List<string> { $"• [{f.Priority}] {f.Title}" };
        if (!string.IsNullOrWhiteSpace(f.Description))
        {
            lines.Add($"  Summary: {f.Description}");
        }

        var reference = BuildReportReference(f.SourceSection, f.SourcePage);
        if (!string.IsNullOrWhiteSpace(reference))
        {
            lines.Add($"  In report: {reference} (open the PDF at this section/page to view inspector photos)");
        }

        if (!string.IsNullOrWhiteSpace(f.SourceExcerpt))
        {
            var quote = f.SourceExcerpt.Length > 280 ? f.SourceExcerpt[..280] + "…" : f.SourceExcerpt;
            lines.Add($"  Inspector note: \"{quote}\"");
        }

        return string.Join('\n', lines);
    }

    private static string? BuildReportReference(string? section, int? page)
    {
        var parts = new List<string>();
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

    private static string TradeToCategoryCode(string trade) => trade switch
    {
        RealtorInspectionTrades.Electrical => "electrical",
        RealtorInspectionTrades.Hvac => "hvac",
        RealtorInspectionTrades.Plumbing => "plumbing",
        RealtorInspectionTrades.Roof => "roofing",
        RealtorInspectionTrades.Paint => "painting",
        RealtorInspectionTrades.Handyman => "handyman",
        _ => "handyman"
    };

    private static int PriorityWeight(string priority) => priority switch
    {
        RealtorInspectionFindingPriorities.Urgent => 3,
        RealtorInspectionFindingPriorities.High => 2,
        _ => 1
    };

    private static string ResolveCompanyName(IndorProveedor? proveedor) =>
        !string.IsNullOrWhiteSpace(proveedor?.DbaName) ? proveedor.DbaName!
        : !string.IsNullOrWhiteSpace(proveedor?.BusinessName) ? proveedor.BusinessName!
        : "INDOR Provider";
}
