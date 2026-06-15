/*
  INDOR — Trash Out flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'TrashOutDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'trashout';

PRINT 'Trash Out catalog link updated.';
GO
