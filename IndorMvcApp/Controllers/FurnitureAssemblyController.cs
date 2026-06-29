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
public class FurnitureAssemblyController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf"];
    private const long MaxFileSize = 25_000_000;

    public FurnitureAssemblyController(AppDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureAssemblyService(int id)
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
    public async Task<IActionResult> FurnitureAssemblyService(FurnitureAssemblyServiceViewModel model, string? action)
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
                return RedirectToAction(nameof(FurnitureAssemblyReview), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(FurnitureAssemblyItems), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your furniture assembly request. Please ensure the furniture assembly flow tables exist in the database and try again.");
            return View(BuildServiceViewModel(bundle.Value.Servicio, bundle.Value.Landing, null, model));
        }
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureAssemblyItems(int id)
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

        var itemsComplete = HasCompletedItems(solicitud);

        return View(new FurnitureAssemblyItemsViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = defaultAddress ?? string.Empty,
            TiposMueble = itemsComplete ? solicitud.TiposMueble ?? string.Empty : string.Empty,
            CantidadItems = itemsComplete ? solicitud.CantidadItems ?? string.Empty : string.Empty,
            CondicionItems = itemsComplete ? solicitud.CondicionItems ?? string.Empty : string.Empty,
            AnclajePared = itemsComplete ? solicitud.AnclajePared ?? string.Empty : string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FurnitureAssemblyItems(FurnitureAssemblyItemsViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(FurnitureAssemblyService), new { id = solicitud.MovingSetupServicioId });
        }

        if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(model.TiposMueble))
            {
                ModelState.AddModelError(nameof(model.TiposMueble), "Select at least one item to assemble.");
            }

            if (!ModelState.IsValid)
            {
                model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
                return View(model);
            }

            solicitud.DireccionPropiedad = model.DireccionPropiedad?.Trim();
            solicitud.TiposMueble = model.TiposMueble?.Trim();
            solicitud.CantidadItems = model.CantidadItems;
            solicitud.CondicionItems = model.CondicionItems;
            solicitud.AnclajePared = model.AnclajePared;
            solicitud.Estado = "ItemsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(FurnitureAssemblyReview), new { id = solicitud.Id });
        }

        if (string.IsNullOrWhiteSpace(model.TiposMueble))
        {
            ModelState.AddModelError(nameof(model.TiposMueble), "Select at least one item to assemble.");
        }

        if (!ModelState.IsValid)
        {
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }

        try
        {
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TiposMueble = model.TiposMueble?.Trim();
            solicitud.CantidadItems = model.CantidadItems;
            solicitud.CondicionItems = model.CondicionItems;
            solicitud.AnclajePared = model.AnclajePared;
            solicitud.Estado = "ItemsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(FurnitureAssemblyPreferences), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save item details. Please ensure the furniture assembly flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureAssemblyPreferences(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new FurnitureAssemblyPreferencesViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            Habitacion = solicitud.Habitacion ?? "Bedroom",
            DetallesAcceso = solicitud.DetallesAcceso ?? "Stairs",
            AyudaMover = solicitud.AyudaMover ?? "No",
            FechaServicio = NormalizeServiceDate(solicitud.FechaServicio),
            VentanaHorario = solicitud.VentanaHorario ?? "Afternoon",
            NotaCorta = solicitud.NotaCorta,
            MinServiceDateIso = DateTime.Today.ToString("yyyy-MM-dd")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FurnitureAssemblyPreferences(FurnitureAssemblyPreferencesViewModel model, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(FurnitureAssemblyItems), new { id = solicitud.Id });
        }

        if (model.FechaServicio.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.FechaServicio), "Please select today or a future date.");
        }

        if (!ModelState.IsValid)
        {
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            model.MinServiceDateIso = DateTime.Today.ToString("yyyy-MM-dd");
            return View(model);
        }

        try
        {
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.Habitacion = model.Habitacion;
            solicitud.DetallesAcceso = model.DetallesAcceso;
            solicitud.AyudaMover = model.AyudaMover;
            solicitud.FechaServicio = model.FechaServicio.Date;
            solicitud.VentanaHorario = model.VentanaHorario;
            solicitud.NotaCorta = model.NotaCorta?.Trim();
            solicitud.Estado = "PreferencesCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await ApplyEstimateAsync(solicitud);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(FurnitureAssemblyReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save preferences. Please ensure the furniture assembly flow tables exist in the database and try again.");
            model.NombreServicio = solicitud.MovingSetupServicio!.Nombre;
            model.MinServiceDateIso = DateTime.Today.ToString("yyyy-MM-dd");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureAssemblyReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var landing = await _db.FurnitureAssemblyServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId && l.Activo);

        await ApplyEstimateAsync(solicitud);
        await _db.SaveChangesAsync();

        return View(BuildReviewViewModel(solicitud, landing?.DisclaimerTexto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> FurnitureAssemblyReview(
        FurnitureAssemblyReviewViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var isBack = string.Equals(action, "back", StringComparison.OrdinalIgnoreCase);
        var isEdit = string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase);

        solicitud.NotaCorta = model.NotaCorta?.Trim();

        if (files != null && files.Count > 0)
        {
            await SaveFilesAsync(solicitud, userId, files);
            await _db.Entry(solicitud).Collection(s => s.Archivos).LoadAsync();
        }

        if (!ModelState.IsValid)
        {
            return View(BuildReviewViewModel(solicitud, model.DisclaimerTexto));
        }

        if (isBack)
        {
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(FurnitureAssemblyPreferences), new { id = solicitud.Id });
        }

        if (isEdit)
        {
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(FurnitureAssemblyItems), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Confirmed";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(FurnitureAssemblyConfirmed), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your request. Please ensure the furniture assembly flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud, model.DisclaimerTexto));
        }
    }

    [HttpGet]
    public async Task<IActionResult> FurnitureAssemblyConfirmed(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(FurnitureAssemblyReview), new { id = solicitud.Id });
        }

        return View(new FurnitureAssemblyConfirmedViewModel
        {
            SolicitudId = solicitud.Id,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            FechaServicioLabel = FurnitureAssemblyDisplayLabels.FormatDate(solicitud.FechaServicio),
            VentanaHorarioLabel = FurnitureAssemblyDisplayLabels.FormatTimeShort(solicitud.VentanaHorario),
            ItemsResumen = FurnitureAssemblyDisplayLabels.FormatPipeList(solicitud.TiposMueble, FurnitureAssemblyDisplayLabels.FormatFurnitureType),
            HabitacionLabel = FurnitureAssemblyDisplayLabels.FormatRoom(solicitud.Habitacion),
            AccesoLabel = FurnitureAssemblyDisplayLabels.FormatPipeList(solicitud.DetallesAcceso, FurnitureAssemblyDisplayLabels.FormatAccess),
            EstadoLabel = "Confirmed"
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<(MovingSetupServicio Servicio, FurnitureAssemblyServicioLanding Landing)?> LoadLandingBundleAsync(int movingSetupServicioId)
    {
        var servicio = await _db.MovingSetupServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == movingSetupServicioId && s.Activo);

        if (servicio == null) return null;

        var landing = await _db.FurnitureAssemblyServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == movingSetupServicioId && l.Activo);

        if (landing == null) return null;

        return (servicio, landing);
    }

    private static FurnitureAssemblyServiceViewModel BuildServiceViewModel(
        MovingSetupServicio servicio,
        FurnitureAssemblyServicioLanding landing,
        SolicitudFurnitureAssembly? existing,
        FurnitureAssemblyServiceViewModel? posted = null)
    {
        var texts = SplitPipe(landing.IncluyeItems);
        var icons = SplitPipe(landing.IncluyeIconos);
        var included = new List<FurnitureAssemblyIncludedItemViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            included.Add(new FurnitureAssemblyIncludedItemViewModel
            {
                Text = texts[i],
                Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-check"
            });
        }

        var badgeTexts = SplitPipe(landing.BadgesTexto);
        var badgeIcons = SplitPipe(landing.BadgesIconos);
        var badges = new List<FurnitureAssemblyBadgeViewModel>();
        for (var i = 0; i < badgeTexts.Length; i++)
        {
            badges.Add(new FurnitureAssemblyBadgeViewModel
            {
                Text = badgeTexts[i],
                Icon = i < badgeIcons.Length && !string.IsNullOrWhiteSpace(badgeIcons[i]) ? badgeIcons[i] : "fa-check"
            });
        }

        return new FurnitureAssemblyServiceViewModel
        {
            MovingSetupServicioId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            PageTitle = landing.PageTitle,
            LandingTitulo = landing.LandingTitulo,
            LandingSubtitulo = landing.LandingSubtitulo,
            ImagenUrl = landing.ImagenUrl,
            PrecioDesde = landing.PrecioDesde,
            Badges = badges,
            IncludedItems = included,
            EstimatedTimeLabel = landing.EstimatedTimeLabel,
            EstimatedTimeValue = landing.EstimatedTimeValue,
            EstimatedTimeNote = landing.EstimatedTimeNote,
            BestForLabel = landing.BestForLabel,
            BestForValue = landing.BestForValue,
            BestForNote = landing.BestForNote,
            CtaContinueTexto = landing.CtaContinueTexto,
            CtaUploadTexto = landing.CtaUploadTexto
        };
    }

    private static FurnitureAssemblyReviewViewModel BuildReviewViewModel(SolicitudFurnitureAssembly solicitud, string? disclaimer)
    {
        var fechaLabel = FurnitureAssemblyDisplayLabels.FormatDate(solicitud.FechaServicio);
        var timeLabel = FurnitureAssemblyDisplayLabels.FormatTimeWindow(solicitud.VentanaHorario);
        var windowShort = solicitud.VentanaHorario switch
        {
            "Morning" => "Morning",
            "Afternoon" => "Afternoon",
            "Evening" => "Evening",
            _ => "Afternoon"
        };

        return new FurnitureAssemblyReviewViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            ItemsResumen = FurnitureAssemblyDisplayLabels.FormatPipeList(solicitud.TiposMueble, FurnitureAssemblyDisplayLabels.FormatFurnitureType),
            FechaHorarioLabel = $"{fechaLabel} - {windowShort}",
            HabitacionLabel = FurnitureAssemblyDisplayLabels.FormatRoom(solicitud.Habitacion),
            AccesoLabel = FurnitureAssemblyDisplayLabels.FormatPipeList(solicitud.DetallesAcceso, FurnitureAssemblyDisplayLabels.FormatAccess),
            NotaCorta = solicitud.NotaCorta,
            DisclaimerTexto = disclaimer,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingFurnitureAssemblyFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
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

    private async Task<SolicitudFurnitureAssembly?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesFurnitureAssembly
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudFurnitureAssembly> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudFurnitureAssembly? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesFurnitureAssembly
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            solicitud = new SolicitudFurnitureAssembly
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesFurnitureAssembly.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudFurnitureAssembly?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudFurnitureAssembly> query = _db.SolicitudesFurnitureAssembly
            .Include(s => s.MovingSetupServicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private async Task ApplyEstimateAsync(SolicitudFurnitureAssembly solicitud)
    {
        var landing = await _db.FurnitureAssemblyServicioLanding
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.MovingSetupServicioId == solicitud.MovingSetupServicioId);

        solicitud.PrecioEstimado = FurnitureAssemblyDisplayLabels.CalculateEstimate(
            landing?.PrecioBaseEstimado ?? 89,
            solicitud.CantidadItems,
            solicitud.TiposMueble,
            solicitud.CondicionItems,
            solicitud.AnclajePared,
            solicitud.AyudaMover);
    }

    private async Task SaveFilesAsync(SolicitudFurnitureAssembly solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "furniture-assembly", solicitud.Id.ToString());
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

            var categoria = ext == ".pdf" ? "Manual" : "Photo";
            _db.ArchivosFurnitureAssembly.Add(new ArchivoFurnitureAssembly
            {
                SolicitudFurnitureAssemblyId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/furniture-assembly/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                CategoriaArchivo = categoria,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static string[] SplitPipe(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static bool HasCompletedItems(SolicitudFurnitureAssembly solicitud) =>
        string.Equals(solicitud.Estado, "ItemsCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "PreferencesCompleted", StringComparison.OrdinalIgnoreCase)
        || string.Equals(solicitud.Estado, "Confirmed", StringComparison.OrdinalIgnoreCase);

    private static DateTime NormalizeServiceDate(DateTime? date)
    {
        var today = DateTime.Today;
        return date is { } value && value.Date >= today ? value.Date : today.AddDays(30);
    }
}
