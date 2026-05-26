-- =============================================================
-- Script para crear la tabla Microservicios e insertar los
-- registros iniciales (Sacar basura, Filtro de aire, Cortar
-- césped, Limpieza). Cada registro guarda una imagen SVG
-- representativa codificada en Base64 (data URI) para poder
-- usarla directamente en una etiqueta <img src="...">.
--
-- Base de datos: SQL Server / LocalDB
-- =============================================================

IF OBJECT_ID(N'[dbo].[Microservicios]', N'U') IS NOT NULL
BEGIN
	PRINT 'La tabla [dbo].[Microservicios] ya existe. No se recrea.';
END
ELSE
BEGIN
	CREATE TABLE [dbo].[Microservicios]
	(
		[Id]             INT             IDENTITY(1,1) NOT NULL,
		[Nombre]         NVARCHAR(150)   NOT NULL,
		[Descripcion]    NVARCHAR(1000)  NOT NULL,
		[Frecuencia]     NVARCHAR(100)   NOT NULL,   -- Ej: "Semanal", "Mensual", "Trimestral"
		[Valor]          DECIMAL(10,2)   NOT NULL,   -- Precio del servicio
		[Moneda]         NVARCHAR(10)    NOT NULL CONSTRAINT [DF_Microservicios_Moneda] DEFAULT (N'USD'),
		[ImagenBase64]   NVARCHAR(MAX)   NOT NULL,   -- data:image/svg+xml;base64,XXXX
		[Activo]         BIT             NOT NULL CONSTRAINT [DF_Microservicios_Activo] DEFAULT (1),
		[FechaCreacion]  DATETIME2(7)    NOT NULL CONSTRAINT [DF_Microservicios_FechaCreacion] DEFAULT (SYSDATETIME()),

		CONSTRAINT [PK_Microservicios] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	CREATE UNIQUE INDEX [UX_Microservicios_Nombre] ON [dbo].[Microservicios] ([Nombre]);

	PRINT 'Tabla [dbo].[Microservicios] creada correctamente.';
END
GO

-- =============================================================
-- SEED de los 4 microservicios iniciales
-- Las SVG se convierten a Base64 con la función nativa
-- xs:base64Binary de XQuery para no escribir cadenas enormes
-- =============================================================

-- Helper: variables con cada SVG (ASCII puro)
DECLARE @svgBasura       VARBINARY(MAX);
DECLARE @svgFiltro       VARBINARY(MAX);
DECLARE @svgCesped       VARBINARY(MAX);
DECLARE @svgLimpieza     VARBINARY(MAX);

-- 1) Sacar la basura (cubo de basura verde)
SET @svgBasura = CAST('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="128" height="128">'
	+ '<rect x="26" y="6" width="12" height="6" rx="2" fill="#15803d"/>'
	+ '<rect x="8" y="12" width="48" height="8" rx="2" fill="#15803d"/>'
	+ '<path d="M12 20 L52 20 L48 58 L16 58 Z" fill="#16a34a"/>'
	+ '<line x1="24" y1="26" x2="24" y2="52" stroke="#ffffff" stroke-width="2"/>'
	+ '<line x1="32" y1="26" x2="32" y2="52" stroke="#ffffff" stroke-width="2"/>'
	+ '<line x1="40" y1="26" x2="40" y2="52" stroke="#ffffff" stroke-width="2"/>'
	+ '</svg>' AS VARBINARY(MAX));

-- 2) Cambio de filtro de aire (filtro azul con rejilla)
SET @svgFiltro = CAST('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="128" height="128">'
	+ '<rect x="6" y="10" width="52" height="44" rx="4" fill="#1d4ed8"/>'
	+ '<rect x="10" y="14" width="44" height="36" fill="#dbeafe"/>'
	+ '<line x1="10" y1="22" x2="54" y2="22" stroke="#1d4ed8" stroke-width="2"/>'
	+ '<line x1="10" y1="30" x2="54" y2="30" stroke="#1d4ed8" stroke-width="2"/>'
	+ '<line x1="10" y1="38" x2="54" y2="38" stroke="#1d4ed8" stroke-width="2"/>'
	+ '<line x1="10" y1="46" x2="54" y2="46" stroke="#1d4ed8" stroke-width="2"/>'
	+ '<circle cx="48" cy="14" r="6" fill="#22c55e"/>'
	+ '<path d="M45 14 L47.5 16.5 L51 12.5" stroke="#ffffff" stroke-width="2" fill="none" stroke-linecap="round"/>'
	+ '</svg>' AS VARBINARY(MAX));

