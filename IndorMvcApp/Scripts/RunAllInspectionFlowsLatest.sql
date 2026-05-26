/*
  =============================================================================
  INDOR — Unified DB scripts for latest inspection flows
  =============================================================================
  Covers:
    1. Plumbing Inspection
    2. HVAC Inspection
    3. Structural Inspection (6-step flow)
    4. Structural ALTER (if table existed from an older version)
    5. Roof Inspection
    6. Mold and Moisture Inspection
    7. Windows and Insulation Inspection
    8. Home Safety Inspection
    9. Investor Inspection

  Safe to run multiple times (idempotent).
  Prerequisites: AspNetUsers, Inspecciones, Propiedades tables must exist.

  Run in SSMS / Azure Data Studio against your Indor database.
  =============================================================================
*/

PRINT '=== 1/9 Plumbing Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionPlomeria', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionPlomeria](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [FugaAguaAhora] [nvarchar](20) NOT NULL,
        [SituacionesActuales] [nvarchar](300) NULL,
        [CuandoEmpezo] [nvarchar](20) NULL,
        [AguaCerrada] [nvarchar](20) NULL,
        [DescripcionProblema] [nvarchar](500) NULL,
        [NotasAdicionales] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionPlomeria] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_TipoProblema]
        DEFAULT (N'KitchenIssue') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_UbicacionProblema]
        DEFAULT (N'Kitchen') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_FugaAguaAhora]
        DEFAULT (N'No') FOR [FugaAguaAhora];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        ADD CONSTRAINT [DF_SolicitudesInspeccionPlomeria_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionPlomeria_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionPlomeria_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionPlomeria_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionPlomeria_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionPlomeria] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionPlomeria created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionPlomeria already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionPlomeria', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionPlomeria](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionPlomeriaId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionPlomeria] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionPlomeria]
        ADD CONSTRAINT [DF_ArchivosInspeccionPlomeria_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionPlomeria]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionPlomeria_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionPlomeriaId]) REFERENCES [dbo].[SolicitudesInspeccionPlomeria] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionPlomeria_SolicitudId]
        ON [dbo].[ArchivosInspeccionPlomeria] ([SolicitudInspeccionPlomeriaId]);

    PRINT 'Table ArchivosInspeccionPlomeria created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionPlomeria already exists.';
END
GO

PRINT '=== 2/9 HVAC Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionHvac', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionHvac](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [ParteAtencion] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [SistemaFuncionando] [nvarchar](20) NOT NULL,
        [TipoEquipo] [nvarchar](30) NULL,
        [CantidadSistemas] [nvarchar](10) NULL,
        [ComponentesRevision] [nvarchar](200) NULL,
        [EdadSistema] [nvarchar](20) NULL,
        [FiltroCambiado] [nvarchar](20) NULL,
        [TipoTermostato] [nvarchar](20) NULL,
        [DescripcionProblema] [nvarchar](500) NULL,
        [NotasOpcionales] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionHvac] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_TipoProblema]
        DEFAULT (N'NotCooling') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_ParteAtencion]
        DEFAULT (N'WholeSystem') FOR [ParteAtencion];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_SistemaFuncionando]
        DEFAULT (N'Yes') FOR [SistemaFuncionando];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHvac_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHvac_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHvac_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHvac_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionHvac_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionHvac] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionHvac created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionHvac already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionHvac', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionHvac](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionHvacId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionHvac] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionHvac]
        ADD CONSTRAINT [DF_ArchivosInspeccionHvac_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionHvac]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionHvac_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionHvacId]) REFERENCES [dbo].[SolicitudesInspeccionHvac] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionHvac_SolicitudId]
        ON [dbo].[ArchivosInspeccionHvac] ([SolicitudInspeccionHvacId]);

    PRINT 'Table ArchivosInspeccionHvac created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionHvac already exists.';
END
GO

