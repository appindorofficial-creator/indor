using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Rejects values that are empty of letters, digits-only, or too short to be a street address.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ValidStreetAddressAttribute : ValidationAttribute
{
    private static readonly Regex HasLetterRegex = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex OnlyDigitsAndSeparatorsRegex = new(@"^[\d\s.,#\-]+$", RegexOptions.Compiled);

    public ValidStreetAddressAttribute()
        : base("Please enter a valid street address (e.g. 123 Main St, Charlotte, NC).")
    {
    }

    public static bool IsValidStreetAddress(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var address = value.Trim();
        if (address.Length < 5)
        {
            errorMessage = "Enter a complete street address.";
            return false;
        }

        if (!HasLetterRegex.IsMatch(address))
        {
            errorMessage = "Enter a valid street address with a street name.";
            return false;
        }

        if (OnlyDigitsAndSeparatorsRegex.IsMatch(address))
        {
            errorMessage = "Address cannot contain only numbers.";
            return false;
        }

        var hasDigit = address.Any(char.IsDigit);
        var wordParts = address
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Count(part => part.Any(char.IsLetter));

        if (!hasDigit && wordParts < 2)
        {
            errorMessage = "Enter a complete street address (e.g. 123 Main St, Charlotte, NC).";
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

        return IsValidStreetAddress(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Please enter a valid street address.");
    }
}
