using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HomeownerNearbyNetworkService _nearbyNetworkService;
    private readonly HomeCatalogCache _catalogCache;
    private readonly HomeIndexQueryService _homeIndexQueries;

    public HomeController(
        AppDbContext db,
        IDbContextFactory<AppDbContext> dbFactory,
        UserManager<ApplicationUser> userManager,
        ILogger<HomeController> logger,
        IConfiguration configuration,
        HomeownerNearbyNetworkService nearbyNetworkService,
        HomeCatalogCache catalogCache,
        HomeIndexQueryService homeIndexQueries)
    {
        _db = db;
        _dbFactory = dbFactory;
        _userManager = userManager;
        _logger = logger;
        _configuration = configuration;
        _nearbyNetworkService = nearbyNetworkService;
        _catalogCache = catalogCache;
        _homeIndexQueries = homeIndexQueries;
    }

    [Authorize]
    public async Task<IActionResult> Index(string? view, string? filter, string? q)
    {
        // Index is read-only: it loads data for display and never persists changes on
        // this context, so disable EF change tracking for the whole action. This avoids
        // building change-tracking entries for the hundreds of rows loaded below
        // (inspections/emergencies with their file collections), cutting CPU and memory.
        _db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        var userId = _userManager.GetUserId(User);
        var usuario = await _userManager.GetUserAsync(User);
        ViewBag.UsuarioActual = usuario;

        // Shared, near-static catalog tables (identical for every user) are served
        // from an in-memory snapshot instead of hitting the DB on every request.
        var catalog = await _catalogCache.GetAsync(_db);

        var propiedades = await _db.Propiedades
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        ViewBag.PrimaryPropiedadId = propiedades.FirstOrDefault()?.Id;

        ViewBag.Microservicios = catalog.Microservicios;
        ViewBag.Servicios = catalog.Servicios;
        ViewBag.HomeCarePriorityIds = catalog.HomeCarePriorityIds;
        ViewBag.Inspecciones = catalog.Inspecciones;
        ViewBag.ServiciosEmergencia = catalog.ServiciosEmergencia;

        ViewBag.MovingSetupSection = MovingSetupDisplayService.Build(
            catalog.MovingConfig,
            catalog.MovingServicios,
            catalog.MovingEnlaces,
            Url);

        var prioritiesItems = catalog.HomeCarePriorities;
        ViewBag.HomeCarePrioritiesSection = HomeCarePrioritiesDisplayService.Build(
            catalog.PrioritiesConfig,
            prioritiesItems,
            propiedades.FirstOrDefault()?.Id,
            Url);

        // === Datos para secciÃ³n "More" ===
        ViewBag.Planes = catalog.PlanesMembresia;

        var propIds = propiedades.Select(p => p.Id).ToList();
        // Home/Index only renders the "More" stats and the bell notifications, so load just
        // those instead of the full inspection/emergency/payment/history dataset (which is only
        // used by the Perfil pages). This avoids ~40 wasted DB round-trips per Home load.
        var homeEssentials = await _homeIndexQueries.LoadHomeEssentialsAsync(userId!, propIds);

        ViewBag.PlanesInternet = catalog.PlanesInternet;
        HomeIndexViewDataApplier.ApplyEssentialsToViewBag(ViewBag, homeEssentials, usuario, propiedades.Count, Url);

        var homeReturnUrl = Url.Action(nameof(Index), "Home") + "#section-myhome";
        if (HttpContext.Session.GetString(HouseFactPreviewContext.ReturnUrlSessionKey) != homeReturnUrl)
        {
            HttpContext.Session.SetString(HouseFactPreviewContext.ReturnUrlSessionKey, homeReturnUrl);
        }

        var primaryPropiedad = propiedades.FirstOrDefault();
        var previewInfo = HouseFactPreviewContext.Load(HttpContext.Session);
        HouseFactProfileViewModel? houseFactProfile = null;
        var houseFactPreview = false;
        HomeDashboardData? primaryDashboardData = null;
        var scheduleTask = ScheduleDisplayService.BuildAsync(_dbFactory, userId!, primaryPropiedad?.Id, Url);

        if (primaryPropiedad != null)
        {
            try
            {
                var primaryInfo = MyHomeDisplayService.DeserializeProperty(primaryPropiedad);
                var dashboardData = await HomeDashboardDataService.LoadAsync(_db, primaryPropiedad.Id);
                primaryDashboardData = dashboardData;

                var notificationCount = dashboardData.Mantenimiento.Count(m =>
                    m.DueDate.HasValue
                    && m.Status != "Completed"
                    && (m.DueDate.Value.Date - DateTime.Today).Days <= 30);

                ViewBag.HomeDashboard = HomeDashboardDisplayService.Build(
                    usuario,
                    primaryPropiedad,
                    primaryInfo,
                    dashboardData.HvacRecord,
                    dashboardData.WaterHeaterRecord,
                    dashboardData.Mantenimiento,
                    dashboardData.Documentos,
                    dashboardData.Historial,
                    notificationCount,
                    Url,
                    prioritiesItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build home dashboard for propiedad {PropiedadId}", primaryPropiedad.Id);
                var primaryInfo = MyHomeDisplayService.DeserializeProperty(primaryPropiedad);
                ViewBag.HomeDashboard = HomeDashboardDisplayService.BuildBasic(
                    usuario,
                    primaryPropiedad,
                    primaryInfo,
                    Url,
                    prioritiesItems);
            }
        }
        else
        {
            ViewBag.HomeDashboard = HomeDashboardDisplayService.BuildEmpty(usuario);
        }

        var primaryPropiedadForNetwork = propiedades.FirstOrDefault();
        var primaryInfoForNetwork = primaryPropiedadForNetwork != null
            ? MyHomeDisplayService.DeserializeProperty(primaryPropiedadForNetwork)
            : null;
        var networkNotificationCount = (ViewBag.HomeDashboard as HomeDashboardViewModel)?.NotificationCount ?? 0;

        try
        {
            ViewBag.HomeNearbyNetwork = await _nearbyNetworkService.BuildAsync(
                primaryPropiedadForNetwork,
                primaryInfoForNetwork,
                networkNotificationCount,
                view,
                filter,
                q,
                Url,
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build homeowner nearby network feed");
            ViewBag.HomeNearbyNetwork = new HomeownerNearbyNetworkViewModel
            {
                HasProperty = primaryPropiedadForNetwork != null,
                PropiedadId = primaryPropiedadForNetwork?.Id ?? 0,
                NotificationCount = networkNotificationCount,
                ActiveView = string.Equals(view, "map", StringComparison.OrdinalIgnoreCase) ? "map" : "feed",
                ActiveFilter = filter ?? "All",
                SearchQuery = q
            };
        }

        if (primaryPropiedad != null && !string.IsNullOrWhiteSpace(primaryPropiedad.AttomRawJson))
        {
            var info = MyHomeDisplayService.DeserializeProperty(primaryPropiedad);
            houseFactProfile = HouseFactDisplayService.BuildProfile(
                primaryPropiedad.AttomRawJson,
                primaryPropiedad.AttomSyncStatus ?? info?.DataSource,
                info?.FormattedAddress ?? primaryPropiedad.Direccion);
        }
        else if (previewInfo != null)
        {
            houseFactProfile = HouseFactDisplayService.BuildProfile(
                previewInfo.AttomRawJson,
                previewInfo.DataSource,
                previewInfo.FormattedAddress);
            houseFactPreview = true;
        }

        if (houseFactProfile != null)
        {
            houseFactProfile.PropertyImageUrl = PropertyImageResolver.Resolve(
                primaryDashboardData?.Documentos,
                primaryDashboardData?.HvacRecord?.LabelImagePath,
                primaryDashboardData?.WaterHeaterRecord?.LabelImagePath);
            houseFactProfile.NeedsReviewCount = HouseFactOverviewBuilder.CountNeedsReview(houseFactProfile);
            houseFactProfile.ShowSuccessBadge = string.Equals(
                primaryPropiedad?.AttomSyncStatus,
                "Success",
                StringComparison.OrdinalIgnoreCase)
                || houseFactProfile.HasData;
        }

        ViewBag.HouseFactProfile = houseFactProfile;
        ViewBag.HouseFactPreviewMode = houseFactPreview;
        ViewBag.HouseFactPropiedadId = primaryPropiedad?.Id;

        ViewBag.ScheduleSection = await scheduleTask;

        return View(propiedades);
    }

    /// <summary>Full list of Home Care Guide maintenance tasks (not MyHome maintenance log).</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> HomeCareGuide(int? id)
    {
        var userId = _userManager.GetUserId(User);
        int? propiedadId = id;
        if (!propiedadId.HasValue)
        {
            propiedadId = await _db.Propiedades
                .Where(p => p.UserId == userId && p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();
        }
        else if (!await _db.Propiedades.AnyAsync(p => p.Id == propiedadId && p.UserId == userId && p.Activo))
        {
            return NotFound();
        }

        var config = await _db.HomeCarePrioritiesConfig.AsNoTracking().FirstOrDefaultAsync(c => c.Activo);
        var items = await _db.HomeCarePriorities.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ThenBy(p => p.Id)
            .ToListAsync();

        var section = HomeCarePrioritiesDisplayService.Build(config, items, propiedadId, Url);
        if (section == null)
        {
            return RedirectToAction(nameof(Index));
        }

        section.ViewAllUrl = null;
        return View(section);
    }

    /// <summary>Full grid of 24/7 emergency services (not a scroll jump on Home).</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EmergencyGuide()
    {
        var items = await _db.ServiciosEmergencia.AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync();

        var model = EmergencyServicesDisplayService.BuildGuide(items, Url);
        if (model == null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> NearbyNetworkMapData(
        double? lat,
        double? lng,
        string? addressQuery,
        string? filter,
        CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        var propiedad = await _db.Propiedades
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (propiedad == null)
        {
            return NotFound();
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var data = await _nearbyNetworkService.GetMapDataAsync(
            propiedad,
            info,
            lat,
            lng,
            addressQuery,
            filter,
            cancellationToken);

        if (data == null)
        {
            return NotFound(new { error = "Address not found." });
        }

        return Json(data);
    }

    public IActionResult Privacy()
    {
        return RedirectToAction("Privacy", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var showDetails = _configuration.GetValue("Diagnostics:ShowDetailedErrors", true);
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var error = feature?.Error;

        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ShowDetails = showDetails,
            Path = feature?.Path ?? HttpContext.Request.Path
        };

        if (showDetails && error != null)
        {
            model.ExceptionType = error.GetType().FullName;
            model.ExceptionMessage = error.Message;
            model.InnerExceptionMessage = error.InnerException?.Message;
            model.StackTrace = error.StackTrace;
            _logger.LogError(error, "Unhandled exception at {Path}", model.Path);
        }

        return View(model);
    }
}
