/*
  Pest Control Check flow — landing + solicitudes for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.PestControlServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PestControlServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_PestControlServicioLanding_PageTitle] DEFAULT (N'Pest Control Check'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingSubtitulo] [nvarchar](400) NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [WhyItMattersItems] [nvarchar](500) NULL,
        [WhyItMattersIconos] [nvarchar](300) NULL,
        [BestForTexto] [nvarchar](400) NULL,
        [InfoPlanTexto] [nvarchar](400) NULL,
        [WhyYearlyItems] [nvarchar](500) NULL,
        [WhyYearlyIconos] [nvarchar](300) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_PestControlServicioLanding_CtaTexto] DEFAULT (N'Continue'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_PestControlServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PestControlServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PestControlServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_PestControlServicioLanding_HomeCarePriorityId]
        ON dbo.PestControlServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[PestControlServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_PestControlServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table PestControlServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesPestControl', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesPestControl](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [TipoAccionInicial] [nvarchar](20) NULL,
        [UltimoServicio] [nvarchar](20) NULL,
        [SignosSeleccionados] [nvarchar](200) NULL,
        [AreasPreocupacion] [nvarchar](200) NULL,
        [MascotasONinos] [nvarchar](10) NULL,
        [TipoServicio] [nvarchar](20) NULL,
        [TimingPreferido] [nvarchar](20) NULL,
        [RecordatorioAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesPestControl_RecordatorioAnual] DEFAULT (0),
        [Notas] [nvarchar](300) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesPestControl_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesPestControl_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesPestControl] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesPestControl]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPestControl_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesPestControl]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPestControl_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesPestControl]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesPestControl_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesPestControl_UserId_Priority_Estado]
        ON [dbo].[SolicitudesPestControl] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesPestControl created.';
END
GO

DECLARE @PcPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Pest control'
    ORDER BY Id
);

IF @PcPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'PestControl',
        LinkAction = N'PestControlService',
        LinkUrl = NULL
    WHERE Id = @PcPriorityId;

    IF NOT EXISTS (SELECT 1 FROM dbo.PestControlServicioLanding WHERE HomeCarePriorityId = @PcPriorityId)
    BEGIN
        INSERT INTO dbo.PestControlServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingSubtitulo, ImagenUrl,
            WhyItMattersItems, WhyItMattersIconos, BestForTexto, InfoPlanTexto,
            WhyYearlyItems, WhyYearlyIconos, CtaTexto, Activo)
        VALUES (
            @PcPriorityId,
            N'Pest Control Check',
            N'Pest Control Check',
            N'Recommended yearly to help catch problems early and protect your home.',
            N'/priority-pest-control.png',
            N'Spot termites and other pests early|Check for moisture, nests, droppings, and entry points|Help protect wood, insulation, and indoor air quality',
            N'fa-bug|fa-droplet|fa-shield-halved',
            N'Best for: annual inspections, prevention plans, and homes with past pest activity.',
            N'Annual checks are most helpful for homes with past pest activity, moisture issues, wood-to-soil contact, or cracks around the home.',
            N'Helps catch termite or rodent issues early|Checks for moisture, nests, and entry points|Supports ongoing home protection',
            N'fa-bug|fa-droplet|fa-shield-halved',
            N'Continue',
            1);
        PRINT 'PestControlServicioLanding seeded for Pest control.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Pest control" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Pest Control flow schema ready.';
GO
