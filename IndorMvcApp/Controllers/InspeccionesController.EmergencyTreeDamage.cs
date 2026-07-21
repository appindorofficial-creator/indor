using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencyTreeDamageDescribe(int id)
    {
        var servicio = await LoadActiveTreeDamageEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencyTreeDamageSolicitudAsync(userId, id);

        return View(new EmergencyTreeDamageDescribeViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoProblema = existing?.TipoProblema ?? "FallenBranch",
            UbicacionDanio = existing?.UbicacionDanio ?? "FrontYard",
            PeligroInmediato = existing?.PeligroInmediato ?? "NotSure",
            RiesgoUtilidad = existing?.RiesgoUtilidad ?? "NotSure"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyTreeDamageDescribe(EmergencyTreeDamageDescribeViewModel model, string? action)
    {
        var servicio = await LoadActiveTreeDamageEmergencyServiceAsync(model.ServicioEmergenciaId);
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
            var solicitud = await GetOrCreateEmergencyTreeDamageSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoProblema = model.TipoProblema;
            solicitud.UbicacionDanio = model.UbicacionDanio;
            solicitud.PeligroInmediato = model.PeligroInmediato;
            solicitud.RiesgoUtilidad = model.RiesgoUtilidad;
            solicitud.Estado = "DescribeCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(EmergencyTreeDamagePhotos), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency tree damage tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyTreeDamagePhotos(int id)
    {
        var solicitud = await LoadEmergencyTreeDamageSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        var telefono = solicitud.TelefonoContacto;
        if (string.IsNullOrWhiteSpace(telefono))
        {
            telefono = user?.Telefono ?? string.Empty;
        }

        return View(new EmergencyTreeDamagePhotosViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            AccesoCasa = solicitud.AccesoCasa ?? "Yes",
            EntradaBloqueada = solicitud.EntradaBloqueada ?? "NotSure",
            PuedeAlejarse = solicitud.PuedeAlejarse ?? "Yes",
            TelefonoContacto = telefono,
            NotaCorta = solicitud.NotaCorta,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingEmergencyTreeDamageFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] EmergencyTreeDamageAllowedExtensions =
        [".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long EmergencyTreeDamageMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> EmergencyTreeDamagePhotos(
        EmergencyTreeDamagePhotosViewModel model,
        string? action,
        List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesEmergenciaTreeDamage
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsTreeDamageEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyTreeDamageDescribe), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.AccesoCasa = model.AccesoCasa;
        solicitud.EntradaBloqueada = model.EntradaBloqueada;
        solicitud.PuedeAlejarse = model.PuedeAlejarse;
        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.TelefonoContacto = model.TelefonoContacto?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveEmergencyTreeDamageFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                PopulateEmergencyTreeDamagePhotosModel(model, solicitud);
                return View(model);
            }
        }

        if (string.IsNullOrWhiteSpace(model.TelefonoContacto))
        {
            ModelState.AddModelError(nameof(model.TelefonoContacto), "Please enter a callback number.");
            PopulateEmergencyTreeDamagePhotosModel(model, solicitud);
            return View(model);
        }

        solicitud.Estado = "PhotosCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencyTreeDamageReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyTreeDamageReview(int id)
    {
        var solicitud = await LoadEmergencyTreeDamageSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var minutos = solicitud.ServicioEmergencia!.TiempoLlegadaMinutos > 0
            ? solicitud.ServicioEmergencia.TiempoLlegadaMinutos
            : 45;

        return View(new EmergencyTreeDamageReviewViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            TituloServicio = $"{solicitud.ServicioEmergencia.TituloEmergencia} 24/7",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatProblemaTreeDamage(
                solicitud.TipoProblema,
                solicitud.UbicacionDanio),
            UbicacionResumen = EmergencyDisplayLabels.FormatUbicacionTreeDamage(
                solicitud.UbicacionDanio,
                solicitud.TipoProblema),
            PeligroInmediatoResumen = EmergencyDisplayLabels.PeligroInmediatoTreeDamage(solicitud.PeligroInmediato),
            RiesgoUtilidadResumen = EmergencyDisplayLabels.RiesgoUtilidadTreeDamage(solicitud.RiesgoUtilidad),
            AccesoCasaResumen = EmergencyDisplayLabels.AccesoCasaTreeDamage(solicitud.AccesoCasa),
            ArchivosResumen = EmergencyDisplayLabels.ArchivosAdjuntosTreeDamage(solicitud.Archivos.Count),
            TelefonoContacto = solicitud.TelefonoContacto ?? string.Empty,
            TiempoLlegadaRango = EmergencyDisplayLabels.TiempoLlegadaRangoTreeDamage(minutos),
            NotaCorta = EmergencyDisplayLabels.TextoOpcional(solicitud.NotaCorta)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencyTreeDamageReview(EmergencyTreeDamageReviewViewModel model, string? action)
    {
        var solicitud = await LoadEmergencyTreeDamageSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase)
            || string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencyTreeDamageDescribe), new { id = solicitud.ServicioEmergenciaId });
        }

        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencyTreeDamageHistorialAsync(solicitud, solicitud.ServicioEmergencia!, "Submitted");

        return RedirectToAction(nameof(EmergencyTreeDamageSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencyTreeDamageSubmitted(int id)
    {
        var solicitud = await LoadEmergencyTreeDamageSolicitudForUserAsync(id);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;
        var minutos = servicio.TiempoLlegadaMinutos > 0 ? servicio.TiempoLlegadaMinutos : 45;

        return View(new EmergencyTreeDamageSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = $"{servicio.TituloEmergencia} 24/7",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatProblemaTreeDamage(
                solicitud.TipoProblema,
                solicitud.UbicacionDanio),
            TelefonoContacto = solicitud.TelefonoContacto ?? string.Empty,
            TiempoLlegadaRango = EmergencyDisplayLabels.TiempoLlegadaRangoTreeDamage(minutos),
            EstadoResumen = EmergencyDisplayLabels.EstadoTreeDamageConfirmado(solicitud.Estado)
        });
    }

    private static void PopulateEmergencyTreeDamagePhotosModel(
        EmergencyTreeDamagePhotosViewModel model,
        SolicitudEmergenciaTreeDamage solicitud)
    {
        model.NombreServicio = solicitud.ServicioEmergencia!.Nombre;
        model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
        model.DireccionPropiedad = solicitud.DireccionPropiedad;
        model.ArchivosExistentes = solicitud.Archivos
            .Select(a => new ExistingEmergencyTreeDamageFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo,
                CategoriaArchivo = a.CategoriaArchivo
            })
            .ToList();
    }

    private async Task SaveEmergencyTreeDamageFilesAsync(
        SolicitudEmergenciaTreeDamage solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "emergencias-tree-damage", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > EmergencyTreeDamageMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!EmergencyTreeDamageAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/emergencias-tree-damage/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosEmergenciaTreeDamage.Add(new ArchivoEmergenciaTreeDamage
            {
                SolicitudEmergenciaTreeDamageId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetEmergencyTreeDamageFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length
            });
        }

        await _db.SaveChangesAsync();
    }

    private static string GetEmergencyTreeDamageFileCategory(string ext)
    {
        return ext switch
        {
            ".mp4" or ".mov" or ".webm" => "Video",
            _ => "Photo"
        };
    }

    private async Task<ServicioEmergencia?> LoadActiveTreeDamageEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsTreeDamageEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaTreeDamage?> GetActiveEmergencyTreeDamageSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaTreeDamage
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaTreeDamage> GetOrCreateEmergencyTreeDamageSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaTreeDamage? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaTreeDamage
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencyTreeDamageSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaTreeDamage
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaTreeDamage.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaTreeDamage?> LoadEmergencyTreeDamageSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesEmergenciaTreeDamage
            .Include(s => s.ServicioEmergencia)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsTreeDamageEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencyTreeDamageHistorialAsync(
        SolicitudEmergenciaTreeDamage solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaTreeDamage"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaTreeDamage",
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
