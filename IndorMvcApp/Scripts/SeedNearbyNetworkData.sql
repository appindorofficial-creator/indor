/*
  INDOR — OPTIONAL demo seed for Nearby Network (local testing only).
  Production does NOT need this script — realtors create listings in the app
  and coordinates are saved automatically when they enter an address.
  Run after CreateNearbyNetworkTables.sql only if you want sample data.
*/

DECLARE @RealtorEmail NVARCHAR(256) = N'realtor@indor.test';
DECLARE @RealtorId INT;

SELECT @RealtorId = r.Id
FROM dbo.IndorRealtors r
INNER JOIN dbo.AspNetUsers u ON u.Id = r.UserId
WHERE u.Email = @RealtorEmail;

IF @RealtorId IS NULL
BEGIN
    SELECT @RealtorId = r.Id FROM dbo.IndorRealtors r WHERE r.Email = @RealtorEmail;
END

IF @RealtorId IS NULL
BEGIN
    SELECT TOP 1 @RealtorId = r.Id FROM dbo.IndorRealtors r ORDER BY r.Id;
END

IF @RealtorId IS NULL
BEGIN
    PRINT 'No realtor found. Complete registration first.';
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM dbo.IndorNearbyNetworkSettings WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorNearbyNetworkSettings
        (RealtorId, CenterLabel, CenterAddress, CenterLatitude, CenterLongitude, RadiusMiles)
    VALUES
        (@RealtorId, N'Charlotte, NC area', N'Charlotte, NC', 35.227086, -80.843124, 3.0);
    PRINT 'Nearby network settings seeded.';
END

IF NOT EXISTS (SELECT 1 FROM dbo.IndorNearbyNetworkItems)
BEGIN
    INSERT INTO dbo.IndorNearbyNetworkItems
        (OwnerRealtorId, CardType, FilterCategory, BadgeLabel, BadgeCss, Title, Subtitle, Price, Bedrooms, Bathrooms, SquareFeet, SpecsLabel,
         ImageUrl, DistanceMiles, Latitude, Longitude, StatusBadge, StatusCss, PrimaryActionLabel, PrimaryActionUrl, SecondaryActionLabel, SecondaryActionUrl,
         IsOwnedListing, SortOrder)
    VALUES
        (@RealtorId, N'Listing', N'Homes', N'MY LISTING', N'listing', N'$589,900', N'1234 Maple Ridge Dr, Charlotte, NC 28211', 589900, 4, 3.5, 2850,
         N'4 Beds · 3.5 Baths · 2,850 sqft', N'/listing-home-1.png', 1.20, 35.189400, -80.790000, N'ACTIVE', N'active', N'View Home', N'/Realtor/ViewNetworkListing/1', N'Edit Listing', N'/Realtor/EditNetworkListing/1',
         1, 10),

        (NULL, N'OpenHouse', N'OpenHouses', N'OPEN HOUSE', N'openhouse', N'Open House This Saturday', N'8921 Providence Rd, Charlotte, NC 28277', NULL, 4, 3, 3100,
         N'4 Beds · 3 Baths · 3,100 sqft', N'/openhouse-home.png', 2.10, 35.055000, -80.770000, N'OPEN HOUSE', N'openhouse', N'View Details', N'/Realtor/ViewNetworkListing/2', N'Share', N'/Realtor/PublicProfile',
         0, 20),

        (NULL, N'Lead', N'Leads', N'NEW LEAD', N'lead', N'Buyer Interested Nearby', N'Looking for a 3–4 bed home in Charlotte', NULL, NULL, NULL, NULL,
         N'Budget $450K – $650K', NULL, 1.80, NULL, NULL, N'LEAD REQUEST', N'lead', N'View Lead', N'/Realtor/Clients?filter=Buyers', N'Contact Buyer', N'/RealtorInviteClient/New',
         0, 30),

        (NULL, N'Provider', N'Providers', N'PROVIDER', N'provider', N'AC Tune-Up Special This Week', N'Climate Solutions HVAC', NULL, NULL, NULL, NULL,
         NULL, N'/aire.jpeg', 1.30, 35.210000, -80.830000, N'PROMOTION', N'promotion', N'View Offer', N'/Realtor/ProviderNetwork?q=Climate%20Solutions%20HVAC', NULL, NULL,
         0, 40),

        (NULL, N'Promotion', N'Promotions', N'PROMOTION', N'promotion', N'We''re working in your neighborhood today!', N'Carolina Home Services · 10% off same-day requests', NULL, NULL, NULL, NULL,
         NULL, N'/servicio4.jpeg', 2.10, 35.200000, -80.820000, N'PROMOTION', N'promotion', N'Ask for Info', N'/Realtor/ProviderNetwork?q=Carolina%20Home%20Services', N'Request Service', N'/Realtor/Quotes',
         0, 50),

        (NULL, N'Emergency', N'Emergency', N'EMERGENCY', N'emergency', N'Emergency Help Nearby', N'Licensed plumbers responding in your ZIP', NULL, NULL, NULL, NULL,
         NULL, NULL, 2.60, NULL, NULL, N'EMERGENCY', N'emergency', N'Call Now', N'/Realtor/ProviderNetwork?filter=Verified', NULL, NULL,
         0, 60),

        (NULL, N'Listing', N'Homes', N'HOME FOR SALE', N'listing', N'Stunning Home in Ballantyne', N'7404 Wallace Ln, Charlotte, NC 28212', 725000, 4, 3.5, 2850,
         N'4 Beds · 3.5 Baths · 2,850 sqft', N'/listing-home-2.png', 1.20, 35.187000, -80.750000, N'NEW', N'new', N'View Listing', N'/Realtor/ViewNetworkListing/7', N'I''m Interested', N'/RealtorInviteClient/New',
         0, 70);

    UPDATE dbo.IndorNearbyNetworkItems
    SET IconClass = N'fa-users'
    WHERE CardType = N'Lead';

    UPDATE dbo.IndorNearbyNetworkItems
    SET IconClass = N'fa-bell', MetaLabel = N'Average arrival time: 28 mins'
    WHERE CardType = N'Emergency';

    UPDATE dbo.IndorNearbyNetworkItems
    SET TagsJson = N'["Verified Provider","Insurance Active"]', ProviderName = N'Climate Solutions HVAC'
    WHERE CardType = N'Provider';

    UPDATE dbo.IndorNearbyNetworkItems
    SET MetaLabel = N'Sat, May 17 · 12:00–3:00 PM'
    WHERE CardType = N'OpenHouse';

    UPDATE dbo.IndorNearbyNetworkItems
    SET MetaLabel = N'Requested 2 hours ago'
    WHERE CardType = N'Lead';

    PRINT 'Nearby network feed items seeded.';
END
GO
