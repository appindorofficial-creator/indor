/*
  FixHomeCarePrioritiesImagenUrls.sql
  -----------------------------------
  One-shot fix: Home Care Guide must NOT reuse images from Services (servicio*.jpeg)
  or Inspections (inspeccion*.jpeg). Each card has its own /priority-*.png in wwwroot.

  Deploy wwwroot priority-*.png files first, then run this on Azure/local.
  Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-hvac-maintenance.png'      WHERE Nombre = N'HVAC maintenance';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-water-heater-flush.png'   WHERE Nombre = N'Water heater flush';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-crawlspace-check.png'      WHERE Nombre = N'Crawlspace check';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-roof-inspection.png'        WHERE Nombre = N'Roof inspection';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-power-wash-exterior.png'   WHERE Nombre = N'Power wash exterior';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-exterior-paint.png'         WHERE Nombre = N'Exterior paint';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-gutter-cleaning.png'       WHERE Nombre = N'Gutter cleaning';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-pest-control.png'           WHERE Nombre = N'Pest control';
UPDATE dbo.HomeCarePriorities SET ImagenUrl = N'/priority-smoke-detector.png'        WHERE Nombre = N'Smoke Detector';

-- Landing tables (when flow tables exist)
IF OBJECT_ID(N'dbo.HvacMaintenanceServicioLanding', N'U') IS NOT NULL
    UPDATE l SET ImagenUrl = N'/priority-hvac-maintenance.png'
    FROM dbo.HvacMaintenanceServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'HVAC maintenance';

IF OBJECT_ID(N'dbo.WaterHeaterFlushServicioLanding', N'U') IS NOT NULL
    UPDATE l SET ImagenUrl = N'/priority-water-heater-flush.png'
    FROM dbo.WaterHeaterFlushServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'Water heater flush';

IF OBJECT_ID(N'dbo.CrawlspaceCheckServicioLanding', N'U') IS NOT NULL
    UPDATE l SET ImagenUrl = N'/priority-crawlspace-check.png'
    FROM dbo.CrawlspaceCheckServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'Crawlspace check';

IF OBJECT_ID(N'dbo.RoofInspectionServicioLanding', N'U') IS NOT NULL
    UPDATE l SET ImagenUrl = N'/priority-roof-inspection.png'
    FROM dbo.RoofInspectionServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'Roof inspection';

IF OBJECT_ID(N'dbo.PowerWashServicioLanding', N'U') IS NOT NULL
    UPDATE l SET ImagenUrl = N'/priority-power-wash-exterior.png'
    FROM dbo.PowerWashServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'Power wash exterior';

IF OBJECT_ID(N'dbo.ExteriorPaintServicioLanding', N'U') IS NOT NULL
    UPDATE l SET ImagenUrl = N'/priority-exterior-paint.png'
    FROM dbo.ExteriorPaintServicioLanding l
    INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
    WHERE p.Nombre = N'Exterior paint';

PRINT '';
PRINT '=== Home Care Guide — all priority images ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl, LinkController
FROM dbo.HomeCarePriorities
WHERE Activo = 1
ORDER BY Orden;

GO
