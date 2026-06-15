/*
  INDOR — Emergency Electrical flow catalog link for property admin portal.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'EmergencyElectricalDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-electrical';

PRINT 'Emergency Electrical catalog link updated.';
GO
