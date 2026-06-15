/*
  INDOR — Emergency Flood flow catalog link for property admin portal.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'EmergencyFloodDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-flood';

PRINT 'Emergency Flood catalog link updated.';
GO
