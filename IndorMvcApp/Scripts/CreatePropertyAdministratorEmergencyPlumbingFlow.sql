/*
  INDOR — Emergency Plumbing flow catalog link for property admin portal.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'EmergencyPlumbingDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-plumbing';

PRINT 'Emergency Plumbing catalog link updated.';
GO
