using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

/// <summary>
/// Loads the per-user Home/Index data using parallel DB contexts so dozens of
/// independent read queries do not run sequentially (each round-trip to Azure SQL
/// adds latency). Each query gets its own short-lived context because EF Core
/// DbContext is not thread-safe.
/// </summary>
public sealed class HomeIndexQueryService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<HomeIndexUserPageData> LoadAsync(string userId, IReadOnlyList<int> propIds, CancellationToken ct = default)
    {
        var membresiaTask = RunAsync(db => db.MembresiasUsuario
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Activa)
            .OrderByDescending(m => m.FechaInicio)
            .FirstOrDefaultAsync(ct));

        var metodosPagoTask = RunAsync(db => db.MetodosPago
            .Where(m => m.UserId == userId && m.Activo)
            .OrderByDescending(m => m.EsPredeterminado)
            .ThenByDescending(m => m.FechaCreacion)
            .ToListAsync(ct));

        var pagosTask = RunAsync(db => db.Pagos
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync(ct));

        var historialTask = RunAsync(db => db.HistorialServicios
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync(ct));

        var mensajesTask = RunAsync(db => db.MensajesSoporte
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Fecha)
            .ToListAsync(ct));

        var programacionesTask = RunAsync(db => db.ProgramacionesMicroservicio
            .Include(p => p.Microservicio)
            .Include(p => p.Propiedad)
            .Where(p => p.UserId == userId && p.Estado == "Scheduled")
            .OrderBy(p => p.FechaProgramada)
            .ToListAsync(ct));

        var docCountTask = propIds.Count == 0
            ? Task.FromResult(0)
            : RunAsync(db => db.PropiedadDocumentos.CountAsync(d => propIds.Contains(d.PropiedadId), ct));

        var historialCountTask = RunAsync(db => db.HistorialServicios.CountAsync(h => h.UserId == userId, ct));
        var programacionCountTask = RunAsync(db => db.ProgramacionesMicroservicio.CountAsync(p => p.UserId == userId, ct));

        var solicitudesInspeccionTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccion, userId, InspeccionFlowRules.PrePurchaseHomeInspectionName, ct));
        var solicitudesElectricaTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionElectrica, userId, InspeccionFlowRules.ElectricalInspectionName, ct));
        var solicitudesCompletaTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionCompleta, userId, InspeccionFlowRules.CompleteHomeInspectionName, ct));
        var solicitudesPlomeriaTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionPlomeria, userId, InspeccionFlowRules.PlumbingInspectionName, ct));
        var solicitudesHvacTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionHvac, userId, InspeccionFlowRules.HvacInspectionName, ct));
        var solicitudesStructuralTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionStructural, userId, InspeccionFlowRules.StructuralInspectionName, ct));
        var solicitudesRoofTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionRoof, userId, InspeccionFlowRules.RoofInspectionName, ct));
        var solicitudesMoldTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionMoldMoisture, userId, InspeccionFlowRules.MoldMoistureInspectionName, ct));
        var solicitudesWindowsTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionWindowsInsulation, userId, InspeccionFlowRules.WindowsInsulationInspectionName, ct));
        var solicitudesHomeSafetyTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionHomeSafety, userId, InspeccionFlowRules.HomeSafetyInspectionName, ct));
        var solicitudesInvestorTask = RunAsync(db => PendingInspection(db.SolicitudesInspeccionInvestor, userId, InspeccionFlowRules.InvestorInspectionName, ct));

        var emergenciaPlomeriaTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaPlomeria, userId, EmergencyFlowRules.PlumbingEmergencyName, ct));
        var emergenciaPlomeriaEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaPlomeria, userId, EmergencyFlowRules.PlumbingEmergencyName, ct));
        var emergenciaHvacTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaHvac, userId, EmergencyFlowRules.HvacEmergencyName, ct));
        var emergenciaHvacEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaHvac, userId, EmergencyFlowRules.HvacEmergencyName, ct));
        var emergenciaWaterHeaterTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaWaterHeater, userId, EmergencyFlowRules.WaterHeaterEmergencyName, ct));
        var emergenciaWaterHeaterEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaWaterHeater, userId, EmergencyFlowRules.WaterHeaterEmergencyName, ct));
        var emergenciaFloodTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaFlood, userId, EmergencyFlowRules.FloodEmergencyName, ct));
        var emergenciaFloodEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaFlood, userId, EmergencyFlowRules.FloodEmergencyName, ct));
        var emergenciaElectricalTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaElectrical, userId, EmergencyFlowRules.ElectricalEmergencyName, ct));
        var emergenciaElectricalEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaElectrical, userId, EmergencyFlowRules.ElectricalEmergencyName, ct));
        var emergenciaTreeTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaTreeDamage, userId, EmergencyFlowRules.TreeDamageEmergencyName, ct));
        var emergenciaTreeEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaTreeDamage, userId, EmergencyFlowRules.TreeDamageEmergencyName, ct));
        var emergenciaRoofLeakTask = RunAsync(db => PendingEmergency(db.SolicitudesEmergenciaRoofLeak, userId, EmergencyFlowRules.RoofLeakEmergencyName, ct));
        var emergenciaRoofLeakEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaRoofLeak, userId, EmergencyFlowRules.RoofLeakEmergencyName, ct));
        var emergenciaSmokeTask = RunAsync(db => PendingEmergencySmoke(db.SolicitudesEmergenciaSmokeDetector, userId, ct));
        var emergenciaSmokeEnviadasTask = RunAsync(db => SubmittedEmergency(db.SolicitudesEmergenciaSmokeDetector, userId, EmergencyFlowRules.SmokeDetectorEmergencyName, ct));

        var purchaseConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccion, userId, ct));
        var electricalConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionElectrica, userId, ct));
        var completeConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionCompleta, userId, ct));
        var plumbingConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionPlomeria, userId, ct));
        var hvacConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionHvac, userId, ct));
        var structuralConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionStructural, userId, ct));
        var roofConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionRoof, userId, ct));
        var moldConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionMoldMoisture, userId, ct));
        var windowsConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionWindowsInsulation, userId, ct));
        var homeSafetyConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionHomeSafety, userId, ct));
        var investorConfirmedTask = RunAsync(db => ConfirmedInspection(db.SolicitudesInspeccionInvestor, userId, ct));

        await Task.WhenAll(
            membresiaTask, metodosPagoTask, pagosTask, historialTask, mensajesTask, programacionesTask,
            docCountTask, historialCountTask, programacionCountTask,
            solicitudesInspeccionTask, solicitudesElectricaTask, solicitudesCompletaTask, solicitudesPlomeriaTask,
            solicitudesHvacTask, solicitudesStructuralTask, solicitudesRoofTask, solicitudesMoldTask,
            solicitudesWindowsTask, solicitudesHomeSafetyTask, solicitudesInvestorTask,
            emergenciaPlomeriaTask, emergenciaPlomeriaEnviadasTask, emergenciaHvacTask, emergenciaHvacEnviadasTask,
            emergenciaWaterHeaterTask, emergenciaWaterHeaterEnviadasTask, emergenciaFloodTask, emergenciaFloodEnviadasTask,
            emergenciaElectricalTask, emergenciaElectricalEnviadasTask, emergenciaTreeTask, emergenciaTreeEnviadasTask,
            emergenciaRoofLeakTask, emergenciaRoofLeakEnviadasTask, emergenciaSmokeTask, emergenciaSmokeEnviadasTask,
            purchaseConfirmedTask, electricalConfirmedTask, completeConfirmedTask, plumbingConfirmedTask,
            hvacConfirmedTask, structuralConfirmedTask, roofConfirmedTask, moldConfirmedTask,
            windowsConfirmedTask, homeSafetyConfirmedTask, investorConfirmedTask);

        return new HomeIndexUserPageData
        {
            MembresiaActual = await membresiaTask,
            MetodosPago = await metodosPagoTask,
            Pagos = await pagosTask,
            Historial = await historialTask,
            MensajesSoporte = await mensajesTask,
            ProgramacionesMicroservicio = await programacionesTask,
            DocCount = await docCountTask,
            ServiceCount = await historialCountTask + await programacionCountTask,
            SolicitudesInspeccion = await solicitudesInspeccionTask,
            SolicitudesInspeccionElectrica = await solicitudesElectricaTask,
            SolicitudesInspeccionCompleta = await solicitudesCompletaTask,
            SolicitudesInspeccionPlomeria = await solicitudesPlomeriaTask,
            SolicitudesInspeccionHvac = await solicitudesHvacTask,
            SolicitudesInspeccionStructural = await solicitudesStructuralTask,
            SolicitudesInspeccionRoof = await solicitudesRoofTask,
            SolicitudesInspeccionMoldMoisture = await solicitudesMoldTask,
            SolicitudesInspeccionWindowsInsulation = await solicitudesWindowsTask,
            SolicitudesInspeccionHomeSafety = await solicitudesHomeSafetyTask,
            SolicitudesInspeccionInvestor = await solicitudesInvestorTask,
            SolicitudesEmergenciaPlomeria = await emergenciaPlomeriaTask,
            SolicitudesEmergenciaPlomeriaEnviadas = await emergenciaPlomeriaEnviadasTask,
            SolicitudesEmergenciaHvac = await emergenciaHvacTask,
            SolicitudesEmergenciaHvacEnviadas = await emergenciaHvacEnviadasTask,
            SolicitudesEmergenciaWaterHeater = await emergenciaWaterHeaterTask,
            SolicitudesEmergenciaWaterHeaterEnviadas = await emergenciaWaterHeaterEnviadasTask,
            SolicitudesEmergenciaFlood = await emergenciaFloodTask,
            SolicitudesEmergenciaFloodEnviadas = await emergenciaFloodEnviadasTask,
            SolicitudesEmergenciaElectrical = await emergenciaElectricalTask,
            SolicitudesEmergenciaElectricalEnviadas = await emergenciaElectricalEnviadasTask,
            SolicitudesEmergenciaTreeDamage = await emergenciaTreeTask,
            SolicitudesEmergenciaTreeDamageEnviadas = await emergenciaTreeEnviadasTask,
            SolicitudesEmergenciaRoofLeak = await emergenciaRoofLeakTask,
            SolicitudesEmergenciaRoofLeakEnviadas = await emergenciaRoofLeakEnviadasTask,
            SolicitudesEmergenciaSmokeDetector = await emergenciaSmokeTask,
            SolicitudesEmergenciaSmokeDetectorEnviadas = await emergenciaSmokeEnviadasTask,
            PurchaseConfirmed = await purchaseConfirmedTask,
            ElectricalConfirmed = await electricalConfirmedTask,
            CompleteConfirmed = await completeConfirmedTask,
            PlumbingConfirmed = await plumbingConfirmedTask,
            HvacConfirmed = await hvacConfirmedTask,
            StructuralConfirmed = await structuralConfirmedTask,
            RoofConfirmed = await roofConfirmedTask,
            MoldMoistureConfirmed = await moldConfirmedTask,
            WindowsInsulationConfirmed = await windowsConfirmedTask,
            HomeSafetyConfirmed = await homeSafetyConfirmedTask,
            InvestorConfirmed = await investorConfirmedTask
        };
    }

    private async Task<T> RunAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return await query(db);
    }

    private static Task<List<T>> PendingInspection<T>(
        DbSet<T> set,
        string userId,
        string inspectionName,
        CancellationToken ct)
        where T : class
    {
        return set
            .Include("Inspeccion")
            .Include("Archivos")
            .Where(s => EF.Property<string>(s, "UserId") == userId
                        && EF.Property<Inspeccion?>(s, "Inspeccion") != null
                        && EF.Property<Inspeccion?>(s, "Inspeccion")!.Nombre == inspectionName
                        && EF.Property<string>(s, "Estado") != "Skipped"
                        && EF.Property<string>(s, "Estado") != "Completed"
                        && EF.Property<string>(s, "Estado") != "Confirmed")
            .OrderByDescending(s => EF.Property<DateTime?>(s, "FechaActualizacion") ?? EF.Property<DateTime>(s, "FechaCreacion"))
            .ToListAsync(ct);
    }

    private static Task<List<T>> PendingEmergency<T>(
        DbSet<T> set,
        string userId,
        string emergencyName,
        CancellationToken ct)
        where T : class
    {
        return set
            .Include("ServicioEmergencia")
            .Include("Archivos")
            .Where(s => EF.Property<string>(s, "UserId") == userId
                        && EF.Property<ServicioEmergencia?>(s, "ServicioEmergencia") != null
                        && EF.Property<ServicioEmergencia?>(s, "ServicioEmergencia")!.Nombre == emergencyName
                        && EF.Property<string>(s, "Estado") != "Submitted")
            .OrderByDescending(s => EF.Property<DateTime?>(s, "FechaActualizacion") ?? EF.Property<DateTime>(s, "FechaCreacion"))
            .ToListAsync(ct);
    }

    private static Task<List<T>> PendingEmergencySmoke<T>(
        DbSet<T> set,
        string userId,
        CancellationToken ct)
        where T : class
    {
        return set
            .Include("ServicioEmergencia")
            .Where(s => EF.Property<string>(s, "UserId") == userId
                        && EF.Property<ServicioEmergencia?>(s, "ServicioEmergencia") != null
                        && EF.Property<ServicioEmergencia?>(s, "ServicioEmergencia")!.Nombre == EmergencyFlowRules.SmokeDetectorEmergencyName
                        && EF.Property<string>(s, "Estado") != "Submitted")
            .OrderByDescending(s => EF.Property<DateTime?>(s, "FechaActualizacion") ?? EF.Property<DateTime>(s, "FechaCreacion"))
            .ToListAsync(ct);
    }

    private static Task<List<T>> SubmittedEmergency<T>(
        DbSet<T> set,
        string userId,
        string emergencyName,
        CancellationToken ct)
        where T : class
    {
        return set
            .Include("ServicioEmergencia")
            .Where(s => EF.Property<string>(s, "UserId") == userId
                        && EF.Property<ServicioEmergencia?>(s, "ServicioEmergencia") != null
                        && EF.Property<ServicioEmergencia?>(s, "ServicioEmergencia")!.Nombre == emergencyName
                        && EF.Property<string>(s, "Estado") == "Submitted")
            .OrderByDescending(s => EF.Property<DateTime?>(s, "FechaActualizacion") ?? EF.Property<DateTime>(s, "FechaCreacion"))
            .Take(5)
            .ToListAsync(ct);
    }

    private static Task<List<T>> ConfirmedInspection<T>(DbSet<T> set, string userId, CancellationToken ct)
        where T : class
    {
        return set
            .Include("Inspeccion")
            .Where(s => EF.Property<string>(s, "UserId") == userId
                        && EF.Property<string>(s, "Estado") == "Confirmed"
                        && EF.Property<DateTime?>(s, "FechaCitaProgramada") != null
                        && EF.Property<TimeSpan?>(s, "HoraCitaProgramada") != null)
            .ToListAsync(ct);
    }
}

