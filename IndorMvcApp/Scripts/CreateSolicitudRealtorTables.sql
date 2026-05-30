/*
  Realtor request flow — standalone solicitud table.
  Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.SolicitudesRealtor', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SolicitudesRealtor](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PropiedadId] [int] NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [NeedType] [nvarchar](30) NOT NULL,
        [PreferredArea] [nvarchar](120) NULL,
        [Timeframe] [nvarchar](20) NOT NULL,
        [PriceRange] [nvarchar](80) NULL,
        [Notes] [nvarchar](500) NULL,
        [Status] [nvarchar](30) NOT NULL CONSTRAINT [DF_SolicitudesRealtor_Status] DEFAULT (N'MatchingInProgress'),
        [FechaCreacion] [datetime2](7) NOT NULL CONSTRAINT [DF_SolicitudesRealtor_FechaCreacion] DEFAULT (sysutcdatetime()),
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_SolicitudesRealtor] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[SolicitudesRealtor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRealtor_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]) ON DELETE SET NULL;

    ALTER TABLE [dbo].[SolicitudesRealtor]
        WITH CHECK ADD CONSTRAINT [FK_SolicitudesRealtor_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION;

    CREATE INDEX [IX_SolicitudesRealtor_UserId] ON dbo.SolicitudesRealtor ([UserId]);
    CREATE INDEX [IX_SolicitudesRealtor_PropiedadId] ON dbo.SolicitudesRealtor ([PropiedadId]);

    PRINT 'Table SolicitudesRealtor created.';
END
ELSE
    PRINT 'Table SolicitudesRealtor already exists.';
GO
