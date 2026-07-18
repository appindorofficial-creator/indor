using System.Text;

namespace IndorMvcApp.Services;

/// <summary>
/// Shared contact-phone rules for Property Administrator service flows.
/// UI always uses _PropertyAdminUrgentContactPhone (required 10-digit US number).
/// Use <see cref="IsValid"/> / <see cref="IsProvided"/> for server-side checks.
/// </summary>
public static class PropertyAdministratorContactPhone
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var digits = new StringBuilder(10);
        foreach (var ch in value)
        {
            if (char.IsDigit(ch))
            {
                digits.Append(ch);
            }
        }

        var normalized = digits.ToString();
        if (normalized.Length == 11 && normalized[0] == '1')
        {
            normalized = normalized[1..];
        }

        return normalized.Length > 10 ? normalized[..10] : normalized;
    }

    public static bool IsValid(string? value) => Normalize(value).Length == 10;

    /// <summary>Required contact phone is valid only when it has exactly 10 digits.</summary>
    public static bool IsProvided(string? value) => IsValid(value);
}
