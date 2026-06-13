using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Equipment serial numbers: 6–80 chars, letters and numbers only, must include both.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class EquipmentSerialNumberAttribute : ValidationAttribute
{
    public const int MinLength = 6;
    public const int MaxLength = 80;
    public const string HintText = "6–80 characters. Use letters and numbers (e.g. M123456789).";

    private static readonly Regex AllowedCharsRegex = new(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled);
    private static readonly Regex HasLetterRegex = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex HasDigitRegex = new(@"\d", RegexOptions.Compiled);

    public EquipmentSerialNumberAttribute()
        : base($"Enter {MinLength}–{MaxLength} characters using letters and numbers.")
    {
    }

    public static bool IsValidEquipmentSerial(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var serial = value.Trim();
        if (serial.Length < MinLength)
        {
            errorMessage = $"Serial number must be at least {MinLength} characters.";
            return false;
        }

        if (serial.Length > MaxLength)
        {
            errorMessage = $"Serial number cannot exceed {MaxLength} characters.";
            return false;
        }

        if (!AllowedCharsRegex.IsMatch(serial))
        {
            errorMessage = "Use only letters, numbers, and hyphens.";
            return false;
        }

        if (!HasLetterRegex.IsMatch(serial) || !HasDigitRegex.IsMatch(serial))
        {
            errorMessage = "Serial number must include at least one letter and one number.";
            return false;
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        return IsValidEquipmentSerial(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? HintText);
    }
}
