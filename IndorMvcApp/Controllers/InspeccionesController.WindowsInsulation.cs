using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> WindowsInsulationDetails(int id)
    {
        var inspeccion = await LoadActiveWindowsInsulationInspeccionAsync(id);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveWindowsInsulationSolicitudAsync(userId, id);

        var model = new WindowsInsulationDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposProblema = existing?.TiposProblema ?? existing?.TipoProblema ?? "DraftAir",
            TipoProblema = existing?.TipoProblema ?? "DraftAir",
            AreasAtencion = existing?.AreasAtencion ?? existing?.UbicacionProblema ?? "LivingRoom",
            UbicacionProblema = existing?.UbicacionProblema ?? "LivingRoom",
            Urgencia = existing?.Urgencia ?? "Normal",
            DanoHumedadVisible = existing?.DanoHumedadVisible ?? "No"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WindowsInsulationDetails(WindowsInsulationDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveWindowsInsulationInspeccionAsync(model.InspeccionId);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your windows and insulation details anytime.";
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
            var solicitud = await GetOrCreateWindowsInsulationSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

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
            solicitud.DanoHumedadVisible = model.DanoHumedadVisible;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(WindowsInsulationUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(WindowsInsulationPropertyContext), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the windows and insulation inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> WindowsInsulationPropertyContext(int id)
    {
        var solicitud = await LoadWindowsInsulationSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new WindowsInsulationPropertyContextViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            MotivosRevision = solicitud.MotivosRevision ?? solicitud.MotivoRevision ?? "HighUtilityBill",
            MotivoRevision = solicitud.MotivoRevision ?? "HighUtilityBill",
            TipoPropiedad = solicitud.TipoPropiedad ?? "SingleFamily",
            NumeroPisos = solicitud.NumeroPisos ?? "TwoStory",
            AreasEnfoque = solicitud.AreasEnfoque ?? BuildDefaultWindowsInsulationFocusAreas(solicitud),
            AccesoPreferido = solicitud.AccesoPreferido ?? "SomeoneHome",
            TipoVentana = solicitud.TipoVentana ?? "DoublePane",
            AccesoAtticCrawlSpace = solicitud.AccesoAtticCrawlSpace ?? "Yes"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WindowsInsulationPropertyContext(WindowsInsulationPropertyContextViewModel model, string? action)
    {
        var solicitud = await LoadWindowsInsulationSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(WindowsInsulationDetails), new { id = solicitud.InspeccionId });
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
        solicitud.TipoVentana = model.TipoVentana;
        solicitud.AccesoAtticCrawlSpace = model.AccesoAtticCrawlSpace;
        solicitud.AreasEnfoque = string.IsNullOrWhiteSpace(model.AreasEnfoque)
            ? BuildDefaultWindowsInsulationFocusAreas(solicitud)
            : model.AreasEnfoque.Trim();
        solicitud.Estado = "PropertyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(WindowsInsulationUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> WindowsInsulationUpload(int id)
    {
        var solicitud = await LoadWindowsInsulationSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new WindowsInsulationUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingWindowsInsulationFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] WindowsInsulationAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long WindowsInsulationMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> WindowsInsulationUpload(
        WindowsInsulationUploadViewModel model,
        string? action,
        List<IFormFile>? files,
        List<IFormFile>? utilityBillFiles)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesInspeccionWindowsInsulation
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsWindowsInsulationFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveWindowsInsulationFilesAsync(solicitud, userId, files, "photo");
            await SaveWindowsInsulationFilesAsync(solicitud, userId, utilityBillFiles, "utility");

            if (!ModelState.IsValid)
            {
                model.NombreInspeccion = solicitud.Inspeccion.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingWindowsInsulationFileViewModel
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

        return RedirectToAction(nameof(WindowsInsulationReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> WindowsInsulationReview(int id)
    {
        var solicitud = await LoadWindowsInsulationSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var inspeccion = solicitud.Inspeccion!;
        var archivos = solicitud.Archivos.OrderByDescending(a => a.FechaSubida).ToList();

        var model = new WindowsInsulationReviewViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo ?? "On-site evaluation by a certified INDOR inspector.",
            FrecuenciaInspeccion = inspeccion.Frecuencia,
            Precio = inspeccion.Valor ?? 0,
            Moneda = inspeccion.Moneda,
            PrecioPrefijo = inspeccion.PrecioPrefijo ?? "From",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            PreocupacionPrincipal = InspeccionDisplayLabels.FormatWindowsInsulationMainConcern(
                solicitud.TiposProblema, solicitud.TipoProblema, solicitud.DanoHumedadVisible),
            PropiedadResumen = InspeccionDisplayLabels.FormatWindowsInsulationPropertySummary(
                solicitud.TipoPropiedad, solicitud.NumeroPisos, solicitud.TipoVentana),
            AreasEnfoqueResumen = InspeccionDisplayLabels.FormatWindowsInsulationFocusAreas(
                solicitud.AreasEnfoque, solicitud.AreasAtencion),
            AccesoResumen = InspeccionDisplayLabels.AccesoPreferidoStructural(solicitud.AccesoPreferido),
            ArchivosResumen = InspeccionDisplayLabels.FormatWindowsInsulationFilesSummary(
                archivos.Select(a => (a.CategoriaArchivo, (string?)a.NombreArchivo))),
            Archivos = archivos
                .Select(a => new ExistingWindowsInsulationFileViewModel
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
    public async Task<IActionResult> WindowsInsulationReview(WindowsInsulationReviewViewModel model, string? action)
    {
        var solicitud = await LoadWindowsInsulationSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(WindowsInsulationUpload), new { id = solicitud.Id });
        }

        solicitud.Estado = "Confirmed";
        AssignWindowsInsulationAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertWindowsInsulationHistorialAsync(solicitud, solicitud.Inspeccion!, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "windowsinsulation", id = solicitud.Id });
    }

    private async Task SaveWindowsInsulationFilesAsync(
        SolicitudInspeccionWindowsInsulation solicitud,
        string userId,
        List<IFormFile>? files,
        string categoryOverride)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "inspecciones-windows-insulation", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > WindowsInsulationMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!WindowsInsulationAllowedExtensions.Contains(ext))
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

            var category = categoryOverride switch
            {
                "utility" => "utility",
                _ => GetElectricalFileCategory(ext)
            };

            var relativePath = $"/uploads/inspecciones-windows-insulation/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosInspeccionWindowsInsulation.Add(new ArchivoInspeccionWindowsInsulation
            {
                SolicitudInspeccionWindowsInsulationId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = category,
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static string BuildDefaultWindowsInsulationFocusAreas(SolicitudInspeccionWindowsInsulation solicitud)
    {
        var parts = ParsePipeValues(solicitud.AreasAtencion);
        if (parts.Count == 0 && !string.IsNullOrWhiteSpace(solicitud.UbicacionProblema))
        {
            parts.Add(solicitud.UbicacionProblema);
        }

        parts.AddRange(
            (solicitud.TiposProblema ?? solicitud.TipoProblema ?? string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return string.Join("|", parts.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static List<string> ParsePipeValues(string? pipeSeparated)
    {
        if (string.IsNullOrWhiteSpace(pipeSeparated))
        {
            return new List<string>();
        }

        return pipeSeparated
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static (DateTime Date, TimeSpan Time) ComputeWindowsInsulationAppointment(SolicitudInspeccionWindowsInsulation solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(11, 0, 0));
    }

    private static void AssignWindowsInsulationAppointment(SolicitudInspeccionWindowsInsulation solicitud)
    {
        var (date, time) = ComputeWindowsInsulationAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureWindowsInsulationAppointmentSavedAsync(SolicitudInspeccionWindowsInsulation solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignWindowsInsulationAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveWindowsInsulationInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsWindowsInsulationFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionWindowsInsulation?> GetActiveWindowsInsulationSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionWindowsInsulation
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionWindowsInsulation> GetOrCreateWindowsInsulationSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionWindowsInsulation? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionWindowsInsulation
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveWindowsInsulationSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionWindowsInsulation
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionWindowsInsulation.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudInspeccionWindowsInsulation?> LoadWindowsInsulationSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesInspeccionWindowsInsulation
            .Include(s => s.Inspeccion)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsWindowsInsulationFlow(solicitud.Inspeccion.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertWindowsInsulationHistorialAsync(
        SolicitudInspeccionWindowsInsulation solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionWindowsInsulation"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionWindowsInsulation",
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
