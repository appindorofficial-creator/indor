/*
  INDOR — Emergency AC flow columns for property admin service requests.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

IF OBJECT_ID(N'dbo.IndorPropertyAdminServiceRequests', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.IndorPropertyAdminServiceRequests', N'DetailsJson') IS NULL
        ALTER TABLE dbo.IndorPropertyAdminServiceRequests ADD DetailsJson NVARCHAR(4000) NULL;

    IF COL_LENGTH(N'dbo.IndorPropertyAdminServiceRequests', N'TechnicianName') IS NULL
        ALTER TABLE dbo.IndorPropertyAdminServiceRequests ADD TechnicianName NVARCHAR(80) NULL;

    IF COL_LENGTH(N'dbo.IndorPropertyAdminServiceRequests', N'TechnicianRating') IS NULL
        ALTER TABLE dbo.IndorPropertyAdminServiceRequests ADD TechnicianRating DECIMAL(3,1) NULL;

    IF COL_LENGTH(N'dbo.IndorPropertyAdminServiceRequests', N'TechnicianTitle') IS NULL
        ALTER TABLE dbo.IndorPropertyAdminServiceRequests ADD TechnicianTitle NVARCHAR(80) NULL;

    IF COL_LENGTH(N'dbo.IndorPropertyAdminServiceRequests', N'VehicleLabel') IS NULL
        ALTER TABLE dbo.IndorPropertyAdminServiceRequests ADD VehicleLabel NVARCHAR(80) NULL;

    IF COL_LENGTH(N'dbo.IndorPropertyAdminServiceRequests', N'TimelineStep') IS NULL
        ALTER TABLE dbo.IndorPropertyAdminServiceRequests ADD TimelineStep INT NOT NULL
            CONSTRAINT DF_PropAdminReq_TimelineStep DEFAULT (0);

    PRINT 'Emergency AC columns added to IndorPropertyAdminServiceRequests.';
END
GO

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'EmergencyAcDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-ac';

PRINT 'Emergency AC catalog link updated.';
GO
