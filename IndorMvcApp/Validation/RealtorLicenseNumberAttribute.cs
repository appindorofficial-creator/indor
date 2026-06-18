using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Validates a realtor license number (4–20 alphanumeric characters, at least one letter).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class RealtorLicenseNumberAttribute : ValidationAttribute
{
    public const int MinLength = 4;
    public const int MaxLength = 20;

    private static readonly Regex AlphanumericRegex = new(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex HasLetterRegex = new(@"[A-Za-z]", RegexOptions.Compiled);

    public RealtorLicenseNumberAttribute()
        : base($"License number must be {MinLength}–{MaxLength} alphanumeric characters and include at least one letter.")
    {
    }

    public static bool IsValidLicenseNumber(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "License number is required.";
            return false;
        }

        var license = value.Trim();
        if (license.Length < MinLength)
        {
            errorMessage = $"License number must be at least {MinLength} characters.";
            return false;
        }

        if (license.Length > MaxLength)
        {
            errorMessage = $"License number cannot exceed {MaxLength} characters.";
            return false;
        }

        if (!AlphanumericRegex.IsMatch(license))
        {
            errorMessage = "License number can only contain letters and numbers (no spaces or symbols).";
            return false;
        }

        if (!HasLetterRegex.IsMatch(license))
        {
            errorMessage = "License number must include at least one letter (cannot be only numbers).";
            return false;
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return new ValidationResult("License number is required.");
        }

        return IsValidLicenseNumber(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Enter a valid license number.");
    }
}
