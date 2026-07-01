using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class HomeIndexViewDataApplier
{
    public static void ApplyToViewBag(
        dynamic viewBag,
        HomeIndexUserPageData data,
        ApplicationUser? usuario,
        int propertyCount,
        IUrlHelper url)
    {
        viewBag.MembresiaActual = data.MembresiaActual;
        viewBag.MoreProfile = ProfileDisplayService.Build(
            usuario, data.MembresiaActual, propertyCount, data.DocCount, data.ServiceCount, url);
        viewBag.MetodosPago = data.MetodosPago;
        viewBag.Pagos = data.Pagos;
        viewBag.Historial = data.Historial;
        viewBag.MensajesSoporte = data.MensajesSoporte;
        viewBag.ProgramacionesMicroservicio = data.ProgramacionesMicroservicio;

        viewBag.SolicitudesInspeccion = data.SolicitudesInspeccion;
        viewBag.SolicitudesInspeccionElectrica = data.SolicitudesInspeccionElectrica;
        viewBag.SolicitudesInspeccionCompleta = data.SolicitudesInspeccionCompleta;
        viewBag.SolicitudesInspeccionPlomeria = data.SolicitudesInspeccionPlomeria;
        viewBag.SolicitudesInspeccionHvac = data.SolicitudesInspeccionHvac;
        viewBag.SolicitudesInspeccionStructural = data.SolicitudesInspeccionStructural;
        viewBag.SolicitudesInspeccionRoof = data.SolicitudesInspeccionRoof;
        viewBag.SolicitudesInspeccionMoldMoisture = data.SolicitudesInspeccionMoldMoisture;
        viewBag.SolicitudesInspeccionWindowsInsulation = data.SolicitudesInspeccionWindowsInsulation;
        viewBag.SolicitudesInspeccionHomeSafety = data.SolicitudesInspeccionHomeSafety;
        viewBag.SolicitudesInspeccionInvestor = data.SolicitudesInspeccionInvestor;

        viewBag.SolicitudesEmergenciaPlomeria = data.SolicitudesEmergenciaPlomeria;
        viewBag.SolicitudesEmergenciaPlomeriaEnviadas = data.SolicitudesEmergenciaPlomeriaEnviadas;
        viewBag.SolicitudesEmergenciaHvac = data.SolicitudesEmergenciaHvac;
        viewBag.SolicitudesEmergenciaHvacEnviadas = data.SolicitudesEmergenciaHvacEnviadas;
        viewBag.SolicitudesEmergenciaWaterHeater = data.SolicitudesEmergenciaWaterHeater;
        viewBag.SolicitudesEmergenciaWaterHeaterEnviadas = data.SolicitudesEmergenciaWaterHeaterEnviadas;
        viewBag.SolicitudesEmergenciaFlood = data.SolicitudesEmergenciaFlood;
        viewBag.SolicitudesEmergenciaFloodEnviadas = data.SolicitudesEmergenciaFloodEnviadas;
        viewBag.SolicitudesEmergenciaElectrical = data.SolicitudesEmergenciaElectrical;
        viewBag.SolicitudesEmergenciaElectricalEnviadas = data.SolicitudesEmergenciaElectricalEnviadas;
        viewBag.SolicitudesEmergenciaTreeDamage = data.SolicitudesEmergenciaTreeDamage;
        viewBag.SolicitudesEmergenciaTreeDamageEnviadas = data.SolicitudesEmergenciaTreeDamageEnviadas;
        viewBag.SolicitudesEmergenciaRoofLeak = data.SolicitudesEmergenciaRoofLeak;
        viewBag.SolicitudesEmergenciaRoofLeakEnviadas = data.SolicitudesEmergenciaRoofLeakEnviadas;
        viewBag.SolicitudesEmergenciaSmokeDetector = data.SolicitudesEmergenciaSmokeDetector;
        viewBag.SolicitudesEmergenciaSmokeDetectorEnviadas = data.SolicitudesEmergenciaSmokeDetectorEnviadas;

        viewBag.InspeccionesConfirmadas = BuildInspeccionesConfirmadas(data);
    }

    public static List<ConfirmedInspectionViewModel> BuildInspeccionesConfirmadas(HomeIndexUserPageData data) =>
        data.PurchaseConfirmed
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
            .Concat(data.ElectricalConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.CompleteConfirmed.Select(s => new ConfirmedInspectionViewModel
            {
                FlowType = "complete",
                SolicitudId = s.Id,
                NombreServicio = s.Inspeccion?.Nombre ?? "Complete home inspection",
                DireccionPropiedad = s.DireccionPropiedad,
                FechaCita = s.FechaCitaProgramada!.Value,
                HoraCita = InspeccionDisplayLabels.FormatTime(s.HoraCitaProgramada!.Value),
                ResumenPreocupacion = InspeccionDisplayLabels.FormatAreasEnfoque(s.AreasEnfoque)
            }))
            .Concat(data.PlumbingConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.HvacConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.StructuralConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.RoofConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.MoldMoistureConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.WindowsInsulationConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.HomeSafetyConfirmed.Select(s => new ConfirmedInspectionViewModel
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
            .Concat(data.InvestorConfirmed.Select(s => new ConfirmedInspectionViewModel
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
}
