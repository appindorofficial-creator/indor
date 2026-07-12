/* ============================================================
   INDOR — Manual insurance issuance requests
   Idempotent: safe to run multiple times (local / Azure).
   Stores the carrier "Business Quote Sheet" fields collected
   from the provider so INDOR can email the carrier to issue
   a policy manually (pre-API integration).
   ============================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IndorInsuranceIssuanceRequests' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.IndorInsuranceIssuanceRequests (
        Id                  INT             IDENTITY(1,1) NOT NULL,
        ProveedorId         INT             NOT NULL,
        RequestCode         NVARCHAR(40)    NOT NULL,
        [Plan]              NVARCHAR(40)    NULL,
        BusinessName        NVARCHAR(200)   NOT NULL,
        BusinessAddress     NVARCHAR(300)   NOT NULL,
        WorkersComp         BIT             NOT NULL CONSTRAINT DF_IndorInsIss_WC DEFAULT (0),
        GeneralLiability    BIT             NOT NULL CONSTRAINT DF_IndorInsIss_GL DEFAULT (0),
        OwnerName           NVARCHAR(160)   NOT NULL,
        OwnerDateOfBirth    DATE            NULL,
        OwnerPhone          NVARCHAR(40)    NULL,
        OwnerEmail          NVARCHAR(256)   NULL,
        TypeOfBusiness      NVARCHAR(160)   NULL,
        NumberOfEmployees   NVARCHAR(60)    NULL,
        EmployeePayroll     DECIMAL(18,2)   NULL,
        CompanyGross        DECIMAL(18,2)   NULL,
        Notes               NVARCHAR(1000)  NULL,
        Status              NVARCHAR(30)    NOT NULL CONSTRAINT DF_IndorInsIss_Status DEFAULT ('Submitted'),
        CarrierEmail        NVARCHAR(256)   NULL,
        CarrierEmailStatus  NVARCHAR(30)    NULL,
        CarrierEmailSentUtc DATETIME2(7)    NULL,
        SubmittedUtc        DATETIME2(7)    NULL,
        CreatedUtc          DATETIME2(7)    NOT NULL CONSTRAINT DF_IndorInsIss_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorInsuranceIssuanceRequests PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorInsuranceIssuanceRequests_IndorProveedores_ProveedorId FOREIGN KEY (ProveedorId)
            REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_IndorInsuranceIssuanceRequests_RequestCode
        ON dbo.IndorInsuranceIssuanceRequests(RequestCode);
    CREATE INDEX IX_IndorInsuranceIssuanceRequests_ProveedorId_CreatedUtc
        ON dbo.IndorInsuranceIssuanceRequests(ProveedorId, CreatedUtc);

    PRINT 'Table IndorInsuranceIssuanceRequests created.';
END
ELSE
BEGIN
    PRINT 'Table IndorInsuranceIssuanceRequests already exists.';
END
GO
