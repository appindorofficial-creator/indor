/*
  FixRoofInspectionImagenUrl.sql
  ------------------------------
  Roof inspection (Home Care Guide) was using /inspeccion8.jpeg (HVAC unit).
  Correct asset: /priority-roof-inspection.png (roof shingles, leaks, drainage check).

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities
SET ImagenUrl = N'/priority-roof-inspection.png'
WHERE Nombre = N'Roof inspection'
  AND (ImagenUrl IS NULL OR ImagenUrl IN (N'/inspeccion8.jpeg', N'/inspeccion7.jpeg'));

UPDATE l
SET l.ImagenUrl = N'/priority-roof-inspection.png'
FROM dbo.RoofInspectionServicioLanding l
INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
WHERE p.Nombre = N'Roof inspection'
  AND (l.ImagenUrl IS NULL OR l.ImagenUrl IN (N'/inspeccion8.jpeg', N'/inspeccion7.jpeg'));

PRINT '';
PRINT '=== Roof inspection image ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl, LinkController, LinkAction
FROM dbo.HomeCarePriorities
WHERE Nombre = N'Roof inspection';

GO
