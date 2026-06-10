-- Provider onboarding wizard metadata (INDOR Pro path, optional assessment, activation call).
IF COL_LENGTH('dbo.IndorProveedores', 'OnboardingMetaJson') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProveedores
        ADD OnboardingMetaJson NVARCHAR(2000) NULL;
END
GO