PRINT '=== 3/9 Structural Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionStructural', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionStructural](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [MotivoRevision] [nvarchar](40) NOT NULL,
        [TipoPreocupacion] [nvarchar](40) NOT NULL,
        [TiposPreocupacion] [nvarchar](300) NULL,
        [AreaPreocupacion] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [DanoVisible] [nvarchar](20) NOT NULL,
        [SignosVisibles] [nvarchar](300) NULL,
        [SeveridadApariencia] [nvarchar](20) NULL,
        [UbicacionEspecifica] [nvarchar](200) NULL,
        [CuandoNotadoTexto] [nvarchar](50) NULL,
        [DuracionProblema] [nvarchar](20) NULL,
        [Severidad] [nvarchar](20) NULL,
        [ReparacionesPrevias] [nvarchar](20) NULL,
        [CondicionesInseguras] [nvarchar](200) NULL,
        [MejorHorarioVisita] [nvarchar](20) NULL,
        [TipoPropiedad] [nvarchar](30) NULL,
        [TipoFundacion] [nvarchar](20) NULL,
        [TieneReporte] [nvarchar](10) NULL,
        [CambiosRecientes] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](30) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [CuandoNotado] [nvarchar](20) NULL,
        [EdadPropiedad] [nvarchar](20) NULL,
        [RemodelReciente] [nvarchar](20) NULL,
        [DescripcionProblema] [nvarchar](500) NULL,
        [NotasOpcionales] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionStructural] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_MotivoRevision]
        DEFAULT (N'BeforePurchase') FOR [MotivoRevision];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_TipoPreocupacion]
        DEFAULT (N'FoundationCrack') FOR [TipoPreocupacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_AreaPreocupacion]
        DEFAULT (N'Foundation') FOR [AreaPreocupacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_DanoVisible]
        DEFAULT (N'Yes') FOR [DanoVisible];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        ADD CONSTRAINT [DF_SolicitudesInspeccionStructural_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionStructural_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionStructural_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionStructural_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionStructural_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionStructural] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionStructural created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionStructural already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionStructural', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionStructural](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionStructuralId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionStructural] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionStructural]
        ADD CONSTRAINT [DF_ArchivosInspeccionStructural_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionStructural]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionStructural_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionStructuralId]) REFERENCES [dbo].[SolicitudesInspeccionStructural] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionStructural_SolicitudId]
        ON [dbo].[ArchivosInspeccionStructural] ([SolicitudInspeccionStructuralId]);

    PRINT 'Table ArchivosInspeccionStructural created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionStructural already exists.';
END
GO

PRINT '=== 4/9 Structural Inspection — ALTER (legacy tables) ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionStructural', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TiposPreocupacion') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TiposPreocupacion] NVARCHAR(300) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'SignosVisibles') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [SignosVisibles] NVARCHAR(300) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'SeveridadApariencia') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [SeveridadApariencia] NVARCHAR(20) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'UbicacionEspecifica') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [UbicacionEspecifica] NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'CuandoNotadoTexto') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [CuandoNotadoTexto] NVARCHAR(50) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'DuracionProblema') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [DuracionProblema] NVARCHAR(20) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'Severidad') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [Severidad] NVARCHAR(20) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'ReparacionesPrevias') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [ReparacionesPrevias] NVARCHAR(20) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'CondicionesInseguras') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [CondicionesInseguras] NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'MejorHorarioVisita') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [MejorHorarioVisita] NVARCHAR(20) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TipoPropiedad') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TipoPropiedad] NVARCHAR(30) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TipoFundacion') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TipoFundacion] NVARCHAR(20) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TieneReporte') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TieneReporte] NVARCHAR(10) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'CambiosRecientes') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [CambiosRecientes] NVARCHAR(200) NULL;
    IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'AccesoPreferido') IS NULL
        ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [AccesoPreferido] NVARCHAR(30) NULL;

    PRINT 'Structural inspection step columns updated.';
END
ELSE
BEGIN
    PRINT 'SolicitudesInspeccionStructural not found — ALTER skipped.';
END
GO

PRINT '=== 5/9 Roof Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionRoof', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionRoof](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](200) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [MaterialTecho] [nvarchar](20) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionRoof] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_TipoProblema]
        DEFAULT (N'ActiveLeak') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_UbicacionProblema]
        DEFAULT (N'MainRoof') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        ADD CONSTRAINT [DF_SolicitudesInspeccionRoof_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionRoof_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionRoof_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionRoof_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionRoof_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionRoof] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionRoof created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionRoof already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionRoof', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionRoof](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionRoofId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionRoof] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionRoof]
        ADD CONSTRAINT [DF_ArchivosInspeccionRoof_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionRoof]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionRoof_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionRoofId]) REFERENCES [dbo].[SolicitudesInspeccionRoof] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionRoof_SolicitudId]
        ON [dbo].[ArchivosInspeccionRoof] ([SolicitudInspeccionRoofId]);

    PRINT 'Table ArchivosInspeccionRoof created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionRoof already exists.';
