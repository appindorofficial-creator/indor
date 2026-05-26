using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> InvestorDetails(int id)
    {
        var inspeccion = await LoadActiveInvestorInspeccionAsync(id);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveInvestorSolicitudAsync(userId, id);

        var model = new InvestorDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TipoInversion = existing?.TipoInversion ?? "Flip",
            EnfoquesInversion = existing?.EnfoquesInversion ?? "RehabBudget",
            Urgencia = existing?.Urgencia ?? "Normal"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InvestorDetails(InvestorDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveInvestorInspeccionAsync(model.InspeccionId);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your investor details anytime.";
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
            var solicitud = await GetOrCreateInvestorSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TipoInversion = model.TipoInversion;
            solicitud.EnfoquesInversion = string.IsNullOrWhiteSpace(model.EnfoquesInversion)
                ? "RehabBudget"
                : model.EnfoquesInversion.Trim();
            solicitud.Urgencia = model.Urgencia;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(InvestorUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(InvestorPropertyContext), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the investor inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> InvestorPropertyContext(int id)
    {
        var solicitud = await LoadInvestorSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new InvestorPropertyContextViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            TipoPropiedad = solicitud.TipoPropiedad ?? "SingleFamily",
            Ocupacion = solicitud.Ocupacion ?? "TenantOccupied",
            NivelRehab = solicitud.NivelRehab ?? "Light",
            AreasRevision = solicitud.AreasRevision ?? "Roof|Hvac|Plumbing|Electrical",
            AccesoPreferido = solicitud.AccesoPreferido ?? "SomeoneHome"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InvestorPropertyContext(InvestorPropertyContextViewModel model, string? action)
    {
        var solicitud = await LoadInvestorSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(InvestorDetails), new { id = solicitud.InspeccionId });
        }

        solicitud.TipoPropiedad = model.TipoPropiedad;
        solicitud.Ocupacion = model.Ocupacion;
        solicitud.NivelRehab = model.NivelRehab;
        solicitud.AreasRevision = string.IsNullOrWhiteSpace(model.AreasRevision)
            ? "Roof"
            : model.AreasRevision.Trim();
        solicitud.AccesoPreferido = model.AccesoPreferido;
        solicitud.Estado = "PropertyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(InvestorUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> InvestorUpload(int id)
    {
        var solicitud = await LoadInvestorSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        return View(new InvestorUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingInvestorFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo,
                    CategoriaArchivo = a.CategoriaArchivo
                })
                .ToList()
        });
    }

    private static readonly string[] InvestorAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long InvestorMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> InvestorUpload(InvestorUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesInspeccionInvestor
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsInvestorFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        solicitud.ComentariosProveedor = model.ComentariosProveedor?.Trim();

        if (!string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            await SaveInvestorFilesAsync(solicitud, userId, files);

            if (!ModelState.IsValid)
            {
                model.NombreInspeccion = solicitud.Inspeccion.Nombre;
                model.DireccionPropiedad = solicitud.DireccionPropiedad;
                model.ArchivosExistentes = solicitud.Archivos
                    .Select(a => new ExistingInvestorFileViewModel
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

        return RedirectToAction(nameof(InvestorReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> InvestorReview(int id)
    {
        var solicitud = await LoadInvestorSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var inspeccion = solicitud.Inspeccion!;
        var archivos = solicitud.Archivos.OrderByDescending(a => a.FechaSubida).ToList();

        var model = new InvestorReviewViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo ?? "Invest smart.",
            FrecuenciaInspeccion = inspeccion.Frecuencia,
            Precio = inspeccion.Valor ?? 0,
            Moneda = inspeccion.Moneda,
            PrecioPrefijo = inspeccion.PrecioPrefijo ?? "From",
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ObjetivoInversion = InspeccionDisplayLabels.FormatInvestorGoal(
                solicitud.TipoInversion, solicitud.EnfoquesInversion),
            PropiedadResumen = InspeccionDisplayLabels.FormatInvestorPropertySummary(
                solicitud.TipoPropiedad, solicitud.Ocupacion),
            NivelRehabResumen = InspeccionDisplayLabels.NivelRehabInvestor(solicitud.NivelRehab),
            AreasRevisionResumen = InspeccionDisplayLabels.FormatInvestorFocusAreas(solicitud.AreasRevision),
            AccesoResumen = InspeccionDisplayLabels.AccesoPreferidoStructural(solicitud.AccesoPreferido),
            ArchivosResumen = InspeccionDisplayLabels.FormatInvestorFilesSummary(
                archivos.Select(a => (a.CategoriaArchivo, (string?)a.NombreArchivo))),
            Archivos = archivos
                .Select(a => new ExistingInvestorFileViewModel
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
    public async Task<IActionResult> InvestorReview(InvestorReviewViewModel model, string? action)
    {
        var solicitud = await LoadInvestorSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(InvestorUpload), new { id = solicitud.Id });
        }

        solicitud.Estado = "Confirmed";
        AssignInvestorAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertInvestorHistorialAsync(solicitud, solicitud.Inspeccion!, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "investor", id = solicitud.Id });
    }

    private async Task SaveInvestorFilesAsync(
        SolicitudInspeccionInvestor solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "inspecciones-investor", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > InvestorMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!InvestorAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/inspecciones-investor/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosInspeccionInvestor.Add(new ArchivoInspeccionInvestor
            {
                SolicitudInspeccionInvestorId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetElectricalFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static (DateTime Date, TimeSpan Time) ComputeInvestorAppointment(SolicitudInspeccionInvestor solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(11, 0, 0));
    }

    private static void AssignInvestorAppointment(SolicitudInspeccionInvestor solicitud)
    {
        var (date, time) = ComputeInvestorAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureInvestorAppointmentSavedAsync(SolicitudInspeccionInvestor solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignInvestorAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveInvestorInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsInvestorFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionInvestor?> GetActiveInvestorSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionInvestor
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionInvestor> GetOrCreateInvestorSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionInvestor? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionInvestor
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveInvestorSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionInvestor
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionInvestor.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudInspeccionInvestor?> LoadInvestorSolicitudForUserAsync(
        int id,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesInspeccionInvestor
            .Include(s => s.Inspeccion)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsInvestorFlow(solicitud.Inspeccion.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertInvestorHistorialAsync(
        SolicitudInspeccionInvestor solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionInvestor"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionInvestor",
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
