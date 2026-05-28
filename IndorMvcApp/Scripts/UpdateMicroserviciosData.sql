-- =============================================================
-- Actualiza la tabla Microservicios con los campos completos
-- (subtítulo, descripción larga, "Incluye", CTA, imagen URL,
-- prefijo de precio) y reemplaza los 4 registros con la data
-- definitiva proporcionada por negocio.
--
-- Ejecutable múltiples veces (idempotente).
-- =============================================================

-- 1. Agregar columnas nuevas si no existen
IF COL_LENGTH('dbo.Microservicios', 'Subtitulo') IS NULL
	ALTER TABLE dbo.Microservicios ADD Subtitulo NVARCHAR(250) NULL;
GO
IF COL_LENGTH('dbo.Microservicios', 'DescripcionCompleta') IS NULL
	ALTER TABLE dbo.Microservicios ADD DescripcionCompleta NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.Microservicios', 'Incluye') IS NULL
	ALTER TABLE dbo.Microservicios ADD Incluye NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.Microservicios', 'PrecioPrefijo') IS NULL
	ALTER TABLE dbo.Microservicios ADD PrecioPrefijo NVARCHAR(50) NULL;
GO
IF COL_LENGTH('dbo.Microservicios', 'CtaTexto') IS NULL
	ALTER TABLE dbo.Microservicios ADD CtaTexto NVARCHAR(80) NULL;
GO
IF COL_LENGTH('dbo.Microservicios', 'ImagenUrl') IS NULL
	ALTER TABLE dbo.Microservicios ADD ImagenUrl NVARCHAR(300) NULL;
GO

-- ImagenBase64 ya no es obligatoria; la dejamos opcional
IF EXISTS (
	SELECT 1 FROM sys.columns
	WHERE object_id = OBJECT_ID('dbo.Microservicios')
	  AND name = 'ImagenBase64'
	  AND is_nullable = 0
)
BEGIN
	ALTER TABLE dbo.Microservicios ALTER COLUMN ImagenBase64 NVARCHAR(MAX) NULL;
END
GO

-- 2. Limpiar registros antiguos para reemplazarlos por la versión final
DELETE FROM dbo.Microservicios;
DBCC CHECKIDENT ('dbo.Microservicios', RESEED, 0);
GO

-- 3. Insertar los 4 microservicios con la data final
INSERT INTO dbo.Microservicios
	(Nombre, Subtitulo, Descripcion, DescripcionCompleta, Incluye,
	 Frecuencia, Valor, Moneda, PrecioPrefijo, CtaTexto, ImagenUrl, ImagenBase64, Activo)
VALUES
(
	N'Aire Seguro 365',
	N'Respira tranquilo. Nosotros nos encargamos.',
	N'Cambio profesional del filtro de aire acondicionado para mejorar la calidad del aire y proteger tu sistema.',
	N'Mantén tu hogar saludable y tu sistema funcionando al 100%. Reemplazamos el filtro de tu aire acondicionado de forma rápida y segura, ayudando a reducir polvo, alergias y fallas costosas.',
	N'Revisión básica del sistema|Cambio de filtro|Instalación profesional',
	N'Cada 3 meses',
	49.00, N'USD', N'Desde', N'Agendar servicio',
	N'/aire.jpeg', N'', 1
),
(
	N'Jardín Siempre Perfecto',
	N'Tu casa se ve mejor… sin esfuerzo.',
	N'Servicio de corte de césped y mantenimiento básico para mantener tu propiedad impecable.',
	N'Nos encargamos de que tu jardín siempre luzca limpio, ordenado y profesional. Ideal para mantener el valor de tu propiedad sin perder tiempo.',
	N'Corte de césped|Limpieza básica de áreas verdes|Recolección de residuos',
	N'Semanal o quincenal',
	45.00, N'USD', N'Desde', N'Reservar mantenimiento',
	N'/cesped.jpeg', N'', 1
),
(
	N'Basura Sin Estrés',
	N'Nunca olvides el día de recolección.',
	N'Sacamos tu basura por ti y devolvemos los contenedores. Simple, automático y sin preocupaciones.',
	N'Olvídate de multas, malos olores o descuidos. Nos aseguramos de que tu basura esté lista en el día correcto y regresamos los contenedores a su lugar.',
	N'Sacar contenedores el día indicado|Retorno a su lugar|Servicio puntual y confiable',
	N'1–2 veces por semana',
	29.00, N'USD', N'Mensual', N'Activar servicio',
	N'/basura.jpeg', N'', 1
),
(
	N'Cleaning Pro',
	N'Limpieza profesional que se nota.',
	N'Servicio de limpieza para mantener tu hogar en perfectas condiciones, sin esfuerzo.',
	N'Disfruta de un hogar limpio, organizado y listo para vivir o alquilar. Nuestro equipo realiza limpiezas detalladas adaptadas a tus necesidades. Opciones: Limpieza estándar o Limpieza profunda.',
	N'Limpieza de superficies|Baños y cocina|Aspirado y trapeado',
	N'Semanal, quincenal o mensual',
	99.00, N'USD', N'Desde', N'Agendar limpieza',
	N'/limpieza.jpeg', N'', 1
);
GO

-- 4. Verificación
SELECT Id, Nombre, Subtitulo, Frecuencia, PrecioPrefijo, Valor, Moneda, CtaTexto, ImagenUrl, Activo
FROM dbo.Microservicios
ORDER BY Id;
GO
