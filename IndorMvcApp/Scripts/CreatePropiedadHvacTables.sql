/*
  Propiedad HVAC system — user-entered HVAC profile (values suggested from OpenAI House Fact).
  Safe to run multiple times.

  Note: UserId FK uses NO ACTION because Propiedades already cascades from AspNetUsers;
  dual CASCADE paths would trigger SQL Server error 1785.
*/

IF OBJECT_ID(N'dbo.PropiedadHvacSistemas', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PropiedadHvacSistemas](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [SystemType] [nvarchar](30) NOT NULL CONSTRAINT [DF_PropiedadHvacSistemas_SystemType] DEFAULT (N'CentralAC'),
        [Brand] [nvarchar](80) NULL,
        [Model] [nvarchar](80) NULL,
        [SerialNumber] [nvarchar](80) NULL,
        [InstallYear] [int] NULL,
        [FilterSize] [nvarchar](40) NULL,
        [LastServiceDate] [datetime2](7) NULL,
        [FilterRemindersEnabled] [bit] NOT NULL CONSTRAINT [DF_PropiedadHvacSistemas_FilterRemindersEnabled] DEFAULT (1),
        [FilterReminderDays] [int] NOT NULL CONSTRAINT [DF_PropiedadHvacSistemas_FilterReminderDays] DEFAULT (90),
        [OpenAiDataSource] [nvarchar](120) NULL,
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PropiedadHvacSistemas_FechaCreacion] DEFAULT (sysutcdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_PropiedadHvacSistemas] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[PropiedadHvacSistemas]
        WITH CHECK ADD CONSTRAINT [FK_PropiedadHvacSistemas_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[PropiedadHvacSistemas]
        WITH CHECK ADD CONSTRAINT [FK_PropiedadHvacSistemas_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;

    CREATE UNIQUE INDEX [UX_PropiedadHvacSistemas_PropiedadId]
        ON dbo.PropiedadHvacSistemas ([PropiedadId]);

    PRINT 'Table PropiedadHvacSistemas created.';
END
ELSE
    PRINT 'Table PropiedadHvacSistemas already exists.';
GO

-- Repair partial runs (table created but FK/index missing after error 1785)
IF OBJECT_ID(N'dbo.PropiedadHvacSistemas', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_PropiedadHvacSistemas_Propiedades'
          AND parent_object_id = OBJECT_ID(N'dbo.PropiedadHvacSistemas'))
    BEGIN
        ALTER TABLE [dbo].[PropiedadHvacSistemas]
            WITH CHECK ADD CONSTRAINT [FK_PropiedadHvacSistemas_Propiedades]
            FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;
        PRINT 'Added FK_PropiedadHvacSistemas_Propiedades.';
    END

    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_PropiedadHvacSistemas_AspNetUsers'
          AND parent_object_id = OBJECT_ID(N'dbo.PropiedadHvacSistemas')
          AND delete_referential_action_desc = N'CASCADE')
    BEGIN
        ALTER TABLE [dbo].[PropiedadHvacSistemas]
            DROP CONSTRAINT [FK_PropiedadHvacSistemas_AspNetUsers];
        PRINT 'Dropped CASCADE FK_PropiedadHvacSistemas_AspNetUsers for repair.';
    END

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_PropiedadHvacSistemas_AspNetUsers'
          AND parent_object_id = OBJECT_ID(N'dbo.PropiedadHvacSistemas'))
    BEGIN
        ALTER TABLE [dbo].[PropiedadHvacSistemas]
            WITH CHECK ADD CONSTRAINT [FK_PropiedadHvacSistemas_AspNetUsers]
            FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;
        PRINT 'Added FK_PropiedadHvacSistemas_AspNetUsers (NO ACTION).';
    END

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'UX_PropiedadHvacSistemas_PropiedadId'
          AND object_id = OBJECT_ID(N'dbo.PropiedadHvacSistemas'))
    BEGIN
        CREATE UNIQUE INDEX [UX_PropiedadHvacSistemas_PropiedadId]
            ON dbo.PropiedadHvacSistemas ([PropiedadId]);
        PRINT 'Added UX_PropiedadHvacSistemas_PropiedadId.';
    END
END
GO