END
GO

PRINT '=== 6/9 Mold and Moisture Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionMoldMoisture', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionMoldMoisture](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](300) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [HumedadActiva] [nvarchar](20) NOT NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [UbicacionPrincipal] [nvarchar](30) NULL,
        [IntrusionAguaReciente] [nvarchar](20) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [ComentariosProveedor] [nvarchar](500) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionMoldMoisture] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_TipoProblema]
        DEFAULT (N'VisibleMold') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_UbicacionProblema]
        DEFAULT (N'Bathroom') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_HumedadActiva]
        DEFAULT (N'Yes') FOR [HumedadActiva];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_SolicitudesInspeccionMoldMoisture_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionMoldMoisture_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionMoldMoisture_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionMoldMoisture_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionMoldMoisture_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionMoldMoisture] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionMoldMoisture created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionMoldMoisture already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionMoldMoisture', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionMoldMoisture](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionMoldMoistureId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionMoldMoisture] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionMoldMoisture]
        ADD CONSTRAINT [DF_ArchivosInspeccionMoldMoisture_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionMoldMoisture]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionMoldMoisture_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionMoldMoistureId]) REFERENCES [dbo].[SolicitudesInspeccionMoldMoisture] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionMoldMoisture_SolicitudId]
        ON [dbo].[ArchivosInspeccionMoldMoisture] ([SolicitudInspeccionMoldMoistureId]);

    PRINT 'Table ArchivosInspeccionMoldMoisture created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionMoldMoisture already exists.';
END
GO

PRINT '=== 7/9 Windows and Insulation Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionWindowsInsulation', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionWindowsInsulation](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](300) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [AreasAtencion] [nvarchar](300) NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [DanoHumedadVisible] [nvarchar](20) NOT NULL,
        [MotivosRevision] [nvarchar](200) NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [TipoVentana] [nvarchar](20) NULL,
        [AccesoAtticCrawlSpace] [nvarchar](20) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionWindowsInsulation] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_TipoProblema]
        DEFAULT (N'DraftAir') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_UbicacionProblema]
        DEFAULT (N'LivingRoom') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_DanoHumedadVisible]
        DEFAULT (N'No') FOR [DanoHumedadVisible];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_SolicitudesInspeccionWindowsInsulation_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionWindowsInsulation_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionWindowsInsulation_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionWindowsInsulation_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionWindowsInsulation_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionWindowsInsulation] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionWindowsInsulation created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionWindowsInsulation already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionWindowsInsulation', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionWindowsInsulation](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionWindowsInsulationId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionWindowsInsulation] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionWindowsInsulation]
        ADD CONSTRAINT [DF_ArchivosInspeccionWindowsInsulation_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionWindowsInsulation]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionWindowsInsulation_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionWindowsInsulationId]) REFERENCES [dbo].[SolicitudesInspeccionWindowsInsulation] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionWindowsInsulation_SolicitudId]
        ON [dbo].[ArchivosInspeccionWindowsInsulation] ([SolicitudInspeccionWindowsInsulationId]);

    PRINT 'Table ArchivosInspeccionWindowsInsulation created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionWindowsInsulation already exists.';
END
GO

PRINT '=== 8/9 Home Safety Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionHomeSafety', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionHomeSafety](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TiposProblema] [nvarchar](300) NULL,
        [TipoProblema] [nvarchar](40) NOT NULL,
        [AreasAtencion] [nvarchar](300) NULL,
        [UbicacionProblema] [nvarchar](30) NOT NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [RiesgoActivo] [nvarchar](20) NOT NULL,
        [MotivosRevision] [nvarchar](200) NULL,
        [MotivoRevision] [nvarchar](30) NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [NumeroPisos] [nvarchar](10) NULL,
        [AreasEnfoque] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [OcupantesHogar] [nvarchar](100) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionHomeSafety] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_TipoProblema]
        DEFAULT (N'SmokeDetectorConcern') FOR [TipoProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_UbicacionProblema]
        DEFAULT (N'Hallway') FOR [UbicacionProblema];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_RiesgoActivo]
        DEFAULT (N'No') FOR [RiesgoActivo];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        ADD CONSTRAINT [DF_SolicitudesInspeccionHomeSafety_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHomeSafety_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHomeSafety_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionHomeSafety_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionHomeSafety_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionHomeSafety] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionHomeSafety created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionHomeSafety already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionHomeSafety', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionHomeSafety](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionHomeSafetyId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionHomeSafety] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionHomeSafety]
        ADD CONSTRAINT [DF_ArchivosInspeccionHomeSafety_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionHomeSafety]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionHomeSafety_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionHomeSafetyId]) REFERENCES [dbo].[SolicitudesInspeccionHomeSafety] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionHomeSafety_SolicitudId]
        ON [dbo].[ArchivosInspeccionHomeSafety] ([SolicitudInspeccionHomeSafetyId]);

    PRINT 'Table ArchivosInspeccionHomeSafety created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionHomeSafety already exists.';
