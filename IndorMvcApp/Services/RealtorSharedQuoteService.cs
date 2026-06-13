using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorSharedQuoteService(AppDbContext db, RealtorPortalService portalService)
{
    public async Task<RealtorEditSharedQuoteViewModel?> BuildEditAsync(
        IndorRealtor realtor, int quoteId, int bidId, CancellationToken ct = default)
    {
        var context = await LoadContextAsync(realtor, quoteId, bidId, ct);
        if (context == null)
        {
            return null;
        }

        var (quote, bid, shell) = context.Value;
        var draft = await db.IndorRealtorSharedQuotes
            .FirstOrDefaultAsync(s =>
                s.RealtorId == realtor.Id && s.QuoteId == quoteId && s.BidId == bidId &&
                s.Status == RealtorSharedQuoteStatuses.Draft, ct);

        var homeowner = await ResolveHomeownerAsync(realtor.Id, quote, ct);
        var firstName = homeowner.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";

        return new RealtorEditSharedQuoteViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SharedQuoteId = draft?.Id ?? 0,
            QuoteId = quoteId,
            BidId = bidId,
            Address = quote.Address,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            HomeownerName = draft?.HomeownerName ?? homeowner.Name,
            HomeownerEmail = draft?.HomeownerEmail ?? homeowner.Email,
            HomeownerPhone = draft?.HomeownerPhone ?? homeowner.Phone,
            HomeownerInitials = DeriveInitials(draft?.HomeownerName ?? homeowner.Name),
            ShareProviderInfo = draft?.ShareProviderInfo ?? true,
            ShareFullPriceBreakdown = draft?.ShareFullPriceBreakdown ?? false,
            ShareScopeOfWork = draft?.ShareScopeOfWork ?? true,
            ShareWarranty = draft?.ShareWarranty ?? true,
            ShareIncludedRepairs = draft?.ShareIncludedRepairs ?? true,
            ShareTimeline = draft?.ShareTimeline ?? true,
            PricingDisplayMode = draft?.PricingDisplayMode ?? RealtorSharedQuotePricingModes.TotalOnly,
            MessageToHomeowner = draft?.MessageToHomeowner ?? BuildDefaultMessage(firstName, quote),
            InternalNotes = draft?.InternalNotes ?? BuildDefaultInternalNotes(quote)
        };
    }

    public async Task<int?> SaveEditAsync(
        IndorRealtor realtor, RealtorEditSharedQuoteViewModel input, CancellationToken ct = default)
    {
        var context = await LoadContextAsync(realtor, input.QuoteId, input.BidId, ct);
        if (context == null)
        {
            return null;
        }

        var draft = input.SharedQuoteId > 0
            ? await db.IndorRealtorSharedQuotes
                .FirstOrDefaultAsync(s => s.Id == input.SharedQuoteId && s.RealtorId == realtor.Id, ct)
            : await db.IndorRealtorSharedQuotes
                .FirstOrDefaultAsync(s =>
                    s.RealtorId == realtor.Id && s.QuoteId == input.QuoteId && s.BidId == input.BidId &&
                    s.Status == RealtorSharedQuoteStatuses.Draft, ct);

        if (draft == null)
        {
            draft = new IndorRealtorSharedQuote
            {
                RealtorId = realtor.Id,
                QuoteId = input.QuoteId,
                BidId = input.BidId,
                Status = RealtorSharedQuoteStatuses.Draft
            };
            db.IndorRealtorSharedQuotes.Add(draft);
        }

        draft.HomeownerName = input.HomeownerName.Trim();
        draft.HomeownerEmail = NullIfEmpty(input.HomeownerEmail);
        draft.HomeownerPhone = NullIfEmpty(input.HomeownerPhone);
        draft.ShareProviderInfo = input.ShareProviderInfo;
        draft.ShareFullPriceBreakdown = input.ShareFullPriceBreakdown;
        draft.ShareScopeOfWork = input.ShareScopeOfWork;
        draft.ShareWarranty = input.ShareWarranty;
        draft.ShareIncludedRepairs = input.ShareIncludedRepairs;
        draft.ShareTimeline = input.ShareTimeline;
        draft.PricingDisplayMode = input.PricingDisplayMode == RealtorSharedQuotePricingModes.FullDetails
            ? RealtorSharedQuotePricingModes.FullDetails
            : RealtorSharedQuotePricingModes.TotalOnly;
        draft.MessageToHomeowner = NullIfEmpty(input.MessageToHomeowner);
        draft.InternalNotes = NullIfEmpty(input.InternalNotes);
        draft.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return draft.Id;
    }

    public async Task<RealtorPreviewSharedQuoteViewModel?> BuildPreviewAsync(
        IndorRealtor realtor, int sharedQuoteId, CancellationToken ct = default)
    {
        var shared = await LoadOwnedSharedAsync(realtor.Id, sharedQuoteId, ct);
        if (shared == null)
        {
            return null;
        }

        var viewQuote = await portalService.BuildViewQuoteAsync(realtor, shared.QuoteId, shared.BidId, ct);
        if (viewQuote == null)
        {
            return null;
        }

        var shell = await portalService.BuildShellForRealtorAsync(realtor, ct);
        var showBreakdown = shared.ShareFullPriceBreakdown
            || shared.PricingDisplayMode == RealtorSharedQuotePricingModes.FullDetails;

        return new RealtorPreviewSharedQuoteViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SharedQuoteId = shared.Id,
            QuoteId = shared.QuoteId,
            BidId = shared.BidId,
            HomeownerName = shared.HomeownerName,
            Address = viewQuote.Address,
            PhotoUrl = viewQuote.PhotoUrl,
            ProviderName = viewQuote.ProviderName,
            ProviderInitials = viewQuote.ProviderInitials,
            ShowProviderInfo = shared.ShareProviderInfo,
            TotalLabel = viewQuote.TotalLabel,
            TimelineLabel = shared.ShareTimeline ? viewQuote.TimelineLabel : "",
            WarrantyLabel = shared.ShareWarranty ? viewQuote.WarrantyLabel : "",
            ScopeOfWork = shared.ShareScopeOfWork ? viewQuote.ScopeOfWork : "",
            IncludedRepairs = shared.ShareIncludedRepairs ? viewQuote.IncludedRepairs : [],
            PriceLines = showBreakdown ? viewQuote.PriceLines : [],
            TotalAmountLabel = viewQuote.TotalAmountLabel,
            ShowFullPriceBreakdown = showBreakdown,
            MessageToHomeowner = shared.MessageToHomeowner ?? "",
            DeliveryMethod = shared.DeliveryMethod,
            ShareLink = BuildShareLink(shared.ShareToken)
        };
    }

    public async Task<RealtorSharedQuoteTrackingViewModel?> BuildTrackingAsync(
        IndorRealtor realtor, int sharedQuoteId, CancellationToken ct = default)
    {
        var shared = await LoadOwnedSharedAsync(realtor.Id, sharedQuoteId, ct);
        if (shared == null || shared.Status == RealtorSharedQuoteStatuses.Draft)
        {
            return null;
        }

        var viewQuote = await portalService.BuildViewQuoteAsync(realtor, shared.QuoteId, shared.BidId, ct);
        if (viewQuote == null)
        {
            return null;
        }

        var shell = await portalService.BuildShellForRealtorAsync(realtor, ct);
        var timeline = BuildTimeline(shared);
        var recent = shared.ViewedUtc.HasValue
            ? $"Homeowner viewed the quote {FormatRelative(shared.ViewedUtc.Value)}"
            : shared.DeliveredUtc.HasValue
                ? $"Quote delivered {FormatRelative(shared.DeliveredUtc.Value)}"
                : "Waiting for homeowner to open the quote";

        return new RealtorSharedQuoteTrackingViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SharedQuoteId = shared.Id,
            QuoteId = shared.QuoteId,
            BidId = shared.BidId,
            Address = viewQuote.Address,
            PhotoUrl = viewQuote.PhotoUrl,
            HomeownerName = shared.HomeownerName,
            ProviderName = viewQuote.ProviderName,
            TotalAmountLabel = viewQuote.TotalAmountLabel,
            StatusBadge = shared.AcceptedUtc.HasValue ? "Accepted"
                : shared.ViewedUtc.HasValue ? "Viewed"
                : "Waiting",
            RecentActivityLabel = recent,
            RecentActivityTime = shared.ViewedUtc.HasValue ? "Just now" : "",
            Timeline = timeline,
            ViewSharedQuoteUrl = $"/SharedQuote/View/{shared.ShareToken}",
            ShareLink = BuildShareLink(shared.ShareToken)
        };
    }

    public async Task<bool> SendAsync(
        IndorRealtor realtor, int sharedQuoteId, string deliveryMethod, CancellationToken ct = default)
    {
        var shared = await db.IndorRealtorSharedQuotes
            .Include(s => s.Quote)
            .FirstOrDefaultAsync(s => s.Id == sharedQuoteId && s.RealtorId == realtor.Id, ct);

        if (shared == null)
        {
            return false;
        }

        var now = DateTime.UtcNow;
        shared.DeliveryMethod = NormalizeDelivery(deliveryMethod);
        shared.Status = RealtorSharedQuoteStatuses.Sent;
        shared.SentUtc = now;
        shared.DeliveredUtc = now.AddMinutes(1);
        shared.FechaActualizacion = now;

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "quote",
            Description = $"Quote shared with {shared.HomeownerName} for {shared.Quote?.Address ?? "property"}",
            CategoryTag = "Quotes",
            OccurredUtc = now
        });

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<HomeownerSharedQuoteViewModel?> BuildHomeownerViewAsync(Guid token, CancellationToken ct = default)
    {
        var shared = await db.IndorRealtorSharedQuotes.AsNoTracking()
            .Include(s => s.Quote)
            .Include(s => s.Bid)
            .Include(s => s.Realtor)
            .FirstOrDefaultAsync(s => s.ShareToken == token && s.Status != RealtorSharedQuoteStatuses.Draft, ct);

        if (shared?.Quote == null || shared.Bid == null || shared.Realtor == null)
        {
            return null;
        }

        if (!shared.ViewedUtc.HasValue)
        {
            var tracked = await db.IndorRealtorSharedQuotes
                .FirstOrDefaultAsync(s => s.Id == shared.Id, ct);
            if (tracked != null)
            {
                tracked.ViewedUtc = DateTime.UtcNow;
                tracked.Status = RealtorSharedQuoteStatuses.Viewed;
                tracked.FechaActualizacion = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }
        }

        var viewQuote = await portalService.BuildViewQuoteAsync(shared.Realtor, shared.QuoteId, shared.BidId, ct);
        if (viewQuote == null)
        {
            return null;
        }

        var showBreakdown = shared.ShareFullPriceBreakdown
            || shared.PricingDisplayMode == RealtorSharedQuotePricingModes.FullDetails;

        return new HomeownerSharedQuoteViewModel
        {
            ShareToken = shared.ShareToken,
            Address = viewQuote.Address,
            PhotoUrl = viewQuote.PhotoUrl,
            RealtorName = shared.Realtor.DisplayName ?? "Your Realtor",
            ProviderName = viewQuote.ProviderName,
            ProviderInitials = viewQuote.ProviderInitials,
            ShowProviderInfo = shared.ShareProviderInfo,
            TotalLabel = viewQuote.TotalLabel,
            TimelineLabel = shared.ShareTimeline ? viewQuote.TimelineLabel : "",
            WarrantyLabel = shared.ShareWarranty ? viewQuote.WarrantyLabel : "",
            ScopeOfWork = shared.ShareScopeOfWork ? viewQuote.ScopeOfWork : "",
            IncludedRepairs = shared.ShareIncludedRepairs ? viewQuote.IncludedRepairs : [],
            PriceLines = showBreakdown ? viewQuote.PriceLines : [],
            TotalAmountLabel = viewQuote.TotalAmountLabel,
            ShowFullPriceBreakdown = showBreakdown,
            RealtorMessage = shared.MessageToHomeowner ?? "",
            AcceptUrl = $"/SharedQuote/Accept/{shared.ShareToken}"
        };
    }

    public async Task<bool> AcceptHomeownerAsync(Guid token, CancellationToken ct = default)
    {
        var shared = await db.IndorRealtorSharedQuotes
            .Include(s => s.Quote)
            .FirstOrDefaultAsync(s => s.ShareToken == token, ct);

        if (shared == null || shared.Status == RealtorSharedQuoteStatuses.Draft)
        {
            return false;
        }

        shared.Status = RealtorSharedQuoteStatuses.Accepted;
        shared.AcceptedUtc = DateTime.UtcNow;
        shared.FechaActualizacion = DateTime.UtcNow;

        if (shared.Quote != null)
        {
            shared.Quote.Status = "Accepted";
            shared.Quote.SelectedBidId = shared.BidId;
            shared.Quote.AcceptedUtc = DateTime.UtcNow;
            shared.Quote.FooterNote = $"Accepted by homeowner";
            shared.Quote.UpdatedUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public string BuildEditUrl(int quoteId, int bidId) =>
        $"/Realtor/EditSharedQuote?quoteId={quoteId}&bidId={bidId}";

    private async Task<(IndorRealtorQuote Quote, IndorRealtorQuoteBid Bid, RealtorPortalShellViewModel Shell)?>
        LoadContextAsync(IndorRealtor realtor, int quoteId, int bidId, CancellationToken ct)
    {
        var quote = await db.IndorRealtorQuotes.AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.RealtorId == realtor.Id, ct);
        var bid = await db.IndorRealtorQuoteBids.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bidId && b.QuoteId == quoteId, ct);

        if (quote == null || bid == null)
        {
            return null;
        }

        var shell = await portalService.BuildShellForRealtorAsync(realtor, ct);
        return (quote, bid, shell);
    }

    private async Task<IndorRealtorSharedQuote?> LoadOwnedSharedAsync(
        int realtorId, int sharedQuoteId, CancellationToken ct) =>
        await db.IndorRealtorSharedQuotes.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sharedQuoteId && s.RealtorId == realtorId, ct);

    private async Task<(string Name, string Email, string Phone)> ResolveHomeownerAsync(
        int realtorId, IndorRealtorQuote quote, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(quote.ClientName))
        {
            var client = await db.IndorRealtorClients.AsNoTracking()
                .Where(c => c.RealtorId == realtorId &&
                            (c.FullName == quote.ClientName ||
                             (c.PropertyAddress != null && c.PropertyAddress == quote.Address)))
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync(ct);

            if (client != null)
            {
                return (client.FullName, client.Email ?? "", "");
            }

            return (quote.ClientName, "", "");
        }

        return ("Homeowner", "", "");
    }

    private static List<RealtorSharedQuoteTimelineItemViewModel> BuildTimeline(IndorRealtorSharedQuote shared)
    {
        var items = new List<RealtorSharedQuoteTimelineItemViewModel>();

        if (shared.SentUtc.HasValue)
        {
            items.Add(new()
            {
                Label = "Sent to Homeowner",
                TimestampLabel = FormatTimestamp(shared.SentUtc.Value),
                Icon = "fa-circle-check",
                State = "done"
            });
        }

        if (shared.DeliveredUtc.HasValue)
        {
            items.Add(new()
            {
                Label = "Delivered",
                TimestampLabel = FormatTimestamp(shared.DeliveredUtc.Value),
                Icon = "fa-circle-check",
                State = "done"
            });
        }

        if (shared.ViewedUtc.HasValue)
        {
            items.Add(new()
            {
                Label = "Viewed by Homeowner",
                TimestampLabel = FormatTimestamp(shared.ViewedUtc.Value),
                Icon = "fa-eye",
                State = "done"
            });
        }

        items.Add(new()
        {
            Label = "Accepted / Waiting",
            SubLabel = "Pending homeowner response",
            Icon = "fa-clock",
            State = shared.AcceptedUtc.HasValue ? "done" : "pending",
            Badge = shared.AcceptedUtc.HasValue ? null : "Waiting"
        });

        return items;
    }

    private static string BuildDefaultMessage(string firstName, IndorRealtorQuote quote) =>
        $"Hi {firstName}! I've reviewed the {quote.ServiceType.ToLowerInvariant()} quote for {quote.Address}. " +
        "This includes addressing the priority issues we discussed. Let me know if you have any questions! - Your Realtor";

    private static string BuildDefaultInternalNotes(IndorRealtorQuote quote) =>
        $"Quote request {quote.QuoteCode} shared from INDOR Realtor portal.";

    private string BuildShareLink(Guid token) =>
        $"/SharedQuote/View/{token}";

    private static string NormalizeDelivery(string method) =>
        method switch
        {
            RealtorSharedQuoteDeliveryMethods.Email => RealtorSharedQuoteDeliveryMethods.Email,
            RealtorSharedQuoteDeliveryMethods.Text => RealtorSharedQuoteDeliveryMethods.Text,
            RealtorSharedQuoteDeliveryMethods.Link => RealtorSharedQuoteDeliveryMethods.Link,
            _ => RealtorSharedQuoteDeliveryMethods.InApp
        };

    private static string DeriveInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
            : name.Length > 0 ? name[..1].ToUpperInvariant() : "H";
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string FormatTimestamp(DateTime utc) =>
        utc.ToLocalTime().ToString("MMM d, yyyy 'at' h:mm tt");

    private static string FormatRelative(DateTime utc)
    {
        var mins = (int)(DateTime.UtcNow - utc).TotalMinutes;
        return mins switch
        {
            < 1 => "just now",
            < 60 => $"{mins} min ago",
            < 1440 => $"{mins / 60} hr ago",
            _ => utc.ToLocalTime().ToString("MMM d")
        };
    }
}
