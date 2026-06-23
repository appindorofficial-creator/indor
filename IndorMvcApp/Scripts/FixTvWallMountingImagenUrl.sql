/*
  FixTvWallMountingImagenUrl.sql
  ------------------------------
  TV Wall Mounting was using /servicio13.jpeg (HVAC ceiling vent / air filter).
  Correct asset: /tv-wall-mounting-hero.png

  Run on Azure/local after deploy. Idempotent.
*/

SET NOCOUNT ON;

DECLARE @TvServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'TV Wall Mounting' ORDER BY Id
);

IF @TvServicioId IS NOT NULL
BEGIN
    UPDATE dbo.TvWallMountingServicioLanding
    SET ImagenUrl = N'/tv-wall-mounting-hero.png'
    WHERE MovingSetupServicioId = @TvServicioId
      AND (ImagenUrl IS NULL OR ImagenUrl = N'/servicio13.jpeg');
END

PRINT '';
PRINT '=== TV Wall Mounting landing image ===';
SELECT s.Nombre, l.ImagenUrl
FROM dbo.MovingSetupServicios s
LEFT JOIN dbo.TvWallMountingServicioLanding l ON l.MovingSetupServicioId = s.Id
WHERE s.Nombre = N'TV Wall Mounting';

GO
