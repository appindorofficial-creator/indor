/*
  INDOR Provider operations — customers, jobs, leads, estimates, invoices, approvals.
  Run on IndorDB after CreateProviderPortalTables.sql.
*/

IF OBJECT_ID(N'dbo.IndorProveedorClientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorClientes (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProveedorId   INT            NOT NULL,
        Name          NVARCHAR(120)  NOT NULL,
        Email         NVARCHAR(256)  NULL,
        Phone         NVARCHAR(30)   NULL,
        CityState     NVARCHAR(120)  NULL,
        Address       NVARCHAR(250)  NULL,
        ConnectionStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_IndorProvCli_Conn DEFAULT (N'Connected'),
        IsPropertyVerified BIT       NOT NULL CONSTRAINT DF_IndorProvCli_Verified DEFAULT (0),
        IsAppConnected BIT           NOT NULL CONSTRAINT DF_IndorProvCli_AppConn DEFAULT (0),
        PropertiesCount INT          NOT NULL CONSTRAINT DF_IndorProvCli_Props DEFAULT (1),
        HouseFactsCount INT          NOT NULL CONSTRAINT DF_IndorProvCli_HouseFacts DEFAULT (0),
        LastActivityNote NVARCHAR(200) NULL,
        MemberSince   DATETIME2      NOT NULL CONSTRAINT DF_IndorProvCli_Member DEFAULT (SYSUTCDATETIME()),
        Activo        BIT            NOT NULL CONSTRAINT DF_IndorProvCli_Activo DEFAULT (1),
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorProvCli_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorClientes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvCli_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorProvCli_Proveedor ON dbo.IndorProveedorClientes(ProveedorId);
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorJobs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorJobs (
        Id                INT            IDENTITY(1,1) NOT NULL,
        ProveedorId       INT            NOT NULL,
        ClienteId         INT            NULL,
        JobCode           NVARCHAR(40)   NOT NULL,
        Title             NVARCHAR(150)  NOT NULL,
        Address           NVARCHAR(250)  NOT NULL,
        Status            NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorProvJob_Status DEFAULT (N'Scheduled'),
        ServiceType       NVARCHAR(80)   NULL,
        ChecklistStatus   NVARCHAR(40)   NULL,
        PhotosCount       INT            NOT NULL CONSTRAINT DF_IndorProvJob_Photos DEFAULT (0),
        HouseFactsStatus  NVARCHAR(60)   NULL,
        ViewedByCustomer  BIT            NOT NULL CONSTRAINT DF_IndorProvJob_Viewed DEFAULT (0),
        EstimateAmount    DECIMAL(12,2)  NULL,
        ScheduledAt       DATETIME2      NULL,
        FechaCreacion     DATETIME2      NOT NULL CONSTRAINT DF_IndorProvJob_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion DATETIME2     NULL,
        CONSTRAINT PK_IndorProveedorJobs PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvJob_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvJob_Cliente FOREIGN KEY (ClienteId) REFERENCES dbo.IndorProveedorClientes(Id)
    );
    CREATE INDEX IX_IndorProvJob_Proveedor ON dbo.IndorProveedorJobs(ProveedorId);
    CREATE INDEX IX_IndorProvJob_Scheduled ON dbo.IndorProveedorJobs(ProveedorId, ScheduledAt);
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorLeads', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorLeads (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProveedorId   INT            NOT NULL,
        Address       NVARCHAR(250)  NOT NULL,
        ServiceType   NVARCHAR(120)  NOT NULL,
        Urgency       NVARCHAR(40)   NOT NULL CONSTRAINT DF_IndorProvLead_Urgency DEFAULT (N'Standard'),
        Status        NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvLead_Status DEFAULT (N'New'),
        CustomerName  NVARCHAR(120)  NULL,
        CustomerEmail NVARCHAR(256)  NULL,
        CustomerPhone NVARCHAR(30)   NULL,
        LeadCode      NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvLead_Code DEFAULT (N''),
        IsHomeownerVerified BIT      NOT NULL CONSTRAINT DF_IndorProvLead_Verified DEFAULT (0),
        ProblemDescription NVARCHAR(2000) NULL,
        ImageUrl      NVARCHAR(500)  NULL,
        PhotosJson    NVARCHAR(1000) NULL,
        DistanceMiles DECIMAL(5,1)   NULL,
        TimelineNote  NVARCHAR(120)  NULL,
        HomeType      NVARCHAR(40)   NULL,
        SquareFeet    INT            NULL,
        YearBuilt     INT            NULL,
        Stories       INT            NULL,
        AccessNotes   NVARCHAR(500)  NULL,
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorProvLead_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorLeads PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvLead_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorProvLead_Proveedor ON dbo.IndorProveedorLeads(ProveedorId, Status);
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorEstimates', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorEstimates (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProveedorId   INT            NOT NULL,
        JobId         INT            NULL,
        EstimateCode  NVARCHAR(30)   NOT NULL,
        Amount        DECIMAL(12,2)  NOT NULL,
        Address       NVARCHAR(250)  NOT NULL,
        Status        NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvEst_Status DEFAULT (N'Sent'),
        SentUtc       DATETIME2      NULL,
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorProvEst_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorEstimates PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvEst_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvEst_Job FOREIGN KEY (JobId) REFERENCES dbo.IndorProveedorJobs(Id)
    );
    CREATE INDEX IX_IndorProvEst_Proveedor ON dbo.IndorProveedorEstimates(ProveedorId, Status);
END
GO

IF OBJECT_ID(N'dbo.IndorProveedorInvoices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorInvoices (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProveedorId   INT            NOT NULL,
        JobId         INT            NULL,
        Amount        DECIMAL(12,2)  NOT NULL,
        Status        NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvInv_Status DEFAULT (N'Pending'),
        DueDate       DATETIME2      NULL,
        PaidDate      DATETIME2      NULL,
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorProvInv_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorInvoices PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvInv_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvInv_Job FOREIGN KEY (JobId) REFERENCES dbo.IndorProveedorJobs(Id)
    );
    CREATE INDEX IX_IndorProvInv_Proveedor ON dbo.IndorProveedorInvoices(ProveedorId, Status);
END
GO

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

IF OBJECT_ID(N'dbo.IndorProveedorApprovals', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProveedorApprovals (
        Id            INT            IDENTITY(1,1) NOT NULL,
        ProveedorId   INT            NOT NULL,
        JobId         INT            NULL,
        Address       NVARCHAR(250)  NOT NULL,
        ImageUrl      NVARCHAR(500)  NULL,
        Status        NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorProvAppr_Status DEFAULT (N'Pending'),
        FechaCreacion DATETIME2      NOT NULL CONSTRAINT DF_IndorProvAppr_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorProveedorApprovals PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorProvAppr_Proveedor FOREIGN KEY (ProveedorId) REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorProvAppr_Job FOREIGN KEY (JobId) REFERENCES dbo.IndorProveedorJobs(Id)
    );
    CREATE INDEX IX_IndorProvAppr_Proveedor ON dbo.IndorProveedorApprovals(ProveedorId, Status);
END
GO
