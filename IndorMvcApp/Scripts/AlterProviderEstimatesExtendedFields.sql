-- Extended fields for Quick Estimate flow from leads
IF COL_LENGTH('dbo.IndorProveedorEstimates', 'LeadId') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD LeadId INT NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ServiceType') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD ServiceType NVARCHAR(120) NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'CustomerName') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD CustomerName NVARCHAR(120) NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'LaborAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD LaborAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_IndorProvEst_Labor DEFAULT (0);

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'MaterialsAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD MaterialsAmount DECIMAL(12,2) NOT NULL CONSTRAINT DF_IndorProvEst_Materials DEFAULT (0);

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'ScopeItemsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD ScopeItemsJson NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'Timeline') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD Timeline NVARCHAR(80) NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'Warranty') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD Warranty NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'HomeownerNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD HomeownerNotes NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'NotifyHomeowner') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD NotifyHomeowner BIT NOT NULL CONSTRAINT DF_IndorProvEst_Notify DEFAULT (1);

IF COL_LENGTH('dbo.IndorProveedorEstimates', 'SaveCopyToLeads') IS NULL
    ALTER TABLE dbo.IndorProveedorEstimates ADD SaveCopyToLeads BIT NOT NULL CONSTRAINT DF_IndorProvEst_SaveCopy DEFAULT (1);

GO
