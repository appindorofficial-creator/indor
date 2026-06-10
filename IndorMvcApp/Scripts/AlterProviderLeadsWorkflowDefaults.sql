-- Lead workflow defaults: estimate suggestions, visit scheduling, job templates (from INDOR)
IF COL_LENGTH('dbo.IndorProveedorLeads', 'SuggestedScopeItemsJson') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SuggestedScopeItemsJson NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'SuggestedLaborAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SuggestedLaborAmount DECIMAL(12,2) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'SuggestedMaterialsAmount') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SuggestedMaterialsAmount DECIMAL(12,2) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'SuggestedTimeline') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SuggestedTimeline NVARCHAR(80) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'SuggestedWarranty') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SuggestedWarranty NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'SuggestedHomeownerNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD SuggestedHomeownerNotes NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultVisitType') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultVisitType NVARCHAR(40) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultAssignedTechnician') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultAssignedTechnician NVARCHAR(120) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultVisitNotes') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultVisitNotes NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultVisitAt') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultVisitAt DATETIME2 NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultVisitTimeLabel') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultVisitTimeLabel NVARCHAR(20) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultChecklistJson') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultChecklistJson NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultMaterialsUsedJson') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultMaterialsUsedJson NVARCHAR(2000) NULL;

IF COL_LENGTH('dbo.IndorProveedorLeads', 'DefaultLaborWarranty') IS NULL
    ALTER TABLE dbo.IndorProveedorLeads ADD DefaultLaborWarranty NVARCHAR(200) NULL;

IF COL_LENGTH('dbo.IndorProveedorJobs', 'LeadId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedorJobs ADD LeadId INT NULL;
    ALTER TABLE dbo.IndorProveedorJobs ADD CONSTRAINT FK_IndorProvJob_Lead
        FOREIGN KEY (LeadId) REFERENCES dbo.IndorProveedorLeads(Id);
END
GO
