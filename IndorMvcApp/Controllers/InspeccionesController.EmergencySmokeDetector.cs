using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public partial class InspeccionesController
{
    [HttpGet]
    public async Task<IActionResult> EmergencySmokeDetectorDetails(int id)
    {
        var servicio = await LoadActiveSmokeDetectorEmergencyServiceAsync(id);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var propiedad = await GetLatestPropertyAsync(userId);
        var existing = await GetActiveEmergencySmokeDetectorSolicitudAsync(userId, id);

        return View(new EmergencySmokeDetectorDetailsViewModel
        {
            ServicioEmergenciaId = servicio.Id,
            SolicitudId = existing?.Id,
            NombreServicio = servicio.Nombre,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = existing?.DireccionPropiedad ?? propiedad?.Direccion ?? string.Empty,
            TiposProblema = existing?.TiposProblema ?? "SmokeDetectorBeeping",
            UbicacionesDetectores = existing?.UbicacionesDetectores ?? "Hallway",
            SituacionActual = existing?.SituacionActual ?? "IntermittentChirp",
            PuedePermanecerAdentro = existing?.PuedePermanecerAdentro ?? "Yes",
            Urgencia = existing?.Urgencia ?? "Emergency"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencySmokeDetectorDetails(
        EmergencySmokeDetectorDetailsViewModel model,
        string? action)
    {
        var servicio = await LoadActiveSmokeDetectorEmergencyServiceAsync(model.ServicioEmergenciaId);
        if (servicio == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        if (string.IsNullOrWhiteSpace(model.TiposProblema))
        {
            ModelState.AddModelError(nameof(model.TiposProblema), "Select at least one issue.");
        }

        if (string.IsNullOrWhiteSpace(model.UbicacionesDetectores))
        {
            ModelState.AddModelError(nameof(model.UbicacionesDetectores), "Select at least one location.");
        }

        if (!ModelState.IsValid)
        {
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }

        try
        {
            var propiedadId = await GetLatestPropertyIdAsync(userId);
            var solicitud = await GetOrCreateEmergencySmokeDetectorSolicitudAsync(
                userId,
                model.ServicioEmergenciaId,
                model.SolicitudId);

            solicitud.PropiedadId = propiedadId;
            solicitud.DireccionPropiedad = model.DireccionPropiedad.Trim();
            solicitud.TiposProblema = model.TiposProblema.Trim();
            solicitud.UbicacionesDetectores = model.UbicacionesDetectores.Trim();
            solicitud.SituacionActual = model.SituacionActual;
            solicitud.PuedePermanecerAdentro = model.PuedePermanecerAdentro;
            solicitud.Urgencia = ResolveSmokeDetectorUrgency(
                model,
                string.Equals(action, "immediate", StringComparison.OrdinalIgnoreCase));
            solicitud.Estado = "DetailsCompleted";
            solicitud.FechaActualizacion = DateTime.Now;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(EmergencySmokeDetectorYourInfo), new { id = solicitud.Id });
        }
        catch (Exception)
        {
            ModelState.AddModelError("",
                "Could not save your request. Please ensure the emergency smoke detector tables exist in the database and try again.");
            model.NombreServicio = servicio.Nombre;
            model.TituloServicio = servicio.TituloEmergencia;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EmergencySmokeDetectorYourInfo(int id)
    {
        var solicitud = await LoadEmergencySmokeDetectorSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        var userId = await RequireUserIdAsync();
        if (userId == null) return Challenge();

        var user = await _userManager.FindByIdAsync(userId);
        var telefono = solicitud.TelefonoContacto;
        if (string.IsNullOrWhiteSpace(telefono))
        {
            telefono = user?.Telefono ?? string.Empty;
        }

        return View(new EmergencySmokeDetectorYourInfoViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            NombreServicio = solicitud.ServicioEmergencia!.Nombre,
            TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatTiposProblemaSmokeDetector(solicitud.TiposProblema),
            AccesoPropiedad = solicitud.AccesoPropiedad ?? "AdultHomeNow",
            TelefonoContacto = telefono
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencySmokeDetectorYourInfo(
        EmergencySmokeDetectorYourInfoViewModel model,
        string? action)
    {
        var solicitud = await LoadEmergencySmokeDetectorSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencySmokeDetectorDetails), new { id = solicitud.ServicioEmergenciaId });
        }

        if (!ModelState.IsValid)
        {
            model.NombreServicio = solicitud.ServicioEmergencia!.Nombre;
            model.TituloServicio = solicitud.ServicioEmergencia.TituloEmergencia;
            model.ProblemaResumen = EmergencyDisplayLabels.FormatTiposProblemaSmokeDetector(solicitud.TiposProblema);
            return View(model);
        }

        solicitud.AccesoPropiedad = model.AccesoPropiedad;
        solicitud.TelefonoContacto = model.TelefonoContacto.Trim();
        solicitud.Estado = "YourInfoCompleted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(EmergencySmokeDetectorReview), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencySmokeDetectorReview(int id)
    {
        var solicitud = await LoadEmergencySmokeDetectorSolicitudForUserAsync(id);
        if (solicitud == null) return NotFound();

        return View(BuildEmergencySmokeDetectorReviewViewModel(solicitud));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmergencySmokeDetectorReview(
        EmergencySmokeDetectorReviewViewModel model,
        string? action)
    {
        var solicitud = await LoadEmergencySmokeDetectorSolicitudForUserAsync(model.SolicitudId);
        if (solicitud == null) return NotFound();

        if (string.Equals(action, "back", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(EmergencySmokeDetectorYourInfo), new { id = solicitud.Id });
        }

        solicitud.NotaCorta = model.NotaCorta?.Trim();
        solicitud.Estado = "Submitted";
        solicitud.FechaActualizacion = DateTime.Now;
        await _db.SaveChangesAsync();

        await UpsertEmergencySmokeDetectorHistorialAsync(
            solicitud,
            solicitud.ServicioEmergencia!,
            "Submitted");

        return RedirectToAction(nameof(EmergencySmokeDetectorSubmitted), new { id = solicitud.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EmergencySmokeDetectorSubmitted(int id)
    {
        var solicitud = await LoadEmergencySmokeDetectorSolicitudForUserAsync(id);
        if (solicitud == null || solicitud.Estado != "Submitted") return NotFound();

        var servicio = solicitud.ServicioEmergencia!;
        var minutos = servicio.TiempoLlegadaMinutos > 0 ? servicio.TiempoLlegadaMinutos : 45;

        return View(new EmergencySmokeDetectorSubmittedViewModel
        {
            SolicitudId = solicitud.Id,
            TituloServicio = servicio.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            PreocupacionResumen = EmergencyDisplayLabels.PreocupacionSmokeDetector(
                solicitud.TiposProblema,
                solicitud.SituacionActual),
            UbicacionesResumen = EmergencyDisplayLabels.FormatUbicacionesSmokeDetector(
                solicitud.UbicacionesDetectores),
            UrgenciaResumen = EmergencyDisplayLabels.UrgenciaEmergencia(solicitud.Urgencia),
            EstadoResumen = EmergencyDisplayLabels.EstadoSmokeDetectorConfirmado(solicitud.Estado),
            TiempoCallbackRango = EmergencyDisplayLabels.TiempoCallbackRangoSmokeDetector(minutos)
        });
    }

    private static EmergencySmokeDetectorReviewViewModel BuildEmergencySmokeDetectorReviewViewModel(
        SolicitudEmergenciaSmokeDetector solicitud)
    {
        return new EmergencySmokeDetectorReviewViewModel
        {
            SolicitudId = solicitud.Id,
            ServicioEmergenciaId = solicitud.ServicioEmergenciaId,
            TituloServicio = solicitud.ServicioEmergencia!.TituloEmergencia,
            DireccionPropiedad = solicitud.DireccionPropiedad,
            ProblemaResumen = EmergencyDisplayLabels.FormatTiposProblemaSmokeDetector(solicitud.TiposProblema),
            UbicacionesResumen = EmergencyDisplayLabels.FormatUbicacionesSmokeDetector(
                solicitud.UbicacionesDetectores),
            SeguridadResumen = EmergencyDisplayLabels.PuedePermanecerAdentroSmokeDetector(
                solicitud.PuedePermanecerAdentro),
            OlorGasResumen = EmergencyDisplayLabels.OlorGasSmokeDetector(
                solicitud.TiposProblema,
                solicitud.SituacionActual),
            AccesoResumen = EmergencyDisplayLabels.AccesoPropiedadSmokeDetector(solicitud.AccesoPropiedad),
            NotaCorta = solicitud.NotaCorta
        };
    }

    private static string ResolveSmokeDetectorUrgency(
        EmergencySmokeDetectorDetailsViewModel model,
        bool immediate)
    {
        if (immediate)
        {
            return "Emergency";
        }

        if (string.Equals(model.PuedePermanecerAdentro, "No", StringComparison.OrdinalIgnoreCase)
            || string.Equals(model.SituacionActual, "GasSmell", StringComparison.OrdinalIgnoreCase)
            || string.Equals(model.SituacionActual, "AlarmSounding", StringComparison.OrdinalIgnoreCase)
            || model.TiposProblema.Contains("CoDetectorAlert", StringComparison.OrdinalIgnoreCase)
            || model.TiposProblema.Contains("SmellOfGas", StringComparison.OrdinalIgnoreCase))
        {
            return "Emergency";
        }

        if (string.Equals(model.PuedePermanecerAdentro, "NotSure", StringComparison.OrdinalIgnoreCase)
            || string.Equals(model.SituacionActual, "NotSure", StringComparison.OrdinalIgnoreCase))
        {
            return "Priority";
        }

        return "Normal";
    }

    private async Task<ServicioEmergencia?> LoadActiveSmokeDetectorEmergencyServiceAsync(int id)
    {
        var servicio = await _db.ServiciosEmergencia
            .FirstOrDefaultAsync(s => s.Id == id && s.Activo);

        if (servicio == null || !EmergencyFlowRules.SupportsSmokeDetectorEmergencyFlow(servicio.Nombre))
        {
            return null;
        }

        return servicio;
    }

    private async Task<SolicitudEmergenciaSmokeDetector?> GetActiveEmergencySmokeDetectorSolicitudAsync(
        string userId,
        int servicioEmergenciaId)
    {
        return await _db.SolicitudesEmergenciaSmokeDetector
            .Where(s => s.UserId == userId
                        && s.ServicioEmergenciaId == servicioEmergenciaId
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private async Task<SolicitudEmergenciaSmokeDetector> GetOrCreateEmergencySmokeDetectorSolicitudAsync(
        string userId,
        int servicioEmergenciaId,
        int? solicitudId)
    {
        SolicitudEmergenciaSmokeDetector? solicitud = null;

        if (solicitudId.HasValue)
        {
            solicitud = await _db.SolicitudesEmergenciaSmokeDetector
                .FirstOrDefaultAsync(s => s.Id == solicitudId.Value && s.UserId == userId);
        }

        solicitud ??= await GetActiveEmergencySmokeDetectorSolicitudAsync(userId, servicioEmergenciaId);

        if (solicitud != null)
        {
            return solicitud;
        }

        solicitud = new SolicitudEmergenciaSmokeDetector
        {
            UserId = userId,
            ServicioEmergenciaId = servicioEmergenciaId,
            Estado = "InProgress",
            FechaCreacion = DateTime.Now
        };
        _db.SolicitudesEmergenciaSmokeDetector.Add(solicitud);
        await _db.SaveChangesAsync();
        return solicitud;
    }

    private async Task<SolicitudEmergenciaSmokeDetector?> LoadEmergencySmokeDetectorSolicitudForUserAsync(int id)
    {
        var userId = await RequireUserIdAsync();
        if (userId == null) return null;

        var solicitud = await _db.SolicitudesEmergenciaSmokeDetector
            .Include(s => s.ServicioEmergencia)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (solicitud?.ServicioEmergencia == null
            || !EmergencyFlowRules.SupportsSmokeDetectorEmergencyFlow(solicitud.ServicioEmergencia.Nombre))
        {
            return null;
        }

        return solicitud;
    }

    private async Task UpsertEmergencySmokeDetectorHistorialAsync(
        SolicitudEmergenciaSmokeDetector solicitud,
        ServicioEmergencia servicio,
        string estado)
    {
        var historial = await _db.HistorialServicios
            .FirstOrDefaultAsync(h =>
                h.UserId == solicitud.UserId
                && h.Tipo == "EmergenciaSmokeDetector"
                && h.ItemId == solicitud.Id);

        if (historial == null)
        {
            historial = new HistorialServicio
            {
                UserId = solicitud.UserId,
                Tipo = "EmergenciaSmokeDetector",
                ItemId = solicitud.Id,
                NombreItem = servicio.TituloEmergencia,
                Fecha = DateTime.Now
            };
            _db.HistorialServicios.Add(historial);
        }

        historial.Estado = estado;
        historial.Fecha = DateTime.Now;
        await _db.SaveChangesAsync();
    }
}
