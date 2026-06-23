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
public class WaterHeaterFlushController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 2;

    public WaterHeaterFlushController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterFlushService(int id)
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
    public async Task<IActionResult> WaterHeaterFlushService(WaterHeaterFlushServiceViewModel model, string? action)
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
            return RedirectToAction(nameof(WaterHeaterFlushSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your water heater flush request. Please ensure the Water Heater Flush flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterFlushSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var detailsEntered = string.Equals(solicitud.Estado, "SetupCompleted", StringComparison.OrdinalIgnoreCase);

        return View(new WaterHeaterFlushSetupViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Water Heater Flush",
            TipoCalentador = detailsEntered ? (solicitud.TipoCalentador ?? "") : "",
            FuenteEnergia = detailsEntered ? (solicitud.FuenteEnergia ?? "") : "",
            NumeroSerie = detailsEntered ? solicitud.NumeroSerie : null,
            SerialDesconocido = detailsEntered && solicitud.SerialDesconocido,
            MarcaModelo = detailsEntered ? solicitud.MarcaModelo : null,
            Ubicacion = detailsEntered ? (solicitud.Ubicacion ?? "") : "",
            ArchivosExistentes = MapExistingFiles(solicitud)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> WaterHeaterFlushSetup(
        WaterHeaterFlushSetupViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(WaterHeaterFlushService), new { id = solicitud.HomeCarePriorityId });
        }

        if (model.SerialDesconocido)
        {
            model.NumeroSerie = null;
            ModelState.Remove(nameof(model.NumeroSerie));
        }

        if (!ModelState.IsValid)
        {
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }

        try
        {
            var userId = RequireUserId()!;
            solicitud.TipoCalentador = model.TipoCalentador;
            solicitud.FuenteEnergia = model.FuenteEnergia;
            solicitud.NumeroSerie = model.NumeroSerie?.Trim();
            solicitud.SerialDesconocido = model.SerialDesconocido;
            solicitud.MarcaModelo = model.MarcaModelo?.Trim();
            solicitud.Ubicacion = model.Ubicacion;
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await SaveFilesAsync(solicitud, userId, files);
            if (!ModelState.IsValid)
            {
                model.ArchivosExistentes = MapExistingFiles(solicitud);
                return View(model);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(WaterHeaterFlushDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your water heater details. Please ensure the Water Heater Flush flow tables exist in the database and try again.");
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterFlushDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(await BuildDetailsViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WaterHeaterFlushDetails(WaterHeaterFlushDetailsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(WaterHeaterFlushSetup), new { id = solicitud.Id });
        }

        if (string.IsNullOrWhiteSpace(model.SintomasSeleccionados))
        {
            model.SintomasSeleccionados = "NoIssues";
        }

        if (string.Equals(model.PreferenciaTiempo, "ChooseDate", StringComparison.OrdinalIgnoreCase)
            && (!model.FechaVisita.HasValue || model.FechaVisita.Value.Date < DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.FechaVisita), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            var details = await BuildDetailsViewModelAsync(solicitud);
            details.UltimoFlush = model.UltimoFlush;
            details.SintomasSeleccionados = model.SintomasSeleccionados;
            details.TipoServicio = model.TipoServicio;
            details.PreferenciaTiempo = model.PreferenciaTiempo;
            details.FechaVisita = model.FechaVisita;
            details.NotasAdicionales = model.NotasAdicionales;
            return View(details);
        }

        try
        {
            await EnsureContactDefaultsAsync(solicitud);

            solicitud.UltimoFlush = model.UltimoFlush;
            solicitud.SintomasSeleccionados = model.SintomasSeleccionados;
            solicitud.TipoServicio = model.TipoServicio;
            solicitud.RecordatorioAnual = string.Equals(model.TipoServicio, "YearlyReminder", StringComparison.OrdinalIgnoreCase);
            solicitud.PreferenciaTiempo = model.PreferenciaTiempo;
            solicitud.FechaVisita = ResolveVisitDate(model.PreferenciaTiempo, model.FechaVisita);
            solicitud.NotasAdicionales = model.NotasAdicionales?.Trim();
            solicitud.PrecioEstimado = WaterHeaterFlushPricingService.GetEstimatedPrice();
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(WaterHeaterFlushConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your water heater flush request. Please ensure the Water Heater Flush flow tables exist in the database and try again.");
            return View(await BuildDetailsViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> WaterHeaterFlushConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(WaterHeaterFlushDetails), new { id = solicitud.Id });
        }

        var landing = await _db.WaterHeaterFlushServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return View(new WaterHeaterFlushConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? solicitud.HomeCarePriority?.Nombre ?? "Water Heater Flush",
            FrequencyLabel = WaterHeaterFlushDisplayLabels.FormatServiceType(solicitud.TipoServicio, solicitud.RecordatorioAnual),
            PreferredTimeLabel = WaterHeaterFlushDisplayLabels.FormatPreferredTime(solicitud.PreferenciaTiempo, solicitud.FechaVisita),
            UnitInfoLabel = "Complete",
            PrecioEstimado = solicitud.PrecioEstimado ?? WaterHeaterFlushPricingService.StartingPrice,
            Moneda = "USD"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, WaterHeaterFlushServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await _db.WaterHeaterFlushServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

        if (landing == null)
        {
            landing = new WaterHeaterFlushServicioLanding
            {
                HomeCarePriorityId = priorityId,
                PageTitle = "Water Heater Flush",
                LandingTitulo = priority.Nombre,
                LandingTagline = "Recommended yearly",
                LandingSubtitulo = "Recommended yearly to keep your system clean and efficient.",
                ImagenUrl = priority.ImagenUrl ?? "/priority-water-heater-flush.png",
                PrecioDesde = WaterHeaterFlushPricingService.StartingPrice,
                PrecioTexto = WaterHeaterFlushDisplayLabels.FormatPrice(WaterHeaterFlushPricingService.StartingPrice),
                IncluyeItems = "Remove sediment buildup|Improve efficiency|Extend tank life",
                IncluyeIconos = "fa-water|fa-leaf|fa-shield-halved",
                PreviewItems = "Serial number|Last maintenance|Any symptoms|Preferred date",
                PreviewIconos = "fa-barcode|fa-calendar|fa-circle-question|fa-calendar-check",
                InfoBoxTexto = "Over time, sediment builds up at the bottom of your water heater tank. This can reduce efficiency, cause rumbling noises, and shorten the life of your system.",
                ResumenServicioTexto = "Annual flush + basic visual check",
                CtaTexto = "Continue"
            };
        }

        return (priority, landing);
    }

    private static WaterHeaterFlushServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        WaterHeaterFlushServicioLanding landing,
        SolicitudWaterHeaterFlush? existing,
        WaterHeaterFlushServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? priority.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : WaterHeaterFlushPricingService.StartingPrice,
            PrecioTexto = landing.PrecioTexto ?? WaterHeaterFlushDisplayLabels.FormatPrice(landing.PrecioDesde),
            BenefitItems = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos),
            PreviewItems = SplitPipePairs(landing.PreviewItems, landing.PreviewIconos),
            WhyItMattersTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto
        };

    private async Task<WaterHeaterFlushDetailsViewModel> BuildDetailsViewModelAsync(SolicitudWaterHeaterFlush solicitud)
    {
        var landing = await _db.WaterHeaterFlushServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return new WaterHeaterFlushDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Maintenance details",
            UltimoFlush = solicitud.UltimoFlush ?? "",
            SintomasSeleccionados = solicitud.SintomasSeleccionados ?? "",
            TipoServicio = solicitud.TipoServicio ?? "",
            PreferenciaTiempo = solicitud.PreferenciaTiempo ?? "",
            FechaVisita = solicitud.FechaVisita ?? GetNextAvailableDate(),
            NotasAdicionales = solicitud.NotasAdicionales,
            ResumenServicioTexto = landing?.ResumenServicioTexto ?? "Annual flush + basic visual check",
            PrecioEstimado = WaterHeaterFlushPricingService.GetEstimatedPrice(),
            PrecioTexto = landing?.PrecioTexto ?? WaterHeaterFlushDisplayLabels.FormatPrice(WaterHeaterFlushPricingService.StartingPrice)
        };
    }

    private async Task EnsureContactDefaultsAsync(SolicitudWaterHeaterFlush solicitud)
    {
        var userId = RequireUserId();
        if (userId == null) return;

        if (string.IsNullOrWhiteSpace(solicitud.DireccionPropiedad))
        {
            var address = await _db.Propiedades.AsNoTracking()
                .Where(p => p.UserId == userId && p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(address))
            {
                solicitud.DireccionPropiedad = address;
            }
        }

        if (string.IsNullOrWhiteSpace(solicitud.TelefonoContacto))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (!string.IsNullOrWhiteSpace(user?.Telefono))
            {
                solicitud.TelefonoContacto = user.Telefono;
            }
        }
    }

    private static DateTime? ResolveVisitDate(string? preferencia, DateTime? chosenDate) =>
        preferencia switch
        {
            "ThisWeek" => DateTime.Today.AddDays(3),
            "ChooseDate" => chosenDate?.Date,
            _ => DateTime.Today.AddDays(1)
        };

    private static DateTime GetNextAvailableDate()
    {
        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static List<WaterHeaterFlushFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new WaterHeaterFlushFeatureItemViewModel
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

    private async Task<SolicitudWaterHeaterFlush?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesWaterHeaterFlush
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudWaterHeaterFlush> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudWaterHeaterFlush? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesWaterHeaterFlush
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudWaterHeaterFlush
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesWaterHeaterFlush.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudWaterHeaterFlush?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        var query = _db.SolicitudesWaterHeaterFlush
            .Include(s => s.HomeCarePriority)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private static List<ExistingWaterHeaterFlushFileViewModel> MapExistingFiles(SolicitudWaterHeaterFlush solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingWaterHeaterFlushFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SaveFilesAsync(SolicitudWaterHeaterFlush solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosWaterHeaterFlush
            .CountAsync(a => a.SolicitudWaterHeaterFlushId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "water-heater-flush", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in incoming)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", $"File type not allowed: {file.FileName}. Use JPG or PNG.");
                continue;
            }

            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError("", $"File too large: {file.FileName}. Max 10 MB.");
                continue;
            }

            var storedName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, storedName);
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            _db.ArchivosWaterHeaterFlush.Add(new ArchivoWaterHeaterFlush
            {
                SolicitudWaterHeaterFlushId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/water-heater-flush/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudWaterHeaterFlush solicitud)
    {
        if (!solicitud.PropiedadId.HasValue)
        {
            return;
        }

        const string title = "Water Heater Flush";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Type: {WaterHeaterFlushDisplayLabels.FormatHeaterType(solicitud.TipoCalentador)} | " +
                    $"Power: {WaterHeaterFlushDisplayLabels.FormatPowerSource(solicitud.FuenteEnergia)} | " +
                    $"Symptoms: {WaterHeaterFlushDisplayLabels.FormatSymptomsList(solicitud.SintomasSeleccionados)} | " +
                    $"Timing: {WaterHeaterFlushDisplayLabels.FormatPreferredTime(solicitud.PreferenciaTiempo, solicitud.FechaVisita)}";

        if (existing != null)
        {
            existing.DueDate = solicitud.FechaVisita;
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
                DueDate = solicitud.FechaVisita,
                Status = "Upcoming",
                Notes = notes,
                FechaCreacion = DateTime.UtcNow
            });
        }
    }
}
