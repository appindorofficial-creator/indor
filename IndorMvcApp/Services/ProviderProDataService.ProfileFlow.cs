using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public partial class ProviderProDataService
{
    public async Task<ProviderProfileCompletionViewModel> GetProfileCompletionAsync(
        IndorProveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        var loaded = await LoadProveedorForProfileFlowAsync(proveedor.Id, cancellationToken);
        if (loaded == null)
        {
            return new ProviderProfileCompletionViewModel { CompanyName = ResolveCompanyName(proveedor) };
        }

        var sections = BuildProfileSections(loaded);
        var completed = sections.Count(s => s.IsComplete);
        var next = sections.FirstOrDefault(s => !s.IsComplete);

        return new ProviderProfileCompletionViewModel
        {
            CompanyName = ResolveCompanyName(loaded),
            DisplayBusinessName = ResolvePublicBusinessName(loaded),
            CompanyInitial = BuildCompanyInitial(ResolveCompanyName(loaded)),
            LogoUrl = ResolveProviderLogoUrl(loaded),
            LocationLabel = FormatCityState(loaded.PrimaryCity),
            ServiceAreaSummary = BuildServiceAreaSummary(loaded),
            CompletedSections = completed,
            Sections = sections,
            ContinueAction = next?.Action,
            ContinueLabel = next == null ? null : ProviderProDisplayLocalization.L("Continue Setup"),
            NextStepTitle = next?.Title,
            NextStepAction = next?.Action
        };
    }

    public async Task<ProviderProfileBusinessViewModel> GetProfileBusinessAsync(
        IndorProveedor proveedor,
        ProviderProfileBusinessInput? input = null,
        CancellationToken cancellationToken = default)
    {
        var loaded = await LoadProveedorForProfileFlowAsync(proveedor.Id, cancellationToken);
        if (loaded == null)
        {
            loaded = proveedor;
        }

        var meta = ReadOnboardingMeta(loaded.OnboardingMetaJson);
        var servicesVm = await GetEditProfileServicesAsync(loaded, cancellationToken);
        var categoryRows = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.SortOrder)
            .Select(c => new { c.Id, c.LabelEn, c.LabelEs })
            .ToListAsync(cancellationToken);

        var categories = categoryRows
            .Select(c => new ProviderProfileCategoryOptionViewModel
            {
                Id = c.Id,
                Label = ProviderProDisplayLocalization.CatalogLabel(c.LabelEn, c.LabelEs)
            })
            .ToList();

        if (categories.Count == 0)
        {
            categories = OnboardingCatalog.ProviderCategories
                .Select(c => new ProviderProfileCategoryOptionViewModel
                {
                    Id = c.Id,
                    Label = ProviderProDisplayLocalization.L(c.Label)
                })
                .ToList();
        }

        var primaryCategory = loaded.Categorias.Select(c => c.CategoriaId).FirstOrDefault()
            ?? input?.PrimaryCategoryId;

        return new ProviderProfileBusinessViewModel
        {
            CompanyName = ResolveCompanyName(loaded),
            CompanyInitial = BuildCompanyInitial(ResolveCompanyName(loaded)),
            LogoUrl = ResolveProviderLogoUrl(loaded),
            BusinessName = input?.BusinessName?.Trim() ?? loaded.BusinessName ?? loaded.DbaName ?? "",
            PrimaryCategoryId = input?.PrimaryCategoryId ?? primaryCategory,
            Phone = input?.Phone?.Trim() ?? loaded.Phone ?? "",
            Email = input?.Email?.Trim() ?? loaded.Email ?? "",
            Website = (input?.WebsiteNotApplicable ?? false) ? null : (input?.Website?.Trim() ?? meta.Website),
            WebsiteNotApplicable = input?.WebsiteNotApplicable ?? string.IsNullOrWhiteSpace(meta.Website),
            ServiceDescription = input?.ServiceDescription?.Trim() ?? loaded.ServiceDescription ?? "",
            PreferredHours = input?.PreferredHours?.Trim() ?? loaded.PreferredHours ?? "",
            EmergencyService = ParseTriStatePreference(input?.EmergencyPreference, loaded.EmergencyService),
            SameDayJobs = ParseTriStatePreference(input?.SameDayPreference, loaded.SameDayJobs),
            EmergencyPreference = input?.EmergencyPreference ?? TriStatePreference(loaded.EmergencyService),
            SameDayPreference = input?.SameDayPreference ?? TriStatePreference(loaded.SameDayJobs),
            PrimaryCity = input?.PrimaryCity?.Trim() ?? loaded.PrimaryCity ?? "",
            ServiceZipCodes = input?.ServiceZipCodes?.Trim() ?? FormatZipNeighborhoods(loaded.ZipNeighborhoodsJson),
            ServiceOptions = servicesVm.Options,
            CategoryOptions = categories
        };
    }

    public async Task<bool> SaveProfileBusinessAsync(
        int proveedorId,
        ProviderProfileBusinessInput input,
        CancellationToken cancellationToken = default)
    {
        await ProviderDatabaseSchemaInitializer.EnsureEditProfileColumnsAsync(db, logger, cancellationToken);

        var entity = await db.IndorProveedores
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        entity.BusinessName = TrimOrEmpty(input.BusinessName);
        entity.Phone = TrimOrEmpty(input.Phone);
        entity.Email = TrimOrEmpty(input.Email);
        entity.ServiceDescription = TrimOrEmpty(input.ServiceDescription);
        entity.PreferredHours = TrimOrEmpty(input.PreferredHours);
        entity.PrimaryCity = TrimOrEmpty(input.PrimaryCity);
        entity.ZipNeighborhoodsJson = ParseZipNeighborhoodsJson(input.ServiceZipCodes);
        entity.EmergencyService = ParseTriStatePreference(input.EmergencyPreference, entity.EmergencyService);
        entity.SameDayJobs = ParseTriStatePreference(input.SameDayPreference, entity.SameDayJobs);
        entity.FechaActualizacion = DateTime.UtcNow;

        UpdateOnboardingMeta(entity, meta =>
        {
            meta.Website = input.WebsiteNotApplicable ? null : TrimOrEmpty(input.Website);
        });

        if (!string.IsNullOrWhiteSpace(input.PrimaryCategoryId))
        {
            SyncCategoriesOnEntity(entity, [input.PrimaryCategoryId]);
        }

        if (input.ServiceIds is { Length: > 0 })
        {
            await SaveEditProfileServicesAsync(proveedorId, input.ServiceIds, cancellationToken);
        }

        try
        {
            await ProviderGeolocationHelper.ApplyGeocodeAsync(entity, addressLookup, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Geocoding skipped for provider {ProviderId} during profile business save.", proveedorId);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to save profile business for provider {ProviderId}.", proveedorId);
            return false;
        }
    }

    public async Task<ProviderProfileDocumentsViewModel> GetProfileDocumentsAsync(
        IndorProveedor proveedor,
        string? expandSection = null,
        CancellationToken cancellationToken = default)
    {
        var loaded = await LoadProveedorForProfileFlowAsync(proveedor.Id, cancellationToken) ?? proveedor;
        var meta = ReadOnboardingMeta(loaded.OnboardingMetaJson);
        var docMeta = meta.ProfileDocuments;
        var docs = loaded.Documentos;

        bool HasDoc(string type) =>
            docs.Any(d => string.Equals(d.DocumentType, type, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl));

        string? GetDocUrl(string type) =>
            docs.FirstOrDefault(d => string.Equals(d.DocumentType, type, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl))?.FileUrl;

        var sections = new List<ProviderProfileDocumentSectionViewModel>
        {
            BuildDocumentSection(
                "license", ProviderProDisplayLocalization.L("License"), ProviderProDisplayLocalization.L("Upload or verify your business license"), "fa-id-card",
                ProviderDocumentTypes.License,
                IsLicenseComplete(loaded, docMeta, HasDoc(ProviderDocumentTypes.License)),
                IsLicensePending(loaded, docMeta, HasDoc(ProviderDocumentTypes.License)),
                expandSection, GetDocUrl(ProviderDocumentTypes.License), HasDoc(ProviderDocumentTypes.License),
                docMeta.LicenseNotApplicable, docMeta.LicenseUnknown,
                new Dictionary<string, string?>
                {
                    ["LicenseNumber"] = docMeta.LicenseNumber ?? loaded.LicenseNumber,
                    ["LicenseType"] = docMeta.LicenseType,
                    ["LicenseState"] = docMeta.LicenseState,
                    ["LicenseExpiry"] = docMeta.LicenseExpiry
                }),
            BuildDocumentSection(
                "insurance", ProviderProDisplayLocalization.L("Insurance & COI"), ProviderProDisplayLocalization.L("Add proof of insurance and COI"), "fa-shield-halved",
                ProviderDocumentTypes.Insurance,
                IsInsuranceComplete(loaded, docMeta, HasDoc(ProviderDocumentTypes.Insurance)),
                IsInsurancePending(loaded, docMeta, HasDoc(ProviderDocumentTypes.Insurance)),
                expandSection, GetDocUrl(ProviderDocumentTypes.Insurance), HasDoc(ProviderDocumentTypes.Insurance),
                docMeta.InsuranceNotApplicable, docMeta.InsuranceUnknown,
                new Dictionary<string, string?>
                {
                    ["InsuranceCompany"] = docMeta.InsuranceCompany,
                    ["PolicyNumber"] = docMeta.PolicyNumber,
                    ["CoverageAmount"] = docMeta.CoverageAmount,
                    ["InsuranceExpiry"] = docMeta.InsuranceExpiry
                }),
            BuildDocumentSection(
                "w9", ProviderProDisplayLocalization.L("W-9"), ProviderProDisplayLocalization.L("Upload your tax form"), "fa-file-invoice",
                ProviderDocumentTypes.W9,
                IsW9Complete(docMeta, HasDoc(ProviderDocumentTypes.W9)),
                IsW9Pending(docMeta, HasDoc(ProviderDocumentTypes.W9)),
                expandSection, GetDocUrl(ProviderDocumentTypes.W9), HasDoc(ProviderDocumentTypes.W9),
                docMeta.W9NotApplicable, docMeta.W9Unknown,
                new Dictionary<string, string?>
                {
                    ["W9LegalName"] = docMeta.W9LegalName ?? loaded.BusinessName,
                    ["W9DbaName"] = docMeta.W9DbaName ?? loaded.DbaName,
                    ["W9TaxClassification"] = docMeta.W9TaxClassification,
                    ["W9Ein"] = docMeta.W9Ein ?? meta.EinNumber
                }),
            BuildDocumentSection(
                "background", ProviderProDisplayLocalization.L("Background Check"), ProviderProDisplayLocalization.L("Complete trust and safety review"), "fa-user-shield",
                ProviderDocumentTypes.GovernmentId,
                IsBackgroundComplete(loaded, docMeta),
                IsBackgroundPending(loaded, docMeta),
                expandSection, null, loaded.BackgroundCheckConsent,
                false, false,
                new Dictionary<string, string?>
                {
                    ["BackgroundFullName"] = docMeta.BackgroundFullName ?? loaded.PrimaryContact,
                    ["BackgroundDob"] = docMeta.BackgroundDob,
                    ["BackgroundSsnLast4"] = docMeta.BackgroundSsnLast4,
                    ["BackgroundState"] = docMeta.BackgroundState ?? loaded.PrimaryCity
                })
        };

        var nextDoc = sections.FirstOrDefault(s => !s.IsComplete);
        var activeId = expandSection ?? nextDoc?.Id;

        if (!string.IsNullOrWhiteSpace(activeId))
        {
            for (var i = 0; i < sections.Count; i++)
            {
                var s = sections[i];
                sections[i] = new ProviderProfileDocumentSectionViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    IconClass = s.IconClass,
                    DocumentType = s.DocumentType,
                    StatusLabel = s.StatusLabel,
                    StatusKind = s.StatusKind,
                    ActionLabel = s.ActionLabel,
                    IsComplete = s.IsComplete,
                    IsExpanded = string.Equals(s.Id, activeId, StringComparison.OrdinalIgnoreCase),
                    IsUploaded = s.IsUploaded,
                    FileUrl = s.FileUrl,
                    NotApplicable = s.NotApplicable,
                    Unknown = s.Unknown,
                    Fields = s.Fields
                };
            }
        }

        return new ProviderProfileDocumentsViewModel
        {
            CompanyName = ResolveCompanyName(loaded),
            CompanyInitial = BuildCompanyInitial(ResolveCompanyName(loaded)),
            CompletedDocuments = sections.Count(s => s.IsComplete),
            Sections = sections,
            ActiveSectionId = activeId,
            ContinueAction = nextDoc == null
                ? "/Proveedor/ProfileCompletion"
                : $"/Proveedor/ProfileDocuments?section={nextDoc.Id}"
        };
    }

    public async Task<bool> SaveProfileDocumentsAsync(
        int proveedorId,
        ProviderProfileDocumentsInput input,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.IndorProveedores
            .FirstOrDefaultAsync(p => p.Id == proveedorId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        UpdateOnboardingMeta(entity, meta =>
        {
            meta.ProfileDocuments.LicenseNumber = TrimOrNull(input.LicenseNumber);
            meta.ProfileDocuments.LicenseType = TrimOrNull(input.LicenseType);
            meta.ProfileDocuments.LicenseState = TrimOrNull(input.LicenseState);
            meta.ProfileDocuments.LicenseExpiry = TrimOrNull(input.LicenseExpiry);
            meta.ProfileDocuments.LicenseNotApplicable = input.LicenseNotApplicable;
            meta.ProfileDocuments.LicenseUnknown = input.LicenseUnknown;

            meta.ProfileDocuments.InsuranceCompany = TrimOrNull(input.InsuranceCompany);
            meta.ProfileDocuments.PolicyNumber = TrimOrNull(input.PolicyNumber);
            meta.ProfileDocuments.CoverageAmount = TrimOrNull(input.CoverageAmount);
            meta.ProfileDocuments.InsuranceExpiry = TrimOrNull(input.InsuranceExpiry);
            meta.ProfileDocuments.InsuranceNotApplicable = input.InsuranceNotApplicable;
            meta.ProfileDocuments.InsuranceUnknown = input.InsuranceUnknown;

            meta.ProfileDocuments.W9LegalName = TrimOrNull(input.W9LegalName);
            meta.ProfileDocuments.W9DbaName = TrimOrNull(input.W9DbaName);
            meta.ProfileDocuments.W9TaxClassification = TrimOrNull(input.W9TaxClassification);
            meta.ProfileDocuments.W9Ein = TrimOrNull(input.W9Ein);
            meta.ProfileDocuments.W9NotApplicable = input.W9NotApplicable;
            meta.ProfileDocuments.W9Unknown = input.W9Unknown;

            meta.ProfileDocuments.BackgroundFullName = TrimOrNull(input.BackgroundFullName);
            meta.ProfileDocuments.BackgroundDob = TrimOrNull(input.BackgroundDob);
            meta.ProfileDocuments.BackgroundSsnLast4 = TrimOrNull(input.BackgroundSsnLast4);
            meta.ProfileDocuments.BackgroundState = TrimOrNull(input.BackgroundState);
            meta.ProfileDocuments.BackgroundConsent = input.BackgroundConsent;

            if (!string.IsNullOrWhiteSpace(input.W9Ein))
            {
                meta.EinNumber = TrimOrNull(input.W9Ein);
            }
        });

        entity.LicenseNumber = TrimOrNull(input.LicenseNumber) ?? entity.LicenseNumber;
        entity.BackgroundCheckConsent = input.BackgroundConsent;
        entity.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<IndorProveedor?> LoadProveedorForProfileFlowAsync(int proveedorId, CancellationToken ct)
    {
        return await db.IndorProveedores
            .Include(p => p.Categorias)
            .Include(p => p.Ofertas)
            .Include(p => p.Documentos)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == proveedorId, ct);
    }

    private List<ProviderProfileSectionViewModel> BuildProfileSections(IndorProveedor proveedor)
    {
        var meta = ReadOnboardingMeta(proveedor.OnboardingMetaJson);
        var docMeta = meta.ProfileDocuments;
        var docs = proveedor.Documentos;

        bool HasDoc(string type) =>
            docs.Any(d => string.Equals(d.DocumentType, type, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(d.FileUrl));

        var basicComplete = IsBasicInfoComplete(proveedor);
        var licenseComplete = IsLicenseComplete(proveedor, docMeta, HasDoc(ProviderDocumentTypes.License));
        var insuranceComplete = IsInsuranceComplete(proveedor, docMeta, HasDoc(ProviderDocumentTypes.Insurance));
        var w9Complete = IsW9Complete(docMeta, HasDoc(ProviderDocumentTypes.W9));
        var backgroundComplete = IsBackgroundComplete(proveedor, docMeta);
        var servicesComplete = proveedor.Categorias.Count > 0 || proveedor.Ofertas.Count > 0;
        var areasComplete = !string.IsNullOrWhiteSpace(proveedor.PrimaryCity)
            || !string.IsNullOrWhiteSpace(FormatZipNeighborhoods(proveedor.ZipNeighborhoodsJson));

        return
        [
            MakeSection("basic", ProviderProDisplayLocalization.L("Basic Information"), ProviderProDisplayLocalization.L("Add your business details and contact info"), "fa-circle-info", basicComplete, false, ProviderProDisplayLocalization.L("Edit"), "/Proveedor/ProfileBusiness"),
            MakeSection("services", ProviderProDisplayLocalization.L("Services Offered"), ProviderProDisplayLocalization.L("Tell homeowners what you specialize in"), "fa-wrench", servicesComplete, false, ProviderProDisplayLocalization.L("Edit"), "/Proveedor/ProfileBusiness#services"),
            MakeSection("areas", ProviderProDisplayLocalization.L("Service Areas"), ProviderProDisplayLocalization.L("Define the areas you serve"), "fa-map-location-dot", areasComplete, false, ProviderProDisplayLocalization.L("Edit"), "/Proveedor/ProfileBusiness#areas"),
            MakeSection("license", ProviderProDisplayLocalization.L("License"), ProviderProDisplayLocalization.L("Upload or verify your business license"), "fa-id-card", licenseComplete, IsLicensePending(proveedor, docMeta, HasDoc(ProviderDocumentTypes.License)), licenseComplete ? ProviderProDisplayLocalization.L("Edit") : ProviderProDisplayLocalization.L("Complete"), "/Proveedor/ProfileDocuments?section=license"),
            MakeSection("insurance", ProviderProDisplayLocalization.L("Insurance & COI"), ProviderProDisplayLocalization.L("Add proof of insurance and COI"), "fa-shield-halved", insuranceComplete, IsInsurancePending(proveedor, docMeta, HasDoc(ProviderDocumentTypes.Insurance)), insuranceComplete ? ProviderProDisplayLocalization.L("Edit") : ProviderProDisplayLocalization.L("Upload"), "/Proveedor/ProfileDocuments?section=insurance"),
            MakeSection("w9", ProviderProDisplayLocalization.L("W-9"), ProviderProDisplayLocalization.L("Upload your tax form"), "fa-file-invoice", w9Complete, IsW9Pending(docMeta, HasDoc(ProviderDocumentTypes.W9)), w9Complete ? ProviderProDisplayLocalization.L("Edit") : ProviderProDisplayLocalization.L("Upload"), "/Proveedor/ProfileDocuments?section=w9"),
            MakeSection("background", ProviderProDisplayLocalization.L("Background Check"), ProviderProDisplayLocalization.L("Complete trust and safety review"), "fa-user-shield", backgroundComplete, IsBackgroundPending(proveedor, docMeta), backgroundComplete ? ProviderProDisplayLocalization.L("Edit") : ProviderProDisplayLocalization.L("Complete"), "/Proveedor/ProfileDocuments?section=background")
        ];

        ProviderProfileSectionViewModel MakeSection(
            string id, string title, string description, string icon, bool complete, bool pendingReview,
            string actionLabel, string action)
        {
            var (statusKind, statusLabel) = ResolveSectionStatus(id, complete, pendingReview);

            return new ProviderProfileSectionViewModel
            {
                Id = id,
                Title = title,
                Description = description,
                IconClass = icon,
                IsComplete = complete,
                ActionLabel = actionLabel,
                Action = action,
                StatusLabel = statusLabel,
                StatusKind = statusKind
            };
        }
    }

    private static (string Kind, string Label) ResolveSectionStatus(string id, bool complete, bool pendingReview) =>
        ProviderProDisplayLocalization.SectionStatus(id, complete, pendingReview);

    private static bool IsBasicInfoComplete(IndorProveedor p) =>
        (!string.IsNullOrWhiteSpace(p.BusinessName) || !string.IsNullOrWhiteSpace(p.DbaName))
        && !string.IsNullOrWhiteSpace(p.Phone)
        && !string.IsNullOrWhiteSpace(p.Email);

    private static bool IsLicenseComplete(IndorProveedor p, ProviderProfileDocumentMeta m, bool hasDoc) =>
        m.LicenseNotApplicable || (hasDoc && (!string.IsNullOrWhiteSpace(m.LicenseNumber) || !string.IsNullOrWhiteSpace(p.LicenseNumber)));

    private static bool IsInsuranceComplete(IndorProveedor p, ProviderProfileDocumentMeta m, bool hasDoc) =>
        m.InsuranceNotApplicable || hasDoc;

    private static bool IsW9Complete(ProviderProfileDocumentMeta m, bool hasDoc) =>
        m.W9NotApplicable || hasDoc;

    private static bool IsBackgroundComplete(IndorProveedor p, ProviderProfileDocumentMeta m) =>
        p.BackgroundCheckConsent
        && !string.IsNullOrWhiteSpace(m.BackgroundFullName ?? p.PrimaryContact)
        && !string.IsNullOrWhiteSpace(m.BackgroundDob);

    private static bool IsLicensePending(IndorProveedor p, ProviderProfileDocumentMeta m, bool hasDoc) =>
        !IsLicenseComplete(p, m, hasDoc) && !m.LicenseNotApplicable
        && (hasDoc || !string.IsNullOrWhiteSpace(m.LicenseNumber) || !string.IsNullOrWhiteSpace(p.LicenseNumber)
            || !string.IsNullOrWhiteSpace(m.LicenseType) || !string.IsNullOrWhiteSpace(m.LicenseState));

    private static bool IsInsurancePending(IndorProveedor p, ProviderProfileDocumentMeta m, bool hasDoc) =>
        !IsInsuranceComplete(p, m, hasDoc) && !m.InsuranceNotApplicable
        && (hasDoc || !string.IsNullOrWhiteSpace(m.InsuranceCompany) || !string.IsNullOrWhiteSpace(m.PolicyNumber));

    private static bool IsW9Pending(ProviderProfileDocumentMeta m, bool hasDoc) =>
        !IsW9Complete(m, hasDoc) && !m.W9NotApplicable
        && (hasDoc || !string.IsNullOrWhiteSpace(m.W9LegalName) || !string.IsNullOrWhiteSpace(m.W9Ein));

    private static bool IsBackgroundPending(IndorProveedor p, ProviderProfileDocumentMeta m) =>
        !IsBackgroundComplete(p, m)
        && (p.BackgroundCheckConsent || !string.IsNullOrWhiteSpace(m.BackgroundFullName ?? p.PrimaryContact)
            || !string.IsNullOrWhiteSpace(m.BackgroundDob));

    private static ProviderProfileDocumentSectionViewModel BuildDocumentSection(
        string id, string title, string description, string icon, string docType,
        bool complete, bool pendingReview, string? expandSection,
        string? fileUrl, bool isUploaded, bool notApplicable, bool unknown,
        Dictionary<string, string?> fields)
    {
        var (statusKind, statusLabel) = ResolveSectionStatus(id, complete, pendingReview);
        var actionLabel = complete
            ? ProviderProDisplayLocalization.L("Edit")
            : id is "insurance" or "w9"
                ? ProviderProDisplayLocalization.L("Upload")
                : ProviderProDisplayLocalization.L("Complete");

        return new ProviderProfileDocumentSectionViewModel
        {
            Id = id,
            Title = title,
            Description = description,
            IconClass = icon,
            DocumentType = docType,
            StatusLabel = statusLabel,
            StatusKind = statusKind,
            ActionLabel = actionLabel,
            IsComplete = complete,
            IsExpanded = string.Equals(expandSection, id, StringComparison.OrdinalIgnoreCase),
            IsUploaded = isUploaded,
            FileUrl = fileUrl,
            NotApplicable = notApplicable,
            Unknown = unknown,
            Fields = fields
        };
    }

    private static string FormatCityState(string? city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return ProviderProDisplayLocalization.L("Add your location");
        }

        return city.Contains(',') ? city.Trim() : city.Trim();
    }

    private static string ResolvePublicBusinessName(IndorProveedor proveedor)
    {
        if (!string.IsNullOrWhiteSpace(proveedor.BusinessName))
        {
            return proveedor.BusinessName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(proveedor.DbaName))
        {
            return proveedor.DbaName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(proveedor.PrimaryContact))
        {
            return proveedor.PrimaryContact.Trim();
        }

        return ProviderProDisplayLocalization.L("Your business name");
    }

    private static string BuildServiceAreaSummary(IndorProveedor proveedor)
    {
        if (!string.IsNullOrWhiteSpace(proveedor.PrimaryCity))
        {
            return ProviderProDisplayLocalization.T("Serving {0} and surrounding areas", proveedor.PrimaryCity.Trim());
        }

        return ProviderProDisplayLocalization.L("Add your service areas so homeowners can find you.");
    }

    private static string TriStatePreference(bool value) => value ? "yes" : "no";

    private static bool ParseTriStatePreference(string? preference, bool current)
    {
        if (string.IsNullOrWhiteSpace(preference))
        {
            return current;
        }

        return preference.Trim().ToLowerInvariant() switch
        {
            "yes" or "true" or "1" => true,
            "no" or "false" or "0" => false,
            "na" or "n/a" => false,
            _ => current
        };
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
