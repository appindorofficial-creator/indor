/*
  INDOR — Standard Cleaning flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'StandardCleaningDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'standard-cleaning';

PRINT 'Standard Cleaning catalog link updated.';
GO
