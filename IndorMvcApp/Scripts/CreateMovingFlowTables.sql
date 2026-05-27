/*
  Moving flow — landing content + solicitudes + file uploads.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.MovingServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MovingServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_MovingServicioLanding_PageTitle] DEFAULT (N'Moving Service'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingSubtitulo] [nvarchar](300) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [EstimatedTimeLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_MovingServicioLanding_EstimatedTimeLabel] DEFAULT (N'Estimated time'),
        [EstimatedTimeValue] [nvarchar](60) NOT NULL CONSTRAINT [DF_MovingServicioLanding_EstimatedTimeValue] DEFAULT (N'2-6 hours'),
        [EstimatedTimeNote] [nvarchar](120) NULL,
        [BestForLabel] [nvarchar](60) NOT NULL CONSTRAINT [DF_MovingServicioLanding_BestForLabel] DEFAULT (N'Best for'),
        [BestForValue] [nvarchar](120) NOT NULL,
        [BestForNote] [nvarchar](120) NULL,
        [MoveTypes] [nvarchar](200) NOT NULL,
        [MoveTypeIcons] [nvarchar](200) NULL,
        [MoveTypeValues] [nvarchar](200) NOT NULL,
        [CtaContinueTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_MovingServicioLanding_CtaContinueTexto] DEFAULT (N'Continue'),
        [CtaEstimateTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_MovingServicioLanding_CtaEstimateTexto] DEFAULT (N'Get estimate'),
        [PrecioEstimadoMin] [decimal](10,2) NOT NULL CONSTRAINT [DF_MovingServicioLanding_PrecioEstimadoMin] DEFAULT (420),
        [PrecioEstimadoMax] [decimal](10,2) NOT NULL CONSTRAINT [DF_MovingServicioLanding_PrecioEstimadoMax] DEFAULT (620),
        [DuracionEstimadaMinHoras] [int] NOT NULL CONSTRAINT [DF_MovingServicioLanding_DuracionEstimadaMinHoras] DEFAULT (2),
        [DuracionEstimadaMaxHoras] [int] NOT NULL CONSTRAINT [DF_MovingServicioLanding_DuracionEstimadaMaxHoras] DEFAULT (6),
        [DisclaimerTexto] [nvarchar](300) NULL,
        [Activo] [bit] NOT NULL CONSTRAINT [DF_MovingServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_MovingServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_MovingServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_MovingServicioLanding_MovingSetupServicioId]
        ON dbo.MovingServicioLanding ([MovingSetupServicioId]);

    ALTER TABLE [dbo].[MovingServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_MovingServicioLanding_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    PRINT 'Table MovingServicioLanding created.';
END
ELSE
BEGIN
    PRINT 'Table MovingServicioLanding already exists.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesMoving', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesMoving](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MovingSetupServicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [TipoMovimiento] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesMoving_TipoMovimiento] DEFAULT (N'MoveIn'),
        [TipoPropiedad] [nvarchar](30) NULL,
        [TamanoHogar] [nvarchar](30) NULL,
        [DireccionOrigen] [nvarchar](300) NULL,
        [DireccionDestino] [nvarchar](300) NULL,
        [FechaMovimiento] [date] NULL,
        [VentanaHorario] [nvarchar](60) NULL,
        [TipoServicio] [nvarchar](30) NULL,
        [ItemsMover] [nvarchar](400) NULL,
        [TamanoMovimiento] [nvarchar](30) NULL,
        [CondicionesAcceso] [nvarchar](300) NULL,
        [RequiereMontaje] [nvarchar](10) NULL,
        [NotaCorta] [nvarchar](500) NULL,
        [PrecioEstimadoMin] [decimal](10,2) NULL,
        [PrecioEstimadoMax] [decimal](10,2) NULL,
        [DuracionEstimadaMinHoras] [int] NULL,
        [DuracionEstimadaMaxHoras] [int] NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesMoving_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesMoving_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesMoving] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesMoving]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesMoving_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesMoving]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesMoving_MovingSetupServicios]
        FOREIGN KEY([MovingSetupServicioId]) REFERENCES [dbo].[MovingSetupServicios] ([Id]);

    ALTER TABLE [dbo].[SolicitudesMoving]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesMoving_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesMoving_UserId_Servicio_Estado]
        ON [dbo].[SolicitudesMoving] ([UserId], [MovingSetupServicioId], [Estado]);

    PRINT 'Table SolicitudesMoving created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesMoving already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosMoving', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosMoving](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudMovingId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosMoving_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosMoving] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosMoving]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosMoving_SolicitudesMoving]
        FOREIGN KEY([SolicitudMovingId]) REFERENCES [dbo].[SolicitudesMoving] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ArchivosMoving]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosMoving_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]);

    PRINT 'Table ArchivosMoving created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosMoving already exists.';
END
GO

DECLARE @MovingServicioId INT = (
    SELECT TOP 1 Id FROM dbo.MovingSetupServicios WHERE Nombre = N'Moving' ORDER BY Id
);

IF @MovingServicioId IS NOT NULL
BEGIN
    UPDATE dbo.MovingSetupServicios
    SET LinkController = N'Moving',
        LinkAction = N'MovingService',
        LinkRouteId = Id
    WHERE Id = @MovingServicioId
      AND (LinkController IS NULL OR LinkController = N'MovingSetup');

    IF NOT EXISTS (SELECT 1 FROM dbo.MovingServicioLanding WHERE MovingSetupServicioId = @MovingServicioId)
    BEGIN
        INSERT INTO dbo.MovingServicioLanding (
            MovingSetupServicioId, PageTitle, LandingTitulo, LandingSubtitulo, ImagenUrl,
            IncluyeItems, IncluyeIconos,
            EstimatedTimeLabel, EstimatedTimeValue, EstimatedTimeNote,
            BestForLabel, BestForValue, BestForNote,
            MoveTypes, MoveTypeIcons, MoveTypeValues,
            CtaContinueTexto, CtaEstimateTexto,
            PrecioEstimadoMin, PrecioEstimadoMax,
            DuracionEstimadaMinHoras, DuracionEstimadaMaxHoras,
            DisclaimerTexto, Activo)
        VALUES (
            @MovingServicioId,
            N'Moving Service',
            N'Moving Help',
            N'Book moving help for move-in, move-out, or full relocation.',
            N'/inspeccion2.jpeg',
            N'Loading & unloading|Room-to-room moving|Furniture protection & wrapping|Optional disassembly & reassembly',
            N'fa-box-open|fa-dolly|fa-shield-check|fa-screwdriver-wrench',
            N'Estimated time', N'2-6 hours', N'Based on home size',
            N'Best for', N'Apartment & House Moves', N'Small to large homes',
            N'Move-In|Move-Out|Local Move',
            N'fa-house-circle-check|fa-house-circle-xmark|fa-truck-moving',
            N'MoveIn|MoveOut|LocalMove',
            N'Continue', N'Get estimate',
            420, 620, 2, 6,
            N'Final pricing may vary based on onsite assessment and actual service needs.',
            1);
        PRINT 'MovingServicioLanding seeded for Moving service.';
    END
END
GO

PRINT 'Moving flow schema ready.';
GO
