using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorPortalService(AppDbContext db)
{
    public async Task<RealtorHomeViewModel> BuildHomeAsync(IndorRealtor realtor, CancellationToken ct = default)
    {
        var shell = await BuildShellAsync(realtor, ct);

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

        return new RealtorHomeViewModel
        {
            DisplayName = shell.DisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            QuickActions =
            [
                new() { Label = "Invite client", Icon = "fa-user-plus", Url = "/RealtorInviteClient/ClientInfo" },
                new() { Label = "New property file", Icon = "fa-folder-plus", Url = "/Realtor/Files" },
                new() { Label = "Upload inspection report", Icon = "fa-cloud-arrow-up", Url = "/RealtorInspectionUpload/Upload" },
                new() { Label = "Urgente quote", Icon = "fa-comment-dollar", Url = "/RealtorUrgentQuote/Property" }
            ],
            PropertyFiles = properties.Select(MapPropertyCard).ToList(),
            PendingQuotes = quotes.Select(MapQuoteCard).ToList(),
            SharedPackages = packages.Select(MapPackageCard).ToList()
        };
    }

    public async Task<RealtorClientsViewModel> BuildClientsAsync(
        IndorRealtor realtor, string? search, string? filter, CancellationToken ct = default)
    {
        var shell = await BuildShellAsync(realtor, ct);
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
            "Pending" => clientsQuery.Where(c => false),
            _ => clientsQuery
        };

        var clients = await clientsQuery
            .OrderByDescending(c => c.LastActiveUtc)
            .Take(20)
            .ToListAsync(ct);

        var invitationsQuery = db.IndorRealtorInvitations.AsNoTracking()
            .Where(i => i.RealtorId == realtor.Id && i.Status == RealtorInvitationStatuses.Sent);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            invitationsQuery = invitationsQuery.Where(i =>
                i.FullName.Contains(term) || i.Email.Contains(term));
        }

        var invitations = activeFilter is "All" or "Pending"
            ? await invitationsQuery.OrderByDescending(i => i.SentUtc).Take(10).ToListAsync(ct)
            : [];

        var activities = await db.IndorRealtorActivities.AsNoTracking()
            .Where(a => a.RealtorId == realtor.Id)
            .OrderByDescending(a => a.OccurredUtc)
            .Take(5)
            .ToListAsync(ct);

        return new RealtorClientsViewModel
        {
            DisplayName = shell.DisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            ActiveClients = clients.Select(MapClient).ToList(),
            PendingInvitations = invitations.Select(MapInvitation).ToList(),
            RecentActivity = activities.Select(MapActivity).ToList()
        };
    }

    public async Task<RealtorFilesViewModel> BuildFilesAsync(
        IndorRealtor realtor, string? search, string? filter, CancellationToken ct = default)
    {
        var shell = await BuildShellAsync(realtor, ct);
        var activeFilter = NormalizeFileFilter(filter);

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
            "Archived" => query.Where(p => p.Status == "Archived"),
            _ when activeFilter != "All" => query.Where(p => p.FilePhase == activeFilter),
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
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            ActiveFiles = files.Select(MapFile).ToList(),
            RecentActivity = activities.Select(MapActivity).ToList()
        };
    }

    public async Task<RealtorQuotesViewModel> BuildQuotesAsync(
        IndorRealtor realtor, string? search, string? filter, CancellationToken ct = default)
    {
        var shell = await BuildShellAsync(realtor, ct);
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

        if (activeFilter != "All")
        {
            query = query.Where(q => q.Status == activeFilter);
        }

        var quotes = await query
            .OrderByDescending(q => q.UpdatedUtc ?? q.RequestedUtc)
            .Take(20)
            .ToListAsync(ct);

        var compareQuote = await db.IndorRealtorQuotes.AsNoTracking()
            .Where(q => q.RealtorId == realtor.Id && q.Status == "Compare")
            .OrderByDescending(q => q.UpdatedUtc ?? q.RequestedUtc)
            .FirstOrDefaultAsync(ct);

        RealtorCompareQuotesViewModel? compare = null;
        if (compareQuote != null)
        {
            var bids = await db.IndorRealtorQuoteBids.AsNoTracking()
                .Where(b => b.QuoteId == compareQuote.Id)
                .OrderBy(b => b.SortOrder)
                .Take(3)
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

        return new RealtorQuotesViewModel
        {
            DisplayName = shell.DisplayName,
            ProfilePhotoUrl = shell.ProfilePhotoUrl,
            BadgeLabel = shell.BadgeLabel,
            IsVerified = shell.IsVerified,
            HasNotifications = shell.HasNotifications,
            SearchQuery = search,
            ActiveFilter = activeFilter,
            OpenQuotes = quotes.Select(MapOpenQuote).ToList(),
            CompareQuotes = compare,
            RecentActivity = activities.Select(MapActivity).ToList()
        };
    }

    public async Task<RealtorProfileViewModel> BuildProfileAsync(IndorRealtor realtor, CancellationToken ct = default)
    {
        var shell = await BuildShellAsync(realtor, ct);

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

        return new RealtorProfileViewModel
        {
            DisplayName = shell.DisplayName,
            FullName = realtor.DisplayName ?? shell.DisplayName,
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
            Documents = docSlots
        };
    }

    private async Task<RealtorPortalShellViewModel> BuildShellAsync(IndorRealtor realtor, CancellationToken ct)
    {
        var hasNotifications = await db.IndorRealtorActivities.AsNoTracking()
            .AnyAsync(a => a.RealtorId == realtor.Id &&
                           a.OccurredUtc >= DateTime.UtcNow.AddDays(-1), ct);

        var firstName = (realtor.DisplayName ?? "Realtor").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                        ?? "Realtor";

        return new RealtorPortalShellViewModel
        {
            DisplayName = firstName,
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

    private static string NormalizeClientFilter(string? filter) =>
        filter switch
        {
            "Buyers" or "Sellers" or "Homeowners" or "Pending" => filter,
            _ => "All"
        };

    private static string NormalizeFileFilter(string? filter) =>
        filter switch
        {
            "All" or "Active" or "Pre-Closing" or "Repair Review" or "Transfer" or "Archived" => filter,
            _ => "Active"
        };

    private static string NormalizeQuoteFilter(string? filter) =>
        filter switch
        {
            "All" or "Pending" or "Received" or "Compare" or "Accepted" or "Expired" => filter,
            _ => "Pending"
        };

    private static RealtorPropertyFileCardViewModel MapPropertyCard(IndorRealtorPropertyFile file)
    {
        var specs = new List<string>();
        if (file.Beds.HasValue) specs.Add($"{file.Beds} bed");
        if (file.Baths.HasValue) specs.Add($"{file.Baths:0.#} bath");
        if (file.SqFt.HasValue) specs.Add($"{file.SqFt:N0} sq ft");

        return new RealtorPropertyFileCardViewModel
        {
            Id = file.Id,
            Title = file.Title,
            Address = string.IsNullOrWhiteSpace(file.CityRegion)
                ? file.Address
                : $"{file.Address}, {file.CityRegion}",
            SpecsLabel = string.Join(" · ", specs),
            PhotoUrl = string.IsNullOrWhiteSpace(file.PhotoUrl) ? "/welcome-house.png" : file.PhotoUrl,
            StatusLabel = file.Status
        };
    }

    private static RealtorQuoteCardViewModel MapQuoteCard(IndorRealtorQuote quote) =>
        new()
        {
            Id = quote.Id,
            QuoteCode = FormatQuoteCode(quote.QuoteCode),
            Address = quote.Address,
            ServiceType = quote.ServiceType,
            RequestedLabel = quote.RequestedUtc.ToLocalTime().ToString("MMM d, yyyy"),
            StatusLabel = quote.Status
        };

    private static RealtorSharedPackageCardViewModel MapPackageCard(IndorRealtorSharedPackage package) =>
        new()
        {
            Id = package.Id,
            ClientName = package.ClientName,
            Address = package.Address,
            SharedLabel = package.SharedUtc.ToLocalTime().ToString("MMM d, yyyy"),
            StatusLabel = package.StatusLabel
        };

    private static RealtorClientCardViewModel MapClient(IndorRealtorClient client) =>
        new()
        {
            Id = client.Id,
            FullName = client.FullName,
            ClientRole = client.ClientRole,
            ProfileImageUrl = client.ProfileImageUrl,
            PropertyAddress = client.PropertyAddress,
            StatusSummary = client.StatusSummary ?? "",
            LastActiveLabel = FormatRelativeTime(client.LastActiveUtc)
        };

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

    private static RealtorFileCardViewModel MapFile(IndorRealtorPropertyFile file) =>
        new()
        {
            Id = file.Id,
            Address = file.Address,
            CityRegion = file.CityRegion ?? "",
            PhotoUrl = string.IsNullOrWhiteSpace(file.PhotoUrl) ? "/welcome-house.png" : file.PhotoUrl,
            FilePhase = file.FilePhase ?? file.Status,
            ClientName = file.ClientName ?? "",
            RepairItemsCount = file.RepairItemsCount,
            QuotesReceivedCount = file.QuotesReceivedCount,
            UpdatedLabel = FormatRelativeTime(file.UpdatedUtc ?? file.FechaCreacion)
        };

    private static RealtorOpenQuoteCardViewModel MapOpenQuote(IndorRealtorQuote quote) =>
        new()
        {
            Id = quote.Id,
            QuoteCode = FormatQuoteCode(quote.QuoteCode),
            Address = quote.Address,
            ClientName = quote.ClientName ?? "",
            ServiceType = quote.ServiceType,
            StatusLabel = quote.Status,
            PhotoUrl = string.IsNullOrWhiteSpace(quote.PhotoUrl) ? "/welcome-house.png" : quote.PhotoUrl,
            FooterNote = quote.FooterNote ?? DefaultQuoteFooter(quote),
            UpdatedLabel = FormatRelativeTime(quote.UpdatedUtc ?? quote.RequestedUtc)
        };

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
}
