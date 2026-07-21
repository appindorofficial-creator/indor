using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencyWaterHeaterIssue(int id)
    {
        var servicio = await LoadActiveWaterHeaterEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencyWaterHeaterSolicitudAsync(userId, id);

        return View(new EmergencyWaterHeaterIssueViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposProblema = existing?.TiposProblema ?? existing?.TipoProblema ?? "NoHotWater",
            TipoProblema = existing?.TipoProblema ?? "NoHotWater",
            Urgencia = existing?.Urgencia ?? "Emergency",
            UnidadFuncionando = existing?.UnidadFuncionando ?? "No"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyWaterHeaterIssue(EmergencyWaterHeaterIssueViewModel model, string? action)
    {
        var servicio = await LoadActiveWaterHeaterEmergencyServiceAsync(model.ServicioEmergenciaId);
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
            var solicitud = await GetOrCreateEmergencyWaterHeaterSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            var tipos = string.IsNullOrWhiteSpace(model.TiposProblema)
                ? model.TipoProblema
                : model.TiposProblema.Trim();
            var firstTipo = tipos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? model.TipoProblema;

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TiposProblema = tipos;
            solicitud.TipoProblema = firstTipo;
            solicitud.Urgencia = model.Urgencia;
            solicitud.UnidadFuncionando = model.UnidadFuncionando;
            solicitud.Estado = "IssueCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(EmergencyWaterHeaterUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(EmergencyWaterHeaterDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency water heater tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyWaterHeaterDetails(int id)
    {
        var solicitud = await LoadEmergencyWaterHeaterSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new EmergencyWaterHeaterDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            UbicacionProblema = solicitud.UbicacionProblema ?? "Garage",
            TipoUnidad = solicitud.TipoUnidad ?? "Gas",
            SintomasVisibles = solicitud.SintomasVisibles ?? string.Empty,
            NotaCorta = solicitud.NotaCorta
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyWaterHeaterDetails(EmergencyWaterHeaterDetailsViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyWaterHeaterSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyWaterHeaterIssue), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.UbicacionProblema = model.UbicacionProblema;
        solicitud.TipoUnidad = model.TipoUnidad;
        solicitud.SintomasVisibles = string.IsNullOrWhiteSpace(model.SintomasVisibles)
            ? null
            : model.SintomasVisibles.Trim();
        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.Estado = "DetailsCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyWaterHeaterUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyWaterHeaterUpload(int id)
    {
        var solicitud = await LoadEmergencyWaterHeaterSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new EmergencyWaterHeaterUploadViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            DetallesAcceso = solicitud.DetallesAcceso,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingEmergencyWaterHeaterFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] EmergencyWaterHeaterAllowedExtensions =
        [".jpg", ".jpeg", ".png", ".pdf", ".mp4", ".mov", ".webm"];

    private const long EmergencyWaterHeaterMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> EmergencyWaterHeaterUpload(
        EmergencyWaterHeaterUploadViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesEmergenciaWaterHeater
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsWaterHeaterEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyWaterHeaterDetails), new { id = solicitud.Id });
        }

        solicitud.DetallesAcceso = model.DetallesAcceso?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveEmergencyWaterHeaterFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                PopulateEmergencyWaterHeaterUploadModel(model, solicitud);
                return View(model);
            }
        }

        solicitud.Estado = "PhotosCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyWaterHeaterReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyWaterHeaterReview(int id)
    {
        var solicitud = await LoadEmergencyWaterHeaterSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new EmergencyWaterHeaterReviewViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            TituloServicio = solicitud.ServicioEmergencia!.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatWaterHeaterIssues(
                solicitud.TiposProblema,
                solicitud.TipoProblema),
            TipoUnidadResumen = EmergencyDisplayLabels.TipoUnidadWaterHeater(solicitud.TipoUnidad),
            UbicacionResumen = EmergencyDisplayLabels.UbicacionWaterHeater(solicitud.UbicacionProblema),
            UrgenciaResumen = EmergencyDisplayLabels.UrgenciaEmergencia(solicitud.Urgencia),
            UnidadFuncionandoResumen = EmergencyDisplayLabels.UnidadFuncionando(solicitud.UnidadFuncionando),
            ArchivosResumen = EmergencyDisplayLabels.ArchivosSubidos(solicitud.Archivos.Count),
            DetallesAcceso = EmergencyDisplayLabels.TextoOpcional(solicitud.DetallesAcceso)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyWaterHeaterReview(EmergencyWaterHeaterReviewViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyWaterHeaterSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase)
            || string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyWaterHeaterIssue), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencyWaterHeaterHistorialAsync(solicitud, solicitud.ServicioEmergencia!, "Submitted");

        return RedirectToAction(nameof(EmergencyWaterHeaterSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyWaterHeaterSubmitted(int id)
    {
        var solicitud = await LoadEmergencyWaterHeaterSolicitudForUserAsync(id);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;

        return View(new EmergencyWaterHeaterSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            FechaServicio = DateTime.Today,
            HoraServicio = "ASAP",
            ProblemaResumen = EmergencyDisplayLabels.FormatWaterHeaterIssues(
                solicitud.TiposProblema,
                solicitud.TipoProblema),
            UrgenciaResumen = EmergencyDisplayLabels.UrgenciaEmergencia(solicitud.Urgencia),
            EstadoResumen = EmergencyDisplayLabels.EstadoWaterHeaterConfirmado(solicitud.Estado)
        });
    }

    private static void PopulateEmergencyWaterHeaterUploadModel(
        EmergencyWaterHeaterUploadViewModel model,
        SolicitudEmergenciaWaterHeater solicitud)
    {
        model.NombreServicio = solicitud.ServicioEmergencia!.Nombre;
        model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
        model.DireccionPropiedad = solicitud.DireccionPropiedad;
        model.ArchivosExistentes = solicitud.Archivos
            .Select(a => new ExistingEmergencyWaterHeaterFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo,
                CategoriaArchivo = a.CategoriaArchivo
            })
            .ToList();
    }

    private async Task SaveEmergencyWaterHeaterFilesAsync(
        SolicitudEmergenciaWaterHeater solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "emergencias-water-heater", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > EmergencyWaterHeaterMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!EmergencyWaterHeaterAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/emergencias-water-heater/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosEmergenciaWaterHeater.Add(new ArchivoEmergenciaWaterHeater
            {
                SolicitudEmergenciaWaterHeaterId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetEmergencyWaterHeaterFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length
            });
        }

        await _db.SaveChangesAsync();
    }

    private static string GetEmergencyWaterHeaterFileCategory(string ext)
    {
        return ext switch
        {
            ".pdf" => "Report",
            ".mp4" or ".mov" or ".webm" => "Video",
            _ => "Photo"
        };
    }

    private async Task<ServicioEmergencia?> LoadActiveWaterHeaterEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsWaterHeaterEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaWaterHeater?> GetActiveEmergencyWaterHeaterSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaWaterHeater
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaWaterHeater> GetOrCreateEmergencyWaterHeaterSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaWaterHeater? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaWaterHeater
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencyWaterHeaterSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaWaterHeater
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaWaterHeater.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaWaterHeater?> LoadEmergencyWaterHeaterSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesEmergenciaWaterHeater
            .Include(s => s.ServicioEmergencia)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsWaterHeaterEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencyWaterHeaterHistorialAsync(
        SolicitudEmergenciaWaterHeater solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaWaterHeater"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaWaterHeater",
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
