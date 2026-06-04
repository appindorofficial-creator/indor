using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<HomeController> logger,
        IConfiguration configuration)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
        _configuration = configuration;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var usuario = await _userManager.GetUserAsync(User);
        ViewBag.UsuarioActual = usuario;

        var propiedades = await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        ViewBag.PrimaryPropiedadId = propiedades.FirstOrDefault()?.Id;

        var microservicios = await _db.Microservicios
            .Where(m => m.Activo)
            .OrderBy(m => m.Id)
            .ToListAsync();
        ViewBag.Microservicios = microservicios;

        var servicios = await _db.Servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync();
        ViewBag.Servicios = servicios;

        ViewBag.HomeCarePriorityIds = await _db.HomeCarePriorities
            .Where(p => p.Activo)
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Nombre.Trim(), p => p.Id, StringComparer.OrdinalIgnoreCase);

        var inspecciones = await _db.Inspecciones
            .Where(i => i.Activo)
            .OrderBy(i => i.Orden)
            .ThenBy(i => i.Id)
            .ToListAsync();
        ViewBag.Inspecciones = inspecciones;

        ViewBag.ServiciosEmergencia = await _db.ServiciosEmergencia
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync();

        var movingConfig = await _db.MovingSetupConfig.FirstOrDefaultAsync(c => c.Activo);
        var movingServicios = await _db.MovingSetupServicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ToListAsync();
        var movingEnlaces = await _db.MovingSetupEnlacesRapidos
            .Where(e => e.Activo)
            .OrderBy(e => e.Orden)
            .ToListAsync();
        ViewBag.MovingSetupSection = MovingSetupDisplayService.Build(
            movingConfig,
            movingServicios,
            movingEnlaces,
            Url);

        var prioritiesConfig = await _db.HomeCarePrioritiesConfig.FirstOrDefaultAsync(c => c.Activo);
        var prioritiesItems = await _db.HomeCarePriorities
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ThenBy(p => p.Id)
            .ToListAsync();
        ViewBag.HomeCarePrioritiesSection = HomeCarePrioritiesDisplayService.Build(
            prioritiesConfig,
            prioritiesItems,
            propiedades.FirstOrDefault()?.Id,
            Url);

        // === Datos para sección "More" ===
        ViewBag.Planes = await _db.PlanesMembresia
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        var membresiaActual = await _db.MembresiasUsuario
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Activa)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync();
        ViewBag.MembresiaActual = membresiaActual;

        var propIds = propiedades.Select(p => p.Id).ToList();
        var docCount = propIds.Count == 0
            ? 0
            : await _db.PropiedadDocumentos.CountAsync(d => propIds.Contains(d.PropiedadId));
        var serviceCount = await _db.HistorialServicios.CountAsync(h => h.UserId == userId)
            + await _db.ProgramacionesMicroservicio.CountAsync(p => p.UserId == userId);
        ViewBag.MoreProfile = ProfileDisplayService.Build(
            usuario, membresiaActual, propiedades.Count, docCount, serviceCount);

        ViewBag.MetodosPago = await _db.MetodosPago
            .Where(m => m.UserId == userId && m.Activo)
            .OrderByDescending(m => m.EsPredeterminado)
            .ThenByDescending(m => m.FechaCreacion)
            .ToListAsync();

        ViewBag.Pagos = await _db.Pagos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

        ViewBag.PlanesInternet = await _db.PlanesInternet
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync();

        ViewBag.Historial = await _db.HistorialServicios
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();

        ViewBag.MensajesSoporte = await _db.MensajesSoporte
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Fecha)
            .ToListAsync();

        ViewBag.ProgramacionesMicroservicio = await _db.ProgramacionesMicroservicio
            .Include(p => p.Microservicio)
            .Include(p => p.Propiedad)
            .Where(p => p.UserId == userId && p.Estado == "Scheduled")
            .OrderBy(p => p.FechaProgramada)
            .ToListAsync();

        ViewBag.SolicitudesInspeccion = await _db.SolicitudesInspeccion
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.PrePurchaseHomeInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionElectrica = await _db.SolicitudesInspeccionElectrica
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.ElectricalInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionCompleta = await _db.SolicitudesInspeccionCompleta
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.CompleteHomeInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionPlomeria = await _db.SolicitudesInspeccionPlomeria
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.PlumbingInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionHvac = await _db.SolicitudesInspeccionHvac
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.HvacInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionStructural = await _db.SolicitudesInspeccionStructural
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.StructuralInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionRoof = await _db.SolicitudesInspeccionRoof
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.RoofInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionMoldMoisture = await _db.SolicitudesInspeccionMoldMoisture
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.MoldMoistureInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionWindowsInsulation = await _db.SolicitudesInspeccionWindowsInsulation
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.WindowsInsulationInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionHomeSafety = await _db.SolicitudesInspeccionHomeSafety
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.HomeSafetyInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesInspeccionInvestor = await _db.SolicitudesInspeccionInvestor
            .Include(s => s.Inspeccion)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.Inspeccion != null
                        && s.Inspeccion.Nombre == InspeccionFlowRules.InvestorInspectionName
                        && s.Estado != "Skipped"
                        && s.Estado != "Completed"
                        && s.Estado != "Confirmed")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaPlomeria = await _db.SolicitudesEmergenciaPlomeria
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.PlumbingEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaPlomeriaEnviadas = await _db.SolicitudesEmergenciaPlomeria
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.PlumbingEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaHvac = await _db.SolicitudesEmergenciaHvac
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.HvacEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaHvacEnviadas = await _db.SolicitudesEmergenciaHvac
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.HvacEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaWaterHeater = await _db.SolicitudesEmergenciaWaterHeater
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.WaterHeaterEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaWaterHeaterEnviadas = await _db.SolicitudesEmergenciaWaterHeater
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.WaterHeaterEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaFlood = await _db.SolicitudesEmergenciaFlood
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.FloodEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaFloodEnviadas = await _db.SolicitudesEmergenciaFlood
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.FloodEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaElectrical = await _db.SolicitudesEmergenciaElectrical
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.ElectricalEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaElectricalEnviadas = await _db.SolicitudesEmergenciaElectrical
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.ElectricalEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaTreeDamage = await _db.SolicitudesEmergenciaTreeDamage
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.TreeDamageEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaTreeDamageEnviadas = await _db.SolicitudesEmergenciaTreeDamage
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.TreeDamageEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaRoofLeak = await _db.SolicitudesEmergenciaRoofLeak
            .Include(s => s.ServicioEmergencia)
            .Include(s => s.Archivos)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.RoofLeakEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaRoofLeakEnviadas = await _db.SolicitudesEmergenciaRoofLeak
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.RoofLeakEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaSmokeDetector = await _db.SolicitudesEmergenciaSmokeDetector
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.SmokeDetectorEmergencyName
                        && s.Estado != "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        ViewBag.SolicitudesEmergenciaSmokeDetectorEnviadas = await _db.SolicitudesEmergenciaSmokeDetector
            .Include(s => s.ServicioEmergencia)
            .Where(s => s.UserId == userId
                        && s.ServicioEmergencia != null
                        && s.ServicioEmergencia.Nombre == EmergencyFlowRules.SmokeDetectorEmergencyName
                        && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .Take(5)
            .ToListAsync();

        var purchaseConfirmed = await _db.SolicitudesInspeccion
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var electricalConfirmed = await _db.SolicitudesInspeccionElectrica
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var completeConfirmed = await _db.SolicitudesInspeccionCompleta
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var plumbingConfirmed = await _db.SolicitudesInspeccionPlomeria
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var hvacConfirmed = await _db.SolicitudesInspeccionHvac
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var structuralConfirmed = await _db.SolicitudesInspeccionStructural
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var roofConfirmed = await _db.SolicitudesInspeccionRoof
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var moldMoistureConfirmed = await _db.SolicitudesInspeccionMoldMoisture
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var windowsInsulationConfirmed = await _db.SolicitudesInspeccionWindowsInsulation
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var homeSafetyConfirmed = await _db.SolicitudesInspeccionHomeSafety
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        var investorConfirmed = await _db.SolicitudesInspeccionInvestor
            .Include(s => s.Inspeccion)
            .Where(s => s.UserId == userId
                        && s.Estado == "Confirmed"
                        && s.FechaCitaProgramada != null
                        && s.HoraCitaProgramada != null)
            .ToListAsync();

        ViewBag.InspeccionesConfirmadas = purchaseConfirmed
            .Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "purchase",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Home inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatPurchaseConcern(
                    s.ObjetivoPrincipal, s.NotasRevision, s.RolComprador)
            })
            .Concat(electricalConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "electrical",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Electrical inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatElectricalConcern(
                    s.PreocupacionPrincipal, s.MotivoRevision)
            }))
            .Concat(completeConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "complete",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Complete home inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatAreasEnfoque(s.AreasEnfoque)
            }))
            .Concat(plumbingConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "plumbing",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Plumbing inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatPlumbingConcern(
                    s.TipoProblema, s.UbicacionProblema, s.SituacionesActuales)
            }))
            .Concat(hvacConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "hvac",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "HVAC inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatHvacConcern(
                    s.TipoProblema, s.ParteAtencion)
            }))
            .Concat(structuralConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "structural",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Structural inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatStructuralConcern(
                    s.TipoPreocupacion, s.AreaPreocupacion, s.TiposPreocupacion)
            }))
            .Concat(roofConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "roof",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Roof inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatRoofConcern(
                    s.TipoProblema, s.UbicacionProblema, s.TiposProblema)
            }))
            .Concat(moldMoistureConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "moldmoisture",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Mold and moisture inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatMoldMoistureConcern(
                    s.TipoProblema, s.UbicacionProblema, s.TiposProblema)
            }))
            .Concat(windowsInsulationConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "windowsinsulation",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Windows and insulation inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatWindowsInsulationConcern(
                    s.TiposProblema, s.TipoProblema)
            }))
            .Concat(homeSafetyConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "homesafety",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Home safety inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatHomeSafetyConcern(
                    s.TiposProblema, s.TipoProblema)
            }))
            .Concat(investorConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "investor",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Investor inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatInvestorGoal(
                    s.TipoInversion, s.EnfoquesInversion)
            }))
            .OrderBy(s => s.FechaCita)
            .ToList();

        HttpContext.Session.SetString(
            HouseFactPreviewContext.ReturnUrlSessionKey,
            Url.Action(nameof(Index), "Home") + "#section-myhome");

        var primaryPropiedad = propiedades.FirstOrDefault();
        var previewInfo = HouseFactPreviewContext.Load(HttpContext.Session);
        HouseFactProfileViewModel? houseFactProfile = null;
        var houseFactPreview = false;

        if (primaryPropiedad != null)
        {
            try
            {
                var primaryInfo = MyHomeDisplayService.DeserializeProperty(primaryPropiedad);
                var dashboardData = await HomeDashboardDataService.LoadAsync(_db, primaryPropiedad.Id);

                var notificationCount = dashboardData.Mantenimiento.Count(m =>
                    m.DueDate.HasValue
                    && m.Status != "Completed"
                    && (m.DueDate.Value.Date - DateTime.Today).Days <= 30);

                ViewBag.HomeDashboard = HomeDashboardDisplayService.Build(
                    usuario,
                    primaryPropiedad,
                    primaryInfo,
                    dashboardData.HvacRecord,
                    dashboardData.WaterHeaterRecord,
                    dashboardData.Mantenimiento,
                    dashboardData.Documentos,
                    dashboardData.Historial,
                    notificationCount,
                    Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build home dashboard for propiedad {PropiedadId}", primaryPropiedad.Id);
                var primaryInfo = MyHomeDisplayService.DeserializeProperty(primaryPropiedad);
                ViewBag.HomeDashboard = HomeDashboardDisplayService.BuildBasic(usuario, primaryPropiedad, primaryInfo, Url);
            }
        }
        else
        {
            ViewBag.HomeDashboard = HomeDashboardDisplayService.BuildEmpty(usuario);
        }

        if (primaryPropiedad != null && !string.IsNullOrWhiteSpace(primaryPropiedad.AttomRawJson))
        {
            var info = MyHomeDisplayService.DeserializeProperty(primaryPropiedad);
            houseFactProfile = HouseFactDisplayService.BuildProfile(
                primaryPropiedad.AttomRawJson,
                primaryPropiedad.AttomSyncStatus ?? info?.DataSource,
                info?.FormattedAddress ?? primaryPropiedad.Direccion);
        }
        else if (previewInfo != null)
        {
            houseFactProfile = HouseFactDisplayService.BuildProfile(
                previewInfo.AttomRawJson,
                previewInfo.DataSource,
                previewInfo.FormattedAddress);
            houseFactPreview = true;
        }

        ViewBag.HouseFactProfile = houseFactProfile;
        ViewBag.HouseFactPreviewMode = houseFactPreview;
        ViewBag.HouseFactPropiedadId = primaryPropiedad?.Id;

        return View(propiedades);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var showDetails = _configuration.GetValue("Diagnostics:ShowDetailedErrors", true);
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var error = feature?.Error;

        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ShowDetails = showDetails,
            Path = feature?.Path ?? HttpContext.Request.Path
        };

        if (showDetails && error != null)
        {
            model.ExceptionType = error.GetType().FullName;
            model.ExceptionMessage = error.Message;
            model.InnerExceptionMessage = error.InnerException?.Message;
            model.StackTrace = error.StackTrace;
            _logger.LogError(error, "Unhandled exception at {Path}", model.Path);
        }

        return View(model);
    }
}
