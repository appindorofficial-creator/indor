using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.Validation;

/// <summary>
/// Optional US phone: empty is OK; if provided, must be 10 digits (optional leading country code 1).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UsPhoneOptionalAttribute : ValidationAttribute
{
    public const int UsLocalDigits = 10;

    public UsPhoneOptionalAttribute()
        : base("Enter a valid 10-digit US phone number (e.g. 555 123 4567).")
    {
    }

    public static string? NormalizeToStorage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            return null;
        }

        if (digits.Length == 11 && digits[0] == '1')
        {
            digits = digits[1..];
        }

        return digits.Length == UsLocalDigits ? digits : null;
    }

    public static bool IsValidOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            return false;
        }

        if (digits.Length == 11 && digits[0] == '1')
        {
            digits = digits[1..];
        }

        return digits.Length == UsLocalDigits;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var text = value as string;
        return IsValidOptional(text)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage ?? "Enter a valid 10-digit US phone number.");
    }
}
