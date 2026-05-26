/*
  Adds scheduled appointment fields to inspection request tables.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.SolicitudesInspeccion', N'FechaCitaProgramada') IS NULL
BEGIN
    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD [FechaCitaProgramada] [date] NULL;
    PRINT 'Column SolicitudesInspeccion.FechaCitaProgramada added.';
END
GO

IF COL_LENGTH(N'dbo.SolicitudesInspeccion', N'HoraCitaProgramada') IS NULL
BEGIN
    ALTER TABLE [dbo].[SolicitudesInspeccion]
        ADD [HoraCitaProgramada] [time](0) NULL;
    PRINT 'Column SolicitudesInspeccion.HoraCitaProgramada added.';
END
GO

IF COL_LENGTH(N'dbo.SolicitudesInspeccionElectrica', N'FechaCitaProgramada') IS NULL
BEGIN
    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD [FechaCitaProgramada] [date] NULL;
    PRINT 'Column SolicitudesInspeccionElectrica.FechaCitaProgramada added.';
END
GO

IF COL_LENGTH(N'dbo.SolicitudesInspeccionElectrica', N'HoraCitaProgramada') IS NULL
BEGIN
    ALTER TABLE [dbo].[SolicitudesInspeccionElectrica]
        ADD [HoraCitaProgramada] [time](0) NULL;
    PRINT 'Column SolicitudesInspeccionElectrica.HoraCitaProgramada added.';
END
GO
