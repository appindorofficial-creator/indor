-- Provider lead workflow: AI summary on leads, invoice link to estimate
IF COL_LENGTH('dbo.IndorProveedorLeads', 'AnalysisSummary') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorLeads ADD AnalysisSummary NVARCHAR(2000) NULL;
    PRINT 'Column IndorProveedorLeads.AnalysisSummary added.';
END

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'EstimateId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorInvoices ADD EstimateId INT NULL;
    PRINT 'Column IndorProveedorInvoices.EstimateId added.';
END

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'LeadId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorInvoices ADD LeadId INT NULL;
    PRINT 'Column IndorProveedorInvoices.LeadId added.';
END

IF COL_LENGTH('dbo.IndorProveedorInvoices', 'SentUtc') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorInvoices ADD SentUtc DATETIME2 NULL;
    PRINT 'Column IndorProveedorInvoices.SentUtc added.';
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_IndorProveedorInvoices_Estimate')
BEGIN
    ALTER TABLE dbo.IndorProveedorInvoices WITH CHECK ADD CONSTRAINT FK_IndorProveedorInvoices_Estimate
        FOREIGN KEY (EstimateId) REFERENCES dbo.IndorProveedorEstimates(Id);
    PRINT 'FK IndorProveedorInvoices.EstimateId created.';
END
