/*
  INDOR — Fix Nearby Network feed images that all pointed to /inspeccion2.jpeg.
  Safe to run multiple times on existing databases.
*/

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/inspeccion3.jpeg'
WHERE CardType = N'Listing'
  AND (ImageUrl IS NULL OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg'))
  AND Title LIKE N'$589%';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/inspeccion7.jpeg'
WHERE CardType = N'Listing'
  AND (ImageUrl IS NULL OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg'))
  AND Title LIKE N'%Ballantyne%';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/inspeccion5.jpeg'
WHERE CardType = N'OpenHouse'
  AND (ImageUrl IS NULL OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg'));

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/aire.jpeg'
WHERE CardType = N'Provider'
  AND (ImageUrl IS NULL OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg'))
  AND (Title LIKE N'%AC%' OR Title LIKE N'%HVAC%' OR Subtitle LIKE N'%HVAC%' OR ProviderName LIKE N'%HVAC%');

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/servicio4.jpeg'
WHERE CardType = N'Promotion'
  AND (ImageUrl IS NULL OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg'));

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = NULL,
    IconClass = COALESCE(IconClass, N'fa-users')
WHERE CardType = N'Lead';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = NULL,
    IconClass = COALESCE(IconClass, N'fa-bell')
WHERE CardType = N'Emergency';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/inspeccion9.jpeg'
WHERE CardType = N'Listing'
  AND ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg');

PRINT 'Nearby network feed images updated.';
GO
