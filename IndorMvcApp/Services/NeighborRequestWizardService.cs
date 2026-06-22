using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    private static readonly (string Value, string Label, TimeOnly Start, TimeOnly End)[] TimeWindowPresets =
    [
        ("morning", "9:00 AM – 12:00 PM", new TimeOnly(9, 0), new TimeOnly(12, 0)),
        ("afternoon", "12:00 PM – 3:00 PM", new TimeOnly(12, 0), new TimeOnly(15, 0)),
        ("late-afternoon", "3:00 PM – 6:00 PM", new TimeOnly(15, 0), new TimeOnly(18, 0)),
        ("evening", "6:00 PM – 9:00 PM", new TimeOnly(18, 0), new TimeOnly(21, 0))
    ];

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

    public NeighborRequestPreferencesStepViewModel BuildPreferencesStep(NeighborRequestDraftState draft)
    {
        var isEdit = draft.EditingRequestId is > 0;
        return new NeighborRequestPreferencesStepViewModel
        {
            PropiedadId = draft.PropiedadId,
            RequestId = draft.EditingRequestId,
            IsEditMode = isEdit,
            PageTitle = isEdit ? "Edit request" : "Post a Request",
            DisplayStep = isEdit ? 2 : 3,
            TotalSteps = isEdit ? 3 : 5,
            StepLabels = isEdit
                ? ["Details", "Preferences", "Review"]
                : ["Category", "Describe", "Preferences", "Review", "Done"],
            TimelineCode = draft.TimelineCode,
            AudienceCode = draft.AudienceCode,
            SelectedAudiences = ExpandAudienceCode(draft.AudienceCode),
            BudgetAmount = draft.BudgetAmount,
            BackUrl = isEdit && draft.EditingRequestId is > 0
                ? $"/NeighborRequest/Edit/{draft.EditingRequestId}"
                : $"/NeighborRequest/Describe?propiedadId={draft.PropiedadId}",
            CloseUrl = isEdit && draft.EditingRequestId is > 0
                ? $"/NeighborRequest/Detail/{draft.EditingRequestId}"
                : "/Home/Index",
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
    }

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

        var isEdit = draft.EditingRequestId is > 0;
        return new NeighborRequestReviewStepViewModel
        {
            PropiedadId = draft.PropiedadId,
            RequestId = draft.EditingRequestId,
            IsEditMode = isEdit,
            PageTitle = isEdit ? "Edit request" : "Review request",
            DisplayStep = isEdit ? 3 : 4,
            TotalSteps = isEdit ? 3 : 5,
            StepLabels = isEdit
                ? ["Details", "Preferences", "Review"]
                : ["Category", "Describe", "Preferences", "Review", "Done"],
            CategoryLabel = category.LabelEn,
            CategoryIconClass = category.IconClass,
            Title = draft.Title,
            DetailsSummary = string.IsNullOrWhiteSpace(draft.DetailsSummary) ? null : draft.DetailsSummary.Trim(),
            Description = draft.Description,
            PhotoUrls = draft.PhotoPaths.ToList(),
            LocationAddress = draft.LocationAddress,
            TimelineLabel = FormatTimelineLabel(draft.TimelineCode),
            AudienceLabel = FormatAudienceLabel(draft.AudienceCode),
            NeededByLabel = draft.NeededByDate?.ToString("dddd, MMMM d", CultureInfo.GetCultureInfo("en-US")),
            TimeWindowLabel = FormatTimeWindowLabel(draft.TimeWindowStart, draft.TimeWindowEnd),
            BudgetLabel = draft.BudgetAmount is > 0
                ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", draft.BudgetAmount.Value)
                : null,
            BackUrl = isEdit
                ? $"/NeighborRequest/Preferences?propiedadId={draft.PropiedadId}"
                : $"/NeighborRequest/Preferences?propiedadId={draft.PropiedadId}",
            CloseUrl = isEdit && draft.EditingRequestId is > 0
                ? $"/NeighborRequest/Detail/{draft.EditingRequestId}"
                : "/Home/Index",
            PublishButtonLabel = isEdit ? "Save changes" : "Post request",
            PublishSuccessUrl = isEdit && draft.EditingRequestId is > 0
                ? $"/NeighborRequest/Detail/{draft.EditingRequestId}"
                : string.Empty
        };
    }

    public async Task<NeighborRequestEditDetailsStepViewModel?> BuildEditDetailsStepAsync(
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
                .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }

        if (request == null || request.Status is NeighborRequestStatuses.Completed or NeighborRequestStatuses.Cancelled)
        {
            return null;
        }

        var categories = await LoadCategoriesAsync(ct);
        var category = request.Category ?? categories.FirstOrDefault(c => c.Id == request.CategoryId);
        var (parsedStats, narrativeDescription) = BuildDetailStats(request.Description, null);
        var detailsSummary = request.DetailsSummary;
        if (string.IsNullOrWhiteSpace(detailsSummary) && parsedStats.Count > 0)
        {
            detailsSummary = string.Join(", ", parsedStats.Select(s => s.Label));
        }

        var description = !string.IsNullOrWhiteSpace(narrativeDescription)
            ? narrativeDescription
            : string.IsNullOrWhiteSpace(detailsSummary)
                ? request.Description
                : null;

        return new NeighborRequestEditDetailsStepViewModel
        {
            PropiedadId = request.PropiedadId,
            RequestId = request.Id,
            IsEditMode = true,
            PageTitle = "Edit request",
            DisplayStep = 1,
            TotalSteps = 3,
            StepLabels = ["Details", "Preferences", "Review"],
            BackUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }),
            CloseUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id })!,
            CategoryId = request.CategoryId,
            CategoryLabel = ResolveCategoryLabel(category?.Code ?? string.Empty, category?.LabelEn),
            CategoryIconClass = ResolveCategoryIcon(category?.Code ?? string.Empty, category?.IconClass),
            DetailsSummary = detailsSummary,
            Description = description,
            NeededByDate = request.NeededByDate,
            TimeWindowPreset = ResolveTimeWindowPresetValue(request.TimeWindowStart, request.TimeWindowEnd),
            AudienceCode = request.AudienceCode,
            Categories = categories.Select(c => new NeighborRequestCategoryOptionViewModel
            {
                Id = c.Id,
                Label = ResolveCategoryLabel(c.Code, c.LabelEn),
                Description = ResolveCategoryDescription(c.Code, c.DescriptionEn),
                IconClass = ResolveCategoryIcon(c.Code, c.IconClass)
            }).ToList(),
            TimeWindowOptions = TimeWindowPresets.Select(p => (p.Value, p.Label)).ToList()
        };
    }

    public async Task<bool> SaveEditDetailsStepAsync(
        string userId,
        NeighborRequestEditDetailsStepViewModel model,
        ISession session,
        CancellationToken ct)
    {
        IndorNeighborRequest? request;
        try
        {
            request = await db.IndorNeighborRequests
                .FirstOrDefaultAsync(r => r.Id == model.RequestId && r.UserId == userId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return false;
        }

        if (request == null || request.Status is NeighborRequestStatuses.Completed or NeighborRequestStatuses.Cancelled)
        {
            return false;
        }

        var categoryExists = await db.IndorNeighborRequestCategories
            .AnyAsync(c => c.Id == model.CategoryId && c.IsActive, ct);

        if (!categoryExists)
        {
            return false;
        }

        var (timeStart, timeEnd) = ResolveTimeWindowPreset(model.TimeWindowPreset);
        var now = DateTime.UtcNow;

        request.CategoryId = model.CategoryId;
        request.DetailsSummary = string.IsNullOrWhiteSpace(model.DetailsSummary) ? null : model.DetailsSummary.Trim();
        request.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        request.NeededByDate = model.NeededByDate?.Date;
        request.TimeWindowStart = timeStart;
        request.TimeWindowEnd = timeEnd;
        request.AudienceCode = NormalizeAudienceCode(model.AudienceCode);
        request.UpdatedUtc = now;

        await db.SaveChangesAsync(ct);

        var draft = LoadDraft(session) ?? new NeighborRequestDraftState { PropiedadId = request.PropiedadId };
        draft.PropiedadId = request.PropiedadId;
        draft.EditingRequestId = request.Id;
        draft.CategoryId = request.CategoryId;
        draft.Title = request.Title;
        draft.DetailsSummary = request.DetailsSummary ?? string.Empty;
        draft.Description = request.Description ?? string.Empty;
        draft.LocationAddress = request.LocationAddress ?? draft.LocationAddress;
        draft.NeededByDate = request.NeededByDate;
        draft.TimeWindowStart = request.TimeWindowStart;
        draft.TimeWindowEnd = request.TimeWindowEnd;
        draft.AudienceCode = request.AudienceCode;
        draft.TimelineCode = request.TimelineCode;
        draft.BudgetAmount = request.BudgetAmount;
        SaveDraft(session, draft);
        return true;
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

        if (draft.EditingRequestId is > 0)
        {
            var existing = await db.IndorNeighborRequests
                .Include(r => r.Photos)
                .FirstOrDefaultAsync(r => r.Id == draft.EditingRequestId.Value && r.UserId == userId, ct);

            if (existing == null)
            {
                return null;
            }

            existing.CategoryId = draft.CategoryId;
            existing.Title = draft.Title.Trim();
            existing.Description = string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim();
            existing.DetailsSummary = string.IsNullOrWhiteSpace(draft.DetailsSummary) ? null : draft.DetailsSummary.Trim();
            existing.LocationAddress = draft.LocationAddress.Trim();
            existing.NeededByDate = draft.NeededByDate?.Date;
            existing.TimeWindowStart = draft.TimeWindowStart;
            existing.TimeWindowEnd = draft.TimeWindowEnd;
            existing.TimelineCode = draft.TimelineCode;
            existing.AudienceCode = draft.AudienceCode;
            existing.BudgetAmount = draft.BudgetAmount;
            existing.Latitude = lat;
            existing.Longitude = lng;
            existing.Status = NeighborRequestStatuses.Active;
            existing.UpdatedUtc = now;
            existing.PublishedUtc ??= now;

            await db.SaveChangesAsync(ct);
            await SyncRequestPhotosAsync(existing, draft, now, ct);
            return existing.Id;
        }

        var request = new IndorNeighborRequest
        {
            PropiedadId = draft.PropiedadId,
            UserId = userId,
            CategoryId = draft.CategoryId,
            Title = draft.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(draft.Description) ? null : draft.Description.Trim(),
            DetailsSummary = string.IsNullOrWhiteSpace(draft.DetailsSummary) ? null : draft.DetailsSummary.Trim(),
            LocationAddress = draft.LocationAddress.Trim(),
            NeededByDate = draft.NeededByDate?.Date,
            TimeWindowStart = draft.TimeWindowStart,
            TimeWindowEnd = draft.TimeWindowEnd,
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
                var extension = Path.GetExtension(photoPath);
                var destFileName = $"photo-{++sort}{extension}";
                var destPath = Path.Combine(permanentDir, destFileName);
                if (!TryCopyDraftPhoto(photoPath, destPath))
                {
                    continue;
                }

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

    public async Task<int?> LoadRequestForEditAsync(
        string userId,
        int requestId,
        ISession session,
        CancellationToken ct)
    {
        IndorNeighborRequest? request;
        try
        {
            request = await db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Photos.OrderBy(p => p.SortOrder))
                .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }

        if (request == null || request.Status is NeighborRequestStatuses.Completed or NeighborRequestStatuses.Cancelled)
        {
            return null;
        }

        var (parsedStats, narrativeDescription) = BuildDetailStats(request.Description, null);
        var detailsSummary = request.DetailsSummary;
        if (string.IsNullOrWhiteSpace(detailsSummary) && parsedStats.Count > 0)
        {
            detailsSummary = string.Join(", ", parsedStats.Select(s => s.Label));
        }

        var description = !string.IsNullOrWhiteSpace(narrativeDescription)
            ? narrativeDescription
            : string.IsNullOrWhiteSpace(detailsSummary)
                ? request.Description ?? string.Empty
                : string.Empty;

        SaveDraft(session, new NeighborRequestDraftState
        {
            PropiedadId = request.PropiedadId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            DetailsSummary = detailsSummary ?? string.Empty,
            Description = description ?? string.Empty,
            LocationAddress = request.LocationAddress ?? string.Empty,
            NeededByDate = request.NeededByDate,
            TimeWindowStart = request.TimeWindowStart,
            TimeWindowEnd = request.TimeWindowEnd,
            TimelineCode = request.TimelineCode,
            AudienceCode = request.AudienceCode,
            BudgetAmount = request.BudgetAmount,
            PhotoPaths = request.Photos.OrderBy(p => p.SortOrder).Select(p => p.FilePath).ToList(),
            EditingRequestId = request.Id
        });

        return request.PropiedadId;
    }

    public async Task<int?> CancelRequestAsync(
        string userId,
        int requestId,
        string? cancelReasonCode,
        string? cancelNote,
        CancellationToken ct)
    {
        IndorNeighborRequest? request;
        try
        {
            request = await db.IndorNeighborRequests
                .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }

        if (request == null || request.Status is NeighborRequestStatuses.Completed or NeighborRequestStatuses.Cancelled)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        request.Status = NeighborRequestStatuses.Cancelled;
        request.CancelReasonCode = NormalizeCancelReasonCode(cancelReasonCode);
        request.CancelNote = string.IsNullOrWhiteSpace(cancelNote) ? null : cancelNote.Trim();
        request.CancelledUtc = now;
        request.UpdatedUtc = now;
        await db.SaveChangesAsync(ct);
        return request.PropiedadId;
    }

    public async Task<NeighborRequestCancelViewModel?> BuildCancelStepAsync(
        string userId,
        int requestId,
        IUrlHelper url,
        CancellationToken ct)
    {
        IndorNeighborRequest? request;
        Propiedad? propiedad;
        try
        {
            request = await db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, ct);

            propiedad = request == null
                ? null
                : await db.Propiedades.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.PropiedadId && p.UserId == userId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }

        if (request == null || request.Status is NeighborRequestStatuses.Completed or NeighborRequestStatuses.Cancelled)
        {
            return null;
        }

        var propertyInfo = propiedad == null ? null : MyHomeDisplayService.DeserializeProperty(propiedad);
        var statsSource = !string.IsNullOrWhiteSpace(request.DetailsSummary)
            ? request.DetailsSummary
            : request.Description;
        var (detailStats, _) = BuildDetailStats(statsSource, propertyInfo);

        return new NeighborRequestCancelViewModel
        {
            Id = request.Id,
            PropiedadId = request.PropiedadId,
            Title = request.Title,
            IconClass = ResolveCategoryIcon(request.Category?.Code ?? string.Empty, request.Category?.IconClass),
            StatusLabel = FormatStatusLabel(request.Status),
            StatusCss = request.Status.ToLowerInvariant(),
            LocationAddress = request.LocationAddress ?? string.Empty,
            DetailStats = detailStats,
            BackUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }) ?? "/",
            KeepUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }) ?? "/",
            CreateUrl = url.Action("Create", "NeighborRequest", new { propiedadId = request.PropiedadId }) ?? "/",
            ReasonOptions = NeighborRequestCancelReasons.Options
        };
    }

    private static string? NormalizeCancelReasonCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return NeighborRequestCancelReasons.Options.Any(o =>
            string.Equals(o.Value, code, StringComparison.OrdinalIgnoreCase))
            ? code.Trim()
            : null;
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
        Propiedad? propiedad;
        try
        {
            request = await db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Photos.OrderBy(p => p.SortOrder))
                .Include(r => r.Offers.OrderByDescending(o => o.CreatedUtc))
                .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, ct);

            propiedad = request == null
                ? null
                : await db.Propiedades.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == request.PropiedadId && p.UserId == userId, ct);
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

        var neighborOfferEntities = pendingOffers
            .Where(o => o.OfferType != NeighborRequestOfferTypes.Provider)
            .ToList();
        var providerOfferEntities = pendingOffers
            .Where(o => o.OfferType == NeighborRequestOfferTypes.Provider)
            .ToList();

        var neighborOffers = neighborOfferEntities
            .Select(o => MapOfferToViewModel(o, request, url))
            .ToList();

        var providerOffers = providerOfferEntities
            .Select(o => MapOfferToViewModel(o, request, url))
            .ToList();

        if (providerOffers.Count == 0 && ShouldShowProvidersPanel(request))
        {
            providerOffers = await LoadSuggestedProviderOffersAsync(request, url, ct);
        }

        var hasNeighborOffers = neighborOffers.Count > 0;
        var offerCountLabel = neighborOffers.Count switch
        {
            0 => "No offers yet",
            1 => "1 offer received",
            _ => $"{neighborOffers.Count} offers received"
        };

        var propertyInfo = propiedad == null ? null : MyHomeDisplayService.DeserializeProperty(propiedad);
        var photoUrls = request.Photos.Select(p => p.FilePath).ToList();
        var statsSource = !string.IsNullOrWhiteSpace(request.DetailsSummary)
            ? request.DetailsSummary
            : request.Description;
        var (detailStats, parsedDescription) = BuildDetailStats(statsSource, propertyInfo);
        var descriptionText = !string.IsNullOrWhiteSpace(request.DetailsSummary)
            ? request.Description
            : parsedDescription ?? request.Description;
        const int descriptionPreviewLength = 140;

        return new NeighborRequestDetailViewModel
        {
            Id = request.Id,
            PropiedadId = request.PropiedadId,
            Title = request.Title,
            CategoryLabel = request.Category?.LabelEn ?? "Request",
            IconClass = ResolveCategoryIcon(request.Category?.Code ?? string.Empty, request.Category?.IconClass),
            StatusLabel = FormatStatusLabel(request.Status),
            StatusCss = request.Status.ToLowerInvariant(),
            PostedLabel = FormatRelativeTime(request.PublishedUtc ?? request.CreatedUtc),
            LocationAddress = request.LocationAddress ?? string.Empty,
            Description = request.Description,
            DescriptionBody = descriptionText,
            IsDescriptionLong = !string.IsNullOrWhiteSpace(descriptionText) && descriptionText.Length > descriptionPreviewLength,
            HeroImageUrl = photoUrls.FirstOrDefault() ?? "/welcome-house.png",
            PhotoUrls = photoUrls,
            DetailStats = detailStats,
            Offers = neighborOffers,
            NeighborOffers = neighborOffers,
            ProviderOffers = providerOffers,
            TotalNeighborOfferCount = neighborOffers.Count,
            OfferCountLabel = offerCountLabel,
            HasNeighborOffers = hasNeighborOffers,
            ShowProvidersPanel = ShouldShowProvidersPanel(request),
            CanManage = request.Status is NeighborRequestStatuses.Active or NeighborRequestStatuses.InProgress && !hasNeighborOffers,
            BackUrl = url.Action("Index", "Home") ?? "/",
            EditUrl = url.Action("Edit", "NeighborRequest", new { id = request.Id }) ?? "#",
            CancelUrl = url.Action("Cancel", "NeighborRequest", new { id = request.Id }) ?? "#",
            ProvidersUrl = url.Action("Edit", "NeighborRequest", new { id = request.Id, step = "providers" }) ?? "#",
            SeeAllOffersUrl = url.Action("Offers", "NeighborRequest", new { id = request.Id }) ?? "#",
            ViewProvidersUrl = url.Action("Index", "Home") + "#section-services"
        };
    }

    public async Task<NeighborRequestOffersListViewModel?> BuildOffersListAsync(
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

        var offers = request.Offers
            .Where(o => o.Status == NeighborRequestOfferStatuses.Pending
                && o.OfferType != NeighborRequestOfferTypes.Provider)
            .Select(o => MapOfferToViewModel(o, request, url))
            .ToList();

        return new NeighborRequestOffersListViewModel
        {
            RequestId = request.Id,
            Title = request.Title,
            BackUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }) ?? "/",
            Offers = offers
        };
    }

    private static bool ShouldShowProvidersPanel(IndorNeighborRequest request) =>
        request.Status is NeighborRequestStatuses.Active or NeighborRequestStatuses.InProgress;

    private NeighborRequestOfferItemViewModel MapOfferToViewModel(
        IndorNeighborRequestOffer offer,
        IndorNeighborRequest request,
        IUrlHelper url)
    {
        var isProvider = offer.OfferType == NeighborRequestOfferTypes.Provider;
        return new NeighborRequestOfferItemViewModel
        {
            Id = offer.Id,
            ProviderId = offer.ProviderId,
            OffererName = offer.OffererName,
            OffererPhotoUrl = offer.OffererPhotoUrl,
            AvatarIconClass = ResolveOfferAvatarIcon(offer, isProvider),
            Message = offer.Message,
            PriceLabel = offer.PriceAmount is > 0
                ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", offer.PriceAmount.Value)
                : null,
            ScheduleLabel = offer.ScheduleLabel ?? BuildDefaultScheduleLabel(request, offer),
            RatingLabel = FormatRatingLabel(offer.Rating, offer.Id),
            MetaLabel = BuildOfferMetaLabel(offer),
            RoleLabel = isProvider ? "INDOR Provider" : "Neighbor",
            IsVerified = offer.IsVerified,
            IsProviderOffer = isProvider,
            DetailUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }) ?? "#",
            ViewUrl = url.Action("Offers", "NeighborRequest", new { id = request.Id }) ?? "#",
            MessageUrl = url.Action("Index", "Home") + "#section-more"
        };
    }

    private async Task<List<NeighborRequestOfferItemViewModel>> LoadSuggestedProviderOffersAsync(
        IndorNeighborRequest request,
        IUrlHelper url,
        CancellationToken ct)
    {
        var categoryCode = MapRequestCategoryToProviderCategory(request.Category?.Code);
        if (string.IsNullOrWhiteSpace(categoryCode))
        {
            return [];
        }

        List<int> providerIds;
        try
        {
            providerIds = await db.IndorProveedorCategoriasSel
                .AsNoTracking()
                .Where(s => s.CategoriaId == categoryCode)
                .Select(s => s.ProveedorId)
                .Distinct()
                .Take(6)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        if (providerIds.Count == 0)
        {
            return [];
        }

        var activeStatuses = new[]
        {
            ProviderRegistrationStatuses.Approved,
            ProviderRegistrationStatuses.IndorProActive,
            ProviderRegistrationStatuses.PendingReview
        };

        List<IndorProveedor> providers;
        try
        {
            providers = await db.IndorProveedores
                .AsNoTracking()
                .Where(p => providerIds.Contains(p.Id) && activeStatuses.Contains(p.RegistrationStatus))
                .OrderByDescending(p => p.FechaActualizacion)
                .Take(3)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        var icons = new[] { "fa-house", "fa-leaf", "fa-broom" };
        var basePrice = request.BudgetAmount is > 0 ? request.BudgetAmount.Value : 120m;
        var results = new List<NeighborRequestOfferItemViewModel>();

        for (var i = 0; i < providers.Count; i++)
        {
            var provider = providers[i];
            var name = !string.IsNullOrWhiteSpace(provider.DbaName)
                ? provider.DbaName.Trim()
                : provider.BusinessName?.Trim() ?? "INDOR Provider";
            var price = basePrice + (i * 5m) + (provider.Id % 3);

            results.Add(new NeighborRequestOfferItemViewModel
            {
                Id = 0,
                ProviderId = provider.Id,
                OffererName = name,
                AvatarIconClass = icons[i % icons.Length],
                PriceLabel = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", price),
                ScheduleLabel = BuildSuggestedScheduleLabel(request, i),
                RatingLabel = FormatRatingLabel(null, provider.Id),
                RoleLabel = "INDOR Provider",
                MetaLabel = "INDOR Provider",
                IsVerified = string.Equals(provider.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(provider.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase),
                IsProviderOffer = true,
                DetailUrl = url.Action("Index", "Home") + "#section-services",
                ViewUrl = url.Action("Index", "Home") + "#section-services",
                MessageUrl = url.Action("Index", "Home") + "#section-more"
            });
        }

        return results;
    }

    private static string? MapRequestCategoryToProviderCategory(string? code) =>
        code?.Trim().ToLowerInvariant() switch
        {
            "cleaning" => "cleaning",
            "yard-patio" => "landscaping",
            "home-improvements" => "handyman",
            "moving-hauling" => "handyman",
            "tech-internet" => "handyman",
            _ => "handyman"
        };

    private static string ResolveOfferAvatarIcon(IndorNeighborRequestOffer offer, bool isProvider) =>
        isProvider ? "fa-shield-halved" : "fa-wand-magic-sparkles";

    private static string? FormatRatingLabel(decimal? rating, int seed) =>
        rating is > 0
            ? rating.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : (4.7m + (seed % 3) * 0.1m).ToString("0.0", CultureInfo.InvariantCulture);

    private static string BuildDefaultScheduleLabel(IndorNeighborRequest request, IndorNeighborRequestOffer offer)
    {
        if (!string.IsNullOrWhiteSpace(offer.ScheduleLabel))
        {
            return offer.ScheduleLabel;
        }

        if (request.NeededByDate is { } neededBy)
        {
            var time = request.TimeWindowStart?.ToString("h:mm tt", CultureInfo.GetCultureInfo("en-US"));
            var dayLabel = neededBy.Date == DateTime.UtcNow.Date
                ? "Today"
                : neededBy.ToString("MMM d", CultureInfo.GetCultureInfo("en-US"));
            return string.IsNullOrWhiteSpace(time) ? dayLabel : $"{dayLabel}, {time}";
        }

        return "Available soon";
    }

    private static string BuildSuggestedScheduleLabel(IndorNeighborRequest request, int index)
    {
        var baseDate = request.NeededByDate?.Date ?? DateTime.UtcNow.Date.AddDays(1);
        if (index > 0)
        {
            baseDate = baseDate.AddDays(index);
        }

        var time = request.TimeWindowStart?.AddHours(index).ToString("h:mm tt", CultureInfo.GetCultureInfo("en-US"))
            ?? "9:00 AM";
        var dayLabel = baseDate.Date == DateTime.UtcNow.Date
            ? "Today"
            : baseDate.Date == DateTime.UtcNow.Date.AddDays(1)
                ? "Tomorrow"
                : baseDate.ToString("MMM d", CultureInfo.GetCultureInfo("en-US"));

        return $"{dayLabel}, {time}";
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
        code switch
        {
            NeighborRequestAudienceCodes.CertifiedProviders => "INDOR Providers",
            NeighborRequestAudienceCodes.Both => "Neighbors and INDOR Providers",
            _ => "Neighbors"
        };

    private static string NormalizeAudienceCode(string? code) =>
        code?.Trim() switch
        {
            NeighborRequestAudienceCodes.CertifiedProviders => NeighborRequestAudienceCodes.CertifiedProviders,
            NeighborRequestAudienceCodes.Both => NeighborRequestAudienceCodes.Both,
            _ => NeighborRequestAudienceCodes.Neighbors
        };

    public static List<string> ExpandAudienceCode(string? code) =>
        code?.Trim() switch
        {
            NeighborRequestAudienceCodes.Both =>
                [NeighborRequestAudienceCodes.Neighbors, NeighborRequestAudienceCodes.CertifiedProviders],
            NeighborRequestAudienceCodes.CertifiedProviders =>
                [NeighborRequestAudienceCodes.CertifiedProviders],
            _ => [NeighborRequestAudienceCodes.Neighbors]
        };

    public static string CombineAudienceCodes(IEnumerable<string>? codes)
    {
        var set = codes is null
            ? new HashSet<string>()
            : new HashSet<string>(codes.Select(c => c?.Trim()).Where(c => !string.IsNullOrEmpty(c))!, StringComparer.OrdinalIgnoreCase);

        var hasNeighbors = set.Contains(NeighborRequestAudienceCodes.Neighbors);
        var hasProviders = set.Contains(NeighborRequestAudienceCodes.CertifiedProviders);

        if (hasNeighbors && hasProviders)
        {
            return NeighborRequestAudienceCodes.Both;
        }

        if (hasProviders)
        {
            return NeighborRequestAudienceCodes.CertifiedProviders;
        }

        return NeighborRequestAudienceCodes.Neighbors;
    }

    private static (TimeOnly? Start, TimeOnly? End) ResolveTimeWindowPreset(string? presetValue)
    {
        if (string.IsNullOrWhiteSpace(presetValue))
        {
            return (null, null);
        }

        var match = TimeWindowPresets.FirstOrDefault(p =>
            string.Equals(p.Value, presetValue, StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrWhiteSpace(match.Label)
            ? (null, null)
            : (match.Start, match.End);
    }

    private static string? ResolveTimeWindowPresetValue(TimeOnly? start, TimeOnly? end)
    {
        if (start == null || end == null)
        {
            return TimeWindowPresets[0].Value;
        }

        var match = TimeWindowPresets.FirstOrDefault(p => p.Start == start && p.End == end);
        return string.IsNullOrWhiteSpace(match.Label) ? TimeWindowPresets[0].Value : match.Value;
    }

    private static string? FormatTimeWindowLabel(TimeOnly? start, TimeOnly? end)
    {
        if (start == null || end == null)
        {
            return null;
        }

        var match = TimeWindowPresets.FirstOrDefault(p => p.Start == start && p.End == end);
        if (!string.IsNullOrWhiteSpace(match.Label))
        {
            return match.Label;
        }

        var culture = CultureInfo.GetCultureInfo("en-US");
        return $"{start.Value.ToString("h:mm tt", culture)} – {end.Value.ToString("h:mm tt", culture)}";
    }

    public async Task PersistPreferencesAsync(string userId, NeighborRequestDraftState draft, CancellationToken ct)
    {
        if (draft.EditingRequestId is not > 0)
        {
            return;
        }

        try
        {
            var request = await db.IndorNeighborRequests
                .FirstOrDefaultAsync(r => r.Id == draft.EditingRequestId && r.UserId == userId, ct);

            if (request == null)
            {
                return;
            }

            request.TimelineCode = draft.TimelineCode;
            request.BudgetAmount = draft.BudgetAmount;
            request.UpdatedUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            // Tables not deployed yet.
        }
    }

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
            "cleaning" => "House cleaning",
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

    private static string FormatStatusLabel(string status) =>
        status switch
        {
            NeighborRequestStatuses.Active => "ACTIVE",
            NeighborRequestStatuses.InProgress => "IN PROGRESS",
            NeighborRequestStatuses.Completed => "COMPLETED",
            NeighborRequestStatuses.Cancelled => "CANCELLED",
            _ => status.ToUpperInvariant()
        };

    private static (List<NeighborRequestDetailStatViewModel> Stats, string? DescriptionBody) BuildDetailStats(
        string? description,
        PropertyInfoViewModel? propertyInfo)
    {
        var stats = new List<NeighborRequestDetailStatViewModel>();
        var text = description?.Trim() ?? string.Empty;
        var hasBedStat = false;
        var hasBathStat = false;

        if (propertyInfo?.PropertyDetails?.Bedrooms is > 0)
        {
            stats.Add(CreateStat("fa-bed", propertyInfo.PropertyDetails.Bedrooms.Value, "bedroom", "bedrooms"));
            hasBedStat = true;
        }

        if (propertyInfo?.PropertyDetails?.Bathrooms is > 0)
        {
            var baths = propertyInfo.PropertyDetails.Bathrooms.Value;
            var bathLabel = baths % 1 == 0
                ? $"{baths:0} {(baths == 1 ? "bathroom" : "bathrooms")}"
                : $"{baths:0.#} bathrooms";
            stats.Add(new NeighborRequestDetailStatViewModel { IconClass = "fa-bath", Label = bathLabel });
            hasBathStat = true;
        }

        if (!hasBedStat)
        {
            var beds = MatchCount(text, @"(\d+)\s*(?:bed(?:room)?s?|cuartos?)");
            if (beds is > 0)
            {
                stats.Add(CreateStat("fa-bed", beds, "bedroom", "bedrooms"));
                text = Regex.Replace(text, @"(\d+)\s*(?:bed(?:room)?s?|cuartos?)", string.Empty, RegexOptions.IgnoreCase);
            }
        }

        if (!hasBathStat)
        {
            var baths = MatchCount(text, @"(\d+)\s*(?:bath(?:room)?s?|baños?)");
            if (baths is > 0)
            {
                stats.Add(CreateStat("fa-bath", baths, "bathroom", "bathrooms"));
                text = Regex.Replace(text, @"(\d+)\s*(?:bath(?:room)?s?|baños?)", string.Empty, RegexOptions.IgnoreCase);
            }
        }

        var cats = MatchCount(text, @"(\d+)\s*(?:cat|cats|gato|gatos)");
        if (cats is > 0)
        {
            stats.Add(CreateStat("fa-cat", cats, "cat", "cats"));
            text = Regex.Replace(text, @"(\d+)\s*(?:cat|cats|gato|gatos)", string.Empty, RegexOptions.IgnoreCase);
        }
        else if (Regex.IsMatch(text, @"\b(un|una|1)\s+(?:cat|gato)\b", RegexOptions.IgnoreCase))
        {
            stats.Add(new NeighborRequestDetailStatViewModel { IconClass = "fa-cat", Label = "1 cat" });
            text = Regex.Replace(text, @"\b(un|una|1)\s+(?:cat|gato)\b", string.Empty, RegexOptions.IgnoreCase);
        }

        var dogs = MatchCount(text, @"(\d+)\s*(?:dog|dogs|perro|perros)");
        if (dogs is > 0)
        {
            stats.Add(CreateStat("fa-dog", dogs, "dog", "dogs"));
            text = Regex.Replace(text, @"(\d+)\s*(?:dog|dogs|perro|perros)", string.Empty, RegexOptions.IgnoreCase);
        }

        text = Regex.Replace(text, @"\s{2,}", " ").Trim(' ', ',', '.', ';');
        return (stats, string.IsNullOrWhiteSpace(text) ? null : text);
    }

    private static NeighborRequestDetailStatViewModel CreateStat(string icon, int count, string singular, string plural) =>
        new()
        {
            IconClass = icon,
            Label = count == 1 ? $"1 {singular}" : $"{count} {plural}"
        };

    private static int MatchCount(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var count) ? count : 0;
    }

    private async Task SyncRequestPhotosAsync(
        IndorNeighborRequest request,
        NeighborRequestDraftState draft,
        DateTime now,
        CancellationToken ct)
    {
        if (draft.PhotoPaths.Count == 0)
        {
            return;
        }

        var permanentDir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "neighbor-requests", request.Id.ToString());
        Directory.CreateDirectory(permanentDir);

        db.IndorNeighborRequestPhotos.RemoveRange(request.Photos);

        var sort = 0;
        foreach (var photoPath in draft.PhotoPaths.Take(MaxPhotos))
        {
            var extension = Path.GetExtension(photoPath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            var destFileName = $"photo-{++sort}{extension}";
            var destPath = Path.Combine(permanentDir, destFileName);
            if (!TryCopyDraftPhoto(photoPath, destPath))
            {
                continue;
            }

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

    private bool TryCopyDraftPhoto(string photoPath, string destPath)
    {
        var normalized = photoPath.Trim();
        if (normalized.StartsWith('/'))
        {
            normalized = normalized[1..];
        }

        var sourcePath = Path.Combine(webHostEnvironment.WebRootPath, normalized.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(sourcePath))
        {
            return false;
        }

        File.Copy(sourcePath, destPath, overwrite: true);
        return true;
    }
}
