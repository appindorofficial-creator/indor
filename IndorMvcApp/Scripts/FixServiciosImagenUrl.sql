/*
  FixServiciosImagenUrl.sql
  -------------------------
  Corrige la asignación de imágenes en la sección "Services" (tabla Servicios).

  Problema: cada servicio apuntaba a servicio{N}.jpeg por número de Id, pero el
  contenido real de cada archivo no coincide con ese número.

  Inventario del contenido actual en /wwwroot (Azure):
    servicio1.jpeg   -> Cocina
    servicio2.jpeg   -> Obra / estructura (ampliación)
    servicio3.jpeg   -> Sala / interior renovado
    servicio4.jpeg   -> Baño moderno
    servicio5.jpeg   -> Fachada exterior
    servicio6.jpeg   -> Terraza / patio
    servicio7.jpeg   -> Calentador de agua / unidad en pared
    servicio8.jpeg   -> Instalación de pisos
    servicio9.jpeg   -> Pintura interior (techo)
    servicio10.jpeg  -> Pintura exterior de casa
    servicio11.jpeg  -> Pintura interior (muros)
    servicio12.jpeg  -> Entrada / concreto (trabajadores)
    servicio13.jpeg  -> Rejilla / ventilación HVAC en techo

  También disponible en el proyecto:
    priority-smoke-detector.png -> Detectores de humo (Home Security)

  Ejecutar en SSMS o Azure Query editor contra la BD de Indor2.
  Idempotente: puede ejecutarse varias veces.
*/

SET NOCOUNT ON;

-- ------------------------------------------------------------------
-- 1) Corregir ImagenUrl por Id (catálogo estándar Id 1..13)
-- ------------------------------------------------------------------
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio1.jpeg'                WHERE Id = 1;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio4.jpeg'                WHERE Id = 2;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio3.jpeg'                WHERE Id = 3;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio2.jpeg'                WHERE Id = 4;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio5.jpeg'                WHERE Id = 5;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio6.jpeg'                WHERE Id = 6;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio13.jpeg'               WHERE Id = 7;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio7.jpeg'                WHERE Id = 8;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio8.jpeg'                WHERE Id = 9;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio11.jpeg'               WHERE Id = 10;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio10.jpeg'               WHERE Id = 11;
UPDATE dbo.Servicios SET ImagenUrl = N'/priority-smoke-detector.png'   WHERE Id = 12;
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio12.jpeg'               WHERE Id = 13;

-- ------------------------------------------------------------------
-- 2) Respaldo por nombre (inglés o español) por si los Id difieren
-- ------------------------------------------------------------------
UPDATE dbo.Servicios SET ImagenUrl = N'/servicio1.jpeg'
WHERE Nombre IN (N'Dream Kitchen', N'Cocina de Ensueño');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio4.jpeg'
WHERE Nombre IN (N'Modern Bath Pro', N'Baño Moderno Pro');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio3.jpeg'
WHERE Nombre IN (N'Total Interior Renovation', N'Renovación Interior Total');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio2.jpeg'
WHERE Nombre IN (N'Space Expansion', N'Expansión de Espacios');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio5.jpeg'
WHERE Nombre IN (N'Impactful Exteriors', N'Exteriores que Impactan');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio6.jpeg'
WHERE Nombre IN (N'Perfect Patio', N'Terraza Perfecta');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio13.jpeg'
WHERE Nombre IN (N'Air Conditioning Installation', N'Instalación Aire Acondicionado');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio7.jpeg'
WHERE Nombre IN (N'Water Heater Pro', N'Calentador de Agua Pro');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio8.jpeg'
WHERE Nombre IN (N'Perfect Floors', N'Pisos Perfectos');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio11.jpeg'
WHERE Nombre IN (N'Professional Interior Painting', N'Pintura Interior Profesional');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio10.jpeg'
WHERE Nombre IN (N'Premium Exterior Painting', N'Pintura Exterior Premium');

UPDATE dbo.Servicios SET ImagenUrl = N'/priority-smoke-detector.png'
WHERE Nombre IN (N'Home Security', N'Seguridad del Hogar');

UPDATE dbo.Servicios SET ImagenUrl = N'/servicio12.jpeg'
WHERE Nombre IN (N'Pro Concrete Driveway', N'Entrada de Concreto Pro');

-- ------------------------------------------------------------------
-- 3) Verificación
-- ------------------------------------------------------------------
PRINT '';
PRINT '=== Servicios — imagen asignada ===';

SELECT
    s.Id,
    s.Orden,
    s.Nombre,
    s.ImagenUrl,
    CASE s.Id
        WHEN 1  THEN N'Kitchen'
        WHEN 2  THEN N'Bathroom'
        WHEN 3  THEN N'Interior living room'
        WHEN 4  THEN N'Construction / expansion'
        WHEN 5  THEN N'House exterior'
        WHEN 6  THEN N'Patio'
        WHEN 7  THEN N'HVAC ceiling vent'
        WHEN 8  THEN N'Water heater'
        WHEN 9  THEN N'Floor installation'
        WHEN 10 THEN N'Interior painting'
        WHEN 11 THEN N'Exterior painting'
        WHEN 12 THEN N'Smoke detector'
        WHEN 13 THEN N'Concrete driveway'
        ELSE N'(custom)'
    END AS ExpectedImageTheme
FROM dbo.Servicios s
WHERE s.Activo = 1
ORDER BY s.Orden, s.Id;

GO
