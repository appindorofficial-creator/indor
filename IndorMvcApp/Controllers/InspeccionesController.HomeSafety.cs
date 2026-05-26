using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> HomeSafetyDetails(int id)
    {
        var inspeccion = await LoadActiveHomeSafetyInspeccionAsync(id);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveHomeSafetySolicitudAsync(userId, id);

        var model = new HomeSafetyDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposProblema = existing?.TiposProblema ?? existing?.TipoProblema ?? "SmokeDetectorConcern",
            TipoProblema = existing?.TipoProblema ?? "SmokeDetectorConcern",
            AreasAtencion = existing?.AreasAtencion ?? existing?.UbicacionProblema ?? "Hallway",
            UbicacionProblema = existing?.UbicacionProblema ?? "Hallway",
            Urgencia = existing?.Urgencia ?? "Normal",
            RiesgoActivo = existing?.RiesgoActivo ?? "No"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HomeSafetyDetails(HomeSafetyDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveHomeSafetyInspeccionAsync(model.InspeccionId);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your home safety details anytime.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateHomeSafetySolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            var tipos = string.IsNullOrWhiteSpace(model.TiposProblema)
                ? model.TipoProblema
                : model.TiposProblema.Trim();
            var firstTipo = tipos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? model.TipoProblema;

            var areas = string.IsNullOrWhiteSpace(model.AreasAtencion)
                ? model.UbicacionProblema
                : model.AreasAtencion.Trim();
            var firstArea = areas.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? model.UbicacionProblema;

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TiposProblema = tipos;
            solicitud.TipoProblema = firstTipo;
            solicitud.AreasAtencion = areas;
            solicitud.UbicacionProblema = firstArea;
            solicitud.Urgencia = model.Urgencia;
            solicitud.RiesgoActivo = model.RiesgoActivo;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(HomeSafetyUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(HomeSafetyPropertyContext), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the home safety inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> HomeSafetyPropertyContext(int id)
    {
        var solicitud = await LoadHomeSafetySolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new HomeSafetyPropertyContextViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            MotivosRevision = solicitud.MotivosRevision ?? solicitud.MotivoRevision ?? "AnnualReview",
            MotivoRevision = solicitud.MotivoRevision ?? "AnnualReview",
            TipoPropiedad = solicitud.TipoPropiedad ?? "SingleFamily",
            NumeroPisos = solicitud.NumeroPisos ?? "TwoStory",
            AreasEnfoque = solicitud.AreasEnfoque ?? BuildDefaultHomeSafetyFocusAreas(solicitud),
            AccesoPreferido = solicitud.AccesoPreferido ?? "SomeoneHome",
            OcupantesHogar = solicitud.OcupantesHogar ?? "Children"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HomeSafetyPropertyContext(HomeSafetyPropertyContextViewModel model, string? action)
    {
        var solicitud = await LoadHomeSafetySolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(HomeSafetyDetails), new { id = solicitud.InspeccionId });
        }

        var motivos = string.IsNullOrWhiteSpace(model.MotivosRevision)
            ? model.MotivoRevision
            : model.MotivosRevision.Trim();
        var firstMotivo = motivos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? model.MotivoRevision;

        solicitud.MotivosRevision = motivos;
        solicitud.MotivoRevision = firstMotivo;
        solicitud.TipoPropiedad = model.TipoPropiedad;
        solicitud.NumeroPisos = model.NumeroPisos;
        solicitud.AccesoPreferido = model.AccesoPreferido;
        solicitud.OcupantesHogar = model.OcupantesHogar?.Trim();
        solicitud.AreasEnfoque = string.IsNullOrWhiteSpace(model.AreasEnfoque)
            ? BuildDefaultHomeSafetyFocusAreas(solicitud)
            : model.AreasEnfoque.Trim();
        solicitud.Estado = "PropertyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(HomeSafetyUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> HomeSafetyUpload(int id)
    {
        var solicitud = await LoadHomeSafetySolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new HomeSafetyUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingHomeSafetyFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] HomeSafetyAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long HomeSafetyMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> HomeSafetyUpload(HomeSafetyUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesInspeccionHomeSafety
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsHomeSafetyFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveHomeSafetyFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                model.NombreInspeccion = solicitud.Inspeccion.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingHomeSafetyFileViewModel
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

        solicitud.Estado = "PhotosCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(HomeSafetyReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> HomeSafetyReview(int id)
    {
        var solicitud = await LoadHomeSafetySolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var inspeccion = solicitud.Inspeccion!;
        var archivos = solicitud.Archivos.OrderByDescending(a => a.FechaSubida).ToList();

        var model = new HomeSafetyReviewViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo ?? "Security and prevention systems review.",
            FrecuenciaInspeccion = inspeccion.Frecuencia,
            Precio = inspeccion.Valor ?? 0,
            Moneda = inspeccion.Moneda,
            PrecioPrefijo = inspeccion.PrecioPrefijo ?? "From",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            PreocupacionPrincipal = InspeccionDisplayLabels.FormatHomeSafetyMainConcern(
                solicitud.TiposProblema, solicitud.TipoProblema, solicitud.RiesgoActivo),
            PropiedadResumen = InspeccionDisplayLabels.FormatHomeSafetyPropertySummary(
                solicitud.TipoPropiedad, solicitud.NumeroPisos),
            AreasEnfoqueResumen = InspeccionDisplayLabels.FormatHomeSafetyFocusAreas(
                solicitud.AreasEnfoque, solicitud.AreasAtencion),
            AccesoResumen = InspeccionDisplayLabels.AccesoPreferidoStructural(solicitud.AccesoPreferido),
            ArchivosResumen = InspeccionDisplayLabels.FormatHomeSafetyFilesSummary(
                archivos.Select(a => (a.CategoriaArchivo, (string?)a.NombreArchivo))),
            Archivos = archivos
                .Select(a => new ExistingHomeSafetyFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList(),
            ComentariosProveedor = solicitud.ComentariosProveedor
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HomeSafetyReview(HomeSafetyReviewViewModel model, string? action)
    {
        var solicitud = await LoadHomeSafetySolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(HomeSafetyUpload), new { id = solicitud.Id });
        }

        solicitud.Estado = "Confirmed";
        AssignHomeSafetyAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertHomeSafetyHistorialAsync(solicitud, solicitud.Inspeccion!, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "homesafety", id = solicitud.Id });
    }

    private async Task SaveHomeSafetyFilesAsync(
        SolicitudInspeccionHomeSafety solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "inspecciones-home-safety", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > HomeSafetyMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!HomeSafetyAllowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("",
                    $"File {file.FileName} is not allowed. Use JPG, PNG, PDF, MP4, MOV, or WEBM.");
                continue;
            }

            var storedName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
            var physicalPath = Path.Combine(uploadFolder, storedName);
            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/inspecciones-home-safety/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosInspeccionHomeSafety.Add(new ArchivoInspeccionHomeSafety
            {
                SolicitudInspeccionHomeSafetyId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetElectricalFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static string BuildDefaultHomeSafetyFocusAreas(SolicitudInspeccionHomeSafety solicitud)
    {
        var parts = ParseHomeSafetyPipeValues(solicitud.AreasAtencion);
        if (parts.Count == 0 && !string.IsNullOrWhiteSpace(solicitud.UbicacionProblema))
        {
            parts.Add(solicitud.UbicacionProblema);
        }

        parts.AddRange(
            (solicitud.TiposProblema ?? solicitud.TipoProblema ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return string.Join("|", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static List<string> ParseHomeSafetyPipeValues(string? pipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(pipeSeparated))
        {
            return new List<string>();
        }

        return pipeSeparated
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static (DateTime Date, TimeSpan Time) ComputeHomeSafetyAppointment(SolicitudInspeccionHomeSafety solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(11, 0, 0));
    }

    private static void AssignHomeSafetyAppointment(SolicitudInspeccionHomeSafety solicitud)
    {
        var (date, time) = ComputeHomeSafetyAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureHomeSafetyAppointmentSavedAsync(SolicitudInspeccionHomeSafety solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignHomeSafetyAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveHomeSafetyInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsHomeSafetyFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionHomeSafety?> GetActiveHomeSafetySolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionHomeSafety
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionHomeSafety> GetOrCreateHomeSafetySolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionHomeSafety? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionHomeSafety
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveHomeSafetySolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionHomeSafety
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionHomeSafety.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudInspeccionHomeSafety?> LoadHomeSafetySolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesInspeccionHomeSafety
            .Include(s => s.Inspeccion)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsHomeSafetyFlow(solicitud.Inspeccion.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertHomeSafetyHistorialAsync(
        SolicitudInspeccionHomeSafety solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionHomeSafety"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionHomeSafety",
                ItemId = solicitud.Id,
                NombreItem = inspeccion.Nombre,
                Fecha = DateTime.Now
            };
            _db.HistorialServicios.Add(historial);
        }

        historial.Estado = estado;
        historial.Monto = inspeccion.Valor;
        historial.Moneda = inspeccion.Moneda;
        historial.Notas = solicitud.DireccionPropiedad;
        historial.Fecha = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
