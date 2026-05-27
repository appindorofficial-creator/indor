using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencyElectricalProblem(int id)
    {
        var servicio = await LoadActiveElectricalEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencyElectricalSolicitudAsync(userId, id);

        return View(new EmergencyElectricalProblemViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoProblema = existing?.TipoProblema ?? "BreakerTripping",
            Urgencia = existing?.Urgencia ?? "Emergency",
            PuedeApagarBreaker = existing?.PuedeApagarBreaker ?? "NotSure"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyElectricalProblem(EmergencyElectricalProblemViewModel model, string? action)
    {
        var servicio = await LoadActiveElectricalEmergencyServiceAsync(model.ServicioEmergenciaId);
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
            var solicitud = await GetOrCreateEmergencyElectricalSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoProblema = model.TipoProblema;
            solicitud.Urgencia = model.Urgencia;
            solicitud.PuedeApagarBreaker = model.PuedeApagarBreaker;
            solicitud.Estado = "ProblemCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(EmergencyElectricalContact), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(EmergencyElectricalLocation), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency electrical tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyElectricalLocation(int id)
    {
        var solicitud = await LoadEmergencyElectricalSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new EmergencyElectricalLocationViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.TipoProblemaElectrical(solicitud.TipoProblema),
            UbicacionProblema = solicitud.UbicacionProblema ?? "Garage",
            SintomasNotados = solicitud.SintomasNotados ?? string.Empty,
            EnergiaEncendida = solicitud.EnergiaEncendida ?? "Yes",
            PuedeAlejarse = solicitud.PuedeAlejarse ?? "Yes"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyElectricalLocation(EmergencyElectricalLocationViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyElectricalSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyElectricalProblem), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.UbicacionProblema = model.UbicacionProblema;
        solicitud.SintomasNotados = string.IsNullOrWhiteSpace(model.SintomasNotados)
            ? null
            : model.SintomasNotados.Trim();
        solicitud.EnergiaEncendida = model.EnergiaEncendida;
        solicitud.PuedeAlejarse = model.PuedeAlejarse;
        solicitud.Estado = "LocationCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyElectricalContact), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyElectricalContact(int id)
    {
        var solicitud = await LoadEmergencyElectricalSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        var telefono = solicitud.TelefonoContacto;
        if (string.IsNullOrWhiteSpace(telefono))
        {
            telefono = user?.Telefono ?? string.Empty;
        }

        return View(new EmergencyElectricalContactViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.TipoProblemaElectrical(solicitud.TipoProblema),
            NotaCorta = solicitud.NotaCorta,
            TelefonoContacto = telefono,
            AceptaTextos = solicitud.AceptaTextos ?? "Yes",
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingEmergencyElectricalFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] EmergencyElectricalAllowedExtensions =
        [".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long EmergencyElectricalMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> EmergencyElectricalContact(
        EmergencyElectricalContactViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesEmergenciaElectrical
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsElectricalEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyElectricalLocation), new { id = solicitud.Id });
        }

        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.TelefonoContacto = model.TelefonoContacto?.Trim();
        solicitud.AceptaTextos = model.AceptaTextos;

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveEmergencyElectricalFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                PopulateEmergencyElectricalContactModel(model, solicitud);
                return View(model);
            }
        }

        if (string.IsNullOrWhiteSpace(model.TelefonoContacto))
        {
            ModelState.AddModelError(nameof(model.TelefonoContacto), "Please enter a callback number.");
            PopulateEmergencyElectricalContactModel(model, solicitud);
            return View(model);
        }

        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencyElectricalHistorialAsync(solicitud, solicitud.ServicioEmergencia!, "Submitted");

        return RedirectToAction(nameof(EmergencyElectricalSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyElectricalSubmitted(int id)
    {
        var solicitud = await LoadEmergencyElectricalSolicitudForUserAsync(id);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;
        var minutos = servicio.TiempoLlegadaMinutos > 0 ? servicio.TiempoLlegadaMinutos : 45;

        return View(new EmergencyElectricalSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = $"{servicio.TituloEmergencia} 24/7",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.TipoProblemaElectrical(solicitud.TipoProblema),
            AreaResumen = EmergencyDisplayLabels.FormatAreaElectrical(solicitud.UbicacionProblema),
            EnergiaEncendidaResumen = EmergencyDisplayLabels.EnergiaEncendidaElectrical(solicitud.EnergiaEncendida),
            PuedeApagarBreakerResumen = EmergencyDisplayLabels.PuedeApagarBreakerElectrical(solicitud.PuedeApagarBreaker),
            EstadoResumen = EmergencyDisplayLabels.EstadoElectricalConfirmado(solicitud.Estado),
            TiempoLlegadaRango = EmergencyDisplayLabels.TiempoLlegadaRangoElectrical(minutos)
        });
    }

    private static void PopulateEmergencyElectricalContactModel(
        EmergencyElectricalContactViewModel model,
        SolicitudEmergenciaElectrical solicitud)
    {
        model.NombreServicio = solicitud.ServicioEmergencia!.Nombre;
        model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
        model.DireccionPropiedad = solicitud.DireccionPropiedad;
        model.ProblemaResumen = EmergencyDisplayLabels.TipoProblemaElectrical(solicitud.TipoProblema);
        model.ArchivosExistentes = solicitud.Archivos
            .Select(a => new ExistingEmergencyElectricalFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo,
                CategoriaArchivo = a.CategoriaArchivo
            })
            .ToList();
    }

    private async Task SaveEmergencyElectricalFilesAsync(
        SolicitudEmergenciaElectrical solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "emergencias-electrical", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > EmergencyElectricalMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!EmergencyElectricalAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/emergencias-electrical/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosEmergenciaElectrical.Add(new ArchivoEmergenciaElectrical
            {
                SolicitudEmergenciaElectricalId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetEmergencyElectricalFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length
            });
        }

        await _db.SaveChangesAsync();
    }

    private static string GetEmergencyElectricalFileCategory(string ext)
    {
        return ext switch
        {
            ".mp4" or ".mov" or ".webm" => "Video",
            _ => "Photo"
        };
    }

    private async Task<ServicioEmergencia?> LoadActiveElectricalEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsElectricalEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaElectrical?> GetActiveEmergencyElectricalSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaElectrical
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaElectrical> GetOrCreateEmergencyElectricalSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaElectrical? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaElectrical
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencyElectricalSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaElectrical
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaElectrical.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaElectrical?> LoadEmergencyElectricalSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesEmergenciaElectrical
            .Include(s => s.ServicioEmergencia)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsElectricalEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencyElectricalHistorialAsync(
        SolicitudEmergenciaElectrical solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaElectrical"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaElectrical",
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
