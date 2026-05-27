using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingDetails(int id)
    {
        var servicio = await LoadActivePlumbingEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencyPlumbingSolicitudAsync(userId, id);

        var model = new EmergencyPlumbingDetailsViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoProblema = existing?.TipoProblema ?? "BurstPipe",
            AguaFluyendo = existing?.AguaFluyendo ?? "Yes",
            PuedeCerrarAgua = existing?.PuedeCerrarAgua ?? "Yes",
            Urgencia = existing?.Urgencia ?? "Emergency"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyPlumbingDetails(EmergencyPlumbingDetailsViewModel model, string? action)
    {
        var servicio = await LoadActivePlumbingEmergencyServiceAsync(model.ServicioEmergenciaId);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (!ModelState.IsValid)
        {
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateEmergencyPlumbingSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoProblema = model.TipoProblema;
            solicitud.AguaFluyendo = model.AguaFluyendo;
            solicitud.PuedeCerrarAgua = model.PuedeCerrarAgua;
            solicitud.Urgencia = model.Urgencia;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(EmergencyPlumbingContact), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(EmergencyPlumbingContact), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency plumbing tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingContact(int id)
    {
        var solicitud = await LoadEmergencyPlumbingSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        var telefono = solicitud.TelefonoContacto;
        if (string.IsNullOrWhiteSpace(telefono))
        {
            telefono = user?.Telefono ?? string.Empty;
        }

        return View(new EmergencyPlumbingContactViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            NotaCorta = solicitud.NotaCorta,
            TelefonoContacto = telefono,
            AccesoSiAusente = solicitud.AccesoSiAusente ?? "Yes",
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingEmergencyPlumbingFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] EmergencyPlumbingAllowedExtensions =
        [".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long EmergencyPlumbingMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> EmergencyPlumbingContact(
        EmergencyPlumbingContactViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesEmergenciaPlomeria
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsPlumbingEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyPlumbingDetails), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.TelefonoContacto = model.TelefonoContacto?.Trim();
        solicitud.AccesoSiAusente = model.AccesoSiAusente;

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveEmergencyPlumbingFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                model.NombreServicio = solicitud.ServicioEmergencia.Nombre;
                model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingEmergencyPlumbingFileViewModel
                    {
                        Id = a.Id,
                        NombreArchivo = a.NombreArchivo,
                        RutaArchivo = a.RutaArchivo,
                        CategoriaArchivo = a.CategoriaArchivo
                    })
                    .ToList();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.TelefonoContacto))
            {
                ModelState.AddModelError(nameof(model.TelefonoContacto), "Please enter a phone number.");
                model.NombreServicio = solicitud.ServicioEmergencia.Nombre;
                model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingEmergencyPlumbingFileViewModel
                    {
                        Id = a.Id,
                        NombreArchivo = a.NombreArchivo,
                        RutaArchivo = a.RutaArchivo,
                        CategoriaArchivo = a.CategoriaArchivo
                    })
                    .ToList();
                return View(model);
            }
        }
        else if (string.IsNullOrWhiteSpace(model.TelefonoContacto))
        {
            ModelState.AddModelError(nameof(model.TelefonoContacto), "Please enter a phone number.");
            model.NombreServicio = solicitud.ServicioEmergencia.Nombre;
            model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
            model.DireccionPropiedad = solicitud.DireccionPropiedad;
            model.ArchivosExistentes = solicitud.Archivos
                .Select(a => new ExistingEmergencyPlumbingFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList();
            return View(model);
        }

        solicitud.Estado = "ContactCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyPlumbingReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingReview(int id)
    {
        var solicitud = await LoadEmergencyPlumbingSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var servicio = solicitud.ServicioEmergencia!;
        var archivos = solicitud.Archivos.Count;

        return View(new EmergencyPlumbingReviewViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatPlumbingProblem(
                solicitud.TipoProblema,
                solicitud.AguaFluyendo),
            AguaFluyendoResumen = EmergencyDisplayLabels.AguaFluyendo(solicitud.AguaFluyendo),
            CierreAguaResumen = EmergencyDisplayLabels.PuedeCerrarAgua(solicitud.PuedeCerrarAgua),
            UrgenciaResumen = EmergencyDisplayLabels.UrgenciaEmergencia(solicitud.Urgencia),
            TelefonoContacto = solicitud.TelefonoContacto ?? string.Empty,
            ArchivosResumen = EmergencyDisplayLabels.ArchivosResumen(archivos),
            TiempoLlegadaMinutos = servicio.TiempoLlegadaMinutos
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyPlumbingReview(EmergencyPlumbingReviewViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyPlumbingSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyPlumbingContact), new { id = solicitud.Id });
        }

        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencyPlumbingHistorialAsync(solicitud, solicitud.ServicioEmergencia!, "Submitted");

        return RedirectToAction(nameof(EmergencyPlumbingSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyPlumbingSubmitted(int id)
    {
        var solicitud = await LoadEmergencyPlumbingSolicitudForUserAsync(id);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;

        return View(new EmergencyPlumbingSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatPlumbingProblem(
                solicitud.TipoProblema,
                solicitud.AguaFluyendo),
            TiempoLlegadaMinutos = servicio.TiempoLlegadaMinutos,
            TelefonoContacto = solicitud.TelefonoContacto ?? string.Empty,
            EstadoResumen = EmergencyDisplayLabels.EstadoSolicitud(solicitud.Estado)
        });
    }

    private async Task SaveEmergencyPlumbingFilesAsync(
        SolicitudEmergenciaPlomeria solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "emergencias-plomeria", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > EmergencyPlumbingMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!EmergencyPlumbingAllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("",
                    $"File {file.FileName} is not allowed. Use JPG, PNG, MOV, MP4, or WEBM.");
                continue;
            }

            var storedName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
            var physicalPath = Path.Combine(uploadFolder, storedName);
            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/emergencias-plomeria/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosEmergenciaPlomeria.Add(new ArchivoEmergenciaPlomeria
            {
                SolicitudEmergenciaPlomeriaId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetEmergencyPlumbingFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length
            });
        }

        await _db.SaveChangesAsync();
    }

    private static string GetEmergencyPlumbingFileCategory(string ext)
    {
        return ext is ".mp4" or ".mov" or ".webm" ? "Video" : "Photo";
    }

    private async Task<ServicioEmergencia?> LoadActivePlumbingEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsPlumbingEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaPlomeria?> GetActiveEmergencyPlumbingSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaPlomeria
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaPlomeria> GetOrCreateEmergencyPlumbingSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaPlomeria? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaPlomeria
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencyPlumbingSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaPlomeria
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaPlomeria.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaPlomeria?> LoadEmergencyPlumbingSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesEmergenciaPlomeria
            .Include(s => s.ServicioEmergencia)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsPlumbingEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencyPlumbingHistorialAsync(
        SolicitudEmergenciaPlomeria solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaPlomeria"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaPlomeria",
                ItemId = solicitud.Id,
                NombreItem = servicio.TituloEmergencia,
                Fecha = DateTime.Now
            };
            _db.HistorialServicios.Add(historial);
        }

        historial.Estado = estado;
        historial.Notas = solicitud.DireccionPropiedad;
        historial.Fecha = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
