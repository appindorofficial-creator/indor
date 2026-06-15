/*
  INDOR — Air Filter Change flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
  Service request columns (DetailsJson, Technician*, TimelineStep) are added by Emergency AC script.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'AirFilterDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'hvac-filter';

PRINT 'Air Filter Change catalog link updated.';
GO
