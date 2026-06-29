/*
  INDOR — Password reset codes.
  Stores the 6-digit code emailed to the user plus the ASP.NET Identity
  reset token. Each code is valid for 24 hours (ExpiresUtc) and single-use.

  Safe to re-run.
*/

IF OBJECT_ID(N'dbo.IndorPasswordResetCodes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.IndorPasswordResetCodes (
        Id            INT             IDENTITY(1,1) NOT NULL,
        UserId        NVARCHAR(450)   NOT NULL,
        Email         NVARCHAR(256)   NOT NULL,
        Code          NVARCHAR(10)    NOT NULL,
        ResetToken    NVARCHAR(2000)  NOT NULL,
        ExpiresUtc    DATETIME2       NOT NULL,
        Used          BIT             NOT NULL CONSTRAINT DF_IndorPwdReset_Used    DEFAULT (0),
        UsedUtc       DATETIME2       NULL,
        Attempts      INT             NOT NULL CONSTRAINT DF_IndorPwdReset_Attempts DEFAULT (0),
        FechaCreacion DATETIME2       NOT NULL CONSTRAINT DF_IndorPwdReset_Created  DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_IndorPasswordResetCodes PRIMARY KEY CLUSTERED (Id)
    );
    CREATE INDEX IX_IndorPwdReset_Lookup ON dbo.IndorPasswordResetCodes(Email, Code, Used);
    CREATE INDEX IX_IndorPwdReset_User   ON dbo.IndorPasswordResetCodes(UserId, Used);
END
GO
