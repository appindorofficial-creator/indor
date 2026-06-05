/*
  FixPowerWashImagenUrl.sql
  -------------------------
  Power wash exterior was using /servicio5.jpeg (same as Impactful Exteriors in Services).
  Correct asset: /priority-power-wash-exterior.png

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities
SET ImagenUrl = N'/priority-power-wash-exterior.png'
WHERE Nombre = N'Power wash exterior'
  AND (ImagenUrl IS NULL OR ImagenUrl = N'/servicio5.jpeg');

UPDATE l
SET l.ImagenUrl = N'/priority-power-wash-exterior.png'
FROM dbo.PowerWashServicioLanding l
INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
WHERE p.Nombre = N'Power wash exterior'
  AND (l.ImagenUrl IS NULL OR l.ImagenUrl = N'/servicio5.jpeg');

PRINT '';
PRINT '=== Power wash exterior image ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl FROM dbo.HomeCarePriorities WHERE Nombre = N'Power wash exterior';

GO
