/*
  INDOR — Pest Control flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'PestControlDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'pest-control';

PRINT 'Pest Control catalog link updated.';
GO
