using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class InspeccionesController : Controller
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxFileSize = 10_000_000;

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public InspeccionesController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> PurchaseDetails(int id)
    {
        var inspeccion = await LoadActiveInspeccionAsync(id);
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
        var existing = await GetActiveSolicitudAsync(userId, id);

        var model = new PurchaseDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            SubtituloInspeccion = inspeccion.Subtitulo,
            DireccionPropiedad = existing?.DireccionPropiedad
                                 ?? propiedad?.Direccion
                                 ?? string.Empty,
            BajoContrato = existing?.BajoContrato ?? true,
            FechaCierreEstimada = existing?.FechaCierreEstimada ?? DateTime.Today.AddDays(21),
            TieneReporteExistente = existing?.TieneReporteExistente ?? false,
            RolComprador = existing?.RolComprador ?? "Buyer",
            ObjetivoPrincipal = existing?.ObjetivoPrincipal ?? "UnderstandRepairRisks"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurchaseDetails(PurchaseDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveInspeccionAsync(model.InspeccionId);
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
            TempData["InspectionSaved"] = "You can complete your inspection details anytime.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = inspeccion.Nombre;
            model.SubtituloInspeccion = inspeccion.Subtitulo;
            return View(model);
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.BajoContrato = model.BajoContrato;
            solicitud.FechaCierreEstimada = model.FechaCierreEstimada?.Date;
            solicitud.TieneReporteExistente = model.TieneReporteExistente;
            solicitud.RolComprador = model.RolComprador;
            solicitud.ObjetivoPrincipal = model.ObjetivoPrincipal;
            solicitud.Estado = model.TieneReporteExistente ? "ReportPending" : "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(UploadReport), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please run Scripts/CreateSolicitudesInspeccion.sql on the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            model.SubtituloInspeccion = inspeccion.Subtitulo;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> UploadReport(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccion
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsPurchaseFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        var model = new UploadReportViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            NotasRevision = solicitud.NotasRevision,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingReportFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadReport(UploadReportViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccion
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsPurchaseFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        if (string.Equals(action, "skip", StringComparison.OrdinalIgnoreCase))
        {
            solicitud.NotasRevision = model.NotasRevision?.Trim();
            solicitud.Estado = "ReportPending";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["InspectionSaved"] = "You can upload your report later from the Schedule tab.";
            return RedirectToAction("Index", "Home");
        }

        solicitud.NotasRevision = model.NotasRevision?.Trim();

        var uploadedCount = 0;
        if (files != null && files.Count > 0)
        {
            var uploadFolder = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "inspecciones",
                userId,
                solicitud.Id.ToString());
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (file.Length > MaxFileSize)
                {
                    ModelState.AddModelError("", $"File {file.FileName} exceeds the 10 MB limit.");
                    continue;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", $"File {file.FileName} is not allowed. Use PDF, JPG, or PNG.");
                    continue;
                }

                var storedName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileName(file.FileName)}";
                var physicalPath = Path.Combine(uploadFolder, storedName);
                await using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/inspecciones/{userId}/{solicitud.Id}/{storedName}";
                _db.ArchivosReporteInspeccion.Add(new ArchivoReporteInspeccion
                {
                    SolicitudInspeccionId = solicitud.Id,
                    NombreArchivo = file.FileName,
                    RutaArchivo = relativePath,
                    TipoArchivo = ext.TrimStart('.'),
                    TamanioBytes = file.Length,
                    FechaSubida = DateTime.Now
                });
                uploadedCount++;
            }
        }

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = solicitud.Inspeccion.Nombre;
            model.DireccionPropiedad = solicitud.DireccionPropiedad;
            model.ArchivosExistentes = solicitud.Archivos
                .Select(a => new ExistingReportFileViewModel
                {
                    Id = a.Id,
                    NombreArchivo = a.NombreArchivo,
                    RutaArchivo = a.RutaArchivo
                })
                .ToList();
            return View(model);
        }

        solicitud.Estado = "Confirmed";
        AssignPurchaseAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertHistorialAsync(solicitud, solicitud.Inspeccion, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "purchase", id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> BookingConfirmed(string type, int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        if (string.Equals(type, "purchase", StringComparison.OrdinalIgnoreCase))
        {
            var solicitud = await _db.SolicitudesInspeccion
                .Include(s => s.Inspeccion)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (solicitud?.Inspeccion == null
                || !InspeccionFlowRules.SupportsPurchaseFlow(solicitud.Inspeccion.Nombre))
            {
                return NotFound();
            }

            await EnsurePurchaseAppointmentSavedAsync(solicitud);

            var model = new BookingConfirmedViewModel
            {
                SolicitudId = solicitud.Id,
                FlowType = "purchase",
                NombreServicio = solicitud.Inspeccion.Nombre,
                DireccionPropiedad = solicitud.DireccionPropiedad,
                FechaCita = solicitud.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(solicitud.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatPurchaseConcern(
                    solicitud.ObjetivoPrincipal,
                    solicitud.NotasRevision,
                    solicitud.RolComprador),
                ResumenEtiqueta = "Concern",
                Estado = "Confirmed"
            };

            return View(model);
        }

        if (string.Equals(type, "electrical", StringComparison.OrdinalIgnoreCase))
        {
            var solicitud = await _db.SolicitudesInspeccionElectrica
                .Include(s => s.Inspeccion)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (solicitud?.Inspeccion == null
                || !InspeccionFlowRules.SupportsElectricalFlow(solicitud.Inspeccion.Nombre))
            {
                return NotFound();
            }

            await EnsureElectricalAppointmentSavedAsync(solicitud);

            var model = new BookingConfirmedViewModel
            {
                SolicitudId = solicitud.Id,
                FlowType = "electrical",
                NombreServicio = solicitud.Inspeccion.Nombre,
                DireccionPropiedad = solicitud.DireccionPropiedad,
                FechaCita = solicitud.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(solicitud.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatElectricalConcern(
                    solicitud.PreocupacionPrincipal,
                    solicitud.MotivoRevision),
                ResumenEtiqueta = "Concern",
                Estado = "Confirmed"
            };

            return View(model);
        }

        if (string.Equals(type, "complete", StringComparison.OrdinalIgnoreCase))
        {
            var solicitud = await _db.SolicitudesInspeccionCompleta
                .Include(s => s.Inspeccion)
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (solicitud?.Inspeccion == null
                || !InspeccionFlowRules.SupportsCompleteHomeFlow(solicitud.Inspeccion.Nombre))
            {
                return NotFound();
            }

            await EnsureCompleteAppointmentSavedAsync(solicitud);

            var model = new BookingConfirmedViewModel
            {
                SolicitudId = solicitud.Id,
                FlowType = "complete",
                NombreServicio = solicitud.Inspeccion.Nombre,
                DireccionPropiedad = solicitud.DireccionPropiedad,
                FechaCita = solicitud.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(solicitud.HoraCitaProgramada!.Value),
                ResumenEtiqueta = "Focus areas",
                ResumenPreocupacion = InspeccionDisplayLabels.FormatAreasEnfoque(solicitud.AreasEnfoque),
                InfoMensaje = "Your provider will review your selected focus areas before arriving at your property.",
                Estado = "Confirmed"
            };

            return View(model);
        }

        return NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> HomeReviewDetails(int id)
    {
        var inspeccion = await LoadActiveCompleteInspeccionAsync(id);
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
        var existing = await GetActiveCompleteSolicitudAsync(userId, id);

        var model = new HomeReviewDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad
                                 ?? propiedad?.Direccion
                                 ?? string.Empty,
            MotivoInspeccion = existing?.MotivoInspeccion ?? "BuyingHome",
            AreasEnfoque = existing?.AreasEnfoque ?? "Electrical|HVAC|GeneralStructure",
            TamanoPropiedad = existing?.TamanoPropiedad,
            EsUrgente = existing?.EsUrgente ?? "No"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HomeReviewDetails(HomeReviewDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveCompleteInspeccionAsync(model.InspeccionId);
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
            TempData["InspectionSaved"] = "You can complete your home review details anytime.";
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrWhiteSpace(model.AreasEnfoque)
            || model.AreasEnfoque.Split('|', StringSplitOptions.RemoveEmptyEntries).Length == 0)
        {
            ModelState.AddModelError(nameof(model.AreasEnfoque), "Select at least one focus area.");
        }

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateCompleteSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.MotivoInspeccion = model.MotivoInspeccion;
            solicitud.AreasEnfoque = model.AreasEnfoque.Trim();
            solicitud.TamanoPropiedad = model.TamanoPropiedad?.Trim();
            solicitud.EsUrgente = model.EsUrgente;
            solicitud.Estado = "Confirmed";
            AssignCompleteAppointment(solicitud);
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();

            await UpsertCompleteHistorialAsync(solicitud, inspeccion, "Confirmed");

            return RedirectToAction(nameof(BookingConfirmed), new { type = "complete", id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please run Scripts/CreateSolicitudesInspeccionCompleta.sql on the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> HomeReviewUpload(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionCompleta
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud == null)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(HomeReviewDetails), new { id = solicitud.InspeccionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> HomeReviewUpload(HomeReviewUploadViewModel model, string? action, List<IFormFile>? files)
    {
        return RedirectToAction(nameof(HomeReviewDetails), new { id = model.InspeccionId });
    }

    [HttpGet]
    public async Task<IActionResult> ElectricalDetails(int id)
    {
        var inspeccion = await LoadActiveElectricalInspeccionAsync(id);
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
        var existing = await GetActiveElectricalSolicitudAsync(userId, id);

        var model = new ElectricalDetailsViewModel
        {
            InspeccionId = inspeccion.Id,
            SolicitudId = existing?.Id,
            NombreInspeccion = inspeccion.Nombre,
            DireccionPropiedad = existing?.DireccionPropiedad
                                 ?? propiedad?.Direccion
                                 ?? string.Empty,
            MotivoRevision = existing?.MotivoRevision ?? "BuyingHome",
            PreocupacionPrincipal = existing?.PreocupacionPrincipal ?? "GeneralReview",
            OcurreAhora = existing?.OcurreAhora ?? "No",
            Urgencia = existing?.Urgencia ?? "Normal"
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ElectricalDetails(ElectricalDetailsViewModel model, string? action)
    {
        var inspeccion = await LoadActiveElectricalInspeccionAsync(model.InspeccionId);
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
            TempData["InspectionSaved"] = "You can complete your electrical inspection details anytime.";
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
            var solicitud = await GetOrCreateElectricalSolicitudAsync(userId, model.InspeccionId, model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.MotivoRevision = model.MotivoRevision;
            solicitud.PreocupacionPrincipal = model.PreocupacionPrincipal;
            solicitud.OcurreAhora = model.OcurreAhora;
            solicitud.Urgencia = model.Urgencia;
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(ElectricalUpload), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please run Scripts/CreateSolicitudesInspeccionElectrica.sql on the database and try again.");
            model.NombreInspeccion = inspeccion.Nombre;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ElectricalUpload(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionElectrica
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsElectricalFlow(solicitud.Inspeccion.Nombre))
        {
            return NotFound();
        }

        var model = new ElectricalUploadViewModel
        {
            SolicitudId = solicitud.Id,
            InspeccionId = solicitud.InspeccionId,
            NombreInspeccion = solicitud.Inspeccion.Nombre,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ComentariosProveedor = solicitud.ComentariosProveedor,
            ArchivosExistentes = solicitud.Archivos
                .OrderByDescending(a => a.FechaSubida)
                .Select(a => new ExistingElectricalFileViewModel
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

    private static readonly string[] ElectricalAllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".webm"];

    private const long ElectricalMaxFileSize = 25_000_000;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> ElectricalUpload(ElectricalUploadViewModel model, string? action, List<IFormFile>? files)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null)
        {
            return Challenge();
        }

        var solicitud = await _db.SolicitudesInspeccionElectrica
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .FirstOrDefaultAsync(s => s.Id == model.SolicitudId && s.UserId == userId);

        if (solicitud?.Inspeccion == null
            || !InspeccionFlowRules.SupportsElectricalFlow(solicitud.Inspeccion.Nombre))
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

        var uploadedCount = 0;
        if (files != null && files.Count > 0)
        {
            var uploadFolder = Path.Combine(
                _env.WebRootPath,
                "uploads",
                "inspecciones-electricas",
                userId,
                solicitud.Id.ToString());
            Directory.CreateDirectory(uploadFolder);

            foreach (var file in files.Where(f => f.Length > 0))
            {
                if (file.Length > ElectricalMaxFileSize)
                {
                    ModelState.AddModelError("", $"File {file.FileName} exceeds the 25 MB limit.");
                    continue;
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!ElectricalAllowedExtensions.Contains(ext))
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

                var relativePath = $"/uploads/inspecciones-electricas/{userId}/{solicitud.Id}/{storedName}";
                _db.ArchivosInspeccionElectrica.Add(new ArchivoInspeccionElectrica
                {
                    SolicitudInspeccionElectricaId = solicitud.Id,
                    NombreArchivo = file.FileName,
                    RutaArchivo = relativePath,
                    CategoriaArchivo = GetElectricalFileCategory(ext),
                    TipoArchivo = ext.TrimStart('.'),
                    TamanioBytes = file.Length,
                    FechaSubida = DateTime.Now
                });
                uploadedCount++;
            }
        }

        if (!ModelState.IsValid)
        {
            model.NombreInspeccion = solicitud.Inspeccion.Nombre;
            model.DireccionPropiedad = solicitud.DireccionPropiedad;
            model.ArchivosExistentes = solicitud.Archivos
                .Select(a => new ExistingElectricalFileViewModel
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
        AssignElectricalAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertElectricalHistorialAsync(solicitud, solicitud.Inspeccion, "Confirmed");

        return RedirectToAction(nameof(BookingConfirmed), new { type = "electrical", id = solicitud.Id });
    }

    private static string GetElectricalFileCategory(string ext)
    {
        return ext switch
        {
            ".pdf" => "report",
            ".mp4" or ".mov" or ".webm" => "video",
            _ => "photo"
        };
    }

    private static DateTime NextBusinessDay(DateTime date)
    {
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }

    private static (DateTime Date, TimeSpan Time) ComputePurchaseAppointment(SolicitudInspeccion solicitud)
    {
        var defaultTime = new TimeSpan(10, 0, 0);

        if (solicitud.FechaCierreEstimada.HasValue)
        {
            var candidate = solicitud.FechaCierreEstimada.Value.AddDays(-14).Date;
            if (candidate <= DateTime.Today)
            {
                candidate = NextBusinessDay(DateTime.Today.AddDays(7));
            }

            return (NextBusinessDay(candidate), defaultTime);
        }

        return (NextBusinessDay(DateTime.Today.AddDays(14)), defaultTime);
    }

    private static (DateTime Date, TimeSpan Time) ComputeElectricalAppointment(SolicitudInspeccionElectrica solicitud)
    {
        var days = solicitud.Urgencia switch
        {
            "Emergency" => 1,
            "Priority" => 3,
            _ => 7
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(10, 0, 0));
    }

    private static void AssignPurchaseAppointment(SolicitudInspeccion solicitud)
    {
        var (date, time) = ComputePurchaseAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private static void AssignElectricalAppointment(SolicitudInspeccionElectrica solicitud)
    {
        var (date, time) = ComputeElectricalAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsurePurchaseAppointmentSavedAsync(SolicitudInspeccion solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignPurchaseAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task EnsureElectricalAppointmentSavedAsync(SolicitudInspeccionElectrica solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignElectricalAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<string?> RequireUserIdAsync()
    {
        return _userManager.GetUserId(User);
    }

    private async Task<Inspeccion?> LoadActiveInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsPurchaseFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<Inspeccion?> LoadActiveElectricalInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsElectricalFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<Propiedad?> GetLatestPropertyAsync(string userId)
    {
        return await _db.Propiedades
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<int?> GetLatestPropertyIdAsync(string userId)
    {
        return await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccion?> GetActiveSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccion
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccion> GetOrCreateSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccion? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccion
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccion
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccion.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudInspeccionElectrica?> GetActiveElectricalSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionElectrica
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionElectrica> GetOrCreateElectricalSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionElectrica? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionElectrica
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveElectricalSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionElectrica
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionElectrica.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task UpsertHistorialAsync(
        SolicitudInspeccion solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "Inspeccion"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "Inspeccion",
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

    private async Task UpsertElectricalHistorialAsync(
        SolicitudInspeccionElectrica solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionElectrica"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionElectrica",
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

    private static (DateTime Date, TimeSpan Time) ComputeCompleteAppointment(SolicitudInspeccionCompleta solicitud)
    {
        var days = solicitud.EsUrgente switch
        {
            "Yes" => 3,
            "NotSure" => 5,
            _ => 10
        };

        return (NextBusinessDay(DateTime.Today.AddDays(days)), new TimeSpan(10, 0, 0));
    }

    private static void AssignCompleteAppointment(SolicitudInspeccionCompleta solicitud)
    {
        var (date, time) = ComputeCompleteAppointment(solicitud);
        solicitud.FechaCitaProgramada = date;
        solicitud.HoraCitaProgramada = time;
    }

    private async Task EnsureCompleteAppointmentSavedAsync(SolicitudInspeccionCompleta solicitud)
    {
        if (solicitud.FechaCitaProgramada.HasValue && solicitud.HoraCitaProgramada.HasValue)
        {
            return;
        }

        AssignCompleteAppointment(solicitud);
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    private async Task<Inspeccion?> LoadActiveCompleteInspeccionAsync(int id)
    {
        var inspeccion = await _db.Inspecciones
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

        if (inspeccion == null || !InspeccionFlowRules.SupportsCompleteHomeFlow(inspeccion.Nombre))
        {
            return null;
        }

        return inspeccion;
    }

    private async Task<SolicitudInspeccionCompleta?> GetActiveCompleteSolicitudAsync(string userId, int inspeccionId)
    {
        return await _db.SolicitudesInspeccionCompleta
            .Where(s => s.UserId == userId
                        && s.InspeccionId == inspeccionId
                        && s.Estado != "Completed"
                        && s.Estado != "Skipped"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudInspeccionCompleta> GetOrCreateCompleteSolicitudAsync(
        string userId,
        int inspeccionId,
        int? solicitudId)
    {
        SolicitudInspeccionCompleta? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesInspeccionCompleta
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveCompleteSolicitudAsync(userId, inspeccionId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudInspeccionCompleta
        {
            UserId = userId,
            InspeccionId = inspeccionId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesInspeccionCompleta.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task UpsertCompleteHistorialAsync(
        SolicitudInspeccionCompleta solicitud,
        Inspeccion inspeccion,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "InspeccionCompleta"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "InspeccionCompleta",
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
