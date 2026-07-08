using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed class ContractorVerificationService(
    AppDbContext db,
    ILogger<ContractorVerificationService> logger) : IContractorVerificationService
{
    // ---------------------------------------------------------------- Queue

    public async Task<VerificationQueueViewModel> GetQueueAsync(
        IndorProveedor me, string? tab, string? query, CancellationToken cancellationToken = default)
    {
        var catalog = await LoadCatalogAsync(cancellationToken);
        var providers = await LoadProvidersAsync(me.Id, cancellationToken);
        var records = await LoadRecordsAsync(providers.Select(p => p.Id), cancellationToken);

        var normalizedTab = string.IsNullOrWhiteSpace(tab) ? "pending" : tab.Trim().ToLowerInvariant();
        var normalizedQuery = query?.Trim();

        var rows = providers
            .Select(p =>
            {
                records.TryGetValue(p.Id, out var rec);
                return BuildCard(p, rec, catalog);
            })
            .ToList();

        var pending = rows.Count(r => StatusOf(r) == VerificationStatuses.Pending);
        var inReview = rows.Count(r => StatusOf(r) == VerificationStatuses.InReview);
        var approved = rows.Count(r => StatusOf(r) == VerificationStatuses.Approved);
        var flagged = rows.Count(r => StatusOf(r) == VerificationStatuses.Flagged);

        var filtered = rows.Where(r => string.Equals(StatusOf(r), TabToStatus(normalizedTab), StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(normalizedQuery))
        {
            filtered = filtered.Where(r =>
                (r.Card.Name?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false)
                || (r.Card.TradeLabel?.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return new VerificationQueueViewModel
        {
            ActiveTab = normalizedTab,
            Query = normalizedQuery,
            Tabs =
            [
                new() { Id = "pending", Label = "Pending", Count = pending },
                new() { Id = "review", Label = "In Review", Count = inReview },
                new() { Id = "approved", Label = "Approved", Count = approved },
                new() { Id = "flagged", Label = "Flagged", Count = flagged }
            ],
            Contractors = filtered
                .OrderByDescending(r => r.Card.ItemsComplete)
                .Select(r => r.Card)
                .ToList()
        };
    }

    // ---------------------------------------------------------------- Detail

    public async Task<ContractorVerificationViewModel?> GetDetailAsync(
        IndorProveedor me, int contractorId, CancellationToken cancellationToken = default)
    {
        var provider = await LoadProviderAsync(contractorId, cancellationToken);
        if (provider == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var record = await EnsureRecordAsync(provider, markInReview: true, cancellationToken);

        var (items, complete) = BuildItems(provider, record, detailed: true);
        var (_, trade, icon) = ResolveTrade(provider, catalog);

        var followUps = items
            .Where(i => i.State != "ok")
            .Select(i => i.Key switch
            {
                "profile" => "Complete company profile",
                "license" => "Verify trade license",
                "insurance" => "Add certificate of insurance",
                "w9" => "Upload W-9 form",
                "background" => "Complete background check",
                _ => i.Label
            })
            .ToList();

        return new ContractorVerificationViewModel
        {
            Id = provider.Id,
            Name = ResolveName(provider),
            TradeLabel = trade,
            LocationLabel = ComposeLocation(provider),
            IconClass = icon,
            PhotoUrl = LogoUrl(provider),
            ContactPerson = provider.PrimaryContact,
            Phone = provider.Phone,
            Email = provider.Email,
            SubmittedLabel = (provider.ProfileSubmittedUtc ?? provider.FechaCreacion).ToLocalTime().ToString("MMM d, yyyy"),
            StatusLabel = StatusLabel(record?.Status ?? VerificationStatuses.InReview),
            StatusKind = "review",
            Items = items,
            ItemsComplete = complete,
            OperatorNotes = record?.OperatorNotes,
            NotesSavedLabel = record?.FechaActualizacion != null ? "Last saved " + Ago(record.FechaActualizacion.Value) : null,
            FollowUps = followUps
        };
    }

    public async Task<bool> SaveReviewAsync(
        IndorProveedor me, int contractorId, string? operatorNotes, string? mode, CancellationToken cancellationToken = default)
    {
        var provider = await db.IndorProveedores.FirstOrDefaultAsync(p => p.Id == contractorId, cancellationToken);
        if (provider == null)
        {
            return false;
        }

        var record = await GetOrCreateTrackedRecordAsync(provider, cancellationToken);
        record.OperatorNotes = Trim(operatorNotes, 600);
        record.ReviewerName = ResolveName(me);
        record.FechaActualizacion = DateTime.UtcNow;

        if (string.Equals(mode, "request", StringComparison.OrdinalIgnoreCase))
        {
            record.Status = VerificationStatuses.Flagged;
            record.FollowUpNote = "Information requested from contractor.";
        }
        else if (record.Status is not (VerificationStatuses.Approved))
        {
            record.Status = VerificationStatuses.InReview;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ---------------------------------------------------------------- Complete

    public async Task<VerificationCompleteViewModel?> GetCompleteAsync(
        IndorProveedor me, int contractorId, CancellationToken cancellationToken = default)
    {
        var provider = await LoadProviderAsync(contractorId, cancellationToken);
        if (provider == null)
        {
            return null;
        }

        var catalog = await LoadCatalogAsync(cancellationToken);
        var record = await GetRecordAsync(contractorId, cancellationToken);
        var (items, complete) = BuildItems(provider, record, detailed: false);
        var (_, trade, icon) = ResolveTrade(provider, catalog);

        return new VerificationCompleteViewModel
        {
            Id = provider.Id,
            Name = ResolveName(provider),
            TradeLabel = trade,
            LocationLabel = ComposeLocation(provider),
            IconClass = icon,
            Items = items,
            ItemsComplete = complete,
            AlreadyApproved = record?.Status == VerificationStatuses.Approved
        };
    }

    public async Task<bool> ApproveAsync(IndorProveedor me, int contractorId, CancellationToken cancellationToken = default)
    {
        var provider = await db.IndorProveedores.FirstOrDefaultAsync(p => p.Id == contractorId, cancellationToken);
        if (provider == null)
        {
            return false;
        }

        var record = await GetOrCreateTrackedRecordAsync(provider, cancellationToken);
        record.LicenseVerified = true;
        record.InsuranceVerified = true;
        record.W9Verified = true;
        record.BackgroundStatus = BackgroundCheckStatuses.Clear;
        record.Status = VerificationStatuses.Approved;
        record.ReviewerName = ResolveName(me);
        record.ApprovedUtc = DateTime.UtcNow;
        record.FechaActualizacion = DateTime.UtcNow;

        provider.IsInsured = true;
        provider.IsLicensed = true;
        provider.RegistrationStatus = ProviderRegistrationStatuses.Approved;
        provider.FechaActualizacion = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ---------------------------------------------------------------- Loaders

    private async Task<Dictionary<string, IndorProveedorCategoriaCatalogo>> LoadCatalogAsync(CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorCategoriasCatalogo.AsNoTracking().Where(c => c.Activo).ToDictionaryAsync(c => c.Id, c => c, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<List<IndorProveedor>> LoadProvidersAsync(int excludeId, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Include(p => p.Documentos)
                .Where(p => p.Id != excludeId)
                .Where(p => p.BusinessName != null || p.DbaName != null || p.PrimaryContact != null)
                .OrderByDescending(p => p.FechaActualizacion)
                .Take(200)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<IndorProveedor?> LoadProviderAsync(int id, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Include(p => p.Documentos)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }

    private async Task<Dictionary<int, IndorProveedorVerificacion>> LoadRecordsAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        try
        {
            return await db.IndorProveedorVerificaciones
                .AsNoTracking()
                .Where(v => idList.Contains(v.ProveedorId))
                .ToDictionaryAsync(v => v.ProveedorId, v => v, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }
    }

    private async Task<IndorProveedorVerificacion?> GetRecordAsync(int contractorId, CancellationToken ct)
    {
        try
        {
            return await db.IndorProveedorVerificaciones.AsNoTracking().FirstOrDefaultAsync(v => v.ProveedorId == contractorId, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }

    private async Task<IndorProveedorVerificacion?> EnsureRecordAsync(IndorProveedor provider, bool markInReview, CancellationToken ct)
    {
        try
        {
            var record = await db.IndorProveedorVerificaciones.FirstOrDefaultAsync(v => v.ProveedorId == provider.Id, ct);
            if (record == null)
            {
                record = NewRecordFrom(provider);
                if (markInReview)
                {
                    record.Status = VerificationStatuses.InReview;
                }
                db.IndorProveedorVerificaciones.Add(record);
                await db.SaveChangesAsync(ct);
            }
            else if (markInReview && record.Status == VerificationStatuses.Pending)
            {
                record.Status = VerificationStatuses.InReview;
                record.FechaActualizacion = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }

            return record;
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            logger.LogWarning(ex, "Verification table missing; returning derived record.");
            return null;
        }
    }

    private async Task<IndorProveedorVerificacion> GetOrCreateTrackedRecordAsync(IndorProveedor provider, CancellationToken ct)
    {
        var record = await db.IndorProveedorVerificaciones.FirstOrDefaultAsync(v => v.ProveedorId == provider.Id, ct);
        if (record != null)
        {
            return record;
        }

        record = NewRecordFrom(provider);
        db.IndorProveedorVerificaciones.Add(record);
        return record;
    }

    private static IndorProveedorVerificacion NewRecordFrom(IndorProveedor p) => new()
    {
        ProveedorId = p.Id,
        Status = VerificationStatuses.Pending,
        LicenseVerified = p.IsLicensed && !string.IsNullOrWhiteSpace(p.LicenseNumber),
        InsuranceVerified = p.IsInsured,
        W9Verified = p.Documentos.Any(d => string.Equals(d.DocumentType, ProviderDocumentTypes.W9, StringComparison.OrdinalIgnoreCase) && d.UploadedUtc != null),
        BackgroundStatus = BackgroundCheckStatuses.Pending,
        FechaCreacion = DateTime.UtcNow
    };

    // ---------------------------------------------------------------- Builders

    private (VerificationQueueCardViewModel Card, string Status) BuildCard(
        IndorProveedor p, IndorProveedorVerificacion? rec, IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog)
    {
        var (items, complete) = BuildItems(p, rec, detailed: false);
        var (_, trade, icon) = ResolveTrade(p, catalog);
        var insuranceOk = items.First(i => i.Key == "insurance").State == "ok";
        var status = StatusOf(p, rec);

        var (pillLabel, pillKind) = status switch
        {
            VerificationStatuses.Approved => ("Verified", "ready"),
            VerificationStatuses.Flagged => ("Needs info", "warn"),
            _ when !insuranceOk => ("Missing COI", "warn"),
            _ when complete >= 5 => ("Ready to Approve", "ready"),
            VerificationStatuses.InReview => ("In Review", "pending"),
            _ => ("Pending Review", "pending")
        };

        return (new VerificationQueueCardViewModel
        {
            Id = p.Id,
            Name = ResolveName(p),
            TradeLabel = trade,
            LocationLabel = ComposeLocation(p),
            IconClass = icon,
            PhotoUrl = LogoUrl(p),
            PillLabel = pillLabel,
            PillKind = pillKind,
            Items = items,
            ItemsComplete = complete
        }, status);
    }

    private static string StatusOf((VerificationQueueCardViewModel Card, string Status) row) => row.Status;

    private static string StatusOf(IndorProveedor p, IndorProveedorVerificacion? rec) => rec?.Status ?? VerificationStatuses.Pending;

    /// <summary>Builds the 5 checklist items. Stored record decisions win over derived data.</summary>
    private (List<VerificationItemViewModel> Items, int Complete) BuildItems(
        IndorProveedor p, IndorProveedorVerificacion? rec, bool detailed)
    {
        var licenseOk = rec?.LicenseVerified ?? (p.IsLicensed && !string.IsNullOrWhiteSpace(p.LicenseNumber));
        var insuranceOk = rec?.InsuranceVerified ?? p.IsInsured;
        var w9Ok = rec?.W9Verified ?? p.Documentos.Any(d =>
            string.Equals(d.DocumentType, ProviderDocumentTypes.W9, StringComparison.OrdinalIgnoreCase) && d.UploadedUtc != null);
        var background = rec?.BackgroundStatus ?? BackgroundCheckStatuses.Pending;
        var profilePct = ProfileCompletion(p);
        var profileOk = profilePct >= 100;

        var items = new List<VerificationItemViewModel>
        {
            new()
            {
                Key = "license",
                Label = "License",
                IconClass = "fa-shield-halved",
                State = licenseOk ? "ok" : "pending",
                StatusLabel = licenseOk ? "Verified" : "Pending",
                Detail = detailed ? (string.IsNullOrWhiteSpace(p.LicenseNumber) ? "License on file" : $"License #{p.LicenseNumber}") : null,
                Meta = detailed && rec?.LicenseExpiry != null ? $"expires {rec.LicenseExpiry:MM/dd/yyyy}" : null
            },
            new()
            {
                Key = "insurance",
                Label = detailed ? "Insurance / COI" : "Insurance",
                IconClass = "fa-file-shield",
                State = insuranceOk ? "ok" : "warn",
                StatusLabel = insuranceOk ? "Verified" : "Missing",
                Detail = detailed ? (insuranceOk ? "General Liability Active" : "No certificate on file") : null,
                Meta = detailed && rec?.InsuranceExpiry != null ? $"expires {rec.InsuranceExpiry:MM/dd/yyyy}" : null
            },
            new()
            {
                Key = "w9",
                Label = "W-9",
                IconClass = "fa-file-lines",
                State = w9Ok ? "ok" : "pending",
                StatusLabel = w9Ok ? "Uploaded" : "Missing",
                Detail = detailed ? (w9Ok ? "Uploaded" : "Not uploaded") : null
            },
            new()
            {
                Key = "background",
                Label = "Background",
                IconClass = "fa-user-shield",
                State = background == BackgroundCheckStatuses.Clear ? "ok" : background == BackgroundCheckStatuses.Flagged ? "warn" : "pending",
                StatusLabel = background switch
                {
                    BackgroundCheckStatuses.Clear => "Clear",
                    BackgroundCheckStatuses.Flagged => "Flagged",
                    _ => "Pending"
                },
                Detail = detailed ? (background switch
                {
                    BackgroundCheckStatuses.Clear => "Report clear",
                    BackgroundCheckStatuses.Flagged => "Needs attention",
                    _ => "Awaiting result"
                }) : null
            },
            new()
            {
                Key = "profile",
                Label = detailed ? "Profile Completion" : "Profile",
                IconClass = "fa-circle-notch",
                State = profileOk ? "ok" : "pending",
                StatusLabel = $"{profilePct}%"
            }
        };

        var complete = items.Count(i => i.State == "ok");
        return (items, complete);
    }

    private static int ProfileCompletion(IndorProveedor p)
    {
        var checks = new[]
        {
            !string.IsNullOrWhiteSpace(p.BusinessName) || !string.IsNullOrWhiteSpace(p.DbaName),
            !string.IsNullOrWhiteSpace(p.PrimaryContact),
            !string.IsNullOrWhiteSpace(p.Phone),
            !string.IsNullOrWhiteSpace(p.Email),
            !string.IsNullOrWhiteSpace(p.ServiceDescription),
            !string.IsNullOrWhiteSpace(p.BusinessAddress) || !string.IsNullOrWhiteSpace(p.PrimaryCity),
            p.Categorias.Count > 0,
            !string.IsNullOrWhiteSpace(p.LicenseNumber),
            p.IsInsured,
            p.Documentos.Any(d => string.Equals(d.DocumentType, ProviderDocumentTypes.Logo, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(d.FileUrl))
        };

        var filled = checks.Count(c => c);
        return (int)Math.Round(100.0 * filled / checks.Length);
    }

    // ---------------------------------------------------------------- Helpers

    private static string TabToStatus(string tab) => tab switch
    {
        "review" => VerificationStatuses.InReview,
        "approved" => VerificationStatuses.Approved,
        "flagged" => VerificationStatuses.Flagged,
        _ => VerificationStatuses.Pending
    };

    private static string StatusLabel(string status) => status switch
    {
        VerificationStatuses.Approved => "Approved",
        VerificationStatuses.Flagged => "Flagged",
        VerificationStatuses.InReview => "In Review",
        _ => "Pending"
    };

    private static (string Id, string Label, string Icon) ResolveTrade(
        IndorProveedor p, IReadOnlyDictionary<string, IndorProveedorCategoriaCatalogo> catalog)
    {
        var primary = p.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
        if (primary != null && catalog.TryGetValue(primary, out var cat))
        {
            return (cat.Id, cat.LabelEn, cat.IconClass);
        }

        return ("", p.ServiceDescription ?? "General", "fa-screwdriver-wrench");
    }

    private static string ResolveName(IndorProveedor p) =>
        !string.IsNullOrWhiteSpace(p.DbaName) ? p.DbaName!.Trim()
        : !string.IsNullOrWhiteSpace(p.BusinessName) ? p.BusinessName!.Trim()
        : !string.IsNullOrWhiteSpace(p.PrimaryContact) ? p.PrimaryContact!.Trim()
        : "INDOR Provider";

    private static string? LogoUrl(IndorProveedor p) =>
        p.Documentos.FirstOrDefault(d =>
            string.Equals(d.DocumentType, ProviderDocumentTypes.Logo, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(d.FileUrl))?.FileUrl;

    private static string? ComposeLocation(IndorProveedor p)
    {
        if (!string.IsNullOrWhiteSpace(p.PrimaryCity))
        {
            return p.PrimaryCity!.Trim();
        }

        return string.IsNullOrWhiteSpace(p.BusinessAddress) ? null : p.BusinessAddress!.Trim();
    }

    private static string Ago(DateTime utc)
    {
        var span = DateTime.UtcNow - utc;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} hr ago";
        return utc.ToLocalTime().ToString("MMM d, yyyy");
    }

    private static string? Trim(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();
        return value.Length > max ? value[..max] : value;
    }
}
