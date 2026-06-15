/*
  INDOR — Pool & Hot Tub flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'PoolHotTubDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'pool-hot-tub';

PRINT 'Pool & Hot Tub catalog link updated.';
GO
