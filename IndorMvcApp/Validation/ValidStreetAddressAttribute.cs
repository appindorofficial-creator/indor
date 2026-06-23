using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Rejects values that are empty of letters, digits-only, too short, or that lack a
/// city/state component (so just a bare street line like "9713 Falling Stream Dr" is
/// rejected as incomplete). A complete address must include a street number, a street
/// name, and a city/state hint (a comma separator or a 5-digit ZIP code that is not the
/// leading street number).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ValidStreetAddressAttribute : ValidationAttribute
{
    private static readonly Regex HasLetterRegex = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex OnlyDigitsAndSeparatorsRegex = new(@"^[\d\s.,#\-]+$", RegexOptions.Compiled);
    private static readonly Regex ZipTokenRegex = new(@"^\d{5}(-\d{4})?$", RegexOptions.Compiled);

    private const string IncompleteMessage =
        "Enter a complete address with city and state (e.g. 123 Main St, Charlotte, NC).";

    /// <summary>
    /// When true, the address must include a city/state hint (a comma separator or a
    /// ZIP code). Use for single, combined address fields (e.g. moving From/To). Leave
    /// false for street-only fields that have separate City/State/ZIP inputs.
    /// </summary>
    public bool RequireCityOrZip { get; set; }

    public ValidStreetAddressAttribute()
        : base("Please enter a valid street address (e.g. 123 Main St, Charlotte, NC).")
    {
    }

    public static bool IsValidStreetAddress(string? value, out string? errorMessage, bool requireCityOrZip = false)
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

        var tokens = address.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var hasDigit = address.Any(char.IsDigit);
        var wordParts = tokens.Count(part => part.Any(char.IsLetter));

        if (!hasDigit && wordParts < 2)
        {
            errorMessage = "Enter a complete street address (e.g. 123 Main St, Charlotte, NC).";
            return false;
        }

        if (requireCityOrZip)
        {
            // A bare street line ("9713 Falling Stream Dr") lacks any city/state hint.
            // Require a comma separator or a ZIP code that is not the leading street number.
            var hasComma = address.Contains(',');
            var hasZip = tokens.Skip(1).Any(t => ZipTokenRegex.IsMatch(t.Trim(',', '.')));
            if (!hasComma && !hasZip)
            {
                errorMessage = IncompleteMessage;
                return false;
            }
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        return IsValidStreetAddress(text, out var message, RequireCityOrZip)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Please enter a valid street address.");
    }
}
