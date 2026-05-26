using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> HvacDetails(int id)
    {
        var inspeccion = await LoadActiveHvacInspeccionAsync(id);
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
        var existing = await GetActiveHvacSolicitudAsync(userId, id);

        var model = new HvacDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoProblema = existing?.TipoProblema ?? "NotCooling",
            ParteAtencion = existing?.ParteAtencion ?? "WholeSystem",
            Urgencia = existing?.Urgencia ?? "Normal",
            SistemaFuncionando = existing?.SistemaFuncionando ?? "Yes"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HvacDetails(HvacDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveHvacInspeccionAsync(model.InspeccionId);
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
            TempData["InspectionSaved"] = "You can complete your HVAC details anytime.";
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
            var solicitud = await GetOrCreateHvacSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoProblema = model.TipoProblema;
            solicitud.ParteAtencion = model.ParteAtencion;
            solicitud.Urgencia = model.Urgencia;
            solicitud.SistemaFuncionando = model.SistemaFuncionando;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(HvacUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(HvacSystemDetails), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the HVAC inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> HvacSystemDetails(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionHvac
            .Include(s => s.Inspeccion)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsHvacFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        var model = new HvacSystemDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            TipoProblema = solicitud.TipoProblema,
            ParteAtencion = solicitud.ParteAtencion,
            ResumenProblema = InspeccionDisplayLabels.FormatHvacSummary(
                solicitud.TipoProblema, solicitud.ParteAtencion),
            ResumenArea = InspeccionDisplayLabels.ParteAtencionHvac(solicitud.ParteAtencion),
            TipoEquipo = solicitud.TipoEquipo ?? "CentralAC",
            CantidadSistemas = solicitud.CantidadSistemas ?? "One",
            ComponentesRevision = solicitud.ComponentesRevision ?? "OutdoorCondenser|IndoorCoil|Thermostat|Filters",
            EdadSistema = solicitud.EdadSistema ?? "NotSure",
            FiltroCambiado = solicitud.FiltroCambiado ?? "NotSure",
            TipoTermostato = solicitud.TipoTermostato ?? "NotSure",
            DescripcionProblema = solicitud.DescripcionProblema,
            NotasOpcionales = solicitud.NotasOpcionales
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HvacSystemDetails(HvacSystemDetailsViewModel model, string? action)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionHvac
            .Include(s => s.Inspeccion)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsHvacFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can add system details later from the Schedule tab.";
            return RedirectToAction("Index", "Home");
        }

        solicitud.TipoEquipo = model.TipoEquipo;
        solicitud.CantidadSistemas = model.CantidadSistemas;
        solicitud.ComponentesRevision = model.ComponentesRevision?.Trim();
        solicitud.EdadSistema = model.EdadSistema;
        solicitud.FiltroCambiado = model.FiltroCambiado;
        solicitud.TipoTermostato = model.TipoTermostato;
        solicitud.DescripcionProblema = model.DescripcionProblema?.Trim();
        solicitud.NotasOpcionales = model.NotasOpcionales?.Trim();
        solicitud.Estado = "SystemCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(HvacUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> HvacUpload(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionHvac
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsHvacFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        var model = new HvacUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingHvacFileViewModel
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

    private static readonly string[] HvacAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long HvacMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> HvacUpload(HvacUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionHvac
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsHvacFlow(solicitud.Inspeccion.Nombre))
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
                "inspecciones-hvac",
                userId,
                solicitud.Id.ToString());
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (file.Length > HvacMaxFileSize)
                {
                    ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                    continue;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!HvacAllowedExtensions.Contains(ext))
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

                var relativePath = $"/uploads/inspecciones-hvac/{userId}/{solicitud.Id}/{storedName}";
                _db.ArchivosInspeccionHvac.Add(new ArchivoInspeccionHvac
                {
                    SolicitudInspeccionHvacId = solicitud.Id,
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
                .Select(a => new ExistingHvacFileViewModel
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
        AssignHvacAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertHvacHistorialAsync(solicitud, solicitud.Inspeccion, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "hvac", id = solicitud.Id });
    }

    private static (DateTime Date, TimeSpan Time) ComputeHvacAppointment(SolicitudInspeccionHvac solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Emergency" => 1,
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(9, 30, 0));
    }

    private static void AssignHvacAppointment(SolicitudInspeccionHvac solicitud)
    {
        var (date, time) = ComputeHvacAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureHvacAppointmentSavedAsync(SolicitudInspeccionHvac solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignHvacAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveHvacInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsHvacFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionHvac?> GetActiveHvacSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionHvac
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionHvac> GetOrCreateHvacSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionHvac? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionHvac
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveHvacSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionHvac
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionHvac.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task UpsertHvacHistorialAsync(
        SolicitudInspeccionHvac solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionHvac"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionHvac",
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
