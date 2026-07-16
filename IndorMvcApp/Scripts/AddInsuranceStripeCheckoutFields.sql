IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'StripeCheckoutSessionId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProviderInsuranceQuotes
        ADD StripeCheckoutSessionId NVARCHAR(120) NULL;
END
GO

IF COL_LENGTH('dbo.IndorProviderInsuranceQuotes', 'StripePaymentIntentId') IS NULL
BEGIN
    ALTER TABLE dbo.IndorProviderInsuranceQuotes
        ADD StripePaymentIntentId NVARCHAR(120) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_IndorProviderInsuranceQuotes_StripeCheckoutSessionId'
      AND object_id = OBJECT_ID('dbo.IndorProviderInsuranceQuotes')
)
BEGIN
    CREATE INDEX IX_IndorProviderInsuranceQuotes_StripeCheckoutSessionId
        ON dbo.IndorProviderInsuranceQuotes (StripeCheckoutSessionId);
END
GO