END
GO

PRINT '=== 9/9 Investor Inspection ===';
GO

IF OBJECT_ID(N'dbo.SolicitudesInspeccionInvestor', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesInspeccionInvestor](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [InspeccionId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [DireccionPropiedad] [nvarchar](300) NOT NULL,
        [TipoInversion] [nvarchar](30) NOT NULL,
        [EnfoquesInversion] [nvarchar](300) NULL,
        [Urgencia] [nvarchar](20) NOT NULL,
        [TipoPropiedad] [nvarchar](20) NULL,
        [Ocupacion] [nvarchar](20) NULL,
        [NivelRehab] [nvarchar](20) NULL,
        [AreasRevision] [nvarchar](200) NULL,
        [AccesoPreferido] [nvarchar](20) NULL,
        [ComentariosProveedor] [nvarchar](1000) NULL,
        [FechaCitaProgramada] [date] NULL,
        [HoraCitaProgramada] [time](0) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesInspeccionInvestor] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_TipoInversion]
        DEFAULT (N'Flip') FOR [TipoInversion];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_Urgencia]
        DEFAULT (N'Normal') FOR [Urgencia];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_Estado]
        DEFAULT (N'InProgress') FOR [Estado];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        ADD CONSTRAINT [DF_SolicitudesInspeccionInvestor_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionInvestor_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionInvestor_Inspecciones]
        FOREIGN KEY([InspeccionId]) REFERENCES [dbo].[Inspecciones] ([Id]);

    ALTER TABLE [dbo].[SolicitudesInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesInspeccionInvestor_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_SolicitudesInspeccionInvestor_UserId_InspeccionId]
        ON [dbo].[SolicitudesInspeccionInvestor] ([UserId], [InspeccionId], [Estado]);

    PRINT 'Table SolicitudesInspeccionInvestor created.';
END
ELSE
BEGIN
    PRINT 'Table SolicitudesInspeccionInvestor already exists.';
END
GO

IF OBJECT_ID(N'dbo.ArchivosInspeccionInvestor', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ArchivosInspeccionInvestor](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [SolicitudInspeccionInvestorId] [int] NOT NULL,
        [NombreArchivo] [nvarchar](260) NOT NULL,
        [RutaArchivo] [nvarchar](500) NOT NULL,
        [CategoriaArchivo] [nvarchar](20) NULL,
        [TipoArchivo] [nvarchar](20) NULL,
        [TamanioBytes] [bigint] NOT NULL,
        [FechaSubida] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_ArchivosInspeccionInvestor] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ArchivosInspeccionInvestor]
        ADD CONSTRAINT [DF_ArchivosInspeccionInvestor_FechaSubida]
        DEFAULT (sysdatetime()) FOR [FechaSubida];

    ALTER TABLE [dbo].[ArchivosInspeccionInvestor]
        WITH CHECK ADD CONSTRAINT [FK_ArchivosInspeccionInvestor_Solicitudes]
        FOREIGN KEY([SolicitudInspeccionInvestorId]) REFERENCES [dbo].[SolicitudesInspeccionInvestor] ([Id]) ON DELETE CASCADE;

    CREATE NONCLUSTERED INDEX [IX_ArchivosInspeccionInvestor_SolicitudId]
        ON [dbo].[ArchivosInspeccionInvestor] ([SolicitudInspeccionInvestorId]);

    PRINT 'Table ArchivosInspeccionInvestor created.';
END
ELSE
BEGIN
    PRINT 'Table ArchivosInspeccionInvestor already exists.';
END
GO

PRINT '=== All latest inspection flow scripts completed successfully. ===';
GO
