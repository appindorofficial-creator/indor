/*
  INDOR — Full property administrator service catalog (matches Services tab mockup).
  Run after CreatePropertyAdministratorPortalTables.sql. Safe to run multiple times.
*/

MERGE dbo.IndorPropertyAdminServiceCatalog AS t
USING (VALUES
    -- Homecare & Maintenance
    (N'homecare', N'Homecare & Maintenance', 1, N'Preventive Maintenance', N'preventive-maintenance', N'fa-screwdriver-wrench', N'tone-blue', N'Administrador', N'PreventiveMaintenanceServices', NULL, 1),
    (N'homecare', N'Homecare & Maintenance', 1, N'HVAC Filter Change', N'hvac-filter', N'fa-fan', N'tone-blue', N'Administrador', N'AirFilterDetails', NULL, 2),
    (N'homecare', N'Homecare & Maintenance', 1, N'Smoke Detector Check', N'smoke-detector', N'fa-bell', N'tone-green', N'Administrador', N'SmokeDetectorDetails', NULL, 3),

    -- Cleaning & Turnover
    (N'cleaning', N'Cleaning & Turnover', 2, N'Turnover Cleaning', N'turnover-cleaning', N'fa-broom', N'tone-purple', N'Administrador', N'TurnoverCleaningDetails', NULL, 1),
    (N'cleaning', N'Cleaning & Turnover', 2, N'Standard Cleaning', N'standard-cleaning', N'fa-spray-can-sparkles', N'tone-purple', N'Administrador', N'StandardCleaningDetails', NULL, 2),
    (N'cleaning', N'Cleaning & Turnover', 2, N'Pet Deep Clean', N'pet-deep-clean', N'fa-paw', N'tone-purple', N'Administrador', N'PetDeepCleanDetails', NULL, 3),
    (N'cleaning', N'Cleaning & Turnover', 2, N'Linen / Supply Restock', N'linen-restock', N'fa-box', N'tone-purple', NULL, NULL, NULL, 4),
    (N'cleaning', N'Cleaning & Turnover', 2, N'Trashout', N'trashout', N'fa-dumpster', N'tone-purple', N'Administrador', N'TrashOutDetails', NULL, 5),

    -- Outdoor & Exterior
    (N'outdoor', N'Outdoor & Exterior', 3, N'Lawn Care / Grass Cutting', N'lawn-care', N'fa-seedling', N'tone-green', N'Administrador', N'LawnCareDetails', NULL, 1),
    (N'outdoor', N'Outdoor & Exterior', 3, N'Landscaping', N'landscaping', N'fa-leaf', N'tone-green', N'Administrador', N'LandscapingDetails', NULL, 2),
    (N'outdoor', N'Outdoor & Exterior', 3, N'Pressure Washing', N'pressure-washing', N'fa-spray-can', N'tone-green', N'Administrador', N'PressureWashingDetails', NULL, 3),
    (N'outdoor', N'Outdoor & Exterior', 3, N'Pest Control', N'pest-control', N'fa-bug', N'tone-green', N'Administrador', N'PestControlDetails', NULL, 4),
    (N'outdoor', N'Outdoor & Exterior', 3, N'Pool / Hot Tub Service', N'pool-hot-tub', N'fa-water-ladder', N'tone-green', N'Administrador', N'PoolHotTubDetails', NULL, 5),

    -- Moving & Logistics
    (N'moving', N'Moving & Logistics', 4, N'Moving Help', N'moving-help', N'fa-truck', N'tone-blue', N'Administrador', N'MovingHelpDetails', NULL, 1),
    (N'moving', N'Moving & Logistics', 4, N'Junk Removal', N'junk-removal', N'fa-dolly', N'tone-blue', N'Administrador', N'JunkRemovalDetails', NULL, 2),
    (N'moving', N'Moving & Logistics', 4, N'Furniture Haul Away', N'furniture-haul-away', N'fa-couch', N'tone-blue', N'Administrador', N'FurnitureHaulAwayDetails', NULL, 3),

    -- Emergency Services last (CategoryOrder 99 — older seeds used 1)
    (N'emergency', N'Emergency Services', 99, N'Emergency AC', N'emergency-ac', N'fa-snowflake', N'tone-red', N'Administrador', N'EmergencyAcDetails', NULL, 1),
    (N'emergency', N'Emergency Services', 99, N'Emergency Plumbing', N'emergency-plumbing', N'fa-droplet', N'tone-red', N'Administrador', N'EmergencyPlumbingDetails', NULL, 2),
    (N'emergency', N'Emergency Services', 99, N'Emergency Electrical', N'emergency-electrical', N'fa-bolt', N'tone-red', N'Administrador', N'EmergencyElectricalDetails', NULL, 3),
    (N'emergency', N'Emergency Services', 99, N'Emergency Flood', N'emergency-flood', N'fa-water', N'tone-red', N'Administrador', N'EmergencyFloodDetails', NULL, 4),
    (N'emergency', N'Emergency Services', 99, N'Emergency Roof Leak', N'emergency-roof-leak', N'fa-house-chimney-crack', N'tone-red', N'Administrador', N'EmergencyRoofLeakDetails', NULL, 5),
    (N'emergency', N'Emergency Services', 99, N'Tree / Branch Emergency', N'emergency-tree-branch', N'fa-tree', N'tone-red', N'Administrador', N'EmergencyTreeBranchDetails', NULL, 6),
    (N'emergency', N'Emergency Services', 99, N'Lockout / Access', N'lockout-access', N'fa-key', N'tone-red', N'Administrador', N'LockoutAccessDetails', NULL, 7),
    (N'emergency', N'Emergency Services', 99, N'Broken Window / Board-Up', N'broken-window-board-up', N'fa-window-maximize', N'tone-red', N'Administrador', N'BrokenWindowDetails', NULL, 8),
    (N'emergency', N'Emergency Services', 99, N'Sewer Backup', N'sewer-backup', N'fa-toilet', N'tone-red', N'Administrador', N'SewerBackupDetails', NULL, 9),
    (N'emergency', N'Emergency Services', 99, N'Water Heater Emergency', N'emergency-water-heater', N'fa-fire-flame-simple', N'tone-red', N'Administrador', N'WaterHeaterDetails', NULL, 10)
) AS s (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, LinkRouteId, Orden)
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
        LinkRouteId = s.LinkRouteId,
        Orden = s.Orden,
        Activo = 1
WHEN NOT MATCHED THEN
    INSERT (CategoryKey, CategoryTitle, CategoryOrder, ServiceName, ServiceSlug, IconClass, ToneClass, LinkController, LinkAction, LinkRouteId, Orden)
    VALUES (s.CategoryKey, s.CategoryTitle, s.CategoryOrder, s.ServiceName, s.ServiceSlug, s.IconClass, s.ToneClass, s.LinkController, s.LinkAction, s.LinkRouteId, s.Orden);

-- Older seeds used CategoryOrder=1 for emergency and rendered it first.
UPDATE dbo.IndorPropertyAdminServiceCatalog
SET CategoryOrder = 99
WHERE CategoryKey = N'emergency';

PRINT 'Property administrator service catalog synced to full Services mockup.';
GO
