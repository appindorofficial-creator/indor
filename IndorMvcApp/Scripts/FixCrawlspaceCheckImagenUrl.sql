/*
  FixCrawlspaceCheckImagenUrl.sql
  --------------------------------
  Crawlspace check (Home Care Guide) was using /inspeccion3.jpeg (electrical panel).
  Correct asset: /priority-crawlspace-check.png (under-house crawlspace inspection).

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.HomeCarePriorities
SET ImagenUrl = N'/priority-crawlspace-check.png'
WHERE Nombre = N'Crawlspace check'
  AND (ImagenUrl IS NULL OR ImagenUrl = N'/inspeccion3.jpeg');

UPDATE l
SET l.ImagenUrl = N'/priority-crawlspace-check.png'
FROM dbo.CrawlspaceCheckServicioLanding l
INNER JOIN dbo.HomeCarePriorities p ON p.Id = l.HomeCarePriorityId
WHERE p.Nombre = N'Crawlspace check'
  AND (l.ImagenUrl IS NULL OR l.ImagenUrl = N'/inspeccion3.jpeg');

PRINT '';
PRINT '=== Crawlspace check image ===';
SELECT Id, Nombre, Subtitulo, ImagenUrl, LinkController, LinkAction
FROM dbo.HomeCarePriorities
WHERE Nombre = N'Crawlspace check';

GO
