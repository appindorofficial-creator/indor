/*
  ProgramacionesMicroservicio — user scheduled dates for microservices
  (e.g. air filter change). Used for Schedule tab and Home notifications.

  Run against your Indor database after backup.
*/

IF OBJECT_ID(N'dbo.ProgramacionesMicroservicio', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ProgramacionesMicroservicio](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [MicroservicioId] [int] NOT NULL,
        [PropiedadId] [int] NULL,
        [FechaProgramada] [date] NOT NULL,
        [Notas] [nvarchar](500) NULL,
        [Estado] [nvarchar](30) NOT NULL,
        [FechaCreacion] [datetime2](7) NOT NULL,
        [FechaActualizacion] [datetime2](7) NULL,
        CONSTRAINT [PK_ProgramacionesMicroservicio] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[ProgramacionesMicroservicio]
        ADD CONSTRAINT [DF_ProgramacionesMicroservicio_Estado]
        DEFAULT (N'Scheduled') FOR [Estado];

    ALTER TABLE [dbo].[ProgramacionesMicroservicio]
        ADD CONSTRAINT [DF_ProgramacionesMicroservicio_FechaCreacion]
        DEFAULT (sysdatetime()) FOR [FechaCreacion];

    ALTER TABLE [dbo].[ProgramacionesMicroservicio]
        WITH CHECK ADD CONSTRAINT [FK_ProgramacionesMicroservicio_AspNetUsers]
        FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[ProgramacionesMicroservicio]
        WITH CHECK ADD CONSTRAINT [FK_ProgramacionesMicroservicio_Microservicios]
        FOREIGN KEY([MicroservicioId]) REFERENCES [dbo].[Microservicios] ([Id]);

    ALTER TABLE [dbo].[ProgramacionesMicroservicio]
        WITH CHECK ADD CONSTRAINT [FK_ProgramacionesMicroservicio_Propiedades]
        FOREIGN KEY([PropiedadId]) REFERENCES [dbo].[Propiedades] ([Id]);

    CREATE NONCLUSTERED INDEX [IX_ProgramacionesMicroservicio_UserId_Fecha]
        ON [dbo].[ProgramacionesMicroservicio] ([UserId], [FechaProgramada], [Estado]);

    CREATE NONCLUSTERED INDEX [IX_ProgramacionesMicroservicio_MicroservicioId]
        ON [dbo].[ProgramacionesMicroservicio] ([MicroservicioId]);

    PRINT 'Table ProgramacionesMicroservicio created.';
END
ELSE
BEGIN
    PRINT 'Table ProgramacionesMicroservicio already exists.';
END
GO
