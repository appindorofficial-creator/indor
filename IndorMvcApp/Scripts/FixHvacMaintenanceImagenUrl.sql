/*
  FixHvacMaintenanceImagenUrl.sql
  ---------------------------------
  HVAC maintenance (Home Care Guide) image:
    - Was /inspeccion5.jpeg (plumbing under sink)
    - Then /inspeccion8.jpeg (generic HVAC inspection photo on Azure)
    - Now /priority-hvac-maintenance.png (dedicated asset in wwwroot)

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities
SET ImagenUrl = N'/priority-hvac-maintenance.png'
WHERE Nombre = N'HVAC maintenance'
  AND (ImagenUrl IS NULL OR ImagenUrl IN (N'/inspeccion5.jpeg', N'/inspeccion8.jpeg'));

UPDATE l
SET l.ImagenUrl = N'/priority-hvac-maintenance.png'
FROM dbo.HvacMaintenanceServicioLanding l
INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
WHERE p.Nombre = N'HVAC maintenance'
  AND (l.ImagenUrl IS NULL OR l.ImagenUrl IN (N'/inspeccion5.jpeg', N'/inspeccion8.jpeg'));

PRINT '';
PRINT '=== HVAC maintenance image ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl, LinkController, LinkAction
FROM dbo.HomeCarePriorities
WHERE Nombre = N'HVAC maintenance';

GO
