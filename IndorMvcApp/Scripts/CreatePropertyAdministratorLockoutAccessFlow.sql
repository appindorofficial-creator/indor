/*
  INDOR — Lockout / Access flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'LockoutAccessDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'lockout-access';

PRINT 'Lockout / Access catalog link updated.';
GO
