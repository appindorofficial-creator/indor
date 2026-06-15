/*
  INDOR — Lawn Care flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'LawnCareDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'lawn-care';

PRINT 'Lawn Care catalog link updated.';
GO
