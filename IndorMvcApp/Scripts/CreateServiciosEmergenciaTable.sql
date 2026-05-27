/*
  ServiciosEmergencia — 24/7 emergency services grid for Home / Services.
  Safe to run multiple times (idempotent seed).
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
    (N'HVAC', N'HVAC Emergency', N'No heat, no cool, or system failure.', 45, N'fa-snowflake', N'/inspeccion5.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 1),

    (N'Water Heater', N'Water Heater Emergency', N'No hot water, leaks, or pilot issues.', 45, N'fa-fire-flame-simple', N'/servicio4.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 2),

    (N'Plumbing', N'Plumbing Emergency', N'Leaks, clogs, pipe bursts & more.', 45, N'fa-droplet', N'/inspeccion4.jpeg',
     N'Most requested', 1, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 3),

    (N'Drain Cleaning', N'Drain Cleaning Emergency', N'Slow drains, backups, and main line clogs.', 45, N'fa-sink', N'/inspeccion4.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 4),

    (N'Electrical', N'Electrical Emergency', N'Power loss, sparks, tripped breakers & more.', 45, N'fa-bolt', N'/inspeccion3.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 5),

    (N'Roof Leak', N'Roof Leak Emergency', N'Active leaks, storm damage, and urgent patches.', 45, N'fa-house-chimney-crack', N'/inspeccion7.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 6),

    (N'Mold Remediation', N'Mold Remediation Emergency', N'Visible mold, moisture, and air quality concerns.', 45, N'fa-biohazard', N'/inspeccion9.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 7),

    (N'Gas Line', N'Gas Line Emergency', N'Gas smell, leaks, or shutoff assistance.', 45, N'fa-fire-flame-curved', N'/inspeccion2.jpeg',
     NULL, 0, N'Arrives fast|Trusted pros|Upfront pricing', N'fa-clock|fa-shield-halved|fa-star', N'Request help', 1, 8);

    PRINT 'ServiciosEmergencia seed data inserted.';
END
ELSE
BEGIN
    PRINT 'ServiciosEmergencia already has data — seed skipped.';
END
GO
