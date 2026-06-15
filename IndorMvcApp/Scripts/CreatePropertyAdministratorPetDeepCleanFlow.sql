/*
  INDOR — Pet Deep Clean flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'PetDeepCleanDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'pet-deep-clean';

PRINT 'Pet Deep Clean catalog link updated.';
GO
