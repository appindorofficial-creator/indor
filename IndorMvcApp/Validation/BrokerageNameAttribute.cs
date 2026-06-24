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
        : base("{0} must include letters (e.g. Keller Williams, RE/MAX).")
    {
    }

    public static bool IsValidBrokerageName(string? value, out string? errorMessage, string fieldLabel = "Brokerage Name")
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = $"{fieldLabel} is required.";
            return false;
        }

        var name = value.Trim();
        if (name.Length < MinLength)
        {
            errorMessage = $"{fieldLabel} must be at least {MinLength} characters.";
            return false;
        }

        if (name.Length > MaxLength)
        {
            errorMessage = $"{fieldLabel} cannot exceed {MaxLength} characters.";
            return false;
        }

        if (!HasLetterRegex.IsMatch(name))
        {
            errorMessage = $"{fieldLabel} must include letters (e.g. Keller Williams, RE/MAX).";
            return false;
        }

        if (OnlyDigitsAndSpacesRegex.IsMatch(name))
        {
            errorMessage = $"{fieldLabel} cannot contain only numbers.";
            return false;
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var fieldLabel = validationContext.DisplayName ?? "Brokerage Name";
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return new ValidationResult($"{fieldLabel} is required.");
        }

        return IsValidBrokerageName(text, out var message, fieldLabel)
            ? ValidationResult.Success
            : new ValidationResult(message ?? string.Format(ErrorMessageString, fieldLabel));
    }

    public override string FormatErrorMessage(string name) =>
        string.Format(ErrorMessageString, name);
}
