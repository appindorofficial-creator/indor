/*
  General Guidance wizard fields for SolicitudesRealtor.
  Run after CreateSolicitudRealtorTables.sql
*/
SET NOCOUNT ON;

IF COL_LENGTH('dbo.SolicitudesRealtor', 'GuidanceStep') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD GuidanceStep int NOT NULL CONSTRAINT DF_SolicitudesRealtor_GuidanceStep DEFAULT (0);
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'RentComfortRange') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD RentComfortRange nvarchar(30) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'HomeType') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD HomeType nvarchar(30) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'Bedrooms') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD Bedrooms nvarchar(10) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'Bathrooms') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD Bathrooms nvarchar(10) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'Occupants') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD Occupants nvarchar(10) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'Pets') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD Pets nvarchar(30) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'OutdoorSpaceImportance') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD OutdoorSpaceImportance nvarchar(30) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'ParkingNeed') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD ParkingNeed nvarchar(20) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'OpenToNearbyAreas') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD OpenToNearbyAreas bit NOT NULL CONSTRAINT DF_SolicitudesRealtor_OpenToNearby DEFAULT (0);
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'Priorities') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD Priorities nvarchar(200) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'ContactPhone') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD ContactPhone nvarchar(30) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'ContactEmail') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD ContactEmail nvarchar(256) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'PreferredContactMethod') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD PreferredContactMethod nvarchar(30) NULL;
GO

IF COL_LENGTH('dbo.SolicitudesRealtor', 'GuidanceNotes') IS NULL
    ALTER TABLE dbo.SolicitudesRealtor ADD GuidanceNotes nvarchar(500) NULL;
GO

PRINT 'SolicitudesRealtor General Guidance columns ready.';
GO
