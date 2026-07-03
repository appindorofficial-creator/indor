using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

/// <summary>
/// Stricter email than <see cref="EmailAddressAttribute"/>: valid format, real-looking TLD,
/// and blocks common typos (e.g. hmail.com instead of gmail.com).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ValidEmailAttribute : ValidationAttribute
{
    private static readonly Regex FormatRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> BlockedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "hmail.com", "hmial.com", "gnail.com", "gmial.com", "gmal.com", "gamil.com", "gmai.com",
        "gmail.con", "gmail.co", "gmail.cm", "hotmial.com", "hotmal.com", "hotmai.com",
        "yahooo.com", "yaho.com", "yahho.com", "outlok.com", "outlook.con", "outlook.co",
        "icloud.con", "live.con", "msn.con", "email.com"
    };

    private static readonly Dictionary<string, string> DomainSuggestions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hmail.com"] = "gmail.com",
        ["hmial.com"] = "gmail.com",
        ["gnail.com"] = "gmail.com",
        ["gmial.com"] = "gmail.com",
        ["gmal.com"] = "gmail.com",
        ["gamil.com"] = "gmail.com",
        ["hotmial.com"] = "hotmail.com",
        ["hotmal.com"] = "hotmail.com",
        ["outlok.com"] = "outlook.com",
        ["yahooo.com"] = "yahoo.com",
        ["hotmail.c"] = "hotmail.com",
        ["hotmail.co"] = "hotmail.com",
        ["gmail.c"] = "gmail.com",
        ["gmail.co"] = "gmail.com",
        ["yahoo.c"] = "yahoo.com",
        ["outlook.c"] = "outlook.com",
        ["icloud.c"] = "icloud.com"
    };

    public ValidEmailAttribute()
        : base("Enter a valid email address.")
    {
    }

    public static bool IsValidAddress(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var email = value.Trim();
        if (email.Length > 254 || !FormatRegex.IsMatch(email))
        {
            errorMessage = "Enter a valid email address.";
            return false;
        }

        try
        {
            _ = new MailAddress(email);
        }
        catch
        {
            errorMessage = "Enter a valid email address.";
            return false;
        }

        var at = email.LastIndexOf('@');
        if (at < 1)
        {
            errorMessage = "Enter a valid email address.";
            return false;
        }

        var domain = email[(at + 1)..];
        if (BlockedDomains.Contains(domain))
        {
            errorMessage = DomainSuggestions.TryGetValue(domain, out var suggested)
                ? $"Check your email address. Did you mean @{suggested}?"
                : "Enter a valid email address with a recognized domain.";
            return false;
        }

        if (DomainSuggestions.TryGetValue(domain, out var truncatedSuggestion))
        {
            errorMessage = $"Check your email address. Did you mean @{truncatedSuggestion}?";
            return false;
        }

        var lastDot = domain.LastIndexOf('.');
        if (lastDot < 1 || lastDot >= domain.Length - 2)
        {
            errorMessage = "Enter a valid email address with a complete domain (e.g. name@email.com).";
            return false;
        }

        var tld = domain[(lastDot + 1)..];
        if (tld.Length < 2 || !tld.All(char.IsLetter))
        {
            errorMessage = "Enter a valid email address with a complete domain (e.g. name@email.com).";
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

        return IsValidAddress(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Enter a valid email address.");
    }
}
