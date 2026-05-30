/*
  Optional metadata for inspection report uploads.
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.PropiedadDocumentos', N'InspectionDate') IS NULL
    ALTER TABLE dbo.PropiedadDocumentos ADD [InspectionDate] [datetime2](7) NULL;
GO

IF COL_LENGTH(N'dbo.PropiedadDocumentos', N'Notes') IS NULL
    ALTER TABLE dbo.PropiedadDocumentos ADD [Notes] [nvarchar](300) NULL;
GO

PRINT 'PropiedadDocumentos inspection fields ready.';
GO
