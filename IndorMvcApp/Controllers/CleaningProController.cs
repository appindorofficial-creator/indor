using System.Globalization;
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
public class CleaningProController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public CleaningProController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> CleaningProService(int id)
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
    public async Task<IActionResult> CleaningProService(CleaningProServiceViewModel model, string? action)
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
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.MicroservicioId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.Estado = "InProgress";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(CleaningProSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your cleaning request. Please ensure the Cleaning Pro flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Microservicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningProSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var defaultAddress = solicitud.DireccionPropiedad;
        if (string.IsNullOrWhiteSpace(defaultAddress) && solicitud.PropiedadId.HasValue)
        {
            defaultAddress = await _db.Propiedades.AsNoTracking()
                .Where(p => p.Id == solicitud.PropiedadId)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();
        }

        return View(new CleaningProSetupViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            Frecuencia = solicitud.Frecuencia ?? "OneTime",
            CantidadLimpiadores = solicitud.CantidadLimpiadores ?? "One",
            DireccionPropiedad = defaultAddress ?? string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleaningProSetup(CleaningProSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningProService), new { id = solicitud.MicroservicioId });
        }

        await EnsureAddressFromPropertyAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.Frecuencia = model.Frecuencia;
            solicitud.CantidadLimpiadores = CleaningProPricingService.NormalizeCrewCode(model.CantidadLimpiadores);
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TarifaHoraria = CleaningProPricingService.GetHourlyRate(solicitud.CantidadLimpiadores);
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            ApplyPricing(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CleaningProCustomize), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your cleaning setup. Please ensure the Cleaning Pro flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningProCustomize(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildCustomizeViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleaningProCustomize(CleaningProCustomizeViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningProSetup), new { id = solicitud.Id });
        }

        if (string.IsNullOrWhiteSpace(model.AreasLimpieza))
        {
            ModelState.AddModelError(nameof(model.AreasLimpieza), "Select at least one area to clean.");
        }

        if (!ModelState.IsValid)
        {
            model.SummaryLine = CleaningProDisplayLabels.FormatSummaryLine(
                model.Frecuencia, model.CantidadLimpiadores, model.HorasEstimadas,
                CleaningProPricingService.Calculate(model.CantidadLimpiadores, model.HorasEstimadas, model.AddonsSeleccionados).Subtotal);
            return View(model);
        }

        try
        {
            solicitud.Frecuencia = model.Frecuencia;
            solicitud.CantidadLimpiadores = CleaningProPricingService.NormalizeCrewCode(model.CantidadLimpiadores);
            solicitud.AreasLimpieza = model.AreasLimpieza;
            solicitud.HorasEstimadas = model.HorasEstimadas;
            solicitud.AddonsSeleccionados = model.AddonsSeleccionados;
            solicitud.Estado = "CustomizeCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            ApplyPricing(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CleaningProReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your cleaning preferences. Please ensure the Cleaning Pro flow tables exist in the database and try again.");
            return View(BuildCustomizeViewModel(solicitud, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningProReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildReviewViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleaningProReview(CleaningProReviewViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningProCustomize), new { id = solicitud.Id });
        }

        if (model.FechaServicio.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaServicio), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            var review = BuildReviewViewModel(solicitud);
            review.FechaServicio = model.FechaServicio;
            review.VentanaHorario = model.VentanaHorario;
            review.NotasLimpiador = model.NotasLimpiador;
            return View(review);
        }

        try
        {
            solicitud.FechaServicio = model.FechaServicio.Date;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.NotasLimpiador = model.NotasLimpiador?.Trim();
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            ApplyPricing(solicitud);
            await UpsertProgramacionAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CleaningProConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your cleaning request. Please ensure the Cleaning Pro flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningProConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningProReview), new { id = solicitud.Id });
        }

        var breakdown = CleaningProPricingService.Calculate(
            solicitud.CantidadLimpiadores,
            solicitud.HorasEstimadas ?? 3m,
            solicitud.AddonsSeleccionados);

        return View(new CleaningProConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            NombreServicio = solicitud.Microservicio?.Nombre ?? "Cleaning Pro",
            CrewSummary = $"{CleaningProDisplayLabels.FormatCrew(solicitud.CantidadLimpiadores)} • ${breakdown.HourlyRate:0}{DisplayLabelsLocalization.L("/hr")}",
            HoursLabel = CleaningProDisplayLabels.FormatHours(solicitud.HorasEstimadas),
            ServiceEstimate = breakdown.ServiceSubtotal,
            FrequencyLabel = string.Format(
                CultureInfo.CurrentUICulture,
                DisplayLabelsLocalization.L("{0} cleaning"),
                CleaningProDisplayLabels.FormatFrequency(solicitud.Frecuencia)),
            ScheduledLabel = CleaningProDisplayLabels.FormatScheduledRange(solicitud.FechaServicio, solicitud.VentanaHorario),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            PrecioTotal = solicitud.PrecioTotal ?? breakdown.Total,
            Moneda = solicitud.Microservicio?.Moneda ?? "USD"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task EnsureAddressFromPropertyAsync(CleaningProSetupViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.DireccionPropiedad))
        {
            return;
        }

        var userId = RequireUserId();
        if (userId == null) return;

        var propiedad = await _db.Propiedades.AsNoTracking()
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(propiedad?.Direccion))
        {
            return;
        }

        model.DireccionPropiedad = propiedad.Direccion;
        ModelState.Remove(nameof(model.DireccionPropiedad));
    }

    private async Task<(Microservicio Microservicio, CleaningProServicioLanding Landing)?> LoadLandingBundleAsync(int microservicioId)
    {
        var microservicio = await _db.Microservicios.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == microservicioId && m.Activo);
        if (microservicio == null) return null;

        var landing = await _db.CleaningProServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == microservicioId && l.Activo);

        if (landing == null)
        {
            landing = new CleaningProServicioLanding
            {
                MicroservicioId = microservicioId,
                PageTitle = "Cleaning Pro",
                LandingTitulo = "Cleaning Pro",
                LandingTagline = "Customized cleaning, your way.",
                LandingSubtitulo = microservicio.DescripcionCompleta ?? microservicio.Descripcion,
                ImagenUrl = microservicio.ImagenUrl,
                PrecioDesde = 35,
                PrecioTexto = "From $35/hr per cleaner"
            };
        }

        return (microservicio, landing);
    }

    private static CleaningProServiceViewModel BuildServiceViewModel(
        Microservicio microservicio,
        CleaningProServicioLanding landing,
        SolicitudCleaningPro? existing,
        CleaningProServiceViewModel? posted = null)
    {
        var items = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos);
        if (items.Count == 0)
        {
            items = SplitPipePairs(microservicio.Incluye, null);
        }

        return new CleaningProServiceViewModel
        {
            MicroservicioId = microservicio.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? microservicio.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? microservicio.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : 35,
            PrecioTexto = landing.PrecioTexto ?? "From $35/hr per cleaner",
            IncludedItems = items,
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto
        };
    }

    private CleaningProCustomizeViewModel BuildCustomizeViewModel(
        SolicitudCleaningPro solicitud,
        CleaningProCustomizeViewModel? posted = null)
    {
        var frequency = posted?.Frecuencia ?? solicitud.Frecuencia ?? "OneTime";
        var crew = CleaningProPricingService.NormalizeCrewCode(
            posted?.CantidadLimpiadores ?? solicitud.CantidadLimpiadores);
        var hours = posted?.HorasEstimadas ?? solicitud.HorasEstimadas ?? 3m;
        var addons = posted?.AddonsSeleccionados ?? solicitud.AddonsSeleccionados ?? string.Empty;
        var breakdown = CleaningProPricingService.Calculate(crew, hours, addons);

        return new CleaningProCustomizeViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            Frecuencia = frequency,
            CantidadLimpiadores = crew,
            AreasLimpieza = posted?.AreasLimpieza ?? solicitud.AreasLimpieza ?? "Bathrooms|Kitchen|LivingRoom|Baseboards|Floors|InsideFridge|Windows|Dusting",
            HorasEstimadas = hours,
            AddonsSeleccionados = addons,
            FromTotal = breakdown.Subtotal,
            SummaryLine = CleaningProDisplayLabels.FormatSummaryLine(frequency, crew, hours, breakdown.Subtotal)
        };
    }

    private CleaningProReviewViewModel BuildReviewViewModel(SolicitudCleaningPro solicitud)
    {
        var hours = solicitud.HorasEstimadas ?? 3m;
        var breakdown = CleaningProPricingService.Calculate(
            solicitud.CantidadLimpiadores, hours, solicitud.AddonsSeleccionados);

        return new CleaningProReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            NombreServicio = solicitud.Microservicio?.Nombre ?? "Cleaning Pro",
            FrequencyLabel = CleaningProDisplayLabels.FormatFrequency(solicitud.Frecuencia),
            AreasLabel = CleaningProDisplayLabels.FormatAreasList(solicitud.AreasLimpieza),
            CrewLabel = CleaningProDisplayLabels.FormatCrewShort(solicitud.CantidadLimpiadores),
            HoursLabel = CleaningProDisplayLabels.FormatHours(solicitud.HorasEstimadas),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicio = solicitud.FechaServicio ?? GetNextTuesday(),
            VentanaHorario = solicitud.VentanaHorario ?? "Morning10",
            NotasLimpiador = solicitud.NotasLimpiador,
            AddonsSeleccionados = solicitud.AddonsSeleccionados ?? string.Empty,
            TarifaHoraria = breakdown.HourlyRate,
            HorasEstimadas = hours,
            ServiceSubtotal = breakdown.ServiceSubtotal,
            AddonsTotal = breakdown.AddonsTotal,
            Subtotal = breakdown.Subtotal,
            ImpuestoVenta = breakdown.Tax,
            PrecioTotal = breakdown.Total,
            AddonLines = CleaningProPricingService.ParseAddons(solicitud.AddonsSeleccionados)
                .Select(a => new CleaningProAddonLineViewModel { Label = a.Label, Amount = a.Price })
                .ToList()
        };
    }

    private static void ApplyPricing(SolicitudCleaningPro solicitud)
    {
        var breakdown = CleaningProPricingService.Calculate(
            solicitud.CantidadLimpiadores,
            solicitud.HorasEstimadas ?? 3m,
            solicitud.AddonsSeleccionados);

        solicitud.TarifaHoraria = breakdown.HourlyRate;
        solicitud.Subtotal = breakdown.Subtotal;
        solicitud.ImpuestoVenta = breakdown.Tax;
        solicitud.PrecioTotal = breakdown.Total;
    }

    private static DateTime GetNextTuesday()
    {
        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek != DayOfWeek.Tuesday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static List<CleaningProFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new CleaningProFeatureItemViewModel
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

    private async Task<SolicitudCleaningPro?> GetActiveSolicitudAsync(string userId, int microservicioId) =>
        await _db.SolicitudesCleaningPro
            .Where(s => s.UserId == userId
                        && s.MicroservicioId == microservicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudCleaningPro> GetOrCreateSolicitudAsync(
        string userId,
        int microservicioId,
        int? solicitudId)
    {
        SolicitudCleaningPro? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesCleaningPro
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, microservicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudCleaningPro
            {
                UserId = userId,
                MicroservicioId = microservicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                Frecuencia = "OneTime",
                CantidadLimpiadores = "One",
                AreasLimpieza = "Bathrooms|Kitchen|LivingRoom|Baseboards|Floors|InsideFridge|Windows|Dusting",
                HorasEstimadas = 3m,
                VentanaHorario = "Morning10"
            };
            _db.SolicitudesCleaningPro.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudCleaningPro?> LoadSolicitudForUserAsync(int id)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await _db.SolicitudesCleaningPro
            .Include(s => s.Microservicio)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertProgramacionAsync(SolicitudCleaningPro solicitud)
    {
        var fechaProgramada = solicitud.FechaServicio ?? DateTime.Today.AddDays(7);

        var existing = await _db.ProgramacionesMicroservicio
            .Where(p => p.UserId == solicitud.UserId
                        && p.MicroservicioId == solicitud.MicroservicioId
                        && p.Estado == "Scheduled")
            .OrderByDescending(p => p.FechaActualizacion ?? p.FechaCreacion)
            .FirstOrDefaultAsync();

        var notas = BuildScheduleNotes(solicitud);

        if (existing != null)
        {
            existing.FechaProgramada = fechaProgramada;
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
                FechaProgramada = fechaProgramada,
                Notas = notas,
                Estado = "Scheduled",
                FechaCreacion = DateTime.Now
            });
        }
    }

    private static string BuildScheduleNotes(SolicitudCleaningPro solicitud)
    {
        var parts = new List<string>
        {
            $"Frequency: {CleaningProDisplayLabels.FormatFrequency(solicitud.Frecuencia)}",
            $"Crew: {CleaningProDisplayLabels.FormatCrew(solicitud.CantidadLimpiadores)}",
            $"Areas: {CleaningProDisplayLabels.FormatAreasList(solicitud.AreasLimpieza)}",
            $"Hours: {CleaningProDisplayLabels.FormatHours(solicitud.HorasEstimadas)}",
            $"Time: {CleaningProDisplayLabels.FormatDateTime(solicitud.FechaServicio, solicitud.VentanaHorario)}",
            $"Total: ${(solicitud.PrecioTotal ?? 0m):0.00}"
        };

        return string.Join(" | ", parts);
    }
}
