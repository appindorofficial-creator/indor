using System.Globalization;
using System.Text.Json;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class NeighborRequestWizardService(
    AppDbContext db,
    IAddressLookupService addressLookup,
    IWebHostEnvironment webHostEnvironment)
{
    public const string DraftSessionKey = "NeighborRequestDraft";
    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const int MaxPhotoBytes = 10_000_000;
    private const int MaxPhotos = 5;

    public NeighborRequestDraftState? LoadDraft(ISession session)
    {
        var json = session.GetString(DraftSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<NeighborRequestDraftState>(json);
        }
        catch
        {
            return null;
        }
    }

    public void SaveDraft(ISession session, NeighborRequestDraftState draft) =>
        session.SetString(DraftSessionKey, JsonSerializer.Serialize(draft));

    public void ClearDraft(ISession session) => session.Remove(DraftSessionKey);

    public async Task<Propiedad?> ValidatePropiedadAsync(string userId, int propiedadId, CancellationToken ct) =>
        await db.Propiedades
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propiedadId && p.UserId == userId && p.Activo, ct);

    public async Task<List<IndorNeighborRequestCategory>> LoadCategoriesAsync(CancellationToken ct)
    {
        try
        {
            return await db.IndorNeighborRequestCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    public async Task<NeighborRequestCategoryStepViewModel> BuildCategoryStepAsync(
        int propiedadId,
        IUrlHelper url,
        CancellationToken ct)
    {
        var categories = await LoadCategoriesAsync(ct);
        return new NeighborRequestCategoryStepViewModel
        {
            PropiedadId = propiedadId,
            DisplayStep = 1,
            BackUrl = url.Action("Index", "Home"),
            CloseUrl = url.Action("Index", "Home")!,
            Categories = categories.Select(c => new NeighborRequestCategoryOptionViewModel
            {
                Id = c.Id,
                Label = ResolveCategoryLabel(c.Code, c.LabelEn),
                Description = ResolveCategoryDescription(c.Code, c.DescriptionEn),
                IconClass = ResolveCategoryIcon(c.Code, c.IconClass)
            }).ToList()
        };
    }

    public async Task<NeighborRequestDescribeStepViewModel?> BuildDescribeStepAsync(
        NeighborRequestDraftState draft,
        CancellationToken ct)
    {
        var category = await db.IndorNeighborRequestCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == draft.CategoryId, ct);

        if (category == null)
        {
            return null;
        }

        return new NeighborRequestDescribeStepViewModel
        {
            PropiedadId = draft.PropiedadId,
            DisplayStep = 2,
            CategoryLabel = category.LabelEn,
            Title = draft.Title,
            Description = draft.Description,
            LocationAddress = draft.LocationAddress,
            NeededByDate = draft.NeededByDate,
            ExistingPhotoUrls = draft.PhotoPaths.ToList(),
            BackUrl = $"/NeighborRequest/Category?propiedadId={draft.PropiedadId}"
        };
    }

    public NeighborRequestPreferencesStepViewModel BuildPreferencesStep(NeighborRequestDraftState draft) =>
        new()
        {
            PropiedadId = draft.PropiedadId,
            DisplayStep = 3,
            TimelineCode = draft.TimelineCode,
            AudienceCode = draft.AudienceCode,
            BudgetAmount = draft.BudgetAmount,
            BackUrl = $"/NeighborRequest/Describe?propiedadId={draft.PropiedadId}",
            TimelineOptions =
            [
                (NeighborRequestTimelineCodes.Asap, "As soon as possible"),
                (NeighborRequestTimelineCodes.ThisWeek, "This week"),
                (NeighborRequestTimelineCodes.ThisMonth, "This month"),
                (NeighborRequestTimelineCodes.Flexible, "Flexible")
            ],
            AudienceOptions =
            [
                (NeighborRequestAudienceCodes.Neighbors, "Neighbors", "People in your network", "fa-users"),
                (NeighborRequestAudienceCodes.CertifiedProviders, "Certified providers", "Verified professionals", "fa-shield-halved")
            ]
        };

    public async Task<NeighborRequestReviewStepViewModel?> BuildReviewStepAsync(
        NeighborRequestDraftState draft,
        CancellationToken ct)
    {
        var category = await db.IndorNeighborRequestCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == draft.CategoryId, ct);

        if (category == null)
        {
            return null;
        }

        return new NeighborRequestReviewStepViewModel
        {
            PropiedadId = draft.PropiedadId,
            PageTitle = "Review request",
            DisplayStep = 4,
            CategoryLabel = category.LabelEn,
            CategoryIconClass = category.IconClass,
            Title = draft.Title,
            Description = draft.Description,
            PhotoUrls = draft.PhotoPaths.ToList(),
            LocationAddress = draft.LocationAddress,
            TimelineLabel = FormatTimelineLabel(draft.TimelineCode),
            AudienceLabel = FormatAudienceLabel(draft.AudienceCode),
            NeededByLabel = draft.NeededByDate?.ToString("MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US")),
            BudgetLabel = draft.BudgetAmount is > 0
                ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", draft.BudgetAmount.Value)
                : null,
            BackUrl = $"/NeighborRequest/Preferences?propiedadId={draft.PropiedadId}"
        };
    }

    public async Task<string?> SavePhotosAsync(
        NeighborRequestDraftState draft,
        IEnumerable<IFormFile>? files,
        CancellationToken ct)
    {
        if (files == null)
        {
            return null;
        }

        var uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "neighbor-requests", "drafts");
        Directory.CreateDirectory(uploadsDir);

        foreach (var file in files.Take(MaxPhotos - draft.PhotoPaths.Count))
        {
            if (file.Length <= 0 || file.Length > MaxPhotoBytes)
            {
                continue;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedPhotoExtensions.Contains(extension))
            {
                continue;
            }

            var fileName = $"draft-{draft.PropiedadId}-{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using var stream = File.Create(fullPath);
            await file.CopyToAsync(stream, ct);

            draft.PhotoPaths.Add($"/uploads/neighbor-requests/drafts/{fileName}");
        }

        return null;
    }

    public async Task<int?> PublishAsync(
        string userId,
        NeighborRequestDraftState draft,
        CancellationToken ct)
    {
        var propiedad = await db.Propiedades
            .FirstOrDefaultAsync(p => p.Id == draft.PropiedadId && p.UserId == userId && p.Activo, ct);

        if (propiedad == null)
        {
            return null;
        }

        var categoryExists = await db.IndorNeighborRequestCategories
            .AnyAsync(c => c.Id == draft.CategoryId && c.IsActive, ct);

        if (!categoryExists)
        {
            return null;
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var (lat, lng) = await ResolveCoordinatesAsync(propiedad, info, draft.LocationAddress, ct);

        var now = DateTime.UtcNow;
        var request = new IndorNeighborRequest
        {
            PropiedadId = draft.PropiedadId,
            UserId = userId,
            CategoryId = draft.CategoryId,
            Title = draft.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim(),
            LocationAddress = draft.LocationAddress.Trim(),
            NeededByDate = draft.NeededByDate?.Date,
            TimelineCode = draft.TimelineCode,
            AudienceCode = draft.AudienceCode,
            BudgetAmount = draft.BudgetAmount,
            Latitude = lat,
            Longitude = lng,
            Status = NeighborRequestStatuses.Active,
            IsActive = true,
            CreatedUtc = now,
            PublishedUtc = now,
            UpdatedUtc = now
        };

        db.IndorNeighborRequests.Add(request);
        await db.SaveChangesAsync(ct);

        if (draft.PhotoPaths.Count > 0)
        {
            var permanentDir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "neighbor-requests", request.Id.ToString());
            Directory.CreateDirectory(permanentDir);

            var sort = 0;
            foreach (var photoPath in draft.PhotoPaths.Take(MaxPhotos))
            {
                var sourcePath = Path.Combine(webHostEnvironment.WebRootPath, photoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(sourcePath))
                {
                    continue;
                }

                var extension = Path.GetExtension(sourcePath);
                var destFileName = $"photo-{++sort}{extension}";
                var destPath = Path.Combine(permanentDir, destFileName);
                File.Copy(sourcePath, destPath, overwrite: true);

                db.IndorNeighborRequestPhotos.Add(new IndorNeighborRequestPhoto
                {
                    RequestId = request.Id,
                    FilePath = $"/uploads/neighbor-requests/{request.Id}/{destFileName}",
                    SortOrder = sort,
                    CreatedUtc = now
                });
            }

            await db.SaveChangesAsync(ct);
        }

        return request.Id;
    }

    public async Task<NeighborRequestListViewModel> BuildMineAsync(
        string userId,
        int propiedadId,
        string? tab,
        IUrlHelper url,
        CancellationToken ct)
    {
        var activeTab = NormalizeTab(tab);
        var statusFilter = activeTab switch
        {
            "InProgress" => NeighborRequestStatuses.InProgress,
            "Completed" => NeighborRequestStatuses.Completed,
            _ => NeighborRequestStatuses.Active
        };

        List<IndorNeighborRequest> requests;
        try
        {
            requests = await db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Offers)
                .Where(r => r.UserId == userId && r.PropiedadId == propiedadId)
                .Where(r => r.Status == statusFilter)
                .OrderByDescending(r => r.PublishedUtc ?? r.CreatedUtc)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            requests = [];
        }

        return new NeighborRequestListViewModel
        {
            PropiedadId = propiedadId,
            ActiveTab = activeTab,
            Items = requests.Select(r =>
            {
                var offerCount = r.Offers.Count(o => o.Status == NeighborRequestOfferStatuses.Pending);
                return new NeighborRequestListItemViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    CategoryLabel = r.Category?.LabelEn ?? "Request",
                    PostedLabel = FormatRelativeTime(r.PublishedUtc ?? r.CreatedUtc),
                    Status = r.Status,
                    IconClass = r.Category?.IconClass ?? "fa-comment-dots",
                    OfferCount = offerCount,
                    OfferCountLabel = offerCount switch
                    {
                        0 => "No offers yet",
                        1 => "1 offer",
                        _ => $"{offerCount} offers"
                    },
                    DetailUrl = url.Action("Detail", "NeighborRequest", new { id = r.Id })!
                };
            }).ToList()
        };
    }

    public async Task<NeighborRequestDetailViewModel?> BuildDetailAsync(
        string userId,
        int requestId,
        IUrlHelper url,
        CancellationToken ct)
    {
        IndorNeighborRequest? request;
        try
        {
            request = await db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Photos.OrderBy(p => p.SortOrder))
                .Include(r => r.Offers.OrderByDescending(o => o.CreatedUtc))
                .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }

        if (request == null)
        {
            return null;
        }

        var pendingOffers = request.Offers
            .Where(o => o.Status == NeighborRequestOfferStatuses.Pending)
            .ToList();

        return new NeighborRequestDetailViewModel
        {
            Id = request.Id,
            PropiedadId = request.PropiedadId,
            Title = request.Title,
            CategoryLabel = request.Category?.LabelEn ?? "Request",
            IconClass = request.Category?.IconClass ?? "fa-comment-dots",
            StatusLabel = request.Status.ToUpperInvariant(),
            StatusCss = request.Status.ToLowerInvariant(),
            PostedLabel = FormatRelativeTime(request.PublishedUtc ?? request.CreatedUtc),
            LocationAddress = request.LocationAddress ?? string.Empty,
            Description = request.Description,
            PhotoUrls = request.Photos.Select(p => p.FilePath).ToList(),
            OfferCountLabel = pendingOffers.Count switch
            {
                0 => "No offers yet",
                1 => "1 offer",
                _ => $"{pendingOffers.Count} offers"
            },
            Offers = pendingOffers.Select(o => new NeighborRequestOfferItemViewModel
            {
                Id = o.Id,
                OffererName = o.OffererName,
                OffererPhotoUrl = o.OffererPhotoUrl,
                Message = o.Message,
                PriceLabel = o.PriceAmount is > 0
                    ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", o.PriceAmount.Value)
                    : null,
                MetaLabel = BuildOfferMetaLabel(o),
                IsVerified = o.IsVerified,
                DetailUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id })!
            }).ToList()
        };
    }

    public async Task EnsureDraftAsync(
        ISession session,
        Propiedad propiedad,
        CancellationToken ct)
    {
        var existing = LoadDraft(session);
        if (existing?.PropiedadId == propiedad.Id)
        {
            if (string.IsNullOrWhiteSpace(existing.LocationAddress))
            {
                existing.LocationAddress = await ResolveDefaultAddressAsync(propiedad, ct);
                SaveDraft(session, existing);
            }

            return;
        }

        SaveDraft(session, new NeighborRequestDraftState
        {
            PropiedadId = propiedad.Id,
            LocationAddress = await ResolveDefaultAddressAsync(propiedad, ct)
        });
    }

    private static async Task<string> ResolveDefaultAddressAsync(Propiedad propiedad, CancellationToken ct)
    {
        await Task.CompletedTask;
        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return !string.IsNullOrWhiteSpace(info?.FormattedAddress)
            ? info!.FormattedAddress
            : (propiedad.Direccion ?? string.Empty);
    }

    private async Task<(decimal? Lat, decimal? Lng)> ResolveCoordinatesAsync(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        string? locationAddress,
        CancellationToken ct)
    {
        if (info is { Latitude: not 0, Longitude: not 0 })
        {
            return (info.Latitude, info.Longitude);
        }

        var address = !string.IsNullOrWhiteSpace(locationAddress)
            ? locationAddress
            : info?.FormattedAddress ?? propiedad.Direccion;

        if (!string.IsNullOrWhiteSpace(address))
        {
            var coords = await addressLookup.GeocodeAddressAsync(address.Trim(), ct);
            if (coords is { Latitude: var lat, Longitude: var lng })
            {
                return (lat, lng);
            }
        }

        return (null, null);
    }

    private static string NormalizeTab(string? tab) =>
        tab?.Trim() switch
        {
            "InProgress" or "inprogress" or "In Progress" => "InProgress",
            "Completed" or "completed" => "Completed",
            _ => "Active"
        };

    private static string FormatTimelineLabel(string code) =>
        code switch
        {
            NeighborRequestTimelineCodes.Asap => "As soon as possible",
            NeighborRequestTimelineCodes.ThisMonth => "This month",
            NeighborRequestTimelineCodes.Flexible => "Flexible",
            _ => "This week"
        };

    private static string FormatAudienceLabel(string code) =>
        code == NeighborRequestAudienceCodes.CertifiedProviders
            ? "Certified providers"
            : "Neighbors";

    private static string BuildOfferMetaLabel(IndorNeighborRequestOffer offer)
    {
        var parts = new List<string>();
        if (offer.OfferType == NeighborRequestOfferTypes.Provider)
        {
            parts.Add(offer.IsVerified ? "Certified provider" : "Provider");
        }
        else
        {
            parts.Add("Neighbor");
        }

        if (offer.DistanceMiles is > 0)
        {
            parts.Add($"{offer.DistanceMiles:0.#} mi away");
        }

        return string.Join(" · ", parts);
    }

    public static string FormatRelativeTime(DateTime createdUtc)
    {
        var elapsed = DateTime.UtcNow - createdUtc;
        if (elapsed.TotalMinutes < 2)
        {
            return "Posted just now";
        }

        if (elapsed.TotalMinutes < 60)
        {
            var mins = Math.Max(1, (int)elapsed.TotalMinutes);
            return $"Posted {mins} min{(mins == 1 ? "" : "s")} ago";
        }

        if (elapsed.TotalHours < 24)
        {
            var hours = Math.Max(1, (int)elapsed.TotalHours);
            return $"Posted {hours} hour{(hours == 1 ? "" : "s")} ago";
        }

        var days = Math.Max(1, (int)elapsed.TotalDays);
        return $"Posted {days} day{(days == 1 ? "" : "s")} ago";
    }

    private static string ResolveCategoryLabel(string code, string? labelEn) =>
        !string.IsNullOrWhiteSpace(labelEn) ? labelEn.Trim() : code.Trim().ToLowerInvariant() switch
        {
            "home-improvements" => "Home Improvement",
            "yard-patio" => "Lawn & Yard",
            "cleaning" => "Cleaning",
            "moving-hauling" => "Moving & Hauling",
            "tech-internet" => "Tech & Internet",
            "other" => "Other",
            _ => code
        };

    private static string? ResolveCategoryDescription(string code, string? descriptionEn) =>
        !string.IsNullOrWhiteSpace(descriptionEn) ? descriptionEn.Trim() : code.Trim().ToLowerInvariant() switch
        {
            "home-improvements" => "Repairs, maintenance, upgrades",
            "yard-patio" => "Mowing, landscaping, cleanup",
            "cleaning" => "Home, windows, gutters",
            "moving-hauling" => "Moving help, junk removal",
            "tech-internet" => "Wi-Fi, devices, smart home",
            "other" => "Something else",
            _ => null
        };

    private static string ResolveCategoryIcon(string code, string? iconClass) =>
        !string.IsNullOrWhiteSpace(iconClass) ? iconClass.Trim() : code.Trim().ToLowerInvariant() switch
        {
            "home-improvements" => "fa-house",
            "yard-patio" => "fa-seedling",
            "cleaning" => "fa-wand-magic-sparkles",
            "moving-hauling" => "fa-dolly",
            "tech-internet" => "fa-wifi",
            "other" => "fa-ellipsis",
            _ => "fa-circle"
        };
}
