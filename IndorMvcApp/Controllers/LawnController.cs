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
public class LawnController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LawnController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> LawnService(int id)
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
    public async Task<IActionResult> LawnService(LawnServiceViewModel model, string? action)
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
            return RedirectToAction(nameof(LawnSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your lawn service request. Please ensure the lawn flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Microservicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildSetupViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnSetup(LawnSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnService), new { id = solicitud.MicroservicioId });
        }

        if (!ModelState.IsValid)
        {
            model.AreaOptions = BuildAreaCards(model.AreaServicio);
            model.EstimatedTotal = LawnPricingService.CalculateTotal(model.TipoServicio, model.AreaServicio, solicitud.AddonsSeleccionados);
            return View(model);
        }

        try
        {
            solicitud.TipoServicio = model.TipoServicio;
            solicitud.Frecuencia = model.Frecuencia;
            solicitud.AreaServicio = model.AreaServicio;
            solicitud.PrecioBase = LawnPricingService.GetBasePrice(model.AreaServicio);
            solicitud.PrecioAddons = LawnPricingService.GetAddonsTotal(solicitud.AddonsSeleccionados);
            solicitud.DescuentoSuscripcion = LawnPricingService.GetSubscriptionDiscount(model.TipoServicio);
            solicitud.PrecioTotal = LawnPricingService.CalculateTotal(model.TipoServicio, model.AreaServicio, solicitud.AddonsSeleccionados);
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(LawnAddons), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your lawn setup. Please ensure the lawn flow tables exist in the database and try again.");
            model.AreaOptions = BuildAreaCards(model.AreaServicio);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnAddons(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildAddonsViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnAddons(LawnAddonsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnSetup), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            return View(BuildAddonsViewModel(solicitud, model));
        }

        try
        {
            solicitud.AddonsSeleccionados = model.AddonsSeleccionados;
            solicitud.PreferenciaExtra = model.PreferenciaExtra;
            solicitud.PrecioAddons = LawnPricingService.GetAddonsTotal(model.AddonsSeleccionados);
            solicitud.DescuentoSuscripcion = LawnPricingService.GetSubscriptionDiscount(solicitud.TipoServicio);
            solicitud.PrecioTotal = LawnPricingService.CalculateTotal(
                solicitud.TipoServicio, solicitud.AreaServicio, model.AddonsSeleccionados);
            solicitud.Estado = "AddonsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(LawnReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save add-ons. Please ensure the lawn flow tables exist in the database and try again.");
            return View(BuildAddonsViewModel(solicitud, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(await BuildReviewViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnReview(LawnReviewViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnAddons), new { id = solicitud.Id });
        }

        if (model.FechaPreferida.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaPreferida), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            var review = await BuildReviewViewModelAsync(solicitud);
            review.FechaPreferida = model.FechaPreferida;
            review.VentanaHorario = model.VentanaHorario;
            return View(review);
        }

        try
        {
            solicitud.FechaPreferida = model.FechaPreferida.Date;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertProgramacionAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(LawnConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your booking. Please ensure the lawn flow tables exist in the database and try again.");
            return View(await BuildReviewViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnReview), new { id = solicitud.Id });
        }

        return View(new LawnConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            NombreServicio = solicitud.Microservicio?.Nombre ?? "Always Perfect Lawn",
            SubscriptionLabel = LawnDisplayLabels.FormatSubscriptionLabel(solicitud.TipoServicio, solicitud.Frecuencia),
            AreaLabel = LawnDisplayLabels.FormatArea(solicitud.AreaServicio),
            AddonsLabel = LawnDisplayLabels.FormatAddonsList(solicitud.AddonsSeleccionados),
            ScheduledLabel = LawnDisplayLabels.FormatScheduledLabel(solicitud.FechaPreferida, solicitud.VentanaHorario),
            PrecioTotal = solicitud.PrecioTotal ?? 0m,
            Moneda = solicitud.Microservicio?.Moneda ?? "USD"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(Microservicio Microservicio, LawnServicioLanding Landing)?> LoadLandingBundleAsync(int microservicioId)
    {
        var microservicio = await _db.Microservicios.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == microservicioId && m.Activo);
        if (microservicio == null) return null;

        var landing = await _db.LawnServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == microservicioId && l.Activo);

        if (landing == null)
        {
            landing = new LawnServicioLanding
            {
                MicroservicioId = microservicioId,
                PageTitle = microservicio.Nombre,
                LandingTitulo = microservicio.Nombre,
                LandingTagline = microservicio.Subtitulo,
                LandingSubtitulo = microservicio.DescripcionCompleta ?? microservicio.Descripcion,
                ImagenUrl = microservicio.ImagenUrl,
                PrecioDesde = microservicio.Valor > 0 ? microservicio.Valor : 45,
                PrecioTexto = $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 45):0} USD"
            };
        }

        return (microservicio, landing);
    }

    private static LawnServiceViewModel BuildServiceViewModel(
        Microservicio microservicio,
        LawnServicioLanding landing,
        SolicitudLawn? existing,
        LawnServiceViewModel? posted = null)
    {
        var items = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos);
        if (items.Count == 0)
        {
            items = SplitPipePairs(microservicio.Incluye, null);
        }

        return new LawnServiceViewModel
        {
            MicroservicioId = microservicio.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? microservicio.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? microservicio.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : (microservicio.Valor > 0 ? microservicio.Valor : 45),
            PrecioTexto = landing.PrecioTexto ?? $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 45):0} USD",
            IncludedItems = items,
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto
        };
    }

    private LawnSetupViewModel BuildSetupViewModel(SolicitudLawn solicitud) =>
        new()
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            TipoServicio = solicitud.TipoServicio ?? "Subscription",
            Frecuencia = solicitud.Frecuencia ?? "Biweekly",
            AreaServicio = solicitud.AreaServicio ?? "FrontBack",
            AreaOptions = BuildAreaCards(solicitud.AreaServicio ?? "FrontBack"),
            EstimatedTotal = LawnPricingService.CalculateTotal(
                solicitud.TipoServicio, solicitud.AreaServicio, solicitud.AddonsSeleccionados)
        };

    private static List<LawnAreaCardViewModel> BuildAreaCards(string selectedCode) =>
        LawnPricingService.AreaOptions.Select(a => new LawnAreaCardViewModel
        {
            Code = a.Code,
            Label = a.Label,
            Price = a.Price,
            Icon = a.Icon,
            IsCustomQuote = a.IsCustomQuote
        }).ToList();

    private LawnAddonsViewModel BuildAddonsViewModel(SolicitudLawn solicitud, LawnAddonsViewModel? posted = null)
    {
        var selectedCodes = (posted?.AddonsSeleccionados ?? solicitud.AddonsSeleccionados ?? string.Empty)
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var addonsPipe = posted?.AddonsSeleccionados ?? solicitud.AddonsSeleccionados;
        var basePrice = LawnPricingService.GetBasePrice(solicitud.AreaServicio);
        var addonsPrice = LawnPricingService.GetAddonsTotal(addonsPipe);
        var discount = LawnPricingService.GetSubscriptionDiscount(solicitud.TipoServicio);

        return new LawnAddonsViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            TipoServicio = solicitud.TipoServicio,
            AreaServicio = solicitud.AreaServicio ?? "FrontBack",
            AddonsSeleccionados = addonsPipe ?? string.Empty,
            PreferenciaExtra = posted?.PreferenciaExtra ?? solicitud.PreferenciaExtra ?? "NoThanks",
            PrecioBase = basePrice,
            PrecioAddons = addonsPrice,
            DescuentoSuscripcion = discount,
            PrecioTotal = Math.Max(0m, basePrice + addonsPrice - discount),
            AreaLabel = LawnDisplayLabels.FormatArea(solicitud.AreaServicio),
            AddonOptions = LawnPricingService.AddonOptions.Select(a => new LawnAddonCardViewModel
            {
                Code = a.Code,
                Label = a.Label,
                Price = a.Price,
                Icon = a.Icon,
                Selected = selectedCodes.Contains(a.Code)
            }).ToList(),
            SelectedAddonLines = LawnPricingService.ParseAddons(addonsPipe)
                .Select(a => new LawnLineItemViewModel { Label = a.Label, Amount = a.Price, Icon = a.Icon })
                .ToList()
        };
    }

    private async Task<LawnReviewViewModel> BuildReviewViewModelAsync(SolicitudLawn solicitud)
    {
        var landing = await _db.LawnServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == solicitud.MicroservicioId);

        var fecha = solicitud.FechaPreferida ?? GetNextSaturday();
        var dateOptions = BuildDateOptions(fecha);

        return new LawnReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            ImagenUrl = ResolveImageUrl(landing?.ImagenUrl ?? solicitud.Microservicio?.ImagenUrl),
            SubscriptionLabel = LawnDisplayLabels.FormatSubscriptionLabel(solicitud.TipoServicio, solicitud.Frecuencia),
            AreaLabel = LawnDisplayLabels.FormatArea(solicitud.AreaServicio),
            AddonsLabel = LawnDisplayLabels.FormatAddonsList(solicitud.AddonsSeleccionados),
            PreferredDayLabel = LawnDisplayLabels.FormatPreferredDay(fecha),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaPreferida = fecha,
            VentanaHorario = solicitud.VentanaHorario ?? "Morning8_11",
            PrecioBase = solicitud.PrecioBase ?? LawnPricingService.GetBasePrice(solicitud.AreaServicio),
            PrecioAddons = solicitud.PrecioAddons ?? LawnPricingService.GetAddonsTotal(solicitud.AddonsSeleccionados),
            DescuentoSuscripcion = solicitud.DescuentoSuscripcion ?? LawnPricingService.GetSubscriptionDiscount(solicitud.TipoServicio),
            PrecioTotal = solicitud.PrecioTotal ?? LawnPricingService.CalculateTotal(
                solicitud.TipoServicio, solicitud.AreaServicio, solicitud.AddonsSeleccionados),
            AddonLines = LawnPricingService.ParseAddons(solicitud.AddonsSeleccionados)
                .Select(a => new LawnLineItemViewModel { Label = a.Label, Amount = a.Price, Icon = a.Icon })
                .ToList(),
            DateOptions = dateOptions
        };
    }

    private static List<LawnDateOptionViewModel> BuildDateOptions(DateTime selected)
    {
        var start = DateTime.Today.AddDays(1);
        return Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = start.AddDays(offset);
                return new LawnDateOptionViewModel
                {
                    Date = date,
                    DayLabel = date.ToString("ddd"),
                    DateLabel = date.Day.ToString(),
                    Selected = date.Date == selected.Date
                };
            })
            .ToList();
    }

    private static DateTime GetNextSaturday()
    {
        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek != DayOfWeek.Saturday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static List<LawnFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new LawnFeatureItemViewModel
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

    private async Task<SolicitudLawn?> GetActiveSolicitudAsync(string userId, int microservicioId) =>
        await _db.SolicitudesLawn
            .Where(s => s.UserId == userId
                        && s.MicroservicioId == microservicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudLawn> GetOrCreateSolicitudAsync(
        string userId,
        int microservicioId,
        int? solicitudId)
    {
        SolicitudLawn? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesLawn
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, microservicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudLawn
            {
                UserId = userId,
                MicroservicioId = microservicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                TipoServicio = "Subscription",
                Frecuencia = "Biweekly",
                AreaServicio = "FrontBack",
                PreferenciaExtra = "NoThanks"
            };
            _db.SolicitudesLawn.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudLawn?> LoadSolicitudForUserAsync(int id)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await _db.SolicitudesLawn
            .Include(s => s.Microservicio)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertProgramacionAsync(SolicitudLawn solicitud)
    {
        var fechaProgramada = solicitud.FechaPreferida ?? DateTime.Today.AddDays(7);

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

    private static string BuildScheduleNotes(SolicitudLawn solicitud)
    {
        var parts = new List<string>
        {
            $"{LawnDisplayLabels.FormatSubscriptionLabel(solicitud.TipoServicio, solicitud.Frecuencia)}",
            $"Area: {LawnDisplayLabels.FormatArea(solicitud.AreaServicio)}",
            $"Add-ons: {LawnDisplayLabels.FormatAddonsList(solicitud.AddonsSeleccionados)}",
            $"Time: {LawnDisplayLabels.FormatScheduledLabel(solicitud.FechaPreferida, solicitud.VentanaHorario)}",
            $"Total: ${(solicitud.PrecioTotal ?? 0m):0}"
        };

        return string.Join(" | ", parts);
    }
}
