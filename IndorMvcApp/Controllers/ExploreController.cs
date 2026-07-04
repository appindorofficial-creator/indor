using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

/// <summary>
/// Public, no-login catalog so visitors can freely browse the services and
/// service providers before deciding to create an account. Account-based
/// actions (booking, scheduling, quotes, contacting a provider) still require
/// sign-in via the [Authorize] flow controllers. Required by App Store
/// guideline 5.1.1(v).
/// </summary>
[AllowAnonymous]
public class ExploreController : Controller
{
    private readonly AppDbContext _db;
    private readonly HomeCatalogCache _catalogCache;

    public ExploreController(AppDbContext db, HomeCatalogCache catalogCache)
    {
        _db = db;
        _catalogCache = catalogCache;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Authenticated homeowners get the personalized dashboard instead.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        var catalog = await _catalogCache.GetAsync(_db, cancellationToken);
        var providers = await LoadPublicProvidersAsync(cancellationToken);

        return View(new ExploreViewModel
        {
            Catalog = catalog,
            Providers = providers
        });
    }

    /// <summary>
    /// Read-only directory of real, registered providers for the public page.
    /// Never returns placeholder/sample data — an empty list simply renders an
    /// empty state.
    /// </summary>
    private async Task<List<ExploreProviderCardViewModel>> LoadPublicProvidersAsync(CancellationToken ct)
    {
        var activeStatuses = new[]
        {
            ProviderRegistrationStatuses.IndorProActive,
            ProviderRegistrationStatuses.Approved
        };

        List<IndorProveedor> providers;
        try
        {
            providers = await _db.IndorProveedores
                .AsNoTracking()
                .Include(p => p.Categorias)
                .Where(p => activeStatuses.Contains(p.RegistrationStatus))
                .Where(p => p.BusinessName != null || p.DbaName != null)
                .OrderByDescending(p => p.FechaActualizacion)
                .Take(60)
                .ToListAsync(ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return [];
        }

        if (providers.Count == 0)
        {
            return [];
        }

        Dictionary<string, string> categoryLabels;
        try
        {
            categoryLabels = await _db.IndorProveedorCategoriasCatalogo
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Id, c => c.LabelEn, ct);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            categoryLabels = new Dictionary<string, string>();
        }

        return providers.Select(p =>
        {
            var name = !string.IsNullOrWhiteSpace(p.DbaName)
                ? p.DbaName!.Trim()
                : (p.BusinessName?.Trim() ?? "INDOR Provider");

            var categoryId = p.Categorias.Select(c => c.CategoriaId).FirstOrDefault();
            categoryLabels.TryGetValue(categoryId ?? "", out var categoryLabel);

            var isVerified = string.Equals(p.RegistrationStatus, ProviderRegistrationStatuses.IndorProActive, StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.RegistrationStatus, ProviderRegistrationStatuses.Approved, StringComparison.OrdinalIgnoreCase);

            return new ExploreProviderCardViewModel
            {
                Name = name,
                CategoryLabel = string.IsNullOrWhiteSpace(categoryLabel) ? p.ServiceDescription : categoryLabel,
                Location = p.PrimaryCity,
                IsVerified = isVerified
            };
        }).ToList();
    }
}
