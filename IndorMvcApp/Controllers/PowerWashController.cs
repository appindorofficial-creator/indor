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
public class PowerWashController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 5;

    public PowerWashController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> PowerWashService(int id)
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
    public async Task<IActionResult> PowerWashService(PowerWashServiceViewModel model, string? action)
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
            return RedirectToAction(nameof(PowerWashDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your power wash request. Please ensure the Power Wash flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Priority, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PowerWashDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var detailsComplete = HasCompletedDetails(solicitud);

        return View(new PowerWashDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = solicitud.HomeCarePriority?.Nombre ?? "Power Wash Exterior",
            AreasSeleccionadas = detailsComplete ? solicitud.AreasSeleccionadas ?? string.Empty : string.Empty,
            MaterialExterior = detailsComplete ? solicitud.MaterialExterior ?? string.Empty : string.Empty,
            NumeroPisos = detailsComplete ? solicitud.NumeroPisos ?? string.Empty : string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PowerWashDetails(PowerWashDetailsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PowerWashService), new { id = solicitud.HomeCarePriorityId });
        }

        if (string.IsNullOrWhiteSpace(model.AreasSeleccionadas))
        {
            ModelState.AddModelError(nameof(model.AreasSeleccionadas), "Select at least one area to wash.");
        }
        else
        {
            model.AreasSeleccionadas = SanitizeAreasSeleccionadas(model.AreasSeleccionadas);
            if (string.IsNullOrWhiteSpace(model.AreasSeleccionadas))
            {
                ModelState.AddModelError(nameof(model.AreasSeleccionadas), "Select at least one area to wash.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            solicitud.AreasSeleccionadas = model.AreasSeleccionadas;
            solicitud.MaterialExterior = model.MaterialExterior;
            solicitud.NumeroPisos = model.NumeroPisos;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(PowerWashCondition), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save property details. Please ensure the Power Wash flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PowerWashCondition(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(await BuildConditionViewModelAsync(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> PowerWashCondition(
        PowerWashConditionViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PowerWashDetails), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            var vm = await BuildConditionViewModelAsync(solicitud);
            vm.ProblemasSeleccionados = model.ProblemasSeleccionados;
            vm.AreasDelicadas = model.AreasDelicadas;
            vm.AccesoGrifo = model.AccesoGrifo;
            vm.TimingPreferido = model.TimingPreferido;
            vm.VentanaHorario = model.VentanaHorario;
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
                return View(await BuildConditionViewModelAsync(solicitud));
            }

            await EnsureAddressAsync(solicitud);

            solicitud.ProblemasSeleccionados = model.ProblemasSeleccionados;
            solicitud.AreasDelicadas = model.AreasDelicadas;
            solicitud.AccesoGrifo = model.AccesoGrifo;
            solicitud.TimingPreferido = model.TimingPreferido;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.FechaPreferida = PowerWashDisplayLabels.GetDefaultVisitDate();
            solicitud.RecordatorioAnual = true;
            solicitud.Notas = model.Notas?.Trim();
            solicitud.PrecioEstimado = PowerWashPricingService.GetEstimatedPrice();
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;

            await UpsertMaintenanceTaskAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PowerWashConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your power wash request. Please ensure the Power Wash flow tables exist in the database and try again.");
            return View(await BuildConditionViewModelAsync(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PowerWashConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PowerWashCondition), new { id = solicitud.Id });
        }

        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);

        return View(new PowerWashConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PropiedadId = solicitud.PropiedadId,
            NombreServicio = landing?.LandingTitulo ?? "Power Wash Exterior",
            AreaLabel = PowerWashDisplayLabels.FormatPipeList(solicitud.AreasSeleccionadas, PowerWashDisplayLabels.FormatArea),
            MaterialLabel = PowerWashDisplayLabels.FormatMaterial(solicitud.MaterialExterior),
            StoriesLabel = PowerWashDisplayLabels.FormatStories(solicitud.NumeroPisos),
            ConditionLabel = PowerWashDisplayLabels.FormatPipeList(solicitud.ProblemasSeleccionados, PowerWashDisplayLabels.FormatIssue),
            WaterAccessLabel = PowerWashDisplayLabels.FormatYesNo(solicitud.AccesoGrifo),
            PreferredTimeLabel = PowerWashDisplayLabels.FormatPreferredTime(solicitud.TimingPreferido, solicitud.VentanaHorario),
            TipConfirmacionTexto = landing?.TipConfirmacionTexto
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(HomeCarePriority Priority, PowerWashServicioLanding Landing)?> LoadLandingBundleAsync(int priorityId)
    {
        var priority = await _db.HomeCarePriorities.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == priorityId && p.Activo);
        if (priority == null) return null;

        var landing = await GetLandingAsync(priorityId);
        landing ??= new PowerWashServicioLanding
        {
            HomeCarePriorityId = priorityId,
            PageTitle = "Power Wash Exterior",
            LandingTitulo = priority.Nombre,
            LandingTagline = "Recommended every 1–2 years",
            InfoBoxTexto = "This service helps remove dirt, algae, mildew, pollen, and surface buildup from the exterior of your home.",
            ImagenUrl = priority.ImagenUrl ?? "/priority-power-wash-exterior.png",
            BestForItems = "Vinyl siding|Brick|Stucco|Driveway|Patio|Fence",
            BestForIconos = "fa-house|fa-table-cells|fa-braille|fa-road|fa-umbrella-beach|fa-grip-lines",
            PreviewTexto = "We'll use your answers to understand your surface type, condition, and access so we can recommend the right approach.",
            TipConfirmacionTexto = "Power washing is commonly recommended every 1–2 years, or sooner if you notice mildew, pollen, or staining.",
            InfoCondicionTexto = "We use this to choose the safest wash pressure for your home.",
            CtaTexto = "Start exterior check"
        };

        return (priority, landing);
    }

    private async Task<PowerWashServicioLanding?> GetLandingAsync(int priorityId) =>
        await _db.PowerWashServicioLanding.AsNoTracking()
            .FirstOrDefaultAsync(l => l.HomeCarePriorityId == priorityId && l.Activo);

    private static PowerWashServiceViewModel BuildServiceViewModel(
        HomeCarePriority priority,
        PowerWashServicioLanding landing,
        SolicitudPowerWash? existing,
        PowerWashServiceViewModel? posted = null) =>
        new()
        {
            HomeCarePriorityId = priority.Id,
            SolicitudId = existing?.Id ?? posted?.SolicitudId,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline ?? priority.Subtitulo,
            InfoBoxTexto = landing.InfoBoxTexto,
            ImagenUrl = ResolveImageUrl(landing.ImagenUrl ?? priority.ImagenUrl),
            BestForItems = SplitPipePairs(landing.BestForItems, landing.BestForIconos),
            PreviewTexto = landing.PreviewTexto,
            CtaTexto = landing.CtaTexto
        };

    private async Task<PowerWashConditionViewModel> BuildConditionViewModelAsync(SolicitudPowerWash solicitud)
    {
        var landing = await GetLandingAsync(solicitud.HomeCarePriorityId);

        return new PowerWashConditionViewModel
        {
            SolicitudId = solicitud.Id,
            HomeCarePriorityId = solicitud.HomeCarePriorityId,
            PageTitle = landing?.LandingTitulo ?? "Power Wash Exterior",
            InfoCondicionTexto = landing?.InfoCondicionTexto,
            ProblemasSeleccionados = solicitud.ProblemasSeleccionados ?? string.Empty,
            AreasDelicadas = solicitud.AreasDelicadas ?? string.Empty,
            AccesoGrifo = solicitud.AccesoGrifo ?? "Yes",
            TimingPreferido = solicitud.TimingPreferido ?? "NextWeek",
            VentanaHorario = solicitud.VentanaHorario ?? "Morning",
            Notas = solicitud.Notas,
            ArchivosExistentes = MapExistingFiles(solicitud)
        };
    }

    private static List<ExistingPowerWashFileViewModel> MapExistingFiles(SolicitudPowerWash solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingPowerWashFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private async Task SaveFilesAsync(SolicitudPowerWash solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosPowerWash
            .CountAsync(a => a.SolicitudPowerWashId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "power-wash", solicitud.Id.ToString());
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

            _db.ArchivosPowerWash.Add(new ArchivoPowerWash
            {
                SolicitudPowerWashId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/power-wash/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private async Task EnsureAddressAsync(SolicitudPowerWash solicitud)
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

    private static List<PowerWashFeatureItemViewModel> SplitPipePairs(string? texts, string? icons)
    {
        var textItems = string.IsNullOrWhiteSpace(texts)
            ? Array.Empty<string>()
            : texts.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var iconItems = string.IsNullOrWhiteSpace(icons)
            ? Array.Empty<string>()
            : icons.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return textItems.Select((text, index) => new PowerWashFeatureItemViewModel
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

    private async Task<SolicitudPowerWash?> GetActiveSolicitudAsync(string userId, int priorityId) =>
        await _db.SolicitudesPowerWash
            .Where(s => s.UserId == userId
                        && s.HomeCarePriorityId == priorityId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudPowerWash> GetOrCreateSolicitudAsync(
        string userId,
        int priorityId,
        int? solicitudId)
    {
        SolicitudPowerWash? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesPowerWash
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, priorityId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudPowerWash
            {
                UserId = userId,
                HomeCarePriorityId = priorityId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                AccesoGrifo = "Yes",
                TimingPreferido = "NextWeek",
                VentanaHorario = "Morning",
                RecordatorioAnual = true
            };
            _db.SolicitudesPowerWash.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudPowerWash?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        var query = _db.SolicitudesPowerWash
            .Include(s => s.HomeCarePriority)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task UpsertMaintenanceTaskAsync(SolicitudPowerWash solicitud)
    {
        if (!solicitud.PropiedadId.HasValue) return;

        const string title = "Power Wash Exterior";
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == solicitud.PropiedadId.Value
                        && m.Title == title
                        && m.Status != "Completed")
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        var notes = $"Area: {PowerWashDisplayLabels.FormatPipeList(solicitud.AreasSeleccionadas, PowerWashDisplayLabels.FormatArea)} | " +
                    $"Material: {PowerWashDisplayLabels.FormatMaterial(solicitud.MaterialExterior)} | " +
                    $"Condition: {PowerWashDisplayLabels.FormatPipeList(solicitud.ProblemasSeleccionados, PowerWashDisplayLabels.FormatIssue)} | " +
                    $"Timing: {PowerWashDisplayLabels.FormatPreferredTime(solicitud.TimingPreferido, solicitud.VentanaHorario)}";

        if (existing != null)
        {
            existing.DueDate = solicitud.FechaPreferida;
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
                DueDate = solicitud.FechaPreferida,
                Status = "Upcoming",
                Notes = notes,
                FechaCreacion = DateTime.UtcNow
            });
        }
    }

    private static bool HasCompletedDetails(SolicitudPowerWash solicitud) =>
        string.Equals(solicitud.Estado, "DetailsCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

    private static string SanitizeAreasSeleccionadas(string areas)
    {
        var selected = areas
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (selected.Any(a => string.Equals(a, "FullExterior", StringComparison.OrdinalIgnoreCase)))
        {
            selected.RemoveAll(a =>
                string.Equals(a, "FrontOnly", StringComparison.OrdinalIgnoreCase)
                || string.Equals(a, "BackOnly", StringComparison.OrdinalIgnoreCase));
        }

        return string.Join("|", selected);
    }
}
