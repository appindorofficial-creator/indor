IF COL_LENGTH('AspNetUsers', 'PreferredUiCulture') IS NULL
BEGIN
    ALTER TABLE AspNetUsers
        ADD PreferredUiCulture NVARCHAR(10) NULL;
END
GO
