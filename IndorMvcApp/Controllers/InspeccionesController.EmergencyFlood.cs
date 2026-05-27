using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencyFloodDetails(int id)
    {
        var servicio = await LoadActiveFloodEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencyFloodSolicitudAsync(userId, id);

        return View(new EmergencyFloodDetailsViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            CausaAgua = existing?.CausaAgua ?? "UnknownSource",
            UbicacionAgua = existing?.UbicacionAgua ?? "FirstFloor",
            AguaActiva = existing?.AguaActiva ?? "Yes"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyFloodDetails(EmergencyFloodDetailsViewModel model, string? action)
    {
        var servicio = await LoadActiveFloodEmergencyServiceAsync(model.ServicioEmergenciaId);
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
            var solicitud = await GetOrCreateEmergencyFloodSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.CausaAgua = model.CausaAgua;
            solicitud.UbicacionAgua = model.UbicacionAgua;
            solicitud.AguaActiva = model.AguaActiva;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(EmergencyFloodUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(EmergencyFloodSafety), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency flood tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyFloodSafety(int id)
    {
        var solicitud = await LoadEmergencyFloodSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new EmergencyFloodSafetyViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            PuedeCerrarAgua = solicitud.PuedeCerrarAgua ?? "NotSure",
            UbicacionCierreAgua = solicitud.UbicacionCierreAgua ?? "DontKnow",
            PuedeApagarElectricidad = solicitud.PuedeApagarElectricidad ?? "NotSure",
            CantidadAgua = solicitud.CantidadAgua ?? "OneRoom"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyFloodSafety(EmergencyFloodSafetyViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyFloodSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyFloodDetails), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.PuedeCerrarAgua = model.PuedeCerrarAgua;
        solicitud.UbicacionCierreAgua = model.UbicacionCierreAgua;
        solicitud.PuedeApagarElectricidad = model.PuedeApagarElectricidad;
        solicitud.CantidadAgua = model.CantidadAgua;
        solicitud.Estado = "SafetyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyFloodUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyFloodUpload(int id)
    {
        var solicitud = await LoadEmergencyFloodSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new EmergencyFloodUploadViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            NotaCorta = solicitud.NotaCorta,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingEmergencyFloodFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] EmergencyFloodAllowedExtensions =
        [".jpg", ".jpeg", ".png", ".pdf", ".mp4", ".mov", ".webm"];

    private const long EmergencyFloodMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> EmergencyFloodUpload(
        EmergencyFloodUploadViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesEmergenciaFlood
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsFloodEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyFloodSafety), new { id = solicitud.Id });
        }

        solicitud.NotaCorta = model.NotaCorta?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveEmergencyFloodFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                PopulateEmergencyFloodUploadModel(model, solicitud);
                return View(model);
            }
        }

        solicitud.Estado = "PhotosCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyFloodReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyFloodReview(int id)
    {
        var solicitud = await LoadEmergencyFloodSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new EmergencyFloodReviewViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            TituloServicio = solicitud.ServicioEmergencia!.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            CausaAguaResumen = EmergencyDisplayLabels.CausaAguaFlood(solicitud.CausaAgua),
            UbicacionAguaResumen = EmergencyDisplayLabels.UbicacionAguaFlood(solicitud.UbicacionAgua),
            AguaActivaResumen = EmergencyDisplayLabels.AguaActivaFlood(solicitud.AguaActiva),
            PuedeCerrarAguaResumen = EmergencyDisplayLabels.PuedeCerrarAgua(solicitud.PuedeCerrarAgua),
            UbicacionCierreAguaResumen = EmergencyDisplayLabels.UbicacionCierreAguaFlood(solicitud.UbicacionCierreAgua),
            PuedeApagarElectricidadResumen = EmergencyDisplayLabels.PuedeApagarElectricidadFlood(solicitud.PuedeApagarElectricidad),
            CantidadAguaResumen = EmergencyDisplayLabels.CantidadAguaFlood(solicitud.CantidadAgua),
            ArchivosResumen = EmergencyDisplayLabels.ArchivosSubidos(solicitud.Archivos.Count),
            NotaCorta = string.IsNullOrWhiteSpace(solicitud.NotaCorta) ? "None provided" : solicitud.NotaCorta
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyFloodReview(EmergencyFloodReviewViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyFloodSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase)
            || string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyFloodDetails), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencyFloodHistorialAsync(solicitud, solicitud.ServicioEmergencia!, "Submitted");

        return RedirectToAction(nameof(EmergencyFloodSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyFloodSubmitted(int id)
    {
        var solicitud = await LoadEmergencyFloodSolicitudForUserAsync(id);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;
        var minutos = servicio.TiempoLlegadaMinutos > 0 ? servicio.TiempoLlegadaMinutos : 45;

        return View(new EmergencyFloodSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = $"{servicio.TituloEmergencia} 24/7",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            AreaResumen = EmergencyDisplayLabels.FormatFloodArea(solicitud.UbicacionAgua, solicitud.CantidadAgua),
            CausaAguaResumen = EmergencyDisplayLabels.CausaAguaFlood(solicitud.CausaAgua),
            PuedeCerrarAguaResumen = EmergencyDisplayLabels.PuedeCerrarAgua(solicitud.PuedeCerrarAgua),
            EstadoResumen = EmergencyDisplayLabels.EstadoSolicitud(solicitud.Estado),
            TiempoLlegadaMinutos = minutos,
            TiempoLlegadaRango = EmergencyDisplayLabels.TiempoLlegadaRango(minutos)
        });
    }

    private static void PopulateEmergencyFloodUploadModel(
        EmergencyFloodUploadViewModel model,
        SolicitudEmergenciaFlood solicitud)
    {
        model.NombreServicio = solicitud.ServicioEmergencia!.Nombre;
        model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
        model.DireccionPropiedad = solicitud.DireccionPropiedad;
        model.ArchivosExistentes = solicitud.Archivos
            .Select(a => new ExistingEmergencyFloodFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo,
                CategoriaArchivo = a.CategoriaArchivo
            })
            .ToList();
    }

    private async Task SaveEmergencyFloodFilesAsync(
        SolicitudEmergenciaFlood solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "emergencias-flood", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > EmergencyFloodMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!EmergencyFloodAllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("",
                    $"File {file.FileName} is not allowed. Use JPG, PNG, PDF, MOV, MP4, or WEBM.");
                continue;
            }

            var storedName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
            var physicalPath = Path.Combine(uploadFolder, storedName);
            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/emergencias-flood/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosEmergenciaFlood.Add(new ArchivoEmergenciaFlood
            {
                SolicitudEmergenciaFloodId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetEmergencyFloodFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length
            });
        }

        await _db.SaveChangesAsync();
    }

    private static string GetEmergencyFloodFileCategory(string ext)
    {
        return ext switch
        {
            ".pdf" => "Report",
            ".mp4" or ".mov" or ".webm" => "Video",
            _ => "Photo"
        };
    }

    private async Task<ServicioEmergencia?> LoadActiveFloodEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsFloodEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaFlood?> GetActiveEmergencyFloodSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaFlood
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaFlood> GetOrCreateEmergencyFloodSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaFlood? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaFlood
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencyFloodSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaFlood
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaFlood.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaFlood?> LoadEmergencyFloodSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesEmergenciaFlood
            .Include(s => s.ServicioEmergencia)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsFloodEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencyFloodHistorialAsync(
        SolicitudEmergenciaFlood solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaFlood"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaFlood",
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
