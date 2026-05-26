/*
  Add columns for the 6-step structural inspection flow.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TiposPreocupacion') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TiposPreocupacion] NVARCHAR(300) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'SignosVisibles') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [SignosVisibles] NVARCHAR(300) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'SeveridadApariencia') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [SeveridadApariencia] NVARCHAR(20) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'UbicacionEspecifica') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [UbicacionEspecifica] NVARCHAR(200) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'CuandoNotadoTexto') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [CuandoNotadoTexto] NVARCHAR(50) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'DuracionProblema') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [DuracionProblema] NVARCHAR(20) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'Severidad') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [Severidad] NVARCHAR(20) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'ReparacionesPrevias') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [ReparacionesPrevias] NVARCHAR(20) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'CondicionesInseguras') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [CondicionesInseguras] NVARCHAR(200) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'MejorHorarioVisita') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [MejorHorarioVisita] NVARCHAR(20) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TipoPropiedad') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TipoPropiedad] NVARCHAR(30) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TipoFundacion') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TipoFundacion] NVARCHAR(20) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'TieneReporte') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [TieneReporte] NVARCHAR(10) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'CambiosRecientes') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [CambiosRecientes] NVARCHAR(200) NULL;
GO
IF COL_LENGTH(N'dbo.SolicitudesInspeccionStructural', N'AccesoPreferido') IS NULL
    ALTER TABLE dbo.SolicitudesInspeccionStructural ADD [AccesoPreferido] NVARCHAR(30) NULL;
GO

PRINT 'Structural inspection step columns updated.';
GO
