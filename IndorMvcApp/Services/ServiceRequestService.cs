using System.Globalization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed record CreateServiceRequestInput(
    string CategoryId,
    string Title,
    string? Description,
    int? PropiedadId,
    string? Address,
    string? ContactPhone,
    DateTime? PreferredDate,
    string? PreferredTime,
    decimal? BudgetAmount,
    string Urgency);

public interface IServiceRequestService
{
    Task<List<ServiceRequestCategoryOption>> GetCategoryOptionsAsync(bool isSpanish, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ApplicationUser homeowner, CreateServiceRequestInput input, CancellationToken cancellationToken = default);
    Task<HomeownerRequestsViewModel> GetHomeownerRequestsAsync(string userId, bool isSpanish, CancellationToken cancellationToken = default);
    Task<HomeownerRequestDetailViewModel?> GetHomeownerRequestDetailAsync(string userId, int id, bool isSpanish, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(string userId, int id, CancellationToken cancellationToken = default);

    Task<ProviderAvailableRequestsViewModel> GetAvailableForProviderAsync(IndorProveedor proveedor, bool isSpanish, CancellationToken cancellationToken = default);
    Task<ProviderRequestDetailViewModel?> GetProviderRequestDetailAsync(IndorProveedor proveedor, int id, bool isSpanish, CancellationToken cancellationToken = default);
    Task<ClaimServiceRequestResult> ClaimAsync(IndorProveedor proveedor, int id, CancellationToken cancellationToken = default);
}

public class ServiceRequestService(
    AppDbContext db,
    INotificationService notifications,
    ITransactionalEmailSender email,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ServiceRequestService> logger) : IServiceRequestService
{
    private static readonly string[] ActiveProviderStatuses =
    {
        ProviderRegistrationStatuses.Approved,
        ProviderRegistrationStatuses.IndorProActive,
        ProviderRegistrationStatuses.PendingReview
    };

    // ---------------------------------------------------------------- Categories

    public async Task<List<ServiceRequestCategoryOption>> GetCategoryOptionsAsync(bool isSpanish, CancellationToken cancellationToken = default)
    {
        var cats = await db.IndorProveedorCategoriasCatalogo
            .AsNoTracking()
            .Where(c => c.Activo)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.LabelEn)
            .ToListAsync(cancellationToken);

        return cats.Select(c => new ServiceRequestCategoryOption
        {
            Id = c.Id,
            Label = CategoryLabel(c, isSpanish),
            IconClass = string.IsNullOrWhiteSpace(c.IconClass) ? "fa-screwdriver-wrench" : c.IconClass,
            Description = isSpanish ? (c.DescriptionEs ?? c.DescriptionEn) : c.DescriptionEn
        }).ToList();
    }

    // ---------------------------------------------------------------- Create

    public async Task<int> CreateAsync(ApplicationUser homeowner, CreateServiceRequestInput input, CancellationToken cancellationToken = default)
    {
        var request = new IndorServiceRequest
        {
            UserId = homeowner.Id,
            PropiedadId = input.PropiedadId,
            CategoryId = input.CategoryId,
            Title = input.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(input.Description) ? null : input.Description.Trim(),
            Address = string.IsNullOrWhiteSpace(input.Address) ? null : input.Address.Trim(),
            ContactPhone = string.IsNullOrWhiteSpace(input.ContactPhone) ? null : input.ContactPhone.Trim(),
            PreferredDate = input.PreferredDate,
            PreferredTime = string.IsNullOrWhiteSpace(input.PreferredTime) ? null : input.PreferredTime.Trim(),
            BudgetAmount = input.BudgetAmount,
            Urgency = NormalizeUrgency(input.Urgency),
            Status = ServiceRequestStatuses.Open,
            FechaCreacion = DateTime.UtcNow
        };

        // Resolve address from property if not supplied.
        if (string.IsNullOrWhiteSpace(request.Address) && input.PropiedadId.HasValue)
        {
            var prop = await db.Propiedades.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == input.PropiedadId.Value && p.UserId == homeowner.Id, cancellationToken);
            request.Address = prop?.Direccion;
        }

