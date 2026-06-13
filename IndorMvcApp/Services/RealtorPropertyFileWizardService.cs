using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorPropertyFileWizardService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IRealtorRegistrationService registration) : IRealtorPropertyFileWizardService
{
    private const string DraftIdSessionKey = "RealtorPropertyFileDraftId";

    public async Task<IndorRealtorPropertyFileDraft> EnsureDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var draftId = session.GetInt32(DraftIdSessionKey);
        if (draftId is > 0)
        {
            var existing = await db.IndorRealtorPropertyFileDrafts
                .Include(d => d.Categories)
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                          d.Status == RealtorPropertyFileDraftStatuses.Draft, cancellationToken);
            if (existing != null)
            {
                return existing;
            }
        }

        var entity = new IndorRealtorPropertyFileDraft
        {
            RealtorId = realtor.Id,
            Status = RealtorPropertyFileDraftStatuses.Draft,
            CurrentStep = 1,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorPropertyFileDrafts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(DraftIdSessionKey, entity.Id);
        return entity;
    }

    public async Task<IndorRealtorPropertyFileDraft?> GetDraftAsync(CancellationToken cancellationToken = default)
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

        return await db.IndorRealtorPropertyFileDrafts
            .Include(d => d.Categories)
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                        d.Status == RealtorPropertyFileDraftStatuses.Draft, cancellationToken);
    }

    public async Task CancelDraftAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null)
        {
            httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
            return;
        }

        db.IndorRealtorPropertyFileDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
    }

    public string ResolveResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Details",
        2 => "AddItems",
        3 => "AddContent",
        4 => "Review",
        _ => "Details"
    };

    public async Task<RealtorPropertyFileDetailsViewModel> BuildDetailsAsync(string? search, CancellationToken cancellationToken = default)
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

        return new RealtorPropertyFileDetailsViewModel
        {
            DisplayStep = 1,
            Subtitle = "Start a file for pre-closing, repair review, transfer, or homeowner records.",
            SearchQuery = search,
            SelectedPropertyId = draft.SourcePropertyId,
            FilePhase = draft.FilePhase ?? RealtorPropertyFilePhases.PreClosing,
            Properties = properties.Select(MapPropertyPicker).ToList(),
            FilePhaseOptions = RealtorPropertyFilePhases.Options
        };
    }

    public async Task SaveDetailsAsync(int sourcePropertyId, string filePhase, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await EnsureDraftAsync(cancellationToken);

        var property = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == sourcePropertyId && p.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Property not found.");

        draft.SourcePropertyId = property.Id;
        draft.Title = property.Title;
        draft.Address = property.Address;
        draft.CityRegion = property.CityRegion;
        draft.ClientName = property.ClientName;
        draft.PhotoUrl = property.PhotoUrl;
        draft.FilePhase = string.IsNullOrWhiteSpace(filePhase) ? RealtorPropertyFilePhases.PreClosing : filePhase.Trim();
        draft.CurrentStep = 2;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorPropertyFileAddItemsViewModel> BuildAddItemsAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete details first.");
        if (draft.SourcePropertyId is not > 0)
        {
            throw new InvalidOperationException("Select a property first.");
        }

        var selected = draft.Categories.Select(c => c.CategoryType).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new RealtorPropertyFileAddItemsViewModel
        {
            DisplayStep = 2,
            Subtitle = "Choose the items you want to add to this file",
            PropertyDisplay = FormatPropertyDisplay(draft),
            FilePhaseLabel = FormatFilePhaseLabel(draft.FilePhase),
            PhotoUrl = draft.PhotoUrl,
            Categories = RealtorPropertyFileCategoryTypes.All.Select(cat =>
                new RealtorPropertyFileCategoryOptionViewModel
                {
                    CategoryType = cat.Type,
                    Label = cat.Label,
                    Description = cat.Description,
                    Icon = cat.Icon,
                    Selected = selected.Contains(cat.Type)
                }).ToList()
        };
    }

    public async Task SaveAddItemsAsync(IEnumerable<string> categoryTypes, CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        var types = categoryTypes?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    ?? [];

        if (types.Count == 0)
        {
            throw new InvalidOperationException("Select at least one item type.");
        }

        var valid = RealtorPropertyFileCategoryTypes.All.Select(c => c.Type).ToHashSet(StringComparer.OrdinalIgnoreCase);
        types = types.Where(t => valid.Contains(t)).ToList();

        var existing = await db.IndorRealtorPropertyFileDraftCategories
            .Where(c => c.DraftId == draft.Id)
            .ToListAsync(cancellationToken);
        db.IndorRealtorPropertyFileDraftCategories.RemoveRange(existing);

        foreach (var type in types)
        {
            db.IndorRealtorPropertyFileDraftCategories.Add(new IndorRealtorPropertyFileDraftCategory
            {
                DraftId = draft.Id,
                CategoryType = type
            });
        }

        draft.CurrentStep = 3;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorPropertyFileContentViewModel> BuildAddContentAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete previous steps first.");

        await db.Entry(draft).Collection(d => d.Categories).LoadAsync(cancellationToken);
        await db.Entry(draft).Collection(d => d.Items).LoadAsync(cancellationToken);

        var sections = draft.Categories
            .Select(c => RealtorPropertyFileCategoryTypes.All.FirstOrDefault(a =>
                a.Type.Equals(c.CategoryType, StringComparison.OrdinalIgnoreCase)))
            .Where(m => !string.IsNullOrEmpty(m.Type))
            .Select(m => new RealtorPropertyFileContentSectionViewModel
            {
                CategoryType = m.Type,
                Label = m.Label,
                NoteText = m.Type == RealtorPropertyFileCategoryTypes.NotesDocuments ? draft.NoteText : null,
                Items = draft.Items
                    .Where(i => i.CategoryType.Equals(m.Type, StringComparison.OrdinalIgnoreCase))
                    .Select(MapItemCard)
                    .ToList()
            }).ToList();

        return new RealtorPropertyFileContentViewModel
        {
            DisplayStep = 2,
            Title = "Add File Content",
            Subtitle = "Upload and organize the items for this property file",
            PropertyDisplay = FormatPropertyDisplay(draft),
            FilePhaseLabel = FormatFilePhaseLabel(draft.FilePhase),
            Sections = sections
        };
    }

    public async Task SaveDraftItemAsync(
        string categoryType, string itemLabel, string? fileUrl, long? fileSizeBytes,
        string? noteText, DateTime? expirationUtc, CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        db.IndorRealtorPropertyFileDraftItems.Add(new IndorRealtorPropertyFileDraftItem
        {
            DraftId = draft.Id,
            CategoryType = categoryType,
            ItemLabel = itemLabel,
            FileUrl = fileUrl,
            FileSizeBytes = fileSizeBytes,
            NoteText = noteText,
            ExpirationUtc = expirationUtc
        });
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteAddContentAsync(string? noteText, CancellationToken cancellationToken = default)
    {
        var draft = await EnsureDraftAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(noteText))
        {
            draft.NoteText = noteText.Trim().Length > 1000 ? noteText.Trim()[..1000] : noteText.Trim();
            if (draft.Items.All(i => i.CategoryType != RealtorPropertyFileCategoryTypes.NotesDocuments) &&
                draft.Categories.Any(c => c.CategoryType == RealtorPropertyFileCategoryTypes.NotesDocuments))
            {
                db.IndorRealtorPropertyFileDraftItems.Add(new IndorRealtorPropertyFileDraftItem
                {
                    DraftId = draft.Id,
                    CategoryType = RealtorPropertyFileCategoryTypes.NotesDocuments,
                    ItemLabel = "Note",
                    NoteText = draft.NoteText
                });
            }
        }

        draft.CurrentStep = 4;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorPropertyFileReviewViewModel> BuildReviewAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("No draft found.");

        await db.Entry(draft).Collection(d => d.Categories).LoadAsync(cancellationToken);
        await db.Entry(draft).Collection(d => d.Items).LoadAsync(cancellationToken);

        var included = draft.Categories
            .Select(c =>
            {
                var meta = RealtorPropertyFileCategoryTypes.All.First(a =>
                    a.Type.Equals(c.CategoryType, StringComparison.OrdinalIgnoreCase));
                var count = draft.Items.Count(i =>
                    i.CategoryType.Equals(c.CategoryType, StringComparison.OrdinalIgnoreCase));
                if (c.CategoryType == RealtorPropertyFileCategoryTypes.NotesDocuments &&
                    !string.IsNullOrWhiteSpace(draft.NoteText) && count == 0)
                {
                    count = 1;
                }

                return new RealtorPropertyFileReviewItemViewModel
                {
                    Label = meta.Label,
                    Icon = meta.Icon,
                    CountLabel = count == 1 ? "1 item" : $"{count} items"
                };
            }).ToList();

        return new RealtorPropertyFileReviewViewModel
        {
            DisplayStep = 3,
            Subtitle = "Confirm the file details before creating it",
            PropertyDisplay = FormatPropertyDisplay(draft),
            ClientName = draft.ClientName ?? "",
            FilePhaseLabel = FormatFilePhaseLabel(draft.FilePhase),
            PhotoUrl = draft.PhotoUrl,
            IncludedItems = included,
            CreateAndContinueLater = draft.CreateAndContinueLater
        };
    }

    public async Task<int> CreateFileAsync(bool createAndContinueLater, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("No draft found.");
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        await db.Entry(draft).Collection(d => d.Categories).LoadAsync(cancellationToken);
        await db.Entry(draft).Collection(d => d.Items).LoadAsync(cancellationToken);

        var repairCount = draft.Items.Count(i =>
            i.CategoryType == RealtorPropertyFileCategoryTypes.RepairItems);
        var quoteCount = draft.Categories.Any(c =>
            c.CategoryType == RealtorPropertyFileCategoryTypes.QuotesEstimates) ? 0 : 0;

        var file = new IndorRealtorPropertyFile
        {
            RealtorId = realtor.Id,
            Title = draft.Title ?? draft.Address ?? "Property File",
            Address = draft.Address ?? "",
            CityRegion = draft.CityRegion,
            ClientName = draft.ClientName,
            PhotoUrl = draft.PhotoUrl,
            Status = "Active",
            FilePhase = draft.FilePhase,
            RepairItemsCount = repairCount,
            QuotesReceivedCount = quoteCount,
            UpdatedUtc = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorPropertyFiles.Add(file);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var item in draft.Items)
        {
            db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
            {
                PropertyFileId = file.Id,
                CategoryType = item.CategoryType,
                ItemLabel = item.ItemLabel,
                FileUrl = item.FileUrl,
                NoteText = item.NoteText,
                FileSizeBytes = item.FileSizeBytes,
                ExpirationUtc = item.ExpirationUtc,
                UploadedUtc = item.UploadedUtc
            });
        }

        if (!string.IsNullOrWhiteSpace(draft.NoteText) &&
            !draft.Items.Any(i => i.CategoryType == RealtorPropertyFileCategoryTypes.NotesDocuments))
        {
            db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
            {
                PropertyFileId = file.Id,
                CategoryType = RealtorPropertyFileCategoryTypes.NotesDocuments,
                ItemLabel = "Note",
                NoteText = draft.NoteText,
                UploadedUtc = DateTime.UtcNow
            });
        }

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "upload",
            Description = $"{FormatFilePhaseLabel(draft.FilePhase)} created for {draft.Address}",
            CategoryTag = "Files",
            OccurredUtc = DateTime.UtcNow
        });

        draft.Status = RealtorPropertyFileDraftStatuses.Created;
        draft.CreateAndContinueLater = createAndContinueLater;
        draft.CurrentStep = 5;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        db.IndorRealtorPropertyFileDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);

        return file.Id;
    }

    public async Task<RealtorPropertyFileSuccessViewModel> BuildSuccessAsync(int propertyFileId, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var file = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .Include(f => f.Items)
            .FirstAsync(f => f.Id == propertyFileId && f.RealtorId == realtor.Id, cancellationToken);

        var parts = file.Items
            .GroupBy(i => i.CategoryType)
            .Select(g =>
            {
                var meta = RealtorPropertyFileCategoryTypes.All.FirstOrDefault(a =>
                    a.Type.Equals(g.Key, StringComparison.OrdinalIgnoreCase));
                var count = g.Count();
                var shortLabel = meta.Label switch
                {
                    var l when l.Contains("Photos") => $"{count} Photo{(count == 1 ? "" : "s")}",
                    var l when l.Contains("Inspection") => $"{count} Inspection Report{(count == 1 ? "" : "s")}",
                    var l when l.Contains("Warrant") => $"{count} Warrant{(count == 1 ? "y" : "ies")}",
                    var l when l.Contains("Notes") => $"{count} Note{(count == 1 ? "" : "s")}/Document{(count == 1 ? "" : "s")}",
                    _ => $"{count} {meta.Label}"
                };
                return shortLabel;
            }).ToList();

        return new RealtorPropertyFileSuccessViewModel
        {
            PropertyFileId = file.Id,
            FilePhaseLabel = FormatFilePhaseLabel(file.FilePhase),
            PropertyDisplay = string.IsNullOrWhiteSpace(file.CityRegion)
                ? file.Address
                : $"{file.Address}, {file.CityRegion}",
            ClientName = file.ClientName ?? "",
            AddedNowLabel = parts.Count > 0 ? string.Join(", ", parts) : "No items added yet",
            StatusLabel = file.Status
        };
    }

    public async Task<RealtorPropertyFileViewViewModel> BuildViewAsync(int propertyFileId, CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var file = await db.IndorRealtorPropertyFiles.AsNoTracking()
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == propertyFileId && f.RealtorId == realtor.Id, cancellationToken)
            ?? throw new InvalidOperationException("Property file not found.");

        var (badge, css) = DeriveFileStatus(file);
        var (primaryLabel, primaryUrl, secondaryLabel, secondaryUrl) = DeriveFileActions(file);
        var firstName = (realtor.DisplayName ?? "Realtor").Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                        ?? "Realtor";

        var sections = file.Items
            .GroupBy(i => i.CategoryType)
            .Select(g =>
            {
                var meta = RealtorPropertyFileCategoryTypes.All.FirstOrDefault(a =>
                    a.Type.Equals(g.Key, StringComparison.OrdinalIgnoreCase));
                return new RealtorPropertyFileViewSectionViewModel
                {
                    Label = meta.Label ?? g.Key,
                    Icon = meta.Icon ?? "fa-folder",
                    Items = g.OrderByDescending(i => i.UploadedUtc).Select(i => new RealtorPropertyFileViewItemViewModel
                    {
                        ItemLabel = i.ItemLabel,
                        FileUrl = i.FileUrl,
                        NoteText = i.NoteText,
                        MetaLabel = i.FileSizeBytes.HasValue
                            ? FormatFileSize(i.FileSizeBytes.Value)
                            : i.ExpirationUtc?.ToLocalTime().ToString("Expires MMM d, yyyy")
                    }).ToList()
                };
            })
            .Where(s => s.Items.Count > 0)
            .OrderBy(s => s.Label)
            .ToList();

        return new RealtorPropertyFileViewViewModel
        {
            DisplayName = firstName,
            FullDisplayName = realtor.DisplayName ?? firstName,
            ProfilePhotoUrl = string.IsNullOrWhiteSpace(realtor.ProfilePhotoUrl) ? null : realtor.ProfilePhotoUrl,
            BadgeLabel = realtor.RegistrationStatus == RealtorRegistrationStatuses.Verified
                ? "Verified Realtor"
                : "Realtor Basic",
            IsVerified = realtor.RegistrationStatus == RealtorRegistrationStatuses.Verified,
            PropertyFileId = file.Id,
            FileCode = $"PF-{file.Id}",
            Address = file.Address,
            CityRegion = file.CityRegion ?? "",
            PhotoUrl = string.IsNullOrWhiteSpace(file.PhotoUrl) ? "/welcome-house.png" : file.PhotoUrl,
            ClientName = file.ClientName ?? "",
            FilePhaseLabel = FormatFilePhaseLabel(file.FilePhase),
            StatusBadge = badge,
            StatusCss = css,
            RepairItemsCount = file.RepairItemsCount,
            QuotesReceivedCount = file.QuotesReceivedCount,
            UpdatedLabel = $"Last updated {FormatRelativeTime(file.UpdatedUtc ?? file.FechaCreacion)}",
            PrimaryActionLabel = primaryLabel,
            PrimaryActionUrl = primaryUrl,
            SecondaryActionLabel = secondaryLabel,
            SecondaryActionUrl = secondaryUrl,
            Sections = sections
        };
    }

    private static RealtorPropertyPickerViewModel MapPropertyPicker(IndorRealtorPropertyFile p) =>
        new()
        {
            Id = p.Id,
            Address = p.Address,
            CityRegion = p.CityRegion ?? "",
            ClientName = p.ClientName ?? "",
            PhotoUrl = string.IsNullOrWhiteSpace(p.PhotoUrl) ? "/welcome-house.png" : p.PhotoUrl,
            DisplayAddress = string.IsNullOrWhiteSpace(p.CityRegion)
                ? p.Address
                : $"{p.Address}, {p.CityRegion}"
        };

    private static RealtorPropertyFileItemCardViewModel MapItemCard(IndorRealtorPropertyFileDraftItem item) =>
        new()
        {
            Id = item.Id,
            ItemLabel = item.ItemLabel,
            FileUrl = item.FileUrl,
            SizeLabel = item.FileSizeBytes.HasValue ? FormatFileSize(item.FileSizeBytes.Value) : null,
            ExpirationLabel = item.ExpirationUtc?.ToLocalTime().ToString("MMM d, yyyy"),
            IsImage = !string.IsNullOrWhiteSpace(item.FileUrl) &&
                      (item.FileUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                       item.FileUrl.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                       item.FileUrl.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                       item.FileUrl.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
        };

    private static string FormatPropertyDisplay(IndorRealtorPropertyFileDraft draft) =>
        string.IsNullOrWhiteSpace(draft.CityRegion)
            ? draft.Address ?? ""
            : $"{draft.Address}, {draft.CityRegion}";

    private static string FormatFilePhaseLabel(string? phase) =>
        string.IsNullOrWhiteSpace(phase) ? "Property File" : $"{phase} File";

    private static string FormatFileSize(long bytes) =>
        bytes >= 1_000_000 ? $"{bytes / 1_000_000.0:0.#} MB" : $"{bytes / 1_000.0:0.#} KB";

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

    private static (string Badge, string Css) DeriveFileStatus(IndorRealtorPropertyFile file)
    {
        if (file.FilePhase == "Transfer")
        {
            return ("Shared Package", "shared");
        }

        if (file.QuotesReceivedCount > 0)
        {
            return ("Quotes Pending", "quotes");
        }

        if (file.RepairItemsCount > 0 || file.FilePhase == "Repair Review")
        {
            return ("Inspection Uploaded", "inspection");
        }

        if (file.Status == "Archived")
        {
            return ("Closed", "closed");
        }

        return ("Active", "active");
    }

    private static (string Label, string Url, string? SecondaryLabel, string? SecondaryUrl) DeriveFileActions(
        IndorRealtorPropertyFile file)
    {
        var id = file.Id;
        var viewUrl = $"/RealtorPropertyFile/View?id={id}";

        if (file.FilePhase == "Transfer")
        {
            return ("View Package", viewUrl, null, null);
        }

        if (file.RepairItemsCount > 0 && file.QuotesReceivedCount == 0)
        {
            return (
                "Request Quotes",
                $"/RealtorQuoteRequest/Start?propertyFileId={id}",
                "View Inspection",
                viewUrl);
        }

        if (file.RepairItemsCount == 0 && file.FilePhase == "Pre-Closing")
        {
            return (
                "Upload Report",
                $"/RealtorInspectionUpload/Upload?propertyFileId={id}",
                null,
                null);
        }

        if (file.QuotesReceivedCount > 0)
        {
            return (
                "View Quotes",
                "/Realtor/Quotes",
                "Open File",
                viewUrl);
        }

        return ("Open File", viewUrl, null, null);
    }
}
