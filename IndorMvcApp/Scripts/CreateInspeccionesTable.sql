-- =============================================================
-- Tabla Inspecciones + seed con las 15 inspecciones del hogar.
-- Cada inspección usa la imagen cuyo CONTENIDO coincide con el servicio.
-- Ver FixInspeccionesImagenUrl.sql para el inventario completo.
-- Idempotente: puede ejecutarse varias veces.
-- =============================================================

IF OBJECT_ID(N'[dbo].[Inspecciones]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Inspecciones]
	(
		[Id]                  INT             IDENTITY(1,1) NOT NULL,
		[Nombre]              NVARCHAR(150)   NOT NULL,
		[Subtitulo]           NVARCHAR(250)   NULL,
		[Descripcion]         NVARCHAR(1000)  NOT NULL,
		[DescripcionCompleta] NVARCHAR(MAX)   NULL,
		[Incluye]             NVARCHAR(MAX)   NULL,
		[Frecuencia]          NVARCHAR(100)   NULL,
		[Valor]               DECIMAL(12,2)   NULL,
		[Moneda]              NVARCHAR(10)    NOT NULL CONSTRAINT [DF_Inspecciones_Moneda] DEFAULT (N'USD'),
		[PrecioPrefijo]       NVARCHAR(50)    NULL,
		[PrecioTexto]         NVARCHAR(50)    NULL,
		[CtaTexto]            NVARCHAR(80)    NULL,
		[ImagenUrl]           NVARCHAR(300)   NULL,
		[Activo]              BIT             NOT NULL CONSTRAINT [DF_Inspecciones_Activo] DEFAULT (1),
		[Orden]               INT             NOT NULL CONSTRAINT [DF_Inspecciones_Orden]  DEFAULT (0),
		[FechaCreacion]       DATETIME2(7)    NOT NULL CONSTRAINT [DF_Inspecciones_FechaCreacion] DEFAULT (SYSDATETIME()),

		CONSTRAINT [PK_Inspecciones] PRIMARY KEY CLUSTERED ([Id] ASC)
	);

	CREATE UNIQUE INDEX [UX_Inspecciones_Nombre] ON [dbo].[Inspecciones] ([Nombre]);

	PRINT 'Tabla [dbo].[Inspecciones] creada.';
END
ELSE
BEGIN
	PRINT 'La tabla [dbo].[Inspecciones] ya existe.';
END
GO

DELETE FROM dbo.Inspecciones;
DBCC CHECKIDENT ('dbo.Inspecciones', RESEED, 0);
GO

INSERT INTO dbo.Inspecciones
	(Nombre, Subtitulo, Descripcion, DescripcionCompleta, Incluye,
	 Frecuencia, Valor, Moneda, PrecioPrefijo, PrecioTexto, CtaTexto, ImagenUrl, Orden, Activo)
VALUES
(N'Inspección Pre-Compra de Vivienda',
 N'Compra seguro. Evita errores costosos.',
 N'Evaluación completa antes de comprar una propiedad.',
 N'Analizamos el estado real de la vivienda antes de que tomes una decisión. Detectamos problemas ocultos que pueden costarte miles después de la compra.',
 N'Revisión estructural básica|Sistemas mecánicos (HVAC, plomería, electricidad)|Evaluación general del estado|Reporte detallado',
 N'Antes de cada compra', 149.00, N'USD', N'Desde', NULL, N'Agendar inspección',
 N'/inspeccion1.jpeg', 1, 1),

(N'Inspección Completa del Hogar',
 N'Todo en un solo diagnóstico.',
 N'Revisión integral de todos los sistemas del hogar.',
 N'Inspeccionamos cada área clave de la propiedad para darte una visión completa del estado del hogar y ayudarte a prevenir fallas futuras.',
 N'Electricidad|Plomería|HVAC|Estructura general',
 N'Cada 1–2 años', 129.00, N'USD', N'Desde', NULL, N'Solicitar inspección',
 N'/inspeccion2.jpeg', 2, 1),

(N'Inspección Eléctrica',
 N'Evita riesgos invisibles.',
 N'Revisión del sistema eléctrico para garantizar seguridad.',
 N'Detectamos fallas, sobrecargas o instalaciones defectuosas que pueden representar un riesgo para tu hogar y tu familia.',
 N'Panel eléctrico|Cableado|Tomas y conexiones',
 N'Cada 2–3 años', 99.00, N'USD', N'Desde', NULL, N'Revisar sistema eléctrico',
 N'/inspeccion3.jpeg', 3, 1),

(N'Inspección de Plomería',
 N'Evita fugas y gastos innecesarios.',
 N'Evaluación del sistema de agua y drenaje.',
 N'Identificamos fugas, presión inadecuada y posibles daños ocultos que pueden afectar la estructura del hogar.',
 N'Tuberías|Drenajes|Presión de agua',
 N'Cada 1–2 años', 99.00, N'USD', N'Desde', NULL, N'Inspeccionar plomería',
 N'/inspeccion5.jpeg', 4, 1),

(N'Inspección HVAC',
 N'Aire limpio, sistema eficiente.',
 N'Revisión completa del sistema de aire acondicionado.',
 N'Evaluamos el funcionamiento del sistema HVAC para asegurar eficiencia, ahorro energético y confort.',
 N'Unidad de aire|Filtros|Funcionamiento general',
 N'Cada 6–12 meses', 89.00, N'USD', N'Desde', NULL, N'Revisar aire acondicionado',
 N'/inspeccion8.jpeg', 5, 1),

(N'Inspección Estructural',
 N'La base de tu inversión.',
 N'Análisis de la estabilidad y seguridad de la propiedad.',
 N'Evaluamos posibles daños en la estructura que puedan comprometer la seguridad o generar costos mayores en el futuro.',
 N'Fundaciones|Paredes|Techos',
 N'Antes de compra o remodelación', 149.00, N'USD', N'Desde', NULL, N'Evaluar estructura',
 N'/inspeccion6.jpeg', 6, 1),

(N'Inspección de Techos',
 N'Protege tu hogar desde arriba.',
 N'Revisión del estado del techo y posibles filtraciones.',
 N'Detectamos desgaste, daños o filtraciones que puedan afectar la protección de tu hogar.',
 N'Tejas|Sellado|Drenaje',
 N'Cada 1–2 años', 99.00, N'USD', N'Desde', NULL, N'Revisar techo',
 N'/inspeccion7.jpeg', 7, 1),

(N'Inspección de Fundaciones',
 N'Evita problemas estructurales graves.',
 N'Evaluación de la base de la vivienda.',
 N'Identificamos grietas, asentamientos o fallas que pueden comprometer la estabilidad del inmueble.',
 N'Base estructural|Grietas|Nivelación',
 N'Cada 2–3 años', 129.00, N'USD', N'Desde', NULL, N'Revisar fundación',
 N'/inspeccion9.jpeg', 8, 1),

(N'Inspección de Moho y Humedad',
 N'Protege tu salud y tu hogar.',
 N'Detección de humedad y presencia de moho.',
 N'Localizamos problemas de humedad que pueden generar moho y afectar tanto la estructura como la salud.',
 N'Detección de humedad|Evaluación de paredes|Identificación de moho',
 N'Cuando haya señales o cada 2 años', 119.00, N'USD', N'Desde', NULL, N'Detectar humedad',
 N'/inspeccion10.jpeg', 9, 1),

(N'Inspección de Ventanas y Aislamiento',
 N'Ahorra energía sin darte cuenta.',
 N'Evaluación de sellado y eficiencia térmica.',
 N'Revisamos ventanas y aislamiento para evitar fugas de energía y mejorar la eficiencia del hogar.',
 N'Sellado|Aislamiento|Pérdidas térmicas',
 N'Cada 2–3 años', 89.00, N'USD', N'Desde', NULL, N'Evaluar eficiencia',
 N'/inspeccion11.jpeg', 10, 1),

(N'Inspección de Seguridad del Hogar',
 N'Tu familia primero.',
 N'Revisión de sistemas de seguridad y prevención.',
 N'Evaluamos detectores de humo, riesgos potenciales y condiciones que puedan comprometer la seguridad del hogar.',
 N'Detectores|Riesgos básicos|Recomendaciones',
 N'Anual', 79.00, N'USD', N'Desde', NULL, N'Mejorar seguridad',
 N'/priority-smoke-detector.png', 11, 1),

(N'Inspección con Reporte Profesional',
 N'Decisiones con datos reales.',
 N'Informe detallado del estado del hogar.',
 N'Recibe un reporte claro y profesional con hallazgos, fotos y recomendaciones para tomar decisiones inteligentes.',
 N'Reporte digital|Evidencia fotográfica|Recomendaciones',
 N'Cada inspección', 49.00, N'USD', N'Desde', NULL, N'Obtener reporte',
 N'/inspeccion12.jpeg', 12, 1),

(N'Inspección para Inversionistas',
 N'Invierte con inteligencia.',
 N'Evaluación estratégica para compra de propiedades.',
 N'Analizamos propiedades desde un enfoque de inversión para ayudarte a maximizar rentabilidad y reducir riesgos.',
 N'Evaluación general|Riesgos potenciales|Recomendaciones de inversión',
 N'Por propiedad', 149.00, N'USD', N'Desde', NULL, N'Evaluar inversión',
 N'/inspeccion13.jpeg', 13, 1),

(N'Inspección de Problemas Ocultos',
 N'Lo que no ves… es lo más peligroso.',
 N'Detección de fallas invisibles.',
 N'Identificamos daños ocultos que no son visibles a simple vista pero pueden generar costos altos.',
 N'Evaluación profunda|Detección de riesgos|Diagnóstico técnico',
 N'Cuando haya sospechas', 129.00, N'USD', N'Desde', NULL, N'Detectar problemas',
 N'/inspeccion4.jpeg', 14, 1),

(N'Inspección Express',
 N'Rápido, claro y efectivo.',
 N'Revisión rápida de puntos clave del hogar.',
 N'Ideal para decisiones rápidas, evaluamos lo más importante en poco tiempo sin perder precisión.',
 N'Puntos críticos|Evaluación rápida|Recomendaciones básicas',
 N'Según necesidad', 79.00, N'USD', N'Desde', NULL, N'Inspección rápida',
 N'/inspeccion15.jpeg', 15, 1);
GO

-- Verificación
SELECT Id, Orden, Nombre, Subtitulo, PrecioPrefijo, Valor, CtaTexto, ImagenUrl, Activo
FROM dbo.Inspecciones
ORDER BY Orden;
GO
