/*
  =============================================================================
  DEPLOY BUG PACKAGE #26 – #86 — Azure / IndorDB
  =============================================================================

  Oscar: ejecuta ESTE archivo en SSMS o Azure Data Studio contra IndorDB.
  Todos los scripts incluidos son idempotentes (se pueden correr más de una vez).

  IMPORTANTE ANTES DE CORRER SQL:
    1. Publicar la app en Azure (los bugs #29–#86 son mayormente código).
    2. Verificar que las imágenes nuevas estén en wwwroot (priority-*.png,
       emergency-*.png). Sin ellas, las URLs en BD apuntan a archivos vacíos.

  CÓMO EJECUTAR (SSMS / Azure Data Studio):
    - Abrir este archivo desde la carpeta Scripts (las rutas :r son relativas).
    - Conectar a IndorDB → F5 (Execute).

  CÓMO EJECUTAR (sqlcmd):
    sqlcmd -S <servidor> -d IndorDB -i DeployBugPackage26-86_Azure.sql

  =============================================================================
  RESUMEN — qué corrige cada fase
  =============================================================================

  FASE 1 — Tablas (solo crea si NO existen):
    #40  Remodeling services flow (8 servicios del carrusel Services)
    #39/#44/#50  Emergency Services grid
    Home Care Guide + Moving Setup + flujos de reserva

  FASE 2 — Datos / catálogos:
    #39/#50  Quitar Plumbing preseleccionado (EsPredeterminado = 0)
    #39/#44  Renombrar Flood, Tree Damage, Smoke Detector en emergencias
    Imágenes correctas en Emergency Services y Home Care Guide
    #71      Roof inspection frecuencia "Every 1–2 years"
    #73      Imágenes distintas por servicio en Moving Setup
    Enlaces de navegación Moving Setup → flujos reales

  BUGS SIN SQL (solo deploy de código):
    #29–#38, #41–#43, #45–#49, #51–#70, #72, #74–#86

  =============================================================================
*/

USE [IndorDB];
GO

SET NOCOUNT ON;
PRINT '';
PRINT '=============================================================';
PRINT ' FASE 1 — Tablas de flujo (idempotente, solo si faltan)';
PRINT '=============================================================';
GO

:r .\CreateServiciosEmergenciaTable.sql
GO

:r .\CreateRemodelingServicioFlowTables.sql
GO

:r .\CreateHomeCarePrioritiesTables.sql
GO

:r .\CreateMovingSetupTables.sql
GO

:r .\CreateMovingFlowTables.sql
GO

:r .\CreateCleaningFlowTables.sql
GO

:r .\CreatePackingFlowTables.sql
GO

:r .\CreateFurnitureAssemblyFlowTables.sql
GO

:r .\CreateTvWallMountingFlowTables.sql
GO

:r .\CreateHvacMaintenanceFlowTables.sql
GO

:r .\CreateWaterHeaterFlushFlowTables.sql
GO

:r .\CreateCrawlspaceCheckFlowTables.sql
GO

:r .\CreateRoofInspectionFlowTables.sql
GO

:r .\CreatePowerWashFlowTables.sql
GO

:r .\CreateExteriorPaintFlowTables.sql
GO

PRINT '';
PRINT '=============================================================';
PRINT ' FASE 2 — Correcciones de datos (bugs #39–#73)';
PRINT '=============================================================';
GO

-- #39 / #50 — Ningún servicio de emergencia preseleccionado
IF OBJECT_ID(N'dbo.ServiciosEmergencia', N'U') IS NOT NULL
BEGIN
    UPDATE dbo.ServiciosEmergencia
    SET EsPredeterminado = 0
    WHERE EsPredeterminado = 1;

    PRINT 'ServiciosEmergencia: EsPredeterminado = 0 en todos.';
END
GO

:r .\UpdateServiciosEmergenciaMockup.sql
GO

:r .\FixServiciosEmergenciaImagenUrls.sql
GO

:r .\FixHomeCarePrioritiesImagenUrls.sql
GO

:r .\FixRoofInspectionHomeCareFrequency.sql
GO

:r .\FixMovingSetupServicioImagenUrls.sql
GO

:r .\FixMovingSetupServicioLinks.sql
GO

PRINT '';
PRINT '=============================================================';
PRINT ' VERIFICACIÓN FINAL';
PRINT '=============================================================';
GO

IF OBJECT_ID(N'dbo.ServiciosEmergencia', N'U') IS NOT NULL
BEGIN
    PRINT '--- Emergency Services (ninguno debe tener EsPredeterminado = 1) ---';
    SELECT Id, Nombre, TituloEmergencia, ImagenUrl, EsPredeterminado, Orden
    FROM dbo.ServiciosEmergencia
    WHERE Activo = 1
    ORDER BY Orden;
END
GO

IF OBJECT_ID(N'dbo.HomeCarePriorities', N'U') IS NOT NULL
BEGIN
    PRINT '--- Home Care Guide ---';
    SELECT Id, Nombre, Subtitulo, ImagenUrl, Orden
    FROM dbo.HomeCarePriorities
    WHERE Activo = 1
    ORDER BY Orden;
END
GO

IF OBJECT_ID(N'dbo.MovingSetupServicios', N'U') IS NOT NULL
BEGIN
    PRINT '--- Moving Setup (enlaces e imágenes) ---';
    SELECT Id, Nombre, LinkController, LinkAction, Orden
    FROM dbo.MovingSetupServicios
    ORDER BY Orden;
END
GO

IF OBJECT_ID(N'dbo.SolicitudesRemodelingServicio', N'U') IS NOT NULL
    PRINT 'OK: tabla SolicitudesRemodelingServicio existe (Bug #40/#44).';
ELSE
    PRINT 'AVISO: falta SolicitudesRemodelingServicio — revisar Fase 1.';
GO

PRINT '';
PRINT 'DeployBugPackage26-86_Azure.sql — COMPLETADO.';
PRINT 'Siguiente paso: publicar la app y probar en móvil.';
GO
