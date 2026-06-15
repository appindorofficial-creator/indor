/*
  INDOR — Furniture Haul Away flow catalog link.
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

MERGE dbo.IndorPropertyAdminServiceCatalog AS t
USING (VALUES
    (N'moving', N'Moving & Logistics', 5, N'Furniture Haul Away', N'furniture-haul-away', N'fa-couch', N'tone-blue', N'Administrador', N'FurnitureHaulAwayDetails', 3)
) AS s (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, Orden)
ON t.ServiceSlug = s.ServiceSlug
WHEN MATCHED THEN
    UPDATE SET
        CategoryKey = s.CategoryKey,
        CategoryTitle = s.CategoryTitle,
        CategoryOrder = s.CategoryOrder,
        ServiceName = s.ServiceName,
        IconClass = s.IconClass,
        ToneClass = s.ToneClass,
        LinkController = s.LinkController,
        LinkAction = s.LinkAction,
        Orden = s.Orden,
        Activo = 1
WHEN NOT MATCHED THEN
    INSERT (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, Orden)
    VALUES (s.CategoryKey, s.CategoryTitle, s.CategoryOrder, s.ServiceName, s.ServiceSlug, s.IconClass, s.ToneClass, s.LinkController, s.LinkAction, s.Orden);

PRINT 'Furniture Haul Away catalog link updated.';
GO
