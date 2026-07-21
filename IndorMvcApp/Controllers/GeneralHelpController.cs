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
public class GeneralHelpController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;
    private const int MaxFiles = 5;

    public GeneralHelpController(AppDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GeneralHelpRequest(int id)
    {
        var servicio = await LoadServicioAsync(id);
        if (servicio == null) return NotFound();

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var solicitud = await GetOrCreateSolicitudAsync(userId, id, null);

        if (propiedad != null)
        {
            solicitud.PropiedadId = propiedad.Id;
            solicitud.DireccionPropiedad ??= propiedad.Direccion;
            await _db.SaveChangesAsync();
        }

        var detailsEntered = string.Equals(solicitud.Estado, "RequestCompleted", StringComparison.OrdinalIgnoreCase);

        return View(new GeneralHelpRequestViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = id,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            TipoAyuda = detailsEntered ? (solicitud.TipoAyuda ?? "") : "",
            VentanaTiempo = detailsEntered ? (solicitud.VentanaTiempo ?? "") : "",
            Urgencia = detailsEntered ? (solicitud.Urgencia ?? "") : ""
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneralHelpRequest(GeneralHelpRequestViewModel model, string? action)
    {
        var servicio = await LoadServicioAsync(model.MovingSetupServicioId);
        if (servicio == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Index", "Home");
        }

        var userId = RequireUserId();
        if (userId == null) return Challenge();

        await EnsureAddressFromPropertyAsync(userId, model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId);
            if (solicitud == null) return NotFound();

            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoAyuda = model.TipoAyuda;
            solicitud.VentanaTiempo = model.VentanaTiempo;
            solicitud.Urgencia = model.Urgencia;
            solicitud.Estado = "RequestCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(GeneralHelpDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the general help flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GeneralHelpDetails(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new GeneralHelpDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? string.Empty,
            Descripcion = solicitud.Descripcion ?? string.Empty,
            ContactoPreferido = solicitud.ContactoPreferido ?? "Text",
            NotasAcceso = solicitud.NotasAcceso ?? "Apartment",
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingGeneralHelpFileViewModel
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
    [RequestSizeLimit(60_000_000)]
    public async Task<IActionResult> GeneralHelpDetails(
        GeneralHelpDetailsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var solicitud = await LoadSolicitudForUserAsync(model.SolicitudId, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GeneralHelpRequest), new { id = solicitud.MovingSetupServicioId });
        }

        if (!ModelState.IsValid)
        {
            model.ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingGeneralHelpFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo
                })
                .ToList();
            return View(model);
        }

        try
        {
            solicitud.Descripcion = model.Descripcion.Trim();
            solicitud.ContactoPreferido = model.ContactoPreferido;
            solicitud.NotasAcceso = model.NotasAcceso?.Trim();
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            if (files != null && files.Count > 0)
            {
                await SaveFilesAsync(solicitud, RequireUserId()!, files);
                if (!ModelState.IsValid)
                {
                    model.ArchivosExistentes = solicitud.Archivos
                        .OrderByDescending(a => a.FechaSubida)
                        .Select(a => new ExistingGeneralHelpFileViewModel
                        {
                            Id = a.Id,
                            NombreArchivo = a.NombreArchivo,
                            RutaArchivo = a.RutaArchivo
                        })
                        .ToList();
                    return View(model);
                }
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(GeneralHelpReview), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save details. Please ensure the general help flow tables exist in the database and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GeneralHelpReview(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(BuildReviewViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneralHelpReview(int id, string? action)
    {
        var solicitud = await LoadSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GeneralHelpDetails), new { id = solicitud.Id });
        }

        try
        {
            solicitud.Estado = "Submitted";
            solicitud.FechaConfirmacion = DateTime.Now;
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(GeneralHelpSent), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not submit your request. Please ensure the general help flow tables exist in the database and try again.");
            return View(BuildReviewViewModel(solicitud));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GeneralHelpSent(int id)
    {
        var solicitud = await LoadSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        if (!string.Equals(solicitud.Estado, "Submitted", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(GeneralHelpReview), new { id = solicitud.Id });
        }

        return View(new GeneralHelpSentViewModel
        {
            SolicitudId = solicitud.Id,
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            VentanaTiempoLabel = GeneralHelpDisplayLabels.FormatTiming(solicitud.VentanaTiempo),
            EstadoLabel = GeneralHelpDisplayLabels.FormatPendingConfirmationStatus()
        });
    }

    private string? RequireUserId() => _userManager.GetUserId(User);

    private async Task EnsureAddressFromPropertyAsync(string userId, GeneralHelpRequestViewModel model)
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

    private async Task<MovingSetupServicio?> LoadServicioAsync(int id) =>
        await _db.MovingSetupServicios.AsNoTracking()
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

    private async Task<SolicitudGeneralHelp?> GetActiveSolicitudAsync(string userId, int movingSetupServicioId) =>
        await _db.SolicitudesGeneralHelp
            .Where(s => s.UserId == userId
                        && s.MovingSetupServicioId == movingSetupServicioId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();

    private async Task<SolicitudGeneralHelp> GetOrCreateSolicitudAsync(
        string userId,
        int movingSetupServicioId,
        int? solicitudId)
    {
        SolicitudGeneralHelp? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesGeneralHelp
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, movingSetupServicioId);

        if (solicitud == null)
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            solicitud = new SolicitudGeneralHelp
            {
                UserId = userId,
                MovingSetupServicioId = movingSetupServicioId,
                PropiedadId = propiedadId,
                Estado = "InProgress",
                FechaCreacion = DateTime.Now,
                ContactoPreferido = "Text"
            };
            _db.SolicitudesGeneralHelp.Add(solicitud);
            await _db.SaveChangesAsync();
        }

        return solicitud;
    }

    private async Task<SolicitudGeneralHelp?> LoadSolicitudForUserAsync(int id, bool includeArchivos = false)
    {
        var userId = RequireUserId();
        if (userId == null) return null;

        IQueryable<SolicitudGeneralHelp> query = _db.SolicitudesGeneralHelp
            .Include(s => s.MovingSetupServicio);

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    private static GeneralHelpReviewViewModel BuildReviewViewModel(SolicitudGeneralHelp solicitud) =>
        new()
        {
            SolicitudId = solicitud.Id,
            MovingSetupServicioId = solicitud.MovingSetupServicioId,
            NombreServicio = solicitud.MovingSetupServicio?.Nombre ?? "General Help",
            TipoAyudaLabel = GeneralHelpDisplayLabels.FormatHelpType(solicitud.TipoAyuda),
            DireccionPropiedad = solicitud.DireccionPropiedad ?? "—",
            VentanaTiempoLabel = GeneralHelpDisplayLabels.FormatTiming(solicitud.VentanaTiempo),
            UrgenciaLabel = GeneralHelpDisplayLabels.FormatUrgency(solicitud.Urgencia),
            ContactoPreferidoLabel = GeneralHelpDisplayLabels.FormatContact(solicitud.ContactoPreferido),
            AccesoLabel = GeneralHelpDisplayLabels.FormatAccessList(solicitud.NotasAcceso),
            Descripcion = solicitud.Descripcion,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingGeneralHelpFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo
                })
                .ToList()
        };

    private async Task SaveFilesAsync(SolicitudGeneralHelp solicitud, string userId, List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var currentCount = await _db.ArchivosGeneralHelp
            .CountAsync(a => a.SolicitudGeneralHelpId == solicitud.Id);

        var incoming = files.Where(f => f.Length > 0).ToList();
        if (currentCount + incoming.Count > MaxFiles)
        {
            ModelState.AddModelError("", $"You can upload up to {MaxFiles} photos.");
            return;
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "general-help", solicitud.Id.ToString());
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

            _db.ArchivosGeneralHelp.Add(new ArchivoGeneralHelp
            {
                SolicitudGeneralHelpId = solicitud.Id,
                UserId = userId,
                NombreArchivo = file.FileName,
                RutaArchivo = $"/uploads/general-help/{solicitud.Id}/{storedName}",
                TipoContenido = file.ContentType,
                TamanoBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }
}
