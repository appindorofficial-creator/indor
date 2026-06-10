/*
  Extended fields for INDOR PRO Lead Details screen.
  Run after CreateProviderOperationsTables.sql.
*/

IF COL_LENGTH('dbo.IndorProveedorLeads', 'LeadCode') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD LeadCode NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorProvLead_Code DEFAULT (N'');
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'CustomerEmail') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD CustomerEmail NVARCHAR(256) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'CustomerPhone') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD CustomerPhone NVARCHAR(30) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'IsHomeownerVerified') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD IsHomeownerVerified BIT NOT NULL CONSTRAINT DF_IndorProvLead_Verified DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'ProblemDescription') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD ProblemDescription NVARCHAR(2000) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'ImageUrl') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD ImageUrl NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'PhotosJson') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD PhotosJson NVARCHAR(1000) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DistanceMiles') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DistanceMiles DECIMAL(5,1) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'TimelineNote') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD TimelineNote NVARCHAR(120) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'HomeType') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD HomeType NVARCHAR(40) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'SquareFeet') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SquareFeet INT NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'YearBuilt') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD YearBuilt INT NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'Stories') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD Stories INT NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'AccessNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD AccessNotes NVARCHAR(500) NULL;
GO