        db.IndorServiceRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);

        await NotifyMatchingProvidersAsync(request, cancellationToken);

        return request.Id;
    }

    private async Task NotifyMatchingProvidersAsync(IndorServiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await db.IndorProveedorCategoriasCatalogo.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

            var providerIds = await db.IndorProveedorCategoriasSel
                .AsNoTracking()
                .Where(s => s.CategoriaId == request.CategoryId)
                .Select(s => s.ProveedorId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var providers = await db.IndorProveedores
                .AsNoTracking()
                .Where(p => providerIds.Contains(p.Id)
                    && ActiveProviderStatuses.Contains(p.RegistrationStatus)
                    && p.UserId != null)
                .ToListAsync(cancellationToken);

            if (providers.Count == 0)
            {
                logger.LogInformation("Service request {Id}: no matching providers for category {Category}.", request.Id, request.CategoryId);
                return;
            }

            var location = ComposeLocation(request);
            var actionPath = $"/Proveedor/AvailableRequestDetails/{request.Id}";
            var actionUrl = AbsoluteUrl(actionPath);

            var newNotifications = new List<NewAppNotification>();

            foreach (var provider in providers)
            {
                var providerName = ProviderDisplayName(provider);
                var catEn = category?.LabelEn ?? request.CategoryId;
                var catEs = category?.LabelEs ?? catEn;

                newNotifications.Add(new NewAppNotification(
                    RecipientUserId: provider.UserId!,
                    Audience: AppNotificationAudiences.Provider,
                    TitleEn: "New service request",
                    TitleEs: "Nueva solicitud de servicio",
                    BodyEn: $"{catEn} — {request.Title}" + (string.IsNullOrWhiteSpace(location) ? "" : $" · {location}"),
                    BodyEs: $"{catEs} — {request.Title}" + (string.IsNullOrWhiteSpace(location) ? "" : $" · {location}"),
                    CategoryTag: "Requests",
                    IconClass: string.IsNullOrWhiteSpace(category?.IconClass) ? "fa-bell-concierge" : category!.IconClass,
                    TargetUrl: actionPath));

                // Email (best-effort, per-provider culture).
                var providerEmail = !string.IsNullOrWhiteSpace(provider.Email) ? provider.Email : null;
                if (providerEmail != null)
                {
                    var isEs = IsSpanishUser(provider.User);
                    var html = IndorEmailTemplates.ProviderNewRequest(isEs, new ProviderNewRequestEmail(
                        ProviderName: providerName,
                        Title: request.Title,
                        CategoryLabel: isEs ? catEs : catEn,
                        Location: location ?? "—",
                        WhenLabel: WhenLabel(request, isEs) ?? (isEs ? "Flexible" : "Flexible"),
                        BudgetLabel: BudgetLabel(request, isEs) ?? (isEs ? "A convenir" : "Open"),
                        Description: string.IsNullOrWhiteSpace(request.Description) ? "—" : request.Description!,
                        ActionUrl: actionUrl));

                    var subject = isEs ? $"Nueva solicitud: {request.Title}" : $"New request: {request.Title}";
                    await email.SendAsync(new TransactionalEmailModel(providerEmail, providerName, subject, html), cancellationToken);
                }
            }

            await notifications.CreateManyAsync(newNotifications, cancellationToken);

            request.NotifiedProviderCount = providers.Count;
            await db.IndorServiceRequests
                .Where(r => r.Id == request.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.NotifiedProviderCount, providers.Count), cancellationToken);
        }
        catch (Exception ex)
        {
            // Never block request creation because of fan-out issues.
            logger.LogError(ex, "Failed to notify providers for service request {Id}.", request.Id);
        }
    }

    // ---------------------------------------------------------------- Homeowner reads

    public async Task<HomeownerRequestsViewModel> GetHomeownerRequestsAsync(string userId, bool isSpanish, CancellationToken cancellationToken = default)
    {
        var rows = await db.IndorServiceRequests
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.ClaimedByProveedor)
            .Include(r => r.Propiedad)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync(cancellationToken);

        var vm = new HomeownerRequestsViewModel();
        foreach (var r in rows)
        {
            var item = MapListItem(r, isSpanish);
            switch (r.Status)
            {
                case ServiceRequestStatuses.Open:
                    vm.Open.Add(item);
                    break;
                case ServiceRequestStatuses.Claimed:
                    vm.Claimed.Add(item);
                    break;
                default:
                    vm.Closed.Add(item);
                    break;
            }
        }
        return vm;
    }

    public async Task<HomeownerRequestDetailViewModel?> GetHomeownerRequestDetailAsync(string userId, int id, bool isSpanish, CancellationToken cancellationToken = default)
    {
        var r = await db.IndorServiceRequests
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ClaimedByProveedor)
            .Include(x => x.Propiedad)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (r == null)
        {
            return null;
        }

        ClaimedProviderContactViewModel? provider = null;
        if (r.ClaimedByProveedor != null)
        {
            var p = r.ClaimedByProveedor;
            provider = new ClaimedProviderContactViewModel
            {
                Name = ProviderDisplayName(p),
                Contact = p.PrimaryContact,
                Phone = p.Phone,
                Email = p.Email,
                IsInsured = p.IsInsured,
                IsLicensed = p.IsLicensed,
                YearsExperience = p.YearsExperience
            };
        }

        return new HomeownerRequestDetailViewModel
        {
            Request = MapListItem(r, isSpanish),
            ContactPhone = r.ContactPhone,
            ClaimedUtc = r.ClaimedUtc,
            Provider = provider
        };
    }

    public async Task<bool> CancelAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        var affected = await db.IndorServiceRequests
            .Where(r => r.Id == id && r.UserId == userId && r.Status == ServiceRequestStatuses.Open)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ServiceRequestStatuses.Cancelled)
                .SetProperty(r => r.CancelledUtc, DateTime.UtcNow)
                .SetProperty(r => r.FechaActualizacion, DateTime.UtcNow), cancellationToken);
        return affected > 0;
    }

    // ---------------------------------------------------------------- Provider reads

    public async Task<ProviderAvailableRequestsViewModel> GetAvailableForProviderAsync(IndorProveedor proveedor, bool isSpanish, CancellationToken cancellationToken = default)
    {
        var categoryIds = await db.IndorProveedorCategoriasSel
            .AsNoTracking()
            .Where(s => s.ProveedorId == proveedor.Id)
            .Select(s => s.CategoriaId)
            .ToListAsync(cancellationToken);

        var vm = new ProviderAvailableRequestsViewModel
        {
            CompanyName = ProviderDisplayName(proveedor),
            HasMatchingCategories = categoryIds.Count > 0
        };

        if (categoryIds.Count == 0)
        {
            return vm;
        }

        var rows = await db.IndorServiceRequests
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Propiedad)
            .Where(r => r.Status == ServiceRequestStatuses.Open && categoryIds.Contains(r.CategoryId))
            .OrderByDescending(r => r.Urgency == ServiceRequestUrgencies.Emergency)
            .ThenByDescending(r => r.Urgency == ServiceRequestUrgencies.Urgent)
            .ThenByDescending(r => r.FechaCreacion)
            .Take(100)
            .ToListAsync(cancellationToken);

        vm.Requests = rows.Select(r => MapListItem(r, isSpanish)).ToList();
        return vm;
    }

    public async Task<ProviderRequestDetailViewModel?> GetProviderRequestDetailAsync(IndorProveedor proveedor, int id, bool isSpanish, CancellationToken cancellationToken = default)
    {
        var r = await db.IndorServiceRequests
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Propiedad)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (r == null)
        {
            return null;
        }

        var categoryIds = await db.IndorProveedorCategoriasSel
            .AsNoTracking()
            .Where(s => s.ProveedorId == proveedor.Id)
            .Select(s => s.CategoriaId)
            .ToListAsync(cancellationToken);

        var matches = categoryIds.Contains(r.CategoryId);
        var takenByMe = r.ClaimedByProveedorId == proveedor.Id;

        // Only expose requests the provider is eligible for (matching category) or already took.
        if (!matches && !takenByMe)
        {
            return null;
        }

        var homeownerName = r.User != null ? FullName(r.User) : null;

        return new ProviderRequestDetailViewModel
        {
            CompanyName = ProviderDisplayName(proveedor),
            Request = MapListItem(r, isSpanish),
            Description = r.Description,
            ContactPhone = takenByMe ? r.ContactPhone : null, // contact revealed after claiming
            HomeownerName = takenByMe ? homeownerName : null,
            CanTake = r.Status == ServiceRequestStatuses.Open,
            AlreadyTaken = r.Status == ServiceRequestStatuses.Claimed && !takenByMe,
            TakenByMe = takenByMe
        };
    }

    // ---------------------------------------------------------------- Claim (atomic)

    public async Task<ClaimServiceRequestResult> ClaimAsync(IndorProveedor proveedor, int id, CancellationToken cancellationToken = default)
    {
        // Eligibility: provider must serve the request's category.
        var request = await db.IndorServiceRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (request == null)
        {
            return ClaimServiceRequestResult.NotFound;
        }
        if (request.Status == ServiceRequestStatuses.Claimed && request.ClaimedByProveedorId == proveedor.Id)
        {
            return ClaimServiceRequestResult.Success; // idempotent
        }
        if (request.Status != ServiceRequestStatuses.Open)
        {
            return ClaimServiceRequestResult.AlreadyTaken;
        }

        var eligible = await db.IndorProveedorCategoriasSel
            .AnyAsync(s => s.ProveedorId == proveedor.Id && s.CategoriaId == request.CategoryId, cancellationToken);
        if (!eligible)
        {
            return ClaimServiceRequestResult.NotEligible;
        }

        // Atomic first-come claim: only one UPDATE can flip Open -> Claimed.
        var affected = await db.IndorServiceRequests
            .Where(r => r.Id == id && r.Status == ServiceRequestStatuses.Open)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ServiceRequestStatuses.Claimed)
                .SetProperty(r => r.ClaimedByProveedorId, proveedor.Id)
                .SetProperty(r => r.ClaimedUtc, DateTime.UtcNow)
                .SetProperty(r => r.FechaActualizacion, DateTime.UtcNow), cancellationToken);

        if (affected == 0)
        {
            return ClaimServiceRequestResult.AlreadyTaken;
        }

        await NotifyHomeownerClaimedAsync(id, proveedor, cancellationToken);
        return ClaimServiceRequestResult.Success;
    }

    private async Task NotifyHomeownerClaimedAsync(int requestId, IndorProveedor proveedor, CancellationToken cancellationToken)
    {
        try
        {
            var request = await db.IndorServiceRequests.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);
            if (request?.User == null)
            {
                return;
            }

            var homeowner = request.User;
            var providerName = ProviderDisplayName(proveedor);
            var detailPath = $"/ServiceRequest/Detail/{request.Id}";

            await notifications.CreateAsync(new NewAppNotification(
                RecipientUserId: homeowner.Id,
                Audience: AppNotificationAudiences.Homeowner,
                TitleEn: "Your request was accepted",
                TitleEs: "Tu solicitud fue aceptada",
                BodyEn: $"{providerName} took your \u201C{request.Title}\u201D request.",
                BodyEs: $"{providerName} tomó tu solicitud \u201C{request.Title}\u201D.",
                CategoryTag: "Requests",
                IconClass: "fa-handshake-angle",
                TargetUrl: detailPath), cancellationToken);

            var homeownerEmail = homeowner.Email;
            if (!string.IsNullOrWhiteSpace(homeownerEmail))
            {
                var isEs = IsSpanishUser(homeowner);
                var html = IndorEmailTemplates.HomeownerClaimed(isEs, new HomeownerClaimedEmail(
                    HomeownerName: FullName(homeowner),
                    ProviderName: providerName,
                    ProviderContact: proveedor.PrimaryContact ?? "—",
                    ProviderPhone: proveedor.Phone ?? "—",
                    ProviderEmail: proveedor.Email ?? "—",
                    Title: request.Title,
                    ActionUrl: AbsoluteUrl(detailPath)));

                var subject = isEs ? $"{providerName} tomó tu solicitud" : $"{providerName} took your request";
                await email.SendAsync(new TransactionalEmailModel(homeownerEmail, FullName(homeowner), subject, html,
                    ReplyToEmail: proveedor.Email), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify homeowner for claimed service request {Id}.", requestId);
        }
    }

    // ---------------------------------------------------------------- Mapping helpers

    private ServiceRequestListItemViewModel MapListItem(IndorServiceRequest r, bool isSpanish) => new()
    {
        Id = r.Id,
        Title = r.Title,
        CategoryLabel = r.Category != null ? CategoryLabel(r.Category, isSpanish) : r.CategoryId,
        CategoryIcon = string.IsNullOrWhiteSpace(r.Category?.IconClass) ? "fa-screwdriver-wrench" : r.Category!.IconClass,
        Location = ComposeLocation(r),
        WhenLabel = WhenLabel(r, isSpanish),
        BudgetLabel = BudgetLabel(r, isSpanish),
        Status = r.Status,
        Urgency = r.Urgency,
        CreatedUtc = r.FechaCreacion,
        ClaimedProviderName = r.ClaimedByProveedor != null ? ProviderDisplayName(r.ClaimedByProveedor) : null,
        Description = r.Description
    };

    private static string CategoryLabel(IndorProveedorCategoriaCatalogo c, bool isSpanish) =>
        isSpanish ? (string.IsNullOrWhiteSpace(c.LabelEs) ? c.LabelEn : c.LabelEs!) : c.LabelEn;

    private static string? ComposeLocation(IndorServiceRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.Address)) return r.Address;
        if (!string.IsNullOrWhiteSpace(r.Propiedad?.Direccion)) return r.Propiedad!.Direccion;
        return null;
    }

    private static string? WhenLabel(IndorServiceRequest r, bool isSpanish)
    {
        if (r.PreferredDate == null && string.IsNullOrWhiteSpace(r.PreferredTime))
        {
            return null;
        }
        var culture = isSpanish ? new CultureInfo("es-US") : new CultureInfo("en-US");
        var datePart = r.PreferredDate?.ToString("MMM d, yyyy", culture);
        var parts = new[] { datePart, r.PreferredTime }.Where(s => !string.IsNullOrWhiteSpace(s));
        var label = string.Join(" · ", parts);
        return string.IsNullOrWhiteSpace(label) ? null : label;
    }

    private static string? BudgetLabel(IndorServiceRequest r, bool isSpanish)
    {
        if (r.BudgetAmount == null || r.BudgetAmount <= 0) return null;
        return "$" + r.BudgetAmount.Value.ToString("#,0.##", CultureInfo.InvariantCulture);
    }

    private static string ProviderDisplayName(IndorProveedor p)
    {
        if (!string.IsNullOrWhiteSpace(p.BusinessName)) return p.BusinessName!;
        if (!string.IsNullOrWhiteSpace(p.DbaName)) return p.DbaName!;
        if (!string.IsNullOrWhiteSpace(p.PrimaryContact)) return p.PrimaryContact!;
        return "INDOR Pro";
    }

    private static string FullName(ApplicationUser u)
    {
        var name = $"{u.Nombre} {u.Apellidos}".Trim();
        return string.IsNullOrWhiteSpace(name) ? (u.Email ?? "") : name;
    }

    private static bool IsSpanishUser(ApplicationUser? u)
    {
        if (u?.PreferredUiCulture != null)
        {
            return u.PreferredUiCulture.StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeUrgency(string? urgency) => urgency switch
    {
        ServiceRequestUrgencies.Urgent => ServiceRequestUrgencies.Urgent,
        ServiceRequestUrgencies.Emergency => ServiceRequestUrgencies.Emergency,
        _ => ServiceRequestUrgencies.Standard
    };

    private string AbsoluteUrl(string path)
    {
        var req = httpContextAccessor.HttpContext?.Request;
        if (req != null)
        {
            return $"{req.Scheme}://{req.Host}{path}";
        }
        return path;
    }
}
