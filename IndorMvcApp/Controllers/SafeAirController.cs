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
public class SafeAirController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<SafeAirController> _logger;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 3;

    public SafeAirController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        ILogger<SafeAirController> logger)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> SafeAirService(int id)
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
    public async Task<IActionResult> SafeAirService(SafeAirServiceViewModel model, string? action)
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
            solicitud.TipoNecesidad = string.Equals(action, "changed", StringComparison.OrdinalIgnoreCase)
                ? "ChangedMyself"
                : "IndorReplaces";
            solicitud.Estado = "InProgress";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(SafeAirDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your Safe Air 365 request. Please ensure the Safe Air flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Microservicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SafeAirDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new SafeAirDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            PageTitle = solicitud.Microservicio?.Nombre ?? "Safe Air 365",
            TipoNecesidad = solicitud.TipoNecesidad ?? "IndorReplaces",
            CantidadFiltros = solicitud.CantidadFiltros ?? "One",
            FiltroAncho = solicitud.FiltroAncho,
            FiltroAlto = solicitud.FiltroAlto,
            FiltroProfundidad = solicitud.FiltroProfundidad,
            FiltroTamanioDesconocido = solicitud.FiltroTamanioDesconocido,
            UbicacionFiltro = solicitud.UbicacionFiltro ?? "Ceiling",
            ProveedorFiltro = solicitud.ProveedorFiltro ?? "IndorBrings",
            RecordatorioActivo = solicitud.RecordatorioActivo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SafeAirDetails(SafeAirDetailsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SafeAirService), new { id = solicitud.MicroservicioId });
        }

        if (model.FiltroTamanioDesconocido)
        {
            model.FiltroAncho = null;
            model.FiltroAlto = null;
            model.FiltroProfundidad = null;
            ModelState.Remove(nameof(model.FiltroAncho));
            ModelState.Remove(nameof(model.FiltroAlto));
            ModelState.Remove(nameof(model.FiltroProfundidad));
        }

        if (string.Equals(model.TipoNecesidad, "RemindOnly", StringComparison.OrdinalIgnoreCase))
        {
            model.ProveedorFiltro = null;
            ModelState.Remove(nameof(model.ProveedorFiltro));
        }

        if (!model.FiltroTamanioDesconocido)
        {
            ValidateFilterSizeFormValue(Request.Form["FiltroAncho"].ToString(), nameof(model.FiltroAncho));
            ValidateFilterSizeFormValue(Request.Form["FiltroAlto"].ToString(), nameof(model.FiltroAlto));
            ValidateFilterSizeFormValue(Request.Form["FiltroProfundidad"].ToString(), nameof(model.FiltroProfundidad));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.TipoNecesidad = model.TipoNecesidad;
            solicitud.CantidadFiltros = model.CantidadFiltros;
            solicitud.FiltroAncho = model.FiltroAncho;
            solicitud.FiltroAlto = model.FiltroAlto;
            solicitud.FiltroProfundidad = model.FiltroProfundidad;
            solicitud.FiltroTamanioDesconocido = model.FiltroTamanioDesconocido;
            solicitud.UbicacionFiltro = model.UbicacionFiltro;
            solicitud.ProveedorFiltro = model.ProveedorFiltro;
            solicitud.RecordatorioActivo = model.RecordatorioActivo;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(SafeAirSchedule), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save filter details. Please ensure the Safe Air flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> SafeAirSchedule(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(BuildScheduleViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(40_000_000)]
    public async Task<IActionResult> SafeAirSchedule(
        SafeAirScheduleViewModel model,
        string? action,
        [FromForm] List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SafeAirDetails), new { id = solicitud.Id });
        }

        var userId = RequireUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("LoginForm", "Account");
        }

        if (files != null && files.Count > 0)
        {
            await SaveFilesAsync(solicitud, userId, files);
        }

        if (!ModelState.IsValid)
        {
            return View(MergeScheduleViewModel(solicitud, model));
        }

        try
        {
            solicitud.VentanaTiempo = model.VentanaTiempo;
            solicitud.DetallesAcceso = model.DetallesAcceso;
            solicitud.NotasAcceso = model.NotasAcceso?.Trim();
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            if (solicitud.RecordatorioActivo)
            {
                solicitud.FechaProximoRecordatorio = DateTime.Today.AddMonths(3);
            }

            await UpsertProgramacionAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(SafeAirConfirmed), new { id = solicitud.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SafeAir schedule confirm failed for solicitud {SolicitudId}", solicitud.Id);
            ModelState.AddModelError("",
                "Could not confirm your request. Please try again. If uploading photos, use JPG or PNG under 10 MB each.");
            return View(MergeScheduleViewModel(solicitud, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SafeAirConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(SafeAirSchedule), new { id = solicitud.Id });
        }

        return View(new SafeAirConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            NombreServicio = solicitud.Microservicio?.Nombre ?? "Safe Air 365",
            TipoNecesidad = solicitud.TipoNecesidad ?? "IndorReplaces",
            VisitaLabel = SafeAirDisplayLabels.FormatVisitLabel(solicitud.TipoNecesidad, solicitud.VentanaTiempo),
            FiltroTamanioLabel = SafeAirDisplayLabels.FormatFilterSizeSummary(
                solicitud.FiltroAncho, solicitud.FiltroAlto, solicitud.FiltroProfundidad, solicitud.FiltroTamanioDesconocido),
            ProveedorLabel = string.Equals(solicitud.TipoNecesidad, "IndorReplaces", StringComparison.OrdinalIgnoreCase)
                ? "INDOR partner"
                : "Self-service",
            RecordatorioLabel = SafeAirDisplayLabels.FormatReminder(solicitud.RecordatorioActivo),
            FechaProximoRecordatorio = solicitud.FechaProximoRecordatorio,
            TieneFotos = solicitud.Archivos.Count > 0,
            RecordatorioActivo = solicitud.RecordatorioActivo
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(Microservicio Microservicio, SafeAirServicioLanding Landing)?> LoadLandingBundleAsync(int microservicioId)
    {
        var microservicio = await _db.Microservicios.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == microservicioId && m.Activo);
        if (microservicio == null) return null;

        var landing = await _db.SafeAirServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.MicroservicioId == microservicioId && l.Activo);

        if (landing == null)
        {
            landing = new SafeAirServicioLanding
            {
                MicroservicioId = microservicioId,
                PageTitle = microservicio.Nombre,
                LandingTitulo = microservicio.Nombre,
                LandingTagline = microservicio.Subtitulo,
                LandingSubtitulo = microservicio.DescripcionCompleta ?? microservicio.Descripcion,
                ImagenUrl = microservicio.ImagenUrl,
                PrecioDesde = microservicio.Valor > 0 ? microservicio.Valor : 49,
                PrecioTexto = $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 49):0} for provider replacement"
            };
        }

        return (microservicio, landing);
    }

    private static SafeAirServiceViewModel BuildServiceViewModel(
        Microservicio microservicio,
        SafeAirServicioLanding landing,
        SolicitudSafeAir? existing,
        SafeAirServiceViewModel? posted = null)
    {
        var items = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos);
        if (items.Count == 0)
        {
            items = SplitPipePairs(microservicio.Incluye, null);
        }

        return new SafeAirServiceViewModel
        {
            MicroservicioId = microservicio.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? microservicio.Subtitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? microservicio.ImagenUrl),
            PrecioDesde = landing.PrecioDesde > 0 ? landing.PrecioDesde : (microservicio.Valor > 0 ? microservicio.Valor : 49),
            PrecioTexto = landing.PrecioTexto ?? $"From ${(microservicio.Valor > 0 ? microservicio.Valor : 49):0} for provider replacement",
            IncludedItems = items,
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaScheduleTexto = landing.CtaScheduleTexto,
            CtaChangedTexto = landing.CtaChangedTexto
        };
    }

    private static List<SafeAirFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new SafeAirFeatureItemViewModel
        {
            Text = text,
            Icon = NormalizeFeatureIcon(index < iconItems.Length ? iconItems[index] : null)
        }).ToList();
    }

    private static string NormalizeFeatureIcon(string? icon)
    {
        var value = (icon ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(value))
        {
            return "fa-check";
        }

        if (!value.StartsWith("fa-", StringComparison.Ordinal))
        {
            value = $"fa-{value}";
        }

        return value.Equals("fa-user-hard-hat", StringComparison.OrdinalIgnoreCase)
            ? "fa-screwdriver-wrench"
            : value;
    }

    private static string? ResolveImageUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.StartsWith('/') ? url : $"/{url}";

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudSafeAir?> GetActiveSolicitudAsync(string userId, int microservicioId) =>
        await _db.SolicitudesSafeAir
            .Where(s => s.UserId == userId
                        && s.MicroservicioId == microservicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudSafeAir> GetOrCreateSolicitudAsync(
        string userId,
        int microservicioId,
        int? solicitudId)
    {
        SolicitudSafeAir? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesSafeAir
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, microservicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudSafeAir
            {
                UserId = userId,
                MicroservicioId = microservicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                TipoNecesidad = "IndorReplaces",
                CantidadFiltros = "One",
                UbicacionFiltro = "Ceiling",
                ProveedorFiltro = "IndorBrings",
                RecordatorioActivo = true
            };
            _db.SolicitudesSafeAir.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudSafeAir?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudSafeAir> query = _db.SolicitudesSafeAir
            .Include(s => s.Microservicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private SafeAirScheduleViewModel BuildScheduleViewModel(SolicitudSafeAir solicitud) =>
        new()
        {
            SolicitudId = solicitud.Id,
            MicroservicioId = solicitud.MicroservicioId,
            PageTitle = solicitud.Microservicio?.Nombre ?? "Safe Air 365",
            TipoNecesidad = solicitud.TipoNecesidad ?? "IndorReplaces",
            VentanaTiempo = solicitud.VentanaTiempo ?? "NextAvailable",
            DetallesAcceso = solicitud.DetallesAcceso ?? "House",
            NotasAcceso = solicitud.NotasAcceso,
            NombreServicio = solicitud.Microservicio?.Nombre ?? "Safe Air 365",
            CantidadFiltrosLabel = SafeAirDisplayLabels.FormatFilterCount(solicitud.CantidadFiltros),
            FiltroTamanioLabel = SafeAirDisplayLabels.FormatFilterSizeSummary(
                solicitud.FiltroAncho, solicitud.FiltroAlto, solicitud.FiltroProfundidad, solicitud.FiltroTamanioDesconocido),
            ProveedorFiltroLabel = SafeAirDisplayLabels.FormatProviderLong(solicitud.ProveedorFiltro),
            RecordatorioLabel = SafeAirDisplayLabels.FormatReminder(solicitud.RecordatorioActivo),
            ShowScheduleOptions = string.Equals(solicitud.TipoNecesidad, "IndorReplaces", StringComparison.OrdinalIgnoreCase),
            ArchivosExistentes = MapExistingFiles(solicitud)
        };

    private SafeAirScheduleViewModel MergeScheduleViewModel(SolicitudSafeAir solicitud, SafeAirScheduleViewModel posted)
    {
        var vm = BuildScheduleViewModel(solicitud);
        vm.VentanaTiempo = posted.VentanaTiempo;
        vm.DetallesAcceso = posted.DetallesAcceso;
        vm.NotasAcceso = posted.NotasAcceso;
        return vm;
    }

    private static List<ExistingSafeAirFileViewModel> MapExistingFiles(SolicitudSafeAir solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingSafeAirFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SaveFilesAsync(SolicitudSafeAir solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosSafeAir
            .CountAsync(a => a.SolicitudSafeAirId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "safe-air", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in incoming)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)
                && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ext = ".jpg";
            }

            var allowedType = AllowedExtensions.Contains(ext)
                || file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            if (!allowedType)
            {
                ModelState.AddModelError("", $"File type not allowed: {file.FileName}. Use JPG or PNG.");
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

            _db.ArchivosSafeAir.Add(new ArchivoSafeAir
            {
                SolicitudSafeAirId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/safe-air/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private async Task UpsertProgramacionAsync(SolicitudSafeAir solicitud)
    {
        if (!solicitud.RecordatorioActivo && !string.Equals(solicitud.TipoNecesidad, "IndorReplaces", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fechaProgramada = solicitud.FechaProximoRecordatorio
            ?? DateTime.Today.AddMonths(3);

        if (string.Equals(solicitud.TipoNecesidad, "IndorReplaces", StringComparison.OrdinalIgnoreCase))
        {
            fechaProgramada = DateTime.Today.AddDays(1);
        }

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

    private static string BuildScheduleNotes(SolicitudSafeAir solicitud)
    {
        var parts = new List<string>
        {
            $"Need: {SafeAirDisplayLabels.FormatNeedType(solicitud.TipoNecesidad)}",
            $"Filters: {SafeAirDisplayLabels.FormatFilterCount(solicitud.CantidadFiltros)}",
            $"Size: {SafeAirDisplayLabels.FormatFilterSizeSummary(solicitud.FiltroAncho, solicitud.FiltroAlto, solicitud.FiltroProfundidad, solicitud.FiltroTamanioDesconocido)}"
        };

        if (!string.IsNullOrWhiteSpace(solicitud.VentanaTiempo))
        {
            parts.Add($"Time: {SafeAirDisplayLabels.FormatTimeWindow(solicitud.VentanaTiempo)}");
        }

        return string.Join(" | ", parts);
    }

    private void ValidateFilterSizeFormValue(string raw, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return;
        }

        if (!decimal.TryParse(raw.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            ModelState.Remove(fieldName);
            ModelState.AddModelError(fieldName, "Please enter a number.");
        }
    }
}
