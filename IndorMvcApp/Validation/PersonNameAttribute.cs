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

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        var name = text.Trim();

        if (name.Length < 2)
        {
            return new ValidationResult("Full name must be at least 2 characters.");
        }

        if (!HasLetterRegex.IsMatch(name))
        {
            return new ValidationResult(ErrorMessage ?? "Enter a valid full name using letters.");
        }

        if (OnlyDigitsAndSpacesRegex.IsMatch(name))
        {
            return new ValidationResult("Full name cannot contain only numbers.");
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || parts.Any(part => !part.Any(char.IsLetter)))
        {
            return new ValidationResult("Enter your first and last name.");
        }

        return ValidationResult.Success;
    }
}
