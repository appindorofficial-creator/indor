/*
  Roof Inspection flow — landing + solicitudes for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.RoofInspectionServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RoofInspectionServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_RoofInspectionServicioLanding_PageTitle] DEFAULT (N'Roof Inspection'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](500) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_RoofInspectionServicioLanding_PrecioDesde] DEFAULT (99),
        [PrecioTexto] [nvarchar](120) NULL,
        [RecomendacionItems] [nvarchar](500) NULL,
        [RecomendacionIconos] [nvarchar](300) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_RoofInspectionServicioLanding_CtaTexto] DEFAULT (N'Set roof check'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_RoofInspectionServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_RoofInspectionServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_RoofInspectionServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_RoofInspectionServicioLanding_HomeCarePriorityId]
        ON dbo.RoofInspectionServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[RoofInspectionServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_RoofInspectionServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table RoofInspectionServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesRoofInspection', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesRoofInspection](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoTecho] [nvarchar](20) NULL,
        [EdadTecho] [nvarchar](20) NULL,
        [UltimaInspeccion] [nvarchar](20) NULL,
        [TipoServicio] [nvarchar](20) NULL,
        [Frecuencia] [nvarchar](20) NULL,
        [TimingPreferido] [nvarchar](20) NULL,
        [FechaPreferida] [date] NULL,
        [Notas] [nvarchar](300) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesRoofInspection_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesRoofInspection_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesRoofInspection] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesRoofInspection]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRoofInspection_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesRoofInspection]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRoofInspection_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesRoofInspection]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRoofInspection_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesRoofInspection_UserId_Priority_Estado]
        ON [dbo].[SolicitudesRoofInspection] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesRoofInspection created.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosRoofInspection', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosRoofInspection](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudRoofInspectionId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [TipoContenido] [nvarchar](100) NULL,
        [TamanoBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL CONSTRAINT [DF_ArchivosRoofInspection_FechaSubida] DEFAULT (sysdatetime()),
        CONSTRAINT [PK_ArchivosRoofInspection] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosRoofInspection]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosRoofInspection_SolicitudesRoofInspection]
        FOREIGN KEY([SolicitudRoofInspectionId]) REFERENCES [dbo].[SolicitudesRoofInspection] ([Id]) ON DELETE CASCADE;

    PRINT 'Table ArchivosRoofInspection created.';
END
GO

DECLARE @RoofPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Roof inspection'
    ORDER BY Id
);

IF @RoofPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'RoofInspection',
        LinkAction = N'RoofInspectionService',
        LinkUrl = NULL,
        ImagenUrl = CASE
            WHEN ImagenUrl IS NULL OR ImagenUrl IN (N'/inspeccion8.jpeg', N'/inspeccion7.jpeg') THEN N'/priority-roof-inspection.png'
            ELSE ImagenUrl
        END
    WHERE Id = @RoofPriorityId;

    UPDATE dbo.RoofInspectionServicioLanding
    SET ImagenUrl = N'/priority-roof-inspection.png'
    WHERE HomeCarePriorityId = @RoofPriorityId
      AND (ImagenUrl IS NULL OR ImagenUrl IN (N'/inspeccion8.jpeg', N'/inspeccion7.jpeg'));

    IF NOT EXISTS (SELECT 1 FROM dbo.RoofInspectionServicioLanding WHERE HomeCarePriorityId = @RoofPriorityId)
    BEGIN
        INSERT INTO dbo.RoofInspectionServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, RecomendacionItems, RecomendacionIconos,
            IncluyeItems, IncluyeIconos, InfoBoxTexto, CtaTexto, Activo)
        VALUES (
            @RoofPriorityId,
            N'Roof Inspection',
            N'Roof Inspection',
            N'Regular roof inspections help catch loose shingles, failing sealant, damaged flashing, clogged drainage, and leak risks before they become major repairs.',
            N'/priority-roof-inspection.png',
            99,
            N'From $99',
            N'Visual roof check: spring & fall|Professional inspection: every 1–2 years|After major storms: inspect again|Older roof or active issues: inspect sooner',
            N'fa-calendar|fa-shield-halved|fa-cloud-bolt|fa-clock',
            N'Shingles|Flashing & sealant|Vents / skylights|Gutters & valleys|Attic moisture signs|Debris / branches',
            N'fa-house-chimney|fa-spray-can|fa-wind|fa-water|fa-droplet|fa-leaf',
            N'Vetted professionals. Clear reports. Peace of mind.',
            N'Set roof check',
            1);
        PRINT 'RoofInspectionServicioLanding seeded for Roof inspection.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Roof inspection" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Roof Inspection flow schema ready.';
GO