public sealed class HomeIndexUserPageData
{
    public MembresiaUsuario? MembresiaActual { get; init; }
    public List<MetodoPago> MetodosPago { get; init; } = [];
    public List<Pago> Pagos { get; init; } = [];
    public List<HistorialServicio> Historial { get; init; } = [];
    public List<MensajeSoporte> MensajesSoporte { get; init; } = [];
    public List<ProgramacionMicroservicio> ProgramacionesMicroservicio { get; init; } = [];
    public int DocCount { get; init; }
    public int ServiceCount { get; init; }

    public List<SolicitudInspeccion> SolicitudesInspeccion { get; init; } = [];
    public List<SolicitudInspeccionElectrica> SolicitudesInspeccionElectrica { get; init; } = [];
    public List<SolicitudInspeccionCompleta> SolicitudesInspeccionCompleta { get; init; } = [];
    public List<SolicitudInspeccionPlomeria> SolicitudesInspeccionPlomeria { get; init; } = [];
    public List<SolicitudInspeccionHvac> SolicitudesInspeccionHvac { get; init; } = [];
    public List<SolicitudInspeccionStructural> SolicitudesInspeccionStructural { get; init; } = [];
    public List<SolicitudInspeccionRoof> SolicitudesInspeccionRoof { get; init; } = [];
    public List<SolicitudInspeccionMoldMoisture> SolicitudesInspeccionMoldMoisture { get; init; } = [];
    public List<SolicitudInspeccionWindowsInsulation> SolicitudesInspeccionWindowsInsulation { get; init; } = [];
    public List<SolicitudInspeccionHomeSafety> SolicitudesInspeccionHomeSafety { get; init; } = [];
    public List<SolicitudInspeccionInvestor> SolicitudesInspeccionInvestor { get; init; } = [];

