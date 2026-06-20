/*
  INDOR — Edit Profile wizard fields for realtor public profile.
  Run after AlterRealtorBusinessInformation.sql. Safe to run multiple times.
*/

IF COL_LENGTH(N'dbo.IndorRealtors', N'PublicDisplayName') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD PublicDisplayName NVARCHAR(120) NULL;
    PRINT 'Column IndorRealtors.PublicDisplayName added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'RealtorTitle') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD RealtorTitle NVARCHAR(80) NULL;
    PRINT 'Column IndorRealtors.RealtorTitle added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'Website') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD Website NVARCHAR(200) NULL;
    PRINT 'Column IndorRealtors.Website added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeCity') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD OfficeCity NVARCHAR(80) NULL;
    PRINT 'Column IndorRealtors.OfficeCity added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeState') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD OfficeState NVARCHAR(10) NULL;
    PRINT 'Column IndorRealtors.OfficeState added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'OfficeZip') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD OfficeZip NVARCHAR(15) NULL;
    PRINT 'Column IndorRealtors.OfficeZip added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'YearsOfExperience') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD YearsOfExperience NVARCHAR(30) NULL;
    PRINT 'Column IndorRealtors.YearsOfExperience added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'SpecialtiesJson') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD SpecialtiesJson NVARCHAR(200) NULL;
    PRINT 'Column IndorRealtors.SpecialtiesJson added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'TeamName') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD TeamName NVARCHAR(120) NULL;
    PRINT 'Column IndorRealtors.TeamName added.';
END
GO

IF COL_LENGTH(N'dbo.IndorRealtors', N'BrokerInCharge') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtors ADD BrokerInCharge NVARCHAR(120) NULL;
    PRINT 'Column IndorRealtors.BrokerInCharge added.';
END
GO

PRINT 'Realtor edit profile columns ready.';
