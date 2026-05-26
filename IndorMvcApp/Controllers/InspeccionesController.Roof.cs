using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> RoofDetails(int id)
    {
        var inspeccion = await LoadActiveRoofInspeccionAsync(id);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveRoofSolicitudAsync(userId, id);

        var model = new RoofDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposProblema = existing?.TiposProblema ?? existing?.TipoProblema ?? "ActiveLeak",
            TipoProblema = existing?.TipoProblema ?? "ActiveLeak",
            UbicacionProblema = existing?.UbicacionProblema ?? "MainRoof",
            Urgencia = existing?.Urgencia ?? "Normal"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoofDetails(RoofDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveRoofInspeccionAsync(model.InspeccionId);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your roof details anytime.";
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
            var solicitud = await GetOrCreateRoofSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

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
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(RoofUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(RoofPropertyContext), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the roof inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> RoofPropertyContext(int id)
    {
        var solicitud = await LoadRoofSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new RoofPropertyContextViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            MotivoRevision = solicitud.MotivoRevision ?? "LeakConcern",
            TipoPropiedad = solicitud.TipoPropiedad ?? "SingleFamily",
            NumeroPisos = solicitud.NumeroPisos ?? "Two",
            MaterialTecho = solicitud.MaterialTecho ?? "AsphaltShingles",
            AccesoPreferido = solicitud.AccesoPreferido ?? "SomeoneHome",
            AreasEnfoque = solicitud.AreasEnfoque ?? BuildDefaultRoofFocusAreas(solicitud)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoofPropertyContext(RoofPropertyContextViewModel model, string? action)
    {
        var solicitud = await LoadRoofSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RoofDetails), new { id = solicitud.InspeccionId });
        }

        solicitud.MotivoRevision = model.MotivoRevision;
        solicitud.TipoPropiedad = model.TipoPropiedad;
        solicitud.NumeroPisos = model.NumeroPisos;
        solicitud.MaterialTecho = model.MaterialTecho;
        solicitud.AccesoPreferido = model.AccesoPreferido;
        solicitud.AreasEnfoque = string.IsNullOrWhiteSpace(model.AreasEnfoque)
            ? BuildDefaultRoofFocusAreas(solicitud)
            : model.AreasEnfoque.Trim();
        solicitud.Estado = "PropertyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(RoofUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> RoofUpload(int id)
    {
        var solicitud = await LoadRoofSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new RoofUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingRoofFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] RoofAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long RoofMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> RoofUpload(RoofUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesInspeccionRoof
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsRoofFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveRoofFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                model.NombreInspeccion = solicitud.Inspeccion.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingRoofFileViewModel
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

        return RedirectToAction(nameof(RoofReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> RoofReview(int id)
    {
        var solicitud = await LoadRoofSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var inspeccion = solicitud.Inspeccion!;
        var archivos = solicitud.Archivos.OrderByDescending(a => a.FechaSubida).ToList();

        var model = new RoofReviewViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo ?? "On-site evaluation by a certified INDOR inspector.",
            Precio = inspeccion.Valor ?? 0,
            Moneda = inspeccion.Moneda,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemasPrincipales = InspeccionDisplayLabels.FormatRoofProblemsList(
                solicitud.TiposProblema, solicitud.TipoProblema, solicitud.UbicacionProblema),
            PropiedadResumen = InspeccionDisplayLabels.FormatRoofPropertySummary(
                solicitud.TipoPropiedad, solicitud.NumeroPisos, solicitud.MaterialTecho),
            AccesoResumen = InspeccionDisplayLabels.AccesoPreferidoStructural(solicitud.AccesoPreferido),
            ArchivosResumen = archivos.Count == 0
                ? "No files uploaded"
                : $"{archivos.Count} photo{(archivos.Count == 1 ? "" : "s")} uploaded",
            Archivos = archivos
                .Select(a => new ExistingRoofFileViewModel
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
    public async Task<IActionResult> RoofReview(RoofReviewViewModel model, string? action)
    {
        var solicitud = await LoadRoofSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(RoofUpload), new { id = solicitud.Id });
        }

        solicitud.Estado = "Confirmed";
        AssignRoofAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertRoofHistorialAsync(solicitud, solicitud.Inspeccion!, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "roof", id = solicitud.Id });
    }

    private async Task SaveRoofFilesAsync(
        SolicitudInspeccionRoof solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "inspecciones-roof", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > RoofMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!RoofAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/inspecciones-roof/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosInspeccionRoof.Add(new ArchivoInspeccionRoof
            {
                SolicitudInspeccionRoofId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetElectricalFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static string BuildDefaultRoofFocusAreas(SolicitudInspeccionRoof solicitud)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(solicitud.UbicacionProblema))
        {
            parts.Add(solicitud.UbicacionProblema);
        }

        parts.AddRange(
            (solicitud.TiposProblema ?? solicitud.TipoProblema ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return string.Join("|", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static (DateTime Date, TimeSpan Time) ComputeRoofAppointment(SolicitudInspeccionRoof solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Emergency" => 1,
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(11, 0, 0));
    }

    private static void AssignRoofAppointment(SolicitudInspeccionRoof solicitud)
    {
        var (date, time) = ComputeRoofAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureRoofAppointmentSavedAsync(SolicitudInspeccionRoof solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignRoofAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveRoofInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsRoofFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionRoof?> GetActiveRoofSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionRoof
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionRoof> GetOrCreateRoofSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionRoof? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionRoof
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveRoofSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionRoof
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionRoof.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudInspeccionRoof?> LoadRoofSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesInspeccionRoof
            .Include(s => s.Inspeccion)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsRoofFlow(solicitud.Inspeccion.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertRoofHistorialAsync(
        SolicitudInspeccionRoof solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionRoof"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionRoof",
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
