/*
  Update ServiciosEmergencia to match the Home mockup grid:
  - Drain Cleaning  -> Flood
  - Mold Remediation -> Tree Damage
  - Gas Line        -> Smoke Detector (legacy: Gas / CO)

  Safe to run multiple times (updates by Orden or legacy Nombre).
  Run manually after reviewing. Does not delete rows.
*/

SET NOCOUNT ON;

DECLARE @updated INT = 0;

-- Orden 4: Flood
IF EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia WHERE Nombre = N'Drain Cleaning' OR (Orden = 4 AND Nombre <> N'Flood'))
BEGIN
    UPDATE dbo.ServiciosEmergencia
    SET
        Nombre = N'Flood',
        TituloEmergencia = N'Flood Emergency',
        Descripcion = N'Standing water, basement flooding, and urgent water removal.',
        TiempoLlegadaMinutos = 45,
        IconoClase = N'fa-water',
        ImagenUrl = N'/emergency-flood.png',
        BadgeTexto = NULL,
        Caracteristicas = N'Arrives fast|Trusted pros|Upfront pricing',
        IconosCaracteristicas = N'fa-clock|fa-shield-halved|fa-star',
        CtaTexto = N'Request help',
        Activo = 1,
        Orden = 4
    WHERE Nombre = N'Drain Cleaning'
       OR (Orden = 4 AND Nombre <> N'Flood');

    SET @updated += @@ROWCOUNT;
    PRINT 'Updated Orden 4 -> Flood.';
END
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia WHERE Nombre = N'Flood')
BEGIN
    PRINT 'Skipped Orden 4: Flood not found and Drain Cleaning not present.';
END
ELSE
BEGIN
    PRINT 'Orden 4 already set to Flood.';
END

-- Orden 7: Tree Damage
IF EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia WHERE Nombre = N'Mold Remediation' OR (Orden = 7 AND Nombre <> N'Tree Damage'))
BEGIN
    UPDATE dbo.ServiciosEmergencia
    SET
        Nombre = N'Tree Damage',
        TituloEmergencia = N'Tree Damage Emergency',
        Descripcion = N'Fallen trees, limbs on structures, and storm-related hazards.',
        TiempoLlegadaMinutos = 45,
        IconoClase = N'fa-tree',
        ImagenUrl = N'/emergency-tree-damage.png',
        BadgeTexto = NULL,
        Caracteristicas = N'Arrives fast|Trusted pros|Upfront pricing',
        IconosCaracteristicas = N'fa-clock|fa-shield-halved|fa-star',
        CtaTexto = N'Request help',
        Activo = 1,
        Orden = 7
    WHERE Nombre = N'Mold Remediation'
       OR (Orden = 7 AND Nombre <> N'Tree Damage');

    SET @updated += @@ROWCOUNT;
    PRINT 'Updated Orden 7 -> Tree Damage.';
END
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia WHERE Nombre = N'Tree Damage')
BEGIN
    PRINT 'Skipped Orden 7: Tree Damage not found and Mold Remediation not present.';
END
ELSE
BEGIN
    PRINT 'Orden 7 already set to Tree Damage.';
END

-- Orden 8: Smoke Detector
IF EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia
           WHERE Nombre IN (N'Gas Line', N'Gas / CO')
              OR (Orden = 8 AND Nombre <> N'Smoke Detector'))
BEGIN
    UPDATE dbo.ServiciosEmergencia
    SET
        Nombre = N'Smoke Detector',
        TituloEmergencia = N'Smoke Detector & CO Alert',
        Descripcion = N'Chirping alarms, dead batteries, missing detectors, and urgent smoke alarm help.',
        TiempoLlegadaMinutos = 45,
        IconoClase = N'fa-bell',
        ImagenUrl = N'/emergency-smoke-detector.png',
        BadgeTexto = NULL,
        Caracteristicas = N'Arrives fast|Trusted pros|Upfront pricing',
        IconosCaracteristicas = N'fa-clock|fa-shield-halved|fa-star',
        CtaTexto = N'Request help',
        Activo = 1,
        Orden = 8
    WHERE Nombre IN (N'Gas Line', N'Gas / CO')
       OR (Orden = 8 AND Nombre <> N'Smoke Detector');

    SET @updated += @@ROWCOUNT;
    PRINT 'Updated Orden 8 -> Smoke Detector.';
END
ELSE IF NOT EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia WHERE Nombre = N'Smoke Detector')
BEGIN
    PRINT 'Skipped Orden 8: Smoke Detector not found and Gas Line / Gas / CO not present.';
END
ELSE
BEGIN
    PRINT 'Orden 8 already set to Smoke Detector.';
END

PRINT CONCAT('Done. Rows updated: ', @updated);

SELECT Id, Nombre, TituloEmergencia, IconoClase, Orden, Activo
FROM dbo.ServiciosEmergencia
ORDER BY Orden, Id;
GO
