using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorPortalService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
{
    private const string NotificationSessionKeyPrefix = "realtor-notify-";

    private static RealtorNotificationPreferencesInput DefaultNotificationPreferences() => new();
    public async Task<RealtorHomeViewModel> BuildHomeAsync(IndorRealtor realtor, CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);

        var properties = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id && p.Status == "Active")
            .OrderByDescending(p => p.UpdatedUtc ?? p.FechaCreacion)
            .Take(5)
            .ToListAsync(ct);

        var quotes = await db.IndorRealtorQuotes.AsNoTracking()
            .Where(q => q.RealtorId == realtor.Id && q.Status == "Pending")
            .OrderByDescending(q => q.RequestedUtc)
            .Take(5)
            .ToListAsync(ct);

        var packages = await db.IndorRealtorSharedPackages.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id)
            .OrderByDescending(p => p.SharedUtc)
            .Take(5)
            .ToListAsync(ct);

        var stats = await BuildHomeStatsAsync(realtor.Id, ct);

        return new RealtorHomeViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            Stats = stats,
            QuickActions =
            [
                new() { Label = "Invite client", Icon = "fa-user-plus", Url = "/RealtorInviteClient/New" },
                new() { Label = "New property file", Icon = "fa-folder-plus", Url = "/RealtorPropertyFile/Details" },
                new() { Label = "Upload inspection report", Icon = "fa-cloud-arrow-up", Url = "/RealtorInspectionUpload/Upload" },
                new() { Label = "Urgent quote", Icon = "fa-comment-dollar", Url = "/RealtorUrgentQuote/Property" }
            ],
            PropertyFiles = properties.Select(MapPropertyCard).ToList(),
            PendingQuotes = quotes.Select(MapQuoteCard).ToList(),
            SharedPackages = packages.Select(MapPackageCard).ToList(),
            Insights = await BuildHomeInsightsAsync(realtor.Id, ct)
        };
    }

    public async Task<RealtorClientsViewModel> BuildClientsAsync(
        IndorRealtor realtor, string? search, string? filter, CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);
        var activeFilter = NormalizeClientFilter(filter);

        var clientsQuery = db.IndorRealtorClients.AsNoTracking()
            .Where(c => c.RealtorId == realtor.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            clientsQuery = clientsQuery.Where(c =>
                c.FullName.Contains(term) ||
                (c.Email != null && c.Email.Contains(term)) ||
                (c.PropertyAddress != null && c.PropertyAddress.Contains(term)));
        }

        clientsQuery = activeFilter switch
        {
            "Buyers" => clientsQuery.Where(c => c.ClientRole == RealtorClientRoles.Buyer),
            "Sellers" => clientsQuery.Where(c => c.ClientRole == RealtorClientRoles.Seller),
            "Homeowners" => clientsQuery.Where(c => c.ClientRole == RealtorClientRoles.Homeowner),
            "Connect" => clientsQuery.Where(c =>
                c.StatusSummary == null ||
                (!c.StatusSummary.Contains("pending") && !c.StatusSummary.Contains("follow"))),
            "Follow-up" => clientsQuery.Where(c =>
                c.StatusSummary != null &&
                (c.StatusSummary.Contains("pending") || c.StatusSummary.Contains("follow"))),
            "Invited" => clientsQuery.Where(c => false),
            _ => clientsQuery
        };

        var clients = await clientsQuery
            .OrderByDescending(c => c.LastActiveUtc)
            .Take(20)
            .ToListAsync(ct);

        var fileCounts = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id && p.ClientName != null)
            .GroupBy(p => p.ClientName!)
            .Select(g => new { ClientName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientName, x => x.Count, ct);

        var quoteCounts = await db.IndorRealtorQuotes.AsNoTracking()
            .Where(q => q.RealtorId == realtor.Id && q.ClientName != null)
            .GroupBy(q => q.ClientName!)
            .Select(g => new { ClientName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClientName, x => x.Count, ct);

        var invitationsQuery = db.IndorRealtorInvitations.AsNoTracking()
            .Where(i => i.RealtorId == realtor.Id && i.Status == RealtorInvitationStatuses.Sent);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            invitationsQuery = invitationsQuery.Where(i =>
                i.FullName.Contains(term) || i.Email.Contains(term));
        }

        var invitations = activeFilter is "All" or "Invited"
            ? await invitationsQuery.OrderByDescending(i => i.SentUtc).Take(10).ToListAsync(ct)
            : [];

        var activities = await db.IndorRealtorActivities.AsNoTracking()
            .Where(a => a.RealtorId == realtor.Id)
            .OrderByDescending(a => a.OccurredUtc)
            .Take(5)
            .ToListAsync(ct);

        var clientStats = await BuildClientStatsAsync(realtor.Id, activeFilter, ct);
        var pendingInviteCount = await db.IndorRealtorInvitations.AsNoTracking()
            .CountAsync(i => i.RealtorId == realtor.Id && i.Status == RealtorInvitationStatuses.Sent, ct);
        var hasAnyClients = await db.IndorRealtorClients.AsNoTracking()
                .AnyAsync(c => c.RealtorId == realtor.Id, ct)
            || pendingInviteCount > 0;

        return new RealtorClientsViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            HasAnyClients = hasAnyClients,
            Stats = clientStats,
            ActiveClients = clients.Select(c => MapClient(c, fileCounts, quoteCounts)).ToList(),
            PendingInvitations = invitations.Select(MapInvitation).ToList(),
            RecentActivity = activities.Select(MapActivity).ToList(),
            NextSteps = BuildClientNextSteps(pendingInviteCount, clients, quoteCounts, activeFilter)
        };
    }

    public async Task<RealtorFilesViewModel> BuildFilesAsync(
        IndorRealtor realtor, string? search, string? filter, CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);
        var activeFilter = NormalizeFileFilter(filter);

        var hasAnyFiles = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .AnyAsync(p => p.RealtorId == realtor.Id, ct);

        var query = db.IndorRealtorPropertyFiles.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Title.Contains(term) ||
                p.Address.Contains(term) ||
                (p.ClientName != null && p.ClientName.Contains(term)) ||
                (p.CityRegion != null && p.CityRegion.Contains(term)));
        }

        query = activeFilter switch
        {
            "Active" => query.Where(p => p.Status == "Active"),
            "Inspection" => query.Where(p => p.FilePhase == "Repair Review" || p.RepairItemsCount > 0),
            "Quotes" => query.Where(p => p.QuotesReceivedCount > 0),
            "Shared" => query.Where(p => p.FilePhase == "Transfer"),
            "Closed" => query.Where(p => p.Status == "Archived"),
            _ => query
        };

        var files = await query
            .OrderByDescending(p => p.UpdatedUtc ?? p.FechaCreacion)
            .Take(20)
            .ToListAsync(ct);

        var activities = await db.IndorRealtorActivities.AsNoTracking()
            .Where(a => a.RealtorId == realtor.Id &&
                        (a.CategoryTag == "Files" || a.ActivityType == "upload" || a.ActivityType == "job"))
            .OrderByDescending(a => a.OccurredUtc)
            .Take(5)
            .ToListAsync(ct);

        return new RealtorFilesViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            Stats = await BuildFileStatsAsync(realtor.Id, ct),
            ActiveFiles = files.Select(MapFile).ToList(),
            RecentActivity = activities.Select(MapActivity).ToList(),
            Insights = await BuildFileInsightsAsync(realtor.Id, ct),
            HasAnyFiles = hasAnyFiles
        };
    }

    public async Task<RealtorQuoteDetailViewModel?> BuildQuoteDetailAsync(
        IndorRealtor realtor, int quoteId, CancellationToken ct = default)
    {
        var quote = await db.IndorRealtorQuotes.AsNoTracking()
            .Include(q => q.SentProviders)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.RealtorId == realtor.Id, ct);

        if (quote == null)
        {
            return null;
        }

        ApplyBidCountsToQuote(quote, await LoadBidCountsByQuoteAsync([quote.Id], ct));

        var shell = await BuildShellCoreAsync(realtor, ct);
        var bids = await db.IndorRealtorQuoteBids.AsNoTracking()
            .Where(b => b.QuoteId == quote.Id)
            .OrderBy(b => b.Amount)
            .ToListAsync(ct);

        var bidProviderNames = bids.Select(b => b.ProviderName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var (statusLabel, statusCss) = DeriveQuoteStatus(quote);
        var requestedServices = await LoadRequestedServicesAsync(quote.PropertyFileId, quote.ServiceType, ct);
        var providersSent = quote.SentProviders.Count;
        var responsesSoFar = quote.ProviderQuotesReceived;

        return new RealtorQuoteDetailViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            QuoteId = quote.Id,
            QuoteStatus = quote.Status,
            QuoteCode = FormatQuoteCode(quote.QuoteCode),
            Address = quote.Address,
            ServiceType = quote.ServiceType,
            ClientName = quote.ClientName ?? "",
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            StatusLabel = statusLabel,
            StatusCss = statusCss,
            RequestedLabel = $"Requested {quote.RequestedUtc.ToLocalTime():MMM d, yyyy}",
            DueLabel = quote.ResponseDeadlineHours is > 0
                ? $"Due {quote.RequestedUtc.AddHours(quote.ResponseDeadlineHours.Value).ToLocalTime():MMM d, yyyy}"
                : null,
            FooterNote = quote.FooterNote ?? DefaultQuoteFooter(quote),
            OptionalMessage = quote.OptionalMessage,
            ProviderQuotesReceived = quote.ProviderQuotesReceived,
            ProvidersSentCount = providersSent,
            PropertyFileId = quote.PropertyFileId,
            RequestedByLabel = "Realtor (Buyer)",
            ProvidersResponseLabel = responsesSoFar == 0
                ? "0 responses so far"
                : $"{responsesSoFar} response{(responsesSoFar == 1 ? "" : "s")} so far",
            RequestedServices = requestedServices,
            InviteProvidersUrl = BuildInviteProvidersUrl(quote),
            Bids = bids.Select(b => new RealtorQuoteDetailBidViewModel
            {
                Id = b.Id,
                ProviderName = b.ProviderName,
                AmountLabel = b.Amount.ToString("C0"),
                Rating = b.Rating,
                SubmittedLabel = b.SubmittedUtc.HasValue
                    ? $"Submitted {b.SubmittedUtc.Value.ToLocalTime():MMM d, yyyy}"
                    : "Submitted recently"
            }).ToList(),
            SentProviders = quote.SentProviders
                .OrderBy(sp => sp.ProviderName)
                .Select(sp => new RealtorQuoteDetailProviderViewModel
                {
                    ProviderName = sp.ProviderName,
                    StatusLabel = bidProviderNames.Contains(sp.ProviderName) ? "Quote received" : "Waiting",
                    StatusCss = bidProviderNames.Contains(sp.ProviderName) ? "received" : "pending"
                })
                .ToList()
        };
    }

    public async Task<RealtorQuotesViewModel> BuildQuotesAsync(
        IndorRealtor realtor, string? search, string? filter, CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);
        var activeFilter = NormalizeQuoteFilter(filter);

        var query = db.IndorRealtorQuotes.AsNoTracking()
            .Where(q => q.RealtorId == realtor.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(q =>
                q.QuoteCode.Contains(term) ||
                q.Address.Contains(term) ||
                q.ServiceType.Contains(term) ||
                (q.ClientName != null && q.ClientName.Contains(term)));
        }

        if (activeFilter == "Urgent")
        {
            var urgentCutoff = DateTime.UtcNow.AddDays(-4);
            query = query.Where(q => q.Status == "Pending" && q.RequestedUtc <= urgentCutoff);
        }
        else if (activeFilter != "All")
        {
            var dbStatus = MapQuoteFilterToStatus(activeFilter);
            query = query.Where(q => q.Status == dbStatus);
        }

        var quotes = await query
            .OrderByDescending(q => q.UpdatedUtc ?? q.RequestedUtc)
            .Take(20)
            .ToListAsync(ct);

        var bidCounts = await LoadBidCountsByQuoteAsync(quotes.Select(q => q.Id).ToList(), ct);
        foreach (var quote in quotes)
        {
            ApplyBidCountsToQuote(quote, bidCounts);
        }

        var compareQuote = quotes.FirstOrDefault(q => q.ProviderQuotesReceived > 0)
            ?? await db.IndorRealtorQuotes.AsNoTracking()
                .Where(q => q.RealtorId == realtor.Id && q.ProviderQuotesReceived > 0)
                .OrderByDescending(q => q.UpdatedUtc ?? q.RequestedUtc)
                .FirstOrDefaultAsync(ct);

        if (compareQuote != null && compareQuote.ProviderQuotesReceived == 0)
        {
            ApplyBidCountsToQuote(compareQuote, await LoadBidCountsByQuoteAsync([compareQuote.Id], ct));
        }

        RealtorCompareQuotesViewModel? compare = null;
        if (compareQuote != null)
        {
            var bids = await db.IndorRealtorQuoteBids.AsNoTracking()
                .Where(b => b.QuoteId == compareQuote.Id)
                .OrderBy(b => b.Amount)
                .Take(5)
                .ToListAsync(ct);

            compare = new RealtorCompareQuotesViewModel
            {
                QuoteId = compareQuote.Id,
                Address = compareQuote.Address,
                ServiceType = compareQuote.ServiceType,
                Bids = bids.Select(b => new RealtorQuoteBidViewModel
                {
                    ProviderName = b.ProviderName,
                    AmountLabel = b.Amount.ToString("C0"),
                    Rating = b.Rating
                }).ToList()
            };
        }

        var activities = await db.IndorRealtorActivities.AsNoTracking()
            .Where(a => a.RealtorId == realtor.Id &&
                        (a.CategoryTag == "Quotes" || a.CategoryTag == "Providers" || a.CategoryTag == "Clients"))
            .OrderByDescending(a => a.OccurredUtc)
            .Take(5)
            .ToListAsync(ct);

        var quoteStats = await BuildQuoteStatsAsync(realtor.Id, ct);
        var openQuotes = quotes.Select(MapOpenQuote).ToList();
        if (openQuotes.Count > 0)
        {
            var quoteIds = openQuotes.Select(q => q.Id).ToList();
            var topBids = await db.IndorRealtorQuoteBids.AsNoTracking()
                .Where(b => quoteIds.Contains(b.QuoteId))
                .OrderBy(b => b.Amount)
                .ToListAsync(ct);

            foreach (var card in openQuotes)
            {
                var topBid = topBids.FirstOrDefault(b => b.QuoteId == card.Id);
                if (topBid != null)
                {
                    card.ProviderInitials = DeriveInitialsFromName(topBid.ProviderName);
                    if (card.ProviderQuotesReceived == 1)
                    {
                        card.ProviderSummary = $"{card.ProviderInitials} 1 provider quote received";
                    }
                }
            }
        }

        return new RealtorQuotesViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            Stats = quoteStats,
            OpenQuotes = openQuotes,
            CompareQuotes = compare,
            RecentActivity = activities.Select(MapActivity).ToList(),
            Alerts = BuildQuoteAlerts(quoteStats)
        };
    }

    public string? ResolveQuoteFlowUrl(IndorRealtorQuote quote)
    {
        if (quote.Status == "Accepted")
        {
            return $"/Realtor/QuoteSelected/{quote.Id}";
        }

        if (quote.ProviderQuotesReceived >= 2 || quote.Status == "Compare")
        {
            return $"/Realtor/CompareQuotes/{quote.Id}";
        }

        if (quote.ProviderQuotesReceived == 1)
        {
            return $"/Realtor/ViewQuote/{quote.Id}";
        }

        return $"/Realtor/QuoteDetail/{quote.Id}";
    }

    public async Task<RealtorViewQuoteViewModel?> BuildViewQuoteAsync(
        IndorRealtor realtor, int quoteId, int? bidId, CancellationToken ct = default)
    {
        var quote = await LoadOwnedQuoteAsync(realtor.Id, quoteId, ct);
        if (quote == null)
        {
            return null;
        }

        ApplyBidCountsToQuote(quote, await LoadBidCountsByQuoteAsync([quote.Id], ct));

        var bids = await db.IndorRealtorQuoteBids.AsNoTracking()
            .Where(b => b.QuoteId == quote.Id)
            .OrderBy(b => b.Amount)
            .ToListAsync(ct);

        if (bids.Count == 0)
        {
            return null;
        }

        if (bids.Count > 1 && bidId is null)
        {
            return null;
        }

        var bid = bidId is > 0
            ? bids.FirstOrDefault(b => b.Id == bidId) ?? bids[0]
            : bids[0];

        var shell = await BuildShellCoreAsync(realtor, ct);
        var details = await LoadBidEstimateDetailsAsync(bid, ct);
        var (statusLabel, _) = DeriveQuoteStatus(quote);

        return new RealtorViewQuoteViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            QuoteId = quote.Id,
            BidId = bid.Id,
            Address = quote.Address,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            StatusLabel = statusLabel,
            ProviderName = bid.ProviderName,
            ProviderInitials = DeriveInitialsFromName(bid.ProviderName),
            TotalLabel = bid.Amount.ToString("C0"),
            TimelineLabel = details.Timeline,
            WarrantyLabel = details.Warranty,
            ScopeOfWork = details.ScopeOfWork,
            IncludedRepairs = details.IncludedRepairs,
            PriceLines = details.PriceLines,
            TotalAmountLabel = bid.Amount.ToString("C2"),
            CompareQuotesUrl = bids.Count > 1 ? $"/Realtor/CompareQuotes/{quote.Id}" : "#",
            RequestAnotherUrl = BuildInviteProvidersUrl(quote),
            EditSharedQuoteUrl = $"/Realtor/EditSharedQuote?quoteId={quote.Id}&bidId={bid.Id}"
        };
    }

    public async Task<RealtorCompareQuotesPageViewModel?> BuildCompareQuotesPageAsync(
        IndorRealtor realtor, int quoteId, CancellationToken ct = default)
    {
        var quote = await LoadOwnedQuoteAsync(realtor.Id, quoteId, ct);
        if (quote == null)
        {
            return null;
        }

        ApplyBidCountsToQuote(quote, await LoadBidCountsByQuoteAsync([quote.Id], ct));

        var bids = await db.IndorRealtorQuoteBids.AsNoTracking()
            .Where(b => b.QuoteId == quote.Id)
            .OrderBy(b => b.Amount)
            .ToListAsync(ct);

        if (bids.Count < 2)
        {
            return null;
        }

        var shell = await BuildShellCoreAsync(realtor, ct);
        var cards = new List<RealtorCompareQuoteCardViewModel>();
        var timelines = new List<string>();
        var bestBidId = bids[0].Id;

        foreach (var bid in bids)
        {
            var details = await LoadBidEstimateDetailsAsync(bid, ct);
            timelines.Add(details.Timeline);
            cards.Add(new RealtorCompareQuoteCardViewModel
            {
                BidId = bid.Id,
                ProviderName = bid.ProviderName,
                ProviderInitials = DeriveInitialsFromName(bid.ProviderName),
                AmountLabel = bid.Amount.ToString("C0"),
                Rating = bid.Rating,
                ReviewCount = DeriveReviewCount(bid.Rating),
                TimelineLabel = details.Timeline,
                WarrantyLabel = details.Warranty,
                IsBestValue = bid.Id == bestBidId,
                ViewDetailsUrl = $"/Realtor/ViewQuote/{quote.Id}?bidId={bid.Id}"
            });
        }

        var min = bids.Min(b => b.Amount);
        var max = bids.Max(b => b.Amount);

        return new RealtorCompareQuotesPageViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            QuoteId = quote.Id,
            Address = quote.Address,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            StatusLabel = $"{quote.ProviderQuotesReceived} Quotes Received",
            RequestedLabel = $"Requested {quote.RequestedUtc.ToLocalTime():MMM d, yyyy}",
            PriceRangeLabel = $"{min:C0} – {max:C0}",
            TimelineRangeLabel = BuildTimelineRange(timelines),
            InviteProvidersUrl = BuildInviteProvidersUrl(quote),
            Quotes = cards
        };
    }

    public async Task<RealtorQuoteSelectedViewModel?> BuildQuoteSelectedAsync(
        IndorRealtor realtor, int quoteId, CancellationToken ct = default)
    {
        var quote = await LoadOwnedQuoteAsync(realtor.Id, quoteId, ct);
        if (quote == null || quote.Status != "Accepted")
        {
            return null;
        }

        var bid = quote.SelectedBidId is > 0
            ? await db.IndorRealtorQuoteBids.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == quote.SelectedBidId && b.QuoteId == quote.Id, ct)
            : await db.IndorRealtorQuoteBids.AsNoTracking()
                .Where(b => b.QuoteId == quote.Id)
                .OrderBy(b => b.Amount)
                .FirstOrDefaultAsync(ct);

        if (bid == null)
        {
            return null;
        }

        var shell = await BuildShellCoreAsync(realtor, ct);
        var details = await LoadBidEstimateDetailsAsync(bid, ct);
        var approvedUtc = quote.AcceptedUtc ?? DateTime.UtcNow;

        return new RealtorQuoteSelectedViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            QuoteId = quote.Id,
            BidId = bid.Id,
            Address = quote.Address,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            ProviderName = bid.ProviderName,
            TotalAmountLabel = bid.Amount.ToString("C0"),
            ApprovedLabel = approvedUtc.ToLocalTime().ToString("MMM d, yyyy"),
            TimelineLabel = details.Timeline,
            WarrantyLabel = details.Warranty,
            ViewQuoteUrl = $"/Realtor/ViewQuote/{quote.Id}?bidId={bid.Id}",
            NextSteps =
            [
                new() { Label = "Schedule work", Icon = "fa-calendar-days" },
                new() { Label = "Send repair approval to client", Icon = "fa-paper-plane" },
                new() { Label = "Track job progress", Icon = "fa-chart-column" },
                new() { Label = "Convert to invoice after completion", Icon = "fa-file-invoice-dollar" }
            ]
        };
    }

    public async Task<bool> AcceptQuoteAsync(IndorRealtor realtor, int quoteId, int bidId, CancellationToken ct = default)
    {
        var quote = await db.IndorRealtorQuotes
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.RealtorId == realtor.Id, ct);

        if (quote == null)
        {
            return false;
        }

        var bid = await db.IndorRealtorQuoteBids.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bidId && b.QuoteId == quote.Id, ct);

        if (bid == null)
        {
            return false;
        }

        quote.Status = "Accepted";
        quote.SelectedBidId = bid.Id;
        quote.AcceptedUtc = DateTime.UtcNow;
        quote.Amount = bid.Amount;
        quote.FooterNote = $"Selected: {bid.ProviderName}";
        quote.UpdatedUtc = DateTime.UtcNow;

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "quote",
            Description = $"Quote selected for {quote.Address} — {bid.ProviderName} ({bid.Amount:C0})",
            CategoryTag = "Quotes",
            OccurredUtc = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<RealtorProfileViewModel> BuildProfileAsync(IndorRealtor realtor, CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);

        var docs = await db.IndorRealtorDocumentos.AsNoTracking()
            .Where(d => d.RealtorId == realtor.Id)
            .ToListAsync(ct);

        var docSlots = RealtorDocumentTypes.Slots.Select(slot =>
        {
            var row = docs.FirstOrDefault(d =>
                d.DocumentType.Equals(slot.Type, StringComparison.OrdinalIgnoreCase));
            return new RealtorProfileDocumentViewModel
            {
                DocumentType = slot.Type,
                Label = slot.Label,
                Uploaded = !string.IsNullOrWhiteSpace(row?.FileUrl),
                Optional = !slot.Required
            };
        }).ToList();

        var profilePhotoUploaded = !string.IsNullOrWhiteSpace(realtor.ProfilePhotoUrl);
        docSlots.Add(new RealtorProfileDocumentViewModel
        {
            DocumentType = "profile_photo",
            Label = "Profile Photo",
            Uploaded = profilePhotoUploaded,
            Optional = false
        });

        var homeStats = await BuildHomeStatsAsync(realtor.Id, ct);
        var clientCount = await db.IndorRealtorClients.AsNoTracking()
            .CountAsync(c => c.RealtorId == realtor.Id, ct);
        var quoteRequestCount = await db.IndorRealtorQuotes.AsNoTracking()
            .CountAsync(q => q.RealtorId == realtor.Id, ct);
        var filesThisMonth = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtor.Id &&
                             (p.UpdatedUtc ?? p.FechaCreacion) >= DateTime.UtcNow.AddDays(-30), ct);
        var prevMonthFiles = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtor.Id &&
                             (p.UpdatedUtc ?? p.FechaCreacion) >= DateTime.UtcNow.AddDays(-60) &&
                             (p.UpdatedUtc ?? p.FechaCreacion) < DateTime.UtcNow.AddDays(-30), ct);

        var filesTrend = prevMonthFiles > 0
            ? $"+{Math.Round((filesThisMonth - prevMonthFiles) * 100.0 / prevMonthFiles)}% vs last month"
            : filesThisMonth > 0 ? "+100% vs last month" : "";

        var notificationPreferences = await LoadNotificationPreferencesAsync(realtor.Id, ct);

        return new RealtorProfileViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            FullName = realtor.DisplayName ?? shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            Email = realtor.Email ?? "",
            Phone = realtor.Phone ?? "",
            BrokerageName = realtor.BrokerageName ?? "",
            LicenseNumber = realtor.LicenseNumber ?? "",
            LicenseState = realtor.LicenseState ?? "",
            ServiceAreas = realtor.ServiceAreas ?? "",
            CanUpgradeToVerified = realtor.RegistrationStatus != RealtorRegistrationStatuses.Verified,
            Documents = docSlots,
            Stats =
            [
                homeStats[0],
                new RealtorStatItemViewModel { Label = "Clients", Count = clientCount, Icon = "fa-users", ColorClass = "teal" },
                homeStats[2],
                new RealtorStatItemViewModel { Label = "Quote Requests", Count = quoteRequestCount, Icon = "fa-comment-dollar", ColorClass = "orange" }
            ],
            FilesThisMonth = filesThisMonth,
            FilesTrendLabel = filesTrend,
            ClientConnections = clientCount,
            ClientsTrendLabel = clientCount > 0 ? $"+{Math.Min(clientCount, 5)} this month" : "",
            Insights = await BuildHomeInsightsAsync(realtor.Id, ct),
            EmailAlertsEnabled = notificationPreferences.EmailAlertsEnabled,
            QuoteUpdatesEnabled = notificationPreferences.QuoteUpdatesEnabled,
            ReportNotificationsEnabled = notificationPreferences.ReportNotificationsEnabled,
            PackageViewAlertsEnabled = notificationPreferences.PackageViewAlertsEnabled
        };
    }

    public async Task SaveNotificationPreferencesAsync(
        IndorRealtor realtor,
        RealtorNotificationPreferencesInput input,
        CancellationToken ct = default)
    {
        SaveNotificationPreferencesToSession(realtor.Id, input);

        if (!await TrySaveNotificationPreferencesToDatabaseAsync(realtor.Id, input, ct))
        {
            return;
        }
    }

    private async Task<RealtorNotificationPreferencesInput> LoadNotificationPreferencesAsync(
        int realtorId,
        CancellationToken ct)
    {
        var fromDatabase = await TryLoadNotificationPreferencesFromDatabaseAsync(realtorId, ct);
        if (fromDatabase != null)
        {
            SaveNotificationPreferencesToSession(realtorId, fromDatabase);
            return fromDatabase;
        }

        return LoadNotificationPreferencesFromSession(realtorId) ?? DefaultNotificationPreferences();
    }

    private async Task<RealtorNotificationPreferencesInput?> TryLoadNotificationPreferencesFromDatabaseAsync(
        int realtorId,
        CancellationToken ct)
    {
        var connectionString = db.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT NotifyEmailAlerts, NotifyQuoteUpdates, NotifyReportNotifications, NotifyPackageViewAlerts
                FROM IndorRealtors
                WHERE Id = @id
                """;
            command.Parameters.Add(new SqlParameter("@id", realtorId));

            await using var reader = await command.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            return new RealtorNotificationPreferencesInput
            {
                EmailAlertsEnabled = reader.GetBoolean(0),
                QuoteUpdatesEnabled = reader.GetBoolean(1),
                ReportNotificationsEnabled = reader.GetBoolean(2),
                PackageViewAlertsEnabled = reader.GetBoolean(3)
            };
        }
        catch (SqlException ex) when (IsMissingNotificationColumnException(ex))
        {
            return null;
        }
    }

    private async Task<bool> TrySaveNotificationPreferencesToDatabaseAsync(
        int realtorId,
        RealtorNotificationPreferencesInput input,
        CancellationToken ct)
    {
        var connectionString = db.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE IndorRealtors
                SET NotifyEmailAlerts = @emailAlerts,
                    NotifyQuoteUpdates = @quoteUpdates,
                    NotifyReportNotifications = @reportNotifications,
                    NotifyPackageViewAlerts = @packageViewAlerts,
                    FechaActualizacion = SYSUTCDATETIME()
                WHERE Id = @id
                """;
            command.Parameters.Add(new SqlParameter("@emailAlerts", input.EmailAlertsEnabled));
            command.Parameters.Add(new SqlParameter("@quoteUpdates", input.QuoteUpdatesEnabled));
            command.Parameters.Add(new SqlParameter("@reportNotifications", input.ReportNotificationsEnabled));
            command.Parameters.Add(new SqlParameter("@packageViewAlerts", input.PackageViewAlertsEnabled));
            command.Parameters.Add(new SqlParameter("@id", realtorId));

            await command.ExecuteNonQueryAsync(ct);
            return true;
        }
        catch (SqlException ex) when (IsMissingNotificationColumnException(ex))
        {
            return false;
        }
    }

    private static bool IsMissingNotificationColumnException(SqlException ex) =>
        ex.Number == 207 &&
        ex.Message.Contains("Notify", StringComparison.OrdinalIgnoreCase);

    private RealtorNotificationPreferencesInput? LoadNotificationPreferencesFromSession(int realtorId)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return null;
        }

        var json = session.GetString(NotificationSessionKey(realtorId));
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<RealtorNotificationPreferencesInput>(json);
    }

    private void SaveNotificationPreferencesToSession(int realtorId, RealtorNotificationPreferencesInput input)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        session.SetString(NotificationSessionKey(realtorId), JsonSerializer.Serialize(input));
    }

    private static string NotificationSessionKey(int realtorId) =>
        NotificationSessionKeyPrefix + realtorId;

    public async Task<RealtorBusinessInformationViewModel> BuildBusinessInformationAsync(
        IndorRealtor realtor,
        CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);

        var entity = await db.IndorRealtors.AsNoTracking()
            .Include(r => r.Documentos)
            .FirstAsync(r => r.Id == realtor.Id, ct);

        var hasLicensePhoto = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasGovId = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId &&
            !string.IsNullOrWhiteSpace(d.FileUrl));

        var businessName = entity.DisplayName?.Trim() ?? "";
        var brokerage = entity.BrokerageName?.Trim() ?? "";
        var email = entity.Email?.Trim() ?? "";
        var officeAddress = entity.OfficeAddress?.Trim() ?? "";
        var languages = FormatLanguagesLabel(entity.LanguagesJson);
        var licenseNumber = entity.LicenseNumber?.Trim() ?? "";
        var licenseState = entity.LicenseState?.Trim() ?? "";
        var hasLicense = !string.IsNullOrWhiteSpace(licenseNumber) && !string.IsNullOrWhiteSpace(licenseState);
        var licenseDisplay = hasLicense
            ? string.IsNullOrWhiteSpace(licenseState) ? licenseNumber : $"{licenseState}{licenseNumber}"
            : "";

        var (licenseBadge, licenseCss) = ResolveLicenseStatus(entity, hasLicensePhoto, hasGovId, hasLicense);
        var messagingLabel = entity.IndorMessagingEnabled ? "Enabled" : "Disabled";

        var completionChecks = new[]
        {
            !string.IsNullOrWhiteSpace(businessName),
            !string.IsNullOrWhiteSpace(brokerage),
            hasLicense,
            !string.IsNullOrWhiteSpace(email),
            entity.IndorMessagingEnabled,
            !string.IsNullOrWhiteSpace(officeAddress),
            !string.IsNullOrWhiteSpace(languages)
        };
        var completionPercent = (int)Math.Round(completionChecks.Count(c => c) / (double)completionChecks.Length * 100);

        var rows = new List<RealtorBusinessInfoRowViewModel>
        {
            BuildBusinessInfoRow(
                "business-name",
                "Business Name",
                businessName,
                "fa-store",
                "/Realtor/EditProfileContact",
                "Add business name"),
            BuildBusinessInfoRow(
                "brokerage",
                "Brokerage",
                brokerage,
                "fa-user-group",
                "/Realtor/EditProfileContact",
                "Add brokerage"),
            new RealtorBusinessInfoRowViewModel
            {
                Key = "license",
                Label = "License Status",
                Value = licenseDisplay,
                StatusBadge = licenseBadge,
                StatusCss = licenseCss,
                Icon = "fa-file-circle-check",
                EditUrl = "/Realtor/EditProfileLicense",
                IsEmpty = !hasLicense && string.IsNullOrWhiteSpace(licenseBadge)
            },
            BuildBusinessInfoRow(
                "email",
                "Email",
                email,
                "fa-envelope",
                null,
                "Add email"),
            new RealtorBusinessInfoRowViewModel
            {
                Key = "messaging",
                Label = "INDOR Messaging",
                Value = messagingLabel,
                Icon = "fa-comment-dots",
                IsEmpty = false
            },
            BuildBusinessInfoRow(
                "office-address",
                "Office Address",
                officeAddress,
                "fa-location-dot",
                "/Realtor/EditProfileContact",
                "Add office address"),
            BuildBusinessInfoRow(
                "languages",
                "Languages",
                languages,
                "fa-globe",
                "/Realtor/EditProfileContact",
                "Add languages")
        };

        return new RealtorBusinessInformationViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            DisplayStep = 1,
            TotalSteps = 4,
            Title = "Business Information",
            Subtitle = "Set up your business details and professional identity.",
            HeaderBadge = "Start here",
            BackAction = "Profile",
            BackController = "Realtor",
            ProfileCompletionPercent = completionPercent,
            Rows = rows
        };
    }

    public async Task<RealtorEditProfileContactViewModel> BuildEditProfileContactAsync(
        IndorRealtor realtor,
        IReadOnlyList<string> licenseStates,
        CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);
        var entity = await db.IndorRealtors.AsNoTracking()
            .FirstAsync(r => r.Id == realtor.Id, ct);

        return new RealtorEditProfileContactViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            DisplayStep = 2,
            TotalSteps = 4,
            Title = "Company & Contact Details",
            Subtitle = "Add the contact and public business information clients will see.",
            HeaderBadge = "Start here",
            BackAction = RealtorEditProfileActions.BusinessInformation,
            BackController = "Realtor",
            BusinessName = entity.DisplayName ?? "",
            PublicDisplayName = entity.PublicDisplayName ?? "",
            BrokerageName = entity.BrokerageName ?? "",
            RealtorTitle = entity.RealtorTitle ?? "Realtor®",
            Email = entity.Email ?? "",
            Website = entity.Website ?? "",
            OfficeAddress = entity.OfficeAddress ?? "",
            OfficeCity = entity.OfficeCity ?? "",
            OfficeState = entity.OfficeState ?? "",
            OfficeZip = entity.OfficeZip ?? "",
            Languages = DeserializeStringList(entity.LanguagesJson),
            LanguagesCsv = FormatLanguagesLabel(entity.LanguagesJson),
            LicenseStates = licenseStates
        };
    }

    public async Task SaveEditProfileContactAsync(
        IndorRealtor realtor,
        RealtorEditProfileContactViewModel input,
        CancellationToken ct = default)
    {
        var entity = await db.IndorRealtors.FirstAsync(r => r.Id == realtor.Id, ct);

        entity.DisplayName = input.BusinessName.Trim();
        entity.PublicDisplayName = input.PublicDisplayName?.Trim() ?? "";
        entity.BrokerageName = input.BrokerageName.Trim();
        entity.RealtorTitle = input.RealtorTitle?.Trim() ?? "";
        entity.Email = input.Email.Trim();
        entity.Website = input.Website?.Trim() ?? "";
        entity.OfficeAddress = input.OfficeAddress?.Trim() ?? "";
        entity.OfficeCity = input.OfficeCity?.Trim() ?? "";
        entity.OfficeState = input.OfficeState?.Trim() ?? "";
        entity.OfficeZip = input.OfficeZip?.Trim() ?? "";
        entity.LanguagesJson = SerializeStringList(ParseLanguagesCsv(input.LanguagesCsv ?? ""));
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task<RealtorEditProfileLicenseViewModel> BuildEditProfileLicenseAsync(
        IndorRealtor realtor,
        IReadOnlyList<string> licenseStates,
        CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);
        var entity = await db.IndorRealtors.AsNoTracking()
            .Include(r => r.Documentos)
            .FirstAsync(r => r.Id == realtor.Id, ct);

        var docs = entity.Documentos.ToList();
        var hasLicensePhoto = docs.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasGovId = docs.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasLicenseNumber = !string.IsNullOrWhiteSpace(entity.LicenseNumber) &&
                               !string.IsNullOrWhiteSpace(entity.LicenseState);

        var documentSlots = RealtorDocumentTypes.Slots.Select(slot =>
        {
            var row = docs.FirstOrDefault(d =>
                d.DocumentType.Equals(slot.Type, StringComparison.OrdinalIgnoreCase));
            return new RealtorDocumentSlotViewModel
            {
                DocumentType = slot.Type,
                Label = slot.Label,
                Required = slot.Required,
                Uploaded = !string.IsNullOrWhiteSpace(row?.FileUrl),
                FileUrl = row?.FileUrl
            };
        }).ToList();

        return new RealtorEditProfileLicenseViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            DisplayStep = 3,
            TotalSteps = 4,
            Title = "License & Professional Details",
            Subtitle = "Build trust with licensing, experience, specialties, and verification.",
            HeaderBadge = "Start here",
            BackAction = RealtorEditProfileActions.EditProfileContact,
            BackController = "Realtor",
            LicenseNumber = entity.LicenseNumber ?? "",
            LicenseState = entity.LicenseState ?? "",
            YearsOfExperience = entity.YearsOfExperience ?? "",
            SelectedSpecialties = DeserializeStringList(entity.SpecialtiesJson),
            TeamName = entity.TeamName ?? "",
            BrokerInCharge = entity.BrokerInCharge ?? "",
            DocumentSlots = documentSlots,
            VerificationItems =
            [
                new() { Label = "Licensed Realtor", IsVerified = hasLicenseNumber && hasLicensePhoto },
                new() { Label = "Background Verified", IsVerified = hasGovId },
                new() { Label = "INDOR Verified", IsVerified = shell.IsVerified }
            ],
            SpecialtyOptions = RealtorEditProfileOptions.Specialties,
            ExperienceOptions = RealtorEditProfileOptions.ExperienceLevels,
            LicenseStates = licenseStates
        };
    }

    public async Task SaveEditProfileLicenseAsync(
        IndorRealtor realtor,
        RealtorEditProfileLicenseViewModel input,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.LicenseState))
        {
            throw new InvalidOperationException("License state is required.");
        }

        if (!RealtorLicenseNumberAttribute.IsValidLicenseNumber(input.LicenseNumber, out var licenseError))
        {
            throw new InvalidOperationException(licenseError ?? "Enter a valid license number.");
        }

        var entity = await db.IndorRealtors.FirstAsync(r => r.Id == realtor.Id, ct);

        entity.LicenseNumber = input.LicenseNumber.Trim();
        entity.LicenseState = input.LicenseState.Trim();
        entity.YearsOfExperience = input.YearsOfExperience?.Trim() ?? "";
        entity.SpecialtiesJson = SerializeStringList(input.SelectedSpecialties.Take(3).ToList());
        entity.TeamName = input.TeamName?.Trim() ?? "";
        entity.BrokerInCharge = input.BrokerInCharge?.Trim() ?? "";
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task<RealtorEditProfileReviewViewModel> BuildEditProfileReviewAsync(
        IndorRealtor realtor,
        CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);
        var entity = await db.IndorRealtors.AsNoTracking()
            .Include(r => r.Documentos)
            .FirstAsync(r => r.Id == realtor.Id, ct);

        var hasLicensePhoto = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasGovId = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasLicense = !string.IsNullOrWhiteSpace(entity.LicenseNumber) &&
                         !string.IsNullOrWhiteSpace(entity.LicenseState);
        var licenseDisplay = hasLicense
            ? $"{entity.LicenseState}{entity.LicenseNumber}"
            : "";
        var (licenseBadge, licenseCss) = ResolveLicenseStatus(entity, hasLicensePhoto, hasGovId, hasLicense);
        var serviceAreas = ParseServiceAreas(entity.ServiceAreas);
        var serviceAreaLabel = serviceAreas.Count > 0
            ? $"{string.Join(", ", serviceAreas.Take(2))}{(serviceAreas.Count > 2 ? " and surrounding areas" : "")}"
            : BuildOfficeLocationLabel(entity);

        var messagingLines = new List<string>();
        if (!string.IsNullOrWhiteSpace(entity.Email))
        {
            messagingLines.Add(entity.Email.Trim());
        }

        if (entity.IndorMessagingEnabled)
        {
            messagingLines.Add("INDOR Messaging Enabled");
        }

        var publicName = !string.IsNullOrWhiteSpace(entity.PublicDisplayName)
            ? entity.PublicDisplayName.Trim()
            : entity.DisplayName?.Trim() ?? shell.FullDisplayName;

        return new RealtorEditProfileReviewViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            DisplayStep = 4,
            TotalSteps = 4,
            Title = "Review & Save",
            Subtitle = "Review your business information before updating your public profile.",
            HeaderBadge = "Final Step",
            BackAction = RealtorEditProfileActions.EditProfileLicense,
            BackController = "Realtor",
            SummaryRows =
            [
                new()
                {
                    Label = "Business Name",
                    Value = entity.DisplayName ?? "",
                    Icon = "fa-store",
                    EditAction = RealtorEditProfileActions.EditProfileContact
                },
                new()
                {
                    Label = "Brokerage",
                    Value = entity.BrokerageName ?? "",
                    Icon = "fa-user-group",
                    EditAction = RealtorEditProfileActions.EditProfileContact
                },
                new()
                {
                    Label = "Service Area (Preview)",
                    Value = serviceAreaLabel,
                    Icon = "fa-globe",
                    EditAction = RealtorEditProfileActions.EditProfileContact
                },
                new()
                {
                    Label = "Contact & Messaging",
                    Value = string.Join(" · ", messagingLines),
                    Icon = "fa-comment-dots",
                    EditAction = RealtorEditProfileActions.EditProfileContact
                },
                new()
                {
                    Label = "Verification",
                    Value = licenseDisplay,
                    StatusBadge = licenseBadge,
                    StatusCss = licenseCss,
                    Icon = "fa-shield-halved",
                    EditAction = RealtorEditProfileActions.EditProfileLicense
                }
            ],
            Preview = new RealtorEditProfilePreviewViewModel
            {
                FullName = publicName,
                BrokerageName = entity.BrokerageName?.Trim() ?? "",
                LocationLabel = serviceAreaLabel,
                ProfilePhotoUrl = shell.ProfilePhotoUrl
            }
        };
    }

    public async Task FinalizeEditProfileAsync(IndorRealtor realtor, CancellationToken ct = default)
    {
        var entity = await db.IndorRealtors
            .Include(r => r.Documentos)
            .FirstAsync(r => r.Id == realtor.Id, ct);

        var hasLicensePhoto = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasGovId = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId &&
            !string.IsNullOrWhiteSpace(d.FileUrl));

        if (!entity.VerificationSkipped && hasLicensePhoto && hasGovId)
        {
            entity.RegistrationStatus = RealtorRegistrationStatuses.Verified;
        }
        else if (entity.RegistrationStatus == RealtorRegistrationStatuses.Draft)
        {
            entity.RegistrationStatus = RealtorRegistrationStatuses.Basic;
        }

        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<RealtorPublicProfileViewModel> BuildPublicProfileAsync(
        IndorRealtor realtor,
        CancellationToken ct = default)
    {
        var shell = await BuildShellCoreAsync(realtor, ct);

        var docs = await db.IndorRealtorDocumentos.AsNoTracking()
            .Where(d => d.RealtorId == realtor.Id)
            .ToListAsync(ct);

        var hasLicenseDoc = docs.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasGovId = docs.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId &&
            !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasLicenseNumber = !string.IsNullOrWhiteSpace(realtor.LicenseNumber) &&
                               !string.IsNullOrWhiteSpace(realtor.LicenseState);

        var networkItems = await db.IndorNearbyNetworkItems.AsNoTracking()
            .Where(i => i.OwnerRealtorId == realtor.Id && i.IsOwnedListing && i.IsActive)
            .OrderByDescending(i => i.UpdatedUtc ?? i.CreatedUtc)
            .ToListAsync(ct);

        var listings = networkItems
            .Where(i => i.CardType == NearbyNetworkCardTypes.Listing)
            .Select(i => MapPublicListing(i))
            .ToList();

        var openHouses = networkItems
            .Where(i => i.CardType == NearbyNetworkCardTypes.OpenHouse)
            .Select(i => MapPublicOpenHouse(i))
            .ToList();

        var serviceAreas = ParseServiceAreas(realtor.ServiceAreas);
        var clientCount = await db.IndorRealtorClients.AsNoTracking()
            .CountAsync(c => c.RealtorId == realtor.Id, ct);

        var packages = await db.IndorRealtorSharedPackages.AsNoTracking()
            .Where(p => p.RealtorId == realtor.Id)
            .OrderByDescending(p => p.SharedUtc)
            .Take(12)
            .ToListAsync(ct);

        var coverImage = listings.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.ImageUrl))?.ImageUrl
                         ?? openHouses.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.ImageUrl))?.ImageUrl;

        var licenseLabel = hasLicenseNumber
            ? $"License #{realtor.LicenseNumber} ({realtor.LicenseState})"
            : null;

        var heroLocation = serviceAreas.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(heroLocation))
        {
            var officeParts = new[] { realtor.OfficeCity, realtor.OfficeState }
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p!.Trim());
            heroLocation = string.Join(", ", officeParts);
        }
        else if (!heroLocation.Contains(',') && !string.IsNullOrWhiteSpace(realtor.LicenseState))
        {
            heroLocation = $"{heroLocation.Trim()}, {realtor.LicenseState.Trim()}";
        }

        var licensedComplete = hasLicenseDoc && hasLicenseNumber;
        var backgroundComplete = hasGovId;
        var indorVerified = shell.IsVerified;

        var accountStatusLabel = "";
        var accountStatusCss = "";
        if (realtor.RegistrationStatus is RealtorRegistrationStatuses.Verified or RealtorRegistrationStatuses.Basic
            && hasLicenseNumber)
        {
            accountStatusLabel = "Active";
            accountStatusCss = "is-active";
        }

        return new RealtorPublicProfileViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            IsOwnProfile = true,
            FullName = !string.IsNullOrWhiteSpace(realtor.PublicDisplayName)
                ? realtor.PublicDisplayName.Trim()
                : realtor.DisplayName ?? shell.FullDisplayName,
            TitleLabel = !string.IsNullOrWhiteSpace(realtor.RealtorTitle)
                ? realtor.RealtorTitle.Trim()
                : shell.IsVerified ? "Realtor®" : "Realtor",
            Tagline = string.IsNullOrWhiteSpace(realtor.PublicTagline) ? null : realtor.PublicTagline.Trim(),
            Bio = string.IsNullOrWhiteSpace(realtor.PublicBio) ? null : realtor.PublicBio.Trim(),
            CoverImageUrl = coverImage,
            LocationLabel = serviceAreas.Count > 0
                ? $"Serving {string.Join(", ", serviceAreas.Take(3))}{(serviceAreas.Count > 3 ? " and surrounding areas" : "")}"
                : null,
            BrokerageName = string.IsNullOrWhiteSpace(realtor.BrokerageName) ? null : realtor.BrokerageName.Trim(),
            LicenseLabel = licenseLabel,
            HeroLocationLabel = heroLocation ?? "",
            AccountStatusLabel = accountStatusLabel,
            AccountStatusCss = accountStatusCss,
            ShowVerificationPrompt = !indorVerified,
            HeroBadges =
            [
                new() { Label = "Licensed", IsComplete = licensedComplete },
                new() { Label = "Background Checked", IsComplete = backgroundComplete },
                new() { Label = "INDOR Member", IsComplete = indorVerified || realtor.RegistrationStatus == RealtorRegistrationStatuses.Basic }
            ],
            VerificationSteps =
            [
                new() { Label = "License Verification", IsComplete = licensedComplete, StatusLabel = licensedComplete ? "Completed" : "Not Started" },
                new() { Label = "Background Check", IsComplete = backgroundComplete, StatusLabel = backgroundComplete ? "Completed" : "Not Started" },
                new() { Label = "INDOR Verified Status", IsComplete = indorVerified, StatusLabel = indorVerified ? "Completed" : "Not Started" }
            ],
            VerificationItems =
            [
                new() { Label = "Licensed Realtor®", IsComplete = hasLicenseDoc && hasLicenseNumber },
                new() { Label = "Background Verified", IsComplete = hasGovId },
                new() { Label = "INDOR Verified", IsComplete = shell.IsVerified }
            ],
            Stats =
            [
                new() { Label = "Listings Active", Count = listings.Count, Icon = "fa-house", ColorClass = "blue" },
                new() { Label = "Clients Helped All-Time", Count = clientCount, Icon = "fa-users", ColorClass = "teal" },
                new() { Label = "Market Areas Served", Count = serviceAreas.Count, Icon = "fa-map-location-dot", ColorClass = "purple" }
            ],
            ServiceAreaChips = serviceAreas,
            ActiveListings = listings,
            OpenHouses = openHouses,
            SharedPackages = packages.Select(MapPublicPackage).ToList(),
            ShareUrl = $"/Realtor/PublicProfile"
        };
    }

    private static RealtorPublicListingCardViewModel MapPublicListing(IndorNearbyNetworkItem item)
    {
        var priceLabel = item.Price is > 0
            ? FormatCurrency(item.Price.Value)
            : item.Title.StartsWith('$')
                ? item.Title
                : item.Title;

        return new RealtorPublicListingCardViewModel
        {
            ItemId = item.Id,
            Title = item.Title,
            Address = item.Subtitle ?? "",
            PriceLabel = priceLabel,
            StatusBadge = item.StatusBadge ?? item.BadgeLabel,
            ImageUrl = string.IsNullOrWhiteSpace(item.ImageUrl) ? null : item.ImageUrl,
            SpecsLabel = item.SpecsLabel ?? BuildSpecsLabel(item.Bedrooms, item.Bathrooms, item.SquareFeet),
            ViewUrl = $"/Realtor/ViewNetworkListing/{item.Id}"
        };
    }

    private static RealtorPublicOpenHouseViewModel MapPublicOpenHouse(IndorNearbyNetworkItem item) =>
        new()
        {
            ItemId = item.Id,
            Title = item.Title,
            Address = item.Subtitle ?? "",
            MetaLabel = item.MetaLabel,
            ImageUrl = string.IsNullOrWhiteSpace(item.ImageUrl) ? null : item.ImageUrl,
            ViewUrl = $"/Realtor/ViewNetworkListing/{item.Id}"
        };

    private static RealtorPublicSharedPackageViewModel MapPublicPackage(IndorRealtorSharedPackage package)
    {
        var isViewed = package.StatusLabel.Contains("view", StringComparison.OrdinalIgnoreCase);
        return new RealtorPublicSharedPackageViewModel
        {
            Id = package.Id,
            Title = $"{package.Address} Package",
            Subtitle = package.ClientName,
            SharedLabel = $"Shared {package.SharedUtc.ToLocalTime():MMM d, yyyy}",
            StatusLabel = isViewed ? "Viewed" : package.StatusLabel
        };
    }

    private static List<string> ParseServiceAreas(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split([',', ';', '\n', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? BuildSpecsLabel(decimal? beds, decimal? baths, int? sqft)
    {
        var parts = new List<string>();
        if (beds is > 0)
        {
            parts.Add($"{beds:0.#} bd");
        }

        if (baths is > 0)
        {
            parts.Add($"{baths:0.#} ba");
        }

        if (sqft is > 0)
        {
            parts.Add($"{sqft:N0} sqft");
        }

        return parts.Count > 0 ? string.Join(" · ", parts) : null;
    }

    private static string FormatCurrency(decimal amount) =>
        amount.ToString("C0", CultureInfo.GetCultureInfo("en-US"));

    public async Task<RealtorNetworkViewModel> BuildNetworkAsync(
        IndorRealtor realtor,
        string? search,
        string? filter,
        CancellationToken ct = default)
    {
        var shell = await BuildShellAsync(realtor, ct);
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

        var providers = await providersQuery.Take(30).ToListAsync(ct);

        return new RealtorNetworkViewModel
        {
            DisplayName = shell.DisplayName,
            FullDisplayName = shell.FullDisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            Providers = providers.Select(p => new RealtorQuoteProviderCardViewModel
            {
                Id = p.Id,
                CompanyName = p.CompanyName,
                Categories = p.Categories,
                Rating = p.Rating,
                DistanceMiles = p.DistanceMiles,
                BadgeLabel = p.BadgeLabel,
                IsVerified = p.IsVerified
            }).ToList()
        };
    }

    public Task<RealtorPortalShellViewModel> BuildShellForRealtorAsync(IndorRealtor realtor, CancellationToken ct = default) =>
        BuildShellCoreAsync(realtor, ct);

    public Task<RealtorPortalShellViewModel> BuildShellAsync(IndorRealtor realtor, CancellationToken ct = default) =>
        BuildShellCoreAsync(realtor, ct);

    private async Task<RealtorPortalShellViewModel> BuildShellCoreAsync(IndorRealtor realtor, CancellationToken ct)
    {
        var hasNotifications = await db.IndorRealtorActivities.AsNoTracking()
            .AnyAsync(a => a.RealtorId == realtor.Id &&
                           a.OccurredUtc >= DateTime.UtcNow.AddDays(-1), ct);

        var firstName = (realtor.DisplayName ?? "Realtor").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                        ?? "Realtor";

        return new RealtorPortalShellViewModel
        {
            DisplayName = firstName,
            FullDisplayName = realtor.DisplayName ?? firstName,
            ProfilePhotoUrl = string.IsNullOrWhiteSpace(realtor.ProfilePhotoUrl)
                ? null
                : realtor.ProfilePhotoUrl,
            BadgeLabel = realtor.RegistrationStatus == RealtorRegistrationStatuses.Verified
                ? "Verified Realtor"
                : "Realtor Basic",
            IsVerified = realtor.RegistrationStatus == RealtorRegistrationStatuses.Verified,
            HasNotifications = hasNotifications
        };
    }

    private async Task<List<RealtorStatItemViewModel>> BuildHomeStatsAsync(int realtorId, CancellationToken ct)
    {
        var activeFiles = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.Status == "Active", ct);
        var pendingQuotes = await db.IndorRealtorQuotes.AsNoTracking()
            .CountAsync(q => q.RealtorId == realtorId && q.Status == "Pending", ct);
        var sharedPackages = await db.IndorRealtorSharedPackages.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId, ct);
        var connectedClients = await db.IndorRealtorClients.AsNoTracking()
            .CountAsync(c => c.RealtorId == realtorId, ct);

        return
        [
            new() { Label = "Active Files", Count = activeFiles, Icon = "fa-folder-open", ColorClass = "blue", DetailUrl = "/Realtor/Files" },
            new() { Label = "Pending Quotes", Count = pendingQuotes, Icon = "fa-dollar-sign", ColorClass = "teal", DetailUrl = "/Realtor/Quotes" },
            new() { Label = "Shared Packages", Count = sharedPackages, Icon = "fa-box-archive", ColorClass = "purple", DetailUrl = "/Realtor/Dashboard#shared-packages" },
            new() { Label = "Connected Clients", Count = connectedClients, Icon = "fa-user-group", ColorClass = "cyan", DetailUrl = "/Realtor/Clients" }
        ];
    }

    private async Task<List<RealtorStatItemViewModel>> BuildClientStatsAsync(
        int realtorId, string activeFilter, CancellationToken ct)
    {
        var clientsQuery = db.IndorRealtorClients.AsNoTracking().Where(c => c.RealtorId == realtorId);
        var clients = await clientsQuery.CountAsync(ct);
        var buyers = await clientsQuery.CountAsync(c => c.ClientRole == RealtorClientRoles.Buyer, ct);
        var sellers = await clientsQuery.CountAsync(c => c.ClientRole == RealtorClientRoles.Seller, ct);
        var homeowners = await clientsQuery.CountAsync(c => c.ClientRole == RealtorClientRoles.Homeowner, ct);
        var activeFiles = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.Status == "Active", ct);
        var invited = await db.IndorRealtorInvitations.AsNoTracking()
            .CountAsync(i => i.RealtorId == realtorId && i.Status == RealtorInvitationStatuses.Sent, ct);
        var followUpClients = await clientsQuery
            .CountAsync(c => c.StatusSummary != null &&
                             (c.StatusSummary.Contains("pending") || c.StatusSummary.Contains("follow")), ct);
        var connect = await clientsQuery
            .CountAsync(c =>
                c.StatusSummary == null ||
                (!c.StatusSummary.Contains("pending") && !c.StatusSummary.Contains("follow")), ct);
        var followUp = followUpClients + invited;

        return activeFilter switch
        {
            "Buyers" =>
            [
                new() { Label = "Buyers", Count = buyers, Icon = "fa-user-tag", ColorClass = "blue" }
            ],
            "Sellers" =>
            [
                new() { Label = "Sellers", Count = sellers, Icon = "fa-house-chimney", ColorClass = "teal" }
            ],
            "Homeowners" =>
            [
                new() { Label = "Homeowners", Count = homeowners, Icon = "fa-house-user", ColorClass = "purple" }
            ],
            "Invited" =>
            [
                new() { Label = "Invited", Count = invited, Icon = "fa-envelope", ColorClass = "purple" }
            ],
            "Connect" =>
            [
                new() { Label = "Connect", Count = connect, Icon = "fa-circle-check", ColorClass = "teal" }
            ],
            "Follow-up" =>
            [
                new() { Label = "Follow-up", Count = followUp, Icon = "fa-clock", ColorClass = "orange" }
            ],
            _ =>
            [
                new() { Label = "Clients", Count = clients, Icon = "fa-users", ColorClass = "blue" },
                new() { Label = "Active Files", Count = activeFiles, Icon = "fa-folder-open", ColorClass = "teal", DetailUrl = "/Realtor/Files?filter=Active" },
                new() { Label = "Invited", Count = invited, Icon = "fa-envelope", ColorClass = "purple" },
                new() { Label = "Connect", Count = connect, Icon = "fa-circle-check", ColorClass = "teal" },
                new() { Label = "Follow-up", Count = followUp, Icon = "fa-clock", ColorClass = "orange" }
            ]
        };
    }

    private async Task<List<RealtorStatItemViewModel>> BuildFileStatsAsync(int realtorId, CancellationToken ct)
    {
        var active = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.Status == "Active", ct);
        var inspection = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && (p.FilePhase == "Repair Review" || p.RepairItemsCount > 0), ct);
        var quotes = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.QuotesReceivedCount > 0, ct);
        var shared = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.FilePhase == "Transfer", ct);
        var closed = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.Status == "Archived", ct);

        return
        [
            new() { Label = "Active Files", Count = active, Icon = "fa-folder-open", ColorClass = "blue" },
            new() { Label = "Inspection Uploaded", Count = inspection, Icon = "fa-cloud-arrow-up", ColorClass = "teal" },
            new() { Label = "Quotes In Progress", Count = quotes, Icon = "fa-file-invoice-dollar", ColorClass = "orange" },
            new() { Label = "Shared Packages", Count = shared, Icon = "fa-share-nodes", ColorClass = "purple" },
            new() { Label = "Closed Files", Count = closed, Icon = "fa-box-archive", ColorClass = "red" }
        ];
    }

    private async Task<Dictionary<int, int>> LoadBidCountsByQuoteAsync(
        IReadOnlyList<int> quoteIds,
        CancellationToken ct)
    {
        if (quoteIds.Count == 0)
        {
            return [];
        }

        return await db.IndorRealtorQuoteBids.AsNoTracking()
            .Where(b => quoteIds.Contains(b.QuoteId))
            .GroupBy(b => b.QuoteId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
    }

    private static void ApplyBidCountsToQuote(
        IndorRealtorQuote quote,
        IReadOnlyDictionary<int, int> bidCounts)
    {
        if (!bidCounts.TryGetValue(quote.Id, out var count) || count <= 0)
        {
            return;
        }

        quote.ProviderQuotesReceived = Math.Max(quote.ProviderQuotesReceived, count);
        if (quote.Status is "Pending" or "Received" or "Compare")
        {
            quote.Status = count >= 2 ? "Compare" : "Received";
        }

        quote.FooterNote = $"{count} provider quote{(count == 1 ? "" : "s")} received";
    }

    private async Task<List<RealtorStatItemViewModel>> BuildQuoteStatsAsync(int realtorId, CancellationToken ct)
    {
        var all = await db.IndorRealtorQuotes.AsNoTracking().Where(q => q.RealtorId == realtorId).ToListAsync(ct);
        var bidCounts = await LoadBidCountsByQuoteAsync(all.Select(q => q.Id).ToList(), ct);
        foreach (var quote in all)
        {
            ApplyBidCountsToQuote(quote, bidCounts);
        }

        var requested = all.Count(q => q.ProviderQuotesReceived == 0 && q.Status == "Pending");
        var received = all.Count(q => q.ProviderQuotesReceived > 0 && q.Status != "Accepted");
        var compare = all.Count(q => q.Status == "Compare");
        var selected = all.Count(q => q.Status == "Accepted");
        var urgent = all.Count(q => q.Status == "Pending" &&
                                    q.RequestedUtc <= DateTime.UtcNow.AddDays(-4));

        return
        [
            new() { Label = "Requested", Count = requested, Icon = "fa-file-lines", ColorClass = "blue" },
            new() { Label = "Received", Count = received, Icon = "fa-dollar-sign", ColorClass = "teal" },
            new() { Label = "Compare", Count = compare, Icon = "fa-people-arrows", ColorClass = "purple" },
            new() { Label = "Selected", Count = selected, Icon = "fa-circle-check", ColorClass = "blue" },
            new() { Label = "Urgent", Count = urgent, Icon = "fa-triangle-exclamation", ColorClass = "red" }
        ];
    }

    private async Task<List<RealtorInsightViewModel>> BuildHomeInsightsAsync(int realtorId, CancellationToken ct)
    {
        var inspectionReady = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.RepairItemsCount > 0, ct);
        var followUp = await db.IndorRealtorClients.AsNoTracking()
            .CountAsync(c => c.RealtorId == realtorId && c.StatusSummary != null && c.StatusSummary.Contains("pending"), ct);
        var needsSelection = await db.IndorRealtorQuotes.AsNoTracking()
            .CountAsync(q => q.RealtorId == realtorId && q.Status == "Compare", ct);

        var insights = new List<RealtorInsightViewModel>();
        if (inspectionReady > 0)
        {
            insights.Add(new()
            {
                Text = $"{inspectionReady} inspection report{(inspectionReady == 1 ? "" : "s")} ready for review",
                Icon = "fa-file-circle-check",
                ColorClass = "teal",
                TargetUrl = "/Realtor/Files?filter=Inspection"
            });
        }

        if (followUp > 0)
        {
            insights.Add(new()
            {
                Text = $"{followUp} file{(followUp == 1 ? "" : "s")} need client follow-up",
                Icon = "fa-user-clock",
                ColorClass = "blue",
                TargetUrl = "/Realtor/Clients"
            });
        }

        if (needsSelection > 0)
        {
            insights.Add(new()
            {
                Text = $"{needsSelection} quote request{(needsSelection == 1 ? "" : "s")} need provider selection",
                Icon = "fa-hand-pointer",
                ColorClass = "orange",
                TargetUrl = "/Realtor/Quotes?filter=Compare"
            });
        }

        if (insights.Count == 0)
        {
            insights.Add(new()
            {
                Text = "You're all caught up — no urgent tasks right now",
                Icon = "fa-circle-check",
                ColorClass = "teal"
            });
        }

        return insights;
    }

    private async Task<List<RealtorInsightViewModel>> BuildFileInsightsAsync(int realtorId, CancellationToken ct)
    {
        var readyParse = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.FilePhase == "Repair Review", ct);
        var needQuotes = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .CountAsync(p => p.RealtorId == realtorId && p.RepairItemsCount > 0 && p.QuotesReceivedCount == 0, ct);

        var insights = new List<RealtorInsightViewModel>();
        if (readyParse > 0)
        {
            insights.Add(new()
            {
                Text = $"{readyParse} report{(readyParse == 1 ? "" : "s")} ready for parsing",
                Icon = "fa-wand-magic-sparkles",
                ColorClass = "teal"
            });
        }

        if (needQuotes > 0)
        {
            insights.Add(new()
            {
                Text = $"{needQuotes} file{(needQuotes == 1 ? "" : "s")} need contractor selection",
                Icon = "fa-hard-hat",
                ColorClass = "orange"
            });
        }

        return insights;
    }

    private static List<RealtorNextStepViewModel> BuildClientNextSteps(
        int pendingInvites, List<IndorRealtorClient> clients, Dictionary<string, int> quoteCounts, string activeFilter)
    {
        var steps = new List<RealtorNextStepViewModel>();
        if (pendingInvites > 0 && activeFilter is "All" or "Invited")
        {
            steps.Add(new()
            {
                Text = $"{pendingInvites} client{(pendingInvites == 1 ? "" : "s")} need invitations",
                Icon = "fa-user-plus",
                ColorClass = "blue",
                Url = "/RealtorInviteClient/New"
            });
        }

        var pendingQuotes = clients.Count(c =>
            c.StatusSummary != null && c.StatusSummary.Contains("quote", StringComparison.OrdinalIgnoreCase));
        if (pendingQuotes > 0)
        {
            steps.Add(new()
            {
                Text = $"{pendingQuotes} client{(pendingQuotes == 1 ? "" : "s")} have pending quotes",
                Icon = "fa-file-invoice-dollar",
                ColorClass = "purple",
                Url = "/Realtor/Quotes"
            });
        }

        return steps;
    }

    private static List<RealtorNextStepViewModel> BuildQuoteAlerts(List<RealtorStatItemViewModel> stats)
    {
        var alerts = new List<RealtorNextStepViewModel>();
        var compare = stats.FirstOrDefault(s => s.Label == "Compare");
        var urgent = stats.FirstOrDefault(s => s.Label == "Urgent");
        var selected = stats.FirstOrDefault(s => s.Label == "Selected");

        if (compare?.Count > 0)
        {
            alerts.Add(new()
            {
                Text = $"{compare.Count} quote{(compare.Count == 1 ? "" : "s")} need review today",
                Icon = "fa-clock",
                ColorClass = "orange"
            });
        }

        if (urgent?.Count > 0)
        {
            alerts.Add(new()
            {
                Text = $"{urgent.Count} urgent request{(urgent.Count == 1 ? "" : "s")} need provider",
                Icon = "fa-triangle-exclamation",
                ColorClass = "red"
            });
        }

        if (selected?.Count > 0)
        {
            alerts.Add(new()
            {
                Text = $"{selected.Count} selected quote{(selected.Count == 1 ? "" : "s")} ready to share",
                Icon = "fa-circle-check",
                ColorClass = "teal"
            });
        }

        return alerts;
    }

    private static string NormalizeClientFilter(string? filter) =>
        filter switch
        {
            "Buyers" or "Sellers" or "Homeowners" or "Invited" or "Connect" or "Follow-up" => filter,
            _ => "All"
        };

    private static string NormalizeFileFilter(string? filter) =>
        filter switch
        {
            "All" or "Active" or "Inspection" or "Quotes" or "Shared" or "Closed" => filter,
            _ => "All"
        };

    private static string NormalizeQuoteFilter(string? filter) =>
        filter switch
        {
            "All" or "Requested" or "Received" or "Compare" or "Selected" or "Urgent" => filter,
            _ => "All"
        };

    private static string MapQuoteFilterToStatus(string filter) =>
        filter switch
        {
            "Requested" => "Pending",
            "Received" => "Received",
            "Compare" => "Compare",
            "Selected" => "Accepted",
            _ => filter
        };

    private static RealtorPropertyFileCardViewModel MapPropertyCard(IndorRealtorPropertyFile file)
    {
        var (badge, css) = DeriveFileStatus(file);
        var (actionLabel, _) = DeriveFileAction(file);

        return new RealtorPropertyFileCardViewModel
        {
            Id = file.Id,
            Title = file.Title,
            StreetAddress = file.Address,
            CityRegion = file.CityRegion ?? "",
            Address = string.IsNullOrWhiteSpace(file.CityRegion)
                ? file.Address
                : $"{file.Address}, {file.CityRegion}",
            ClientName = file.ClientName ?? "",
            SpecsLabel = BuildSpecsLabel(file),
            PhotoUrl = string.IsNullOrWhiteSpace(file.PhotoUrl) ? "/welcome-house.png" : file.PhotoUrl,
            StatusLabel = badge,
            StatusCss = css,
            ActionLabel = actionLabel,
            ActionUrl = DeriveFileActionUrl(file, actionLabel)
        };
    }

    private static RealtorQuoteCardViewModel MapQuoteCard(IndorRealtorQuote quote)
    {
        var (label, css) = DeriveQuoteStatus(quote);

        return new RealtorQuoteCardViewModel
        {
            Id = quote.Id,
            QuoteCode = FormatQuoteCode(quote.QuoteCode),
            StreetAddress = quote.Address,
            CityRegion = "",
            Address = quote.Address,
            ClientName = quote.ClientName ?? "",
            ServiceType = quote.ServiceType,
            RequestedLabel = $"Requested: {quote.RequestedUtc.ToLocalTime():MMM d, yyyy}",
            DueLabel = quote.ResponseDeadlineHours is > 0
                ? $"Due: {quote.RequestedUtc.AddHours(quote.ResponseDeadlineHours.Value).ToLocalTime():MMM d, yyyy}"
                : "",
            StatusLabel = label,
            StatusCss = css,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            ProviderQuotesReceived = quote.ProviderQuotesReceived,
            ActionLabel = quote.Status == "Compare" ? "Compare Quotes" : "View Request",
            ProviderInitials = DeriveProviderInitials(quote.ProviderQuotesReceived)
        };
    }

    private static RealtorSharedPackageCardViewModel MapPackageCard(IndorRealtorSharedPackage package)
    {
        var isViewed = package.StatusLabel.Contains("view", StringComparison.OrdinalIgnoreCase);

        return new RealtorSharedPackageCardViewModel
        {
            Id = package.Id,
            ClientName = package.ClientName,
            Address = package.Address,
            PackageTitle = $"{package.Address} - Inspection Package",
            SharedLabel = $"Shared {package.SharedUtc.ToLocalTime():MMM d, yyyy}",
            StatusLabel = isViewed ? "Viewed" : "Pending",
            StatusCss = isViewed ? "viewed" : "pending",
            IconColor = isViewed ? "teal" : "purple",
            ActionLabel = "Open Package"
        };
    }

    private static RealtorClientCardViewModel MapClient(
        IndorRealtorClient client,
        Dictionary<string, int> fileCounts,
        Dictionary<string, int> quoteCounts)
    {
        var parts = client.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}"
            : client.FullName.Length > 0 ? client.FullName[..1] : "?";

        fileCounts.TryGetValue(client.FullName, out var files);
        quoteCounts.TryGetValue(client.FullName, out var quotes);

        var (badge, css, action) = DeriveClientStatus(client, files);

        return new RealtorClientCardViewModel
        {
            Id = client.Id,
            FullName = client.FullName,
            Initials = initials.ToUpperInvariant(),
            ClientRole = client.ClientRole,
            ProfileImageUrl = client.ProfileImageUrl,
            PropertyAddress = client.PropertyAddress,
            StatusSummary = client.StatusSummary ?? "",
            StatusBadge = badge,
            StatusCss = css,
            LastActiveLabel = FormatRelativeTime(client.LastActiveUtc),
            FilesCount = files,
            QuotesCount = quotes,
            ActionLabel = action,
            ActionUrl = files > 0 ? "/Realtor/Files" : "#"
        };
    }

    private static RealtorInvitationCardViewModel MapInvitation(IndorRealtorInvitation inv)
    {
        var parts = inv.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2
            ? $"{parts[0][0]}{parts[^1][0]}"
            : inv.FullName.Length > 0 ? inv.FullName[..1] : "?";

        return new RealtorInvitationCardViewModel
        {
            Id = inv.Id,
            FullName = inv.FullName,
            Email = inv.Email,
            Initials = initials.ToUpperInvariant(),
            SentLabel = inv.SentUtc.ToLocalTime().ToString("MMM d, yyyy")
        };
    }

    private static RealtorFileCardViewModel MapFile(IndorRealtorPropertyFile file)
    {
        var (badge, css) = DeriveFileStatus(file);
        var (actionLabel, detailNote) = DeriveFileAction(file);

        return new RealtorFileCardViewModel
        {
            Id = file.Id,
            FileCode = $"PF-{file.Id}",
            Address = file.Address,
            CityRegion = file.CityRegion ?? "",
            PhotoUrl = string.IsNullOrWhiteSpace(file.PhotoUrl) ? "/welcome-house.png" : file.PhotoUrl,
            FilePhase = file.FilePhase ?? file.Status,
            StatusBadge = badge,
            StatusCss = css,
            ClientName = file.ClientName ?? "",
            RepairItemsCount = file.RepairItemsCount,
            QuotesReceivedCount = file.QuotesReceivedCount,
            UpdatedLabel = $"Last updated {FormatRelativeTime(file.UpdatedUtc ?? file.FechaCreacion)}",
            ActionLabel = actionLabel,
            ActionUrl = DeriveFileActionUrl(file, actionLabel),
            DetailNote = detailNote
        };
    }

    private RealtorOpenQuoteCardViewModel MapOpenQuote(IndorRealtorQuote quote)
    {
        var (label, css) = DeriveQuoteStatus(quote);
        var isUrgent = quote.Status == "Pending" && quote.RequestedUtc <= DateTime.UtcNow.AddDays(-4);
        var providerSummary = quote.ProviderQuotesReceived switch
        {
            0 => "0 quotes yet",
            1 => "1 provider quote received",
            _ => $"{quote.ProviderQuotesReceived} provider quotes received"
        };

        string actionLabel;
        string? secondaryLabel = null;
        string? secondaryUrl = null;

        if (quote.Status == "Accepted")
        {
            actionLabel = "View Selected Quote";
        }
        else if (quote.ProviderQuotesReceived >= 2 || quote.Status == "Compare")
        {
            actionLabel = "Compare Quotes";
        }
        else if (quote.ProviderQuotesReceived == 1)
        {
            actionLabel = "View Quote";
            secondaryLabel = "Request Another Quote";
            secondaryUrl = BuildInviteProvidersUrl(quote);
        }
        else if (isUrgent)
        {
            actionLabel = "Invite Providers";
        }
        else
        {
            actionLabel = "View Request";
        }

        string priceLabel = "";
        if (quote.Status == "Accepted" && quote.Amount.HasValue && quote.Amount > 0)
        {
            priceLabel = $"Total {quote.Amount.Value:C0}";
        }
        else if (quote.ProviderQuotesReceived > 1 && quote.Amount.HasValue && quote.Amount > 0)
        {
            priceLabel = $"Starting at {quote.Amount.Value:C0}";
        }
        else if (quote.ProviderQuotesReceived == 1 && quote.Amount.HasValue && quote.Amount > 0)
        {
            priceLabel = quote.Amount.Value.ToString("C0");
        }

        return new RealtorOpenQuoteCardViewModel
        {
            Id = quote.Id,
            QuoteCode = FormatQuoteCode(quote.QuoteCode),
            StreetAddress = quote.Address,
            CityRegion = "",
            Address = quote.Address,
            ClientName = quote.ClientName ?? "",
            ServiceType = quote.ServiceType,
            StatusLabel = quote.Status == "Accepted"
                ? "Quote Selected"
                : isUrgent && quote.ProviderQuotesReceived == 0 ? "Urgent" : label,
            StatusCss = quote.Status == "Accepted" ? "selected" : isUrgent && quote.ProviderQuotesReceived == 0 ? "urgent" : css,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            FooterNote = quote.FooterNote ?? DefaultQuoteFooter(quote),
            UpdatedLabel = FormatRelativeTime(quote.UpdatedUtc ?? quote.RequestedUtc),
            RequestedLabel = $"Requested {quote.RequestedUtc.ToLocalTime():MMM d, yyyy}",
            ProviderQuotesReceived = quote.ProviderQuotesReceived,
            ProviderSummary = providerSummary,
            ProviderInitials = "",
            ActionLabel = actionLabel,
            SecondaryActionLabel = secondaryLabel,
            SecondaryActionUrl = secondaryUrl,
            IsUrgent = isUrgent,
            PriceRangeLabel = priceLabel,
            ActionUrl = ResolveQuoteFlowUrl(quote) ?? $"/Realtor/QuoteDetail/{quote.Id}"
        };
    }

    private static string BuildSpecsLabel(IndorRealtorPropertyFile file)
    {
        var specs = new List<string>();
        if (file.Beds.HasValue) specs.Add($"{file.Beds} bed");
        if (file.Baths.HasValue) specs.Add($"{file.Baths:0.#} bath");
        if (file.SqFt.HasValue) specs.Add($"{file.SqFt:N0} sq ft");
        return string.Join(" · ", specs);
    }

    private static (string Badge, string Css) DeriveFileStatus(IndorRealtorPropertyFile file)
    {
        if (file.FilePhase == "Transfer")
            return ("Shared Package", "shared");
        if (file.QuotesReceivedCount > 0)
            return ("Quotes Pending", "quotes");
        if (file.RepairItemsCount > 0 || file.FilePhase == "Repair Review")
            return ("Inspection Uploaded", "inspection");
        if (file.Status == "Archived")
            return ("Closed", "closed");
        return ("Active", "active");
    }

    private static (string Action, string Detail) DeriveFileAction(IndorRealtorPropertyFile file)
    {
        if (file.FilePhase == "Transfer")
            return ("View Package", "Shared with client");
        if (file.RepairItemsCount > 0 && file.QuotesReceivedCount == 0)
            return ("Request Quotes", $"Repair items: {file.RepairItemsCount}");
        if (file.RepairItemsCount == 0 && file.FilePhase == "Pre-Closing")
            return ("Upload Report", "Awaiting inspection report");
        if (file.QuotesReceivedCount > 0)
            return ("Open File", $"Quotes received: {file.QuotesReceivedCount}");
        return ("Open File", file.RepairItemsCount > 0 ? $"Repair items: {file.RepairItemsCount}" : "");
    }

    private static string DeriveFileActionUrl(IndorRealtorPropertyFile file, string actionLabel)
    {
        var id = file.Id;
        return actionLabel switch
        {
            "Request Quotes" => $"/RealtorQuoteRequest/Start?propertyFileId={id}",
            "Upload Report" => $"/RealtorInspectionUpload/Upload?propertyFileId={id}",
            "View Package" => $"/RealtorPropertyFile/View?id={id}",
            _ => $"/RealtorPropertyFile/View?id={id}"
        };
    }

    private static (string Label, string Css) DeriveQuoteStatus(IndorRealtorQuote quote) =>
        quote.Status switch
        {
            "Compare" => ($"{quote.ProviderQuotesReceived} Quotes Received", "compare"),
            "Accepted" => ("Quote Selected", "selected"),
            "Received" => ($"{quote.ProviderQuotesReceived} Quote{(quote.ProviderQuotesReceived == 1 ? "" : "s")} Received", "received"),
            _ when quote.ProviderQuotesReceived >= 2 => ($"{quote.ProviderQuotesReceived} Quotes Received", "received"),
            _ when quote.ProviderQuotesReceived == 1 => ("1 Quote Received", "received"),
            _ => ("Waiting for Providers", "pending")
        };

    private async Task<IndorRealtorQuote?> LoadOwnedQuoteAsync(int realtorId, int quoteId, CancellationToken ct) =>
        await db.IndorRealtorQuotes.AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.RealtorId == realtorId, ct);

    private async Task<List<RealtorQuoteRequestedServiceViewModel>> LoadRequestedServicesAsync(
        int? propertyFileId, string serviceType, CancellationToken ct)
    {
        if (propertyFileId is not > 0)
        {
            return [];
        }

        var items = await db.IndorRealtorPropertyFileItems.AsNoTracking()
            .Where(i => i.PropertyFileId == propertyFileId
                        && i.CategoryType == RealtorPropertyFileCategoryTypes.RepairItems)
            .OrderBy(i => i.UploadedUtc)
            .Take(8)
            .ToListAsync(ct);

        return items.Select((item, index) => new RealtorQuoteRequestedServiceViewModel
        {
            SortOrder = index + 1,
            Title = item.ItemLabel,
            Icon = DeriveServiceIcon(serviceType, item.ItemLabel)
        }).ToList();
    }

    private async Task<(string ScopeOfWork, string Timeline, string Warranty, List<string> IncludedRepairs, List<RealtorQuotePriceLineViewModel> PriceLines)>
        LoadBidEstimateDetailsAsync(IndorRealtorQuoteBid bid, CancellationToken ct)
    {
        if (bid.EstimateId is > 0)
        {
            var estimate = await db.IndorProveedorEstimates.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == bid.EstimateId, ct);

            if (estimate != null)
            {
                var repairs = ParseScopeLabels(estimate.ScopeItemsJson);
                var serviceCall = Math.Max(0m, estimate.Amount - estimate.LaborAmount - estimate.MaterialsAmount);
                var priceLines = new List<RealtorQuotePriceLineViewModel>
                {
                    new() { Label = "Labor", AmountLabel = estimate.LaborAmount.ToString("C2") },
                    new() { Label = "Materials", AmountLabel = estimate.MaterialsAmount.ToString("C2") }
                };

                if (serviceCall > 0)
                {
                    priceLines.Add(new() { Label = "Service Call", AmountLabel = serviceCall.ToString("C2") });
                }

                var scope = !string.IsNullOrWhiteSpace(estimate.HomeownerNotes)
                    ? estimate.HomeownerNotes
                    : repairs.Count > 0
                        ? string.Join(" ", repairs)
                        : $"Professional {estimate.ServiceType ?? "repair"} services for the requested property.";

                return (
                    scope,
                    estimate.Timeline ?? estimate.EstimatedDuration ?? "1–2 days",
                    estimate.LaborWarranty ?? estimate.Warranty ?? "90 days labor",
                    repairs,
                    priceLines);
            }
        }

        return (
            $"Complete {bid.ProviderName} scope for the requested repairs at this property.",
            "1–2 days",
            "90 days labor",
            [],
            [
                new() { Label = "Total Quote", AmountLabel = bid.Amount.ToString("C2") }
            ]);
    }

    private static List<string> ParseScopeLabels(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var labels = new List<string>();
            foreach (var node in doc.RootElement.EnumerateArray())
            {
                var label = node.TryGetProperty("label", out var l) ? l.GetString()
                    : node.TryGetProperty("Label", out var l2) ? l2.GetString()
                    : null;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    labels.Add(label);
                }
            }

            return labels;
        }
        catch
        {
            return [];
        }
    }

    private static string BuildInviteProvidersUrl(IndorRealtorQuote quote) =>
        quote.PropertyFileId is > 0
            ? $"/RealtorQuoteRequest/Start?propertyFileId={quote.PropertyFileId}"
            : "/RealtorQuoteRequest/Property";

    private static string DeriveInitialsFromName(string name)
    {
        var parts = name.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
        }

        return name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
    }

    private static int DeriveReviewCount(decimal rating) =>
        (int)Math.Round(rating * 26.5m, MidpointRounding.AwayFromZero);

    private static string DeriveServiceIcon(string serviceType, string itemLabel)
    {
        var text = $"{serviceType} {itemLabel}".ToLowerInvariant();
        if (text.Contains("smoke") || text.Contains("alarm"))
        {
            return "fa-bell";
        }

        if (text.Contains("wiring") || text.Contains("electrical") || text.Contains("gfci") || text.Contains("receptacle"))
        {
            return "fa-plug";
        }

        if (text.Contains("plumb") || text.Contains("disposal") || text.Contains("faucet"))
        {
            return "fa-faucet-drip";
        }

        if (text.Contains("roof"))
        {
            return "fa-house-chimney";
        }

        return "fa-wrench";
    }

    private static string BuildTimelineRange(IReadOnlyList<string> timelines)
    {
        if (timelines.Count == 0)
        {
            return "—";
        }

        var days = timelines
            .SelectMany(t => System.Text.RegularExpressions.Regex.Matches(t, @"\d+").Select(m => int.Parse(m.Value)))
            .ToList();

        if (days.Count >= 2)
        {
            return $"{days.Min()} – {days.Max()} days";
        }

        return timelines[0];
    }

    private static (string Badge, string Css, string Action) DeriveClientStatus(IndorRealtorClient client, int files)
    {
        if (client.StatusSummary != null && client.StatusSummary.Contains("quote", StringComparison.OrdinalIgnoreCase))
            return ("Active File", "active-file", "Open File");
        if (client.StatusSummary != null && client.StatusSummary.Contains("progress", StringComparison.OrdinalIgnoreCase))
            return ("Connected", "connected", "Open Client");
        if (files == 0)
            return ("Needs File", "needs-file", "Open Client");
        return ("Connected", "connected", "Open Client");
    }

    private static List<string> DeriveProviderInitials(int count)
    {
        var names = new[] { "SH", "PM", "CA", "HV", "EC" };
        return names.Take(Math.Min(count, 4)).ToList();
    }

    private static RealtorActivityItemViewModel MapActivity(IndorRealtorActivity activity) =>
        new()
        {
            Id = activity.Id,
            ActivityType = activity.ActivityType,
            Description = activity.Description,
            OccurredLabel = FormatRelativeTime(activity.OccurredUtc),
            CategoryTag = activity.CategoryTag
        };

    private static string FormatQuoteCode(string code) =>
        code.StartsWith('#') || code.StartsWith("Quote", StringComparison.OrdinalIgnoreCase)
            ? code
            : $"Quote #{code}";

    private static string DefaultQuoteFooter(IndorRealtorQuote quote) =>
        quote.ProviderQuotesReceived > 0
            ? $"{quote.ProviderQuotesReceived} quotes received"
            : "Waiting on providers";

    private static string FormatRelativeTime(DateTime utc)
    {
        var local = utc.ToLocalTime();
        var today = DateTime.Today;
        if (local.Date == today)
        {
            return $"Today, {local:h:mm tt}";
        }

        if (local.Date == today.AddDays(-1))
        {
            return $"Yesterday, {local:h:mm tt}";
        }

        return local.ToString("MMM d, yyyy");
    }

    private static RealtorBusinessInfoRowViewModel BuildBusinessInfoRow(
        string key,
        string label,
        string value,
        string icon,
        string? editUrl,
        string emptyLabel) =>
        new()
        {
            Key = key,
            Label = label,
            Value = string.IsNullOrWhiteSpace(value) ? emptyLabel : value,
            Icon = icon,
            EditUrl = editUrl,
            IsEmpty = string.IsNullOrWhiteSpace(value)
        };

    private static (string Badge, string Css) ResolveLicenseStatus(
        IndorRealtor realtor,
        bool hasLicensePhoto,
        bool hasGovId,
        bool hasLicense)
    {
        if (!hasLicense)
        {
            return ("Not provided", "is-incomplete");
        }

        if (realtor.RegistrationStatus == RealtorRegistrationStatuses.Verified)
        {
            return ("Active", "is-active");
        }

        if (realtor.RegistrationStatus == RealtorRegistrationStatuses.Basic)
        {
            return ("Active", "is-active");
        }

        if (hasLicensePhoto && hasGovId)
        {
            return ("Pending Review", "is-pending");
        }

        return ("Incomplete", "is-incomplete");
    }

    private static string FormatLanguagesLabel(string? languagesJson) =>
        string.Join(", ", DeserializeStringList(languagesJson));

    private static List<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(json);
            return items?.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string SerializeStringList(IReadOnlyList<string> items)
    {
        var normalized = items
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return JsonSerializer.Serialize(normalized);
    }

    private static List<string> ParseLanguagesCsv(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? []
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(l => RealtorEditProfileOptions.SupportedLanguages.Contains(l, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

    private static string BuildOfficeLocationLabel(IndorRealtor entity)
    {
        var parts = new[]
        {
            entity.OfficeCity?.Trim(),
            entity.OfficeState?.Trim(),
            entity.OfficeZip?.Trim()
        }.Where(p => !string.IsNullOrWhiteSpace(p));

        var cityStateZip = string.Join(", ", parts);
        if (!string.IsNullOrWhiteSpace(entity.OfficeAddress))
        {
            return string.IsNullOrWhiteSpace(cityStateZip)
                ? entity.OfficeAddress.Trim()
                : $"{entity.OfficeAddress.Trim()}, {cityStateZip}";
        }

        return cityStateZip;
    }
}
