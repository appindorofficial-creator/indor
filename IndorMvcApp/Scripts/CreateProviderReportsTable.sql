/*
  INDOR Provider completion reports table.
  Run after CreateProviderOperationsTables.sql if reports table was not included.
*/

IF OBJECT_ID(N'dbo.IndorProveedorReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorReports (
        Id                INT            IDENTITY(1,1) NOT NULL,
        ProveedorId       INT            NOT NULL,
        JobId             INT            NULL,
        ClienteId         INT            NULL,
        ReportCode        NVARCHAR(30)   NOT NULL,
        Title             NVARCHAR(150)  NOT NULL,
        Address           NVARCHAR(250)  NOT NULL,
        CustomerName      NVARCHAR(120)  NULL,
        ServiceType       NVARCHAR(80)   NULL,
        Status            NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvRpt_Status DEFAULT (N'Draft'),
        PhotosCount       INT            NOT NULL CONSTRAINT DF_IndorProvRpt_Photos DEFAULT (0),
        HasChecklist      BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_Checklist DEFAULT (0),
        HasWarranty       BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_Warranty DEFAULT (0),
        HasDocuments      BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_Docs DEFAULT (0),
        AddedToHouseFacts BIT            NOT NULL CONSTRAINT DF_IndorProvRpt_HouseFacts DEFAULT (0),
        CompletedUtc      DATETIME2      NULL,
        FechaCreacion     DATETIME2      NOT NULL CONSTRAINT DF_IndorProvRpt_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion DATETIME2     NULL,
        CONSTRAINT PK_IndorProveedorReports PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvRpt_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvRpt_Job FOREIGN KEY (JobId) REFERENCES dbo.IndorProveedorJobs(Id),
        CONSTRAINT FK_IndorProvRpt_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.IndorProveedorClientes(Id)
    );
    CREATE INDEX IX_IndorProvRpt_Proveedor ON dbo.IndorProveedorReports(ProveedorId, Status);
END
GO
