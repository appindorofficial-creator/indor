/*
  INDOR — Broken Window / Board-Up flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'BrokenWindowDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'broken-window-board-up';

PRINT 'Broken Window / Board-Up catalog link updated.';
GO
