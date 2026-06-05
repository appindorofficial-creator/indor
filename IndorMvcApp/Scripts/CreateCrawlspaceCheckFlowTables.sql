/*
  Crawlspace Check flow — landing + solicitudes for Home Care Priorities.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.CrawlspaceCheckServicioLanding', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[CrawlspaceCheckServicioLanding](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PageTitle] [nvarchar](80) NOT NULL CONSTRAINT [DF_CrawlspaceCheckServicioLanding_PageTitle] DEFAULT (N'Crawlspace Check'),
        [LandingTitulo] [nvarchar](120) NOT NULL,
        [LandingTagline] [nvarchar](200) NULL,
        [LandingSubtitulo] [nvarchar](400) NOT NULL,
        [ImagenUrl] [nvarchar](300) NULL,
        [PrecioDesde] [decimal](10,2) NOT NULL CONSTRAINT [DF_CrawlspaceCheckServicioLanding_PrecioDesde] DEFAULT (89),
        [PrecioTexto] [nvarchar](120) NULL,
        [IncluyeItems] [nvarchar](500) NULL,
        [IncluyeIconos] [nvarchar](300) NULL,
        [PreocupacionItems] [nvarchar](500) NULL,
        [PreocupacionIconos] [nvarchar](300) NULL,
        [InfoBoxTexto] [nvarchar](400) NULL,
        [ResumenServicioTexto] [nvarchar](300) NULL,
        [CtaTexto] [nvarchar](40) NOT NULL CONSTRAINT [DF_CrawlspaceCheckServicioLanding_CtaTexto] DEFAULT (N'Start crawlspace check'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_CrawlspaceCheckServicioLanding_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_CrawlspaceCheckServicioLanding_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_CrawlspaceCheckServicioLanding] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE INDEX [UX_CrawlspaceCheckServicioLanding_HomeCarePriorityId]
        ON dbo.CrawlspaceCheckServicioLanding ([HomeCarePriorityId]);

    ALTER TABLE [dbo].[CrawlspaceCheckServicioLanding]
        WITH CHECK ADD CONSTRAINT [FK_CrawlspaceCheckServicioLanding_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    PRINT 'Table CrawlspaceCheckServicioLanding created.';
END
GO

IF OBJECT_ID(N'dbo.SolicitudesCrawlspaceCheck', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesCrawlspaceCheck](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HomeCarePriorityId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [Encapsulacion] [nvarchar](10) NULL,
        [Aislamiento] [nvarchar](10) NULL,
        [BarreraVapor] [nvarchar](10) NULL,
        [TipoAcceso] [nvarchar](20) NULL,
        [UltimaRevision] [nvarchar](20) NULL,
        [PreocupacionesSeleccionadas] [nvarchar](200) NULL,
        [TimingPreferido] [nvarchar](20) NULL,
        [RecordatorioAnual] [bit] NOT NULL CONSTRAINT [DF_SolicitudesCrawlspaceCheck_RecordatorioAnual] DEFAULT (0),
        [FechaPreferida] [date] NULL,
        [Notas] [nvarchar](300) NULL,
        [DireccionPropiedad] [nvarchar](300) NULL,
        [PrecioEstimado] [decimal](10,2) NULL,
        [Estado] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesCrawlspaceCheck_Estado] DEFAULT (N'InProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesCrawlspaceCheck_FechaCreacion] DEFAULT (sysdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        [FechaConfirmacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesCrawlspaceCheck] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesCrawlspaceCheck]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCrawlspaceCheck_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesCrawlspaceCheck]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCrawlspaceCheck_HomeCarePriorities]
        FOREIGN KEY([HomeCarePriorityId]) REFERENCES [dbo].[HomeCarePriorities] ([Id]);

    ALTER TABLE [dbo].[SolicitudesCrawlspaceCheck]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesCrawlspaceCheck_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesCrawlspaceCheck_UserId_Priority_Estado]
        ON [dbo].[SolicitudesCrawlspaceCheck] ([UserId], [HomeCarePriorityId], [Estado]);

    PRINT 'Table SolicitudesCrawlspaceCheck created.';
END
GO

DECLARE @CsPriorityId INT = (
    SELECT TOP 1 Id FROM dbo.HomeCarePriorities
    WHERE Nombre = N'Crawlspace check'
    ORDER BY Id
);

IF @CsPriorityId IS NOT NULL
BEGIN
    UPDATE dbo.HomeCarePriorities
    SET LinkController = N'CrawlspaceCheck',
        LinkAction = N'CrawlspaceCheckService',
        LinkUrl = NULL,
        ImagenUrl = CASE
            WHEN ImagenUrl IS NULL OR ImagenUrl = N'/inspeccion3.jpeg' THEN N'/priority-crawlspace-check.png'
            ELSE ImagenUrl
        END
    WHERE Id = @CsPriorityId;

    UPDATE dbo.CrawlspaceCheckServicioLanding
    SET ImagenUrl = N'/priority-crawlspace-check.png'
    WHERE HomeCarePriorityId = @CsPriorityId
      AND (ImagenUrl IS NULL OR ImagenUrl = N'/inspeccion3.jpeg');

    IF NOT EXISTS (SELECT 1 FROM dbo.CrawlspaceCheckServicioLanding WHERE HomeCarePriorityId = @CsPriorityId)
    BEGIN
        INSERT INTO dbo.CrawlspaceCheckServicioLanding (
            HomeCarePriorityId, PageTitle, LandingTitulo, LandingTagline, LandingSubtitulo, ImagenUrl,
            PrecioDesde, PrecioTexto, IncluyeItems, IncluyeIconos, PreocupacionItems, PreocupacionIconos,
            InfoBoxTexto, ResumenServicioTexto, CtaTexto, Activo)
        VALUES (
            @CsPriorityId,
            N'Crawlspace Check',
            N'Crawlspace Check',
            N'Recommended yearly',
            N'Inspect moisture, insulation, structure, and air quality before small issues become expensive repairs.',
            N'/priority-crawlspace-check.png',
            89,
            N'From $89',
            N'Moisture|Encapsulation|Insulation|Air leaks|Pests|Cracks',
            N'fa-droplet|fa-layer-group|fa-scroll|fa-wind|fa-bug|fa-bolt',
            N'Standing water|Musty odor|Mold / mildew|Air leaks|Pest signs|Cracks|Pipe leaks|Damaged ducts',
            N'fa-water|fa-wind|fa-bacteria|fa-wind|fa-bug|fa-bolt|fa-faucet-drip|fa-fan',
            N'Check yearly and after heavy rain or moisture events.',
            N'We''ll help inspect moisture, air leaks, insulation, pests, and structural warning signs.',
            N'Start crawlspace check',
            1);
        PRINT 'CrawlspaceCheckServicioLanding seeded for Crawlspace check.';
    END
END
ELSE
BEGIN
    PRINT 'HomeCarePriorities row "Crawlspace check" not found — run CreateHomeCarePrioritiesTables.sql first.';
END
GO

PRINT 'Crawlspace Check flow schema ready.';
GO
