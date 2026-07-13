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
public class TrashController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public TrashController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> TrashService(int id)
    {
        var bundle = await LoadLandingBundleAsync(id);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var existing = await GetActiveSolicitudAsync(userId, id);
        return View(BuildServiceViewModel(bundle.Value.Microservicio, bundle.Value.Landing, existing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrashService(TrashServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.MicroservicioId);
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
            var propiedad = propiedadId.HasValue
                ? await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId)
                : null;

            var solicitud = await GetOrCreateSolicitudAsync(userId, model.MicroservicioId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = propiedad?.Direccion;
            solicitud.Estado = "InProgress";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(TrashSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your trash service request. Please ensure the trash flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Microservicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> TrashSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildSetupViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrashSetup(TrashSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TrashService), new { id = solicitud.MicroservicioId });
        }

        if (string.IsNullOrWhiteSpace(model.BinsSeleccionados))
        {
            ModelState.AddModelError(nameof(model.BinsSeleccionados), "Select at least one bin type.");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildSetupViewModel(solicitud, model));
        }

        try
        {
            solicitud.BinsSeleccionados = model.BinsSeleccionados;
            solicitud.CantidadBins = model.CantidadBins;
            solicitud.Frecuencia = model.Frecuencia;
            solicitud.DiaRecoleccion = model.DiaRecoleccion;
            solicitud.PrecioMensual = TrashPricingService.GetMonthlyPrice(model.CantidadBins);
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(TrashHelp), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your trash setup. Please ensure the trash flow tables exist in the database and try again.");
            return View(BuildSetupViewModel(solicitud, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> TrashHelp(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var helpSaved = string.Equals(solicitud.Estado, "HelpCompleted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

        return View(new TrashHelpViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            TipoAyuda = helpSaved ? (solicitud.TipoAyuda ?? string.Empty) : string.Empty,
            RecordatorioCuando = helpSaved ? (solicitud.RecordatorioCuando ?? string.Empty) : string.Empty,
            VentanaRecoleccion = helpSaved ? (solicitud.VentanaRecoleccion ?? string.Empty) : string.Empty,
            NotasEspeciales = helpSaved ? solicitud.NotasEspeciales : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrashHelp(TrashHelpViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TrashSetup), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.TipoAyuda = model.TipoAyuda;
            solicitud.RecordatorioCuando = model.RecordatorioCuando;
            solicitud.VentanaRecoleccion = model.VentanaRecoleccion;
            solicitud.NotasEspeciales = model.NotasEspeciales?.Trim();
            solicitud.Estado = "HelpCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(TrashReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your preferences. Please ensure the trash flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> TrashReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var landing = await _db.TrashServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == solicitud.MicroservicioId);

        return View(BuildReviewViewModel(solicitud, landing?.InfoBoxTexto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrashReview(int id, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TrashHelp), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertProgramacionAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(TrashConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not activate your service. Please ensure the trash flow tables exist in the database and try again.");
            var landing = await _db.TrashServicioLanding.AsNoTracking()
                .FirstOrDefaultAsync(l => l.MicroservicioId == solicitud.MicroservicioId);
            return View(BuildReviewViewModel(solicitud, landing?.InfoBoxTexto));
        }
    }

    [HttpGet]
    public async Task<IActionResult> TrashConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TrashReview), new { id = solicitud.Id });
        }

        return View(new TrashConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            BinsLabel = TrashDisplayLabels.FormatBinsList(solicitud.BinsSeleccionados),
            ServiceTypeLabel = TrashDisplayLabels.FormatHelpType(solicitud.TipoAyuda),
            FrequencyLabel = TrashDisplayLabels.FormatFrequency(solicitud.Frecuencia),
            PickupDayLabel = TrashDisplayLabels.FormatPickupDay(solicitud.DiaRecoleccion),
            ReminderLabel = TrashDisplayLabels.FormatReminderWhen(solicitud.RecordatorioCuando),
            PrecioMensual = solicitud.PrecioMensual ?? TrashPricingService.GetMonthlyPrice(solicitud.CantidadBins),
            Moneda = solicitud.Microservicio?.Moneda ?? "USD"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(Microservicio Microservicio, TrashServicioLanding Landing)?> LoadLandingBundleAsync(int microservicioId)
    {
        var microservicio = await _db.Microservicios.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == microservicioId && m.Activo);
        if (microservicio == null) return null;

        var landing = await _db.TrashServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == microservicioId && l.Activo);

        if (landing == null)
        {
            landing = new TrashServicioLanding
            {
                MicroservicioId = microservicioId,
                PageTitle = "Trash Day Assistant",
                LandingTitulo = microservicio.Nombre,
                LandingTagline = microservicio.Subtitulo,
                LandingSubtitulo = microservicio.DescripcionCompleta ?? microservicio.Descripcion,
                ImagenUrl = microservicio.ImagenUrl,
                PrecioDesde = microservicio.Valor > 0 ? microservicio.Valor : 20,
                PrecioTexto = $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 20):0} /mo"
            };
        }

        return (microservicio, landing);
    }

    private static TrashServiceViewModel BuildServiceViewModel(
        Microservicio microservicio,
        TrashServicioLanding landing,
        SolicitudTrash? existing,
        TrashServiceViewModel? posted = null)
    {
        var items = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos);
        if (items.Count == 0)
        {
            items = SplitPipePairs(microservicio.Incluye, null);
        }

        return new TrashServiceViewModel
        {
            MicroservicioId = microservicio.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? microservicio.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? microservicio.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : (microservicio.Valor > 0 ? microservicio.Valor : 20),
            PrecioTexto = landing.PrecioTexto ?? $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 20):0} /mo",
            IncludedItems = items,
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto
        };
    }

    private TrashSetupViewModel BuildSetupViewModel(SolicitudTrash solicitud, TrashSetupViewModel? posted = null)
    {
        var setupSaved = !string.Equals(solicitud.Estado, "InProgress", StringComparison.OrdinalIgnoreCase);
        return new()
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            BinsSeleccionados = posted?.BinsSeleccionados
                ?? (setupSaved ? solicitud.BinsSeleccionados : null)
                ?? string.Empty,
            CantidadBins = posted?.CantidadBins
                ?? (setupSaved ? solicitud.CantidadBins : null)
                ?? string.Empty,
            Frecuencia = posted?.Frecuencia
                ?? (setupSaved ? solicitud.Frecuencia : null)
                ?? string.Empty,
            DiaRecoleccion = posted?.DiaRecoleccion
                ?? (setupSaved ? solicitud.DiaRecoleccion : null)
                ?? string.Empty,
            PrecioMensual = TrashPricingService.GetMonthlyPrice(
                posted?.CantidadBins ?? (setupSaved ? solicitud.CantidadBins : null))
        };
    }

    private static TrashReviewViewModel BuildReviewViewModel(SolicitudTrash solicitud, string? infoBox) =>
        new()
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            BinsLabel = TrashDisplayLabels.FormatBinsList(solicitud.BinsSeleccionados),
            ServiceTypeLabel = TrashDisplayLabels.FormatHelpType(solicitud.TipoAyuda),
            FrequencyLabel = TrashDisplayLabels.FormatFrequency(solicitud.Frecuencia),
            PickupDayLabel = TrashDisplayLabels.FormatPickupDay(solicitud.DiaRecoleccion),
            ReminderLabel = TrashDisplayLabels.FormatReminderWhen(solicitud.RecordatorioCuando),
            PickupWindowLabel = TrashDisplayLabels.FormatPickupWindow(solicitud.VentanaRecoleccion),
            NotasEspeciales = solicitud.NotasEspeciales,
            PrecioMensual = solicitud.PrecioMensual ?? TrashPricingService.GetMonthlyPrice(solicitud.CantidadBins),
            Moneda = solicitud.Microservicio?.Moneda ?? "USD",
            InfoBoxTexto = infoBox
        };

    private static List<TrashFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new TrashFeatureItemViewModel
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

    private async Task<SolicitudTrash?> GetActiveSolicitudAsync(string userId, int microservicioId) =>
        await _db.SolicitudesTrash
            .Where(s => s.UserId == userId
                        && s.MicroservicioId == microservicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudTrash> GetOrCreateSolicitudAsync(
        string userId,
        int microservicioId,
        int? solicitudId)
    {
        SolicitudTrash? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesTrash
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, microservicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudTrash
            {
                UserId = userId,
                MicroservicioId = microservicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesTrash.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudTrash?> LoadSolicitudForUserAsync(int id)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await _db.SolicitudesTrash
            .Include(s => s.Microservicio)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertProgramacionAsync(SolicitudTrash solicitud)
    {
        var nextPickup = GetNextPickupDate(solicitud.DiaRecoleccion);

        var existing = await _db.ProgramacionesMicroservicio
            .Where(p => p.UserId == solicitud.UserId
                        && p.MicroservicioId == solicitud.MicroservicioId
                        && p.Estado == "Scheduled")
            .OrderByDescending(p => p.FechaActualizacion ?? p.FechaCreacion)
            .FirstOrDefaultAsync();

        var notas = BuildScheduleNotes(solicitud);

        if (existing != null)
        {
            existing.FechaProgramada = nextPickup;
            existing.PropiedadId = solicitud.PropiedadId;
            existing.Notas = notas;
            existing.FechaActualizacion = DateTime.Now;
        }
        else
        {
            _db.ProgramacionesMicroservicio.Add(new ProgramacionMicroservicio
            {
                UserId = solicitud.UserId,
                MicroservicioId = solicitud.MicroservicioId,
                PropiedadId = solicitud.PropiedadId,
                FechaProgramada = nextPickup,
                Notas = notas,
                Estado = "Scheduled",
                FechaCreacion = DateTime.Now
            });
        }
    }

    private static DateTime GetNextPickupDate(string? dayCode)
    {
        var target = dayCode switch
        {
            "Sun" => DayOfWeek.Sunday,
            "Mon" => DayOfWeek.Monday,
            "Tue" => DayOfWeek.Tuesday,
            "Wed" => DayOfWeek.Wednesday,
            "Thu" => DayOfWeek.Thursday,
            "Fri" => DayOfWeek.Friday,
            "Sat" => DayOfWeek.Saturday,
            _ => DayOfWeek.Tuesday
        };

        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek != target)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static string BuildScheduleNotes(SolicitudTrash solicitud)
    {
        var parts = new List<string>
        {
            $"Bins: {TrashDisplayLabels.FormatBinsList(solicitud.BinsSeleccionados)}",
            $"Service: {TrashDisplayLabels.FormatHelpType(solicitud.TipoAyuda)}",
            $"Frequency: {TrashDisplayLabels.FormatFrequency(solicitud.Frecuencia)}",
            $"Day: {TrashDisplayLabels.FormatPickupDay(solicitud.DiaRecoleccion)}",
            $"Reminder: {TrashDisplayLabels.FormatReminderWhen(solicitud.RecordatorioCuando)}",
            $"Monthly: ${(solicitud.PrecioMensual ?? 0m):0}"
        };

        return string.Join(" | ", parts);
    }
}
