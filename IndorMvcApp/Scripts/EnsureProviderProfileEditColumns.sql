/*
  Provider Edit Profile — ensure IndorProveedores columns exist (Azure / IndorDB).
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorProveedores', N'BusinessAddress') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD BusinessAddress NVARCHAR(300) NULL;
    PRINT 'Column IndorProveedores.BusinessAddress added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'ServiceDescription') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD ServiceDescription NVARCHAR(200) NULL;
    PRINT 'Column IndorProveedores.ServiceDescription added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'Latitude') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD Latitude DECIMAL(9,6) NULL;
    PRINT 'Column IndorProveedores.Latitude added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'Longitude') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD Longitude DECIMAL(9,6) NULL;
    PRINT 'Column IndorProveedores.Longitude added.';
END
GO
