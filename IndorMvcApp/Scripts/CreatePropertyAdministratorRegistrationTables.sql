/*
  INDOR — Property Administrator (Multi-Property Owner) registration.
  Run on IndorDB. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorPropertyAdministrators', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdministrators (
        Id                          INT              IDENTITY(1,1) NOT NULL,
        UserId                      NVARCHAR(450)    NULL,
        RegistrationToken           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_IndorPropAdmin_Token DEFAULT (NEWID()),
        RegistrationStatus          NVARCHAR(30)     NOT NULL CONSTRAINT DF_IndorPropAdmin_Status DEFAULT (N'Draft'),
        CurrentStep                 INT              NOT NULL CONSTRAINT DF_IndorPropAdmin_Step DEFAULT (1),
        DisplayName                 NVARCHAR(120)    NULL,
        Email                       NVARCHAR(256)    NULL,
        Phone                       NVARCHAR(30)     NULL,
        PortfolioBusinessName       NVARCHAR(200)    NULL,
        TermsAccepted               BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_Terms DEFAULT (0),
        MarketingOptIn              BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_Marketing DEFAULT (0),
        TermsAcceptedUtc            DATETIME2(7)     NULL,
        PropertyCountRange          NVARCHAR(20)     NULL,
        PortfolioType               NVARCHAR(40)     NULL,
        OwnershipType               NVARCHAR(40)     NULL,
        PrimaryMarket               NVARCHAR(120)    NULL,
        ManagementStyle             NVARCHAR(40)     NULL,
        ToolMaintenanceRequests     BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolMaint DEFAULT (0),
        ToolTurnoverCleaning        BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolClean DEFAULT (0),
        ToolGuestMessaging          BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolMsg DEFAULT (0),
        ToolInvoicesPayments        BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolInv DEFAULT (0),
        ToolDocumentsWarranties     BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolDoc DEFAULT (0),
        ToolServiceProviders        BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolProv DEFAULT (0),
        ToolTeamAccess              BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_ToolTeam DEFAULT (0),
        NotifyUrgentMaintenance     BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_NotifyUrgent DEFAULT (1),
        NotifyWeeklySummary         BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_NotifyWeekly DEFAULT (1),
        NotifyBookingLeaseUpdates   BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_NotifyLease DEFAULT (1),
        PlatformTermsAccepted       BIT              NOT NULL CONSTRAINT DF_IndorPropAdmin_PlatformTerms DEFAULT (0),
        RegistrationCompletedUtc    DATETIME2(7)     NULL,
        FechaCreacion               DATETIME2(7)     NOT NULL CONSTRAINT DF_IndorPropAdmin_Created DEFAULT (SYSUTCDATETIME()),
        FechaActualizacion          DATETIME2(7)     NULL,
        CONSTRAINT PK_IndorPropertyAdministrators PRIMARY KEY CLUSTERED (Id)
    );

    CREATE UNIQUE INDEX UX_IndorPropAdmin_Token ON dbo.IndorPropertyAdministrators (RegistrationToken);
    CREATE INDEX IX_IndorPropAdmin_UserId ON dbo.IndorPropertyAdministrators (UserId);

    ALTER TABLE dbo.IndorPropertyAdministrators WITH CHECK ADD CONSTRAINT FK_IndorPropAdmin_AspNetUsers
        FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers (Id);

    PRINT 'Table IndorPropertyAdministrators created.';
END
ELSE
    PRINT 'Table IndorPropertyAdministrators already exists.';
GO

IF OBJECT_ID(N'dbo.IndorPropertyAdminPortfolioProperties', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPropertyAdminPortfolioProperties (
        Id              INT            IDENTITY(1,1) NOT NULL,
        AdministratorId INT            NOT NULL,
        PropertyName    NVARCHAR(150)  NOT NULL,
        Location        NVARCHAR(200)  NOT NULL,
        PropertyType    NVARCHAR(40)   NOT NULL,
        ImageUrl        NVARCHAR(300)  NULL,
        PropiedadId     INT            NULL,
        Status          NVARCHAR(20)   NOT NULL CONSTRAINT DF_IndorPropAdminProp_Status DEFAULT (N'Added'),
        FechaCreacion   DATETIME2(7)   NOT NULL CONSTRAINT DF_IndorPropAdminProp_Created DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorPropertyAdminPortfolioProperties PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_IndorPropAdminProp_Admin FOREIGN KEY (AdministratorId)
            REFERENCES dbo.IndorPropertyAdministrators(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IndorPropAdminProp_Propiedad FOREIGN KEY (PropiedadId)
            REFERENCES dbo.Propiedades(Id)
    );

    CREATE INDEX IX_IndorPropAdminProp_Admin ON dbo.IndorPropertyAdminPortfolioProperties(AdministratorId);

    PRINT 'Table IndorPropertyAdminPortfolioProperties created.';
END
ELSE
    PRINT 'Table IndorPropertyAdminPortfolioProperties already exists.';
GO
