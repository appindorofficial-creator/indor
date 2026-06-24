/*
  INDOR — Fix Nearby Network feed images.
  Repoints property cards (listings / open houses) to real house exterior photos
  instead of inspection placeholders, and keeps service/lead/emergency visuals.
  Safe to run multiple times on existing databases.
*/

/*
  Property cards (listings / open houses) were showing inspection photos
  (plumbing, interiors) that don't match a "home for sale". Repoint them to
  real house exterior photos, with a distinct image per card.
*/
UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/listing-home-1.png'
WHERE CardType = N'Listing'
  AND Title LIKE N'$589%';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/listing-home-2.png'
WHERE CardType = N'Listing'
  AND Title LIKE N'%Ballantyne%';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/openhouse-home.png'
WHERE CardType = N'OpenHouse';

-- Any other property card still pointing to an inspection/generic photo gets a house photo.
UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/listing-home-1.png'
WHERE CardType IN (N'Listing', N'OpenHouse')
  AND (ImageUrl IS NULL OR ImageUrl LIKE N'/inspeccion%');

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/aire.jpeg'
WHERE CardType = N'Provider'
  AND (
      ImageUrl IS NULL
      OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg')
      OR ImageUrl LIKE N'%listing-home%'
      OR ImageUrl LIKE N'%openhouse-home%'
      OR ImageUrl LIKE N'%welcome-house%'
  )
  AND (Title LIKE N'%AC%' OR Title LIKE N'%HVAC%' OR Subtitle LIKE N'%HVAC%' OR ProviderName LIKE N'%HVAC%');

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/servicio4.jpeg'
WHERE CardType = N'Provider'
  AND (
      ImageUrl IS NULL
      OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg')
      OR ImageUrl LIKE N'%listing-home%'
      OR ImageUrl LIKE N'%openhouse-home%'
      OR ImageUrl LIKE N'%welcome-house%'
  );

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = N'/servicio4.jpeg'
WHERE CardType = N'Promotion'
  AND (
      ImageUrl IS NULL
      OR ImageUrl IN (N'/inspeccion2.jpeg', N'/inspeccion1.jpeg')
      OR ImageUrl LIKE N'%listing-home%'
      OR ImageUrl LIKE N'%openhouse-home%'
  );

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = NULL,
    IconClass = COALESCE(IconClass, N'fa-users')
WHERE CardType = N'Lead';

UPDATE dbo.IndorNearbyNetworkItems
SET ImageUrl = NULL,
    IconClass = COALESCE(IconClass, N'fa-bell')
WHERE CardType = N'Emergency';

PRINT 'Nearby network feed images updated.';
GO