-- 3) Cortar el césped (cortacésped rojo sobre pasto verde)
SET @svgCesped = CAST('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="128" height="128">'
	+ '<rect x="0" y="50" width="64" height="14" fill="#16a34a"/>'
	+ '<path d="M6 56 L6 50 M12 56 L12 48 M18 56 L18 50 M24 56 L24 48 M30 56 L30 50 M36 56 L36 48 M42 56 L42 50 M48 56 L48 48 M54 56 L54 50 M60 56 L60 48" stroke="#15803d" stroke-width="2"/>'
	+ '<rect x="10" y="34" width="38" height="14" rx="2" fill="#dc2626"/>'
	+ '<rect x="14" y="24" width="30" height="12" rx="2" fill="#1f2937"/>'
	+ '<path d="M44 30 L58 14" stroke="#1f2937" stroke-width="3" fill="none" stroke-linecap="round"/>'
	+ '<circle cx="16" cy="52" r="5" fill="#111827"/>'
	+ '<circle cx="42" cy="52" r="5" fill="#111827"/>'
	+ '<circle cx="16" cy="52" r="2" fill="#9ca3af"/>'
	+ '<circle cx="42" cy="52" r="2" fill="#9ca3af"/>'
	+ '</svg>' AS VARBINARY(MAX));

-- 4) Limpieza (escoba con burbujas azules)
SET @svgLimpieza = CAST('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="128" height="128">'
	+ '<rect x="22" y="6" width="6" height="34" rx="2" fill="#92400e" transform="rotate(-15 25 23)"/>'
	+ '<path d="M12 44 L40 44 L46 60 L8 60 Z" fill="#0ea5e9"/>'
	+ '<line x1="16" y1="48" x2="14" y2="58" stroke="#ffffff" stroke-width="2"/>'
	+ '<line x1="22" y1="48" x2="22" y2="58" stroke="#ffffff" stroke-width="2"/>'
	+ '<line x1="28" y1="48" x2="30" y2="58" stroke="#ffffff" stroke-width="2"/>'
	+ '<line x1="34" y1="48" x2="38" y2="58" stroke="#ffffff" stroke-width="2"/>'
	+ '<circle cx="50" cy="18" r="6" fill="#bae6fd" opacity="0.85"/>'
	+ '<circle cx="56" cy="30" r="4" fill="#bae6fd" opacity="0.75"/>'
	+ '<circle cx="48" cy="34" r="3" fill="#bae6fd" opacity="0.7"/>'
	+ '</svg>' AS VARBINARY(MAX));

-- Convertir cada SVG a Base64 con XQuery
DECLARE @b64Basura   NVARCHAR(MAX) = CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@svgBasura"))',   'NVARCHAR(MAX)');
DECLARE @b64Filtro   NVARCHAR(MAX) = CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@svgFiltro"))',   'NVARCHAR(MAX)');
DECLARE @b64Cesped   NVARCHAR(MAX) = CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@svgCesped"))',   'NVARCHAR(MAX)');
DECLARE @b64Limpieza NVARCHAR(MAX) = CAST(N'' AS XML).value('xs:base64Binary(sql:variable("@svgLimpieza"))', 'NVARCHAR(MAX)');

-- Prefijo data URI para usar directamente en <img src="...">
DECLARE @prefix NVARCHAR(50) = N'data:image/svg+xml;base64,';

-- Insertar solo si aún no existen (idempotente por Nombre)
MERGE INTO [dbo].[Microservicios] AS T
USING (VALUES
	(N'Sacar la basura',
	 N'Sacamos los contenedores de tu casa la noche anterior y los devolvemos a su lugar después de la recolección municipal.',
	 N'Semanal',
	 CAST(15.00 AS DECIMAL(10,2)),
	 @prefix + @b64Basura),

	(N'Cambio de filtro de aire',
	 N'Reemplazo profesional del filtro del aire acondicionado para mejorar la calidad del aire y la eficiencia del sistema HVAC.',
	 N'Trimestral',
	 CAST(35.00 AS DECIMAL(10,2)),
	 @prefix + @b64Filtro),

	(N'Cortar el césped',
	 N'Servicio de jardinería que incluye corte de césped, perfilado de bordes y limpieza de recortes en el patio.',
	 N'Quincenal',
	 CAST(45.00 AS DECIMAL(10,2)),
	 @prefix + @b64Cesped),

	(N'Limpieza',
	 N'Limpieza profunda del hogar: cocina, baños, pisos, polvo y desinfección de superficies de alto contacto.',
	 N'Mensual',
	 CAST(120.00 AS DECIMAL(10,2)),
	 @prefix + @b64Limpieza)
) AS S (Nombre, Descripcion, Frecuencia, Valor, ImagenBase64)
ON T.Nombre = S.Nombre
WHEN NOT MATCHED THEN
	INSERT (Nombre, Descripcion, Frecuencia, Valor, Moneda, ImagenBase64, Activo)
	VALUES (S.Nombre, S.Descripcion, S.Frecuencia, S.Valor, N'USD', S.ImagenBase64, 1);

PRINT 'Microservicios iniciales insertados (o ya existentes).';
GO

-- Verificación rápida
SELECT Id, Nombre, Frecuencia, Valor, Moneda,
	   LEN(ImagenBase64) AS LongitudImagen,
	   Activo, FechaCreacion
FROM [dbo].[Microservicios]
ORDER BY Id;
GO
