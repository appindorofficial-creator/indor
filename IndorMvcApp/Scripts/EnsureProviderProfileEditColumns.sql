/*
  Provider Edit Profile — ensure IndorProveedores columns exist (Azure / IndorDB).
  Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorProveedores', N'EpaCertificationNumber') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD EpaCertificationNumber NVARCHAR(80) NULL;
    PRINT 'Column IndorProveedores.EpaCertificationNumber added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'BackgroundCheckConsent') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD BackgroundCheckConsent BIT NOT NULL
        CONSTRAINT DF_IndorProveedores_BgCheck DEFAULT (0);
    PRINT 'Column IndorProveedores.BackgroundCheckConsent added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'ServiceDescription') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD ServiceDescription NVARCHAR(200) NULL;
    PRINT 'Column IndorProveedores.ServiceDescription added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'IsInsured') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD IsInsured BIT NOT NULL
        CONSTRAINT DF_IndorProveedores_IsInsured DEFAULT (0);
    PRINT 'Column IndorProveedores.IsInsured added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'IsLicensed') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD IsLicensed BIT NOT NULL
        CONSTRAINT DF_IndorProveedores_IsLicensed DEFAULT (0);
    PRINT 'Column IndorProveedores.IsLicensed added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'TeamSize') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD TeamSize NVARCHAR(40) NULL;
    PRINT 'Column IndorProveedores.TeamSize added.';
END
GO

IF COL_LENGTH(N'dbo.IndorProveedores', N'BusinessAddress') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD BusinessAddress NVARCHAR(300) NULL;
    PRINT 'Column IndorProveedores.BusinessAddress added.';
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

IF COL_LENGTH(N'dbo.IndorProveedores', N'OnboardingMetaJson') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores ADD OnboardingMetaJson NVARCHAR(2000) NULL;
    PRINT 'Column IndorProveedores.OnboardingMetaJson added.';
END
GO
