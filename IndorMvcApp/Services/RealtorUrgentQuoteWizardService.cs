using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorUrgentQuoteWizardService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IRealtorRegistrationService registration,
    IWebHostEnvironment env) : IRealtorUrgentQuoteWizardService
{
    private const string DraftIdSessionKey = "RealtorUrgentQuoteDraftId";
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileBytes = 10_000_000;
    private const int MaxPhotos = 3;

    public async Task<IndorRealtorUrgentQuoteDraft> EnsureDraftAsync(CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");

        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var draftId = session.GetInt32(DraftIdSessionKey);
        if (draftId is > 0)
        {
            var existing = await db.IndorRealtorUrgentQuoteDrafts
                .Include(d => d.Photos)
                .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                          d.Status == RealtorUrgentQuoteDraftStatuses.Draft, cancellationToken);
            if (existing != null)
            {
                return existing;
            }
        }

        var entity = new IndorRealtorUrgentQuoteDraft
        {
            RealtorId = realtor.Id,
            Status = RealtorUrgentQuoteDraftStatuses.Draft,
            CurrentStep = 1,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorUrgentQuoteDrafts.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(DraftIdSessionKey, entity.Id);
        return entity;
    }

    public async Task<IndorRealtorUrgentQuoteDraft?> GetDraftAsync(CancellationToken cancellationToken = default)
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

        return await db.IndorRealtorUrgentQuoteDrafts
            .Include(d => d.Photos)
            .FirstOrDefaultAsync(d => d.Id == draftId && d.RealtorId == realtor.Id &&
                                        d.Status == RealtorUrgentQuoteDraftStatuses.Draft, cancellationToken);
    }

    public async Task CancelDraftAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null)
        {
            httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
            return;
        }

        db.IndorRealtorUrgentQuoteDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);
    }

    public string ResolveResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Property",
        2 => "Issue",
        3 => "Photos",
        4 => "Send",
        _ => "Property"
    };

    public async Task<RealtorUrgentQuotePropertyViewModel> BuildPropertyAsync(string? search, CancellationToken cancellationToken = default)
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
                (p.CityRegion != null && p.CityRegion.Contains(term)));
        }

        var properties = await query
            .OrderByDescending(p => p.UpdatedUtc ?? p.FechaCreacion)
            .Take(20)
            .ToListAsync(cancellationToken);

        return new RealtorUrgentQuotePropertyViewModel
        {
            DisplayStep = 1,
            Title = "Urgente Quote",
            Subtitle = "For urgent closing needs, fill only the essentials: property, issue, urgency, and photos.",
            SearchQuery = search,
            SelectedPropertyFileId = draft.PropertyFileId,
            RequestCategory = draft.RequestCategory,
            ServiceType = draft.ServiceType,
            UrgencyLevel = draft.UrgencyLevel,
            Properties = properties.Select(p => new RealtorUrgentQuotePropertyOptionViewModel
            {
                Id = p.Id,
                DisplayAddress = FormatDisplayAddress(p.Address, p.CityRegion),
                SpecsLabel = FormatSpecs(p.Beds, p.Baths, p.SqFt),
                LocationLabel = FormatLocation(p.CityRegion, p.StateCode, p.PostalCode),
                PhotoUrl = p.PhotoUrl ?? "/welcome-house.png"
            }).ToList(),
            CategoryOptions = RealtorUrgentQuoteCategories.Options,
            ServiceTypes = RealtorUrgentQuoteServiceTypes.All,
            UrgencyOptions = RealtorUrgentQuoteUrgencyLevels.Options,
            States = registration.GetLicenseStates()
        };
    }

    public async Task<int> QuickAddPropertyAsync(
        string address, string city, string state, string zip, bool useForQuote,
        CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await EnsureDraftAsync(cancellationToken);

        var street = address?.Trim() ?? "";
        var cityValue = city?.Trim() ?? "";
        var stateValue = state?.Trim() ?? "";
        var zipValue = zip?.Trim() ?? "";

        if (street.Length == 0 || cityValue.Length == 0 || stateValue.Length == 0 || zipValue.Length == 0)
        {
            throw new InvalidOperationException("Street, city, state and ZIP are required.");
        }

        var property = new IndorRealtorPropertyFile
        {
            RealtorId = realtor.Id,
            Title = street,
            Address = street,
            CityRegion = cityValue,
            StateCode = stateValue,
            PostalCode = zipValue,
            Status = "Active",
            FilePhase = RealtorPropertyFilePhases.General,
            UpdatedUtc = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtorPropertyFiles.Add(property);
        await db.SaveChangesAsync(cancellationToken);

        if (useForQuote)
        {
            draft.PropertyFileId = property.Id;
            draft.Address = property.Address;
            draft.CityRegion = property.CityRegion;
            draft.ClientName = property.ClientName;
            draft.PhotoUrl = property.PhotoUrl;
            draft.Beds = property.Beds;
            draft.Baths = property.Baths;
            draft.SqFt = property.SqFt;
            draft.FechaActualizacion = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "file",
            Description = $"Created property {property.Address}",
            CategoryTag = "Properties",
            OccurredUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(cancellationToken);

        return property.Id;
    }

    public async Task SavePropertyAsync(
        int propertyFileId, string requestCategory, string serviceType, string urgencyLevel,
        CancellationToken cancellationToken = default)
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
        draft.Beds = property.Beds;
        draft.Baths = property.Baths;
        draft.SqFt = property.SqFt;
        draft.RequestCategory = ValidateCategory(requestCategory);
        draft.ServiceType = ValidateServiceType(serviceType);
        draft.UrgencyLevel = ValidateUrgency(urgencyLevel);
        draft.CurrentStep = 2;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorUrgentQuoteIssueViewModel> BuildIssueAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete property selection first.");

        return new RealtorUrgentQuoteIssueViewModel
        {
            DisplayStep = 2,
            Title = "Describe the Issue",
            Subtitle = "Choose the type of problem and the response speed needed.",
            Property = BuildSummary(draft),
            ServiceType = draft.ServiceType,
            UrgencyLevel = draft.UrgencyLevel,
            QuickDescription = draft.QuickDescription ?? "",
            RequestTypeTag = draft.RequestTypeTag,
            ServiceTypes = RealtorUrgentQuoteServiceTypes.All,
            UrgencyOptions = RealtorUrgentQuoteUrgencyLevels.Options,
            RequestTagOptions = RealtorUrgentQuoteRequestTags.Options
        };
    }

    public async Task SaveIssueAsync(
        string serviceType, string urgencyLevel, string quickDescription, string requestTypeTag,
        CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete property selection first.");

        draft.ServiceType = ValidateServiceType(serviceType);
        draft.UrgencyLevel = ValidateUrgency(urgencyLevel);
        draft.QuickDescription = string.IsNullOrWhiteSpace(quickDescription)
            ? null
            : quickDescription.Trim()[..Math.Min(quickDescription.Trim().Length, 200)];
        draft.RequestTypeTag = ValidateRequestTag(requestTypeTag);
        draft.CurrentStep = 3;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorUrgentQuotePhotosViewModel> BuildPhotosAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete issue details first.");

        return new RealtorUrgentQuotePhotosViewModel
        {
            DisplayStep = 3,
            Title = "Add Photos",
            Subtitle = "Add 1–3 photos if you have them. You can also skip this step for urgent requests.",
            Property = BuildSummary(draft),
            Photos = draft.Photos.OrderBy(p => p.SortOrder).Select(p => new RealtorUrgentQuotePhotoItemViewModel
            {
                Id = p.Id,
                FileUrl = p.FileUrl
            }).ToList(),
            OptionalNote = draft.OptionalNote ?? ""
        };
    }

    public async Task SavePhotosAsync(
        string? optionalNote, bool skipPhotos, IEnumerable<IFormFile>? photos,
        CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete issue details first.");

        draft.OptionalNote = string.IsNullOrWhiteSpace(optionalNote)
            ? null
            : optionalNote.Trim()[..Math.Min(optionalNote.Trim().Length, 250)];

        if (!skipPhotos && photos != null)
        {
            var photoList = photos.Where(f => f.Length > 0).ToList();
            var currentCount = draft.Photos.Count;
            foreach (var file in photoList.Take(MaxPhotos - currentCount))
            {
                if (file.Length == 0)
                {
                    continue;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext) || file.Length > MaxFileBytes)
                {
                    continue;
                }

                var folder = Path.Combine(env.WebRootPath, "uploads", "realtor-urgent-quotes", draft.Id.ToString());
                Directory.CreateDirectory(folder);
                var fileName = $"photo-{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);
                await using (var stream = File.Create(fullPath))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                db.IndorRealtorUrgentQuoteDraftPhotos.Add(new IndorRealtorUrgentQuoteDraftPhoto
                {
                    DraftId = draft.Id,
                    FileUrl = $"/uploads/realtor-urgent-quotes/{draft.Id}/{fileName}",
                    SortOrder = currentCount++,
                    UploadedUtc = DateTime.UtcNow
                });
            }
        }

        draft.CurrentStep = 4;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePhotoAsync(int photoId, CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken);
        if (draft == null)
        {
            return;
        }

        var photo = await db.IndorRealtorUrgentQuoteDraftPhotos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.DraftId == draft.Id, cancellationToken);
        if (photo != null)
        {
            db.IndorRealtorUrgentQuoteDraftPhotos.Remove(photo);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<RealtorUrgentQuoteSendViewModel> BuildSendAsync(CancellationToken cancellationToken = default)
    {
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Complete photos step first.");

        return new RealtorUrgentQuoteSendViewModel
        {
            DisplayStep = 4,
            Title = "Urgente Quote",
            Subtitle = "Review the essentials and send the request.",
            Property = BuildSummary(draft),
            PhotoCount = draft.Photos.Count,
            ProviderSelectionMode = draft.ProviderSelectionMode,
            SendPayload = draft.SendPayload,
            NotifyClient = draft.NotifyClient
        };
    }

    public async Task<RealtorUrgentQuoteSuccessViewModel> SendAsync(
        string providerSelectionMode, string sendPayload, bool notifyClient,
        CancellationToken cancellationToken = default)
    {
        var realtor = await registration.GetRealtorForCurrentUserAsync(cancellationToken)
            ?? throw new InvalidOperationException("Realtor profile not found.");
        var draft = await GetDraftAsync(cancellationToken)
            ?? throw new InvalidOperationException("Draft not found.");

        if (draft.PropertyFileId is not > 0)
        {
            throw new InvalidOperationException("Property is required.");
        }

        draft.ProviderSelectionMode = providerSelectionMode == RealtorUrgentQuoteProviderModes.Manual
            ? RealtorUrgentQuoteProviderModes.Manual
            : RealtorUrgentQuoteProviderModes.IndorAuto;
        draft.SendPayload = sendPayload == RealtorUrgentQuoteSendPayloads.FullPropertyFile
            ? RealtorUrgentQuoteSendPayloads.FullPropertyFile
            : RealtorUrgentQuoteSendPayloads.IssueOnly;
        draft.NotifyClient = notifyClient;

        var providers = await MatchProvidersAsync(draft.ServiceType, cancellationToken);
        var providerIds = providers.Take(3).Select(p => p.Id).ToList();
        var quoteCode = await GenerateQuoteCodeAsync(realtor.Id, cancellationToken);
        var urgencyLabel = FormatUrgencyLabel(draft.UrgencyLevel);
        var responseHours = draft.UrgencyLevel switch
        {
            RealtorUrgentQuoteUrgencyLevels.Emergency => 24,
            RealtorUrgentQuoteUrgencyLevels.Today => 24,
            _ => 48
        };

        var quote = new IndorRealtorQuote
        {
            RealtorId = realtor.Id,
            QuoteCode = quoteCode,
            Address = draft.Address ?? "",
            ServiceType = $"Urgent · {draft.ServiceType}",
            Status = "Pending",
            ClientName = draft.ClientName,
            PhotoUrl = draft.PhotoUrl,
            ProviderQuotesReceived = 0,
            FooterNote = $"Urgent · Response needed {urgencyLabel.ToLowerInvariant()}",
            RequestedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            PropertyFileId = draft.PropertyFileId,
            RequestType = draft.SendPayload == RealtorUrgentQuoteSendPayloads.FullPropertyFile
                ? RealtorQuoteRequestTypes.EntireFile
                : RealtorQuoteRequestTypes.ByItem,
            ResponseDeadlineHours = responseHours,
            ProviderSelectionMode = draft.ProviderSelectionMode == RealtorUrgentQuoteProviderModes.Manual
                ? RealtorQuoteProviderSelectionModes.Manual
                : RealtorQuoteProviderSelectionModes.IndorRecommended,
            OptionalMessage = BuildOptionalMessage(draft),
            SentUtc = DateTime.UtcNow
        };

        db.IndorRealtorQuotes.Add(quote);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var provider in providers.Take(providerIds.Count))
        {
            db.IndorRealtorQuoteSentProviders.Add(new IndorRealtorQuoteSentProvider
            {
                QuoteId = quote.Id,
                ProviderId = provider.Id,
                ProviderName = provider.CompanyName
            });
        }

        var property = await db.IndorRealtorPropertyFiles
            .FirstOrDefaultAsync(p => p.Id == draft.PropertyFileId && p.RealtorId == realtor.Id, cancellationToken);
        if (property != null)
        {
            property.QuotesReceivedCount += 1;
            property.UpdatedUtc = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(draft.QuickDescription))
            {
                db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
                {
                    PropertyFileId = property.Id,
                    CategoryType = RealtorPropertyFileCategoryTypes.RepairItems,
                    ItemLabel = draft.QuickDescription,
                    NoteText = $"Urgent · {urgencyLabel} · {FormatRequestTagLabel(draft.RequestTypeTag)}",
                    UploadedUtc = DateTime.UtcNow
                });
                property.RepairItemsCount += 1;
            }

            foreach (var photo in draft.Photos)
            {
                db.IndorRealtorPropertyFileItems.Add(new IndorRealtorPropertyFileItem
                {
                    PropertyFileId = property.Id,
                    CategoryType = RealtorPropertyFileCategoryTypes.PhotosVideos,
                    ItemLabel = "Urgent quote photo",
                    FileUrl = photo.FileUrl,
                    UploadedUtc = DateTime.UtcNow
                });
            }
        }

        db.IndorRealtorActivities.Add(new IndorRealtorActivity
        {
            RealtorId = realtor.Id,
            ActivityType = "quote",
            Description = $"Urgent {draft.ServiceType} quote {quoteCode} sent for {draft.Address}",
            CategoryTag = "Quotes",
            OccurredUtc = DateTime.UtcNow
        });

        draft.Status = RealtorUrgentQuoteDraftStatuses.Sent;
        draft.CurrentStep = 5;
        draft.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var result = new RealtorUrgentQuoteSuccessViewModel
        {
            QuoteCode = $"Quote #{quoteCode}",
            PropertyAddress = draft.Address ?? "",
            ServiceType = draft.ServiceType,
            UrgencyLabel = urgencyLabel,
            SentWhenLabel = $"Today, {DateTime.Now:h:mm tt}",
            ProviderCount = providerIds.Count
        };

        db.IndorRealtorUrgentQuoteDrafts.Remove(draft);
        await db.SaveChangesAsync(cancellationToken);
        httpContextAccessor.HttpContext?.Session.Remove(DraftIdSessionKey);

        return result;
    }

    private async Task<List<IndorRealtorQuoteProvider>> MatchProvidersAsync(string serviceType, CancellationToken cancellationToken)
    {
        var all = await db.IndorRealtorQuoteProviders.AsNoTracking()
            .Where(p => p.IsActive)
            .ToListAsync(cancellationToken);

        var keyword = serviceType.ToLowerInvariant();
        var matched = all
            .Where(p => p.Categories.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        p.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.IsRecommended)
            .ThenByDescending(p => p.Rating)
            .ToList();

        return matched.Count > 0 ? matched : all.OrderByDescending(p => p.Rating).Take(3).ToList();
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

    private static RealtorUrgentQuoteSummaryViewModel BuildSummary(IndorRealtorUrgentQuoteDraft draft) =>
        new()
        {
            DisplayAddress = FormatDisplayAddress(draft.Address ?? "", draft.CityRegion),
            SpecsLabel = FormatSpecs(draft.Beds, draft.Baths, draft.SqFt),
            PhotoUrl = draft.PhotoUrl ?? "/welcome-house.png",
            ServiceType = draft.ServiceType,
            UrgencyLabel = FormatUrgencyLabel(draft.UrgencyLevel),
            QuickDescription = draft.QuickDescription
        };

    private static string BuildOptionalMessage(IndorRealtorUrgentQuoteDraft draft)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(draft.QuickDescription))
        {
            parts.Add(draft.QuickDescription);
        }

        if (!string.IsNullOrWhiteSpace(draft.OptionalNote))
        {
            parts.Add(draft.OptionalNote);
        }

        parts.Add($"Request: {FormatRequestTagLabel(draft.RequestTypeTag)}");
        if (draft.NotifyClient)
        {
            parts.Add("Please notify client of updates.");
        }

        var msg = string.Join(" — ", parts);
        return msg.Length > 500 ? msg[..500] : msg;
    }

    private static string ValidateCategory(string value) =>
        RealtorUrgentQuoteCategories.Options.Any(o => o.Value == value)
            ? value
            : RealtorUrgentQuoteCategories.NeedQuoteToday;

    private static string ValidateServiceType(string value) =>
        RealtorUrgentQuoteServiceTypes.All.Contains(value) ? value : "HVAC";

    private static string ValidateUrgency(string value) =>
        RealtorUrgentQuoteUrgencyLevels.Options.Any(o => o.Value == value)
            ? value
            : RealtorUrgentQuoteUrgencyLevels.Today;

    private static string ValidateRequestTag(string value) =>
        RealtorUrgentQuoteRequestTags.Options.Any(o => o.Value == value)
            ? value
            : RealtorUrgentQuoteRequestTags.NeedQuote;

    private static string FormatUrgencyLabel(string value) =>
        RealtorUrgentQuoteUrgencyLevels.Options.FirstOrDefault(o => o.Value == value).Label ?? "Today";

    private static string FormatRequestTagLabel(string value) =>
        RealtorUrgentQuoteRequestTags.Options.FirstOrDefault(o => o.Value == value).Label ?? "Need quote";

    private static string FormatDisplayAddress(string address, string? cityRegion) =>
        string.IsNullOrWhiteSpace(cityRegion) ? address : $"{address}, {cityRegion}";

    private static string FormatLocation(string? cityRegion, string? state, string? zip)
    {
        var city = (cityRegion ?? "").Trim();
        var stateZip = string.Join(" ", new[] { (state ?? "").Trim(), (zip ?? "").Trim() }.Where(s => s.Length > 0));

        if (city.Length > 0 && stateZip.Length > 0)
        {
            return city.Contains(stateZip, StringComparison.OrdinalIgnoreCase) ? city : $"{city}, {stateZip}";
        }

        return city.Length > 0 ? city : stateZip;
    }

    private static string FormatSpecs(int? beds, decimal? baths, int? sqFt)
    {
        var parts = new List<string>();
        if (beds is > 0) parts.Add($"{beds} bed");
        if (baths is > 0) parts.Add($"{baths:0.#} bath");
        if (sqFt is > 0) parts.Add($"{sqFt:N0} sq ft");
        return string.Join(", ", parts);
    }
}
