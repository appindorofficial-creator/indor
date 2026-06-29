using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class HvacMaintenanceController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 2;

    public HvacMaintenanceController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> HvacMaintenanceService(int id)
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
    public async Task<IActionResult> HvacMaintenanceService(HvacMaintenanceServiceViewModel model, string? action)
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
            return RedirectToAction(nameof(HvacMaintenanceDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your HVAC tune-up request. Please ensure the HVAC Maintenance flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> HvacMaintenanceDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new HvacMaintenanceDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "HVAC Tune-Up",
            NumeroSerieAc = solicitud.NumeroSerieAc,
            SerialDesconocido = solicitud.SerialDesconocido,
            UltimoMantenimientoDesconocido = solicitud.UltimoMantenimientoDesconocido,
            FechaUltimoMantenimiento = ParseMaintenanceDate(solicitud.UltimoMantenimiento),
            TamanioFiltro = solicitud.TamanioFiltro,
            NotasTecnico = solicitud.NotasTecnico,
            ArchivosExistentes = MapExistingFiles(solicitud)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> HvacMaintenanceDetails(
        HvacMaintenanceDetailsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(HvacMaintenanceService), new { id = solicitud.HomeCarePriorityId });
        }

        if (model.SerialDesconocido)
        {
            model.NumeroSerieAc = null;
            ModelState.Remove(nameof(model.NumeroSerieAc));
        }
        else if (string.IsNullOrWhiteSpace(model.NumeroSerieAc))
        {
            ModelState.AddModelError(nameof(model.NumeroSerieAc), "Enter the AC serial number or toggle \"I don't know\".");
        }

        if (model.UltimoMantenimientoDesconocido)
        {
            model.FechaUltimoMantenimiento = null;
            ModelState.Remove(nameof(model.FechaUltimoMantenimiento));
        }
        else if (model.FechaUltimoMantenimiento.HasValue
                 && model.FechaUltimoMantenimiento.Value.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaUltimoMantenimiento),
                "Last maintenance date cannot be in the future.");
        }

        if (!ModelState.IsValid)
        {
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }

        try
        {
            var userId = RequireUserId()!;
            solicitud.NumeroSerieAc = model.NumeroSerieAc?.Trim();
            solicitud.SerialDesconocido = model.SerialDesconocido;
            solicitud.UltimoMantenimiento = model.UltimoMantenimientoDesconocido
                ? null
                : FormatMaintenanceDate(model.FechaUltimoMantenimiento);
            solicitud.UltimoMantenimientoDesconocido = model.UltimoMantenimientoDesconocido;
            solicitud.TamanioFiltro = model.TamanioFiltro?.Trim();
            solicitud.NotasTecnico = model.NotasTecnico?.Trim();
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await SaveFilesAsync(solicitud, userId, files);
            if (!ModelState.IsValid)
            {
                model.ArchivosExistentes = MapExistingFiles(solicitud);
                return View(model);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(HvacMaintenanceSchedule), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your system details. Please ensure the HVAC Maintenance flow tables exist in the database and try again.");
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> HvacMaintenanceSchedule(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(await BuildScheduleViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HvacMaintenanceSchedule(HvacMaintenanceScheduleViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(HvacMaintenanceDetails), new { id = solicitud.Id });
        }

        await EnsureContactDefaultsAsync(model);

        if (model.FechaVisita.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaVisita), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            var schedule = await BuildScheduleViewModelAsync(solicitud);
            schedule.FechaVisita = model.FechaVisita;
            schedule.VentanaHorario = model.VentanaHorario;
            schedule.TipoServicio = model.TipoServicio;
            schedule.DireccionPropiedad = model.DireccionPropiedad;
            schedule.TelefonoContacto = model.TelefonoContacto;
            schedule.MinVisitDateIso = DateTime.Today.ToString("yyyy-MM-dd");
            return View(schedule);
        }

        try
        {
            solicitud.FechaVisita = model.FechaVisita.Date;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.TipoServicio = model.TipoServicio;
            solicitud.RecordatorioAnual = string.Equals(model.TipoServicio, "YearlyReminder", StringComparison.OrdinalIgnoreCase);
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TelefonoContacto = model.TelefonoContacto.Trim();
            solicitud.PrecioEstimado = HvacMaintenancePricingService.GetEstimatedPrice(model.TipoServicio);
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(HvacMaintenanceConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your HVAC tune-up request. Please ensure the HVAC Maintenance flow tables exist in the database and try again.");
            return View(await BuildScheduleViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> HvacMaintenanceConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(HvacMaintenanceSchedule), new { id = solicitud.Id });
        }

        var landing = await _db.HvacMaintenanceServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return View(new HvacMaintenanceConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            NombreServicio = landing?.LandingTitulo ?? solicitud.HomeCarePriority?.Nombre ?? "HVAC Tune-Up",
            SerialLabel = HvacMaintenanceDisplayLabels.FormatSerial(solicitud.NumeroSerieAc, solicitud.SerialDesconocido),
            LastMaintenanceLabel = HvacMaintenanceDisplayLabels.FormatLastMaintenance(
                solicitud.UltimoMantenimiento, solicitud.UltimoMantenimientoDesconocido),
            VisitWindowLabel = HvacMaintenanceDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario),
            ReminderLabel = HvacMaintenanceDisplayLabels.FormatServiceType(solicitud.TipoServicio, solicitud.RecordatorioAnual),
            PrecioEstimado = solicitud.PrecioEstimado ?? HvacMaintenancePricingService.StartingPrice,
            Moneda = "USD"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, HvacMaintenanceServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await _db.HvacMaintenanceServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

        if (landing == null)
        {
            landing = new HvacMaintenanceServicioLanding
            {
                HomeCarePriorityId = priorityId,
                PageTitle = "HVAC Tune-Up",
                LandingTitulo = priority.Nombre,
                LandingTagline = priority.Subtitulo,
                LandingSubtitulo = "Yearly preventive maintenance to keep your air conditioning system running efficiently and reliably.",
                ImagenUrl = priority.ImagenUrl ?? "/priority-hvac-maintenance.png",
                PrecioDesde = HvacMaintenancePricingService.StartingPrice,
                PrecioTexto = HvacMaintenanceDisplayLabels.FormatPrice(HvacMaintenancePricingService.StartingPrice),
                IncluyeItems = "System inspection|Filter check|Performance test|Basic tune-up",
                IncluyeIconos = "fa-screwdriver-wrench|fa-filter|fa-gauge-high|fa-fan",
                PreviewItems = "AC serial number|Last maintenance date (if known)|Preferred visit time",
                PreviewIconos = "fa-barcode|fa-calendar|fa-clock",
                CtaTexto = "Start tune-up request"
            };
        }

        return (priority, landing);
    }

    private static HvacMaintenanceServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        HvacMaintenanceServicioLanding landing,
        SolicitudHvacMaintenance? existing,
        HvacMaintenanceServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? priority.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : HvacMaintenancePricingService.StartingPrice,
            PrecioTexto = landing.PrecioTexto ?? HvacMaintenanceDisplayLabels.FormatPrice(landing.PrecioDesde),
            IncludedItems = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos),
            PreviewItems = SplitPipePairs(landing.PreviewItems, landing.PreviewIconos),
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto
        };

    private async Task<HvacMaintenanceScheduleViewModel> BuildScheduleViewModelAsync(SolicitudHvacMaintenance solicitud)
    {
        var landing = await _db.HvacMaintenanceServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        var userId = RequireUserId();
        var user = userId != null ? await _userManager.FindByIdAsync(userId) : null;

        var address = solicitud.DireccionPropiedad;
        if (string.IsNullOrWhiteSpace(address) && solicitud.PropiedadId.HasValue)
        {
            address = await _db.Propiedades.AsNoTracking()
                .Where(p => p.Id == solicitud.PropiedadId)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrWhiteSpace(address) && userId != null)
        {
            address = await _db.Propiedades.AsNoTracking()
                .Where(p => p.UserId == userId && p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();
        }

        var phone = solicitud.TelefonoContacto;
        if (string.IsNullOrWhiteSpace(phone))
        {
            phone = user?.Telefono;
        }

        var tipoServicio = solicitud.TipoServicio ?? "OneTime";

        return new HvacMaintenanceScheduleViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "HVAC Tune-Up",
            FechaVisita = NormalizeVisitDate(solicitud.FechaVisita),
            VentanaHorario = solicitud.VentanaHorario ?? "Morning",
            TipoServicio = tipoServicio,
            DireccionPropiedad = address ?? string.Empty,
            TelefonoContacto = phone ?? string.Empty,
            MinVisitDateIso = DateTime.Today.ToString("yyyy-MM-dd"),
            PrecioEstimado = HvacMaintenancePricingService.GetEstimatedPrice(tipoServicio),
            PrecioTexto = landing?.PrecioTexto ?? HvacMaintenanceDisplayLabels.FormatPrice(HvacMaintenancePricingService.StartingPrice),
            InfoBoxTexto = landing?.InfoBoxTexto
        };
    }

    private async Task EnsureContactDefaultsAsync(HvacMaintenanceScheduleViewModel model)
    {
        var userId = RequireUserId();
        if (userId == null) return;

        if (string.IsNullOrWhiteSpace(model.DireccionPropiedad))
        {
            var address = await _db.Propiedades.AsNoTracking()
                .Where(p => p.UserId == userId && p.Activo)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(address))
            {
                model.DireccionPropiedad = address;
                ModelState.Remove(nameof(model.DireccionPropiedad));
            }
        }

        if (string.IsNullOrWhiteSpace(model.TelefonoContacto))
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (!string.IsNullOrWhiteSpace(user?.Telefono))
            {
                model.TelefonoContacto = user.Telefono;
                ModelState.Remove(nameof(model.TelefonoContacto));
            }
        }
    }

    private static DateTime GetNextAvailableDate()
    {
        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static DateTime NormalizeVisitDate(DateTime? date)
    {
        var today = DateTime.Today;
        if (date is { } value && value.Date >= today)
        {
            return value.Date;
        }

        return GetNextAvailableDate();
    }

    private static List<HvacMaintenanceFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new HvacMaintenanceFeatureItemViewModel
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

    private async Task<SolicitudHvacMaintenance?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesHvacMaintenance
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudHvacMaintenance> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudHvacMaintenance? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesHvacMaintenance
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudHvacMaintenance
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                VentanaHorario = "Morning",
                TipoServicio = "OneTime"
            };
            _db.SolicitudesHvacMaintenance.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudHvacMaintenance?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        var query = _db.SolicitudesHvacMaintenance
            .Include(s => s.HomeCarePriority)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private static List<ExistingHvacMaintenanceFileViewModel> MapExistingFiles(SolicitudHvacMaintenance solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingHvacMaintenanceFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SaveFilesAsync(SolicitudHvacMaintenance solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosHvacMaintenance
            .CountAsync(a => a.SolicitudHvacMaintenanceId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "hvac-maintenance", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in incoming)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)
                && !string.IsNullOrWhiteSpace(file.ContentType)
                && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ext = ".jpg";
            }

            var allowedType = AllowedExtensions.Contains(ext)
                || (!string.IsNullOrWhiteSpace(file.ContentType)
                    && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));
            if (!allowedType)
            {
                ModelState.AddModelError("", $"File type not allowed: {file.FileName}. Use JPG, PNG, or HEIC.");
                continue;
            }

            if (!AllowedExtensions.Contains(ext))
            {
                ext = ".jpg";
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

            _db.ArchivosHvacMaintenance.Add(new ArchivoHvacMaintenance
            {
                SolicitudHvacMaintenanceId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/hvac-maintenance/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudHvacMaintenance solicitud)
    {
        if (!solicitud.PropiedadId.HasValue)
        {
            return;
        }

        var title = "HVAC Tune-Up";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Serial: {HvacMaintenanceDisplayLabels.FormatSerial(solicitud.NumeroSerieAc, solicitud.SerialDesconocido)} | " +
                    $"Window: {HvacMaintenanceDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario)} | " +
                    $"Reminder: {HvacMaintenanceDisplayLabels.FormatServiceType(solicitud.TipoServicio, solicitud.RecordatorioAnual)}";

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

    private static DateTime? ParseMaintenanceDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var isoDate))
        {
            return isoDate.Date;
        }

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date.Date
            : null;
    }

    private static string? FormatMaintenanceDate(DateTime? value) =>
        value?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
