/*
  INDOR — Linen / Supply Restock flow catalog link.
  Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'LinenRestockDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'linen-restock';

PRINT 'Linen / Supply Restock catalog link updated.';
GO
