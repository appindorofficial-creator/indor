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
public class RemodelingServicioController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 5;

    public RemodelingServicioController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> RemodelingService(int id)
    {
        var servicio = await LoadServicioAsync(id);
        if (servicio == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        SolicitudRemodelingServicio? existing = null;
        try
        {
            existing = await GetActiveSolicitudAsync(userId, id);
        }
        catch (Exception)
        {
            // Flow tables may not exist yet — still show the service landing page.
        }

        return View(BuildServiceViewModel(servicio, existing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemodelingService(RemodelingServiceViewModel model, string? action)
    {
        var servicio = await LoadServicioAsync(model.ServicioId);
        if (servicio == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home", null, "services");
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.ServicioId, model.SolicitudId);
            solicitud.PropiedadId = propiedadId;
            var propiedad = await GetLatestPropertyAsync(userId);
            if (propiedad != null)
            {
                solicitud.DireccionPropiedad ??= propiedad.Direccion;
            }

            var resumeCompleted = string.Equals(solicitud.Estado, "DetailsCompleted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);
            if (!resumeCompleted)
            {
                ClearProjectDetailFields(solicitud);
            }

            solicitud.Estado = "ServiceStarted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(RemodelingDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not start your project request. Run Scripts/CreateRemodelingServicioFlowTables.sql on the database and try again.");
            return View(BuildServiceViewModel(servicio, null));
        }
    }

    [HttpGet]
    public async Task<IActionResult> RemodelingDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var detailsEntered = string.Equals(solicitud.Estado, "DetailsCompleted", StringComparison.OrdinalIgnoreCase)
            || string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase);

        return View(new RemodelingDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioId = solicitud.ServicioId,
            PageTitle = solicitud.Servicio?.Nombre ?? "Project details",
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            AlcanceProyecto = detailsEntered ? (solicitud.AlcanceProyecto ?? string.Empty) : string.Empty,
            VentanaTiempo = detailsEntered ? (solicitud.VentanaTiempo ?? string.Empty) : string.Empty,
            PresupuestoEstimado = detailsEntered ? (solicitud.PresupuestoEstimado ?? string.Empty) : string.Empty,
            Descripcion = detailsEntered ? (solicitud.Descripcion ?? string.Empty) : string.Empty,
            ContactoPreferido = detailsEntered ? (solicitud.ContactoPreferido ?? string.Empty) : string.Empty,
            ArchivosExistentes = MapExistingFiles(solicitud)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> RemodelingDetails(
        RemodelingDetailsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RemodelingService), new { id = solicitud.ServicioId });
        }

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        await EnsureAddressFromPropertyAsync(userId, model);

        if (!ModelState.IsValid)
        {
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }

        try
        {
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.AlcanceProyecto = model.AlcanceProyecto;
            solicitud.VentanaTiempo = model.VentanaTiempo;
            solicitud.PresupuestoEstimado = model.PresupuestoEstimado;
            solicitud.Descripcion = model.Descripcion.Trim();
            solicitud.ContactoPreferido = model.ContactoPreferido;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            if (files != null && files.Count > 0)
            {
                await SaveFilesAsync(solicitud, userId, files);
                if (!ModelState.IsValid)
                {
                    model.ArchivosExistentes = MapExistingFiles(solicitud);
                    return View(model);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(RemodelingReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your project details. Please ensure the remodeling flow tables exist in the database and try again.");
            model.ArchivosExistentes = MapExistingFiles(solicitud);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RemodelingReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(BuildReviewViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemodelingReview(int id, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RemodelingDetails), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(RemodelingSent), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your request. Please ensure the remodeling flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> RemodelingSent(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RemodelingReview), new { id = solicitud.Id });
        }

        return View(new RemodelingSentViewModel
        {
            SolicitudId = solicitud.Id,
            NombreServicio = solicitud.Servicio?.Nombre ?? "Remodeling project",
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            VentanaTiempoLabel = RemodelingServicioDisplayLabels.FormatTiming(solicitud.VentanaTiempo),
            EstadoLabel = RemodelingServicioDisplayLabels.FormatPendingQuoteStatus()
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task<Servicio?> LoadServicioAsync(int id) =>
        await _db.Servicios.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

    private async Task<Propiedad?> GetLatestPropertyAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<int?> GetLatestPropertyIdAsync(string userId) =>
        await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    private async Task EnsureAddressFromPropertyAsync(string userId, RemodelingDetailsViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.DireccionPropiedad))
        {
            return;
        }

        var propiedad = await GetLatestPropertyAsync(userId);
        if (string.IsNullOrWhiteSpace(propiedad?.Direccion))
        {
            return;
        }

        model.DireccionPropiedad = propiedad.Direccion;
        ModelState.Remove(nameof(model.DireccionPropiedad));
    }

    private async Task<SolicitudRemodelingServicio?> GetActiveSolicitudAsync(string userId, int servicioId) =>
        await _db.SolicitudesRemodelingServicio
            .Where(s => s.UserId == userId
                        && s.ServicioId == servicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudRemodelingServicio> GetOrCreateSolicitudAsync(
        string userId,
        int servicioId,
        int? solicitudId)
    {
        SolicitudRemodelingServicio? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesRemodelingServicio
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, servicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudRemodelingServicio
            {
                UserId = userId,
                ServicioId = servicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now
            };
            _db.SolicitudesRemodelingServicio.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private static void ClearProjectDetailFields(SolicitudRemodelingServicio solicitud)
    {
        solicitud.AlcanceProyecto = null;
        solicitud.VentanaTiempo = null;
        solicitud.PresupuestoEstimado = null;
        solicitud.Descripcion = null;
        solicitud.ContactoPreferido = null;
    }

    private async Task<SolicitudRemodelingServicio?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudRemodelingServicio> query = _db.SolicitudesRemodelingServicio
            .Include(s => s.Servicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private static RemodelingServiceViewModel BuildServiceViewModel(
        Servicio servicio,
        SolicitudRemodelingServicio? solicitud)
    {
        var includes = string.IsNullOrWhiteSpace(servicio.Incluye)
            ? Array.Empty<string>()
            : servicio.Incluye.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new RemodelingServiceViewModel
        {
            SolicitudId = solicitud?.Id ?? 0,
            ServicioId = servicio.Id,
            PageTitle = servicio.Nombre,
            LandingTitulo = servicio.Nombre,
            LandingSubtitulo = servicio.Subtitulo ?? servicio.Descripcion,
            ImagenUrl = servicio.ImagenUrl,
            IncludedItems = includes,
            CtaTexto = string.IsNullOrWhiteSpace(servicio.CtaTexto) ? "Start my project" : servicio.CtaTexto
        };
    }

    private static List<ExistingRemodelingFileViewModel> MapExistingFiles(SolicitudRemodelingServicio solicitud) =>
        solicitud.Archivos
            .OrderByDescending(a => a.FechaSubida)
            .Select(a => new ExistingRemodelingFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo
            })
            .ToList();

    private static RemodelingReviewViewModel BuildReviewViewModel(SolicitudRemodelingServicio solicitud) =>
        new()
        {
            SolicitudId = solicitud.Id,
            ServicioId = solicitud.ServicioId,
            NombreServicio = solicitud.Servicio?.Nombre ?? "Remodeling project",
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            AlcanceLabel = RemodelingServicioDisplayLabels.FormatScope(solicitud.AlcanceProyecto),
            VentanaTiempoLabel = RemodelingServicioDisplayLabels.FormatTiming(solicitud.VentanaTiempo),
            PresupuestoLabel = RemodelingServicioDisplayLabels.FormatBudget(solicitud.PresupuestoEstimado),
            ContactoPreferidoLabel = RemodelingServicioDisplayLabels.FormatContact(solicitud.ContactoPreferido),
            Descripcion = solicitud.Descripcion,
            ArchivosExistentes = MapExistingFiles(solicitud)
        };

    private async Task SaveFilesAsync(SolicitudRemodelingServicio solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosRemodelingServicio
            .CountAsync(a => a.SolicitudRemodelingServicioId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "remodeling-servicio", solicitud.Id.ToString());
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

            _db.ArchivosRemodelingServicio.Add(new ArchivoRemodelingServicio
            {
                SolicitudRemodelingServicioId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/remodeling-servicio/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }
}
