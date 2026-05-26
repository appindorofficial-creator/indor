using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> PlumbingDetails(int id)
    {
        var inspeccion = await LoadActivePlumbingInspeccionAsync(id);
        if (inspeccion == null)
        {
            return NotFound();
        }

        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActivePlumbingSolicitudAsync(userId, id);

        var model = new PlumbingDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoProblema = existing?.TipoProblema ?? "KitchenIssue",
            UbicacionProblema = existing?.UbicacionProblema ?? "Kitchen",
            Urgencia = existing?.Urgencia ?? "Normal",
            FugaAguaAhora = existing?.FugaAguaAhora ?? "No"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlumbingDetails(PlumbingDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActivePlumbingInspeccionAsync(model.InspeccionId);
        if (inspeccion == null)
        {
            return NotFound();
        }

        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your plumbing details anytime.";
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
            var solicitud = await GetOrCreatePlumbingSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoProblema = model.TipoProblema;
            solicitud.UbicacionProblema = model.UbicacionProblema;
            solicitud.Urgencia = model.Urgencia;
            solicitud.FugaAguaAhora = model.FugaAguaAhora;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(PlumbingUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(PlumbingProblemDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the plumbing inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> PlumbingProblemDetails(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionPlomeria
            .Include(s => s.Inspeccion)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsPlumbingFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        var model = new PlumbingProblemDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            TipoProblema = solicitud.TipoProblema,
            UbicacionProblema = solicitud.UbicacionProblema,
            ResumenProblema = InspeccionDisplayLabels.FormatPlumbingSummary(
                solicitud.TipoProblema, solicitud.UbicacionProblema),
            SituacionesActuales = solicitud.SituacionesActuales ?? "LeakUnderSink",
            CuandoEmpezo = solicitud.CuandoEmpezo ?? "ThisWeek",
            AguaCerrada = solicitud.AguaCerrada ?? "NotNeeded",
            DescripcionProblema = solicitud.DescripcionProblema,
            NotasAdicionales = solicitud.NotasAdicionales
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlumbingProblemDetails(PlumbingProblemDetailsViewModel model, string? action)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionPlomeria
            .Include(s => s.Inspeccion)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsPlumbingFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can add problem details later from the Schedule tab.";
            return RedirectToAction("Index", "Home");
        }

        solicitud.SituacionesActuales = model.SituacionesActuales?.Trim();
        solicitud.CuandoEmpezo = model.CuandoEmpezo;
        solicitud.AguaCerrada = model.AguaCerrada;
        solicitud.DescripcionProblema = model.DescripcionProblema?.Trim();
        solicitud.NotasAdicionales = model.NotasAdicionales?.Trim();
        solicitud.Estado = "ProblemCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(PlumbingUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> PlumbingUpload(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionPlomeria
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsPlumbingFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        var model = new PlumbingUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingPlumbingFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        };

        return View(model);
    }

    private static readonly string[] PlumbingAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long PlumbingMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> PlumbingUpload(PlumbingUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionPlomeria
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsPlumbingFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();
            solicitud.Estado = "FilesPending";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["InspectionSaved"] = "You can upload photos later from the Schedule tab.";
            return RedirectToAction("Index", "Home");
        }

        solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();

        if (files != null && files.Count > 0)
        {
            var uploadFolder = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "inspecciones-plomeria",
                userId,
                solicitud.Id.ToString());
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (file.Length > PlumbingMaxFileSize)
                {
                    ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                    continue;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!PlumbingAllowedExtensions.Contains(ext))
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

                var relativePath = $"/uploads/inspecciones-plomeria/{userId}/{solicitud.Id}/{storedName}";
                _db.ArchivosInspeccionPlomeria.Add(new ArchivoInspeccionPlomeria
                {
                    SolicitudInspeccionPlomeriaId = solicitud.Id,
                    NombreArchivo = file.FileName,
                    RutaArchivo = relativePath,
                    CategoriaArchivo = GetElectricalFileCategory(ext),
                    TipoArchivo = ext.TrimStart('.'),
                    TamanioBytes = file.Length,
                    FechaSubida = DateTime.Now
                });
            }
        }

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = solicitud.Inspeccion.Nombre;
            model.DireccionPropiedad = solicitud.DireccionPropiedad;
            model.ArchivosExistentes = solicitud.Archivos
                .Select(a => new ExistingPlumbingFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList();
            return View(model);
        }

        solicitud.Estado = "Confirmed";
        AssignPlumbingAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertPlumbingHistorialAsync(solicitud, solicitud.Inspeccion, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "plumbing", id = solicitud.Id });
    }

    private static (DateTime Date, TimeSpan Time) ComputePlumbingAppointment(SolicitudInspeccionPlomeria solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Emergency" => 1,
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(10, 30, 0));
    }

    private static void AssignPlumbingAppointment(SolicitudInspeccionPlomeria solicitud)
    {
        var (date, time) = ComputePlumbingAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsurePlumbingAppointmentSavedAsync(SolicitudInspeccionPlomeria solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignPlumbingAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActivePlumbingInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsPlumbingFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionPlomeria?> GetActivePlumbingSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionPlomeria
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionPlomeria> GetOrCreatePlumbingSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionPlomeria? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionPlomeria
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActivePlumbingSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionPlomeria
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionPlomeria.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task UpsertPlumbingHistorialAsync(
        SolicitudInspeccionPlomeria solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionPlomeria"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionPlomeria",
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
