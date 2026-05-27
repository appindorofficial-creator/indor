/*
  My Home — ATTOM columns on Propiedades + history, providers, maintenance, documents.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.Propiedades', N'AttomPropertyId') IS NULL
    ALTER TABLE dbo.Propiedades ADD [AttomPropertyId] [bigint] NULL;
GO
IF COL_LENGTH(N'dbo.Propiedades', N'AttomRawJson') IS NULL
    ALTER TABLE dbo.Propiedades ADD [AttomRawJson] [nvarchar](max) NULL;
GO
IF COL_LENGTH(N'dbo.Propiedades', N'AttomLastSyncUtc') IS NULL
    ALTER TABLE dbo.Propiedades ADD [AttomLastSyncUtc] [datetime2](7) NULL;
GO
IF COL_LENGTH(N'dbo.Propiedades', N'AttomSyncStatus') IS NULL
    ALTER TABLE dbo.Propiedades ADD [AttomSyncStatus] [nvarchar](30) NULL;
GO
IF COL_LENGTH(N'dbo.Propiedades', N'AttomSyncError') IS NULL
    ALTER TABLE dbo.Propiedades ADD [AttomSyncError] [nvarchar](500) NULL;
GO

IF OBJECT_ID(N'dbo.PropiedadProveedores', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PropiedadProveedores](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NOT NULL,
        [Name] [nvarchar](120) NOT NULL,
        [ServiceCategory] [nvarchar](80) NOT NULL,
        [Phone] [nvarchar](30) NULL,
        [Website] [nvarchar](300) NULL,
        [Notes] [nvarchar](500) NULL,
        [Source] [nvarchar](20) NOT NULL CONSTRAINT [DF_PropiedadProveedores_Source] DEFAULT (N'User'),
        [Activo] [bit] NOT NULL CONSTRAINT [DF_PropiedadProveedores_Activo] DEFAULT (1),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PropiedadProveedores_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PropiedadProveedores] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE dbo.PropiedadProveedores
        WITH CHECK ADD CONSTRAINT [FK_PropiedadProveedores_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;

    CREATE INDEX [IX_PropiedadProveedores_PropiedadId] ON dbo.PropiedadProveedores ([PropiedadId]);
    PRINT 'Table PropiedadProveedores created.';
END
GO

IF OBJECT_ID(N'dbo.PropiedadHistorial', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PropiedadHistorial](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NOT NULL,
        [RecordType] [nvarchar](30) NOT NULL,
        [Title] [nvarchar](150) NOT NULL,
        [ProviderName] [nvarchar](120) NULL,
        [PropiedadProveedorId] [int] NULL,
        [CompletionDate] [datetime2](7) NULL,
        [TotalCost] [decimal](12,2) NULL,
        [Description] [nvarchar](1000) NULL,
        [WarrantyStatus] [nvarchar](30) NULL,
        [Source] [nvarchar](20) NOT NULL CONSTRAINT [DF_PropiedadHistorial_Source] DEFAULT (N'User'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PropiedadHistorial_FechaCreacion] DEFAULT (sysutcdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_PropiedadHistorial] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE dbo.PropiedadHistorial
        WITH CHECK ADD CONSTRAINT [FK_PropiedadHistorial_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;

    ALTER TABLE dbo.PropiedadHistorial
        WITH CHECK ADD CONSTRAINT [FK_PropiedadHistorial_PropiedadProveedores]
        FOREIGN KEY([PropiedadProveedorId]) REFERENCES [dbo].[PropiedadProveedores] ([Id]);

    CREATE INDEX [IX_PropiedadHistorial_PropiedadId] ON dbo.PropiedadHistorial ([PropiedadId], [CompletionDate] DESC);
    PRINT 'Table PropiedadHistorial created.';
END
GO

IF OBJECT_ID(N'dbo.PropiedadMantenimiento', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PropiedadMantenimiento](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NOT NULL,
        [Title] [nvarchar](150) NOT NULL,
        [DueDate] [datetime2](7) NULL,
        [CompletedDate] [datetime2](7) NULL,
        [Status] [nvarchar](20) NOT NULL CONSTRAINT [DF_PropiedadMantenimiento_Status] DEFAULT (N'Upcoming'),
        [Notes] [nvarchar](1000) NULL,
        [PropiedadProveedorId] [int] NULL,
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PropiedadMantenimiento_FechaCreacion] DEFAULT (sysutcdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_PropiedadMantenimiento] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE dbo.PropiedadMantenimiento
        WITH CHECK ADD CONSTRAINT [FK_PropiedadMantenimiento_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;

    ALTER TABLE dbo.PropiedadMantenimiento
        WITH CHECK ADD CONSTRAINT [FK_PropiedadMantenimiento_PropiedadProveedores]
        FOREIGN KEY([PropiedadProveedorId]) REFERENCES [dbo].[PropiedadProveedores] ([Id]);

    CREATE INDEX [IX_PropiedadMantenimiento_PropiedadId] ON dbo.PropiedadMantenimiento ([PropiedadId], [DueDate]);
    PRINT 'Table PropiedadMantenimiento created.';
END
GO

IF OBJECT_ID(N'dbo.PropiedadDocumentos', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PropiedadDocumentos](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NOT NULL,
        [Category] [nvarchar](40) NOT NULL,
        [Title] [nvarchar](200) NOT NULL,
        [FileName] [nvarchar](260) NULL,
        [StoragePath] [nvarchar](500) NULL,
        [ContentType] [nvarchar](80) NULL,
        [SizeBytes] [bigint] NULL,
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PropiedadDocumentos_FechaCreacion] DEFAULT (sysutcdatetime()),
        CONSTRAINT [PK_PropiedadDocumentos] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE dbo.PropiedadDocumentos
        WITH CHECK ADD CONSTRAINT [FK_PropiedadDocumentos_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;

    CREATE INDEX [IX_PropiedadDocumentos_PropiedadId] ON dbo.PropiedadDocumentos ([PropiedadId], [Category]);
    PRINT 'Table PropiedadDocumentos created.';
END
GO

PRINT 'My Home schema ready.';
GO
