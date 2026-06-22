/*
  INDOR — Add address detail + property type columns to IndorRealtorPropertyFiles.
  Supports the "Create New Property" step in the Invite Client wizard.
  Safe to run multiple times.
*/

IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'Unit') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD Unit NVARCHAR(60) NULL;

IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'StateCode') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD StateCode NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'PostalCode') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD PostalCode NVARCHAR(12) NULL;

IF COL_LENGTH('dbo.IndorRealtorPropertyFiles', 'PropertyType') IS NULL
    ALTER TABLE dbo.IndorRealtorPropertyFiles ADD PropertyType NVARCHAR(30) NULL;
GO

PRINT 'IndorRealtorPropertyFiles extended with Unit, StateCode, PostalCode, PropertyType.';
