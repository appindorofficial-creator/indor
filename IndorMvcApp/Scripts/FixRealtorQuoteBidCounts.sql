/*
  Repair realtor quote counters after provider estimates were synced before the count fix.
  Safe to run multiple times.
*/

UPDATE q
SET
    ProviderQuotesReceived = ISNULL(b.BidCount, 0),
    Amount = b.MinAmount,
    Status = CASE
        WHEN ISNULL(b.BidCount, 0) >= 2 THEN N'Compare'
        WHEN ISNULL(b.BidCount, 0) = 1 THEN N'Received'
        ELSE q.Status
    END,
    FooterNote = CASE
        WHEN ISNULL(b.BidCount, 0) > 0 THEN
            CAST(ISNULL(b.BidCount, 0) AS NVARCHAR(10)) + N' provider quote' + CASE WHEN b.BidCount = 1 THEN N'' ELSE N's' END + N' received'
        ELSE q.FooterNote
    END,
    UpdatedUtc = SYSUTCDATETIME()
FROM dbo.IndorRealtorQuotes q
LEFT JOIN (
    SELECT QuoteId, COUNT(*) AS BidCount, MIN(Amount) AS MinAmount
    FROM dbo.IndorRealtorQuoteBids
    GROUP BY QuoteId
) b ON b.QuoteId = q.Id
WHERE ISNULL(b.BidCount, 0) > 0
  AND (q.ProviderQuotesReceived = 0 OR q.Status = N'Pending');

PRINT 'Realtor quote bid counts repaired.';
