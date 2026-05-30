/*
  Propiedad water heater — user-entered profile (values suggested from OpenAI House Fact).
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.PropiedadWaterHeaterSistemas', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PropiedadWaterHeaterSistemas](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [HeaterType] [nvarchar](20) NOT NULL CONSTRAINT [DF_PropiedadWaterHeaterSistemas_HeaterType] DEFAULT (N'Tank'),
        [Brand] [nvarchar](80) NULL,
        [Model] [nvarchar](80) NULL,
        [SerialNumber] [nvarchar](80) NULL,
        [InstallYear] [int] NULL,
        [TankSize] [nvarchar](40) NULL,
        [LastServiceDate] [datetime2](7) NULL,
        [FlushRemindersEnabled] [bit] NOT NULL CONSTRAINT [DF_PropiedadWaterHeaterSistemas_FlushRemindersEnabled] DEFAULT (1),
        [FlushReminderDays] [int] NOT NULL CONSTRAINT [DF_PropiedadWaterHeaterSistemas_FlushReminderDays] DEFAULT (365),
        [OpenAiDataSource] [nvarchar](120) NULL,
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_PropiedadWaterHeaterSistemas_FechaCreacion] DEFAULT (sysutcdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_PropiedadWaterHeaterSistemas] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[PropiedadWaterHeaterSistemas]
        WITH CHECK ADD CONSTRAINT [FK_PropiedadWaterHeaterSistemas_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[PropiedadWaterHeaterSistemas]
        WITH CHECK ADD CONSTRAINT [FK_PropiedadWaterHeaterSistemas_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;

    CREATE UNIQUE INDEX [UX_PropiedadWaterHeaterSistemas_PropiedadId]
        ON dbo.PropiedadWaterHeaterSistemas ([PropiedadId]);

    PRINT 'Table PropiedadWaterHeaterSistemas created.';
END
ELSE
    PRINT 'Table PropiedadWaterHeaterSistemas already exists.';
GO
