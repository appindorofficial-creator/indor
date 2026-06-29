using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Required US ZIP: 5 digits or ZIP+4 (12345 or 12345-6789).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UsZipCodeAttribute : ValidationAttribute
{
    private static readonly Regex FormatRegex = new(@"^\d{5}(-\d{4})?$", RegexOptions.Compiled);

    public UsZipCodeAttribute()
        : base("Enter a valid 5-digit ZIP code (e.g. 77002).")
    {
    }

    public static bool IsValidRequired(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "ZIP code is required.";
            return false;
        }

        var zip = value.Trim();
        if (!FormatRegex.IsMatch(zip))
        {
            errorMessage = "Enter a valid 5-digit ZIP code (e.g. 77002).";
            return false;
        }

        return true;
    }

    public static string? NormalizeToStorage(string? value)
    {
        if (!IsValidRequired(value, out _))
        {
            return null;
        }

        return value!.Trim();
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var text = value as string;
        return IsValidRequired(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Enter a valid 5-digit ZIP code.");
    }
}
