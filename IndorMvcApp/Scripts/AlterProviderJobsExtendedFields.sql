/*
  Extended fields for INDOR PRO Jobs cards (service type, photos, checklist, etc.)
  Run after CreateProviderOperationsTables.sql.
*/

IF COL_LENGTH('dbo.IndorProveedorJobs', 'ServiceType') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ServiceType NVARCHAR(80) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorJobs', 'ChecklistStatus') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ChecklistStatus NVARCHAR(40) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorJobs', 'PhotosCount') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD PhotosCount INT NOT NULL CONSTRAINT DF_IndorProvJob_Photos DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorJobs', 'HouseFactsStatus') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD HouseFactsStatus NVARCHAR(60) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorJobs', 'ViewedByCustomer') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ViewedByCustomer BIT NOT NULL CONSTRAINT DF_IndorProvJob_Viewed DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorJobs', 'EstimateAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD EstimateAmount DECIMAL(12,2) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'CustomerName') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD CustomerName NVARCHAR(120) NULL;
GO
