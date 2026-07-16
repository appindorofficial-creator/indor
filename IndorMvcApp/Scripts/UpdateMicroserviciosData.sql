-- =============================================================
-- Actualiza la tabla Microservicios con los campos completos
-- (subtítulo, descripción larga, "Incluye", CTA, imagen URL,
-- prefijo de precio) y deja los 4 registros con la data
-- definitiva proporcionada por negocio.
--
-- Idempotente: NO borra filas (hay FKs desde SafeAirServicioLanding,
-- LawnServicioLanding, etc.). Usa MERGE por Id.
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
IF COL_LENGTH('dbo.Microservicios', 'NombreEs') IS NULL
	ALTER TABLE dbo.Microservicios ADD NombreEs NVARCHAR(150) NULL;
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

-- 2. Actualizar los 4 microservicios existentes (sin DELETE — evita conflictos de FK).
--    Si falta alguno, se inserta con IDENTITY_INSERT para conservar Id 1–4.
SET IDENTITY_INSERT dbo.Microservicios ON;

MERGE dbo.Microservicios AS t
USING (VALUES
	(1, N'Safe Air 365', N'Aire Seguro 365',
		N'Respira tranquilo. Nosotros nos encargamos.',
		N'Cambio profesional del filtro de aire acondicionado para mejorar la calidad del aire y proteger tu sistema.',
		N'Mantén tu hogar saludable y tu sistema funcionando al 100%. Reemplazamos el filtro de tu aire acondicionado de forma rápida y segura, ayudando a reducir polvo, alergias y fallas costosas.',
		N'Revisión básica del sistema|Cambio de filtro|Instalación profesional',
		N'Cada 3 meses', 49.00, N'USD', N'Desde', N'Agendar servicio', N'/aire.jpeg'),
	(2, N'Always Perfect Lawn', N'Césped Siempre Perfecto',
		N'Tu casa se ve mejor… sin esfuerzo.',
		N'Servicio de corte de césped y mantenimiento básico para mantener tu propiedad impecable.',
		N'Nos encargamos de que tu jardín siempre luzca limpio, ordenado y profesional. Ideal para mantener el valor de tu propiedad sin perder tiempo.',
		N'Corte de césped|Limpieza básica de áreas verdes|Recolección de residuos',
		N'Semanal o quincenal', 45.00, N'USD', N'Desde', N'Reservar mantenimiento', N'/cesped.jpeg'),
	(3, N'Stress-Free Trash', N'Basura Sin Estrés',
		N'Nunca olvides el día de recolección.',
		N'Sacamos tu basura por ti y devolvemos los contenedores. Simple, automático y sin preocupaciones.',
		N'Olvídate de multas, malos olores o descuidos. Nos aseguramos de que tu basura esté lista en el día correcto y regresamos los contenedores a su lugar.',
		N'Sacar contenedores el día indicado|Retorno a su lugar|Servicio puntual y confiable',
		N'1–2 veces por semana', 29.00, N'USD', N'Mensual', N'Activar servicio', N'/basura.jpeg'),
	(4, N'Cleaning Pro', N'Limpieza Pro',
		N'Limpieza profesional que se nota.',
		N'Servicio de limpieza para mantener tu hogar en perfectas condiciones, sin esfuerzo.',
		N'Disfruta de un hogar limpio, organizado y listo para vivir o alquilar. Nuestro equipo realiza limpiezas detalladas adaptadas a tus necesidades. Opciones: Limpieza estándar o Limpieza profunda.',
		N'Limpieza de superficies|Baños y cocina|Aspirado y trapeado',
		N'Semanal, quincenal o mensual', 99.00, N'USD', N'Desde', N'Agendar limpieza', N'/limpieza.jpeg')
) AS s (Id, Nombre, NombreEs, Subtitulo, Descripcion, DescripcionCompleta, Incluye,
		Frecuencia, Valor, Moneda, PrecioPrefijo, CtaTexto, ImagenUrl)
ON t.Id = s.Id
WHEN MATCHED THEN
	UPDATE SET
		t.Nombre = s.Nombre,
		t.NombreEs = s.NombreEs,
		t.Subtitulo = s.Subtitulo,
		t.Descripcion = s.Descripcion,
		t.DescripcionCompleta = s.DescripcionCompleta,
		t.Incluye = s.Incluye,
		t.Frecuencia = s.Frecuencia,
		t.Valor = s.Valor,
		t.Moneda = s.Moneda,
		t.PrecioPrefijo = s.PrecioPrefijo,
		t.CtaTexto = s.CtaTexto,
		t.ImagenUrl = s.ImagenUrl,
		t.Activo = 1
WHEN NOT MATCHED BY TARGET THEN
	INSERT (Id, Nombre, NombreEs, Subtitulo, Descripcion, DescripcionCompleta, Incluye,
			Frecuencia, Valor, Moneda, PrecioPrefijo, CtaTexto, ImagenUrl, ImagenBase64, Activo)
	VALUES (s.Id, s.Nombre, s.NombreEs, s.Subtitulo, s.Descripcion, s.DescripcionCompleta, s.Incluye,
			s.Frecuencia, s.Valor, s.Moneda, s.PrecioPrefijo, s.CtaTexto, s.ImagenUrl, N'', 1);

SET IDENTITY_INSERT dbo.Microservicios OFF;

DECLARE @maxId INT = (SELECT ISNULL(MAX(Id), 0) FROM dbo.Microservicios);
DBCC CHECKIDENT ('dbo.Microservicios', RESEED, @maxId);
GO


-- 3. Verificación
SELECT Id, Nombre, NombreEs, Subtitulo, Frecuencia, PrecioPrefijo, Valor, Moneda, CtaTexto, ImagenUrl, Activo
FROM dbo.Microservicios
ORDER BY Id;
GO
