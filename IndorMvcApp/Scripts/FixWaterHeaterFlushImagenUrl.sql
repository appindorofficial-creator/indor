/*
  FixWaterHeaterFlushImagenUrl.sql
  --------------------------------
  Water heater flush (Home Care Guide) was using /inspeccion4.jpeg (electrical panel).
  Correct asset: /priority-water-heater-flush.png (tank drain / sediment flush).

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities
SET ImagenUrl = N'/priority-water-heater-flush.png'
WHERE Nombre = N'Water heater flush'
  AND (ImagenUrl IS NULL OR ImagenUrl = N'/inspeccion4.jpeg');

UPDATE l
SET l.ImagenUrl = N'/priority-water-heater-flush.png'
FROM dbo.WaterHeaterFlushServicioLanding l
INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
WHERE p.Nombre = N'Water heater flush'
  AND (l.ImagenUrl IS NULL OR l.ImagenUrl = N'/inspeccion4.jpeg');

PRINT '';
PRINT '=== Water heater flush image ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl, LinkController, LinkAction
FROM dbo.HomeCarePriorities
WHERE Nombre = N'Water heater flush';

GO
