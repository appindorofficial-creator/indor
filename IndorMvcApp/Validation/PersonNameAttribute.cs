using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Rejects names that are empty, only digits, or contain no letters.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class PersonNameAttribute : ValidationAttribute
{
    private static readonly Regex HasLetterRegex = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex OnlyDigitsAndSpacesRegex = new(@"^[\d\s]+$", RegexOptions.Compiled);

    public PersonNameAttribute()
        : base("Enter a valid full name using letters (e.g. John Smith).")
    {
    }

    public static bool IsValidName(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "Full name is required.";
            return false;
        }

        var name = value.Trim();

        if (name.Length < 2)
        {
            errorMessage = "Full name must be at least 2 characters.";
            return false;
        }

        if (!HasLetterRegex.IsMatch(name))
        {
            errorMessage = "Enter a valid full name using letters (e.g. John Smith).";
            return false;
        }

        if (OnlyDigitsAndSpacesRegex.IsMatch(name))
        {
            errorMessage = "Full name cannot contain only numbers.";
            return false;
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts.Any(part => !part.Any(char.IsLetter)))
        {
            errorMessage = "Enter the client's first and last name.";
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

        return IsValidName(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Enter a valid full name using letters.");
    }
}
