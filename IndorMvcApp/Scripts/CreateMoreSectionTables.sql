-- =============================================================
-- Script para la sección "More" del Home: tarjeta de usuario,
-- pagos, suscripciones, historial, utilidades (internet) y soporte.
-- Idempotente: puede ejecutarse varias veces.
-- =============================================================

-- ---------- 0. ApplicationUser: agregar columna FotoUrl ----------
IF COL_LENGTH('dbo.AspNetUsers', 'FotoUrl') IS NULL
BEGIN
	ALTER TABLE dbo.AspNetUsers ADD FotoUrl NVARCHAR(500) NULL;
	PRINT 'Columna FotoUrl agregada a AspNetUsers.';
END
GO

-- ---------- 1. Planes de membresía ----------
IF OBJECT_ID(N'[dbo].[PlanesMembresia]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[PlanesMembresia]
	(
		[Id]              INT IDENTITY(1,1) NOT NULL,
		[Nombre]          NVARCHAR(100)   NOT NULL,
		[Subtitulo]       NVARCHAR(250)   NULL,
		[Descripcion]     NVARCHAR(1000)  NULL,
		[PrecioMensual]   DECIMAL(12,2)   NOT NULL,
		[Moneda]          NVARCHAR(10)    NOT NULL CONSTRAINT [DF_PlanesMembresia_Moneda] DEFAULT (N'USD'),
		[Caracteristicas] NVARCHAR(MAX)   NULL,
		[Orden]           INT             NOT NULL CONSTRAINT [DF_PlanesMembresia_Orden] DEFAULT (0),
		[Activo]          BIT             NOT NULL CONSTRAINT [DF_PlanesMembresia_Activo] DEFAULT (1),
		[Recomendado]     BIT             NOT NULL CONSTRAINT [DF_PlanesMembresia_Recomendado] DEFAULT (0),
		CONSTRAINT [PK_PlanesMembresia] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
	CREATE UNIQUE INDEX [UX_PlanesMembresia_Nombre] ON [dbo].[PlanesMembresia] ([Nombre]);
END
GO

DELETE FROM dbo.PlanesMembresia;
DBCC CHECKIDENT ('dbo.PlanesMembresia', RESEED, 0);
GO

INSERT INTO dbo.PlanesMembresia (Nombre, Subtitulo, Descripcion, PrecioMensual, Moneda, Caracteristicas, Orden, Activo, Recomendado)
VALUES
(N'Plan Básico', N'Cuida lo esencial de tu hogar',
 N'Acceso a microservicios básicos y notificaciones de mantenimiento.',
 9.99, N'USD',
 N'1 inspección express al año|Notificaciones de garantías|Soporte por chat',
 1, 1, 0),
(N'Plan Mensual', N'Tu hogar siempre protegido',
 N'Cobertura mensual con descuentos en servicios e inspecciones.',
 19.99, N'USD',
 N'2 microservicios al mes|10% de descuento en servicios|Inspección anual incluida|Soporte prioritario',
 2, 1, 1),
(N'Plan Premium', N'Todo incluido para tu hogar',
 N'Plan completo con inspecciones trimestrales y mantenimiento preventivo.',
 39.99, N'USD',
 N'4 microservicios al mes|20% de descuento en servicios|Inspecciones trimestrales|Atención 24/7',
 3, 1, 0);
GO

-- ---------- 2. Membresía del usuario ----------
IF OBJECT_ID(N'[dbo].[MembresiasUsuario]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[MembresiasUsuario]
	(
		[Id]              INT IDENTITY(1,1) NOT NULL,
		[UserId]          NVARCHAR(450)   NOT NULL,
		[PlanMembresiaId] INT             NOT NULL,
		[FechaInicio]     DATETIME2(7)    NOT NULL CONSTRAINT [DF_MembresiasUsuario_FechaInicio] DEFAULT (SYSDATETIME()),
		[FechaFin]        DATETIME2(7)    NULL,
		[Activa]          BIT             NOT NULL CONSTRAINT [DF_MembresiasUsuario_Activa] DEFAULT (1),
		CONSTRAINT [PK_MembresiasUsuario] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_MembresiasUsuario_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
		CONSTRAINT [FK_MembresiasUsuario_PlanesMembresia] FOREIGN KEY ([PlanMembresiaId]) REFERENCES dbo.PlanesMembresia(Id)
	);
	CREATE INDEX [IX_MembresiasUsuario_UserId] ON [dbo].[MembresiasUsuario] ([UserId]);
END
GO

-- ---------- 3. Métodos de pago ----------
IF OBJECT_ID(N'[dbo].[MetodosPago]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[MetodosPago]
	(
		[Id]              INT IDENTITY(1,1) NOT NULL,
		[UserId]          NVARCHAR(450)   NOT NULL,
		[Tipo]            NVARCHAR(30)    NOT NULL CONSTRAINT [DF_MetodosPago_Tipo] DEFAULT (N'Tarjeta'),
		[Marca]           NVARCHAR(30)    NULL,
		[Ultimos4]        NVARCHAR(10)    NULL,
		[Titular]         NVARCHAR(100)   NULL,
		[Expiracion]      NVARCHAR(7)     NULL,
		[EsPredeterminado] BIT            NOT NULL CONSTRAINT [DF_MetodosPago_EsPredeterminado] DEFAULT (0),
		[Activo]          BIT             NOT NULL CONSTRAINT [DF_MetodosPago_Activo] DEFAULT (1),
		[FechaCreacion]   DATETIME2(7)    NOT NULL CONSTRAINT [DF_MetodosPago_FechaCreacion] DEFAULT (SYSDATETIME()),
		CONSTRAINT [PK_MetodosPago] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_MetodosPago_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
	);
	CREATE INDEX [IX_MetodosPago_UserId] ON [dbo].[MetodosPago] ([UserId]);
END
GO

-- ---------- 4. Pagos ----------
IF OBJECT_ID(N'[dbo].[Pagos]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[Pagos]
	(
		[Id]               INT IDENTITY(1,1) NOT NULL,
		[UserId]           NVARCHAR(450)   NOT NULL,
		[Concepto]         NVARCHAR(200)   NOT NULL,
		[Monto]            DECIMAL(12,2)   NOT NULL,
		[Moneda]           NVARCHAR(10)    NOT NULL CONSTRAINT [DF_Pagos_Moneda] DEFAULT (N'USD'),
		[Estado]           NVARCHAR(30)    NOT NULL CONSTRAINT [DF_Pagos_Estado] DEFAULT (N'Pendiente'),
		[FechaCreacion]    DATETIME2(7)    NOT NULL CONSTRAINT [DF_Pagos_FechaCreacion] DEFAULT (SYSDATETIME()),
		[FechaVencimiento] DATETIME2(7)    NULL,
		[FechaPago]        DATETIME2(7)    NULL,
		[MetodoPagoId]     INT             NULL,
		[Cuotas]           INT             NULL,
		[CuotasPagadas]    INT             NULL,
		CONSTRAINT [PK_Pagos] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_Pagos_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
		CONSTRAINT [FK_Pagos_MetodosPago] FOREIGN KEY ([MetodoPagoId]) REFERENCES dbo.MetodosPago(Id)
	);
	CREATE INDEX [IX_Pagos_UserId] ON [dbo].[Pagos] ([UserId]);
END
GO

-- ---------- 5. Planes de internet (catálogo de mercado) ----------
IF OBJECT_ID(N'[dbo].[PlanesInternet]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[PlanesInternet]
	(
		[Id]                       INT IDENTITY(1,1) NOT NULL,
		[Proveedor]                NVARCHAR(100)  NOT NULL,
		[Nombre]                   NVARCHAR(150)  NOT NULL,
		[VelocidadDescargaMbps]    INT            NOT NULL,
		[VelocidadSubidaMbps]      INT            NOT NULL,
		[PrecioMensual]            DECIMAL(12,2)  NOT NULL,
		[Moneda]                   NVARCHAR(10)   NOT NULL CONSTRAINT [DF_PlanesInternet_Moneda] DEFAULT (N'USD'),
		[Caracteristicas]          NVARCHAR(500)  NULL,
		[EsPlanActual]             BIT            NOT NULL CONSTRAINT [DF_PlanesInternet_EsPlanActual] DEFAULT (0),
		[Activo]                   BIT            NOT NULL CONSTRAINT [DF_PlanesInternet_Activo] DEFAULT (1),
		[Orden]                    INT            NOT NULL CONSTRAINT [DF_PlanesInternet_Orden] DEFAULT (0),
		CONSTRAINT [PK_PlanesInternet] PRIMARY KEY CLUSTERED ([Id] ASC)
	);
END
GO

DELETE FROM dbo.PlanesInternet;
DBCC CHECKIDENT ('dbo.PlanesInternet', RESEED, 0);
GO

INSERT INTO dbo.PlanesInternet
	(Proveedor, Nombre, VelocidadDescargaMbps, VelocidadSubidaMbps, PrecioMensual, Moneda, Caracteristicas, EsPlanActual, Activo, Orden)
VALUES
(N'Comcast Xfinity', N'Internet 300 Mbps', 300, 20, 65.00, N'USD',
 N'Wi-Fi básico|Sin contrato|Datos limitados a 1.2 TB', 1, 1, 1),
(N'AT&T Fiber',      N'Fiber 500',          500, 500, 70.00, N'USD',
 N'Fibra simétrica|Datos ilimitados|Wi-Fi 6 incluido', 0, 1, 2),
(N'Verizon Fios',    N'Fios 1 Gig',         1000, 1000, 89.99, N'USD',
 N'Fibra simétrica|Datos ilimitados|Router incluido', 0, 1, 3),
(N'T-Mobile Home',   N'Home Internet 5G',   245, 31, 50.00, N'USD',
 N'5G inalámbrico|Sin contrato|Datos ilimitados', 0, 1, 4),
(N'Spectrum',        N'Internet Ultra',     500, 20, 69.99, N'USD',
 N'Wi-Fi gratis 12 meses|Datos ilimitados|Sin contrato', 0, 1, 5);
GO

-- ---------- 6. Historial de servicios ----------
IF OBJECT_ID(N'[dbo].[HistorialServicios]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[HistorialServicios]
	(
		[Id]         INT IDENTITY(1,1) NOT NULL,
		[UserId]     NVARCHAR(450)   NOT NULL,
		[Tipo]       NVARCHAR(30)    NOT NULL CONSTRAINT [DF_HistorialServicios_Tipo] DEFAULT (N'Microservicio'),
		[ItemId]     INT             NULL,
		[NombreItem] NVARCHAR(200)   NOT NULL,
		[Fecha]      DATETIME2(7)    NOT NULL CONSTRAINT [DF_HistorialServicios_Fecha] DEFAULT (SYSDATETIME()),
		[Estado]     NVARCHAR(50)    NULL,
		[Monto]      DECIMAL(12,2)   NULL,
		[Moneda]     NVARCHAR(10)    NULL,
		[Notas]      NVARCHAR(500)   NULL,
		CONSTRAINT [PK_HistorialServicios] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_HistorialServicios_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
	);
	CREATE INDEX [IX_HistorialServicios_UserId] ON [dbo].[HistorialServicios] ([UserId]);
END
GO

-- ---------- 7. Mensajes de soporte (Chat) ----------
IF OBJECT_ID(N'[dbo].[MensajesSoporte]', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[MensajesSoporte]
	(
		[Id]        INT IDENTITY(1,1) NOT NULL,
		[UserId]    NVARCHAR(450)   NOT NULL,
		[Remitente] NVARCHAR(20)    NOT NULL CONSTRAINT [DF_MensajesSoporte_Remitente] DEFAULT (N'Usuario'),
		[Contenido] NVARCHAR(MAX)   NOT NULL,
		[Fecha]     DATETIME2(7)    NOT NULL CONSTRAINT [DF_MensajesSoporte_Fecha] DEFAULT (SYSDATETIME()),
		[Leido]     BIT             NOT NULL CONSTRAINT [DF_MensajesSoporte_Leido] DEFAULT (0),
		CONSTRAINT [PK_MensajesSoporte] PRIMARY KEY CLUSTERED ([Id] ASC),
		CONSTRAINT [FK_MensajesSoporte_AspNetUsers] FOREIGN KEY ([UserId]) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
	);
	CREATE INDEX [IX_MensajesSoporte_UserId] ON [dbo].[MensajesSoporte] ([UserId]);
END
GO

PRINT 'Estructura para la sección More creada/actualizada correctamente.';
GO
