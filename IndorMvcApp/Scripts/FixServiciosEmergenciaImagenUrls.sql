/*
  FixServiciosEmergenciaImagenUrls.sql
  ------------------------------------
  24/7 Emergency grid was reusing wrong catalog JPEGs:
    HVAC        -> inspeccion5 (plumbing) or priority-hvac
    Water Heater -> servicio4 (bathroom)
    Plumbing    -> inspeccion4 (electrical panel)
    Flood       -> inspeccion4 (same as Plumbing)
    Electrical  -> inspeccion3 (kitchen / mixed scene on some envs)
    Roof Leak   -> inspeccion7 (routine roof check, not leak)
    Tree Damage -> inspeccion9 (foundation cracks)
    Smoke Detector -> inspeccion2 (exterior couple with inspector)

  Each emergency card now has a dedicated /emergency-*.png in wwwroot.

  Deploy wwwroot images first, then run on Azure/local. Idempotent.
*/

SET NOCOUNT ON;

UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-hvac.png'              WHERE Nombre = N'HVAC';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-water-heater.png'     WHERE Nombre = N'Water Heater';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-plumbing.png'         WHERE Nombre = N'Plumbing';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-flood.png'              WHERE Nombre = N'Flood';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-electrical.png'       WHERE Nombre = N'Electrical';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-roof-leak.png'       WHERE Nombre = N'Roof Leak';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-tree-damage.png'     WHERE Nombre = N'Tree Damage';
UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-smoke-detector.png'  WHERE Nombre = N'Smoke Detector';

PRINT '';
PRINT '=== ServiciosEmergencia — images ===';
SELECT Id, Nombre, TituloEmergencia, ImagenUrl, Orden
FROM dbo.ServiciosEmergencia
WHERE Activo = 1
ORDER BY Orden;

GO
