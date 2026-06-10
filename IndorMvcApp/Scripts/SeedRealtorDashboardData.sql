/*
  INDOR — Demo data for Realtor portal (Home, Clients, Files, Quotes, Profile).
  Run after CreateRealtorRegistrationTables.sql and ExtendRealtorPortalTables.sql.
  Set @RealtorEmail to your test realtor account email.
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
    PRINT 'No realtor found for: ' + @RealtorEmail + '. Complete registration first.';
    RETURN;
END

UPDATE dbo.IndorRealtors
SET DisplayName = N'Ricardo Rivera',
    BrokerageName = N'Blue Ocean Realty',
    LicenseNumber = N'BK3456789',
    LicenseState = N'FL',
    ServiceAreas = N'Miami-Dade, Broward, Palm Beach',
    Phone = N'(305) 555-0198',
    RegistrationStatus = N'Basic',
    ProfileCompletedUtc = ISNULL(ProfileCompletedUtc, SYSUTCDATETIME())
WHERE Id = @RealtorId;

-- Property files
IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorPropertyFiles WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorRealtorPropertyFiles
        (RealtorId, Title, Address, CityRegion, Beds, Baths, SqFt, PhotoUrl, Status, FilePhase, ClientName, RepairItemsCount, QuotesReceivedCount, UpdatedUtc)
    VALUES
        (@RealtorId, N'Maple Street Home', N'123 Maple Street', N'Toronto, ON', 3, 2.5, 1850, N'/welcome-house.png', N'Active', N'Pre-Closing', N'Ana Martinez', 0, 0, DATEADD(HOUR, -2, SYSUTCDATETIME())),
        (@RealtorId, N'Oak Drive Listing', N'45 Oak Drive', N'Miami, FL', 4, 3.0, 2200, N'/welcome-house.png', N'Active', N'Repair Review', N'Carlos Ruiz', 5, 2, DATEADD(DAY, -1, SYSUTCDATETIME())),
        (@RealtorId, N'Bayview Condo', N'Bayview 12B Condo', N'Miami Beach, FL', 2, 2.0, 1150, N'/welcome-house.png', N'Active', N'Transfer', N'Sarah Johnson', 0, 1, DATEADD(DAY, -3, SYSUTCDATETIME()));
    PRINT 'Property files seeded.';
END

-- Quotes
DECLARE @QuoteCompareId INT;

IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorQuotes WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorRealtorQuotes
        (RealtorId, QuoteCode, Address, ServiceType, Status, ClientName, PhotoUrl, ProviderQuotesReceived, FooterNote, RequestedUtc, UpdatedUtc)
    VALUES
        (@RealtorId, N'Q-3821', N'123 Maple Street', N'Home Inspection', N'Pending', N'Ana Martinez', N'/welcome-house.png', 0, N'Waiting on providers', DATEADD(DAY, -5, SYSUTCDATETIME()), SYSUTCDATETIME()),
        (@RealtorId, N'Q-3814', N'45 Oak Drive', N'Environmental Test', N'Pending', N'Carlos Ruiz', N'/welcome-house.png', 1, N'1 quote received', DATEADD(DAY, -3, SYSUTCDATETIME()), DATEADD(DAY, -1, SYSUTCDATETIME())),
        (@RealtorId, N'Q-3802', N'123 Maple Street', N'HVAC Repair', N'Compare', N'Ana Martinez', N'/welcome-house.png', 3, N'3 quotes received', DATEADD(DAY, -7, SYSUTCDATETIME()), SYSUTCDATETIME());

    SET @QuoteCompareId = SCOPE_IDENTITY();

    SELECT @QuoteCompareId = Id FROM dbo.IndorRealtorQuotes
    WHERE RealtorId = @RealtorId AND QuoteCode = N'Q-3802';

    INSERT INTO dbo.IndorRealtorQuoteBids (QuoteId, ProviderName, Amount, Rating, SortOrder)
    VALUES
        (@QuoteCompareId, N'Safe HVAC Solution', 750.00, 4.8, 1),
        (@QuoteCompareId, N'Prime Mechanical', 820.00, 4.6, 2),
        (@QuoteCompareId, N'CoolAir Pros', 695.00, 4.9, 3);

    PRINT 'Quotes and bids seeded.';
END

-- Shared packages
IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorSharedPackages WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorRealtorSharedPackages (RealtorId, ClientName, Address, StatusLabel, SharedUtc)
    VALUES
        (@RealtorId, N'Sarah Johnson', N'123 Maple Street', N'Viewed by client', DATEADD(DAY, -3, SYSUTCDATETIME()));
    PRINT 'Shared packages seeded.';
END

-- Clients
IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorClients WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorRealtorClients
        (RealtorId, FullName, Email, ClientRole, PropertyAddress, StatusSummary, LastActiveUtc)
    VALUES
        (@RealtorId, N'Ana Martinez', N'ana.martinez@email.com', N'Buyer', N'123 Maple Street', N'Repair package viewed', DATEADD(HOUR, -3, SYSUTCDATETIME())),
        (@RealtorId, N'Carlos Ruiz', N'carlos.ruiz@email.com', N'Seller', N'45 Oak Drive', N'2 quotes pending', DATEADD(DAY, -1, SYSUTCDATETIME())),
        (@RealtorId, N'Maria Lopez', N'maria.lopez@email.com', N'Homeowner', N'Bayview 12B Condo', N'Job in progress', DATEADD(DAY, -2, SYSUTCDATETIME()));
    PRINT 'Clients seeded.';
END

-- Invitations
IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorInvitations WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorRealtorInvitations (RealtorId, FullName, Email, Status, SentUtc)
    VALUES
        (@RealtorId, N'David Thompson', N'david.thompson@email.com', N'Sent', DATEADD(DAY, -5, SYSUTCDATETIME())),
        (@RealtorId, N'Laura Chen', N'laura.chen@email.com', N'Sent', DATEADD(DAY, -2, SYSUTCDATETIME()));
    PRINT 'Invitations seeded.';
END

-- Activities
IF NOT EXISTS (SELECT 1 FROM dbo.IndorRealtorActivities WHERE RealtorId = @RealtorId)
BEGIN
    INSERT INTO dbo.IndorRealtorActivities (RealtorId, ActivityType, Description, CategoryTag, OccurredUtc)
    VALUES
        (@RealtorId, N'view', N'Ana Martinez viewed Repair Package for 123 Maple Street', N'Files', DATEADD(HOUR, -3, SYSUTCDATETIME())),
        (@RealtorId, N'quote', N'Carlos Ruiz received 2 quotes for 45 Oak Drive', N'Quotes', DATEADD(DAY, -1, SYSUTCDATETIME())),
        (@RealtorId, N'upload', N'Inspection report uploaded for 123 Maple Street', N'Files', DATEADD(HOUR, -5, SYSUTCDATETIME())),
        (@RealtorId, N'share', N'Repair package shared for 45 Oak Drive', N'Clients', DATEADD(DAY, -1, SYSUTCDATETIME())),
        (@RealtorId, N'job', N'Job completed for Bayview 12B Condo', N'Files', DATEADD(DAY, -5, SYSUTCDATETIME())),
        (@RealtorId, N'quote', N'Ana Martinez received 2 quotes for 123 Maple Street', N'Clients', DATEADD(DAY, -2, SYSUTCDATETIME())),
        (@RealtorId, N'link', N'CoolAir Pros submitted a quote for HVAC Repair', N'Providers', DATEADD(DAY, -1, SYSUTCDATETIME()));
    PRINT 'Activities seeded.';
END

PRINT 'Realtor portal seed complete for RealtorId ' + CAST(@RealtorId AS NVARCHAR(10));
