/*
  INDOR — Bridge realtor inspection quotes to provider leads and bids.
  Run on IndorDB after CreateRealtorInspectionUploadWizardTables.sql and provider tables.
  Safe to run multiple times.
*/

-- Leads: link back to realtor quote requests
IF COL_LENGTH('dbo.IndorProveedorLeads', 'RealtorQuoteId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorLeads ADD RealtorQuoteId INT NULL;
    PRINT 'Column IndorProveedorLeads.RealtorQuoteId added.';
END
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'LeadSource') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorLeads ADD LeadSource NVARCHAR(40) NULL;
    PRINT 'Column IndorProveedorLeads.LeadSource added.';
END
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'InspectionReportUrl') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorLeads ADD InspectionReportUrl NVARCHAR(500) NULL;
    PRINT 'Column IndorProveedorLeads.InspectionReportUrl added.';
END
GO

IF COL_LENGTH('dbo.IndorProveedorLeads', 'FindingsJson') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorLeads ADD FindingsJson NVARCHAR(MAX) NULL;
    PRINT 'Column IndorProveedorLeads.FindingsJson added.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_IndorProveedorLeads_RealtorQuote' AND object_id = OBJECT_ID(N'dbo.IndorProveedorLeads'))
BEGIN
    CREATE INDEX IX_IndorProveedorLeads_RealtorQuote ON dbo.IndorProveedorLeads (RealtorQuoteId)
        WHERE RealtorQuoteId IS NOT NULL;
    PRINT 'Index IX_IndorProveedorLeads_RealtorQuote created.';
END
GO

-- Sent providers: real INDOR PRO provider id
IF COL_LENGTH('dbo.IndorRealtorQuoteSentProviders', 'ProveedorId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuoteSentProviders ADD ProveedorId INT NULL;
    PRINT 'Column IndorRealtorQuoteSentProviders.ProveedorId added.';
END
GO

IF COL_LENGTH('dbo.IndorRealtorQuoteSentProviders', 'LeadId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuoteSentProviders ADD LeadId INT NULL;
    PRINT 'Column IndorRealtorQuoteSentProviders.LeadId added.';
END
GO

-- Bids: link to provider estimate
IF COL_LENGTH('dbo.IndorRealtorQuoteBids', 'ProveedorId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuoteBids ADD ProveedorId INT NULL;
    PRINT 'Column IndorRealtorQuoteBids.ProveedorId added.';
END
GO

IF COL_LENGTH('dbo.IndorRealtorQuoteBids', 'EstimateId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuoteBids ADD EstimateId INT NULL;
    PRINT 'Column IndorRealtorQuoteBids.EstimateId added.';
END
GO

IF COL_LENGTH('dbo.IndorRealtorQuoteBids', 'LeadId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuoteBids ADD LeadId INT NULL;
    PRINT 'Column IndorRealtorQuoteBids.LeadId added.';
END
GO

IF COL_LENGTH('dbo.IndorRealtorQuoteBids', 'SubmittedUtc') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorQuoteBids ADD SubmittedUtc DATETIME2(7) NULL;
    PRINT 'Column IndorRealtorQuoteBids.SubmittedUtc added.';
END
GO

-- Findings: optional AI description
IF COL_LENGTH('dbo.IndorRealtorInspectionUploadFindings', 'Description') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorInspectionUploadFindings ADD Description NVARCHAR(1000) NULL;
    PRINT 'Column IndorRealtorInspectionUploadFindings.Description added.';
END
GO

-- Draft: store raw AI summary
IF COL_LENGTH('dbo.IndorRealtorInspectionUploadDrafts', 'AnalysisSummary') IS NULL
BEGIN
    ALTER TABLE dbo.IndorRealtorInspectionUploadDrafts ADD AnalysisSummary NVARCHAR(2000) NULL;
    PRINT 'Column IndorRealtorInspectionUploadDrafts.AnalysisSummary added.';
END
GO

PRINT 'Realtor inspection provider bridge extension complete.';
