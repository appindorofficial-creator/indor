/*
  INDOR PRO – Upload Report wizard fields on IndorProveedorReports.
  Run after CreateProviderReportsTable.sql / CreateProviderOperationsTables.sql.
*/

IF COL_LENGTH('dbo.IndorProveedorReports', 'ReportType') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD ReportType NVARCHAR(40) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'Summary') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Summary NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'WorkCompleted') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD WorkCompleted NVARCHAR(1000) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'MaterialsUsed') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD MaterialsUsed NVARCHAR(1000) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'WarrantyInfo') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD WarrantyInfo NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'Recommendations') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Recommendations NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'InternalNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD InternalNotes NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'SendToHomeowner') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD SendToHomeowner BIT NOT NULL
        CONSTRAINT DF_IndorProvRpt_SendHomeowner DEFAULT (1);
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'RequestApproval') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD RequestApproval BIT NOT NULL
        CONSTRAINT DF_IndorProvRpt_RequestApproval DEFAULT (0);
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'AttachToHouseFacts') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD AttachToHouseFacts BIT NOT NULL
        CONSTRAINT DF_IndorProvRpt_AttachHF DEFAULT (1);
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'PhotoUrlsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD PhotoUrlsJson NVARCHAR(2000) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'DocumentsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD DocumentsJson NVARCHAR(2000) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'FilesCount') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD FilesCount INT NOT NULL
        CONSTRAINT DF_IndorProvRpt_FilesCount DEFAULT (0);
GO
