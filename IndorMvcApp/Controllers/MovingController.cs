using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class MovingController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;
    private readonly IIndorLocalizer _localizer;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 5;

    public MovingController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env,
        IIndorLocalizer localizer)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> MovingService(int id)
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
    public async Task<IActionResult> MovingService(MovingServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.MovingSetupServicioId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        // Prefer the submit button's form value; ambient route "action" can mask it.
        var flowAction = Request.Form["action"].FirstOrDefault() ?? action;

        if (string.IsNullOrWhiteSpace(model.TipoMovimiento))
        {
            ModelState.AddModelError(nameof(model.TipoMovimiento), "Select a move type.");
            ModelState.AddModelError(string.Empty, "Select a move type.");
            ModelState.LocalizeModelState(_localizer);
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.MovingSetupServicioId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.TipoMovimiento = model.TipoMovimiento;
            solicitud.Estado = "ServiceCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            var landing = bundle.Value.Landing;
            solicitud.PrecioEstimadoMin = landing.PrecioEstimadoMin;
            solicitud.PrecioEstimadoMax = landing.PrecioEstimadoMax;
            solicitud.DuracionEstimadaMinHoras = landing.DuracionEstimadaMinHoras;
            solicitud.DuracionEstimadaMaxHoras = landing.DuracionEstimadaMaxHoras;

            await _db.SaveChangesAsync();

            if (string.Equals(flowAction, "estimate", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(MovingReview), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(MovingDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your moving request. Please ensure the moving flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> MovingDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var propiedad = solicitud.Propiedad;
        if (propiedad == null && solicitud.PropiedadId.HasValue)
        {
            propiedad = await _db.Propiedades.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == solicitud.PropiedadId);
        }

        var defaultAddress = propiedad?.Direccion ?? string.Empty;

        // Only pre-select options once the user has actually filled in this step (Bug 20 / Bug 12).
        var detailsEntered = string.Equals(solicitud.Estado, "DetailsCompleted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(solicitud.Estado, "ItemsCompleted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase);

        return View(new MovingDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            TipoMovimiento = string.IsNullOrWhiteSpace(solicitud.TipoMovimiento)
                ? string.Empty
                : MapLandingToDetailsMoveType(solicitud.TipoMovimiento),
            TipoPropiedad = detailsEntered ? (solicitud.TipoPropiedad ?? "") : "",
            TamanoHogar = detailsEntered ? (solicitud.TamanoHogar ?? "") : "",
            DireccionOrigen = solicitud.DireccionOrigen ?? defaultAddress,
            DireccionDestino = detailsEntered ? (solicitud.DireccionDestino ?? string.Empty) : string.Empty,
            FechaMovimiento = detailsEntered ? solicitud.FechaMovimiento : null,
            VentanaHorario = detailsEntered ? (solicitud.VentanaHorario ?? "") : "",
            TipoServicio = detailsEntered ? (solicitud.TipoServicio ?? "") : ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MovingDetails(MovingDetailsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(MovingService), new { id = solicitud.MovingSetupServicioId });
        }

        if (!ModelState.IsValid)
        {
            ModelState.LocalizeModelState(_localizer);
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }

        try
        {
            solicitud.TipoMovimiento = model.TipoMovimiento;
            solicitud.TipoPropiedad = model.TipoPropiedad;
            solicitud.TamanoHogar = model.TamanoHogar;
            solicitud.DireccionOrigen = model.DireccionOrigen.Trim();
            solicitud.DireccionDestino = model.DireccionDestino.Trim();
            solicitud.FechaMovimiento = model.FechaMovimiento;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.TipoServicio = model.TipoServicio;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await ApplyEstimate(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(MovingItems), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your move details. Please ensure the moving flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MovingItems(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new MovingItemsViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            ItemsMover = solicitud.ItemsMover ?? string.Empty,
            TamanoMovimiento = solicitud.TamanoMovimiento ?? "OneTwoBedroom",
            CondicionesAcceso = solicitud.CondicionesAcceso ?? string.Empty,
            RequiereMontaje = solicitud.RequiereMontaje ?? "No",
            NotaCorta = solicitud.NotaCorta,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingMovingFileViewModel
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
    public async Task<IActionResult> MovingItems(
        MovingItemsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(MovingDetails), new { id = solicitud.Id });
        }

        solicitud.ItemsMover = model.ItemsMover?.Trim();
        solicitud.TamanoMovimiento = model.TamanoMovimiento;
        solicitud.CondicionesAcceso = model.CondicionesAcceso?.Trim();
        solicitud.RequiereMontaje = model.RequiereMontaje;
        solicitud.NotaCorta = model.NotaCorta?.Trim();

        if (files != null && files.Count > 0)
        {
            await SaveFilesAsync(solicitud, RequireUserId()!, files);
            if (!ModelState.IsValid)
            {
                model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingMovingFileViewModel
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
            solicitud.Estado = "ItemsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimate(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(MovingReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save items and access details. Please ensure the moving flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoto(int photoId, int solicitudId)
    {
        var solicitud = await LoadSolicitudForUserAsync(solicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var archivo = solicitud.Archivos.FirstOrDefault(a => a.Id == photoId);
        if (archivo == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(archivo.RutaArchivo))
        {
            var relativePath = archivo.RutaArchivo.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_env.WebRootPath, relativePath);
            if (System.IO.File.Exists(physicalPath))
            {
                try
                {
                    System.IO.File.Delete(physicalPath);
                }
                catch
                {
                    // Best-effort delete; DB row is still removed.
                }
            }
        }

        _db.ArchivosMoving.Remove(archivo);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(MovingItems), new { id = solicitudId });
    }

    [HttpGet]
    public async Task<IActionResult> MovingReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var landing = await _db.MovingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);

        await ApplyEstimate(solicitud);
        await _db.SaveChangesAsync();

        return View(BuildReviewViewModel(solicitud, landing?.DisclaimerTexto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MovingReview(MovingReviewViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(MovingDetails), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Confirmed";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(MovingConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your request. Please ensure the moving flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud, model.DisclaimerTexto));
        }
    }

    [HttpGet]
    public async Task<IActionResult> MovingConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(MovingReview), new { id = solicitud.Id });
        }

        return View(new MovingConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            TipoMovimientoLabel = MovingDisplayLabels.FormatMoveType(solicitud.TipoMovimiento),
            FechaMovimientoLabel = MovingDisplayLabels.FormatDate(solicitud.FechaMovimiento),
            VentanaHorarioLabel = MovingDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario),
            DireccionOrigen = solicitud.DireccionOrigen ?? "—",
            DireccionDestino = solicitud.DireccionDestino ?? "—",
            EstadoLabel = "Confirmed"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(MovingSetupServicio Servicio, MovingServicioLanding Landing)?> LoadLandingBundleAsync(int movingSetupServicioId)
    {
        var servicio = await _db.MovingSetupServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == movingSetupServicioId && s.Activo);

        if (servicio == null)
        {
            return null;
        }

        var landing = await _db.MovingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == movingSetupServicioId && l.Activo);

        if (landing == null)
        {
            return null;
        }

        return (servicio, landing);
    }

    private static MovingServiceViewModel BuildServiceViewModel(
        MovingSetupServicio servicio,
        MovingServicioLanding landing,
        SolicitudMoving? existing,
        MovingServiceViewModel? posted = null)
    {
        var texts = SplitPipe(landing.IncluyeItems);
        var icons = SplitPipe(landing.IncluyeIconos);
        var included = new List<MovingIncludedItemViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            included.Add(new MovingIncludedItemViewModel
            {
                Text = texts[i],
                Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-check"
            });
        }

        var labels = SplitPipe(landing.MoveTypes);
        var values = SplitPipe(landing.MoveTypeValues);
        var moveIcons = SplitPipe(landing.MoveTypeIcons);
        var moveTypes = new List<MovingMoveTypeOptionViewModel>();
        for (var i = 0; i < labels.Length; i++)
        {
            moveTypes.Add(new MovingMoveTypeOptionViewModel
            {
                Label = labels[i],
                Value = i < values.Length ? values[i] : labels[i].Replace(" ", ""),
                Icon = i < moveIcons.Length && !string.IsNullOrWhiteSpace(moveIcons[i]) ? moveIcons[i] : "fa-truck"
            });
        }

        return new MovingServiceViewModel
        {
            MovingSetupServicioId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = landing.ImagenUrl,
            IncludedItems = included,
            EstimatedTimeLabel = landing.EstimatedTimeLabel,
            EstimatedTimeValue = landing.EstimatedTimeValue,
            EstimatedTimeNote = landing.EstimatedTimeNote,
            BestForLabel = landing.BestForLabel,
            BestForValue = landing.BestForValue,
            BestForNote = landing.BestForNote,
            MoveTypes = moveTypes,
            CtaContinueTexto = landing.CtaContinueTexto,
            CtaEstimateTexto = landing.CtaEstimateTexto,
            TipoMovimiento = posted?.TipoMovimiento
                ?? (existing != null
                    && !string.Equals(existing.Estado, "InProgress", StringComparison.OrdinalIgnoreCase)
                        ? existing.TipoMovimiento
                        : string.Empty)
                ?? string.Empty
        };
    }

    private static MovingReviewViewModel BuildReviewViewModel(SolicitudMoving solicitud, string? disclaimer)
    {
        return new MovingReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            TipoMovimientoLabel = MovingDisplayLabels.FormatMoveType(solicitud.TipoMovimiento),
            TipoPropiedadLabel = MovingDisplayLabels.FormatPropertyType(solicitud.TipoPropiedad),
            TamanoHogarLabel = MovingDisplayLabels.FormatHomeSize(solicitud.TamanoHogar),
            DireccionOrigen = solicitud.DireccionOrigen ?? "—",
            DireccionDestino = solicitud.DireccionDestino ?? "—",
            FechaMovimientoLabel = MovingDisplayLabels.FormatDate(solicitud.FechaMovimiento),
            VentanaHorarioLabel = MovingDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario),
            TipoServicioLabel = MovingDisplayLabels.FormatServiceType(solicitud.TipoServicio),
            ItemsResumen = MovingDisplayLabels.FormatPipeList(solicitud.ItemsMover, MovingDisplayLabels.FormatItem),
            TamanoMovimientoLabel = MovingDisplayLabels.FormatMoveSize(solicitud.TamanoMovimiento),
            AccesoResumen = MovingDisplayLabels.FormatPipeList(solicitud.CondicionesAcceso, MovingDisplayLabels.FormatAccessCondition),
            RequiereMontajeLabel = MovingDisplayLabels.FormatYesNo(solicitud.RequiereMontaje),
            NotaCorta = solicitud.NotaCorta,
            PrecioEstimadoMin = solicitud.PrecioEstimadoMin ?? 420,
            PrecioEstimadoMax = solicitud.PrecioEstimadoMax ?? 620,
            DuracionEstimadaMinHoras = solicitud.DuracionEstimadaMinHoras ?? 2,
            DuracionEstimadaMaxHoras = solicitud.DuracionEstimadaMaxHoras ?? 6,
            DisclaimerTexto = disclaimer
        };
    }

    private static string MapLandingToDetailsMoveType(string tipoMovimiento) =>
        tipoMovimiento switch
        {
            "LocalMove" => "FullMove",
            _ => tipoMovimiento
        };

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudMoving?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesMoving
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudMoving> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudMoving? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesMoving
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            solicitud = new SolicitudMoving
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesMoving.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudMoving?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudMoving> query = _db.SolicitudesMoving
            .Include(s => s.MovingSetupServicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task ApplyEstimate(SolicitudMoving solicitud)
    {
        var landing = await _db.MovingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId);

        var baseMin = landing?.PrecioEstimadoMin ?? 420;
        var baseMax = landing?.PrecioEstimadoMax ?? 620;
        var baseDurMin = landing?.DuracionEstimadaMinHoras ?? 2;
        var baseDurMax = landing?.DuracionEstimadaMaxHoras ?? 6;

        var estimate = MovingDisplayLabels.CalculateEstimate(
            solicitud.TamanoHogar,
            solicitud.TamanoMovimiento,
            solicitud.TipoServicio,
            baseMin,
            baseMax,
            baseDurMin,
            baseDurMax);

        solicitud.PrecioEstimadoMin = estimate.Min;
        solicitud.PrecioEstimadoMax = estimate.Max;
        solicitud.DuracionEstimadaMinHoras = estimate.DurMin;
        solicitud.DuracionEstimadaMaxHoras = estimate.DurMax;
    }

    private async Task SaveFilesAsync(SolicitudMoving solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0)
        {
            return;
        }

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (incoming.Count == 0)
        {
            return;
        }

        var existingCount = solicitud.Archivos.Count;
        if (existingCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "moving", solicitud.Id.ToString());
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

            var relativePath = $"/uploads/moving/{solicitud.Id}/{storedName}";
            _db.ArchivosMoving.Add(new ArchivoMoving
            {
                SolicitudMovingId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
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
