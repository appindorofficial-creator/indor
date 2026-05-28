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
public class TvWallMountingController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf"];
    private const long MaxFileSize = 25_000_000;

    public TvWallMountingController(AppDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> TvWallMountingService(int id)
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
    public async Task<IActionResult> TvWallMountingService(TvWallMountingServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.MovingSetupServicioId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var propiedad = propiedadId.HasValue
                ? await _db.Propiedades.AsNoTracking().FirstOrDefaultAsync(p => p.Id == propiedadId)
                : null;

            var solicitud = await GetOrCreateSolicitudAsync(userId, model.MovingSetupServicioId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = propiedad?.Direccion;
            solicitud.Estado = "ServiceCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            solicitud.PrecioEstimado = bundle.Value.Landing.PrecioBaseEstimado;
            solicitud.VentanaHorario ??= "Afternoon";

            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(TvWallMountingPrepare), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(TvWallMountingProject), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your TV wall mounting request. Please ensure the TV wall mounting flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> TvWallMountingProject(int id)
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

        return View(new TvWallMountingProjectViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = defaultAddress ?? string.Empty,
            TipoSolicitud = solicitud.TipoSolicitud ?? "MountTv",
            TamanoTv = solicitud.TamanoTv ?? "Size43_55",
            CantidadTvs = solicitud.CantidadTvs ?? "One",
            Habitacion = solicitud.Habitacion ?? "LivingRoom",
            TipoPared = solicitud.TipoPared ?? "Drywall",
            TieneSoporte = solicitud.TieneSoporte ?? "YesHaveIt"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TvWallMountingProject(TvWallMountingProjectViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TvWallMountingService), new { id = solicitud.MovingSetupServicioId });
        }

        await EnsureAddressFromPropertyAsync(solicitud, model);

        if (!ModelState.IsValid)
        {
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }

        try
        {
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoSolicitud = model.TipoSolicitud;
            solicitud.TamanoTv = model.TamanoTv;
            solicitud.CantidadTvs = model.CantidadTvs;
            solicitud.Habitacion = model.Habitacion;
            solicitud.TipoPared = model.TipoPared;
            solicitud.TieneSoporte = model.TieneSoporte;
            solicitud.Estado = "ProjectCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(TvWallMountingPrepare), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save project details. Please ensure the TV wall mounting flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> TvWallMountingPrepare(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new TvWallMountingPrepareViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            HabitacionLabel = TvWallMountingDisplayLabels.FormatRoom(solicitud.Habitacion),
            ConfiguracionCables = solicitud.ConfiguracionCables ?? "BasicVisible",
            TomaCercana = solicitud.TomaCercana ?? "Yes",
            MontajePrevio = solicitud.MontajePrevio ?? "No",
            DetallesAcceso = solicitud.DetallesAcceso ?? "GroundFloor",
            VentanaHorario = solicitud.VentanaHorario ?? "Afternoon",
            FechaServicio = solicitud.FechaServicio ?? DateTime.Today.AddDays(30),
            NotaCorta = solicitud.NotaCorta,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingTvWallMountingFileViewModel
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
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> TvWallMountingPrepare(
        TvWallMountingPrepareViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TvWallMountingProject), new { id = solicitud.Id });
        }

        if (!ModelState.IsValid)
        {
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            model.HabitacionLabel = TvWallMountingDisplayLabels.FormatRoom(solicitud.Habitacion);
            return View(model);
        }

        try
        {
            solicitud.ConfiguracionCables = model.ConfiguracionCables;
            solicitud.TomaCercana = model.TomaCercana;
            solicitud.MontajePrevio = model.MontajePrevio;
            solicitud.DetallesAcceso = model.DetallesAcceso;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.FechaServicio = model.FechaServicio;
            solicitud.NotaCorta = model.NotaCorta?.Trim();
            solicitud.Estado = "PrepareCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            if (files != null && files.Count > 0)
            {
                await SaveFilesAsync(solicitud, RequireUserId()!, files);
                if (!ModelState.IsValid)
                {
                    model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
                    model.HabitacionLabel = TvWallMountingDisplayLabels.FormatRoom(solicitud.Habitacion);
                    model.ArchivosExistentes = solicitud.Archivos
                        .OrderByDescending(a => a.FechaSubida)
                        .Select(a => new ExistingTvWallMountingFileViewModel
                        {
                            Id = a.Id,
                            NombreArchivo = a.NombreArchivo,
                            RutaArchivo = a.RutaArchivo
                        })
                        .ToList();
                    return View(model);
                }
            }

            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(TvWallMountingReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save preparation details. Please ensure the TV wall mounting flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            model.HabitacionLabel = TvWallMountingDisplayLabels.FormatRoom(solicitud.Habitacion);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> TvWallMountingReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var landing = await _db.TvWallMountingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);

        await ApplyEstimateAsync(solicitud);
        await _db.SaveChangesAsync();

        return View(BuildReviewViewModel(solicitud, landing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TvWallMountingReview(TvWallMountingReviewViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TvWallMountingPrepare), new { id = solicitud.Id });
        }

        if (!model.AceptaDisclaimer)
        {
            ModelState.AddModelError(nameof(model.AceptaDisclaimer),
                "Please confirm you understand final pricing may adjust.");
        }

        if (!ModelState.IsValid)
        {
            var landing = await _db.TvWallMountingServicioLanding
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);
            return View(BuildReviewViewModel(solicitud, landing));
        }

        try
        {
            solicitud.Estado = "Confirmed";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(TvWallMountingConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not confirm your booking. Please ensure the TV wall mounting flow tables exist in the database and try again.");
            var landing = await _db.TvWallMountingServicioLanding
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);
            return View(BuildReviewViewModel(solicitud, landing));
        }
    }

    [HttpGet]
    public async Task<IActionResult> TvWallMountingConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(TvWallMountingReview), new { id = solicitud.Id });
        }

        return View(new TvWallMountingConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicioLabel = TvWallMountingDisplayLabels.FormatDate(solicitud.FechaServicio),
            VentanaHorarioLabel = TvWallMountingDisplayLabels.FormatTimeShort(solicitud.VentanaHorario),
            TamanoTvLabel = TvWallMountingDisplayLabels.FormatTvSize(solicitud.TamanoTv),
            HabitacionLabel = TvWallMountingDisplayLabels.FormatRoom(solicitud.Habitacion),
            TipoParedLabel = TvWallMountingDisplayLabels.FormatWallType(solicitud.TipoPared),
            EstadoLabel = "Confirmed"
        });
    }

    private async Task EnsureAddressFromPropertyAsync(SolicitudTvWallMounting solicitud, TvWallMountingProjectViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.DireccionPropiedad))
        {
            return;
        }

        if (solicitud.PropiedadId.HasValue)
        {
            var direccion = await _db.Propiedades.AsNoTracking()
                .Where(p => p.Id == solicitud.PropiedadId.Value)
                .Select(p => p.Direccion)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(direccion))
            {
                model.DireccionPropiedad = direccion;
                ModelState.Remove(nameof(model.DireccionPropiedad));
                return;
            }
        }

        var userId = RequireUserId();
        if (userId == null)
        {
            return;
        }

        var propiedad = await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(propiedad?.Direccion))
        {
            model.DireccionPropiedad = propiedad.Direccion;
            ModelState.Remove(nameof(model.DireccionPropiedad));
        }
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(MovingSetupServicio Servicio, TvWallMountingServicioLanding Landing)?> LoadLandingBundleAsync(int movingSetupServicioId)
    {
        var servicio = await _db.MovingSetupServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == movingSetupServicioId && s.Activo);

        if (servicio == null) return null;

        var landing = await _db.TvWallMountingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == movingSetupServicioId && l.Activo);

        if (landing == null) return null;

        return (servicio, landing);
    }

    private static TvWallMountingServiceViewModel BuildServiceViewModel(
        MovingSetupServicio servicio,
        TvWallMountingServicioLanding landing,
        SolicitudTvWallMounting? existing,
        TvWallMountingServiceViewModel? posted = null)
    {
        var texts = SplitPipe(landing.IncluyeItems);
        var icons = SplitPipe(landing.IncluyeIconos);
        var included = new List<TvWallMountingIncludedItemViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            included.Add(new TvWallMountingIncludedItemViewModel
            {
                Text = texts[i],
                Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-check"
            });
        }

        var bestForLabels = SplitPipe(landing.BestForOptions);
        var bestForValues = SplitPipe(landing.BestForValues);
        var bestForIcons = SplitPipe(landing.BestForIcons);
        var bestFor = new List<TvWallMountingBestForOptionViewModel>();
        for (var i = 0; i < bestForLabels.Length; i++)
        {
            bestFor.Add(new TvWallMountingBestForOptionViewModel
            {
                Label = bestForLabels[i],
                Value = i < bestForValues.Length ? bestForValues[i] : bestForLabels[i],
                Icon = i < bestForIcons.Length && !string.IsNullOrWhiteSpace(bestForIcons[i]) ? bestForIcons[i] : "fa-house"
            });
        }

        return new TvWallMountingServiceViewModel
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
            InfoBoxTexto = landing.InfoBoxTexto,
            EstimatedTimeLabel = landing.EstimatedTimeLabel,
            EstimatedTimeValue = landing.EstimatedTimeValue,
            BestTimingLabel = landing.BestTimingLabel,
            BestTimingValue = landing.BestTimingValue,
            CtaContinueTexto = landing.CtaContinueTexto,
            CtaUploadTexto = landing.CtaUploadTexto
        };
    }

    private static TvWallMountingReviewViewModel BuildReviewViewModel(
        SolicitudTvWallMounting solicitud,
        TvWallMountingServicioLanding? landing)
    {
        return new TvWallMountingReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            TipoSolicitudLabel = TvWallMountingDisplayLabels.FormatRequestType(solicitud.TipoSolicitud),
            TamanoTvLabel = TvWallMountingDisplayLabels.FormatTvSize(solicitud.TamanoTv),
            CantidadTvsLabel = TvWallMountingDisplayLabels.FormatTvCount(solicitud.CantidadTvs),
            HabitacionLabel = TvWallMountingDisplayLabels.FormatRoom(solicitud.Habitacion),
            TipoParedLabel = TvWallMountingDisplayLabels.FormatWallType(solicitud.TipoPared),
            TieneSoporteLabel = TvWallMountingDisplayLabels.FormatWallMount(solicitud.TieneSoporte),
            ConfiguracionCablesLabel = TvWallMountingDisplayLabels.FormatCableSetup(solicitud.ConfiguracionCables),
            TomaCercanaLabel = TvWallMountingDisplayLabels.FormatYesNoNotSure(solicitud.TomaCercana),
            MontajePrevioLabel = TvWallMountingDisplayLabels.FormatYesNoNotSure(solicitud.MontajePrevio),
            AccesoLabel = TvWallMountingDisplayLabels.FormatAccess(solicitud.DetallesAcceso),
            VentanaHorarioLabel = TvWallMountingDisplayLabels.FormatArrival(solicitud.VentanaHorario),
            FechaServicioLabel = TvWallMountingDisplayLabels.FormatDate(solicitud.FechaServicio),
            TiempoEstimadoLabel = landing?.EstimatedTimeValue ?? "60-90 min",
            PrecioEstimado = solicitud.PrecioEstimado ?? landing?.PrecioBaseEstimado ?? 129,
            NotaCorta = solicitud.NotaCorta,
            DisclaimerTexto = landing?.DisclaimerTexto,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingTvWallMountingFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo
                })
                .ToList()
        };
    }

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudTvWallMounting?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesTvWallMounting
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudTvWallMounting> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudTvWallMounting? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesTvWallMounting
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            solicitud = new SolicitudTvWallMounting
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesTvWallMounting.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudTvWallMounting?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudTvWallMounting> query = _db.SolicitudesTvWallMounting
            .Include(s => s.MovingSetupServicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task ApplyEstimateAsync(SolicitudTvWallMounting solicitud)
    {
        var landing = await _db.TvWallMountingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId);

        solicitud.PrecioEstimado = TvWallMountingDisplayLabels.CalculateEstimate(
            landing?.PrecioBaseEstimado ?? 129,
            solicitud.TamanoTv,
            solicitud.CantidadTvs,
            solicitud.TipoPared,
            solicitud.ConfiguracionCables,
            solicitud.TieneSoporte,
            solicitud.TipoSolicitud);
    }

    private async Task SaveFilesAsync(SolicitudTvWallMounting solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "tv-wall-mounting", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", $"File type not allowed: {file.FileName}. Use JPG, PNG, or PDF.");
                continue;
            }

            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError("", $"File too large: {file.FileName}. Max 25 MB.");
                continue;
            }

            var storedName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, storedName);
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            _db.ArchivosTvWallMounting.Add(new ArchivoTvWallMounting
            {
                SolicitudTvWallMountingId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/tv-wall-mounting/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                CategoriaArchivo = ext == ".pdf" ? "Document" : "Photo",
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
