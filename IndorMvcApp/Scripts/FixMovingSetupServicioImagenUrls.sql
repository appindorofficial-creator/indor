/*
  Bug 73 — Each Moving Setup task landing should show a distinct hero image.
  Previously all flows used /inspeccion2.jpeg.

  Safe to run multiple times.
*/

DECLARE @MovingId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Moving' ORDER BY Id);
DECLARE @CleaningId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Cleaning' ORDER BY Id);
DECLARE @PackingId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Packing Help' ORDER BY Id);
DECLARE @FurnitureId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Furniture & Assembly' ORDER BY Id);
DECLARE @TvId INT = (SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'TV Wall Mounting' ORDER BY Id);

IF @MovingId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.MovingServicioLanding WHERE MovingSetupServicioId = @MovingId)
    UPDATE dbo.MovingServicioLanding SET ImagenUrl = N'/inspeccion2.jpeg' WHERE MovingSetupServicioId = @MovingId;

IF @CleaningId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.CleaningServicioLanding WHERE MovingSetupServicioId = @CleaningId)
    UPDATE dbo.CleaningServicioLanding SET ImagenUrl = N'/limpieza.jpeg' WHERE MovingSetupServicioId = @CleaningId;

IF @PackingId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.PackingServicioLanding WHERE MovingSetupServicioId = @PackingId)
    UPDATE dbo.PackingServicioLanding SET ImagenUrl = N'/servicio3.jpeg' WHERE MovingSetupServicioId = @PackingId;

IF @FurnitureId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.FurnitureAssemblyServicioLanding WHERE MovingSetupServicioId = @FurnitureId)
    UPDATE dbo.FurnitureAssemblyServicioLanding SET ImagenUrl = N'/servicio1.jpeg' WHERE MovingSetupServicioId = @FurnitureId;

IF @TvId IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.TvWallMountingServicioLanding WHERE MovingSetupServicioId = @TvId)
    UPDATE dbo.TvWallMountingServicioLanding SET ImagenUrl = N'/tv-wall-mounting-hero.png' WHERE MovingSetupServicioId = @TvId;

IF EXISTS (SELECT 1 FROM dbo.MovingSetupConfig)
    UPDATE dbo.MovingSetupConfig SET FeaturedImagenUrl = N'/inspeccion2.jpeg' WHERE Activo = 1;

PRINT 'Moving Setup service hero images synced.';
GO

PRINT '=== Moving Setup landing images ===';
SELECT s.Nombre, l.ImagenUrl
FROM dbo.MovingSetupServicios s
LEFT JOIN dbo.MovingServicioLanding l ON l.MovingSetupServicioId = s.Id
WHERE s.Nombre = N'Moving'
UNION ALL
SELECT s.Nombre, l.ImagenUrl
FROM dbo.MovingSetupServicios s
LEFT JOIN dbo.CleaningServicioLanding l ON l.MovingSetupServicioId = s.Id
WHERE s.Nombre = N'Cleaning'
UNION ALL
SELECT s.Nombre, l.ImagenUrl
FROM dbo.MovingSetupServicios s
LEFT JOIN dbo.PackingServicioLanding l ON l.MovingSetupServicioId = s.Id
WHERE s.Nombre = N'Packing Help'
UNION ALL
SELECT s.Nombre, l.ImagenUrl
FROM dbo.MovingSetupServicios s
LEFT JOIN dbo.FurnitureAssemblyServicioLanding l ON l.MovingSetupServicioId = s.Id
WHERE s.Nombre = N'Furniture & Assembly'
UNION ALL
SELECT s.Nombre, l.ImagenUrl
FROM dbo.MovingSetupServicios s
LEFT JOIN dbo.TvWallMountingServicioLanding l ON l.MovingSetupServicioId = s.Id
WHERE s.Nombre = N'TV Wall Mounting';
GO
