/*
  OPCIONAL — Datos de demo para mensajes INDOR PRO.
  Ejecutar después de CreateProviderMessagesTables.sql y SeedProviderProOperationsData.sql.
*/

DECLARE @ProveedorEmail NVARCHAR(256) = NULL;
DECLARE @ProveedorId INT = NULL;

IF @ProveedorId IS NULL AND @ProveedorEmail IS NOT NULL
    SELECT TOP 1 @ProveedorId = Id
    FROM dbo.IndorProveedores
    WHERE Email = @ProveedorEmail
    ORDER BY FechaCreacion DESC;

IF @ProveedorId IS NULL
    SELECT TOP 1 @ProveedorId = Id
    FROM dbo.IndorProveedores
    WHERE UserId IS NOT NULL
    ORDER BY FechaCreacion DESC;

IF @ProveedorId IS NULL
BEGIN
    PRINT 'Seed omitido: no hay proveedores registrados.';
    RETURN;
END

IF EXISTS (SELECT 1 FROM dbo.IndorProveedorConversations WHERE ProveedorId = @ProveedorId)
BEGIN
    PRINT 'Messages already seeded for ProveedorId = ' + CAST(@ProveedorId AS nvarchar(20)) + '. Skipping.';
    RETURN;
END

DECLARE @ClienteJamesId INT;
DECLARE @ClienteMariaId INT;
DECLARE @ClienteRobertId INT;
DECLARE @JobWaterHeaterId INT;
DECLARE @JobHvacId INT;
DECLARE @LeadRoofId INT;
DECLARE @LeadPlumbingId INT;
DECLARE @ConvJamesId INT;
DECLARE @ConvMariaId INT;
DECLARE @ConvRobertId INT;
DECLARE @ConvLeadId INT;

SELECT @ClienteJamesId = Id FROM dbo.IndorProveedorClientes WHERE ProveedorId = @ProveedorId AND Name = N'James Smith';
SELECT @ClienteMariaId = Id FROM dbo.IndorProveedorClientes WHERE ProveedorId = @ProveedorId AND Name = N'Maria Lopez';
SELECT @ClienteRobertId = Id FROM dbo.IndorProveedorClientes WHERE ProveedorId = @ProveedorId AND Name = N'Robert Kim';
SELECT @JobWaterHeaterId = Id FROM dbo.IndorProveedorJobs WHERE ProveedorId = @ProveedorId AND JobCode = N'JOB-10234';
SELECT @JobHvacId = Id FROM dbo.IndorProveedorJobs WHERE ProveedorId = @ProveedorId AND JobCode = N'J-TODAY-01';
SELECT @LeadRoofId = Id FROM dbo.IndorProveedorLeads WHERE ProveedorId = @ProveedorId AND LeadCode = N'L-1042';
SELECT @LeadPlumbingId = Id FROM dbo.IndorProveedorLeads WHERE ProveedorId = @ProveedorId AND LeadCode = N'L-1043';

IF @ClienteJamesId IS NULL
BEGIN
    PRINT 'Seed omitido: ejecuta primero SeedProviderProOperationsData.sql.';
    RETURN;
END

INSERT INTO dbo.IndorProveedorConversations (
    ProveedorId, ClienteId, JobId, Category, Status, UnreadCount,
    LastMessagePreview, LastMessageAt, IsCustomerOnline
)
VALUES
(@ProveedorId, @ClienteJamesId, @JobWaterHeaterId, N'Job', N'InProgress', 0,
 N'Perfect, see you tomorrow at 9:00 AM.', DATEADD(minute, -18, SYSUTCDATETIME()), 1);

SET @ConvJamesId = SCOPE_IDENTITY();

INSERT INTO dbo.IndorProveedorConversations (
    ProveedorId, ClienteId, JobId, Category, Status, UnreadCount,
    LastMessagePreview, LastMessageAt, IsCustomerOnline
)
VALUES
(@ProveedorId, @ClienteMariaId, @JobHvacId, N'Job', N'Pending', 1,
 N'Can you give me an update on the HVAC tune-up?', DATEADD(hour, -5, SYSUTCDATETIME()), 0);

SET @ConvMariaId = SCOPE_IDENTITY();

INSERT INTO dbo.IndorProveedorConversations (
    ProveedorId, ClienteId, LeadId, Category, Status, UnreadCount,
    LastMessagePreview, LastMessageAt, IsCustomerOnline
)
VALUES
(@ProveedorId, @ClienteRobertId, @LeadRoofId, N'Lead', N'New', 1,
 N'Hi, I wanted to follow up on the roof repair estimate.', DATEADD(day, -1, SYSUTCDATETIME()), 0);

SET @ConvRobertId = SCOPE_IDENTITY();

IF @LeadPlumbingId IS NOT NULL
BEGIN
    INSERT INTO dbo.IndorProveedorConversations (
        ProveedorId, ClienteId, LeadId, Category, Status, UnreadCount,
        LastMessagePreview, LastMessageAt, IsCustomerOnline
    )
    VALUES
    (@ProveedorId, NULL, @LeadPlumbingId, N'Lead', N'New', 1,
     N'Kitchen sink is still draining slowly.', DATEADD(day, -2, SYSUTCDATETIME()), 0);

    SET @ConvLeadId = SCOPE_IDENTITY();
END

-- James Smith / Water Heater chat
INSERT INTO dbo.IndorProveedorMessages (ConversationId, SenderType, Body, SentAt, IsRead)
VALUES
(@ConvJamesId, N'Customer', N'Hi, can you come tomorrow morning for maintenance?', DATEADD(minute, -22, SYSUTCDATETIME()), 1),
(@ConvJamesId, N'Provider', N'Hi John! Yes, we have availability at 9:00 AM. Does that work?', DATEADD(minute, -21, SYSUTCDATETIME()), 1),
(@ConvJamesId, N'Customer', N'Yes, perfect. Thanks.', DATEADD(minute, -20, SYSUTCDATETIME()), 1),
(@ConvJamesId, N'Provider', N'All set! See you tomorrow at 9:00 AM.', DATEADD(minute, -18, SYSUTCDATETIME()), 1);

-- Maria Lopez
INSERT INTO dbo.IndorProveedorMessages (ConversationId, SenderType, Body, SentAt, IsRead)
VALUES
(@ConvMariaId, N'Customer', N'Can you give me an update on the HVAC tune-up?', DATEADD(hour, -5, SYSUTCDATETIME()), 0);

-- Robert Kim
INSERT INTO dbo.IndorProveedorMessages (ConversationId, SenderType, Body, SentAt, IsRead)
VALUES
(@ConvRobertId, N'Customer', N'Hi, I wanted to follow up on the roof repair estimate.', DATEADD(day, -1, SYSUTCDATETIME()), 0);

IF @ConvLeadId IS NOT NULL
BEGIN
    INSERT INTO dbo.IndorProveedorMessages (ConversationId, SenderType, Body, SentAt, IsRead)
    VALUES
    (@ConvLeadId, N'Customer', N'Kitchen sink is still draining slowly.', DATEADD(day, -2, SYSUTCDATETIME()), 0);
END

PRINT 'Seeded messages for ProveedorId = ' + CAST(@ProveedorId AS nvarchar(20));
