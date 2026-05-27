using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencyRoofLeakDescribe(int id)
    {
        var servicio = await LoadActiveRoofLeakEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencyRoofLeakSolicitudAsync(userId, id);

        return View(new EmergencyRoofLeakDescribeViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoProblema = existing?.TipoProblema ?? "ActiveDripping",
            UbicacionFuga = existing?.UbicacionFuga ?? "Ceiling",
            Urgencia = existing?.Urgencia ?? "Emergency"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyRoofLeakDescribe(EmergencyRoofLeakDescribeViewModel model, string? action)
    {
        var servicio = await LoadActiveRoofLeakEmergencyServiceAsync(model.ServicioEmergenciaId);
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
            var solicitud = await GetOrCreateEmergencyRoofLeakSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoProblema = model.TipoProblema;
            solicitud.UbicacionFuga = model.UbicacionFuga;
            solicitud.Urgencia = model.Urgencia;
            solicitud.Estado = "DescribeCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(EmergencyRoofLeakDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency roof leak tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyRoofLeakDetails(int id)
    {
        var solicitud = await LoadEmergencyRoofLeakSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new EmergencyRoofLeakDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            NotaCorta = solicitud.NotaCorta,
            PuedeColocarCubeta = solicitud.PuedeColocarCubeta ?? "Yes",
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingEmergencyRoofLeakFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] EmergencyRoofLeakAllowedExtensions =
        [".jpg", ".jpeg", ".png", ".pdf", ".mp4", ".mov", ".webm"];

    private const long EmergencyRoofLeakMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> EmergencyRoofLeakDetails(
        EmergencyRoofLeakDetailsViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesEmergenciaRoofLeak
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsRoofLeakEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyRoofLeakDescribe), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.PuedeColocarCubeta = model.PuedeColocarCubeta;

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveEmergencyRoofLeakFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                PopulateEmergencyRoofLeakDetailsModel(model, solicitud);
                return View(model);
            }
        }

        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencyRoofLeakHistorialAsync(solicitud, solicitud.ServicioEmergencia!, "Submitted");

        return RedirectToAction(nameof(EmergencyRoofLeakSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyRoofLeakSubmitted(int id)
    {
        var solicitud = await LoadEmergencyRoofLeakSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;
        var minutos = servicio.TiempoLlegadaMinutos > 0 ? servicio.TiempoLlegadaMinutos : 45;

        return View(new EmergencyRoofLeakSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatProblemaRoofLeak(
                solicitud.TipoProblema,
                solicitud.UbicacionFuga),
            AreaResumen = EmergencyDisplayLabels.FormatAreaRoofLeak(
                solicitud.UbicacionFuga,
                solicitud.NotaCorta),
            UrgenciaResumen = EmergencyDisplayLabels.UrgenciaEmergencia(solicitud.Urgencia),
            ArchivosResumen = EmergencyDisplayLabels.ArchivosAdjuntosRoofLeak(solicitud.Archivos.Count),
            TiempoLlegadaRango = EmergencyDisplayLabels.TiempoLlegadaRangoRoofLeak(minutos),
            EstadoResumen = EmergencyDisplayLabels.EstadoRoofLeakConfirmado(solicitud.Estado)
        });
    }

    private static void PopulateEmergencyRoofLeakDetailsModel(
        EmergencyRoofLeakDetailsViewModel model,
        SolicitudEmergenciaRoofLeak solicitud)
    {
        model.NombreServicio = solicitud.ServicioEmergencia!.Nombre;
        model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
        model.DireccionPropiedad = solicitud.DireccionPropiedad;
        model.ArchivosExistentes = solicitud.Archivos
            .Select(a => new ExistingEmergencyRoofLeakFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo,
                CategoriaArchivo = a.CategoriaArchivo
            })
            .ToList();
    }

    private async Task SaveEmergencyRoofLeakFilesAsync(
        SolicitudEmergenciaRoofLeak solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "emergencias-roof-leak", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > EmergencyRoofLeakMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!EmergencyRoofLeakAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/emergencias-roof-leak/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosEmergenciaRoofLeak.Add(new ArchivoEmergenciaRoofLeak
            {
                SolicitudEmergenciaRoofLeakId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetEmergencyRoofLeakFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length
            });
        }

        await _db.SaveChangesAsync();
    }

    private static string GetEmergencyRoofLeakFileCategory(string ext)
    {
        return ext switch
        {
            ".pdf" => "Report",
            ".mp4" or ".mov" or ".webm" => "Video",
            _ => "Photo"
        };
    }

    private async Task<ServicioEmergencia?> LoadActiveRoofLeakEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsRoofLeakEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaRoofLeak?> GetActiveEmergencyRoofLeakSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaRoofLeak
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaRoofLeak> GetOrCreateEmergencyRoofLeakSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaRoofLeak? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaRoofLeak
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencyRoofLeakSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaRoofLeak
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaRoofLeak.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaRoofLeak?> LoadEmergencyRoofLeakSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesEmergenciaRoofLeak
            .Include(s => s.ServicioEmergencia)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsRoofLeakEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencyRoofLeakHistorialAsync(
        SolicitudEmergenciaRoofLeak solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaRoofLeak"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaRoofLeak",
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
