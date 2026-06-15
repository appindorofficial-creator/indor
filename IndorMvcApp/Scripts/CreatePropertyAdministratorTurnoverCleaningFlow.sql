/*
  INDOR — Turnover Cleaning flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'TurnoverCleaningDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'turnover-cleaning';

PRINT 'Turnover Cleaning catalog link updated.';
GO
