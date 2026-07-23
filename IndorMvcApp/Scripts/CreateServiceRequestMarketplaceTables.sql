/*
  CreateServiceRequestMarketplaceTables.sql
  -----------------------------------------
  INDOR — Homeowner -> Provider service request marketplace.

  Creates:
    - dbo.IndorServiceRequests   (open homeowner requests providers can claim)
    - dbo.IndorAppNotifications  (unified in-app notifications, EN/ES baked in)

  Flow:
    1. Homeowner posts a service request (Status = 'Open').
    2. Providers whose trade/category matches receive an email + in-app notification.
    3. First provider to "Take" it claims it atomically (Status -> 'Claimed');
       it disappears from every other provider's available list.
    4. Homeowner gets an email + in-app notification with the provider's details.

  Idempotent: safe to re-run. Run on IndorDB in SSMS / Azure Data Studio.
*/

SET NOCOUNT ON;

/* ============================================================
   1) IndorServiceRequests
   ============================================================ */
IF OBJECT_ID(N'dbo.IndorServiceRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorServiceRequests (
        Id                    INT             IDENTITY(1,1) NOT NULL,
        UserId                NVARCHAR(450)   NOT NULL,          -- homeowner (AspNetUsers.Id)
        PropiedadId           INT             NULL,              -- optional property context
        CategoryId            NVARCHAR(40)    NOT NULL,          -- matches IndorProveedorCategoriasCatalogo.Id
        Title                 NVARCHAR(150)   NOT NULL,
        Description           NVARCHAR(1000)  NULL,
        Address               NVARCHAR(250)   NULL,
        City                  NVARCHAR(120)   NULL,
        ContactPhone          NVARCHAR(30)    NULL,
        PreferredDate         DATE            NULL,
        PreferredTime         NVARCHAR(40)    NULL,
        BudgetAmount          DECIMAL(10,2)   NULL,
        Urgency               NVARCHAR(20)    NOT NULL CONSTRAINT DF_IndorSvcReq_Urgency DEFAULT ('Standard'),
        Status                NVARCHAR(20)    NOT NULL CONSTRAINT DF_IndorSvcReq_Status  DEFAULT ('Open'),
        ClaimedByProveedorId  INT             NULL,
        ClaimedUtc            DATETIME2       NULL,
        CancelledUtc          DATETIME2       NULL,
        NotifiedProviderCount INT             NOT NULL CONSTRAINT DF_IndorSvcReq_NotifCnt DEFAULT (0),
        FechaCreacion         DATETIME2       NOT NULL CONSTRAINT DF_IndorSvcReq_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion    DATETIME2       NULL,
        CONSTRAINT PK_IndorServiceRequests PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorSvcReq_Category FOREIGN KEY (CategoryId)
            REFERENCES dbo.IndorProveedorCategoriasCatalogo (Id),
        CONSTRAINT FK_IndorSvcReq_Propiedad FOREIGN KEY (PropiedadId)
            REFERENCES dbo.Propiedades (Id) ON DELETE SET NULL,
        CONSTRAINT FK_IndorSvcReq_Proveedor FOREIGN KEY (ClaimedByProveedorId)
            REFERENCES dbo.IndorProveedores (Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_IndorSvcReq_User    ON dbo.IndorServiceRequests (UserId, Status);
    CREATE INDEX IX_IndorSvcReq_Open    ON dbo.IndorServiceRequests (CategoryId, Status);
    CREATE INDEX IX_IndorSvcReq_Claimed ON dbo.IndorServiceRequests (ClaimedByProveedorId);

    PRINT 'Created dbo.IndorServiceRequests.';
END
ELSE
BEGIN
    PRINT 'dbo.IndorServiceRequests already exists.';
END
GO

/* ============================================================
   2) IndorAppNotifications
   ============================================================ */
IF OBJECT_ID(N'dbo.IndorAppNotifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorAppNotifications (
        Id             INT            IDENTITY(1,1) NOT NULL,
        RecipientUserId NVARCHAR(450) NOT NULL,               -- AspNetUsers.Id (homeowner or provider)
        Audience       NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorNotif_Audience DEFAULT ('Homeowner'),
        TitleEn        NVARCHAR(200)  NOT NULL,
        TitleEs        NVARCHAR(200)  NOT NULL,
        BodyEn         NVARCHAR(500)  NULL,
        BodyEs         NVARCHAR(500)  NULL,
        CategoryTag    NVARCHAR(40)   NULL,
        IconClass      NVARCHAR(60)   NULL,
        TargetUrl      NVARCHAR(300)  NULL,
        IsRead         BIT            NOT NULL CONSTRAINT DF_IndorNotif_IsRead DEFAULT (0),
        ReadUtc        DATETIME2      NULL,
        FechaCreacion  DATETIME2      NOT NULL CONSTRAINT DF_IndorNotif_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorAppNotifications PRIMARY KEY CLUSTERED (Id)
    );

    CREATE INDEX IX_IndorNotif_Recipient ON dbo.IndorAppNotifications (RecipientUserId, IsRead, FechaCreacion DESC);

    PRINT 'Created dbo.IndorAppNotifications.';
END
ELSE
BEGIN
    PRINT 'dbo.IndorAppNotifications already exists.';
END
GO

PRINT 'Service request marketplace tables ready.';
GO
