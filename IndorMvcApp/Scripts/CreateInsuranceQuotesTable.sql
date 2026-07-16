/*
  INDOR PRO — Insurance quote requests.
  Stores each provider's insurance quote request (4-step wizard) so it can be
  sent to a licensed insurance partner and traced back to the provider.

  Safe to re-run.
  Run order: after the provider tables (IndorProveedores) exist.
*/

IF OBJECT_ID(N'dbo.IndorProviderInsuranceQuotes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorProviderInsuranceQuotes (
        Id                   INT            IDENTITY(1,1) NOT NULL,
        ProveedorId          INT            NOT NULL,
        QuoteCode            NVARCHAR(40)   NOT NULL,
        Plan                 NVARCHAR(40)   NULL,

        -- Step 1: Coverage
        Coverages            NVARCHAR(300)  NULL,   -- comma separated
        Trade                NVARCHAR(120)  NULL,

        -- Step 2: Business & Owner info
        BusinessName         NVARCHAR(200)  NULL,
        BusinessAddress      NVARCHAR(300)  NULL,
        OwnerName            NVARCHAR(160)  NULL,
        OwnerDateOfBirth     DATE           NULL,
        OwnerPhone           NVARCHAR(40)   NULL,
        OwnerEmail           NVARCHAR(256)  NULL,

        -- Step 3: Business details
        NumberOfEmployees    INT            NULL,
        EmployeePayroll      DECIMAL(18,2)  NULL,
        CompanyGrossRevenue  DECIMAL(18,2)  NULL,
        YearsInBusiness      NVARCHAR(40)   NULL,
        ZipCode              NVARCHAR(20)   NULL,
        WorksAtCustomerHomes BIT            NULL,
        UsesSubcontractors   BIT            NULL,

        -- Step 4: Review / submit
        ConfirmedAccurate    BIT            NOT NULL CONSTRAINT DF_IndorInsQuote_Confirmed DEFAULT (0),
        Status               NVARCHAR(30)   NOT NULL CONSTRAINT DF_IndorInsQuote_Status    DEFAULT (N'Submitted'),
        SubmittedUtc         DATETIME2      NULL,
        FechaCreacion        DATETIME2      NOT NULL CONSTRAINT DF_IndorInsQuote_Created   DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IndorProviderInsuranceQuotes PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorInsQuote_Proveedor FOREIGN KEY (ProveedorId)
            REFERENCES dbo.IndorProveedores(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_IndorInsQuote_Proveedor ON dbo.IndorProviderInsuranceQuotes(ProveedorId, Status);
END
GO

/* ---------------------------------------------------------------
   Additional columns (5-step wizard with integrated payment).
   Safe to re-run — each column is added only if missing.
   --------------------------------------------------------------- */
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'City') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD City NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'State') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD [State] NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'NeedsCOI') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD NeedsCOI BIT NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PayTodayAmount') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PayTodayAmount DECIMAL(18,2) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'MonthlyAmount') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD MonthlyAmount DECIMAL(18,2) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaymentMethod NVARCHAR(40) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'CardLast4') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD CardLast4 NVARCHAR(8) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'AutoPayMonthly') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD AutoPayMonthly BIT NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'FirstBillingDate') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD FirstBillingDate DATE NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaymentStatus') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaymentStatus NVARCHAR(30) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaymentAuthorized') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaymentAuthorized BIT NOT NULL CONSTRAINT DF_IndorInsQuote_PayAuth DEFAULT (0);
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'PaidUtc') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD PaidUtc DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'ReceiptNumber') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD ReceiptNumber NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'StripeCheckoutSessionId') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD StripeCheckoutSessionId NVARCHAR(120) NULL;
GO
IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'StripePaymentIntentId') IS NULL
    ALTER TABLE dbo.IndorProviderInsuranceQuotes ADD StripePaymentIntentId NVARCHAR(120) NULL;
GO
