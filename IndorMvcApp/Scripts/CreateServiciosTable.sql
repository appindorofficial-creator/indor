-- =============================================================
-- Tabla Servicios + seed con los 13 servicios de remodelación
-- e instalación. ImagenUrl apunta al archivo cuyo CONTENIDO coincide
-- con el servicio (ver FixServiciosImagenUrl.sql para el inventario).
--
-- Idempotente: puede ejecutarse varias veces.
-- =============================================================

IF OBJECT_ID(N'[dbo].[Servicios]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Servicios]
	(
		[Id]                  INT             IDENTITY(1,1) NOT NULL,
		[Nombre]              NVARCHAR(150)   NOT NULL,
		[Subtitulo]           NVARCHAR(250)   NULL,
		[Descripcion]         NVARCHAR(1000)  NOT NULL,
		[DescripcionCompleta] NVARCHAR(MAX)   NULL,
		[Incluye]             NVARCHAR(MAX)   NULL,
		[Frecuencia]          NVARCHAR(100)   NULL,
		[Valor]               DECIMAL(12,2)   NULL,
		[Moneda]              NVARCHAR(10)    NOT NULL CONSTRAINT [DF_Servicios_Moneda] DEFAULT (N'USD'),
		[PrecioPrefijo]       NVARCHAR(50)    NULL,
		[PrecioTexto]         NVARCHAR(50)    NULL,
		[CtaTexto]            NVARCHAR(80)    NULL,
		[ImagenUrl]           NVARCHAR(300)   NULL,
		[Activo]              BIT             NOT NULL CONSTRAINT [DF_Servicios_Activo] DEFAULT (1),
		[Orden]               INT             NOT NULL CONSTRAINT [DF_Servicios_Orden]  DEFAULT (0),
		[FechaCreacion]       DATETIME2(7)    NOT NULL CONSTRAINT [DF_Servicios_FechaCreacion] DEFAULT (SYSDATETIME()),

		CONSTRAINT [PK_Servicios] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	CREATE UNIQUE INDEX [UX_Servicios_Nombre] ON [dbo].[Servicios] ([Nombre]);

	PRINT 'Tabla [dbo].[Servicios] creada.';
END
ELSE
BEGIN
	PRINT 'La tabla [dbo].[Servicios] ya existe.';
END
GO

-- Seed: limpiar y reinsertar
DELETE FROM dbo.Servicios;
DBCC CHECKIDENT ('dbo.Servicios', RESEED, 0);
GO

INSERT INTO dbo.Servicios
	(Nombre, Subtitulo, Descripcion, DescripcionCompleta, Incluye,
	 Frecuencia, Valor, Moneda, PrecioPrefijo, PrecioTexto, CtaTexto, ImagenUrl, Orden, Activo)
VALUES
(N'Cocina de Ensueño',
 N'Transforma el corazón de tu hogar.',
 N'Remodelación completa de cocina con diseño moderno y funcional.',
 N'Renovamos tu cocina para mejorar estética, funcionalidad y valor de tu propiedad.',
 N'Diseño personalizado|Instalación de gabinetes|Acabados modernos',
 N'Proyecto único', 5000.00, N'USD', N'Desde', NULL, N'Diseñar mi cocina',
 N'/servicio1.jpeg', 1, 1),

(N'Baño Moderno Pro',
 N'Comodidad, estilo y valor en un solo espacio.',
 N'Remodelación completa de baños con acabados de alta calidad.',
 N'Transformamos tu baño en un espacio moderno, funcional y atractivo.',
 N'Instalación de sanitarios|Duchas modernas|Acabados premium',
 N'Proyecto único', 3500.00, N'USD', N'Desde', NULL, N'Renovar baño',
 N'/servicio4.jpeg', 2, 1),

(N'Renovación Interior Total',
 N'Dale nueva vida a tu hogar.',
 N'Remodelación de espacios interiores para mayor confort y estilo.',
 N'Actualizamos tus espacios interiores con soluciones modernas y funcionales.',
 N'Rediseño de espacios|Mejoras estructurales|Acabados interiores',
 N'Proyecto único', 2500.00, N'USD', N'Desde', NULL, N'Transformar interior',
 N'/servicio3.jpeg', 3, 1),

(N'Expansión de Espacios',
 N'Más espacio, más valor.',
 N'Construcción de extensiones para aumentar el tamaño de tu propiedad.',
 N'Creamos nuevos espacios adaptados a tus necesidades: habitaciones, oficinas o áreas sociales.',
 N'Diseño estructural|Construcción completa|Integración con la vivienda',
 N'Proyecto único', NULL, N'USD', NULL, N'Personalizado', N'Ampliar mi hogar',
 N'/servicio2.jpeg', 4, 1),

(N'Exteriores que Impactan',
 N'La primera impresión lo es todo.',
 N'Remodelación exterior para mejorar estética y valor.',
 N'Mejoramos la apariencia externa de tu propiedad para aumentar su atractivo y valor en el mercado.',
 N'Fachadas|Pintura exterior|Detalles decorativos',
 N'Proyecto único', 2000.00, N'USD', N'Desde', NULL, N'Mejorar exterior',
 N'/servicio5.jpeg', 5, 1),

(N'Terraza Perfecta',
 N'Disfruta tu hogar al aire libre.',
 N'Diseño y construcción de terrazas funcionales y modernas.',
 N'Creamos espacios exteriores ideales para relajarte o compartir con familia y amigos.',
 N'Diseño personalizado|Construcción completa|Acabados resistentes',
 N'Proyecto único', 3000.00, N'USD', N'Desde', NULL, N'Construir terraza',
 N'/servicio6.jpeg', 6, 1),

(N'Instalación Aire Acondicionado',
 N'Confort todo el año.',
 N'Instalación profesional de sistemas HVAC.',
 N'Instalamos sistemas de aire acondicionado eficientes para mejorar tu comodidad y ahorro energético.',
 N'Instalación completa|Configuración|Prueba de funcionamiento',
 N'Servicio único', 1500.00, N'USD', N'Desde', NULL, N'Instalar sistema',
 N'/servicio13.jpeg', 7, 1),

(N'Calentador de Agua Pro',
 N'Agua caliente sin fallas.',
 N'Instalación y reemplazo de calentadores de agua.',
 N'Asegura un suministro confiable de agua caliente con instalación profesional.',
 N'Instalación segura|Pruebas de funcionamiento|Asesoría técnica',
 N'Servicio único', 900.00, N'USD', N'Desde', NULL, N'Instalar calentador',
 N'/servicio7.jpeg', 8, 1),

(N'Pisos Perfectos',
 N'Cada paso cuenta.',
 N'Instalación y remodelación de pisos.',
 N'Renovamos tus pisos con materiales modernos y duraderos.',
 N'Instalación profesional|Nivelación|Acabados de calidad',
 N'Proyecto único', 1800.00, N'USD', N'Desde', NULL, N'Renovar pisos',
 N'/servicio8.jpeg', 9, 1),

(N'Pintura Interior Profesional',
 N'Renueva sin remodelar.',
 N'Pintura interior para transformar espacios rápidamente.',
 N'Dale un nuevo estilo a tu hogar con acabados limpios y profesionales.',
 NULL,
 N'Servicio único', 800.00, N'USD', N'Desde', NULL, N'Pintar interior',
 N'/servicio11.jpeg', 10, 1),

(N'Pintura Exterior Premium',
 N'Protección y estilo en un solo servicio.',
 N'Pintura exterior resistente y duradera.',
 N'Protege tu propiedad contra el clima mientras mejoras su apariencia.',
 NULL,
 N'Servicio único', 1200.00, N'USD', N'Desde', NULL, N'Pintar exterior',
 N'/servicio10.jpeg', 11, 1),

(N'Seguridad del Hogar',
 N'Protege lo que más importa.',
 N'Instalación de detectores de humo y sistemas básicos de seguridad.',
 N'Aumenta la seguridad de tu hogar con sistemas confiables y certificados.',
 NULL,
 N'Servicio único', 300.00, N'USD', N'Desde', NULL, N'Instalar seguridad',
 N'/priority-smoke-detector.png', 12, 1),

(N'Entrada de Concreto Pro',
 N'Resistencia que se nota.',
 N'Construcción de entradas de vehículos en concreto.',
 N'Diseñamos e instalamos entradas duraderas que mejoran la funcionalidad y estética.',
 NULL,
 N'Proyecto único', 2500.00, N'USD', N'Desde', NULL, N'Construir entrada',
 N'/servicio12.jpeg', 13, 1);
GO

-- Verificación
SELECT Id, Orden, Nombre, Subtitulo, PrecioPrefijo, Valor, PrecioTexto, CtaTexto, ImagenUrl, Activo
FROM dbo.Servicios
ORDER BY Orden;
GO
