/*
  INDOR — Add AcceptedUtc to IndorRealtorInvitations for the client accept flow.
  Safe to run multiple times.
*/

IF COL_LENGTH('dbo.IndorRealtorInvitations', 'AcceptedUtc') IS NULL
    ALTER TABLE dbo.IndorRealtorInvitations ADD AcceptedUtc DATETIME2(7) NULL;
GO

PRINT 'IndorRealtorInvitations.AcceptedUtc ready.';
