/*
  INDOR — Add Outdoor & Exterior services to Multi-Property Owner catalog.
  Run on Azure if Services page is missing the Outdoor section.
  Safe to run multiple times.
*/

DECLARE @LawnMicroId INT = (SELECT TOP 1 Id FROM dbo.Microservicios WHERE Nombre = N'Always Perfect Lawn' ORDER BY Id);
DECLARE @PowerWashPriorityId INT = (SELECT TOP 1 Id FROM dbo.HomeCarePriorities WHERE Nombre = N'Power wash exterior' ORDER BY Id);
DECLARE @PestPriorityId INT = (SELECT TOP 1 Id FROM dbo.HomeCarePriorities WHERE Nombre = N'Pest control' ORDER BY Id);

IF OBJECT_ID(N'dbo.IndorPropertyAdminServiceCatalog', N'U') IS NULL
BEGIN
    RAISERROR('Run CreatePropertyAdministratorPortalTables.sql first.', 16, 1);
    RETURN;
END

MERGE dbo.IndorPropertyAdminServiceCatalog AS t
USING (VALUES
    (N'outdoor', N'Outdoor & Exterior', 4, N'Lawn Care / Grass Cutting', N'lawn-care', N'fa-seedling', N'tone-green', N'Lawn', N'LawnService', @LawnMicroId, 1),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Landscaping', N'landscaping', N'fa-leaf', N'tone-green', NULL, NULL, NULL, 2),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pressure Washing', N'pressure-washing', N'fa-spray-can', N'tone-green', N'PowerWash', N'PowerWashService', @PowerWashPriorityId, 3),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pest Control', N'pest-control', N'fa-bug', N'tone-green', N'PestControl', N'PestControlService', @PestPriorityId, 4),
    (N'outdoor', N'Outdoor & Exterior', 4, N'Pool / Hot Tub Service', N'pool-hot-tub', N'fa-water-ladder', N'tone-green', NULL, NULL, NULL, 5),
    (N'cleaning', N'Cleaning & Turnover', 3, N'Linen / Supply Restock', N'linen-restock', N'fa-box', N'tone-purple', NULL, NULL, NULL, 4)
) AS s (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, LinkRouteId, Orden)
ON t.ServiceSlug = s.ServiceSlug
WHEN NOT MATCHED THEN
    INSERT (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, LinkRouteId, Orden)
    VALUES (s.CategoryKey, s.CategoryTitle, s.CategoryOrder, s.ServiceName, s.ServiceSlug, s.IconClass, s.ToneClass, s.LinkController, s.LinkAction, s.LinkRouteId, s.Orden);

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET CategoryKey = N'cleaning', CategoryTitle = N'Cleaning & Turnover', CategoryOrder = 3, Orden = 5
WHERE ServiceSlug = N'trashout' AND CategoryKey = N'moving';

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET CategoryOrder = 5
WHERE CategoryKey = N'moving';

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'Lawn', LinkAction = N'LawnService', LinkRouteId = @LawnMicroId
WHERE ServiceSlug = N'lawn-care' AND @LawnMicroId IS NOT NULL;

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'PowerWash', LinkAction = N'PowerWashService', LinkRouteId = @PowerWashPriorityId
WHERE ServiceSlug = N'pressure-washing' AND @PowerWashPriorityId IS NOT NULL;

UPDATE dbo.IndorPropertyAdminServiceCatalog
SET LinkController = N'PestControl', LinkAction = N'PestControlService', LinkRouteId = @PestPriorityId
WHERE ServiceSlug = N'pest-control' AND @PestPriorityId IS NOT NULL;

PRINT 'Outdoor & Exterior services added to property admin catalog.';
GO
