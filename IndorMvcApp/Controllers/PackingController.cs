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
public class PackingController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf", ".txt"];
    private const long MaxFileSize = 10_000_000;

    public PackingController(AppDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> PackingService(int id)
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
    public async Task<IActionResult> PackingService(PackingServiceViewModel model, string? action)
    {
        var bundle = await LoadLandingBundleAsync(model.MovingSetupServicioId);
        if (bundle == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.IsNullOrWhiteSpace(model.BestForSelection))
        {
            model.BestForSelection = "MoveOut";
        }

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
            solicitud.VentanaHorario ??= "Morning";

            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(PackingDetails), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(PackingAbout), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your packing request. Please ensure the packing flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PackingAbout(int id)
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

        var aboutComplete = HasCompletedAbout(solicitud);

        return View(new PackingAboutViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = defaultAddress ?? string.Empty,
            TipoEmpaque = aboutComplete ? solicitud.TipoEmpaque ?? string.Empty : string.Empty,
            CuandoMudanza = aboutComplete ? solicitud.CuandoMudanza ?? string.Empty : string.Empty,
            TipoPropiedad = aboutComplete ? solicitud.TipoPropiedad ?? string.Empty : string.Empty,
            TamanoHogar = aboutComplete ? solicitud.TamanoHogar ?? string.Empty : string.Empty,
            FechaServicio = aboutComplete ? solicitud.FechaServicio : null
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PackingAbout(PackingAboutViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PackingService), new { id = solicitud.MovingSetupServicioId });
        }

        var skip = string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase);
        if (!skip && !ModelState.IsValid)
        {
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }

        if (skip)
        {
            ModelState.Clear();
        }

        try
        {
            solicitud.DireccionPropiedad = model.DireccionPropiedad?.Trim();
            if (!skip)
            {
                solicitud.TipoEmpaque = model.TipoEmpaque;
                solicitud.CuandoMudanza = model.CuandoMudanza;
                solicitud.TipoPropiedad = model.TipoPropiedad;
                solicitud.TamanoHogar = model.TamanoHogar;
                solicitud.FechaServicio = PackingDisplayLabels.ResolveServiceDate(model.CuandoMudanza, model.FechaServicio);
            }

            solicitud.Estado = "AboutCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PackingDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your packing job details. Please ensure the packing flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PackingDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new PackingDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            HabitacionesEmpacar = solicitud.HabitacionesEmpacar ?? string.Empty,
            ItemsEspeciales = solicitud.ItemsEspeciales ?? string.Empty,
            SuministrosNecesarios = solicitud.SuministrosNecesarios ?? string.Empty,
            DetallesAcceso = solicitud.DetallesAcceso ?? string.Empty,
            NotaCorta = solicitud.NotaCorta,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingPackingFileViewModel
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
    public async Task<IActionResult> PackingDetails(
        PackingDetailsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PackingAbout), new { id = solicitud.Id });
        }

        solicitud.HabitacionesEmpacar = model.HabitacionesEmpacar?.Trim();
        solicitud.ItemsEspeciales = model.ItemsEspeciales?.Trim();
        solicitud.SuministrosNecesarios = model.SuministrosNecesarios?.Trim();
        solicitud.DetallesAcceso = model.DetallesAcceso?.Trim();
        solicitud.NotaCorta = model.NotaCorta?.Trim();

        if (files != null && files.Count > 0)
        {
            await SaveFilesAsync(solicitud, RequireUserId()!, files);
            if (!ModelState.IsValid)
            {
                model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingPackingFileViewModel
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
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PackingReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save packing details. Please ensure the packing flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            model.DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PackingReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var landing = await _db.PackingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);

        await ApplyEstimateAsync(solicitud);
        await _db.SaveChangesAsync();

        return View(BuildReviewViewModel(solicitud, landing?.DisclaimerTexto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PackingReview(PackingReviewViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PackingAbout), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Confirmed";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(PackingConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your request. Please ensure the packing flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud, model.DisclaimerTexto));
        }
    }

    [HttpGet]
    public async Task<IActionResult> PackingConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(PackingReview), new { id = solicitud.Id });
        }

        var fecha = PackingDisplayLabels.ResolveServiceDate(solicitud.CuandoMudanza, solicitud.FechaServicio);

        return View(new PackingConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicioLabel = PackingDisplayLabels.FormatDate(fecha),
            VentanaHorarioLabel = PackingDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario),
            AlcanceLabel = PackingDisplayLabels.FormatPipeList(solicitud.HabitacionesEmpacar, PackingDisplayLabels.FormatRoom),
            SuministrosLabel = PackingDisplayLabels.FormatPipeList(solicitud.SuministrosNecesarios, PackingDisplayLabels.FormatSupply),
            EstadoLabel = "Confirmed"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(MovingSetupServicio Servicio, PackingServicioLanding Landing)?> LoadLandingBundleAsync(int movingSetupServicioId)
    {
        var servicio = await _db.MovingSetupServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == movingSetupServicioId && s.Activo);

        if (servicio == null) return null;

        var landing = await _db.PackingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == movingSetupServicioId && l.Activo);

        if (landing == null) return null;

        return (servicio, landing);
    }

    private static PackingServiceViewModel BuildServiceViewModel(
        MovingSetupServicio servicio,
        PackingServicioLanding landing,
        SolicitudPacking? existing,
        PackingServiceViewModel? posted = null)
    {
        var texts = SplitPipe(landing.IncluyeItems);
        var icons = SplitPipe(landing.IncluyeIconos);
        var included = new List<PackingIncludedItemViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            included.Add(new PackingIncludedItemViewModel
            {
                Text = texts[i],
                Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-check"
            });
        }

        var labels = SplitPipe(landing.BestForOptions);
        var values = SplitPipe(landing.BestForValues);
        var bestIcons = SplitPipe(landing.BestForIcons);
        var bestFor = new List<PackingBestForOptionViewModel>();
        for (var i = 0; i < labels.Length; i++)
        {
            bestFor.Add(new PackingBestForOptionViewModel
            {
                Label = labels[i],
                Value = i < values.Length ? values[i] : labels[i].Replace(" ", ""),
                Icon = i < bestIcons.Length && !string.IsNullOrWhiteSpace(bestIcons[i]) ? bestIcons[i] : "fa-house"
            });
        }

        return new PackingServiceViewModel
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
            CtaUploadTexto = landing.CtaUploadTexto,
            BestForSelection = posted?.BestForSelection ?? bestFor.FirstOrDefault()?.Value ?? "MoveOut"
        };
    }

    private static PackingReviewViewModel BuildReviewViewModel(SolicitudPacking solicitud, string? disclaimer)
    {
        var fecha = PackingDisplayLabels.ResolveServiceDate(solicitud.CuandoMudanza, solicitud.FechaServicio);

        return new PackingReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            TipoEmpaqueLabel = PackingDisplayLabels.FormatPackingType(solicitud.TipoEmpaque),
            FechaServicioLabel = PackingDisplayLabels.FormatDate(fecha),
            TipoPropiedadLabel = PackingDisplayLabels.FormatPropertyType(solicitud.TipoPropiedad),
            AlcanceLabel = PackingDisplayLabels.FormatPipeList(solicitud.HabitacionesEmpacar, PackingDisplayLabels.FormatRoom),
            ItemsEspecialesLabel = PackingDisplayLabels.FormatPipeList(solicitud.ItemsEspeciales, PackingDisplayLabels.FormatSpecialItem),
            SuministrosLabel = PackingDisplayLabels.FormatPipeList(solicitud.SuministrosNecesarios, PackingDisplayLabels.FormatSupply),
            AccesoLabel = PackingDisplayLabels.FormatPipeList(solicitud.DetallesAcceso, PackingDisplayLabels.FormatAccess),
            NotaCorta = solicitud.NotaCorta,
            DisclaimerTexto = disclaimer
        };
    }

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task<SolicitudPacking?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesPacking
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudPacking> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudPacking? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesPacking
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            solicitud = new SolicitudPacking
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesPacking.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private static bool HasCompletedAbout(SolicitudPacking solicitud) =>
        string.Equals(solicitud.Estado, "AboutCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "DetailsCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase);

    private async Task<SolicitudPacking?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudPacking> query = _db.SolicitudesPacking
            .Include(s => s.MovingSetupServicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task ApplyEstimateAsync(SolicitudPacking solicitud)
    {
        var landing = await _db.PackingServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId);

        solicitud.PrecioEstimado = PackingDisplayLabels.CalculateEstimate(
            landing?.PrecioBaseEstimado ?? 89,
            solicitud.TipoEmpaque,
            solicitud.TamanoHogar,
            solicitud.HabitacionesEmpacar,
            solicitud.ItemsEspeciales,
            solicitud.SuministrosNecesarios);
    }

    private async Task SaveFilesAsync(SolicitudPacking solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "packing", solicitud.Id.ToString());
        Directory.CreateDirectory(uploadDir);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("", $"File type not allowed: {file.FileName}. Use JPG, PNG, PDF, or TXT.");
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

            _db.ArchivosPacking.Add(new ArchivoPacking
            {
                SolicitudPackingId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/packing/{solicitud.Id}/{storedName}",
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
