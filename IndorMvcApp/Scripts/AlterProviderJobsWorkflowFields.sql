/*
  INDOR PRO Jobs workflow — schedule, details, active job, completion report.
  Run after AlterProviderJobsExtendedFields.sql.
*/

IF COL_LENGTH('dbo.IndorProveedorJobs', 'EstimateCode') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD EstimateCode NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'InvoiceStatus') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD InvoiceStatus NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'PaymentAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD PaymentAmount DECIMAL(12,2) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'PaymentStatus') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD PaymentStatus NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'DistanceMiles') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD DistanceMiles DECIMAL(5,1) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'ScopeOfWork') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ScopeOfWork NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'MaterialsNeeded') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD MaterialsNeeded NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'AccessInstructions') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD AccessInstructions NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'JobNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD JobNotes NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'AssignedTechnician') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD AssignedTechnician NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'Priority') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD Priority NVARCHAR(20) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'ImageUrl') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ImageUrl NVARCHAR(500) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'ChecklistJson') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ChecklistJson NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'MaterialsUsedJson') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD MaterialsUsedJson NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'PhotoUrlsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD PhotoUrlsJson NVARCHAR(1000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'StartedAt') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD StartedAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'CompletedAt') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD CompletedAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'HomeownerSignature') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD HomeownerSignature NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'HomeownerSignedAt') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD HomeownerSignedAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'ReportCode') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ReportCode NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'WorkPerformed') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD WorkPerformed NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'LaborWarranty') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD LaborWarranty NVARCHAR(200) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'FinalNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD FinalNotes NVARCHAR(2000) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'IsDraft') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD IsDraft BIT NOT NULL CONSTRAINT DF_IndorProvJob_Draft DEFAULT (0);
GO
