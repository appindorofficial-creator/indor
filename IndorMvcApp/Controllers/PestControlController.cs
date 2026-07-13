using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class PestControlController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PestControlController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> PestControlService(int id)
    {
        var bundle = await LoadLandingBundleAsync(id);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var existing = await GetActiveSolicitudAsync(userId, id);
        return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, existing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PestControlService(PestControlServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.HomeCarePriorityId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.HomeCarePriorityId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.TipoAccionInicial = model.TipoAccionInicial;
            solicitud.TipoServicio = MapInitialToServiceType(model.TipoAccionInicial);
            solicitud.Estado = "ServiceSelected";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PestControlSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your pest control request. Please ensure the Pest Control flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PestControlSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var setupEntered = string.Equals(solicitud.Estado, "SetupCompleted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

        return View(new PestControlSetupViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Pest Control Check",
            UltimoServicio = setupEntered ? (solicitud.UltimoServicio ?? string.Empty) : string.Empty,
            SignosSeleccionados = setupEntered ? (solicitud.SignosSeleccionados ?? string.Empty) : string.Empty,
            AreasPreocupacion = setupEntered ? (solicitud.AreasPreocupacion ?? string.Empty) : string.Empty,
            MascotasONinos = setupEntered ? (solicitud.MascotasONinos ?? string.Empty) : string.Empty,
            Notas = setupEntered ? solicitud.Notas : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PestControlSetup(PestControlSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PestControlService), new { id = solicitud.HomeCarePriorityId });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.UltimoServicio = model.UltimoServicio;
            solicitud.SignosSeleccionados = model.SignosSeleccionados;
            solicitud.AreasPreocupacion = model.AreasPreocupacion;
            solicitud.MascotasONinos = model.MascotasONinos;
            solicitud.Notas = model.Notas?.Trim();
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PestControlPlan), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your home details. Please ensure the Pest Control flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PestControlPlan(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(await BuildPlanViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PestControlPlan(PestControlPlanViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PestControlSetup), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            var vm = await BuildPlanViewModelAsync(solicitud);
            vm.TipoServicio = model.TipoServicio;
            vm.TimingPreferido = model.TimingPreferido;
            return View(vm);
        }

        try
        {
            await EnsureAddressAsync(solicitud);

            solicitud.TipoServicio = model.TipoServicio;
            solicitud.TimingPreferido = model.TimingPreferido;
            solicitud.RecordatorioAnual = string.Equals(model.TimingPreferido, "EveryYearSpring", StringComparison.OrdinalIgnoreCase)
                || string.Equals(model.TipoServicio, "ReminderOnly", StringComparison.OrdinalIgnoreCase);
            solicitud.PrecioEstimado = PestControlPricingService.GetEstimatedPrice(model.TipoServicio);
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PestControlConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your pest control plan. Please ensure the Pest Control flow tables exist in the database and try again.");
            return View(await BuildPlanViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PestControlConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PestControlPlan), new { id = solicitud.Id });
        }

        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);

        return View(new PestControlConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? "Pest Control Check",
            ServiceLabel = PestControlDisplayLabels.FormatServiceType(solicitud.TipoServicio),
            TimingLabel = PestControlDisplayLabels.FormatTiming(solicitud.TimingPreferido),
            ConcernsLabel = PestControlDisplayLabels.FormatPipeList(solicitud.AreasPreocupacion, PestControlDisplayLabels.FormatArea),
            PetsChildrenLabel = PestControlDisplayLabels.FormatYesNo(solicitud.MascotasONinos),
            StatusLabel = "Reminder and service saved",
            WhyYearlyItems = SplitPipePairs(landing?.WhyYearlyItems, landing?.WhyYearlyIconos)
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, PestControlServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await GetLandingAsync(priorityId);
        landing ??= new PestControlServicioLanding
        {
            HomeCarePriorityId = priorityId,
            PageTitle = "Pest Control Check",
            LandingTitulo = priority.Nombre,
            LandingSubtitulo = "Recommended yearly to help catch problems early and protect your home.",
            ImagenUrl = priority.ImagenUrl ?? "/priority-pest-control.png",
            WhyItMattersItems = "Spot termites and other pests early|Check for moisture, nests, droppings, and entry points|Help protect wood, insulation, and indoor air quality",
            WhyItMattersIconos = "fa-bug|fa-droplet|fa-shield-halved",
            BestForTexto = "Best for: annual inspections, prevention plans, and homes with past pest activity.",
            InfoPlanTexto = "Annual checks are most helpful for homes with past pest activity, moisture issues, wood-to-soil contact, or cracks around the home.",
            WhyYearlyItems = "Helps catch termite or rodent issues early|Checks for moisture, nests, and entry points|Supports ongoing home protection",
            WhyYearlyIconos = "fa-bug|fa-droplet|fa-shield-halved",
            CtaTexto = "Continue"
        };

        return (priority, landing);
    }

    private async Task<PestControlServicioLanding?> GetLandingAsync(int priorityId) =>
        await _db.PestControlServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

    private static PestControlServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        PestControlServicioLanding landing,
        SolicitudPestControl? existing,
        PestControlServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingSubtitulo = landing.LandingSubtitulo ?? priority.Subtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            WhyItMattersItems = SplitPipePairs(landing.WhyItMattersItems, landing.WhyItMattersIconos),
            BestForTexto = landing.BestForTexto,
            CtaTexto = landing.CtaTexto,
            TipoAccionInicial = existing?.TipoAccionInicial ?? posted?.TipoAccionInicial ?? string.Empty
        };

    private async Task<PestControlPlanViewModel> BuildPlanViewModelAsync(SolicitudPestControl solicitud)
    {
        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);

        return new PestControlPlanViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Pest Control Check",
            InfoPlanTexto = landing?.InfoPlanTexto,
            TipoServicio = solicitud.TipoServicio ?? string.Empty,
            TimingPreferido = solicitud.TimingPreferido ?? string.Empty
        };
    }

    private static string MapInitialToServiceType(string? initialAction) =>
        string.Equals(initialAction, "ScheduleService", StringComparison.OrdinalIgnoreCase)
            ? "AnnualInspection"
            : "ReminderOnly";

    private static List<PestControlFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new PestControlFeatureItemViewModel
        {
            Text = text,
            Icon = index < iconItems.Length ? iconItems[index] : "fa-check"
        }).ToList();
    }

    private static string? ResolveImageUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.StartsWith('/') ? url : $"/{url}";

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudPestControl?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesPestControl
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudPestControl> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudPestControl? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesPestControl
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudPestControl
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                TipoAccionInicial = string.Empty,
                TipoServicio = string.Empty,
                TimingPreferido = string.Empty
            };
            _db.SolicitudesPestControl.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudPestControl?> LoadSolicitudForUserAsync(int id)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await _db.SolicitudesPestControl
            .Include(s => s.HomeCarePriority)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task EnsureAddressAsync(SolicitudPestControl solicitud)
    {
        if (!string.IsNullOrWhiteSpace(solicitud.DireccionPropiedad)) return;

        var userId = RequireUserId();
        if (userId == null) return;

        if (solicitud.PropiedadId.HasValue)
        {
            solicitud.DireccionPropiedad = await _db.Propiedades.AsNoTracking()
                .Where(p => p.Id == solicitud.PropiedadId)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrWhiteSpace(solicitud.DireccionPropiedad))
        {
            solicitud.DireccionPropiedad = await _db.Propiedades.AsNoTracking()
                .Where(p => p.UserId == userId && p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();
        }
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudPestControl solicitud)
    {
        if (!solicitud.PropiedadId.HasValue) return;

        const string title = "Pest Control Check";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Service: {PestControlDisplayLabels.FormatServiceType(solicitud.TipoServicio)} | " +
                    $"Timing: {PestControlDisplayLabels.FormatTiming(solicitud.TimingPreferido)} | " +
                    $"Areas: {PestControlDisplayLabels.FormatPipeList(solicitud.AreasPreocupacion, PestControlDisplayLabels.FormatArea)} | " +
                    $"Pets/children: {PestControlDisplayLabels.FormatYesNo(solicitud.MascotasONinos)}";

        var dueDate = PestControlDisplayLabels.GetDueDate(solicitud.TimingPreferido);

        if (existing != null)
        {
            existing.DueDate = dueDate;
            existing.Status = "Upcoming";
            existing.Notes = notes;
            existing.FechaActualizacion = DateTime.UtcNow;
        }
        else
        {
            _db.PropiedadMantenimiento.Add(new PropiedadMantenimiento
            {
                PropiedadId = solicitud.PropiedadId.Value,
                Title = title,
                DueDate = dueDate,
                Status = "Upcoming",
                Notes = notes,
                FechaCreacion = DateTime.UtcNow
            });
        }
    }
}
