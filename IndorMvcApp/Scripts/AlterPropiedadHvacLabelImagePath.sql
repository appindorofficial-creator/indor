/*
  Stores the user-uploaded HVAC manufacturer label photo path.
*/

IF COL_LENGTH(N'dbo.PropiedadHvacSistemas', N'LabelImagePath') IS NULL
    ALTER TABLE dbo.PropiedadHvacSistemas ADD [LabelImagePath] [nvarchar](300) NULL;

PRINT 'PropiedadHvacSistemas.LabelImagePath ready.';
