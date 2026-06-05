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
public class ExteriorPaintController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 3;

    public ExteriorPaintController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> ExteriorPaintReview(int id)
    {
        var bundle = await LoadLandingBundleAsync(id);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var existing = await GetActiveSolicitudAsync(userId, id);
        return View(BuildReviewViewModel(bundle.Value.Priority, bundle.Value.Landing, existing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExteriorPaintReview(ExteriorPaintReviewViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.HomeCarePriorityId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(BuildReviewViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.HomeCarePriorityId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.UltimaPintura = model.UltimaPintura;
            solicitud.TipoSuperficie = model.TipoSuperficie;
            solicitud.MantenerMismoColor = model.MantenerMismoColor;
            solicitud.Estado = "ReviewCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ExteriorPaintCondition), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your exterior paint details. Please ensure the Exterior Paint flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExteriorPaintCondition(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);
        return View(new ExteriorPaintConditionViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Exterior Paint Review",
            LandingSubtitulo = landing?.LandingSubtitulo ?? "Help us understand the current condition.",
            ImagenUrl = ResolveImageUrl(landing?.ImagenUrl ?? solicitud.HomeCarePriority?.ImagenUrl),
            ProblemasSeleccionados = solicitud.ProblemasSeleccionados ?? string.Empty,
            AreasSeleccionadas = solicitud.AreasSeleccionadas ?? string.Empty,
            ActualizacionColor = solicitud.ActualizacionColor ?? solicitud.MantenerMismoColor ?? "Yes",
            LavadoPresionReciente = solicitud.LavadoPresionReciente ?? "NotSure"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExteriorPaintCondition(ExteriorPaintConditionViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ExteriorPaintReview), new { id = solicitud.HomeCarePriorityId });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.ProblemasSeleccionados = model.ProblemasSeleccionados;
            solicitud.AreasSeleccionadas = model.AreasSeleccionadas;
            solicitud.ActualizacionColor = model.ActualizacionColor;
            solicitud.LavadoPresionReciente = model.LavadoPresionReciente;
            solicitud.Estado = "ConditionCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ExteriorPaintSchedule), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save condition details. Please ensure the Exterior Paint flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExteriorPaintSchedule(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(await BuildScheduleViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(35_000_000)]
    public async Task<IActionResult> ExteriorPaintSchedule(
        ExteriorPaintScheduleViewModel model,
        string? action,
        IFormFile? frontPhoto,
        IFormFile? backPhoto,
        IFormFile? problemPhoto)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ExteriorPaintCondition), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            var schedule = await BuildScheduleViewModelAsync(solicitud);
            schedule.NumeroPisos = model.NumeroPisos;
            schedule.TimingPreferido = model.TimingPreferido;
            schedule.Notas = model.Notas;
            return View(schedule);
        }

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        try
        {
            await SavePhotoAsync(solicitud, userId, frontPhoto, "Front");
            await SavePhotoAsync(solicitud, userId, backPhoto, "Back");
            await SavePhotoAsync(solicitud, userId, problemPhoto, "ProblemArea");

            if (!ModelState.IsValid)
            {
                return View(await BuildScheduleViewModelAsync(solicitud));
            }

            await EnsureAddressAsync(solicitud);

            solicitud.NumeroPisos = model.NumeroPisos;
            solicitud.TimingPreferido = model.TimingPreferido;
            solicitud.RecordatorioAnual = true;
            solicitud.Notas = model.Notas?.Trim();
            solicitud.PrecioEstimado = 0;
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ExteriorPaintConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your paint review request. Please ensure the Exterior Paint flow tables exist in the database and try again.");
            return View(await BuildScheduleViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExteriorPaintConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ExteriorPaintSchedule), new { id = solicitud.Id });
        }

        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);

        return View(new ExteriorPaintConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? "Exterior Paint Review",
            WhyItMattersItems = SplitPipePairs(landing?.WhyItMattersItems, landing?.WhyItMattersIconos),
            NextStepsItems = SplitPipePairs(landing?.NextStepsItems, landing?.NextStepsIconos),
            ReminderTexto = landing?.ReminderTexto ?? "Check paint condition every year"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, ExteriorPaintServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await GetLandingAsync(priorityId);
        landing ??= new ExteriorPaintServicioLanding
        {
            HomeCarePriorityId = priorityId,
            PageTitle = "Exterior Paint Review",
            LandingTitulo = priority.Nombre,
            LandingTagline = "Recommended every 5 years",
            LandingSubtitulo = "Help us understand your exterior so we can schedule the right paint review.",
            ImagenUrl = priority.ImagenUrl ?? "/priority-exterior-paint.png",
            InfoBoxTexto = "Paint sooner if you see peeling, fading, or damaged caulk.",
            WhyItMattersItems = "Fresh exterior paint protects siding and trim|Annual visual checks help catch peeling and bad caulk early|A full repaint is often needed about every 5 years, depending on material and weather",
            WhyItMattersIconos = "fa-shield-halved|fa-magnifying-glass|fa-calendar",
            NextStepsItems = "We'll review your photos|We'll confirm scope and surface type|We'll help you plan timing and color options",
            NextStepsIconos = "fa-image|fa-clipboard-list|fa-paint-roller",
            ReminderTexto = "Check paint condition every year",
            CtaTexto = "Continue"
        };

        return (priority, landing);
    }

    private async Task<ExteriorPaintServicioLanding?> GetLandingAsync(int priorityId) =>
        await _db.ExteriorPaintServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

    private static ExteriorPaintReviewViewModel BuildReviewViewModel(
        HomeCarePriority priority,
        ExteriorPaintServicioLanding landing,
        SolicitudExteriorPaint? existing,
        ExteriorPaintReviewViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? priority.Subtitulo,
            InfoBoxTexto = landing.InfoBoxTexto,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            CtaTexto = landing.CtaTexto,
            UltimaPintura = existing?.UltimaPintura ?? posted?.UltimaPintura ?? "DontKnow",
            TipoSuperficie = existing?.TipoSuperficie ?? posted?.TipoSuperficie ?? "WoodSiding",
            MantenerMismoColor = existing?.MantenerMismoColor ?? posted?.MantenerMismoColor ?? "Yes"
        };

    private async Task<ExteriorPaintScheduleViewModel> BuildScheduleViewModelAsync(SolicitudExteriorPaint solicitud)
    {
        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);

        return new ExteriorPaintScheduleViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Exterior Paint Review",
            LandingSubtitulo = landing?.LandingSubtitulo ?? "Add a few details before we schedule your review.",
            ImagenUrl = ResolveImageUrl(landing?.ImagenUrl ?? solicitud.HomeCarePriority?.ImagenUrl),
            NumeroPisos = solicitud.NumeroPisos ?? "One",
            TimingPreferido = solicitud.TimingPreferido ?? "AsSoonAsPossible",
            Notas = solicitud.Notas,
            SurfaceLabel = ExteriorPaintDisplayLabels.FormatSurface(solicitud.TipoSuperficie),
            IssuesLabel = ExteriorPaintDisplayLabels.FormatPipeList(solicitud.ProblemasSeleccionados, ExteriorPaintDisplayLabels.FormatIssue),
            LastPaintedLabel = ExteriorPaintDisplayLabels.FormatLastPainted(solicitud.UltimaPintura),
            ScopeLabel = ExteriorPaintDisplayLabels.FormatPipeList(solicitud.AreasSeleccionadas, ExteriorPaintDisplayLabels.FormatArea),
            ArchivosExistentes = MapExistingFiles(solicitud)
        };
    }

    private static List<ExistingExteriorPaintFileViewModel> MapExistingFiles(SolicitudExteriorPaint solicitud) =>
        solicitud.Archivos
            .OrderBy(a => a.CategoriaFoto)
            .Select(a => new ExistingExteriorPaintFileViewModel
            {
                Id = a.Id,
                CategoriaFoto = a.CategoriaFoto,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SavePhotoAsync(
        SolicitudExteriorPaint solicitud,
        string userId,
        IFormFile? file,
        string categoria)
    {
        if (file == null || file.Length == 0) return;

        var existing = solicitud.Archivos.FirstOrDefault(a =>
            string.Equals(a.CategoriaFoto, categoria, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            var oldPath = Path.Combine(_env.WebRootPath, existing.RutaArchivo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _db.ArchivosExteriorPaint.Remove(existing);
            solicitud.Archivos.Remove(existing);
        }

        var totalCount = await _db.ArchivosExteriorPaint.CountAsync(a => a.SolicitudExteriorPaintId == solicitud.Id);
        if (totalCount >= MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            ModelState.AddModelError("", $"File type not allowed: {file.FileName}. Use JPG or PNG.");
            return;
        }

        if (file.Length > MaxFileSize)
        {
            ModelState.AddModelError("", $"File too large: {file.FileName}. Max 10 MB.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "exterior-paint", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        var storedName = $"{categoria.ToLowerInvariant()}-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadDir, storedName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var archivo = new ArchivoExteriorPaint
        {
            SolicitudExteriorPaintId = solicitud.Id,
            UserId = userId,
            CategoriaFoto = categoria,
            NombreArchivo = file.FileName,
            RutaArchivo = $"/uploads/exterior-paint/{solicitud.Id}/{storedName}",
            TipoContenido = file.ContentType,
            TamanoBytes = file.Length,
            FechaSubida = DateTime.Now
        };

        _db.ArchivosExteriorPaint.Add(archivo);
        solicitud.Archivos.Add(archivo);
    }

    private async Task EnsureAddressAsync(SolicitudExteriorPaint solicitud)
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

    private static List<ExteriorPaintFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new ExteriorPaintFeatureItemViewModel
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

    private async Task<SolicitudExteriorPaint?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesExteriorPaint
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudExteriorPaint> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudExteriorPaint? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesExteriorPaint
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudExteriorPaint
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                UltimaPintura = "DontKnow",
                TipoSuperficie = "WoodSiding",
                MantenerMismoColor = "Yes",
                TimingPreferido = "AsSoonAsPossible"
            };
            _db.SolicitudesExteriorPaint.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudExteriorPaint?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        var query = _db.SolicitudesExteriorPaint
            .Include(s => s.HomeCarePriority)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudExteriorPaint solicitud)
    {
        if (!solicitud.PropiedadId.HasValue) return;

        const string title = "Exterior Paint Review";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Surface: {ExteriorPaintDisplayLabels.FormatSurface(solicitud.TipoSuperficie)} | " +
                    $"Issues: {ExteriorPaintDisplayLabels.FormatPipeList(solicitud.ProblemasSeleccionados, ExteriorPaintDisplayLabels.FormatIssue)} | " +
                    $"Scope: {ExteriorPaintDisplayLabels.FormatPipeList(solicitud.AreasSeleccionadas, ExteriorPaintDisplayLabels.FormatArea)} | " +
                    $"Timing: {ExteriorPaintDisplayLabels.FormatTiming(solicitud.TimingPreferido)}";

        if (existing != null)
        {
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
                Status = "Upcoming",
                Notes = notes,
                FechaCreacion = DateTime.UtcNow
            });
        }
    }
}
