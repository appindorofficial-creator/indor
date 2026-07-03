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

    private static readonly HashSet<string> StreetSuffixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "st", "street", "rd", "road", "ave", "avenue", "blvd", "boulevard", "dr", "drive",
        "ln", "lane", "way", "ct", "court", "cir", "circle", "pl", "place", "pkwy", "parkway",
        "ter", "terrace", "trl", "trail", "hwy", "highway", "loop", "pass", "path", "row",
        "run", "walk", "xing", "crossing", "pike", "sq", "square", "aly", "alley", "cres",
        "crescent", "cv", "cove", "bnd", "bend", "pt", "point", "grv", "grove", "vw", "view"
    };

    private static readonly HashSet<string> Directionals = new(StringComparer.OrdinalIgnoreCase)
    {
        "n", "s", "e", "w", "ne", "nw", "se", "sw",
        "north", "south", "east", "west"
    };

    private const string StreetTypeMessage =
        "Enter a complete street address with street name and type (e.g. 123 Main St).";

    /// <summary>
    /// When true, the address must include a city/state hint (a comma separator or a
    /// ZIP code). Use for single, combined address fields (e.g. moving From/To). Leave
    /// false for street-only fields that have separate City/State/ZIP inputs.
    /// </summary>
    public bool RequireCityOrZip { get; set; }

    /// <summary>
    /// When true, the address must include a street number (e.g. 123 Main St).
    /// </summary>
    public bool RequireStreetNumber { get; set; }

    public ValidStreetAddressAttribute()
        : base("Please enter a valid street address (e.g. 123 Main St, Charlotte, NC).")
    {
    }

    public static bool IsValidStreetAddress(
        string? value,
        out string? errorMessage,
        bool requireCityOrZip = false,
        bool requireStreetNumber = false)
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

        if (requireStreetNumber && !hasDigit)
        {
            errorMessage = "Enter a street number (e.g. 123 Main St).";
            return false;
        }

        if (requireStreetNumber && !HasCompleteStreetLine(tokens))
        {
            errorMessage = StreetTypeMessage;
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
                errorMessage =
                    "Enter a complete address with city and state (e.g. 123 Main St, Charlotte, NC).";
                return false;
            }
        }

        return true;
    }

    private static bool HasCompleteStreetLine(IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0)
        {
            return false;
        }

        var normalized = tokens
            .Select(NormalizeStreetToken)
            .Where(token => token.Length > 0)
            .ToList();

        if (normalized.Any(StreetSuffixes.Contains))
        {
            return normalized.Any(token => token.Any(char.IsLetter) && !Directionals.Contains(token));
        }

        var nameTokens = normalized
            .Where(token => token.Any(char.IsLetter) && !Directionals.Contains(token))
            .ToList();

        return nameTokens.Count >= 2;
    }

    private static string NormalizeStreetToken(string token) =>
        token.Trim(',', '.', '#').ToLowerInvariant();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        return IsValidStreetAddress(text, out var message, RequireCityOrZip, RequireStreetNumber)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Please enter a valid street address.");
    }
}
