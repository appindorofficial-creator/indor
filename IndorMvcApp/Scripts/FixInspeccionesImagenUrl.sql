/*
  FixInspeccionesImagenUrl.sql
  ----------------------------
  Corrige ImagenUrl en la sección "Inspections" (tabla Inspecciones).

  Problema: cada inspección apuntaba a inspeccion{N}.jpeg por número de fila,
  pero el contenido real de varios archivos no coincide.

  Inventario del contenido actual en /wwwroot (Azure):
    inspeccion1.jpeg   -> Techo / linterna (pre-compra)
    inspeccion2.jpeg   -> Exterior con pareja e inspector
    inspeccion3.jpeg   -> Panel eléctrico en piso
    inspeccion4.jpeg   -> Panel eléctrico / cableado
    inspeccion5.jpeg   -> Plomería bajo fregadero
    inspeccion6.jpeg   -> Muro estructural / ladrillo
    inspeccion7.jpeg   -> Techo / inspector en azotea
    inspeccion8.jpeg   -> Unidad HVAC / mecánica interior
    inspeccion9.jpeg   -> Grietas en concreto / fundación
    inspeccion10.jpeg  -> Medidor de humedad en muro / ventana
    inspeccion11.jpeg  -> Marco de ventana / revisión interior
    inspeccion12.jpeg  -> Reporte / interior (cabinete)
    inspeccion13.jpeg  -> Tablet con reporte (inversionista)
    inspeccion14.jpeg  -> (reserva)
    inspeccion15.jpeg  -> (reserva / express)

  También disponible en el proyecto:
    priority-smoke-detector.png -> Home Safety

  Ejecutar en SSMS o Azure Query editor. Idempotente.
*/

SET NOCOUNT ON;

-- ------------------------------------------------------------------
-- 1) Corregir por Orden (1..15) — más estable entre entornos
-- ------------------------------------------------------------------
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion1.jpeg'              WHERE Orden = 1;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion2.jpeg'              WHERE Orden = 2;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion3.jpeg'              WHERE Orden = 3;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion5.jpeg'              WHERE Orden = 4;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion8.jpeg'              WHERE Orden = 5;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion6.jpeg'              WHERE Orden = 6;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion7.jpeg'              WHERE Orden = 7;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion9.jpeg'              WHERE Orden = 8;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion10.jpeg'             WHERE Orden = 9;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion11.jpeg'             WHERE Orden = 10;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/priority-smoke-detector.png'   WHERE Orden = 11;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion12.jpeg'             WHERE Orden = 12;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion13.jpeg'             WHERE Orden = 13;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion4.jpeg'              WHERE Orden = 14;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion15.jpeg'             WHERE Orden = 15;

-- ------------------------------------------------------------------
-- 2) Respaldo por Id (catálogo inglés Id 0..14)
-- ------------------------------------------------------------------
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion1.jpeg'              WHERE Id = 0;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion2.jpeg'              WHERE Id = 1;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion3.jpeg'              WHERE Id = 2;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion5.jpeg'              WHERE Id = 3;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion8.jpeg'              WHERE Id = 4;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion6.jpeg'              WHERE Id = 5;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion7.jpeg'              WHERE Id = 6;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion9.jpeg'              WHERE Id = 7;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion10.jpeg'             WHERE Id = 8;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion11.jpeg'             WHERE Id = 9;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/priority-smoke-detector.png'   WHERE Id = 10;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion12.jpeg'             WHERE Id = 11;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion13.jpeg'             WHERE Id = 12;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion4.jpeg'              WHERE Id = 13;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion15.jpeg'             WHERE Id = 14;

-- ------------------------------------------------------------------
-- 3) Respaldo por Id (catálogo español Id 1..15)
-- ------------------------------------------------------------------
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion1.jpeg'              WHERE Id = 1  AND Orden = 1;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion2.jpeg'              WHERE Id = 2  AND Orden = 2;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion3.jpeg'              WHERE Id = 3  AND Orden = 3;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion5.jpeg'              WHERE Id = 4  AND Orden = 4;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion8.jpeg'              WHERE Id = 5  AND Orden = 5;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion6.jpeg'              WHERE Id = 6  AND Orden = 6;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion7.jpeg'              WHERE Id = 7  AND Orden = 7;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion9.jpeg'              WHERE Id = 8  AND Orden = 8;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion10.jpeg'             WHERE Id = 9  AND Orden = 9;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion11.jpeg'             WHERE Id = 10 AND Orden = 10;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/priority-smoke-detector.png'   WHERE Id = 11 AND Orden = 11;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion12.jpeg'             WHERE Id = 12 AND Orden = 12;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion13.jpeg'             WHERE Id = 13 AND Orden = 13;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion4.jpeg'              WHERE Id = 14 AND Orden = 14;
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion15.jpeg'             WHERE Id = 15 AND Orden = 15;

-- ------------------------------------------------------------------
-- 4) Respaldo por nombre (inglés o español)
-- ------------------------------------------------------------------
UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion1.jpeg'
WHERE Nombre IN (N'Pre-Purchase Home Inspection', N'Inspección Pre-Compra de Vivienda');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion2.jpeg'
WHERE Nombre IN (N'Complete Home Inspection', N'Inspección Completa del Hogar');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion3.jpeg'
WHERE Nombre IN (N'Electrical Inspection', N'Inspección Eléctrica');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion5.jpeg'
WHERE Nombre IN (N'Plumbing Inspection', N'Inspección de Plomería');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion8.jpeg'
WHERE Nombre IN (N'HVAC Inspection', N'Inspección HVAC');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion6.jpeg'
WHERE Nombre IN (N'Structural Inspection', N'Inspección Estructural');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion7.jpeg'
WHERE Nombre IN (N'Roof Inspection', N'Inspección de Techos');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion9.jpeg'
WHERE Nombre IN (N'Foundation Inspection', N'Inspección de Fundaciones');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion10.jpeg'
WHERE Nombre IN (N'Mold and Moisture Inspection', N'Inspección de Moho y Humedad');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion11.jpeg'
WHERE Nombre IN (N'Windows and Insulation Inspection', N'Inspección de Ventanas y Aislamiento');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/priority-smoke-detector.png'
WHERE Nombre IN (N'Home Safety Inspection', N'Inspección de Seguridad del Hogar');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion12.jpeg'
WHERE Nombre IN (N'Inspection with Professional Report', N'Inspección con Reporte Profesional');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion13.jpeg'
WHERE Nombre IN (N'Investor Inspection', N'Inspección para Inversionistas');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion4.jpeg'
WHERE Nombre IN (N'Hidden Problems Inspection', N'Inspección de Problemas Ocultos');

UPDATE dbo.Inspecciones SET ImagenUrl = N'/inspeccion15.jpeg'
WHERE Nombre IN (N'Express Inspection', N'Inspección Express');

-- ------------------------------------------------------------------
-- 5) Verificación
-- ------------------------------------------------------------------
PRINT '';
PRINT '=== Inspecciones — imagen asignada ===';

SELECT
    i.Id,
    i.Orden,
    i.Nombre,
    i.ImagenUrl
FROM dbo.Inspecciones i
WHERE i.Activo = 1
ORDER BY i.Orden, i.Id;

GO
