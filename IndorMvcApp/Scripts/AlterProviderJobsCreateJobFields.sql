-- Create Job wizard: schedule end time, reminder, calendar flag, category descriptions

IF COL_LENGTH('dbo.IndorProveedorJobs', 'ScheduledEndAt') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ScheduledEndAt DATETIME2 NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'ReminderSetting') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD ReminderSetting NVARCHAR(60) NULL;
GO
IF COL_LENGTH('dbo.IndorProveedorJobs', 'AddToCalendar') IS NULL
    ALTER TABLE dbo.IndorProveedorJobs ADD AddToCalendar BIT NOT NULL CONSTRAINT DF_IndorProvJob_AddToCalendar DEFAULT (1);
GO

IF COL_LENGTH('dbo.IndorProveedorCategoriasCatalogo', 'DescriptionEn') IS NULL
    ALTER TABLE dbo.IndorProveedorCategoriasCatalogo ADD DescriptionEn NVARCHAR(200) NULL;
GO

UPDATE dbo.IndorProveedorCategoriasCatalogo SET DescriptionEn = N'Heating, ventilation & air conditioning' WHERE Id = N'hvac' AND DescriptionEn IS NULL;
UPDATE dbo.IndorProveedorCategoriasCatalogo SET DescriptionEn = N'Pipes, leaks, drains & fixtures' WHERE Id = N'plumbing' AND DescriptionEn IS NULL;
UPDATE dbo.IndorProveedorCategoriasCatalogo SET DescriptionEn = N'Wiring, outlets, panels & lighting' WHERE Id = N'electrical' AND DescriptionEn IS NULL;
UPDATE dbo.IndorProveedorCategoriasCatalogo SET DescriptionEn = N'Roof repair, replacement & inspection' WHERE Id = N'roofing' AND DescriptionEn IS NULL;
UPDATE dbo.IndorProveedorCategoriasCatalogo SET DescriptionEn = N'General repairs & maintenance' WHERE Id = N'handyman' AND DescriptionEn IS NULL;
GO

MERGE dbo.IndorProveedorCategoriasCatalogo AS t
USING (VALUES
    (N'water_heater', N'Water Heater', N'fa-water', N'Installation, repair & replacement', 15, 0),
    (N'other', N'Other', N'fa-ellipsis', N'Something else not listed above', 99, 0)
) AS s (Id, LabelEn, IconClass, DescriptionEn, SortOrder, RequiresTradeExam)
ON t.Id = s.Id
WHEN MATCHED THEN UPDATE SET
    LabelEn = s.LabelEn,
    IconClass = s.IconClass,
    DescriptionEn = s.DescriptionEn,
    SortOrder = s.SortOrder,
    RequiresTradeExam = s.RequiresTradeExam,
    Activo = 1
WHEN NOT MATCHED THEN INSERT (Id, LabelEn, IconClass, DescriptionEn, SortOrder, RequiresTradeExam)
    VALUES (s.Id, s.LabelEn, s.IconClass, s.DescriptionEn, s.SortOrder, s.RequiresTradeExam);
GO
