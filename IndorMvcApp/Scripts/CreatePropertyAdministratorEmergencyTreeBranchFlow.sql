/*
  INDOR — Tree / Branch Emergency flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Administrador', LinkAction = N'EmergencyTreeBranchDetails', LinkRouteId = NULL
WHERE ServiceSlug = N'emergency-tree-branch';

PRINT 'Tree / Branch Emergency catalog link updated.';
GO
