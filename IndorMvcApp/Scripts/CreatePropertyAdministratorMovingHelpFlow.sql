/*
  INDOR — Moving Help flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'MovingHelpDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'moving-help';

PRINT 'Moving Help catalog link updated.';
GO
