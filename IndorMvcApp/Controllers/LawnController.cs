using IndorMvcApp.Data;
using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Controllers;

[Authorize]
public class LawnController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    LawnCatalogService catalog,
    IIndorLocalizer localizer) : Controller
{
    [HttpGet]
    public async Task<IActionResult> LawnService(int id, CancellationToken ct)
    {
        var bundle = await LoadLandingBundleAsync(id, ct);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var existing = await GetActiveSolicitudAsync(userId, id, ct);
        return View(await BuildServiceViewModelAsync(bundle.Value.Microservicio, bundle.Value.Landing, existing, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnService(LawnServiceViewModel model, string? action, CancellationToken ct)
    {
        var bundle = await LoadLandingBundleAsync(model.MicroservicioId, ct);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home", null, "home-care-essentials");
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId, ct);
            var propiedad = propiedadId.HasValue
                ? await db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId, ct)
                : null;

            var solicitud = await GetOrCreateSolicitudAsync(userId, model.MicroservicioId, model.SolicitudId, ct);
            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = propiedad?.Direccion;
            solicitud.ModoServicio = string.Equals(action, "remindOnly", StringComparison.OrdinalIgnoreCase)
                ? LawnServiceModes.ReminderOnly
                : LawnServiceModes.FullService;
            solicitud.RecordatorioActivo = model.RecordatorioActivo;
            solicitud.Estado = "InProgress";
            solicitud.FechaActualizacion = DateTime.Now;

            await db.SaveChangesAsync(ct);
            return RedirectToAction(nameof(LawnSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your lawn service request. Please ensure the lawn flow tables exist in the database and try again.");
            return View(await BuildServiceViewModelAsync(bundle.Value.Microservicio, bundle.Value.Landing, null, ct, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnSetup(int id, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, ct);
        if (solicitud == null) return NotFound();

        return View(await BuildSetupViewModelAsync(solicitud, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnSetup(LawnSetupViewModel model, string? action, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, ct);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnService), new { id = solicitud.MicroservicioId });
        }

        model.AddonsSeleccionados ??= string.Empty;
        ModelState.Remove(nameof(model.AddonsSeleccionados));

        if (!ModelState.IsValid)
        {
            return View(await BuildSetupViewModelAsync(solicitud, ct, model));
        }

        try
        {
            solicitud.Frecuencia = model.Frecuencia;
            solicitud.TipoServicio = LawnCatalogService.MapFrequencyToServiceType(model.Frecuencia);
            solicitud.AreaServicio = model.AreaServicio;
            solicitud.AddonsSeleccionados = NormalizeAddons(model.AddonsSeleccionados);
            solicitud.PrecioBase = await catalog.GetBasePriceAsync(solicitud.MicroservicioId, model.AreaServicio, ct);
            solicitud.PrecioAddons = await catalog.GetAddonsTotalAsync(solicitud.MicroservicioId, solicitud.AddonsSeleccionados, ct);
            solicitud.DescuentoSuscripcion = await catalog.GetSubscriptionDiscountAsync(model.Frecuencia);
            solicitud.PrecioTotal = await catalog.CalculateTotalAsync(
                solicitud.MicroservicioId, model.Frecuencia, model.AreaServicio, solicitud.AddonsSeleccionados, ct);
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await db.SaveChangesAsync(ct);
            return RedirectToAction(nameof(LawnSchedule), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your lawn setup. Please ensure the lawn flow tables exist in the database and try again.");
            return View(await BuildSetupViewModelAsync(solicitud, ct, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnAddons(int id, CancellationToken ct) =>
        RedirectToAction(nameof(LawnSetup), new { id });

    [HttpGet]
    public async Task<IActionResult> LawnSchedule(int id, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, ct);
        if (solicitud == null) return NotFound();

        if (solicitud.Estado is not ("SetupCompleted" or "ScheduleCompleted"))
        {
            return RedirectToAction(nameof(LawnSetup), new { id });
        }

        return View(await BuildScheduleViewModelAsync(solicitud, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnSchedule(LawnScheduleViewModel model, string? action, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, ct);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnSetup), new { id = solicitud.Id });
        }

        if (model.FechaPreferida.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaPreferida), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildScheduleViewModelAsync(solicitud, ct, model));
        }

        try
        {
            solicitud.FechaPreferida = model.FechaPreferida.Date;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.RecordatorioActivo = model.RecordatorioActivo;
            solicitud.RecordatorioAvisoDias = model.RecordatorioAvisoDias;
            solicitud.RecordatorioCanales = NormalizeChannels(model.RecordatorioCanales);
            solicitud.ProximoRecordatorioUtc = model.RecordatorioActivo
                ? LawnCatalogService.ComputeNextReminderUtc(model.FechaPreferida, solicitud.Frecuencia)
                : null;
            solicitud.Estado = "ScheduleCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await db.SaveChangesAsync(ct);
            return RedirectToAction(nameof(LawnReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your schedule. Please ensure the lawn flow tables exist in the database and try again.");
            return View(await BuildScheduleViewModelAsync(solicitud, ct, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnReview(int id, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, ct);
        if (solicitud == null) return NotFound();

        if (solicitud.Estado is not ("ScheduleCompleted" or "Submitted"))
        {
            return RedirectToAction(nameof(LawnSchedule), new { id });
        }

        return View(await BuildReviewViewModelAsync(solicitud, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LawnReview(int id, string? action, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, ct);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnSchedule), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            if (solicitud.RecordatorioActivo)
            {
                solicitud.ProximoRecordatorioUtc ??= LawnCatalogService.ComputeNextReminderUtc(
                    solicitud.FechaPreferida, solicitud.Frecuencia);
            }

            await UpsertProgramacionAsync(solicitud, ct);
            await db.SaveChangesAsync(ct);

            return RedirectToAction(nameof(LawnConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your booking. Please ensure the lawn flow tables exist in the database and try again.");
            return View(await BuildReviewViewModelAsync(solicitud, ct));
        }
    }

    [HttpGet]
    public async Task<IActionResult> LawnConfirmed(int id, CancellationToken ct)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, ct);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(LawnReview), new { id = solicitud.Id });
        }

        return View(await BuildConfirmedViewModelAsync(solicitud, ct));
    }

    private string? RequireUserId() => userManager.GetUserId(User);

    private async Task<(Microservicio Microservicio, LawnServicioLanding Landing)?> LoadLandingBundleAsync(
        int microservicioId,
        CancellationToken ct)
    {
        var microservicio = await db.Microservicios.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == microservicioId && m.Activo, ct);
        if (microservicio == null) return null;

        var landing = await db.LawnServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == microservicioId && l.Activo, ct);

        landing ??= new LawnServicioLanding
        {
            MicroservicioId = microservicioId,
            PageTitle = "Always Perfect Lawn",
            LandingTitulo = "Always Perfect Lawn",
            LandingTagline = microservicio.Subtitulo,
            LandingSubtitulo = microservicio.DescripcionCompleta ?? microservicio.Descripcion ?? string.Empty,
            ImagenUrl = microservicio.ImagenUrl,
            PrecioDesde = microservicio.Valor > 0 ? microservicio.Valor : 45,
            PrecioTexto = $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 45):0} USD",
            ReminderBannerTitulo = "Automatic reminder",
            ReminderBannerTexto = "Remind me every 15 days to mow the lawn. We will send you a notification to schedule or repeat the service.",
            RemindOnlyCtaTexto = "Only remind me",
            ReminderDefaultOn = true
        };

        return (microservicio, landing);
    }

    private async Task<LawnServiceViewModel> BuildServiceViewModelAsync(
        Microservicio microservicio,
        LawnServicioLanding landing,
        SolicitudLawn? existing,
        CancellationToken ct,
        LawnServiceViewModel? posted = null)
    {
        var items = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos);
        if (items.Count == 0)
        {
            items = SplitPipePairs(microservicio.Incluye, null);
        }

        var brandName = microservicio.LocalizedNombre(localizer.IsSpanish);
        return new LawnServiceViewModel
        {
            MicroservicioId = microservicio.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = brandName,
            LandingTitulo = brandName,
            LandingTagline = landing.LandingTagline ?? microservicio.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? microservicio.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : (microservicio.Valor > 0 ? microservicio.Valor : 45),
            PrecioTexto = landing.PrecioTexto ?? $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 45):0} USD",
            IncludedItems = items,
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto,
            ReminderBannerTitulo = landing.ReminderBannerTitulo ?? "Automatic reminder",
            ReminderBannerTexto = landing.ReminderBannerTexto
                ?? "Remind me every 15 days to mow the lawn. We will send you a notification to schedule or repeat the service.",
            RemindOnlyCtaTexto = landing.RemindOnlyCtaTexto ?? "Only remind me",
            RecordatorioActivo = posted?.RecordatorioActivo ?? existing?.RecordatorioActivo ?? landing.ReminderDefaultOn
        };
    }

    private async Task<LawnSetupViewModel> BuildSetupViewModelAsync(
        SolicitudLawn solicitud,
        CancellationToken ct,
        LawnSetupViewModel? posted = null)
    {
        var freqOptions = await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.Frequency, ct);
        var areaOptions = await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.Area, ct);
        var addonOptions = await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.Addon, ct);

        var frequency = posted?.Frecuencia ?? solicitud.Frecuencia ?? "Every15Days";
        var area = posted?.AreaServicio ?? solicitud.AreaServicio ?? "FrontBack";
        var addonsPipe = posted?.AddonsSeleccionados ?? solicitud.AddonsSeleccionados ?? string.Empty;
        var selectedAddons = addonsPipe.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new LawnSetupViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            IsReminderOnly = solicitud.ModoServicio == LawnServiceModes.ReminderOnly,
            Frecuencia = frequency,
            AreaServicio = area,
            AddonsSeleccionados = addonsPipe,
            EstimatedTotal = await catalog.CalculateTotalAsync(
                solicitud.MicroservicioId, frequency, area, addonsPipe, ct),
            FrequencyOptions = freqOptions.Select(o => new LawnOptionCardViewModel
            {
                Code = o.Code,
                Label = LawnCatalogService.PickLabel(o),
                Icon = o.IconClass
            }).ToList(),
            AreaOptions = areaOptions.Select(o => new LawnAreaCardViewModel
            {
                Code = o.Code,
                Label = LawnCatalogService.PickLabel(o),
                Price = o.Price,
                Icon = o.IconClass,
                IsCustomQuote = o.RequiresQuote
            }).ToList(),
            AddonOptions = addonOptions.Select(o => new LawnAddonCardViewModel
            {
                Code = o.Code,
                Label = LawnCatalogService.PickLabel(o),
                Price = o.Price,
                Icon = o.IconClass,
                Selected = selectedAddons.Contains(o.Code)
            }).ToList()
        };
    }

    private async Task<LawnScheduleViewModel> BuildScheduleViewModelAsync(
        SolicitudLawn solicitud,
        CancellationToken ct,
        LawnScheduleViewModel? posted = null)
    {
        var landing = await db.LawnServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == solicitud.MicroservicioId, ct);

        var fecha = posted?.FechaPreferida ?? solicitud.FechaPreferida ?? GetNextSaturday();
        var timeWindows = await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.TimeWindow, ct);
        var reminderLeads = await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.ReminderLead, ct);
        var reminderChannels = await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.ReminderChannel, ct);

        var areaLabel = await catalog.GetLabelAsync(solicitud.MicroservicioId, LawnCatalogGroups.Area, solicitud.AreaServicio, ct);
        var freqLabel = LawnDisplayLabels.FormatFrequencyLabel(solicitud.Frecuencia, solicitud.TipoServicio);

        return new LawnScheduleViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            IsReminderOnly = solicitud.ModoServicio == LawnServiceModes.ReminderOnly,
            ImagenUrl = ResolveImageUrl(landing?.ImagenUrl ?? solicitud.Microservicio?.ImagenUrl),
            FrequencyLabel = freqLabel,
            AreaLabel = areaLabel,
            PrecioTotal = solicitud.PrecioTotal ?? await catalog.CalculateTotalAsync(
                solicitud.MicroservicioId, solicitud.Frecuencia, solicitud.AreaServicio, solicitud.AddonsSeleccionados, ct),
            FechaPreferida = fecha,
            VentanaHorario = posted?.VentanaHorario ?? solicitud.VentanaHorario ?? "Morning8_11",
            RecordatorioActivo = posted?.RecordatorioActivo ?? solicitud.RecordatorioActivo,
            Frecuencia = solicitud.Frecuencia ?? "Every15Days",
            RecordatorioAvisoDias = posted?.RecordatorioAvisoDias ?? solicitud.RecordatorioAvisoDias,
            RecordatorioCanales = posted?.RecordatorioCanales ?? solicitud.RecordatorioCanales ?? "Push",
            DateOptions = BuildDateOptions(fecha),
            TimeWindowOptions = timeWindows.Select(o => new LawnOptionCardViewModel
            {
                Code = o.Code,
                Label = LawnCatalogService.PickLabel(o),
                Icon = o.IconClass
            }).ToList(),
            ReminderLeadOptions = reminderLeads.Select(o => new LawnOptionCardViewModel
            {
                Code = o.Code,
                Label = LawnCatalogService.PickLabel(o),
                Icon = o.IconClass
            }).ToList(),
            ReminderChannelOptions = reminderChannels.Select(o => new LawnOptionCardViewModel
            {
                Code = o.Code,
                Label = LawnCatalogService.PickLabel(o),
                Icon = o.IconClass
            }).ToList()
        };
    }

    private async Task<LawnReviewViewModel> BuildReviewViewModelAsync(SolicitudLawn solicitud, CancellationToken ct)
    {
        var landing = await db.LawnServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == solicitud.MicroservicioId, ct);

        var areaLabel = await catalog.GetLabelAsync(solicitud.MicroservicioId, LawnCatalogGroups.Area, solicitud.AreaServicio, ct);
        var timeLabel = await catalog.GetLabelAsync(solicitud.MicroservicioId, LawnCatalogGroups.TimeWindow, solicitud.VentanaHorario, ct);
        var addonLabels = (await catalog.ParseAddonsAsync(solicitud.MicroservicioId, solicitud.AddonsSeleccionados, ct))
            .Select(a => LawnCatalogService.PickLabel(a));

        var channelLabels = (await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.ReminderChannel, ct))
            .ToDictionary(c => c.Code, c => LawnCatalogService.PickLabel(c));

        return new LawnReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            IsReminderOnly = solicitud.ModoServicio == LawnServiceModes.ReminderOnly,
            ServiceName = Microservicio.ResolveBrandNombre(
                solicitud.MicroservicioId,
                solicitud.Microservicio?.Nombre ?? landing?.LandingTitulo ?? "Always Perfect Lawn",
                solicitud.Microservicio?.NombreEs,
                localizer.IsSpanish),
            FrequencyLabel = LawnDisplayLabels.FormatFrequencyLabel(solicitud.Frecuencia, solicitud.TipoServicio),
            AreaLabel = areaLabel,
            AddonsLabel = LawnDisplayLabels.FormatAddonsList(addonLabels),
            ScheduledLabel = LawnDisplayLabels.FormatScheduledLabel(solicitud.FechaPreferida, solicitud.VentanaHorario, timeLabel),
            ReminderLabel = LawnDisplayLabels.FormatReminderLabel(
                solicitud.RecordatorioActivo,
                solicitud.Frecuencia,
                solicitud.RecordatorioAvisoDias,
                (solicitud.RecordatorioCanales ?? "Push").Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(code => channelLabels.TryGetValue(code, out var label) ? label : code)),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            PrecioBase = solicitud.PrecioBase ?? await catalog.GetBasePriceAsync(solicitud.MicroservicioId, solicitud.AreaServicio, ct),
            PrecioAddons = solicitud.PrecioAddons ?? await catalog.GetAddonsTotalAsync(solicitud.MicroservicioId, solicitud.AddonsSeleccionados, ct),
            DescuentoSuscripcion = solicitud.DescuentoSuscripcion ?? await catalog.GetSubscriptionDiscountAsync(solicitud.Frecuencia),
            PrecioTotal = solicitud.PrecioTotal ?? await catalog.CalculateTotalAsync(
                solicitud.MicroservicioId, solicitud.Frecuencia, solicitud.AreaServicio, solicitud.AddonsSeleccionados, ct),
            AddonLines = (await catalog.ParseAddonsAsync(solicitud.MicroservicioId, solicitud.AddonsSeleccionados, ct))
                .Select(a => new LawnLineItemViewModel { Label = LawnCatalogService.PickLabel(a), Amount = a.Price, Icon = a.IconClass })
                .ToList()
        };
    }

    private async Task<LawnConfirmedViewModel> BuildConfirmedViewModelAsync(SolicitudLawn solicitud, CancellationToken ct)
    {
        var areaLabel = await catalog.GetLabelAsync(solicitud.MicroservicioId, LawnCatalogGroups.Area, solicitud.AreaServicio, ct);
        var timeLabel = await catalog.GetLabelAsync(solicitud.MicroservicioId, LawnCatalogGroups.TimeWindow, solicitud.VentanaHorario, ct);
        var addonLabels = (await catalog.ParseAddonsAsync(solicitud.MicroservicioId, solicitud.AddonsSeleccionados, ct))
            .Select(a => LawnCatalogService.PickLabel(a));
        var channelLabels = (await catalog.LoadGroupAsync(solicitud.MicroservicioId, LawnCatalogGroups.ReminderChannel, ct))
            .ToDictionary(c => c.Code, c => LawnCatalogService.PickLabel(c));

        return new LawnConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            IsReminderOnly = solicitud.ModoServicio == LawnServiceModes.ReminderOnly,
            NombreServicio = Microservicio.ResolveBrandNombre(
                solicitud.MicroservicioId,
                solicitud.Microservicio?.Nombre ?? "Always Perfect Lawn",
                solicitud.Microservicio?.NombreEs,
                localizer.IsSpanish),
            FrequencyLabel = LawnDisplayLabels.FormatFrequencyLabel(solicitud.Frecuencia, solicitud.TipoServicio),
            AreaLabel = areaLabel,
            AddonsLabel = LawnDisplayLabels.FormatAddonsList(addonLabels),
            ScheduledLabel = LawnDisplayLabels.FormatScheduledLabel(solicitud.FechaPreferida, solicitud.VentanaHorario, timeLabel),
            ReminderLabel = LawnDisplayLabels.FormatReminderLabel(
                solicitud.RecordatorioActivo, solicitud.Frecuencia, solicitud.RecordatorioAvisoDias),
            NextReminderLabel = LawnDisplayLabels.FormatNextReminderLabel(solicitud.ProximoRecordatorioUtc, solicitud.Frecuencia),
            NotificationMethodLabel = LawnDisplayLabels.FormatNotificationChannels(solicitud.RecordatorioCanales, channelLabels),
            RecordatorioActivo = solicitud.RecordatorioActivo,
            PrecioTotal = solicitud.PrecioTotal ?? 0m,
            Moneda = solicitud.Microservicio?.Moneda ?? "USD"
        };
    }

    private static List<LawnDateOptionViewModel> BuildDateOptions(DateTime selected)
    {
        var start = DateTime.Today.AddDays(1);
        return Enumerable.Range(0, 10)
            .Select(offset =>
            {
                var date = start.AddDays(offset);
                return new LawnDateOptionViewModel
                {
                    Date = date,
                    DayLabel = date.ToString("ddd").ToUpperInvariant(),
                    DateLabel = date.Day.ToString(),
                    MonthLabel = date.ToString("MMM").ToUpperInvariant(),
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

    private static string NormalizeAddons(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe) ? "NoThanks" : pipe.Trim();

    private static string NormalizeChannels(string? pipe) =>
        string.IsNullOrWhiteSpace(pipe) ? "Push" : pipe.Trim();

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

    private async Task<int?> GetLatestPropertyIdAsync(string userId, CancellationToken ct) =>
        await db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(ct);

    private async Task<SolicitudLawn?> GetActiveSolicitudAsync(string userId, int microservicioId, CancellationToken ct) =>
        await db.SolicitudesLawn
            .Where(s => s.UserId == userId
                        && s.MicroservicioId == microservicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync(ct);

    private async Task<SolicitudLawn> GetOrCreateSolicitudAsync(
        string userId,
        int microservicioId,
        int? solicitudId,
        CancellationToken ct)
    {
        SolicitudLawn? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await db.SolicitudesLawn
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId, ct);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, microservicioId, ct);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId, ct);
            solicitud = new SolicitudLawn
            {
                UserId = userId,
                MicroservicioId = microservicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                ModoServicio = LawnServiceModes.FullService,
                TipoServicio = "Subscription",
                Frecuencia = "Every15Days",
                AreaServicio = "FrontBack",
                AddonsSeleccionados = "NoThanks",
                RecordatorioActivo = true,
                RecordatorioAvisoDias = 1,
                RecordatorioCanales = "Push"
            };
            db.SolicitudesLawn.Add(solicitud);
            await db.SaveChangesAsync(ct);
        }

        return solicitud;
    }

    private async Task<SolicitudLawn?> LoadSolicitudForUserAsync(int id, CancellationToken ct)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        return await db.SolicitudesLawn
            .Include(s => s.Microservicio)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);
    }

    private async Task UpsertProgramacionAsync(SolicitudLawn solicitud, CancellationToken ct)
    {
        var fechaProgramada = solicitud.FechaPreferida ?? DateTime.Today.AddDays(7);
        var isReminderOnly = solicitud.ModoServicio == LawnServiceModes.ReminderOnly;
        var estado = isReminderOnly && !solicitud.RecordatorioActivo
            ? "Cancelled"
            : isReminderOnly
                ? "ReminderActive"
                : "Scheduled";

        var existing = await db.ProgramacionesMicroservicio
            .Where(p => p.UserId == solicitud.UserId
                        && p.MicroservicioId == solicitud.MicroservicioId
                        && p.Estado != "Completed")
            .OrderByDescending(p => p.FechaActualizacion ?? p.FechaCreacion)
            .FirstOrDefaultAsync(ct);

        var notas = BuildScheduleNotes(solicitud);

        if (existing != null)
        {
            existing.FechaProgramada = fechaProgramada;
            existing.PropiedadId = solicitud.PropiedadId;
            existing.Notas = notas;
            existing.Estado = estado;
            existing.FechaActualizacion = DateTime.Now;
        }
        else
        {
            db.ProgramacionesMicroservicio.Add(new ProgramacionMicroservicio
            {
                UserId = solicitud.UserId,
                MicroservicioId = solicitud.MicroservicioId,
                PropiedadId = solicitud.PropiedadId,
                FechaProgramada = fechaProgramada,
                Notas = notas,
                Estado = estado,
                FechaCreacion = DateTime.Now
            });
        }
    }

    private static string BuildScheduleNotes(SolicitudLawn solicitud)
    {
        var parts = new List<string>
        {
            $"Mode: {solicitud.ModoServicio}",
            $"Frequency: {LawnDisplayLabels.FormatFrequencyLabel(solicitud.Frecuencia, solicitud.TipoServicio)}",
            $"Area: {LawnDisplayLabels.FormatArea(solicitud.AreaServicio)}",
            $"Time: {LawnDisplayLabels.FormatScheduledLabel(solicitud.FechaPreferida, solicitud.VentanaHorario)}",
            $"Reminder: {(solicitud.RecordatorioActivo ? "On" : "Off")}",
            $"Notify: {solicitud.RecordatorioAvisoDias} day(s) before via {solicitud.RecordatorioCanales}",
            $"Next reminder UTC: {solicitud.ProximoRecordatorioUtc:O}",
            $"Total: ${(solicitud.PrecioTotal ?? 0m):0}"
        };

        return string.Join(" | ", parts);
    }
}
