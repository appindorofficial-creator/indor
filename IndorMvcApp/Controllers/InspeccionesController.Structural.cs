using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> StructuralDetails(int id)
    {
        var inspeccion = await LoadActiveStructuralInspeccionAsync(id);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveStructuralSolicitudAsync(userId, id);

        var model = new StructuralDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposPreocupacion = existing?.TiposPreocupacion ?? existing?.TipoPreocupacion ?? "FoundationCrack",
            TipoPreocupacion = existing?.TipoPreocupacion ?? "FoundationCrack",
            AreaPreocupacion = existing?.AreaPreocupacion ?? "Foundation",
            Urgencia = existing?.Urgencia ?? "Normal",
            DanoVisible = existing?.DanoVisible ?? "Yes"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StructuralDetails(StructuralDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveStructuralInspeccionAsync(model.InspeccionId);
        if (inspeccion == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "You can complete your structural details anytime.";
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
            var solicitud = await GetOrCreateStructuralSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            var tipos = string.IsNullOrWhiteSpace(model.TiposPreocupacion)
                ? model.TipoPreocupacion
                : model.TiposPreocupacion.Trim();
            var firstTipo = tipos.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()
                              ?? model.TipoPreocupacion;

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TiposPreocupacion = tipos;
            solicitud.TipoPreocupacion = firstTipo;
            solicitud.AreaPreocupacion = model.AreaPreocupacion;
            solicitud.Urgencia = model.Urgencia;
            solicitud.DanoVisible = model.DanoVisible;
            solicitud.Estado = "ConcernCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            if (string.Equals(action, "upload", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(StructuralUpload), new { id = solicitud.Id });
            }

            return RedirectToAction(nameof(StructuralSigns), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the structural inspection tables exist in the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> StructuralSigns(int id)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new StructuralSignsViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            SignosVisibles = solicitud.SignosVisibles,
            SeveridadApariencia = solicitud.SeveridadApariencia ?? "Moderate"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StructuralSigns(StructuralSignsViewModel model, string? action)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(StructuralDetails), new { id = solicitud.InspeccionId });
        }

        solicitud.SignosVisibles = model.SignosVisibles?.Trim();
        solicitud.SeveridadApariencia = model.SeveridadApariencia;
        solicitud.Estado = "SignsCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(StructuralMoreDetails), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> StructuralMoreDetails(int id)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new StructuralMoreDetailsViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            UbicacionEspecifica = solicitud.UbicacionEspecifica,
            CuandoNotadoTexto = solicitud.CuandoNotadoTexto ?? solicitud.CuandoNotado,
            DuracionProblema = solicitud.DuracionProblema ?? "OneToThreeMonths",
            Severidad = solicitud.Severidad ?? solicitud.SeveridadApariencia ?? "Moderate",
            ReparacionesPrevias = solicitud.ReparacionesPrevias ?? "No",
            CondicionesInseguras = solicitud.CondicionesInseguras,
            MejorHorarioVisita = solicitud.MejorHorarioVisita ?? "FirstAvailable"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StructuralMoreDetails(StructuralMoreDetailsViewModel model, string? action)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(StructuralSigns), new { id = solicitud.Id });
        }

        if (string.Equals(action, "save", StringComparison.OrdinalIgnoreCase))
        {
            TempData["InspectionSaved"] = "Your structural details were saved. Continue anytime from Schedule.";
            return RedirectToAction("Index", "Home");
        }

        solicitud.UbicacionEspecifica = model.UbicacionEspecifica?.Trim();
        solicitud.CuandoNotadoTexto = model.CuandoNotadoTexto?.Trim();
        solicitud.CuandoNotado = model.DuracionProblema;
        solicitud.DuracionProblema = model.DuracionProblema;
        solicitud.Severidad = model.Severidad;
        solicitud.ReparacionesPrevias = model.ReparacionesPrevias;
        solicitud.CondicionesInseguras = model.CondicionesInseguras?.Trim();
        solicitud.MejorHorarioVisita = model.MejorHorarioVisita;
        solicitud.Estado = "MoreDetailsCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(StructuralPropertyContext), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> StructuralPropertyContext(int id)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(new StructuralPropertyContextViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            MotivoRevision = solicitud.MotivoRevision is "BuyingHome" ? "BeforePurchase" : solicitud.MotivoRevision,
            TipoPropiedad = solicitud.TipoPropiedad ?? "SingleFamily",
            EdadPropiedad = solicitud.EdadPropiedad,
            TipoFundacion = solicitud.TipoFundacion ?? "CrawlSpace",
            TieneReporte = solicitud.TieneReporte ?? "No",
            CambiosRecientes = solicitud.CambiosRecientes ?? solicitud.NotasOpcionales,
            AccesoPreferido = solicitud.AccesoPreferido ?? "SomeoneHome",
            AreasEnfoque = solicitud.AreasEnfoque ?? "Foundation|Walls|Floors|CrawlSpace"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StructuralPropertyContext(StructuralPropertyContextViewModel model, string? action)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(StructuralMoreDetails), new { id = solicitud.Id });
        }

        solicitud.MotivoRevision = model.MotivoRevision;
        solicitud.TipoPropiedad = model.TipoPropiedad;
        solicitud.EdadPropiedad = model.EdadPropiedad;
        solicitud.TipoFundacion = model.TipoFundacion;
        solicitud.TieneReporte = model.TieneReporte;
        solicitud.CambiosRecientes = model.CambiosRecientes?.Trim();
        solicitud.AccesoPreferido = model.AccesoPreferido;
        solicitud.AreasEnfoque = model.AreasEnfoque?.Trim();
        solicitud.Estado = "PropertyCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(StructuralUpload), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> StructuralUpload(int id)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var model = new StructuralUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion!.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            TipoPropiedad = solicitud.TipoPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingStructuralFileViewModel
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

    private static readonly string[] StructuralAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long StructuralMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> StructuralUpload(StructuralUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var solicitud = await _db.SolicitudesInspeccionStructural
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null || !InspeccionFlowRules.SupportsStructuralFlow(solicitud.Inspeccion.Nombre))
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
        await SaveStructuralFilesAsync(solicitud, userId, files);

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = solicitud.Inspeccion.Nombre;
            model.DireccionPropiedad = solicitud.DireccionPropiedad;
            model.TipoPropiedad = solicitud.TipoPropiedad;
            model.ArchivosExistentes = solicitud.Archivos.Select(a => new ExistingStructuralFileViewModel
            {
                Id = a.Id,
                NombreArchivo = a.NombreArchivo,
                RutaArchivo = a.RutaArchivo,
                CategoriaArchivo = a.CategoriaArchivo
            }).ToList();
            return View(model);
        }

        solicitud.Estado = "PhotosCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(StructuralReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> StructuralReview(int id)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(id, includeArchivos: true);
        if (solicitud == null) return NotFound();

        var inspeccion = solicitud.Inspeccion!;
        var model = new StructuralReviewViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo ?? "Before purchase or remodel",
            Precio = inspeccion.Valor ?? 0,
            Moneda = inspeccion.Moneda,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            Preocupaciones = InspeccionDisplayLabels.FormatStructuralConcernsList(
                solicitud.TiposPreocupacion, solicitud.TipoPreocupacion),
            CuandoNotadoResumen = solicitud.CuandoNotadoTexto
                ?? InspeccionDisplayLabels.DuracionProblemaStructural(solicitud.DuracionProblema),
            SeveridadResumen = InspeccionDisplayLabels.SeveridadStructural(
                solicitud.Severidad ?? solicitud.SeveridadApariencia),
            UrgenciaResumen = InspeccionDisplayLabels.UrgenciaStructural(solicitud.Urgencia),
            TipoFundacionResumen = InspeccionDisplayLabels.TipoFundacionStructural(solicitud.TipoFundacion),
            ReparacionesPreviasResumen = InspeccionDisplayLabels.ReparacionesPreviasStructural(solicitud.ReparacionesPrevias),
            MotivoResumen = InspeccionDisplayLabels.MotivoRevisionStructural(solicitud.MotivoRevision),
            TipoPropiedadResumen = InspeccionDisplayLabels.TipoPropiedadStructural(solicitud.TipoPropiedad),
            ReporteResumen = string.Equals(solicitud.TieneReporte, "Yes", StringComparison.OrdinalIgnoreCase)
                ? "Yes, uploaded" : "No",
            AccesoResumen = InspeccionDisplayLabels.AccesoPreferidoStructural(solicitud.AccesoPreferido),
            Archivos = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingStructuralFileViewModel
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
    public async Task<IActionResult> StructuralReview(StructuralReviewViewModel model)
    {
        var solicitud = await LoadStructuralSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        solicitud.Estado = "Confirmed";
        AssignStructuralAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertStructuralHistorialAsync(solicitud, solicitud.Inspeccion!, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "structural", id = solicitud.Id });
    }

    private async Task SaveStructuralFilesAsync(
        SolicitudInspeccionStructural solicitud,
        string userId,
        List<IFormFile>? files)
    {
        if (files == null || files.Count == 0) return;

        var uploadFolder = Path.Combine(
            _env.WebRootPath, "uploads", "inspecciones-structural", userId, solicitud.Id.ToString());
        Directory.CreateDirectory(uploadFolder);

        foreach (var file in files.Where(f => f.Length > 0))
        {
            if (file.Length > StructuralMaxFileSize)
            {
                ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                continue;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!StructuralAllowedExtensions.Contains(ext))
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

            var relativePath = $"/uploads/inspecciones-structural/{userId}/{solicitud.Id}/{storedName}";
            _db.ArchivosInspeccionStructural.Add(new ArchivoInspeccionStructural
            {
                SolicitudInspeccionStructuralId = solicitud.Id,
                NombreArchivo = file.FileName,
                RutaArchivo = relativePath,
                CategoriaArchivo = GetElectricalFileCategory(ext),
                TipoArchivo = ext.TrimStart('.'),
                TamanioBytes = file.Length,
                FechaSubida = DateTime.Now
            });
        }
    }

    private static (DateTime Date, TimeSpan Time) ComputeStructuralAppointment(SolicitudInspeccionStructural solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Emergency" => 2,
            "Priority" => 5,
            _ => 10
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(11, 0, 0));
    }

    private static void AssignStructuralAppointment(SolicitudInspeccionStructural solicitud)
    {
        var (date, time) = ComputeStructuralAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureStructuralAppointmentSavedAsync(SolicitudInspeccionStructural solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue) return;

        AssignStructuralAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<SolicitudInspeccionStructural?> LoadStructuralSolicitudForUserAsync(
        int solicitudId,
        bool includeArchivos = false)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var query = _db.SolicitudesInspeccionStructural
            .Include(s => s.Inspeccion)
            .AsQueryable();

        if (includeArchivos)
        {
            query = query.Include(s => s.Archivos);
        }

        var solicitud = await query.FirstOrDefaultAsync(s => s.Id == solicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsStructuralFlow(solicitud.Inspeccion.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task<Inspeccion?> LoadActiveStructuralInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsStructuralFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionStructural?> GetActiveStructuralSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionStructural
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionStructural> GetOrCreateStructuralSolicitudAsync(
        string userId, int inspeccionId, int? solicitudId)
    {
        SolicitudInspeccionStructural? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionStructural
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveStructuralSolicitudAsync(userId, inspeccionId);

        if (solicitud != null) return solicitud;

        solicitud = new SolicitudInspeccionStructural
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            MotivoRevision = "BeforePurchase",
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionStructural.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task UpsertStructuralHistorialAsync(
        SolicitudInspeccionStructural solicitud, Inspeccion inspeccion, string estado)
    {
        var historial = await _db.HistorialServicios.FirstOrDefaultAsync(h =>
            h.UserId == solicitud.UserId && h.Tipo == "InspeccionStructural" && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionStructural",
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
