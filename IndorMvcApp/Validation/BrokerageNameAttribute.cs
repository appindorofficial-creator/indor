using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Validates a brokerage or broker business name (letters required; not digits-only).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BrokerageNameAttribute : ValidationAttribute
{
    public const int MinLength = 2;
    public const int MaxLength = 200;

    private static readonly Regex HasLetterRegex = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex OnlyDigitsAndSpacesRegex = new(@"^[\d\s]+$", RegexOptions.Compiled);

    public BrokerageNameAttribute()
        : base("Enter a valid brokerage or broker name using letters.")
    {
    }

    public static bool IsValidBrokerageName(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "Brokerage name is required.";
            return false;
        }

        var name = value.Trim();
        if (name.Length < MinLength)
        {
            errorMessage = $"Brokerage name must be at least {MinLength} characters.";
            return false;
        }

        if (name.Length > MaxLength)
        {
            errorMessage = $"Brokerage name cannot exceed {MaxLength} characters.";
            return false;
        }

        if (!HasLetterRegex.IsMatch(name))
        {
            errorMessage = "Brokerage name must include letters (e.g. Keller Williams, RE/MAX).";
            return false;
        }

        if (OnlyDigitsAndSpacesRegex.IsMatch(name))
        {
            errorMessage = "Brokerage name cannot contain only numbers.";
            return false;
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return new ValidationResult("Brokerage name is required.");
        }

        return IsValidBrokerageName(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Enter a valid brokerage name.");
    }
}
