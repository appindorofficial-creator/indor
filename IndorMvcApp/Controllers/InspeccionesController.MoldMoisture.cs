using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> MoldMoistureDetails(int id)
    {
        var inspeccion = await LoadActiveMoldMoistureInspeccionAsync(id);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveMoldMoistureSolicitudAsync(userId, id);

        var model = new MoldMoistureDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposProblema = existing?.TiposProblema ?? existing?.TipoProblema ?? "VisibleMold",
            TipoProblema = existing?.TipoProblema ?? "VisibleMold",
            UbicacionProblema = existing?.UbicacionProblema ?? "Bathroom",
            Urgencia = existing?.Urgencia ?? "Normal",
            HumedadActiva = existing?.HumedadActiva ?? "Yes"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoldMoistureDetails(MoldMoistureDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveMoldMoistureInspeccionAsync(model.InspeccionId);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your mold and moisture details anytime.";
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
            var solicitud = await GetOrCreateMoldMoistureSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            var tipos = string.IsNullOrWhiteSpace(model.TiposProblema)
                ? model.TipoProblema
                : model.TiposProblema.Trim();
            var firstTipo = tipos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? model.TipoProblema;

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TiposProblema = tipos;
            solicitud.TipoProblema = firstTipo;
            solicitud.UbicacionProblema = model.UbicacionProblema;
            solicitud.Urgencia = model.Urgencia;
            solicitud.HumedadActiva = model.HumedadActiva;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(MoldMoistureUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(MoldMoisturePropertyContext), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the mold and moisture inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> MoldMoisturePropertyContext(int id)
    {
        var solicitud = await LoadMoldMoistureSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new MoldMoisturePropertyContextViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            MotivoRevision = solicitud.MotivoRevision ?? "MustySmellConcern",
            TipoPropiedad = solicitud.TipoPropiedad ?? "SingleFamily",
            UbicacionPrincipal = solicitud.UbicacionPrincipal ?? solicitud.UbicacionProblema ?? "Bathroom",
            IntrusionAguaReciente = solicitud.IntrusionAguaReciente ?? "Yes",
            AccesoPreferido = solicitud.AccesoPreferido ?? "SomeoneHome",
            AreasEnfoque = solicitud.AreasEnfoque ?? BuildDefaultMoldMoistureFocusAreas(solicitud)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoldMoisturePropertyContext(MoldMoisturePropertyContextViewModel model, string? action)
    {
        var solicitud = await LoadMoldMoistureSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(MoldMoistureDetails), new { id = solicitud.InspeccionId });
        }

        solicitud.MotivoRevision = model.MotivoRevision;
        solicitud.TipoPropiedad = model.TipoPropiedad;
        solicitud.UbicacionPrincipal = model.UbicacionPrincipal;
        solicitud.IntrusionAguaReciente = model.IntrusionAguaReciente;
        solicitud.AccesoPreferido = model.AccesoPreferido;
        solicitud.AreasEnfoque = string.IsNullOrWhiteSpace(model.AreasEnfoque)
            ? BuildDefaultMoldMoistureFocusAreas(solicitud)
            : model.AreasEnfoque.Trim();
        solicitud.Estado = "PropertyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(MoldMoistureUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> MoldMoistureUpload(int id)
    {
        var solicitud = await LoadMoldMoistureSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new MoldMoistureUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingMoldMoistureFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] MoldMoistureAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long MoldMoistureMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> MoldMoistureUpload(MoldMoistureUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesInspeccionMoldMoisture
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsMoldMoistureFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveMoldMoistureFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                model.NombreInspeccion = solicitud.Inspeccion.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingMoldMoistureFileViewModel
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

        return RedirectToAction(nameof(MoldMoistureReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> MoldMoistureReview(int id)
    {
        var solicitud = await LoadMoldMoistureSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var inspeccion = solicitud.Inspeccion!;
        var archivos = solicitud.Archivos.OrderByDescending(a => a.FechaSubida).ToList();

        var model = new MoldMoistureReviewViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo ?? "On-site evaluation by a certified INDOR inspector.",
            Precio = inspeccion.Valor ?? 0,
            Moneda = inspeccion.Moneda,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemasPrincipales = InspeccionDisplayLabels.FormatMoldMoistureReviewItems(
                solicitud.TiposProblema, solicitud.TipoProblema, solicitud.UbicacionProblema, solicitud.Urgencia),
            PropiedadResumen = InspeccionDisplayLabels.FormatMoldMoisturePropertySummary(solicitud.TipoPropiedad),
            AccesoResumen = InspeccionDisplayLabels.AccesoPreferidoStructural(solicitud.AccesoPreferido),
            ArchivosResumen = archivos.Count == 0
                ? "No files uploaded"
                : $"{archivos.Count} photo{(archivos.Count == 1 ? "" : "s")} uploaded",
            Archivos = archivos
                .Select(a => new ExistingMoldMoistureFileViewModel
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
    public async Task<IActionResult> MoldMoistureReview(MoldMoistureReviewViewModel model, string? action)
    {
        var solicitud = await LoadMoldMoistureSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(MoldMoistureUpload), new { id = solicitud.Id });
        }

        solicitud.Estado = "Confirmed";
        AssignMoldMoistureAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertMoldMoistureHistorialAsync(solicitud, solicitud.Inspeccion!, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "moldmoisture", id = solicitud.Id });
    }

    private async Task SaveMoldMoistureFilesAsync(
        SolicitudInspeccionMoldMoisture solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "inspecciones-mold-moisture", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > MoldMoistureMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!MoldMoistureAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/inspecciones-mold-moisture/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosInspeccionMoldMoisture.Add(new ArchivoInspeccionMoldMoisture
            {
                SolicitudInspeccionMoldMoistureId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetElectricalFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static string BuildDefaultMoldMoistureFocusAreas(SolicitudInspeccionMoldMoisture solicitud)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(solicitud.UbicacionPrincipal))
        {
            parts.Add(solicitud.UbicacionPrincipal);
        }
        else if (!string.IsNullOrWhiteSpace(solicitud.UbicacionProblema))
        {
            parts.Add(solicitud.UbicacionProblema);
        }

        parts.AddRange(
            (solicitud.TiposProblema ?? solicitud.TipoProblema ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return string.Join("|", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static (DateTime Date, TimeSpan Time) ComputeMoldMoistureAppointment(SolicitudInspeccionMoldMoisture solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Emergency" => 1,
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(11, 0, 0));
    }

    private static void AssignMoldMoistureAppointment(SolicitudInspeccionMoldMoisture solicitud)
    {
        var (date, time) = ComputeMoldMoistureAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureMoldMoistureAppointmentSavedAsync(SolicitudInspeccionMoldMoisture solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignMoldMoistureAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveMoldMoistureInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsMoldMoistureFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionMoldMoisture?> GetActiveMoldMoistureSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionMoldMoisture
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionMoldMoisture> GetOrCreateMoldMoistureSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionMoldMoisture? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionMoldMoisture
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveMoldMoistureSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionMoldMoisture
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionMoldMoisture.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudInspeccionMoldMoisture?> LoadMoldMoistureSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesInspeccionMoldMoisture
            .Include(s => s.Inspeccion)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsMoldMoistureFlow(solicitud.Inspeccion.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertMoldMoistureHistorialAsync(
        SolicitudInspeccionMoldMoisture solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionMoldMoisture"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionMoldMoisture",
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
