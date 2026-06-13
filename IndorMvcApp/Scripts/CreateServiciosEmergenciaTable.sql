/*
  ServiciosEmergencia — 24/7 emergency services grid for Home / Services.
  Safe to run multiple times (idempotent seed).

  IMPORTANT: Use dedicated /emergency-*.png or /priority-*.png assets only.
  Do NOT reuse /servicio*.jpeg or /inspeccion*.jpeg (wrong context and duplicates).
*/

IF OBJECT_ID(N'dbo.ServiciosEmergencia', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ServiciosEmergencia](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](80) NOT NULL,
        [TituloEmergencia] [nvarchar](150) NOT NULL,
        [Descripcion] [nvarchar](300) NOT NULL,
        [TiempoLlegadaMinutos] [int] NOT NULL,
        [IconoClase] [nvarchar](50) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [BadgeTexto] [nvarchar](80) NULL,
        [EsPredeterminado] [bit] NOT NULL,
        [Caracteristicas] [nvarchar](500) NULL,
        [IconosCaracteristicas] [nvarchar](200) NULL,
        [CtaTexto] [nvarchar](80) NOT NULL,
        [Activo] [bit] NOT NULL,
        [Orden] [int] NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ServiciosEmergencia] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_TiempoLlegadaMinutos]
        DEFAULT (45) FOR [TiempoLlegadaMinutos];

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_IconoClase]
        DEFAULT (N'fa-droplet') FOR [IconoClase];

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_EsPredeterminado]
        DEFAULT (0) FOR [EsPredeterminado];

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_CtaTexto]
        DEFAULT (N'Request help') FOR [CtaTexto];

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_Activo]
        DEFAULT (1) FOR [Activo];

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_Orden]
        DEFAULT (0) FOR [Orden];

    ALTER TABLE [dbo].[ServiciosEmergencia]
        ADD CONSTRAINT [DF_ServiciosEmergencia_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    CREATE UNIQUE INDEX [UX_ServiciosEmergencia_Nombre]
        ON [dbo].[ServiciosEmergencia] ([Nombre]);

    PRINT 'Table ServiciosEmergencia created.';
END
ELSE
BEGIN
    PRINT 'Table ServiciosEmergencia already exists.';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia)
BEGIN
    INSERT INTO dbo.ServiciosEmergencia
        (Nombre, TituloEmergencia, Descripcion, TiempoLlegadaMinutos, IconoClase, ImagenUrl,
         BadgeTexto, EsPredeterminado, Caracteristicas, IconosCaracteristicas, CtaTexto, Activo, Orden)
    VALUES
    (N'HVAC', N'HVAC Emergency', N'No heat, no cool, or system failure.', 45, N'fa-snowflake', N'/emergency-hvac.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 1),

    (N'Water Heater', N'Water Heater Emergency', N'No hot water, leaks, or pilot issues.', 45, N'fa-fire-flame-simple', N'/emergency-water-heater.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 2),

    (N'Plumbing', N'Plumbing Emergency', N'Leaks, clogs, pipe bursts & more.', 45, N'fa-droplet', N'/emergency-plumbing.png',
     N'Most requested', 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 3),

    (N'Flood', N'Flood Emergency', N'Standing water, basement flooding, and urgent water removal.', 45, N'fa-water', N'/emergency-flood.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 4),

    (N'Electrical', N'Electrical Emergency', N'Power loss, sparks, tripped breakers & more.', 45, N'fa-bolt', N'/emergency-electrical.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 5),

    (N'Roof Leak', N'Roof Leak Emergency', N'Active leaks, storm damage, and urgent patches.', 45, N'fa-house-chimney-crack', N'/emergency-roof-leak.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 6),

    (N'Tree Damage', N'Tree Damage Emergency', N'Fallen trees, limbs on structures, and storm-related hazards.', 45, N'fa-tree', N'/emergency-tree-damage.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 7),

    (N'Smoke Detector', N'Smoke Detector & CO Alert', N'Chirping alarms, dead batteries, missing detectors, and urgent smoke alarm help.', 45, N'fa-bell', N'/emergency-smoke-detector.png',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 8);

    PRINT 'ServiciosEmergencia seed data inserted.';
END
ELSE
BEGIN
    PRINT 'ServiciosEmergencia already has data — seed skipped.';
END
GO

-- Sync images for existing rows
IF EXISTS (SELECT 1 FROM dbo.ServiciosEmergencia)
BEGIN
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-hvac.png'              WHERE Nombre = N'HVAC';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-water-heater.png'     WHERE Nombre = N'Water Heater';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-plumbing.png'         WHERE Nombre = N'Plumbing';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-flood.png'              WHERE Nombre = N'Flood';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-electrical.png'       WHERE Nombre = N'Electrical';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-roof-leak.png'       WHERE Nombre = N'Roof Leak';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-tree-damage.png'     WHERE Nombre = N'Tree Damage';
    UPDATE dbo.ServiciosEmergencia SET ImagenUrl = N'/emergency-smoke-detector.png'  WHERE Nombre = N'Smoke Detector';
    UPDATE dbo.ServiciosEmergencia SET EsPredeterminado = 0;
    PRINT 'ServiciosEmergencia images synced to emergency-*.png assets.';
END
GO
