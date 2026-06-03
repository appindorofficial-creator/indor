/*
  FindDuplicateUserEmails.sql
  ---------------------------
  Lists AspNetUsers rows that share the same NormalizedEmail.
  Run in SSMS / Azure Query editor against your Indor database.

  Fix duplicates manually (keep the account you use, delete or change Email on the others).
*/

SET NOCOUNT ON;

SELECT
    u.NormalizedEmail,
    COUNT(*) AS DuplicateCount
FROM dbo.AspNetUsers u
WHERE u.NormalizedEmail IS NOT NULL
GROUP BY u.NormalizedEmail
HAVING COUNT(*) > 1
ORDER BY DuplicateCount DESC, u.NormalizedEmail;

PRINT '';
PRINT '--- Detail ---';

SELECT
    u.Id,
    u.UserName,
    u.Email,
    u.NormalizedEmail,
    u.FechaRegistro,
    u.RolUsuario,
    (SELECT COUNT(*) FROM dbo.Propiedades p WHERE p.UserId = u.Id) AS PropertyCount
FROM dbo.AspNetUsers u
WHERE u.NormalizedEmail IN (
    SELECT NormalizedEmail
    FROM dbo.AspNetUsers
    WHERE NormalizedEmail IS NOT NULL
    GROUP BY NormalizedEmail
    HAVING COUNT(*) > 1
)
ORDER BY u.NormalizedEmail, u.FechaRegistro;
