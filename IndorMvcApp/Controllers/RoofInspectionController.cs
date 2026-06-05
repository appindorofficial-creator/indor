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
public class RoofInspectionController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 5;

    public RoofInspectionController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> RoofInspectionService(int id)
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
    public async Task<IActionResult> RoofInspectionService(RoofInspectionServiceViewModel model, string? action)
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
            return RedirectToAction(nameof(RoofInspectionDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your roof inspection request. Please ensure the Roof Inspection flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> RoofInspectionDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new RoofInspectionDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Roof Inspection",
            MotivoRevision = solicitud.MotivoRevision ?? "RoutineInspection",
            TipoTecho = solicitud.TipoTecho ?? "AsphaltShingle",
            EdadTecho = solicitud.EdadTecho ?? "NotSure",
            UltimaInspeccion = solicitud.UltimaInspeccion ?? "DontKnow",
            ArchivosExistentes = MapExistingFiles(solicitud)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> RoofInspectionDetails(
        RoofInspectionDetailsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RoofInspectionService), new { id = solicitud.HomeCarePriorityId });
        }

        if (!ModelState.IsValid)
        {
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }

        try
        {
            var userId = RequireUserId()!;
            solicitud.MotivoRevision = model.MotivoRevision;
            solicitud.TipoTecho = model.TipoTecho;
            solicitud.EdadTecho = model.EdadTecho;
            solicitud.UltimaInspeccion = model.UltimaInspeccion;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await SaveFilesAsync(solicitud, userId, files);
            if (!ModelState.IsValid)
            {
                model.ArchivosExistentes = MapExistingFiles(solicitud);
                return View(model);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(RoofInspectionSchedule), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your roof details. Please ensure the Roof Inspection flow tables exist in the database and try again.");
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RoofInspectionSchedule(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(await BuildScheduleViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoofInspectionSchedule(RoofInspectionScheduleViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RoofInspectionDetails), new { id = solicitud.Id });
        }

        if (model.FechaPreferida.HasValue && model.FechaPreferida.Value.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaPreferida), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            var schedule = await BuildScheduleViewModelAsync(solicitud);
            schedule.TipoServicio = model.TipoServicio;
            schedule.Frecuencia = model.Frecuencia;
            schedule.TimingPreferido = model.TimingPreferido;
            schedule.FechaPreferida = model.FechaPreferida;
            schedule.Notas = model.Notas;
            return View(schedule);
        }

        try
        {
            await EnsureAddressAsync(solicitud);

            solicitud.TipoServicio = model.TipoServicio;
            solicitud.Frecuencia = model.Frecuencia;
            solicitud.TimingPreferido = model.TimingPreferido;
            solicitud.FechaPreferida = model.FechaPreferida?.Date;
            solicitud.Notas = model.Notas?.Trim();
            solicitud.PrecioEstimado = RoofInspectionPricingService.GetEstimatedPrice(model.TipoServicio);
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(RoofInspectionConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your roof check request. Please ensure the Roof Inspection flow tables exist in the database and try again.");
            return View(await BuildScheduleViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> RoofInspectionConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RoofInspectionSchedule), new { id = solicitud.Id });
        }

        var landing = await _db.RoofInspectionServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return View(new RoofInspectionConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? solicitud.HomeCarePriority?.Nombre ?? "Roof Inspection",
            FrequencyLabel = RoofInspectionDisplayLabels.FormatFrequency(solicitud.Frecuencia),
            TimingLabel = RoofInspectionDisplayLabels.FormatTiming(solicitud.TimingPreferido),
            PropertyLabel = solicitud.DireccionPropiedad ?? "—",
            LastInspectionLabel = RoofInspectionDisplayLabels.FormatLastInspection(solicitud.UltimaInspeccion),
            FocusLabel = RoofInspectionDisplayLabels.FormatFocus(solicitud.MotivoRevision)
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, RoofInspectionServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await _db.RoofInspectionServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

        if (landing == null)
        {
            landing = new RoofInspectionServicioLanding
            {
                HomeCarePriorityId = priorityId,
                PageTitle = "Roof Inspection",
                LandingTitulo = priority.Nombre,
                LandingSubtitulo = "Regular roof inspections help catch loose shingles, failing sealant, damaged flashing, clogged drainage, and leak risks before they become major repairs.",
                ImagenUrl = priority.ImagenUrl ?? "/priority-roof-inspection.png",
                RecomendacionItems = "Visual roof check: spring & fall|Professional inspection: every 1–2 years|After major storms: inspect again|Older roof or active issues: inspect sooner",
                RecomendacionIconos = "fa-calendar|fa-shield-halved|fa-cloud-bolt|fa-clock",
                IncluyeItems = "Shingles|Flashing & sealant|Vents / skylights|Gutters & valleys|Attic moisture signs|Debris / branches",
                IncluyeIconos = "fa-house-chimney|fa-spray-can|fa-wind|fa-water|fa-droplet|fa-leaf",
                InfoBoxTexto = "Vetted professionals. Clear reports. Peace of mind.",
                CtaTexto = "Set roof check"
            };
        }

        return (priority, landing);
    }

    private static RoofInspectionServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        RoofInspectionServicioLanding landing,
        SolicitudRoofInspection? existing,
        RoofInspectionServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            RecommendationItems = SplitPipePairs(landing.RecomendacionItems, landing.RecomendacionIconos),
            CheckItems = SplitPipePairs(landing.IncluyeItems, landing.IncluyeIconos),
            TrustTexto = landing.InfoBoxTexto,
            CtaTexto = landing.CtaTexto
        };

    private async Task<RoofInspectionScheduleViewModel> BuildScheduleViewModelAsync(SolicitudRoofInspection solicitud)
    {
        var landing = await _db.RoofInspectionServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == solicitud.HomeCarePriorityId && l.Activo);

        return new RoofInspectionScheduleViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Roof Inspection",
            ScheduleIntro = "Roofers typically recommend a visual roof check in spring and fall, and a professional inspection every 1–2 years or after major storms.",
            TipoServicio = solicitud.TipoServicio ?? "ReminderOnly",
            Frecuencia = solicitud.Frecuencia ?? "Yearly",
            TimingPreferido = solicitud.TimingPreferido ?? "Spring",
            FechaPreferida = solicitud.FechaPreferida,
            Notas = solicitud.Notas,
            CoverageItems = SplitPipePairs(landing?.IncluyeItems, landing?.IncluyeIconos),
            TrustTexto = landing?.InfoBoxTexto
        };
    }

    private async Task EnsureAddressAsync(SolicitudRoofInspection solicitud)
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

    private static List<RoofInspectionFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new RoofInspectionFeatureItemViewModel
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

    private async Task<SolicitudRoofInspection?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesRoofInspection
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudRoofInspection> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudRoofInspection? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesRoofInspection
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudRoofInspection
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                MotivoRevision = "RoutineInspection",
                TipoTecho = "AsphaltShingle",
                EdadTecho = "NotSure",
                UltimaInspeccion = "DontKnow",
                TipoServicio = "ReminderOnly",
                Frecuencia = "Yearly",
                TimingPreferido = "Spring"
            };
            _db.SolicitudesRoofInspection.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudRoofInspection?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        var query = _db.SolicitudesRoofInspection
            .Include(s => s.HomeCarePriority)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private static List<ExistingRoofInspectionFileViewModel> MapExistingFiles(SolicitudRoofInspection solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingRoofInspectionFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SaveFilesAsync(SolicitudRoofInspection solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosRoofInspection
            .CountAsync(a => a.SolicitudRoofInspectionId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "roof-inspection", solicitud.Id.ToString());
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

            _db.ArchivosRoofInspection.Add(new ArchivoRoofInspection
            {
                SolicitudRoofInspectionId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/roof-inspection/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudRoofInspection solicitud)
    {
        if (!solicitud.PropiedadId.HasValue)
        {
            return;
        }

        const string title = "Roof Inspection";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Reason: {RoofInspectionDisplayLabels.FormatReason(solicitud.MotivoRevision)} | " +
                    $"Roof: {RoofInspectionDisplayLabels.FormatRoofType(solicitud.TipoTecho)} | " +
                    $"Frequency: {RoofInspectionDisplayLabels.FormatFrequency(solicitud.Frecuencia)} | " +
                    $"Timing: {RoofInspectionDisplayLabels.FormatTiming(solicitud.TimingPreferido)} | " +
                    $"Service: {RoofInspectionDisplayLabels.FormatServiceType(solicitud.TipoServicio)}";

        var dueDate = solicitud.FechaPreferida
            ?? (string.Equals(solicitud.TimingPreferido, "ThisMonth", StringComparison.OrdinalIgnoreCase)
                ? DateTime.Today.AddDays(14)
                : DateTime.Today.AddMonths(3));

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
