/*
  INDOR — Emergency Roof Leak flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'EmergencyRoofLeakDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-roof-leak';

PRINT 'Emergency Roof Leak catalog link updated.';
GO
