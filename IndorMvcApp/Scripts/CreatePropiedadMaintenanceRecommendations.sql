/*
  INDOR — AI maintenance recommendations for homeowner properties.
  Run on IndorDB. Safe to run multiple times.
*/

IF COL_LENGTH('dbo.Propiedades', 'MantenimientoRecomendadoJson') IS NULL
BEGIN
    ALTER TABLE dbo.Propiedades ADD MantenimientoRecomendadoJson NVARCHAR(MAX) NULL;
    PRINT 'Column Propiedades.MantenimientoRecomendadoJson added.';
END
GO

IF COL_LENGTH('dbo.Propiedades', 'MantenimientoRecomendadoUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Propiedades ADD MantenimientoRecomendadoUtc DATETIME2(7) NULL;
    PRINT 'Column Propiedades.MantenimientoRecomendadoUtc added.';
END
GO

PRINT 'Property maintenance recommendations columns ready.';
