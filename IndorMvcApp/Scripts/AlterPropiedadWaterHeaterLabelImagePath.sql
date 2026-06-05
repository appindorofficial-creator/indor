/*
  Stores the user-uploaded water heater manufacturer label photo path.
*/

IF COL_LENGTH(N'dbo.PropiedadWaterHeaterSistemas', N'LabelImagePath') IS NULL
    ALTER TABLE dbo.PropiedadWaterHeaterSistemas ADD [LabelImagePath] [nvarchar](300) NULL;

PRINT 'PropiedadWaterHeaterSistemas.LabelImagePath ready.';
