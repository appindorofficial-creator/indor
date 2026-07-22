using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using IndorMvcApp.Data;
using IndorMvcApp.Localization;
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
    public const string FreshStartSessionKey = "NeighborRequestFreshStart";
    public const string NeededByDatePastErrorMessage = "Choose today or a future date.";
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

    public static IReadOnlyList<(string Value, string Label)> GetTimeWindowOptions() =>
        TimeWindowPresets.Select(p => (p.Value, p.Label)).ToList();

    public static DateTime MinimumNeededByDate => DateTime.Today;

    public static bool IsNeededByDateAllowed(DateTime? date) =>
        date == null || date.Value.Date >= MinimumNeededByDate;

    public static string? ValidateNeededByDate(DateTime? date) =>
        IsNeededByDateAllowed(date) ? null : NeededByDatePastErrorMessage;

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

    public void ClearDraft(ISession session)
    {
        session.Remove(DraftSessionKey);
        ClearFreshStart(session);
    }

    public void MarkFreshStart(ISession session) =>
        session.SetString(FreshStartSessionKey, "1");

    public bool IsFreshStart(ISession session) =>
        session.GetString(FreshStartSessionKey) == "1";

    public void ClearFreshStart(ISession session) => session.Remove(FreshStartSessionKey);

    public async Task<NeighborRequestDraftState> CreateNewDraftAsync(
        ISession session,
        Propiedad propiedad,
        int categoryId,
        CancellationToken ct)
    {
        var draft = new NeighborRequestDraftState
        {
            PropiedadId = propiedad.Id,
            CategoryId = categoryId,
            LocationAddress = await ResolveDefaultAddressAsync(propiedad, ct),
            AudienceCode = NeighborRequestAudienceCodes.Neighbors
        };
        SaveDraft(session, draft);
        ClearFreshStart(session);
        return draft;
    }

    public async Task<Propiedad?> ValidatePropiedadAsync(string userId, int propiedadId, CancellationToken ct) =>
        await db.Propiedades
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propiedadId && p.UserId == userId && p.Activo, ct);

    public async Task<string> ResolvePortalHomeUrlAsync(
        string userId,
        int propiedadId,
        IUrlHelper url,
        CancellationToken ct)
    {
        try
        {
            var isPropertyAdmin = await db.IndorPropertyAdministrators
                .AsNoTracking()
                .AnyAsync(a => a.UserId == userId
                    && a.RegistrationStatus == PropertyAdministratorRegistrationStatuses.Completed, ct);

            if (!isPropertyAdmin)
            {
                return url.Action("Index", "Home") ?? "/";
            }

            var portfolioPropertyId = await db.IndorPropertyAdminPortfolioProperties
                .AsNoTracking()
                .Where(p => p.PropiedadId == propiedadId)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync(ct);

            return portfolioPropertyId is > 0
                ? url.Action("Index", "Administrador", new { propertyId = portfolioPropertyId }) ?? "/Administrador"
                : url.Action("Index", "Administrador") ?? "/Administrador";
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return url.Action("Index", "Home") ?? "/";
        }
    }

    public async Task ApplyPortalHomeUrlsAsync(
        NeighborRequestWizardShellViewModel model,
        string userId,
        IUrlHelper url,
        CancellationToken ct)
    {
        var homeUrl = await ResolvePortalHomeUrlAsync(userId, model.PropiedadId, url, ct);
        model.CloseUrl = homeUrl;

        if (model.DisplayStep == 1 && !model.IsEditMode)
        {
            model.BackUrl = homeUrl;
        }
    }

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

    public Task<NeighborRequestCategoryStepViewModel> BuildCategoryStepAsync(
        int propiedadId,
        NeighborRequestDraftState? draft,
        IUrlHelper url,
        CancellationToken ct,
        bool useDraftFieldValues = true) =>
        BuildCategoryStepAsync(null, propiedadId, draft, url, ct, useDraftFieldValues);

    public async Task<NeighborRequestCategoryStepViewModel> BuildCategoryStepAsync(
        Propiedad? propiedad,
        int propiedadId,
        NeighborRequestDraftState? draft,
        IUrlHelper url,
        CancellationToken ct,
        bool useDraftFieldValues = true)
    {
        var categories = await LoadCategoriesAsync(ct);
        var defaultAddress = draft?.LocationAddress;
        if (string.IsNullOrWhiteSpace(defaultAddress))
        {
            propiedad ??= await db.Propiedades.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == propiedadId, ct);
            if (propiedad != null)
            {
                defaultAddress = await ResolveDefaultAddressAsync(propiedad, ct);
            }
        }

        var fieldDraft = useDraftFieldValues ? draft : null;

        return new NeighborRequestCategoryStepViewModel
        {
            PropiedadId = propiedadId,
            PageTitle = "Post Quick Job",
            DisplayStep = 1,
            TotalSteps = 5,
            StepLabels = ["Details", "Schedule", "Extras", "Review", "Helpers"],
            BackUrl = url.Action("Index", "Home"),
            CloseUrl = url.Action("Index", "Home")!,
            CategoryId = fieldDraft?.CategoryId ?? 0,
            SelectedCategoryId = fieldDraft?.CategoryId,
            Title = fieldDraft?.Title ?? string.Empty,
            Description = fieldDraft?.Description,
            LocationAddress = defaultAddress ?? string.Empty,
            UseHomeAddress = fieldDraft?.UseHomeAddress ?? true,
            ResumeDraft = useDraftFieldValues && draft is { CategoryId: > 0, EditingRequestId: null },
            Categories = categories.Select(c => new NeighborRequestCategoryOptionViewModel
            {
                Id = c.Id,
                Label = ResolveQuickJobCategoryLabel(c.Code, c.LabelEn),
                Description = ResolveQuickJobCategoryDescription(c.Code),
                IconClass = ResolveCategoryIcon(c.Code, c.IconClass),
                IllustrationClass = ResolveCategoryIllustration(c.Code),
                ImageUrl = ResolveQuickJobCategoryImage(c.Code)
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
            PageTitle = "Post Quick Job",
            DisplayStep = 3,
            TotalSteps = 5,
            StepLabels = ["Details", "Schedule", "Extras", "Review", "Helpers"],
            CategoryLabel = ResolveQuickJobCategoryLabel(category.Code, category.LabelEn),
            ExistingPhotoUrls = draft.PhotoPaths.ToList(),
            SelectedTools = draft.ToolsNeeded.ToList(),
            SpecialNotes = draft.SpecialNotes,
            PetsOnProperty = draft.PetsOnProperty,
            HasStairs = draft.HasStairs,
            GateCode = draft.GateCode,
            ParkingAvailable = draft.ParkingAvailable,
            ToolOptions = GetToolOptions(),
            BackUrl = $"/NeighborRequest/Preferences?propiedadId={draft.PropiedadId}",
            CloseUrl = "/Home/Index"
        };
    }

    public static IReadOnlyList<(string Value, string Label, string IconClass)> GetToolOptions() =>
    [
        ("gloves", "Gloves", "fa-mitten"),
        ("dolly", "Dolly", "fa-dolly"),
        ("tools", "Basic tools", "fa-screwdriver-wrench"),
        ("truck", "Pickup truck", "fa-truck-pickup"),
        ("none", "No tools needed", "fa-ban")
    ];

    public NeighborRequestPreferencesStepViewModel BuildPreferencesStep(NeighborRequestDraftState draft)
    {
        var isEdit = draft.EditingRequestId is > 0;
        return new NeighborRequestPreferencesStepViewModel
        {
            PropiedadId = draft.PropiedadId,
            RequestId = draft.EditingRequestId,
            IsEditMode = isEdit,
            PageTitle = isEdit ? "Edit request" : "Post Quick Job",
            DisplayStep = isEdit ? 2 : 2,
            TotalSteps = isEdit ? 3 : 5,
            StepLabels = isEdit
                ? ["Details", "Preferences", "Review"]
                : ["Details", "Schedule", "Extras", "Review", "Helpers"],
            WhenCode = isEdit || draft.ScheduleConfigured ? draft.TimelineCode : string.Empty,
            PreferredTimeCode = isEdit || draft.ScheduleConfigured ? draft.PreferredTimeCode : string.Empty,
            HelperCount = isEdit || draft.ScheduleConfigured ? draft.HelperCount : 0,
            DurationCode = isEdit || draft.ScheduleConfigured ? draft.DurationCode : string.Empty,
            PayTypeCode = isEdit || draft.ScheduleConfigured ? draft.PayTypeCode : string.Empty,
            TimelineCode = draft.TimelineCode,
            AudienceCode = draft.AudienceCode,
            SelectedAudiences = ExpandAudienceCode(draft.AudienceCode),
            BudgetAmount = draft.BudgetAmount,
            NeededByDate = draft.NeededByDate,
            BackUrl = isEdit && draft.EditingRequestId is > 0
                ? $"/NeighborRequest/Edit/{draft.EditingRequestId}"
                : $"/NeighborRequest/Category?propiedadId={draft.PropiedadId}",
            CloseUrl = isEdit && draft.EditingRequestId is > 0
                ? $"/NeighborRequest/Detail/{draft.EditingRequestId}"
                : "/Home/Index",
            WhenOptions =
            [
                (NeighborRequestTimelineCodes.Today, "Today", "fa-calendar-day"),
                (NeighborRequestTimelineCodes.Tomorrow, "Tomorrow", "fa-sun"),
                (NeighborRequestTimelineCodes.PickDate, "Pick a date", "fa-calendar")
            ],
            PreferredTimeOptions =
            [
                (NeighborRequestPreferredTimeCodes.Morning, "Morning", "fa-sun"),
                (NeighborRequestPreferredTimeCodes.Afternoon, "Afternoon", "fa-cloud-sun"),
                (NeighborRequestPreferredTimeCodes.Evening, "Evening", "fa-moon"),
                (NeighborRequestPreferredTimeCodes.Flexible, "Flexible", "fa-clock")
            ],
            HelperCountOptions =
            [
                (1, "1", "fa-user"),
                (2, "2", "fa-user-group"),
                (3, "3+", "fa-users")
            ],
            DurationOptions =
            [
                (NeighborRequestDurationCodes.OneHour, "1 hour", "fa-clock"),
                (NeighborRequestDurationCodes.TwoHours, "2 hours", "fa-clock"),
                (NeighborRequestDurationCodes.HalfDay, "Half day", "fa-sun"),
                (NeighborRequestDurationCodes.FullDay, "Full day", "fa-calendar-day")
            ],
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
                : ["Details", "Schedule", "Extras", "Review", "Helpers"],
            CategoryLabel = ResolveQuickJobCategoryLabel(category.Code, category.LabelEn),
            CategoryIconClass = category.IconClass,
            Title = draft.Title,
            DetailsSummary = string.IsNullOrWhiteSpace(draft.DetailsSummary) ? null : draft.DetailsSummary.Trim(),
            Description = draft.Description,
            PhotoUrls = draft.PhotoPaths.ToList(),
            LocationAddress = draft.LocationAddress,
            TimelineLabel = FormatTimelineLabel(draft.TimelineCode),
            AudienceLabel = FormatAudienceLabel(draft.AudienceCode),
            NeededByLabel = draft.NeededByDate?.ToString("dddd, MMMM d", CultureInfo.CurrentUICulture),
            TimeWindowLabel = FormatTimeWindowLabel(draft.TimeWindowStart, draft.TimeWindowEnd),
            BudgetLabel = draft.BudgetAmount is > 0
                ? string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}", draft.BudgetAmount.Value)
                : null,
            BackUrl = isEdit
                ? $"/NeighborRequest/Preferences?propiedadId={draft.PropiedadId}"
                : $"/NeighborRequest/Describe?propiedadId={draft.PropiedadId}",
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

        var categoryOptions = categories.Select(c => new NeighborRequestCategoryOptionViewModel
        {
            Id = c.Id,
            Label = ResolveQuickJobCategoryLabel(c.Code, c.LabelEn),
            Description = ResolveQuickJobCategoryDescription(c.Code),
            IconClass = ResolveCategoryIcon(c.Code, c.IconClass)
        }).ToList();

        if (category != null && categoryOptions.All(c => c.Id != category.Id))
        {
            categoryOptions.Insert(0, new NeighborRequestCategoryOptionViewModel
            {
                Id = category.Id,
                Label = ResolveQuickJobCategoryLabel(category.Code, category.LabelEn),
                Description = ResolveQuickJobCategoryDescription(category.Code),
                IconClass = ResolveCategoryIcon(category.Code, category.IconClass)
            });
        }

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
            CategoryLabel = ResolveQuickJobCategoryLabel(category?.Code ?? string.Empty, category?.LabelEn),
            CategoryIconClass = ResolveCategoryIcon(category?.Code ?? string.Empty, category?.IconClass),
            DetailsSummary = detailsSummary,
            Description = description,
            NeededByDate = request.NeededByDate,
            TimeWindowPreset = ResolveTimeWindowPresetValue(request.TimeWindowStart, request.TimeWindowEnd),
            AudienceCode = request.AudienceCode,
            Categories = categoryOptions,
            TimeWindowOptions = GetTimeWindowOptions()
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

        if (!IsNeededByDateAllowed(model.NeededByDate))
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

        if (!IsNeededByDateAllowed(draft.NeededByDate))
        {
            return null;
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var (lat, lng) = await ResolveCoordinatesAsync(propiedad, info, draft.LocationAddress, ct);

        var now = DateTime.UtcNow;
        FinalizeDraftBeforePublish(draft);

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
            EditingRequestId = request.Id,
            ScheduleConfigured = true
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

        List<IndorNeighborRequest> requests;
        try
        {
            var query = db.IndorNeighborRequests
                .AsNoTracking()
                .Include(r => r.Category)
                .Include(r => r.Offers)
                .Where(r => r.UserId == userId && r.IsActive);

            query = activeTab switch
            {
                "InProgress" => query.Where(r => r.Status == NeighborRequestStatuses.InProgress),
                "Completed" => query.Where(r => r.Status == NeighborRequestStatuses.Completed),
                _ => query.Where(r => r.Status != NeighborRequestStatuses.Completed
                                      && r.Status != NeighborRequestStatuses.Cancelled)
            };

            requests = await query
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
            HomeUrl = await ResolvePortalHomeUrlAsync(userId, propiedadId, url, ct),
            ActiveTab = activeTab,
            Items = requests.Select(r =>
            {
                var offerCount = r.Offers.Count(o => o.Status == NeighborRequestOfferStatuses.Pending);
                return new NeighborRequestListItemViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    CategoryLabel = ResolveCategoryDisplayLabel(r.Category),
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
            CategoryLabel = ResolveCategoryDisplayLabel(request.Category),
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
            RatingLabel = FormatRatingLabel(offer.Rating),
            MetaLabel = BuildOfferMetaLabel(offer),
            RoleLabel = isProvider ? "INDOR Provider" : "Neighbor",
            IsVerified = offer.IsVerified,
            IsProviderOffer = isProvider,
            DetailUrl = url.Action("Detail", "NeighborRequest", new { id = request.Id }) ?? "#",
            ViewUrl = url.Action("Offers", "NeighborRequest", new { id = request.Id }) ?? "#",
            MessageUrl = url.Action("Index", "Home") + "#section-more"
        };
    }

    public async Task<NeighborRequestBrowseHelpersViewModel?> BuildBrowseHelpersAsync(
        string userId,
        int propiedadId,
        IUrlHelper url,
        CancellationToken ct)
    {
        var propiedad = await ValidatePropiedadAsync(userId, propiedadId, ct);
        if (propiedad == null)
        {
            return null;
        }

        var locationAddress = await ResolveDefaultAddressAsync(propiedad, ct);
        var helpers = await LoadNearbyHelperCardsAsync(propiedad, locationAddress, url, ct);

        return new NeighborRequestBrowseHelpersViewModel
        {
            PropiedadId = propiedadId,
            HomeUrl = await ResolvePortalHomeUrlAsync(userId, propiedadId, url, ct),
            LocationAddress = locationAddress,
            RadiusLabel = "3 miles around your home",
            PostQuickJobUrl = url.Action("Create", "NeighborRequest", new { propiedadId }) ?? "#",
            Helpers = helpers
        };
    }

    public async Task<NeighborRequestHelpersStepViewModel?> BuildHelpersStepAsync(
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

        if (request == null)
        {
            return null;
        }

        var invitedProviderIds = await LoadInvitedProviderIdsAsync(requestId, ct);
        var helpers = await BuildHelperCardsForRequestAsync(request, invitedProviderIds, url, ct);
        var mapCenter = await ResolveMapCenterAsync(request.LocationAddress, ct);

        return new NeighborRequestHelpersStepViewModel
        {
            PropiedadId = request.PropiedadId,
            RequestId = requestId,
            PageTitle = "Helpers Nearby",
            DisplayStep = 5,
            TotalSteps = 5,
            StepLabels = ["Details", "Schedule", "Extras", "Review", "Helpers"],
            JobTitle = request.Title,
            WhenLabel = FormatWhenLabel(request),
            TimeLabel = FormatPreferredTimeLabel(request),
            HelpersLabel = $"{ExtractHelperCount(request.DetailsSummary)} helpers",
            PayLabel = FormatPayLabel(request.BudgetAmount, request.DetailsSummary?.Contains("Pay:", StringComparison.OrdinalIgnoreCase) == true && request.DetailsSummary.Contains("/hr", StringComparison.OrdinalIgnoreCase) ? NeighborRequestPayTypeCodes.Hourly : NeighborRequestPayTypeCodes.Hourly),
            LocationAddress = request.LocationAddress ?? string.Empty,
            CategoryIllustrationClass = ResolveCategoryIllustration(request.Category?.Code ?? string.Empty),
            Helpers = helpers,
            MapCenterLatitude = mapCenter?.Latitude,
            MapCenterLongitude = mapCenter?.Longitude,
            DetailUrl = url.Action("Detail", "NeighborRequest", new { id = requestId }) ?? "#",
            InviteUrl = url.Action("InviteHelpers", "NeighborRequest", new { id = requestId }) ?? "#",
            BackUrl = url.Action("Detail", "NeighborRequest", new { id = requestId }),
            CloseUrl = await ResolvePortalHomeUrlAsync(userId, request.PropiedadId, url, ct)
        };
    }

    public async Task<string?> InviteSelectedHelpersAsync(
        string userId,
        int requestId,
        IReadOnlyList<string>? selectedKeys,
        IUrlHelper url,
        CancellationToken ct)
    {
        IndorNeighborRequest? request;
        try
        {
            request = await db.IndorNeighborRequests
                .Include(r => r.Offers)
                .Include(r => r.Category)
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

        var invitedProviderIds = request.Offers
            .Where(o => o.OfferType == NeighborRequestOfferTypes.Provider
                && o.Status == NeighborRequestOfferStatuses.Pending
                && o.ProviderId != null)
            .Select(o => o.ProviderId!.Value)
            .ToHashSet();

        var helperCards = await BuildHelperCardsForRequestAsync(request, invitedProviderIds, url, ct);
        var selectedSet = new HashSet<string>(selectedKeys ?? [], StringComparer.Ordinal);
        var cardsToInvite = helperCards
            .Where(c => selectedSet.Contains(c.SelectionKey))
            .ToList();

        if (cardsToInvite.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var card in cardsToInvite)
            {
                if (invitedProviderIds.Contains(card.ProviderId))
                {
                    continue;
                }

                request.Offers.Add(new IndorNeighborRequestOffer
                {
                    RequestId = requestId,
                    OfferType = NeighborRequestOfferTypes.Provider,
                    ProviderId = card.ProviderId,
                    OffererName = card.Name,
                    OffererPhotoUrl = card.PhotoUrl,
                    PriceAmount = ParseHelperPriceAmount(card.PriceLabel),
                    Rating = decimal.TryParse(card.RatingLabel, NumberStyles.Number, CultureInfo.InvariantCulture, out var rating)
                        ? rating
                        : null,
                    ScheduleLabel = "Awaiting response",
                    IsVerified = card.IsVerified,
                    Status = NeighborRequestOfferStatuses.Pending,
                    CreatedUtc = now
                });
                invitedProviderIds.Add(card.ProviderId);
            }

            request.UpdatedUtc = now;
            await db.SaveChangesAsync(ct);
        }

        return url.Action("Detail", "NeighborRequest", new { id = requestId });
    }

    private async Task<HashSet<int>> LoadInvitedProviderIdsAsync(int requestId, CancellationToken ct)
    {
        try
        {
            var ids = await db.IndorNeighborRequestOffers
                .AsNoTracking()
                .Where(o => o.RequestId == requestId
                    && o.OfferType == NeighborRequestOfferTypes.Provider
                    && o.Status == NeighborRequestOfferStatuses.Pending
                    && o.ProviderId != null)
                .Select(o => o.ProviderId!.Value)
                .ToListAsync(ct);

            return ids.ToHashSet();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<List<NeighborRequestHelperCardViewModel>> BuildHelperCardsForRequestAsync(
        IndorNeighborRequest request,
        IReadOnlySet<int> invitedProviderIds,
        IUrlHelper url,
        CancellationToken ct)
    {
        var suggested = await LoadSuggestedProviderOffersAsync(request, url, ct);
        var mapCenter = await ResolveMapCenterAsync(request.LocationAddress, ct);
        var helpers = suggested.Select((offer, index) =>
        {
            var distanceMiles = 0.4m + (index * 0.3m);
            var (lat, lng) = OffsetMapPoint(mapCenter, distanceMiles, index, suggested.Count);
            return new NeighborRequestHelperCardViewModel
            {
                ProviderId = offer.ProviderId is > 0 ? offer.ProviderId.Value : index + 1,
                Name = offer.OffererName,
                PhotoUrl = offer.OffererPhotoUrl,
                AvatarIconClass = offer.AvatarIconClass,
                RatingLabel = offer.RatingLabel,
                ReviewCount = 0,
                DistanceLabel = $"{distanceMiles:0.#} mi away",
                PriceLabel = offer.PriceLabel?.Contains("/hr", StringComparison.OrdinalIgnoreCase) == true
                    ? offer.PriceLabel
                    : $"{offer.PriceLabel}/hr",
                MinHoursLabel = "Min. 2 hrs",
                SkillTags = ["Moving", "Furniture", "Heavy Lifting"],
                IsVerified = offer.IsVerified,
                MessageUrl = offer.MessageUrl,
                Latitude = lat,
                Longitude = lng
            };
        }).ToList();

        for (var i = 0; i < helpers.Count; i++)
        {
            var helper = helpers[i];
            helper.SelectionKey = FormatHelperSelectionKey(i, helper.ProviderId);
            helper.IsSelected = invitedProviderIds.Contains(helper.ProviderId);
        }

        return helpers;
    }

    private async Task<(double Latitude, double Longitude)?> ResolveMapCenterAsync(
        string? locationAddress,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(locationAddress))
        {
            return null;
        }

        var coords = await addressLookup.GeocodeAddressAsync(locationAddress.Trim(), ct);
        return coords is { } resolved
            ? ((double)resolved.Latitude, (double)resolved.Longitude)
            : null;
    }

    private static (double? Latitude, double? Longitude) OffsetMapPoint(
        (double Latitude, double Longitude)? center,
        decimal distanceMiles,
        int index,
        int total)
    {
        if (center is null)
        {
            return (null, null);
        }

        var angle = 2 * Math.PI * (index + 1) / Math.Max(total + 1, 2);
        var miles = (double)distanceMiles;
        const double milesPerDegreeLat = 69.0;
        var milesPerDegreeLng = Math.Max(Math.Cos(center.Value.Latitude * Math.PI / 180.0) * 69.0, 0.01);
        var lat = center.Value.Latitude + (miles * Math.Cos(angle) / milesPerDegreeLat);
        var lng = center.Value.Longitude + (miles * Math.Sin(angle) / milesPerDegreeLng);
        return (lat, lng);
    }

    private static string FormatHelperSelectionKey(int index, int providerId) => $"{index}:{providerId}";

    private static decimal? ParseHelperPriceAmount(string priceLabel)
    {
        if (string.IsNullOrWhiteSpace(priceLabel))
        {
            return null;
        }

        var digits = new string(priceLabel.Where(c => char.IsDigit(c) || c == '.').ToArray());
        return decimal.TryParse(digits, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) && amount > 0
            ? amount
            : null;
    }

    public void ApplyScheduleToDraft(NeighborRequestDraftState draft, NeighborRequestPreferencesStepViewModel model)
    {
        if (model.IsEditMode)
        {
            draft.TimelineCode = NormalizeTimelineCode(model.TimelineCode);
            return;
        }

        draft.TimelineCode = NormalizeWhenCode(model.WhenCode);
        draft.PreferredTimeCode = NormalizePreferredTimeCode(model.PreferredTimeCode);
        draft.HelperCount = model.HelperCount;
        draft.DurationCode = NormalizeDurationCode(model.DurationCode);
        draft.PayTypeCode = NormalizePayTypeCode(model.PayTypeCode);
        draft.BudgetAmount = model.BudgetAmount is > 0 ? model.BudgetAmount : null;
        draft.ScheduleConfigured = true;

        var today = DateTime.UtcNow.Date;
        draft.NeededByDate = draft.TimelineCode switch
        {
            NeighborRequestTimelineCodes.Today => today,
            NeighborRequestTimelineCodes.Tomorrow => today.AddDays(1),
            NeighborRequestTimelineCodes.PickDate => model.NeededByDate?.Date,
            _ => model.NeededByDate?.Date ?? today
        };

        if (!IsNeededByDateAllowed(draft.NeededByDate))
        {
            draft.NeededByDate = draft.TimelineCode == NeighborRequestTimelineCodes.PickDate
                ? null
                : today;
        }

        var (start, end) = ResolvePreferredTimeWindow(draft.PreferredTimeCode);
        draft.TimeWindowStart = start;
        draft.TimeWindowEnd = end;

        if (!model.IsEditMode)
        {
            draft.AudienceCode = NeighborRequestAudienceCodes.Both;
        }
    }

    public void ApplyExtrasToDraft(NeighborRequestDraftState draft, NeighborRequestDescribeStepViewModel model)
    {
        draft.SpecialNotes = model.SpecialNotes?.Trim();
        draft.PetsOnProperty = string.IsNullOrWhiteSpace(model.PetsOnProperty) ? null : model.PetsOnProperty.Trim();
        draft.HasStairs = string.IsNullOrWhiteSpace(model.HasStairs) ? null : model.HasStairs.Trim();
        draft.GateCode = model.GateCode?.Trim();
        draft.ParkingAvailable = string.IsNullOrWhiteSpace(model.ParkingAvailable) ? null : model.ParkingAvailable.Trim();
        draft.ToolsNeeded = model.SelectedTools?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList() ?? [];
    }

    private static void FinalizeDraftBeforePublish(NeighborRequestDraftState draft)
    {
        var parts = new List<string>();

        if (draft.ToolsNeeded.Count > 0)
        {
            var labels = draft.ToolsNeeded
                .Select(t => GetToolOptions().FirstOrDefault(o => o.Value == t).Label ?? t)
                .Where(l => !string.IsNullOrWhiteSpace(l));
            parts.Add($"Bring: {string.Join(", ", labels)}");
        }

        if (!string.IsNullOrWhiteSpace(draft.SpecialNotes))
        {
            parts.Add(draft.SpecialNotes.Trim());
        }

        var safety = new List<string>();
        if (!string.IsNullOrWhiteSpace(draft.PetsOnProperty))
        {
            safety.Add($"Pets: {draft.PetsOnProperty}");
        }

        if (!string.IsNullOrWhiteSpace(draft.HasStairs))
        {
            safety.Add($"Stairs: {draft.HasStairs}");
        }

        if (!string.IsNullOrWhiteSpace(draft.GateCode))
        {
            safety.Add($"Gate code: {draft.GateCode}");
        }

        if (!string.IsNullOrWhiteSpace(draft.ParkingAvailable))
        {
            safety.Add($"Parking: {draft.ParkingAvailable}");
        }

        if (safety.Count > 0)
        {
            parts.Add(string.Join(" | ", safety));
        }

        parts.Add($"Helpers: {Math.Max(1, draft.HelperCount)}");
        parts.Add($"Duration: {FormatDurationLabel(draft.DurationCode)}");
        parts.Add($"Pay: {FormatPayLabel(draft.BudgetAmount, draft.PayTypeCode)}");

        draft.DetailsSummary = string.Join(" · ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static int ExtractHelperCount(string? detailsSummary)
    {
        if (string.IsNullOrWhiteSpace(detailsSummary))
        {
            return 1;
        }

        var match = Regex.Match(detailsSummary, @"(?:Helpers|Ayudantes):\s*(\d+)", RegexOptions.IgnoreCase);
        return match.Success && int.TryParse(match.Groups[1].Value, out var count) ? count : 1;
    }

    private static string FormatPayLabel(decimal? amount, string? payTypeCode)
    {
        if (amount is not > 0)
        {
            return "$25/hr";
        }

        var formatted = string.Format(CultureInfo.GetCultureInfo("en-US"), "${0:0}", amount.Value);
        return string.Equals(payTypeCode, NeighborRequestPayTypeCodes.Fixed, StringComparison.OrdinalIgnoreCase)
            ? formatted
            : $"{formatted}/hr";
    }

    private static string FormatDurationLabel(string? code) =>
        code?.Trim().ToLowerInvariant() switch
        {
            NeighborRequestDurationCodes.OneHour => "1 hour",
            NeighborRequestDurationCodes.HalfDay => "Half day",
            NeighborRequestDurationCodes.FullDay => "Full day",
            _ => "2 hours"
        };

    private static string FormatWhenLabel(IndorNeighborRequest request)
    {
        if (request.NeededByDate is { } date)
        {
            var today = DateTime.UtcNow.Date;
            if (date.Date == today)
            {
                return "Today";
            }

            if (date.Date == today.AddDays(1))
            {
                return "Tomorrow";
            }

            return date.ToString("MMM d", CultureInfo.CurrentUICulture);
        }

        return FormatTimelineLabel(request.TimelineCode);
    }

    private static string FormatPreferredTimeLabel(IndorNeighborRequest request)
    {
        if (request.TimeWindowStart is { } start)
        {
            return start.Hour < 12 ? "Morning"
                : start.Hour < 17 ? "Afternoon"
                : "Evening";
        }

        return "Flexible";
    }

    private static (TimeOnly? Start, TimeOnly? End) ResolvePreferredTimeWindow(string? code) =>
        code?.Trim().ToLowerInvariant() switch
        {
            NeighborRequestPreferredTimeCodes.Morning => (new TimeOnly(9, 0), new TimeOnly(12, 0)),
            NeighborRequestPreferredTimeCodes.Afternoon => (new TimeOnly(12, 0), new TimeOnly(17, 0)),
            NeighborRequestPreferredTimeCodes.Evening => (new TimeOnly(17, 0), new TimeOnly(21, 0)),
            _ => (null, null)
        };

    private static string NormalizeWhenCode(string? code) =>
        code?.Trim() switch
        {
            NeighborRequestTimelineCodes.Tomorrow => NeighborRequestTimelineCodes.Tomorrow,
            NeighborRequestTimelineCodes.PickDate => NeighborRequestTimelineCodes.PickDate,
            NeighborRequestTimelineCodes.Asap => NeighborRequestTimelineCodes.Asap,
            NeighborRequestTimelineCodes.ThisWeek => NeighborRequestTimelineCodes.ThisWeek,
            NeighborRequestTimelineCodes.ThisMonth => NeighborRequestTimelineCodes.ThisMonth,
            NeighborRequestTimelineCodes.Flexible => NeighborRequestTimelineCodes.Flexible,
            _ => NeighborRequestTimelineCodes.Today
        };

    private static string NormalizeTimelineCode(string? code) =>
        code?.Trim() switch
        {
            NeighborRequestTimelineCodes.Asap => NeighborRequestTimelineCodes.Asap,
            NeighborRequestTimelineCodes.ThisMonth => NeighborRequestTimelineCodes.ThisMonth,
            NeighborRequestTimelineCodes.Flexible => NeighborRequestTimelineCodes.Flexible,
            _ => NeighborRequestTimelineCodes.ThisWeek
        };

    private static string NormalizePreferredTimeCode(string? code) =>
        code?.Trim().ToLowerInvariant() switch
        {
            NeighborRequestPreferredTimeCodes.Morning => NeighborRequestPreferredTimeCodes.Morning,
            NeighborRequestPreferredTimeCodes.Afternoon => NeighborRequestPreferredTimeCodes.Afternoon,
            NeighborRequestPreferredTimeCodes.Evening => NeighborRequestPreferredTimeCodes.Evening,
            _ => NeighborRequestPreferredTimeCodes.Flexible
        };

    private static string NormalizeDurationCode(string? code) =>
        code?.Trim().ToLowerInvariant() switch
        {
            NeighborRequestDurationCodes.OneHour => NeighborRequestDurationCodes.OneHour,
            NeighborRequestDurationCodes.HalfDay => NeighborRequestDurationCodes.HalfDay,
            NeighborRequestDurationCodes.FullDay => NeighborRequestDurationCodes.FullDay,
            _ => NeighborRequestDurationCodes.TwoHours
        };

    private static string NormalizePayTypeCode(string? code) =>
        string.Equals(code, NeighborRequestPayTypeCodes.Fixed, StringComparison.OrdinalIgnoreCase)
            ? NeighborRequestPayTypeCodes.Fixed
            : NeighborRequestPayTypeCodes.Hourly;

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
                PriceLabel = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}/hr", price),
                ScheduleLabel = BuildSuggestedScheduleLabel(request, i),
                RatingLabel = FormatRatingLabel(null),
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

    // Only surfaces a rating when a real value exists. Providers without any
    // rating history return null so the UI hides the rating instead of showing
    // fabricated numbers (App Store guideline 2.1(a)).
    private static string? FormatRatingLabel(decimal? rating) =>
        rating is > 0
            ? rating.Value.ToString("0.0", CultureInfo.InvariantCulture)
            : null;

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

    public async Task InitializeBlankDraftAsync(
        ISession session,
        Propiedad propiedad,
        CancellationToken ct)
    {
        SaveDraft(session, new NeighborRequestDraftState
        {
            PropiedadId = propiedad.Id,
            LocationAddress = await ResolveDefaultAddressAsync(propiedad, ct),
            AudienceCode = NeighborRequestAudienceCodes.Neighbors
        });
    }

    public async Task EnsureDraftAsync(
        ISession session,
        Propiedad propiedad,
        CancellationToken ct)
    {
        if (IsFreshStart(session))
        {
            SaveDraft(session, new NeighborRequestDraftState
            {
                PropiedadId = propiedad.Id,
                LocationAddress = await ResolveDefaultAddressAsync(propiedad, ct)
            });
            return;
        }

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
            NeighborRequestTimelineCodes.Today => "Today",
            NeighborRequestTimelineCodes.Tomorrow => "Tomorrow",
            NeighborRequestTimelineCodes.PickDate => "Pick a date",
            _ => "This week"
        };

    private static string ResolveCategoryDisplayLabel(IndorNeighborRequestCategory? category)
    {
        if (category == null)
        {
            return "Request";
        }

        if (UiCulture.IsSpanish(CultureInfo.CurrentUICulture.Name)
            && !string.IsNullOrWhiteSpace(category.LabelEs))
        {
            return category.LabelEs.Trim();
        }

        return ResolveQuickJobCategoryLabel(category.Code ?? string.Empty, category.LabelEn);
    }

    private static string ResolveQuickJobCategoryLabel(string code, string? labelEn) =>
        code.Trim().ToLowerInvariant() switch
        {
            "moving-hauling" => "Moving furniture",
            "yard-patio" => "Yard work",
            "cleaning" => "Cleaning help",
            "home-improvements" => "General labor",
            "tech-internet" => "Carry boxes",
            "other" => "Junk removal",
            "painting" => "Painting help",
            "fence" => "Fence help",
            "assembly" => "Assembly help",
            "outdoor-cleanup" => "Outdoor cleanup",
            _ => ResolveCategoryLabel(code, labelEn)
        };

    private static string ResolveQuickJobCategoryDescription(string code) =>
        code.Trim().ToLowerInvariant() switch
        {
            "home-improvements" => "Help with simple tasks around the house",
            "yard-patio" => "Mowing, trimming, leaves and cleanup",
            "cleaning" => "Home, patio, garage or deep cleaning",
            "moving-hauling" => "Move heavy furniture and items",
            "tech-internet" => "Loading, unloading or lifting boxes",
            "other" => "Remove trash, clutter and unwanted items",
            "painting" => "Help painting walls, fences and touch-ups",
            "fence" => "Fence cleanup, staining or basic repairs",
            "assembly" => "Assemble shelves, furniture and simple items",
            "outdoor-cleanup" => "Help with debris, branches and hauling",
            _ => "Pick the help you need"
        };

    private static string ResolveQuickJobCategoryImage(string code) =>
        "/images/quickjob/" + code.Trim().ToLowerInvariant() switch
        {
            "home-improvements" => "qj-general-labor.png",
            "yard-patio" => "qj-yard-work.png",
            "cleaning" => "qj-cleaning-help.png",
            "moving-hauling" => "qj-moving-furniture.png",
            "tech-internet" => "qj-carry-boxes.png",
            "other" => "qj-junk-removal.png",
            "painting" => "qj-painting-help.png",
            "fence" => "qj-fence-help.png",
            "assembly" => "qj-assembly-help.png",
            "outdoor-cleanup" => "qj-outdoor-cleanup.png",
            _ => "qj-general-labor.png"
        };

    private static string ResolveCategoryIllustration(string code) =>
        code.Trim().ToLowerInvariant() switch
        {
            "moving-hauling" => "nr-cat-ill--moving",
            "yard-patio" => "nr-cat-ill--yard",
            "cleaning" => "nr-cat-ill--cleaning",
            "home-improvements" => "nr-cat-ill--labor",
            "tech-internet" => "nr-cat-ill--boxes",
            "other" => "nr-cat-ill--junk",
            _ => "nr-cat-ill--other"
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
            "painting" => "Painting Help",
            "fence" => "Fence Help",
            "assembly" => "Assembly Help",
            "outdoor-cleanup" => "Outdoor Cleanup",
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
            "painting" => "Walls, fences and touch-ups",
            "fence" => "Cleanup, staining or repairs",
            "assembly" => "Shelves, furniture and items",
            "outdoor-cleanup" => "Debris, branches and hauling",
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
            "painting" => "fa-roller-coaster",
            "fence" => "fa-border-all",
            "assembly" => "fa-screwdriver-wrench",
            "outdoor-cleanup" => "fa-trowel",
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
        var permanentDir = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "neighbor-requests", request.Id.ToString());
        Directory.CreateDirectory(permanentDir);

        db.IndorNeighborRequestPhotos.RemoveRange(request.Photos);

        if (draft.PhotoPaths.Count == 0)
        {
            await db.SaveChangesAsync(ct);
            return;
        }

        var sort = 0;
        foreach (var photoPath in draft.PhotoPaths.Take(MaxPhotos))
        {
            var extension = Path.GetExtension(photoPath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            var destFileName = $"photo-{++sort}{extension}";
            var destWebPath = $"/uploads/neighbor-requests/{request.Id}/{destFileName}";
            var destPath = Path.Combine(permanentDir, destFileName);

            if (IsRequestPhotoAlreadyAtDestination(photoPath, request.Id, destFileName))
            {
                db.IndorNeighborRequestPhotos.Add(new IndorNeighborRequestPhoto
                {
                    RequestId = request.Id,
                    FilePath = NormalizeRequestPhotoWebPath(photoPath),
                    SortOrder = sort,
                    CreatedUtc = now
                });
                continue;
            }

            if (!TryCopyDraftPhoto(photoPath, destPath))
            {
                continue;
            }

            db.IndorNeighborRequestPhotos.Add(new IndorNeighborRequestPhoto
            {
                RequestId = request.Id,
                FilePath = destWebPath,
                SortOrder = sort,
                CreatedUtc = now
            });
        }

        await db.SaveChangesAsync(ct);
    }

    private static bool IsRequestPhotoAlreadyAtDestination(string photoPath, int requestId, string destFileName)
    {
        var normalized = NormalizeRequestPhotoWebPath(photoPath).TrimStart('/');
        var expected = $"uploads/neighbor-requests/{requestId}/{destFileName}";
        return normalized.Equals(expected, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRequestPhotoWebPath(string photoPath)
    {
        var normalized = photoPath.Trim().Replace('\\', '/');
        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        return normalized;
    }

    private string? ResolvePhysicalPhotoPath(string photoPath)
    {
        var normalized = photoPath.Trim().Replace('\\', '/');
        if (normalized.StartsWith('/'))
        {
            normalized = normalized[1..];
        }

        if (normalized.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized["wwwroot/".Length..];
        }

        return Path.Combine(
            webHostEnvironment.WebRootPath,
            normalized.Replace('/', Path.DirectorySeparatorChar));
    }

    private bool TryCopyDraftPhoto(string photoPath, string destPath)
    {
        var sourcePath = ResolvePhysicalPhotoPath(photoPath);
        if (sourcePath == null || !File.Exists(sourcePath))
        {
            return false;
        }

        var fullSource = Path.GetFullPath(sourcePath);
        var fullDest = Path.GetFullPath(destPath);

        if (string.Equals(fullSource, fullDest, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(fullDest)!);

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                CopyPhotoFile(fullSource, fullDest);
                return true;
            }
            catch (IOException) when (attempt < 2)
            {
                Thread.Sleep(50 * (attempt + 1));
            }
            catch (IOException)
            {
                return false;
            }
        }

        return false;
    }

    private static void CopyPhotoFile(string sourcePath, string destPath)
    {
        using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
        source.CopyTo(dest);
    }

    private async Task<List<NeighborRequestHelperCardViewModel>> LoadNearbyHelperCardsAsync(
        Propiedad propiedad,
        string locationAddress,
        IUrlHelper url,
        CancellationToken ct)
    {
        const double radiusMiles = 3d;
        if (string.IsNullOrWhiteSpace(locationAddress))
        {
            return [];
        }

        var coords = await addressLookup.GeocodeAddressAsync(locationAddress.Trim(), ct);
        if (coords is not { } resolved)
        {
            return [];
        }

        var centerLat = (double)resolved.Latitude;
        var centerLng = (double)resolved.Longitude;

        var activeStatuses = new[]
        {
            ProviderRegistrationStatuses.IndorProActive,
            ProviderRegistrationStatuses.Approved,
            ProviderRegistrationStatuses.PendingReview
        };

        List<IndorProveedor> providers;
        try
        {
            providers = await db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Where(p => activeStatuses.Contains(p.RegistrationStatus))
                .Where(p => p.BusinessAddress != null || p.PrimaryCity != null)
                .OrderByDescending(p => p.FechaActualizacion)
                .Take(80)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        Dictionary<string, string> categoryLabels;
        try
        {
            categoryLabels = await db.IndorProveedorCategoriasCatalogo
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Id, c => c.LabelEn, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            categoryLabels = new Dictionary<string, string>();
        }

        var icons = new[] { "fa-house", "fa-leaf", "fa-broom", "fa-wrench" };
        var messageUrl = url.Action("Index", "Home") + "#section-more";
        var results = new List<(NeighborRequestHelperCardViewModel Card, double Distance)>();
        var geocodeBudget = 6;

        foreach (var provider in providers)
        {
            if (provider.Latitude is null || provider.Longitude is null)
            {
                if (geocodeBudget <= 0)
                {
                    continue;
                }

                var tracked = await db.IndorProveedores.FirstOrDefaultAsync(p => p.Id == provider.Id, ct);
                if (tracked == null)
                {
                    continue;
                }

                await ProviderGeolocationHelper.ApplyGeocodeAsync(tracked, addressLookup, ct);
                geocodeBudget--;
                if (tracked.Latitude is null || tracked.Longitude is null)
                {
                    continue;
                }

                await db.SaveChangesAsync(ct);
                provider.Latitude = tracked.Latitude;
                provider.Longitude = tracked.Longitude;
            }

            var lat = (double)provider.Latitude!.Value;
            var lng = (double)provider.Longitude!.Value;
            var distance = CalculateDistanceMiles(centerLat, centerLng, lat, lng);
            if (distance > (decimal)radiusMiles)
            {
                continue;
            }

            var categoryId = provider.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
            categoryLabels.TryGetValue(categoryId ?? "", out var categoryLabel);

            var name = !string.IsNullOrWhiteSpace(provider.DbaName)
                ? provider.DbaName.Trim()
                : provider.BusinessName?.Trim() ?? "INDOR Provider";
            var isVerified = string.Equals(provider.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
                || string.Equals(provider.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);
            var basePrice = 120m + (provider.Id % 5) * 5m;

            results.Add((new NeighborRequestHelperCardViewModel
            {
                ProviderId = provider.Id,
                Name = name,
                AvatarIconClass = icons[provider.Id % icons.Length],
                RatingLabel = FormatRatingLabel(null),
                ReviewCount = 0,
                DistanceLabel = $"{distance:0.#} mi away",
                PriceLabel = string.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C0}/hr", basePrice),
                MinHoursLabel = "Min. 2 hrs",
                SkillTags = BuildProviderSkillTags(categoryLabel),
                IsVerified = isVerified,
                MessageUrl = messageUrl,
                Latitude = lat,
                Longitude = lng
            }, (double)distance));
        }

        return results
            .OrderBy(r => r.Distance)
            .Select(r => r.Card)
            .Take(20)
            .ToList();
    }

    private static List<string> BuildProviderSkillTags(string? categoryLabel) =>
        !string.IsNullOrWhiteSpace(categoryLabel)
            ? [categoryLabel, "Local help"]
            : ["General labor", "Home help"];

    private static decimal CalculateDistanceMiles(double lat1, double lng1, double lat2, double lng2)
    {
        const double earthRadiusMiles = 3958.8;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLng = (lng2 - lng1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
            * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(earthRadiusMiles * c);
    }
}
