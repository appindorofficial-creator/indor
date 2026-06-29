/*
  INDOR PRO — Export Report flow persistence.
  Adds the export-specific fields to IndorProveedorReports and creates a
  dedicated photos table so every uploaded photo is traceable back to the
  report, the provider and the job.

  Export reports are stored in IndorProveedorReports with ReportType = 'Export Report'.

  Safe to re-run.
  Run order: after CreateProviderReportsTable.sql / AlterProviderReportsUploadFields.sql.
*/

------------------------------------------------------------
-- 1) Export fields on IndorProveedorReports
------------------------------------------------------------
IF COL_LENGTH('dbo.IndorProveedorReports', 'ReportDate') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD ReportDate DATE NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'PreparedBy') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD PreparedBy NVARCHAR(120) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'Category') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Category NVARCHAR(60) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'LocationDetail') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD LocationDetail NVARCHAR(120) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'Priority') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Priority NVARCHAR(20) NULL;
GO

IF COL_LENGTH('dbo.IndorProveedorReports', 'Weather') IS NULL
    ALTER TABLE dbo.IndorProveedorReports ADD Weather NVARCHAR(40) NULL;
GO

------------------------------------------------------------
-- 2) Report photos table (traceability)
------------------------------------------------------------
IF OBJECT_ID(N'dbo.IndorProveedorReportPhotos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorReportPhotos (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ReportId      INT            NOT NULL,
        ProveedorId   INT            NOT NULL,
        JobId         INT            NULL,
        Category      NVARCHAR(40)   NULL,
        FileUrl       NVARCHAR(500)  NOT NULL,
        FileName      NVARCHAR(260)  NULL,
        Caption       NVARCHAR(250)  NULL,
        SortOrder     INT            NOT NULL CONSTRAINT DF_IndorRptPhoto_Sort DEFAULT (0),
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorRptPhoto_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorReportPhotos PRIMARY KEY CLUSTERED (Id),
        -- Cascade only from the report; provider/job use NO ACTION to avoid
        -- multiple cascade paths.
        CONSTRAINT FK_IndorRptPhoto_Report    FOREIGN KEY (ReportId)
            REFERENCES dbo.IndorProveedorReports(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorRptPhoto_Proveedor FOREIGN KEY (ProveedorId)
            REFERENCES dbo.IndorProveedores(Id),
        CONSTRAINT FK_IndorRptPhoto_Job       FOREIGN KEY (JobId)
            REFERENCES dbo.IndorProveedorJobs(Id)
    );
    CREATE INDEX IX_IndorRptPhoto_Report ON dbo.IndorProveedorReportPhotos(ReportId, SortOrder);
    CREATE INDEX IX_IndorRptPhoto_Proveedor ON dbo.IndorProveedorReportPhotos(ProveedorId);
END
GO
