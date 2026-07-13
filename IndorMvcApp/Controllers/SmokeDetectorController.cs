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
public class SmokeDetectorController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SmokeDetectorController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> SmokeDetectorService(int id)
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
    public async Task<IActionResult> SmokeDetectorService(SmokeDetectorServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.HomeCarePriorityId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.HomeCarePriorityId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.Estado = "InProgress";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(SmokeDetectorSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your smoke detector reminder setup. Please ensure the Smoke Detector flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SmokeDetectorSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var setupComplete = HasCompletedSetup(solicitud);

        return View(new SmokeDetectorSetupViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Smoke / CO Check",
            CantidadAlarmas = setupComplete ? solicitud.CantidadAlarmas ?? string.Empty : string.Empty,
            UbicacionesSeleccionadas = setupComplete ? solicitud.UbicacionesSeleccionadas ?? string.Empty : string.Empty,
            TiposAlarmas = setupComplete ? solicitud.TiposAlarmas ?? string.Empty : string.Empty,
            UltimaPrueba = setupComplete ? solicitud.UltimaPrueba ?? string.Empty : string.Empty,
            UltimoCambioBateria = setupComplete ? solicitud.UltimoCambioBateria ?? string.Empty : string.Empty,
            AnioInstalacion = setupComplete ? solicitud.AnioInstalacion : null,
            AnioInstalacionDesconocido = setupComplete && solicitud.AnioInstalacionDesconocido,
            ProblemasSeleccionados = setupComplete ? solicitud.ProblemasSeleccionados ?? string.Empty : string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SmokeDetectorSetup(SmokeDetectorSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SmokeDetectorService), new { id = solicitud.HomeCarePriorityId });
        }

        if (!model.AnioInstalacionDesconocido
            && model.AnioInstalacion.HasValue
            && (model.AnioInstalacion.Value < 1990 || model.AnioInstalacion.Value > DateTime.Today.Year))
        {
            ModelState.AddModelError(nameof(model.AnioInstalacion), "Please enter a valid install year.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.CantidadAlarmas = model.CantidadAlarmas;
            solicitud.UbicacionesSeleccionadas = model.UbicacionesSeleccionadas;
            solicitud.TiposAlarmas = model.TiposAlarmas;
            solicitud.UltimaPrueba = model.UltimaPrueba;
            solicitud.UltimoCambioBateria = model.UltimoCambioBateria;
            solicitud.AnioInstalacion = model.AnioInstalacionDesconocido ? null : model.AnioInstalacion;
            solicitud.AnioInstalacionDesconocido = model.AnioInstalacionDesconocido;
            solicitud.ProblemasSeleccionados = model.ProblemasSeleccionados;
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(SmokeDetectorReminders), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your alarm details. Please ensure the Smoke Detector flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SmokeDetectorReminders(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildRemindersViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SmokeDetectorReminders(SmokeDetectorRemindersViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SmokeDetectorSetup), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            var vm = BuildRemindersViewModel(solicitud);
            vm.RecordatorioMensual = model.RecordatorioMensual;
            vm.RecordatorioBateriaAnual = model.RecordatorioBateriaAnual;
            vm.RecordatorioReemplazo10Anos = model.RecordatorioReemplazo10Anos;
            vm.RecordatorioRevisionEstacional = model.RecordatorioRevisionEstacional;
            vm.TipoAccionAyuda = model.TipoAccionAyuda;
            return View(vm);
        }

        try
        {
            await EnsureAddressAsync(solicitud);

            var confirmDate = DateTime.Now;
            var installReference = SmokeDetectorDisplayLabels.ResolveInstallReferenceDate(
                solicitud.AnioInstalacion,
                solicitud.AnioInstalacionDesconocido,
                confirmDate);

            solicitud.RecordatorioMensual = model.RecordatorioMensual;
            solicitud.RecordatorioBateriaAnual = model.RecordatorioBateriaAnual;
            solicitud.RecordatorioReemplazo10Anos = model.RecordatorioReemplazo10Anos;
            solicitud.RecordatorioRevisionEstacional = model.RecordatorioRevisionEstacional;
            solicitud.TipoAccionAyuda = model.TipoAccionAyuda;
            solicitud.FechaInstalacionReferencia = installReference;
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = confirmDate;
            solicitud.FechaActualizacion = confirmDate;

            await UpsertMaintenanceTasksAsync(solicitud, confirmDate);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(SmokeDetectorConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your reminders. Please ensure the Smoke Detector flow tables exist in the database and try again.");
            return View(BuildRemindersViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SmokeDetectorConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SmokeDetectorReminders), new { id = solicitud.Id });
        }

        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);
        var reference = solicitud.FechaInstalacionReferencia ?? solicitud.FechaConfirmacion ?? DateTime.Today;
        var confirmDate = solicitud.FechaConfirmacion ?? DateTime.Today;

        return View(new SmokeDetectorConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.PageTitle ?? "Smoke / CO Check",
            NextMonthlyTestLabel = solicitud.RecordatorioMensual
                ? SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetNextMonthlyTest(reference, confirmDate))
                : "Off",
            NextBatteryLabel = solicitud.RecordatorioBateriaAnual
                ? SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetNextBatteryReminder(reference))
                : "Off",
            ReplacementLabel = solicitud.RecordatorioReemplazo10Anos
                ? $"Replace by {SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetReplacementDate(reference))}"
                : "Off",
            ShowSafetyVisit = string.Equals(solicitud.TipoAccionAyuda, "ScheduleSafetyCheck", StringComparison.OrdinalIgnoreCase)
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, SmokeDetectorServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await GetLandingAsync(priorityId);
        landing ??= new SmokeDetectorServicioLanding
        {
            HomeCarePriorityId = priorityId,
            PageTitle = "Smoke / CO Check",
            LandingTitulo = "Protect your home and the people in it.",
            LandingSubtitulo = "Smoke and carbon monoxide alarms are your first line of defense. Regular checks keep them ready when it matters most.",
            ImagenUrl = priority.ImagenUrl ?? "/priority-smoke-detector.png",
            TrackItems = "Test monthly|Battery check yearly|Replace alarm every 10 years",
            TrackDescriptions = "Press the test button to make sure your alarm is working.|Check and replace batteries at least once a year.|Alarms should be replaced 10 years from the install date.",
            TrackIconos = "fa-calendar|fa-battery-full|fa-rotate",
            WhereTrackItems = "Bedroom alarms|Hallway alarms|Living area alarms|CO combo units",
            WhereTrackIconos = "fa-bed|fa-door-open|fa-couch|fa-circle",
            ReminderBannerTexto = "INDOR will remind you when it's time to test, change batteries, or replace older alarms.",
            CtaTexto = "Start reminder setup"
        };

        return (priority, landing);
    }

    private async Task<SmokeDetectorServicioLanding?> GetLandingAsync(int priorityId) =>
        await _db.SmokeDetectorServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

    private static SmokeDetectorServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        SmokeDetectorServicioLanding landing,
        SolicitudSmokeDetector? existing,
        SmokeDetectorServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingSubtitulo = landing.LandingSubtitulo ?? priority.Subtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            TrackItems = SplitTrackItems(landing.TrackItems, landing.TrackDescriptions, landing.TrackIconos),
            WhereTrackItems = SplitPipePairs(landing.WhereTrackItems, landing.WhereTrackIconos),
            ReminderBannerTexto = landing.ReminderBannerTexto,
            CtaTexto = landing.CtaTexto
        };

    private SmokeDetectorRemindersViewModel BuildRemindersViewModel(SolicitudSmokeDetector solicitud)
    {
        var reference = SmokeDetectorDisplayLabels.ResolveInstallReferenceDate(
            solicitud.AnioInstalacion,
            solicitud.AnioInstalacionDesconocido,
            DateTime.Today);
        var fromDate = DateTime.Today;

        return new SmokeDetectorRemindersViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Smoke / CO Check",
            AlarmCountLabel = SmokeDetectorDisplayLabels.FormatAlarmCount(solicitud.CantidadAlarmas),
            AlarmTypeLabel = SmokeDetectorDisplayLabels.FormatPrimaryAlarmType(solicitud.TiposAlarmas),
            LocationsLabel = SmokeDetectorDisplayLabels.FormatPipeList(solicitud.UbicacionesSeleccionadas, SmokeDetectorDisplayLabels.FormatLocation),
            InstalledLabel = SmokeDetectorDisplayLabels.FormatInstalledDate(solicitud.AnioInstalacion, solicitud.AnioInstalacionDesconocido, reference),
            NextMonthlyTestLabel = SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetNextMonthlyTest(reference, fromDate)),
            NextBatteryLabel = SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetNextBatteryReminder(reference)),
            NextReplacementLabel = SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetReplacementDate(reference)),
            NextSeasonalLabel = SmokeDetectorDisplayLabels.FormatDate(SmokeDetectorDisplayLabels.GetNextSeasonalReview(fromDate)),
            RecordatorioMensual = solicitud.RecordatorioMensual,
            RecordatorioBateriaAnual = solicitud.RecordatorioBateriaAnual,
            RecordatorioReemplazo10Anos = solicitud.RecordatorioReemplazo10Anos,
            RecordatorioRevisionEstacional = solicitud.RecordatorioRevisionEstacional,
            TipoAccionAyuda = solicitud.TipoAccionAyuda ?? string.Empty
        };
    }

    private static List<SmokeDetectorFeatureItemViewModel> SplitTrackItems(string? titles, string? descriptions, string? icons)
    {
        var titleItems = string.IsNullOrWhiteSpace(titles)
            ? Array.Empty<string>()
            : titles.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var descItems = string.IsNullOrWhiteSpace(descriptions)
            ? Array.Empty<string>()
            : descriptions.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return titleItems.Select((text, index) => new SmokeDetectorFeatureItemViewModel
        {
            Text = text,
            Subtext = index < descItems.Length ? descItems[index] : null,
            Icon = index < iconItems.Length ? iconItems[index] : "fa-check"
        }).ToList();
    }

    private static List<SmokeDetectorFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new SmokeDetectorFeatureItemViewModel
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

    private async Task<SolicitudSmokeDetector?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesSmokeDetector
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudSmokeDetector> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudSmokeDetector? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesSmokeDetector
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudSmokeDetector
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                RecordatorioMensual = false,
                RecordatorioBateriaAnual = false,
                RecordatorioReemplazo10Anos = false,
                RecordatorioRevisionEstacional = false,
                TipoAccionAyuda = string.Empty
            };
            _db.SolicitudesSmokeDetector.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private static bool HasCompletedSetup(SolicitudSmokeDetector solicitud) =>
        string.Equals(solicitud.Estado, "SetupCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

    private async Task<SolicitudSmokeDetector?> LoadSolicitudForUserAsync(int id)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await _db.SolicitudesSmokeDetector
            .Include(s => s.HomeCarePriority)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task EnsureAddressAsync(SolicitudSmokeDetector solicitud)
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

    private async Task UpsertMaintenanceTasksAsync(SolicitudSmokeDetector solicitud, DateTime confirmDate)
    {
        if (!solicitud.PropiedadId.HasValue) return;

        var reference = solicitud.FechaInstalacionReferencia ?? confirmDate;
        const string title = "Smoke / CO Check";

        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var dueDate = solicitud.RecordatorioMensual
            ? SmokeDetectorDisplayLabels.GetNextMonthlyTest(reference, confirmDate)
            : solicitud.RecordatorioBateriaAnual
                ? SmokeDetectorDisplayLabels.GetNextBatteryReminder(reference)
                : SmokeDetectorDisplayLabels.GetNextSeasonalReview(confirmDate);

        var notes = $"Alarms: {SmokeDetectorDisplayLabels.FormatAlarmCount(solicitud.CantidadAlarmas)} | " +
                    $"Type: {SmokeDetectorDisplayLabels.FormatPrimaryAlarmType(solicitud.TiposAlarmas)} | " +
                    $"Locations: {SmokeDetectorDisplayLabels.FormatPipeList(solicitud.UbicacionesSeleccionadas, SmokeDetectorDisplayLabels.FormatLocation)} | " +
                    $"Help: {SmokeDetectorDisplayLabels.FormatHelpAction(solicitud.TipoAccionAyuda)}";

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
