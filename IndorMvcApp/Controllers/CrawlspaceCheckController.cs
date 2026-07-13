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
public class CrawlspaceCheckController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CrawlspaceCheckController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> CrawlspaceCheckService(int id)
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
    public async Task<IActionResult> CrawlspaceCheckService(CrawlspaceCheckServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.HomeCarePriorityId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        if (model.CheckItems.Count > 0 && string.IsNullOrWhiteSpace(model.CheckAreasSeleccionadas))
        {
            ModelState.AddModelError(nameof(model.CheckAreasSeleccionadas), "Select at least one area to check.");
        }

        if (!ModelState.IsValid)
        {
            var bundleForView = await LoadLandingBundleAsync(model.HomeCarePriorityId);
            if (bundleForView == null) return NotFound();
            var existingForView = await GetActiveSolicitudAsync(userId, model.HomeCarePriorityId);
            return View(BuildServiceViewModel(bundleForView.Value.Priority, bundleForView.Value.Landing, existingForView, model));
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.HomeCarePriorityId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.PreocupacionesSeleccionadas = model.CheckAreasSeleccionadas.Trim();
            solicitud.Estado = "InProgress";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(CrawlspaceCheckSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your crawlspace check request. Please ensure the Crawlspace Check flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CrawlspaceCheckSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new CrawlspaceCheckSetupViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Crawlspace Check",
            Encapsulacion = HasCompletedSetup(solicitud) ? solicitud.Encapsulacion ?? string.Empty : string.Empty,
            Aislamiento = HasCompletedSetup(solicitud) ? solicitud.Aislamiento ?? string.Empty : string.Empty,
            BarreraVapor = HasCompletedSetup(solicitud) ? solicitud.BarreraVapor ?? string.Empty : string.Empty,
            TipoAcceso = HasCompletedSetup(solicitud) ? solicitud.TipoAcceso ?? string.Empty : string.Empty,
            UltimaRevision = HasCompletedSetup(solicitud) ? solicitud.UltimaRevision ?? string.Empty : string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrawlspaceCheckSetup(CrawlspaceCheckSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CrawlspaceCheckService), new { id = solicitud.HomeCarePriorityId });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.Encapsulacion = model.Encapsulacion;
            solicitud.Aislamiento = model.Aislamiento;
            solicitud.BarreraVapor = model.BarreraVapor;
            solicitud.TipoAcceso = model.TipoAcceso;
            solicitud.UltimaRevision = model.UltimaRevision;
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(CrawlspaceCheckSchedule), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your crawlspace details. Please ensure the Crawlspace Check flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CrawlspaceCheckSchedule(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(await BuildScheduleViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrawlspaceCheckSchedule(CrawlspaceCheckScheduleViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CrawlspaceCheckSetup), new { id = solicitud.Id });
        }

        if (string.IsNullOrWhiteSpace(model.PreocupacionesSeleccionadas))
        {
            ModelState.AddModelError(nameof(model.PreocupacionesSeleccionadas), "Select at least one concern to look for.");
        }

        if (model.FechaPreferida.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaPreferida), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            var schedule = await BuildScheduleViewModelAsync(solicitud);
            schedule.PreocupacionesSeleccionadas = model.PreocupacionesSeleccionadas;
            schedule.TimingPreferido = model.TimingPreferido;
            schedule.FechaPreferida = model.FechaPreferida;
            schedule.Notas = model.Notas;
            return View(schedule);
        }

        try
        {
            await EnsureAddressAsync(solicitud);

            solicitud.PreocupacionesSeleccionadas = model.PreocupacionesSeleccionadas;
            solicitud.TimingPreferido = model.TimingPreferido;
            solicitud.RecordatorioAnual = string.Equals(model.TimingPreferido, "YearlyReminder", StringComparison.OrdinalIgnoreCase);
            solicitud.FechaPreferida = model.FechaPreferida.Date;
            solicitud.Notas = model.Notas?.Trim();
            solicitud.PrecioEstimado = CrawlspaceCheckPricingService.GetEstimatedPrice();
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CrawlspaceCheckConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your crawlspace check request. Please ensure the Crawlspace Check flow tables exist in the database and try again.");
            return View(await BuildScheduleViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CrawlspaceCheckConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CrawlspaceCheckSchedule), new { id = solicitud.Id });
        }

        var landing = await _db.CrawlspaceCheckServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return View(new CrawlspaceCheckConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? solicitud.HomeCarePriority?.Nombre ?? "Crawlspace Check",
            EncapsulacionLabel = CrawlspaceCheckDisplayLabels.FormatYesNoNotSure(solicitud.Encapsulacion),
            AislamientoLabel = CrawlspaceCheckDisplayLabels.FormatYesNoNotSure(solicitud.Aislamiento),
            BarreraVaporLabel = CrawlspaceCheckDisplayLabels.FormatYesNoNotSure(solicitud.BarreraVapor),
            ConcernsLabel = CrawlspaceCheckDisplayLabels.FormatConcernsList(solicitud.PreocupacionesSeleccionadas),
            TimingLabel = CrawlspaceCheckDisplayLabels.FormatTiming(solicitud.TimingPreferido, solicitud.RecordatorioAnual),
            ReminderLabel = CrawlspaceCheckDisplayLabels.FormatReminder(
                solicitud.TimingPreferido, solicitud.RecordatorioAnual, solicitud.FechaPreferida),
            FrequencyTexto = landing?.InfoBoxTexto ?? "Recommended frequency: every year, and also after heavy rain or moisture events.",
            ResumenServicioTexto = landing?.ResumenServicioTexto
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, CrawlspaceCheckServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await _db.CrawlspaceCheckServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

        if (landing == null)
        {
            landing = new CrawlspaceCheckServicioLanding
            {
                HomeCarePriorityId = priorityId,
                PageTitle = "Crawlspace Check",
                LandingTitulo = priority.Nombre,
                LandingTagline = "Recommended yearly",
                LandingSubtitulo = "Inspect moisture, insulation, structure, and air quality before small issues become expensive repairs.",
                ImagenUrl = priority.ImagenUrl ?? "/priority-crawlspace-check.png",
                IncluyeItems = "Moisture|Encapsulation|Insulation|Air leaks|Pests|Cracks",
                IncluyeIconos = "fa-droplet|fa-layer-group|fa-scroll|fa-wind|fa-bug|fa-bolt",
                PreocupacionItems = "Standing water|Musty odor|Mold / mildew|Air leaks|Pest signs|Cracks|Pipe leaks|Damaged ducts",
                PreocupacionIconos = "fa-water|fa-wind|fa-bacteria|fa-wind|fa-bug|fa-bolt|fa-faucet-drip|fa-fan",
                InfoBoxTexto = "Check yearly and after heavy rain or moisture events.",
                ResumenServicioTexto = "We'll help inspect moisture, air leaks, insulation, pests, and structural warning signs.",
                CtaTexto = "Start crawlspace check"
            };
        }

        return (priority, landing);
    }

    private static CrawlspaceCheckServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        CrawlspaceCheckServicioLanding landing,
        SolicitudCrawlspaceCheck? existing,
        CrawlspaceCheckServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? priority.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            CheckItems = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos),
            BestPracticeTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto,
            CheckAreasSeleccionadas = posted?.CheckAreasSeleccionadas
                ?? existing?.PreocupacionesSeleccionadas
                ?? string.Join("|", SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos).Select(i => i.Value))
        };

    private async Task<CrawlspaceCheckScheduleViewModel> BuildScheduleViewModelAsync(SolicitudCrawlspaceCheck solicitud)
    {
        var landing = await _db.CrawlspaceCheckServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return new CrawlspaceCheckScheduleViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Crawlspace Check",
            PreocupacionesSeleccionadas = solicitud.PreocupacionesSeleccionadas ?? string.Empty,
            TimingPreferido = solicitud.TimingPreferido ?? string.Empty,
            FechaPreferida = solicitud.FechaPreferida ?? GetDefaultDate(),
            Notas = solicitud.Notas,
            ConcernOptions = SplitPipePairs(landing?.PreocupacionItems, landing?.PreocupacionIconos),
            TipTexto = "Tip: check crawlspaces yearly and after heavy rain or flooding.",
            ResumenServicioTexto = landing?.ResumenServicioTexto
        };
    }

    private async Task EnsureAddressAsync(SolicitudCrawlspaceCheck solicitud)
    {
        if (!string.IsNullOrWhiteSpace(solicitud.DireccionPropiedad))
        {
            return;
        }

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

    private static DateTime GetDefaultDate()
    {
        var date = DateTime.Today.AddDays(7);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static List<CrawlspaceCheckFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new CrawlspaceCheckFeatureItemViewModel
        {
            Text = text,
            Value = ToCheckItemCode(text),
            Icon = index < iconItems.Length ? iconItems[index] : "fa-check"
        }).ToList();
    }

    private static string ToCheckItemCode(string text) => text.Trim() switch
    {
        "Moisture" => "Moisture",
        "Encapsulation" => "Encapsulation",
        "Insulation" => "Insulation",
        "Air leaks" => "AirLeaks",
        "Pests" => "Pests",
        "Cracks" => "Cracks",
        _ => text.Replace(" ", string.Empty).Replace("/", string.Empty)
    };

    private static string? ResolveImageUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.StartsWith('/') ? url : $"/{url}";

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudCrawlspaceCheck?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesCrawlspaceCheck
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudCrawlspaceCheck> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudCrawlspaceCheck? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesCrawlspaceCheck
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudCrawlspaceCheck
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesCrawlspaceCheck.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private static bool HasCompletedSetup(SolicitudCrawlspaceCheck solicitud) =>
        string.Equals(solicitud.Estado, "SetupCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

    private async Task<SolicitudCrawlspaceCheck?> LoadSolicitudForUserAsync(int id)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await _db.SolicitudesCrawlspaceCheck
            .Include(s => s.HomeCarePriority)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudCrawlspaceCheck solicitud)
    {
        if (!solicitud.PropiedadId.HasValue)
        {
            return;
        }

        const string title = "Crawlspace Check";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Encapsulation: {CrawlspaceCheckDisplayLabels.FormatYesNoNotSure(solicitud.Encapsulacion)} | " +
                    $"Insulation: {CrawlspaceCheckDisplayLabels.FormatYesNoNotSure(solicitud.Aislamiento)} | " +
                    $"Concerns: {CrawlspaceCheckDisplayLabels.FormatConcernsList(solicitud.PreocupacionesSeleccionadas)} | " +
                    $"Timing: {CrawlspaceCheckDisplayLabels.FormatTiming(solicitud.TimingPreferido, solicitud.RecordatorioAnual)}";

        if (existing != null)
        {
            existing.DueDate = solicitud.FechaPreferida;
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
                DueDate = solicitud.FechaPreferida,
                Status = "Upcoming",
                Notes = notes,
                FechaCreacion = DateTime.UtcNow
            });
        }
    }
}
