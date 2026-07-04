/*
  INDOR — Remove placeholder / demo service providers (App Store guideline 2.1(a)).

  Deletes the fictional sample providers that were previously auto-seeded or added
  by optional demo scripts, so the app only ever shows real, registered providers
  (dbo.IndorProveedores) or a proper empty state.

  Safe to run multiple times and safe to run on production.
*/

SET NOCOUNT ON;

-------------------------------------------------------------------------------
-- 1) Realtor quote / inspection provider catalog (IndorRealtorQuoteProviders)
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.IndorRealtorQuoteProviders', N'U') IS NOT NULL
BEGIN
    DECLARE @fakeCompanies TABLE (Name NVARCHAR(120));
    INSERT INTO @fakeCompanies (Name) VALUES
        (N'Safe HVAC Solution'),
        (N'Prime Mechanical'),
        (N'CoolAir Pros'),
        (N'Elite Roofing Co'),
        (N'Quick Fix Plumbing'),
        (N'Safe Electric Co.'),
        (N'Prime Home Services');

    -- Remove dependent references first (draft/sent selections), then the rows.
    IF OBJECT_ID(N'dbo.IndorRealtorQuoteRequestDraftProviders', N'U') IS NOT NULL
        DELETE dp
        FROM dbo.IndorRealtorQuoteRequestDraftProviders dp
        INNER JOIN dbo.IndorRealtorQuoteProviders p ON p.Id = dp.ProviderId
        INNER JOIN @fakeCompanies f ON f.Name = p.CompanyName;

    IF OBJECT_ID(N'dbo.IndorRealtorInspectionDraftProviders', N'U') IS NOT NULL
        DELETE dp
        FROM dbo.IndorRealtorInspectionDraftProviders dp
        INNER JOIN dbo.IndorRealtorQuoteProviders p ON p.Id = dp.ProviderId
        INNER JOIN @fakeCompanies f ON f.Name = p.CompanyName;

    DELETE p
    FROM dbo.IndorRealtorQuoteProviders p
    INNER JOIN @fakeCompanies f ON f.Name = p.CompanyName;

    PRINT 'Removed placeholder rows from IndorRealtorQuoteProviders.';
END
GO

-------------------------------------------------------------------------------
-- 2) Nearby network demo items (from optional SeedNearbyNetworkData.sql)
-------------------------------------------------------------------------------
IF OBJECT_ID(N'dbo.IndorNearbyNetworkItems', N'U') IS NOT NULL
BEGIN
    DELETE FROM dbo.IndorNearbyNetworkItems
    WHERE ProviderName IN (N'Climate Solutions HVAC', N'Carolina Home Services')
       OR Subtitle LIKE N'%Climate Solutions HVAC%'
       OR Subtitle LIKE N'%Carolina Home Services%';

    PRINT 'Removed placeholder provider rows from IndorNearbyNetworkItems (if any).';
END
GO

PRINT 'Placeholder provider cleanup complete.';
