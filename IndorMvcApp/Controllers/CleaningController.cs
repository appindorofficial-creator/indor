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
public class CleaningController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;

    public CleaningController(AppDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> CleaningService(int id)
    {
        var bundle = await LoadLandingBundleAsync(id);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var existing = await GetActiveSolicitudAsync(userId, id);
        return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, existing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleaningService(CleaningServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.MovingSetupServicioId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.IsNullOrWhiteSpace(model.BestForSelection))
        {
            ModelState.AddModelError(nameof(model.BestForSelection), "Please select a cleaning option.");
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var propiedad = propiedadId.HasValue
                ? await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId)
                : null;

            var solicitud = await GetOrCreateSolicitudAsync(userId, model.MovingSetupServicioId, model.SolicitudId);
            var mapped = CleaningDisplayLabels.MapBestForSelection(model.BestForSelection);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = propiedad?.Direccion;
            solicitud.TipoLimpieza = mapped.TipoLimpieza;
            solicitud.CondicionActual = mapped.Condicion;
            solicitud.Estado = "ServiceCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            solicitud.PrecioEstimado = bundle.Value.Landing.PrecioBaseEstimado;

            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(CleaningTasks), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(CleaningDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your cleaning request. Please ensure the cleaning flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningDetails(int id)
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

        // Only pre-select options once the user has actually filled in this step
        // (Estado becomes "DetailsCompleted" after submitting). On the first visit we
        // leave everything blank so nothing comes pre-selected, while still preserving
        // the user's choices when they navigate back to this step.
        var detailsEntered = string.Equals(solicitud.Estado, "DetailsCompleted", StringComparison.OrdinalIgnoreCase);

        return View(new CleaningDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = defaultAddress ?? string.Empty,
            TipoLimpieza = detailsEntered ? solicitud.TipoLimpieza : "",
            TipoPropiedad = detailsEntered ? (solicitud.TipoPropiedad ?? "") : "",
            NumeroHabitaciones = detailsEntered ? (solicitud.NumeroHabitaciones ?? "") : "",
            NumeroBanos = detailsEntered ? (solicitud.NumeroBanos ?? "") : "",
            CondicionActual = detailsEntered ? (solicitud.CondicionActual ?? "") : "",
            FechaServicio = detailsEntered ? solicitud.FechaServicio : null,
            VentanaHorario = detailsEntered ? solicitud.VentanaHorario : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleaningDetails(CleaningDetailsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningService), new { id = solicitud.MovingSetupServicioId });
        }

        var skipDate = string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase);

        // "Skip date for now" only omits the date/time window; the address and the
        // remaining required selections must still be valid.
        if (!ModelState.IsValid)
        {
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }

        try
        {
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoLimpieza = model.TipoLimpieza;
            solicitud.TipoPropiedad = model.TipoPropiedad;
            solicitud.NumeroHabitaciones = model.NumeroHabitaciones;
            solicitud.NumeroBanos = model.NumeroBanos;
            solicitud.CondicionActual = model.CondicionActual;
            solicitud.FechaServicio = skipDate ? null : model.FechaServicio;
            solicitud.VentanaHorario = skipDate ? null : model.VentanaHorario;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CleaningTasks), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your cleaning details. Please ensure the cleaning flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningTasks(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new CleaningTasksViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            AreasPrioridad = HasCompletedTasks(solicitud) ? solicitud.AreasPrioridad ?? string.Empty : string.Empty,
            TareasExtra = HasCompletedTasks(solicitud) ? solicitud.TareasExtra ?? string.Empty : string.Empty,
            SuministrosNecesarios = solicitud.SuministrosNecesarios ?? "Yes",
            NotaCorta = solicitud.NotaCorta,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingCleaningFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo
                })
                .ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> CleaningTasks(
        CleaningTasksViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningDetails), new { id = solicitud.Id });
        }

        solicitud.AreasPrioridad = model.AreasPrioridad?.Trim();
        solicitud.TareasExtra = model.TareasExtra?.Trim();
        solicitud.SuministrosNecesarios = model.SuministrosNecesarios;
        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.MetodoAcceso ??= "Lockbox";

        if (files != null && files.Count > 0)
        {
            await SaveFilesAsync(solicitud, RequireUserId()!, files);
            if (!ModelState.IsValid)
            {
                model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingCleaningFileViewModel
                    {
                        Id = a.Id,
                        NombreArchivo = a.NombreArchivo,
                        RutaArchivo = a.RutaArchivo
                    })
                    .ToList();
                return View(model);
            }
        }

        try
        {
            solicitud.Estado = "TasksCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CleaningReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your cleaning tasks. Please ensure the cleaning flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            model.DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var landing = await _db.CleaningServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);

        await ApplyEstimateAsync(solicitud);
        await _db.SaveChangesAsync();

        return View(BuildReviewViewModel(solicitud, landing?.DisclaimerTexto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleaningReview(CleaningReviewViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningDetails), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Confirmed";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(CleaningConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your request. Please ensure the cleaning flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud, model.DisclaimerTexto));
        }
    }

    [HttpGet]
    public async Task<IActionResult> CleaningConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(CleaningReview), new { id = solicitud.Id });
        }

        return View(new CleaningConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            TipoLimpiezaLabel = CleaningDisplayLabels.FormatCleaningType(solicitud.TipoLimpieza),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicioLabel = CleaningDisplayLabels.FormatDate(solicitud.FechaServicio),
            VentanaHorarioLabel = CleaningDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario),
            AreasResumen = CleaningDisplayLabels.FormatPipeList(solicitud.AreasPrioridad, CleaningDisplayLabels.FormatPriorityArea),
            TareasResumen = CleaningDisplayLabels.FormatPipeList(solicitud.TareasExtra, CleaningDisplayLabels.FormatExtraTask),
            EstadoLabel = "Confirmed"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(MovingSetupServicio Servicio, CleaningServicioLanding Landing)?> LoadLandingBundleAsync(int movingSetupServicioId)
    {
        var servicio = await _db.MovingSetupServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == movingSetupServicioId && s.Activo);

        if (servicio == null) return null;

        var landing = await _db.CleaningServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == movingSetupServicioId && l.Activo);

        if (landing == null) return null;

        return (servicio, landing);
    }

    private static CleaningServiceViewModel BuildServiceViewModel(
        MovingSetupServicio servicio,
        CleaningServicioLanding landing,
        SolicitudCleaning? existing,
        CleaningServiceViewModel? posted = null)
    {
        var texts = SplitPipe(landing.IncluyeItems);
        var icons = SplitPipe(landing.IncluyeIconos);
        var included = new List<CleaningIncludedItemViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            included.Add(new CleaningIncludedItemViewModel
            {
                Text = texts[i],
                Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-check"
            });
        }

        var labels = SplitPipe(landing.BestForOptions);
        var values = SplitPipe(landing.BestForValues);
        var bestIcons = SplitPipe(landing.BestForIcons);
        var bestFor = new List<CleaningBestForOptionViewModel>();
        for (var i = 0; i < labels.Length; i++)
        {
            bestFor.Add(new CleaningBestForOptionViewModel
            {
                Label = labels[i],
                Value = i < values.Length ? values[i] : labels[i].Replace(" ", ""),
                Icon = i < bestIcons.Length && !string.IsNullOrWhiteSpace(bestIcons[i]) ? bestIcons[i] : "fa-house"
            });
        }

        return new CleaningServiceViewModel
        {
            MovingSetupServicioId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingTagline = landing.LandingTagline,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = landing.ImagenUrl,
            PrecioDesde = landing.PrecioDesde,
            IncludedItems = included,
            BestForLabel = landing.BestForLabel,
            BestForOptions = bestFor,
            InfoBoxTitulo = landing.InfoBoxTitulo,
            InfoBoxTexto = landing.InfoBoxTexto,
            CtaContinueTexto = landing.CtaContinueTexto,
            CtaUploadTexto = landing.CtaUploadTexto,
            BestForSelection = posted?.BestForSelection ?? string.Empty
        };
    }

    private static CleaningReviewViewModel BuildReviewViewModel(SolicitudCleaning solicitud, string? disclaimer)
    {
        return new CleaningReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            TipoLimpiezaLabel = CleaningDisplayLabels.FormatCleaningType(solicitud.TipoLimpieza),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            TamanoHogarLabel = CleaningDisplayLabels.FormatHomeSize(solicitud.NumeroHabitaciones, solicitud.NumeroBanos),
            CondicionLabel = CleaningDisplayLabels.FormatCondition(solicitud.CondicionActual),
            FechaServicioLabel = CleaningDisplayLabels.FormatDate(solicitud.FechaServicio),
            VentanaHorarioLabel = CleaningDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario),
            AreasResumen = CleaningDisplayLabels.FormatPipeList(solicitud.AreasPrioridad, CleaningDisplayLabels.FormatPriorityArea),
            TareasResumen = CleaningDisplayLabels.FormatPipeList(solicitud.TareasExtra, CleaningDisplayLabels.FormatExtraTask),
            MetodoAccesoLabel = CleaningDisplayLabels.FormatAccessMethod(solicitud.MetodoAcceso),
            SuministrosLabel = CleaningDisplayLabels.FormatSupplies(solicitud.SuministrosNecesarios),
            NotaCorta = solicitud.NotaCorta,
            PrecioEstimado = solicitud.PrecioEstimado ?? 149,
            DisclaimerTexto = disclaimer
        };
    }

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudCleaning?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesCleaning
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudCleaning> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudCleaning? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesCleaning
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            solicitud = new SolicitudCleaning
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesCleaning.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private static bool HasCompletedTasks(SolicitudCleaning solicitud) =>
        string.Equals(solicitud.Estado, "TasksCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase);

    private async Task<SolicitudCleaning?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudCleaning> query = _db.SolicitudesCleaning
            .Include(s => s.MovingSetupServicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task ApplyEstimateAsync(SolicitudCleaning solicitud)
    {
        var landing = await _db.CleaningServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId);

        solicitud.PrecioEstimado = CleaningDisplayLabels.CalculateEstimate(
            landing?.PrecioBaseEstimado ?? 149,
            solicitud.NumeroHabitaciones,
            solicitud.NumeroBanos,
            solicitud.CondicionActual,
            solicitud.AreasPrioridad,
            solicitud.TareasExtra,
            solicitud.SuministrosNecesarios);
    }

    private async Task SaveFilesAsync(SolicitudCleaning solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "cleaning", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in files.Where(f => f.Length > 0))
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

            _db.ArchivosCleaning.Add(new ArchivoCleaning
            {
                SolicitudCleaningId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/cleaning/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static string[] SplitPipe(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
