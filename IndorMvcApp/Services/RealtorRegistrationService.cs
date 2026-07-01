using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Validation;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorRegistrationService(
    AppDbContext db,
    IHttpContextAccessor httpContextAccessor,
    UserManager<ApplicationUser> userManager) : IRealtorRegistrationService
{
    private const string RealtorIdSessionKey = "RealtorRegistroId";

    private static readonly string[] UsStates =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
        "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
        "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
        "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
        "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC"
    ];

    private static readonly IReadOnlyList<string> SupportedLanguagesList = RealtorEditProfileOptions.SupportedLanguages;

    public IReadOnlyList<string> GetLicenseStates() => UsStates;

    public IReadOnlyList<string> GetSupportedLanguages() => SupportedLanguagesList;

    public async Task<RealtorRegistrationState> GetAsync(CancellationToken cancellationToken = default)
    {
        var realtorId = await ResolveRealtorIdAsync(cancellationToken);
        if (realtorId is not > 0)
        {
            return new RealtorRegistrationState();
        }

        var entity = await db.IndorRealtors
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == realtorId, cancellationToken);

        return entity == null ? new RealtorRegistrationState() : MapFromEntity(entity);
    }

    public async Task SaveProfileAsync(RealtorRegistrationState state, CancellationToken cancellationToken = default)
    {
        var realtorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorRealtors.FirstAsync(r => r.Id == realtorId, cancellationToken);

        entity.BrokerageName = state.BrokerageName.Trim();
        entity.LicenseNumber = state.LicenseNumber.Trim();
        entity.LicenseState = state.LicenseState.Trim();
        entity.ServiceAreas = state.ServiceAreas.Trim();
        entity.OfficeAddress = state.OfficeAddress.Trim();
        entity.OfficeCity = state.OfficeCity.Trim();
        entity.OfficeState = state.OfficeState.Trim();
        entity.OfficeZip = state.OfficeZip.Trim();

        if (!RealtorSupportedLanguages.TryNormalize(state.Languages, out var normalizedLanguages, out var languagesError))
        {
            throw new InvalidOperationException(languagesError ?? "Select at least one language.");
        }

        entity.LanguagesJson = RealtorSupportedLanguages.SerializeJson(normalizedLanguages);
        entity.ProfessionalTermsAccepted = state.ProfessionalTermsAccepted;
        entity.TermsAcceptedUtc = state.ProfessionalTermsAccepted ? DateTime.UtcNow : entity.TermsAcceptedUtc;
        entity.CurrentStep = 2;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        await EnsureDocumentSlotsAsync(cancellationToken);
    }

    public async Task LinkCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var realtorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorRealtors.FirstAsync(r => r.Id == realtorId, cancellationToken);
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return;
        }

        entity.UserId = userId;
        entity.Email ??= user.Email;
        entity.DisplayName = $"{user.Nombre} {user.Apellidos}".Trim();
        if (string.IsNullOrWhiteSpace(entity.DisplayName))
        {
            entity.DisplayName = user.Email ?? "Realtor";
        }

        entity.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureDocumentSlotsAsync(CancellationToken cancellationToken = default)
    {
        var realtorId = await EnsureDraftAsync(cancellationToken);
        var existing = await db.IndorRealtorDocumentos
            .Where(d => d.RealtorId == realtorId)
            .Select(d => d.DocumentType)
            .ToListAsync(cancellationToken);

        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var (type, _, _) in RealtorDocumentTypes.Slots)
        {
            if (existingSet.Contains(type))
            {
                continue;
            }

            db.IndorRealtorDocumentos.Add(new IndorRealtorDocumento
            {
                RealtorId = realtorId,
                DocumentType = type
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RealtorDocumentSlotViewModel>> GetDocumentSlotsAsync(CancellationToken cancellationToken = default)
    {
        var realtorId = await ResolveRealtorIdAsync(cancellationToken);
        if (realtorId is not > 0)
        {
            return [];
        }

        var rows = await db.IndorRealtorDocumentos
            .AsNoTracking()
            .Where(d => d.RealtorId == realtorId)
            .ToListAsync(cancellationToken);

        return RealtorDocumentTypes.Slots.Select(slot =>
        {
            var row = rows.FirstOrDefault(r =>
                r.DocumentType.Equals(slot.Type, StringComparison.OrdinalIgnoreCase));
            var uploaded = !string.IsNullOrWhiteSpace(row?.FileUrl);
            return new RealtorDocumentSlotViewModel
            {
                DocumentType = slot.Type,
                Label = slot.Label,
                Required = slot.Required,
                Uploaded = uploaded,
                FileUrl = row?.FileUrl
            };
        }).ToList();
    }

    public async Task RegisterDocumentUploadAsync(string documentType, string relativeUrl, CancellationToken cancellationToken = default)
    {
        var realtorId = await EnsureDraftAsync(cancellationToken);
        await EnsureDocumentSlotsAsync(cancellationToken);

        var doc = await db.IndorRealtorDocumentos.FirstOrDefaultAsync(
            d => d.RealtorId == realtorId && d.DocumentType == documentType,
            cancellationToken);

        if (doc == null)
        {
            doc = new IndorRealtorDocumento
            {
                RealtorId = realtorId,
                DocumentType = documentType
            };
            db.IndorRealtorDocumentos.Add(doc);
        }

        doc.FileUrl = relativeUrl;
        doc.UploadedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> ClearDocumentAsync(string documentType, CancellationToken cancellationToken = default)
    {
        var realtorId = await EnsureDraftAsync(cancellationToken);

        var doc = await db.IndorRealtorDocumentos.FirstOrDefaultAsync(
            d => d.RealtorId == realtorId && d.DocumentType == documentType,
            cancellationToken);

        if (doc == null || string.IsNullOrWhiteSpace(doc.FileUrl))
        {
            return null;
        }

        var previousUrl = doc.FileUrl;
        doc.FileUrl = null;
        doc.UploadedUtc = null;
        await db.SaveChangesAsync(cancellationToken);
        return previousUrl;
    }

    public async Task CompleteVerificationAsync(bool skipped, CancellationToken cancellationToken = default)
    {
        var realtorId = await EnsureDraftAsync(cancellationToken);
        var entity = await db.IndorRealtors
            .Include(r => r.Documentos)
            .FirstAsync(r => r.Id == realtorId, cancellationToken);

        entity.VerificationSkipped = skipped;
        entity.CurrentStep = 4;

        var hasLicensePhoto = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto && !string.IsNullOrWhiteSpace(d.FileUrl));
        var hasGovId = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId && !string.IsNullOrWhiteSpace(d.FileUrl));

        entity.RegistrationStatus = !skipped && hasLicensePhoto && hasGovId
            ? RealtorRegistrationStatuses.Verified
            : RealtorRegistrationStatuses.Basic;
        entity.ProfileCompletedUtc = DateTime.UtcNow;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RealtorReadyViewModel> GetReadyViewModelAsync(CancellationToken cancellationToken = default)
    {
        var realtorId = await ResolveRealtorIdAsync(cancellationToken);
        if (realtorId is not > 0)
        {
            return new RealtorReadyViewModel();
        }

        var entity = await db.IndorRealtors
            .AsNoTracking()
            .Include(r => r.Documentos)
            .FirstAsync(r => r.Id == realtorId, cancellationToken);

        var licensePhoto = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.LicensePhoto && !string.IsNullOrWhiteSpace(d.FileUrl));
        var govId = entity.Documentos.Any(d =>
            d.DocumentType == RealtorDocumentTypes.GovernmentId && !string.IsNullOrWhiteSpace(d.FileUrl));

        return new RealtorReadyViewModel
        {
            BadgeLabel = entity.RegistrationStatus == RealtorRegistrationStatuses.Verified
                ? "Verified Realtor"
                : "Realtor Basic",
            LicenseNumberSaved = !string.IsNullOrWhiteSpace(entity.LicenseNumber),
            ProfileCreated = entity.ProfileCompletedUtc.HasValue,
            LicensePhotoUploaded = licensePhoto,
            GovernmentIdUploaded = govId,
            CanUpgradeToVerified = entity.RegistrationStatus != RealtorRegistrationStatuses.Verified
        };
    }

    public async Task<IndorRealtor?> GetRealtorForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await db.IndorRealtors
            .AsNoTracking()
            .Include(r => r.Documentos)
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
    }

    public string ResolveWizardResumeAction(int currentStep) => currentStep switch
    {
        <= 1 => "Profile",
        2 => "Verification",
        3 => "Ready",
        _ => "Dashboard"
    };

    public bool IsRegistrationComplete(IndorRealtor realtor) =>
        realtor.RegistrationStatus is RealtorRegistrationStatuses.Basic or RealtorRegistrationStatuses.Verified
        && realtor.ProfileCompletedUtc.HasValue;

    private async Task<int> EnsureDraftAsync(CancellationToken cancellationToken)
    {
        var session = httpContextAccessor.HttpContext?.Session
            ?? throw new InvalidOperationException("Session is not available.");

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        var id = session.GetInt32(RealtorIdSessionKey);
        if (id is > 0)
        {
            var exists = await db.IndorRealtors.AnyAsync(r => r.Id == id, cancellationToken);
            if (exists)
            {
                return id.Value;
            }
        }

        if (!string.IsNullOrEmpty(userId))
        {
            var byUser = await db.IndorRealtors.FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
            if (byUser != null)
            {
                session.SetInt32(RealtorIdSessionKey, byUser.Id);
                return byUser.Id;
            }
        }

        var entity = new IndorRealtor
        {
            UserId = userId,
            RegistrationStatus = RealtorRegistrationStatuses.Draft,
            CurrentStep = 1,
            FechaCreacion = DateTime.UtcNow
        };

        db.IndorRealtors.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        session.SetInt32(RealtorIdSessionKey, entity.Id);
        return entity.Id;
    }

    private async Task<int?> ResolveRealtorIdAsync(CancellationToken cancellationToken)
    {
        var session = httpContextAccessor.HttpContext?.Session;
        var id = session?.GetInt32(RealtorIdSessionKey);
        if (id is > 0)
        {
            return id;
        }

        var userId = userManager.GetUserId(httpContextAccessor.HttpContext!.User);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        var realtor = await db.IndorRealtors.AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
        if (realtor != null)
        {
            session?.SetInt32(RealtorIdSessionKey, realtor.Id);
            return realtor.Id;
        }

        return null;
    }

    private static RealtorRegistrationState MapFromEntity(IndorRealtor entity) =>
        new()
        {
            BrokerageName = entity.BrokerageName ?? "",
            LicenseNumber = entity.LicenseNumber ?? "",
            LicenseState = entity.LicenseState ?? "",
            ServiceAreas = entity.ServiceAreas ?? "",
            OfficeAddress = entity.OfficeAddress ?? "",
            OfficeCity = entity.OfficeCity ?? "",
            OfficeState = entity.OfficeState ?? "",
            OfficeZip = entity.OfficeZip ?? "",
            Languages = FormatLanguages(entity.LanguagesJson),
            ProfessionalTermsAccepted = entity.ProfessionalTermsAccepted,
            VerificationSkipped = entity.VerificationSkipped,
            DisplayName = entity.DisplayName ?? "",
            Email = entity.Email ?? ""
        };

    private static string FormatLanguages(string? languagesJson)
    {
        if (string.IsNullOrWhiteSpace(languagesJson))
        {
            return "";
        }

        try
        {
            var languages = System.Text.Json.JsonSerializer.Deserialize<List<string>>(languagesJson);
            return languages == null ? "" : string.Join(", ", languages.Where(l => !string.IsNullOrWhiteSpace(l)));
        }
        catch
        {
            return "";
        }
    }

    private static string SerializeLanguages(string languages) =>
        RealtorSupportedLanguages.SerializeJson(
            RealtorSupportedLanguages.TryNormalize(languages, out var normalized, out _)
                ? normalized
                : string.Empty);
}
