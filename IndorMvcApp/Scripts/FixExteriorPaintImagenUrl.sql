/*
  FixExteriorPaintImagenUrl.sql
  -----------------------------
  Exterior paint (Home Care Guide) was using /servicio6.jpeg (patio / terrace).
  Correct asset: /priority-exterior-paint.png (facade repainting, exterior protection).

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities
SET ImagenUrl = N'/priority-exterior-paint.png'
WHERE Nombre = N'Exterior paint'
  AND (ImagenUrl IS NULL OR ImagenUrl IN (N'/servicio6.jpeg', N'/servicio10.jpeg', N'/servicio5.jpeg'));

UPDATE l
SET l.ImagenUrl = N'/priority-exterior-paint.png'
FROM dbo.ExteriorPaintServicioLanding l
INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
WHERE p.Nombre = N'Exterior paint'
  AND (l.ImagenUrl IS NULL OR l.ImagenUrl IN (N'/servicio6.jpeg', N'/servicio10.jpeg', N'/servicio5.jpeg'));

PRINT '';
PRINT '=== Exterior paint image ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl, LinkController, LinkAction
FROM dbo.HomeCarePriorities
WHERE Nombre = N'Exterior paint';

GO