    public List<SolicitudEmergenciaPlomeria> SolicitudesEmergenciaPlomeria { get; init; } = [];
    public List<SolicitudEmergenciaPlomeria> SolicitudesEmergenciaPlomeriaEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaHvac> SolicitudesEmergenciaHvac { get; init; } = [];
    public List<SolicitudEmergenciaHvac> SolicitudesEmergenciaHvacEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaWaterHeater> SolicitudesEmergenciaWaterHeater { get; init; } = [];
    public List<SolicitudEmergenciaWaterHeater> SolicitudesEmergenciaWaterHeaterEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaFlood> SolicitudesEmergenciaFlood { get; init; } = [];
    public List<SolicitudEmergenciaFlood> SolicitudesEmergenciaFloodEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaElectrical> SolicitudesEmergenciaElectrical { get; init; } = [];
    public List<SolicitudEmergenciaElectrical> SolicitudesEmergenciaElectricalEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaTreeDamage> SolicitudesEmergenciaTreeDamage { get; init; } = [];
    public List<SolicitudEmergenciaTreeDamage> SolicitudesEmergenciaTreeDamageEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaRoofLeak> SolicitudesEmergenciaRoofLeak { get; init; } = [];
    public List<SolicitudEmergenciaRoofLeak> SolicitudesEmergenciaRoofLeakEnviadas { get; init; } = [];
    public List<SolicitudEmergenciaSmokeDetector> SolicitudesEmergenciaSmokeDetector { get; init; } = [];
    public List<SolicitudEmergenciaSmokeDetector> SolicitudesEmergenciaSmokeDetectorEnviadas { get; init; } = [];

    public List<SolicitudInspeccion> PurchaseConfirmed { get; init; } = [];
    public List<SolicitudInspeccionElectrica> ElectricalConfirmed { get; init; } = [];
    public List<SolicitudInspeccionCompleta> CompleteConfirmed { get; init; } = [];
    public List<SolicitudInspeccionPlomeria> PlumbingConfirmed { get; init; } = [];
    public List<SolicitudInspeccionHvac> HvacConfirmed { get; init; } = [];
    public List<SolicitudInspeccionStructural> StructuralConfirmed { get; init; } = [];
    public List<SolicitudInspeccionRoof> RoofConfirmed { get; init; } = [];
    public List<SolicitudInspeccionMoldMoisture> MoldMoistureConfirmed { get; init; } = [];
    public List<SolicitudInspeccionWindowsInsulation> WindowsInsulationConfirmed { get; init; } = [];
    public List<SolicitudInspeccionHomeSafety> HomeSafetyConfirmed { get; init; } = [];
    public List<SolicitudInspeccionInvestor> InvestorConfirmed { get; init; } = [];
}
