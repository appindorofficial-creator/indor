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
public class GutterCleaningController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 5;

    public GutterCleaningController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GutterCleaningService(int id)
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
    public async Task<IActionResult> GutterCleaningService(GutterCleaningServiceViewModel model, string? action)
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
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.HomeCarePriorityId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.TipoAccionInicial = model.TipoAccionInicial;
            solicitud.ObjetivoHoy = MapInitialToTodayGoal(model.TipoAccionInicial);
            solicitud.FechaActualizacion = DateTime.Now;

            if (string.Equals(model.TipoAccionInicial, "AlreadyDone", StringComparison.OrdinalIgnoreCase))
            {
                solicitud.Estado = "Submitted";
                solicitud.FechaConfirmacion = DateTime.Now;
                solicitud.RecordatorioPrimaveraOtono = true;
                await UpsertMaintenanceTaskAsync(solicitud, completed: true);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(GutterCleaningConfirmed), new { id = solicitud.Id });
            }

            solicitud.Estado = "ServiceSelected";
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GutterCleaningSetup), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your gutter cleaning request. Please ensure the Gutter Cleaning flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GutterCleaningSetup(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (string.Equals(solicitud.TipoAccionInicial, "AlreadyDone", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GutterCleaningConfirmed), new { id = solicitud.Id });
        }

        var setupEntered = string.Equals(solicitud.Estado, "SetupCompleted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

        return View(new GutterCleaningSetupViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Gutter Cleaning",
            NumeroPisos = setupEntered ? (solicitud.NumeroPisos ?? string.Empty) : string.Empty,
            TipoCanaletas = setupEntered ? (solicitud.TipoCanaletas ?? string.Empty) : string.Empty,
            ProtectorCanaletas = setupEntered ? (solicitud.ProtectorCanaletas ?? string.Empty) : string.Empty,
            UltimaLimpieza = setupEntered ? (solicitud.UltimaLimpieza ?? string.Empty) : string.Empty,
            CantidadBajantes = setupEntered ? solicitud.CantidadBajantes : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GutterCleaningSetup(GutterCleaningSetupViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GutterCleaningService), new { id = solicitud.HomeCarePriorityId });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.NumeroPisos = model.NumeroPisos;
            solicitud.TipoCanaletas = model.TipoCanaletas;
            solicitud.ProtectorCanaletas = model.ProtectorCanaletas;
            solicitud.UltimaLimpieza = model.UltimaLimpieza;
            solicitud.CantidadBajantes = model.CantidadBajantes;
            solicitud.Estado = "SetupCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GutterCleaningPreferences), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your home details. Please ensure the Gutter Cleaning flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GutterCleaningPreferences(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(BuildPreferencesViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> GutterCleaningPreferences(
        GutterCleaningPreferencesViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GutterCleaningSetup), new { id = solicitud.Id });
        }

        if (string.Equals(model.PreferenciaRecordatorio, "CustomDate", StringComparison.OrdinalIgnoreCase)
            && (!model.FechaRecordatorioPersonalizada.HasValue
                || model.FechaRecordatorioPersonalizada.Value.Date < DateTime.Today))
        {
            ModelState.AddModelError(nameof(model.FechaRecordatorioPersonalizada),
                "Please select today or a future date for your custom reminder.");
        }

        if (!ModelState.IsValid)
        {
            var vm = BuildPreferencesViewModel(solicitud);
            vm.ProblemasSeleccionados = model.ProblemasSeleccionados;
            vm.AreaProblema = model.AreaProblema;
            vm.ObjetivoHoy = model.ObjetivoHoy;
            vm.PreferenciaRecordatorio = model.PreferenciaRecordatorio;
            vm.FechaRecordatorioPersonalizada = model.FechaRecordatorioPersonalizada;
            vm.Notas = model.Notas;
            return View(vm);
        }

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        try
        {
            await SaveFilesAsync(solicitud, userId, files);
            if (!ModelState.IsValid)
            {
                return View(BuildPreferencesViewModel(solicitud));
            }

            await EnsureAddressAsync(solicitud);

            solicitud.ProblemasSeleccionados = model.ProblemasSeleccionados;
            solicitud.AreaProblema = model.AreaProblema;
            solicitud.ObjetivoHoy = model.ObjetivoHoy;
            solicitud.PreferenciaRecordatorio = model.PreferenciaRecordatorio;
            solicitud.RecordatorioPrimaveraOtono = string.Equals(model.PreferenciaRecordatorio, "SpringFall", StringComparison.OrdinalIgnoreCase);
            solicitud.FechaRecordatorioPersonalizada = string.Equals(model.PreferenciaRecordatorio, "CustomDate", StringComparison.OrdinalIgnoreCase)
                ? model.FechaRecordatorioPersonalizada?.Date
                : null;
            solicitud.FechaVisitaPreferida = string.Equals(model.ObjetivoHoy, "ScheduleService", StringComparison.OrdinalIgnoreCase)
                ? GutterCleaningDisplayLabels.GetDefaultVisitDate()
                : null;
            solicitud.Notas = model.Notas?.Trim();
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(GutterCleaningConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your gutter cleaning request. Please ensure the Gutter Cleaning flow tables exist in the database and try again.");
            return View(BuildPreferencesViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GutterCleaningConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GutterCleaningService), new { id = solicitud.HomeCarePriorityId });
        }

        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);
        var alreadyDone = string.Equals(solicitud.TipoAccionInicial, "AlreadyDone", StringComparison.OrdinalIgnoreCase);
        var springFall = solicitud.RecordatorioPrimaveraOtono
                         || string.Equals(solicitud.PreferenciaRecordatorio, "SpringFall", StringComparison.OrdinalIgnoreCase);
        var showService = !alreadyDone && string.Equals(solicitud.ObjetivoHoy, "ScheduleService", StringComparison.OrdinalIgnoreCase);

        DateTime? nextReminder = springFall
            ? GutterCleaningDisplayLabels.GetNextSpringFallReminderDate(DateTime.Today)
            : solicitud.FechaRecordatorioPersonalizada;

        return View(new GutterCleaningConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? "Gutter Cleaning",
            AlreadyCompleted = alreadyDone,
            FrequencyLabel = GutterCleaningDisplayLabels.FormatFrequency(springFall),
            HomeLabel = GutterCleaningDisplayLabels.FormatStories(solicitud.NumeroPisos),
            GutterGuardsLabel = GutterCleaningDisplayLabels.FormatYesNoNotSure(solicitud.ProtectorCanaletas),
            LastCleanedLabel = GutterCleaningDisplayLabels.FormatLastCleaned(solicitud.UltimaLimpieza),
            NeedLabel = alreadyDone
                ? "Already done"
                : GutterCleaningDisplayLabels.FormatTodayGoal(solicitud.ObjetivoHoy ?? solicitud.TipoAccionInicial),
            ReminderLabel = GutterCleaningDisplayLabels.FormatReminderPreference(solicitud.PreferenciaRecordatorio, springFall),
            NextReminderLabel = nextReminder.HasValue ? $"Next reminder: {nextReminder.Value:MMMM d, yyyy}" : null,
            PreferredVisitLabel = solicitud.FechaVisitaPreferida.HasValue
                ? $"Preferred visit: {solicitud.FechaVisitaPreferida.Value:MMMM d, yyyy}"
                : null,
            ShowServiceRequested = showService,
            NextStepsItems = SplitTimingPairs(landing?.NextStepsItems, landing?.NextStepsIconos),
            RecommendedTimingItems = SplitTimingPairs(landing?.RecommendedTimingItems, landing?.RecommendedTimingIconos),
            InfoConfirmacionTexto = landing?.InfoConfirmacionTexto
        });
    }

    private static string MapInitialToTodayGoal(string? initial) => initial switch
    {
        "ScheduleService" => "ScheduleService",
        "AlreadyDone" => "AlreadyDone",
        _ => "ReminderOnly"
    };

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, GutterCleaningServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await GetLandingAsync(priorityId);
        landing ??= new GutterCleaningServicioLanding
        {
            HomeCarePriorityId = priorityId,
            PageTitle = "Gutter Cleaning",
            LandingTitulo = priority.Nombre,
            LandingTagline = "Recommended twice a year",
            InfoBoxTexto = "Gutters should be cleaned in the spring and fall to help prevent clogs, overflow, fascia damage, foundation issues, and water intrusion.",
            ImagenUrl = priority.ImagenUrl ?? "/priority-gutter-cleaning.png",
            WhyItMattersItems = "Prevents overflow|Helps protect roof edges|Keeps downspouts clear|Reduces water around foundation",
            WhyItMattersIconos = "fa-droplet|fa-house-chimney|fa-faucet|fa-house-flood-water",
            NextStepsItems = "We saved your reminder schedule|A pro can review your request|You can update this anytime in My Home",
            NextStepsIconos = "fa-bell|fa-user-check|fa-house",
            RecommendedTimingItems = "Spring cleaning: March – May|Fall cleaning: September – November",
            RecommendedTimingIconos = "fa-seedling|fa-leaf",
            InfoConfirmacionTexto = "Routine gutter cleaning helps prevent overflow, roof damage, and foundation issues.",
            CtaTexto = "Continue"
        };

        return (priority, landing);
    }

    private async Task<GutterCleaningServicioLanding?> GetLandingAsync(int priorityId) =>
        await _db.GutterCleaningServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

    private static GutterCleaningServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        GutterCleaningServicioLanding landing,
        SolicitudGutterCleaning? existing,
        GutterCleaningServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? priority.Subtitulo,
            InfoBoxTexto = landing.InfoBoxTexto,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            WhyItMattersItems = SplitPipePairs(landing.WhyItMattersItems, landing.WhyItMattersIconos),
            CtaTexto = landing.CtaTexto,
            TipoAccionInicial = existing?.TipoAccionInicial ?? posted?.TipoAccionInicial ?? string.Empty
        };

    private static GutterCleaningPreferencesViewModel BuildPreferencesViewModel(SolicitudGutterCleaning solicitud) =>
        new()
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Gutter Cleaning",
            ProblemasSeleccionados = solicitud.ProblemasSeleccionados ?? string.Empty,
            AreaProblema = solicitud.AreaProblema ?? string.Empty,
            ObjetivoHoy = solicitud.ObjetivoHoy ?? string.Empty,
            PreferenciaRecordatorio = solicitud.PreferenciaRecordatorio ?? string.Empty,
            FechaRecordatorioPersonalizada = solicitud.FechaRecordatorioPersonalizada,
            Notas = solicitud.Notas,
            ArchivosExistentes = MapExistingFiles(solicitud)
        };

    private static string MapInitialToTodayGoalStatic(string? initial) => initial switch
    {
        "ScheduleService" => "ScheduleService",
        _ => "ReminderOnly"
    };

    private static List<ExistingGutterCleaningFileViewModel> MapExistingFiles(SolicitudGutterCleaning solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingGutterCleaningFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SaveFilesAsync(SolicitudGutterCleaning solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosGutterCleaning
            .CountAsync(a => a.SolicitudGutterCleaningId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "gutter-cleaning", solicitud.Id.ToString());
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

            _db.ArchivosGutterCleaning.Add(new ArchivoGutterCleaning
            {
                SolicitudGutterCleaningId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/gutter-cleaning/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private async Task EnsureAddressAsync(SolicitudGutterCleaning solicitud)
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

    private static List<GutterCleaningFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new GutterCleaningFeatureItemViewModel
        {
            Text = text,
            Icon = index < iconItems.Length ? iconItems[index] : "fa-check"
        }).ToList();
    }

    private static List<GutterCleaningFeatureItemViewModel> SplitTimingPairs(string? texts, string? icons)
    {
        var items = SplitPipePairs(texts, icons);
        foreach (var item in items)
        {
            var parts = item.Text.Split(':', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                item.Text = parts[0].Trim();
                item.Subtext = parts[1].Trim();
            }
        }

        return items;
    }

    private static string? ResolveImageUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) ? null : url.StartsWith('/') ? url : $"/{url}";

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudGutterCleaning?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesGutterCleaning
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudGutterCleaning> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudGutterCleaning? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesGutterCleaning
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudGutterCleaning
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                TipoAccionInicial = string.Empty,
                PreferenciaRecordatorio = string.Empty,
                RecordatorioPrimaveraOtono = false
            };
            _db.SolicitudesGutterCleaning.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudGutterCleaning?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        var query = _db.SolicitudesGutterCleaning
            .Include(s => s.HomeCarePriority)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudGutterCleaning solicitud, bool completed = false)
    {
        if (!solicitud.PropiedadId.HasValue) return;

        const string title = "Gutter Cleaning";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = completed
            ? "Marked as already completed by homeowner."
            : $"Stories: {GutterCleaningDisplayLabels.FormatStories(solicitud.NumeroPisos)} | " +
              $"Gutters: {GutterCleaningDisplayLabels.FormatGutterType(solicitud.TipoCanaletas)} | " +
              $"Need: {GutterCleaningDisplayLabels.FormatTodayGoal(solicitud.ObjetivoHoy)} | " +
              $"Reminder: {GutterCleaningDisplayLabels.FormatReminderPreference(solicitud.PreferenciaRecordatorio, solicitud.RecordatorioPrimaveraOtono)}";

        var dueDate = solicitud.FechaVisitaPreferida
                      ?? solicitud.FechaRecordatorioPersonalizada
                      ?? GutterCleaningDisplayLabels.GetNextSpringFallReminderDate(DateTime.Today);

        if (existing != null)
        {
            existing.DueDate = dueDate;
            existing.Status = completed ? "Completed" : "Upcoming";
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
                Status = completed ? "Completed" : "Upcoming",
                Notes = notes,
                FechaCreacion = DateTime.UtcNow
            });
        }
    }
}
