using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.Validation;

/// <summary>
/// Optional US phone: empty is OK; if provided, must be between <see cref="MinDigits"/> and
/// <see cref="UsLocalDigits"/> digits (optional leading country code 1).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UsPhoneOptionalAttribute : ValidationAttribute
{
    public const int UsLocalDigits = 10;

    public int MinDigits { get; set; } = UsLocalDigits;

    public UsPhoneOptionalAttribute()
        : base("Enter a valid 10-digit US phone number (e.g. 555 123 4567).")
    {
    }

    public static string? ExtractLocalDigits(string? value)
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

        return digits.Length <= UsLocalDigits ? digits : null;
    }

    public static string? NormalizeToStorage(string? value, int minDigits = UsLocalDigits)
    {
        var digits = ExtractLocalDigits(value);
        if (digits == null)
        {
            return null;
        }

        return digits.Length >= minDigits && digits.Length <= UsLocalDigits ? digits : null;
    }

    public static bool IsValidOptional(string? value, int minDigits = UsLocalDigits)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var digits = ExtractLocalDigits(value);
        return digits != null && digits.Length >= minDigits;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var text = value as string;
        return IsValidOptional(text, MinDigits)
            ? ValidationResult.Success
            : new ValidationResult(FormatErrorMessage(MinDigits));
    }

    private static string FormatErrorMessage(int minDigits) =>
        minDigits >= UsLocalDigits
            ? "Enter a valid 10-digit US phone number (e.g. 555 123 4567)."
            : "Enter a valid phone number (1–10 digits).";
}
