using System.Globalization;
using System.Text.RegularExpressions;

namespace IndorMvcApp.Validation;

public static class PaymentMethodValidation
{
    private static readonly Regex FourDigitsRegex = new(@"^\d{4}$", RegexOptions.Compiled);
    private static readonly Regex ExpiryRegex = new(@"^(0[1-9]|1[0-2])\/\d{2}$", RegexOptions.Compiled);
    private static readonly Regex HasLetterRegex = new(@"\p{L}", RegexOptions.Compiled);
    private static readonly Regex OnlyDigitsAndSpacesRegex = new(@"^[\d\s]+$", RegexOptions.Compiled);
    private static readonly Regex CardBrandRegex = new(@"^[\p{L}\s.'-]{2,30}$", RegexOptions.Compiled);

    public static bool TryValidate(
        string? tipo,
        string? marca,
        string? ultimos4,
        string? titular,
        string? expiracion,
        out Dictionary<string, string> errors)
    {
        errors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var paymentType = string.IsNullOrWhiteSpace(tipo) ? "Card" : tipo.Trim();

        var last4 = NormalizeLastFour(ultimos4);
        if (string.IsNullOrEmpty(last4))
        {
            errors[nameof(ultimos4)] = "Enter the last 4 digits of the card (numbers only).";
        }
        else if (!FourDigitsRegex.IsMatch(last4))
        {
            errors[nameof(ultimos4)] = "Last 4 digits must be exactly 4 numbers.";
        }

        var expiry = NormalizeExpiry(expiracion);
        if (string.IsNullOrEmpty(expiry))
        {
            errors[nameof(expiracion)] = "Enter expiration as MM/YY (numbers only).";
        }
        else if (!ExpiryRegex.IsMatch(expiry))
        {
            errors[nameof(expiracion)] = "Enter a valid expiration date (MM/YY).";
        }
        else if (IsExpired(expiry))
        {
            errors[nameof(expiracion)] = "This card appears to be expired.";
        }

        var holder = (titular ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(holder))
        {
            errors[nameof(titular)] = "Enter the cardholder name.";
        }
        else if (holder.Length < 2)
        {
            errors[nameof(titular)] = "Cardholder name is too short.";
        }
        else if (!HasLetterRegex.IsMatch(holder) || OnlyDigitsAndSpacesRegex.IsMatch(holder))
        {
            errors[nameof(titular)] = "Cardholder name must use letters (not numbers only).";
        }

        var brand = (marca ?? string.Empty).Trim();
        if (string.Equals(paymentType, "Card", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(brand))
            {
                errors[nameof(marca)] = "Enter the card brand (e.g. Visa, Mastercard).";
            }
            else if (!CardBrandRegex.IsMatch(brand) || OnlyDigitsAndSpacesRegex.IsMatch(brand))
            {
                errors[nameof(marca)] = "Card brand must use letters only.";
            }
        }
        else if (!string.IsNullOrEmpty(brand)
                 && (!CardBrandRegex.IsMatch(brand) || OnlyDigitsAndSpacesRegex.IsMatch(brand)))
        {
            errors[nameof(marca)] = "Brand must use letters only.";
        }

        return errors.Count == 0;
    }

    public static string? NormalizeLastFour(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 4 ? digits : null;
    }

    public static string? NormalizeExpiry(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (ExpiryRegex.IsMatch(trimmed))
        {
            return trimmed;
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length == 4)
        {
            return $"{digits[..2]}/{digits[2..]}";
        }

        return null;
    }

    public static string? NormalizeCardholder(string? value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static bool IsExpired(string mmYy)
    {
        var parts = mmYy.Split('/');
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var month)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var year2))
        {
            return true;
        }

        var year = 2000 + year2;

        try
        {
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return lastDay < DateTime.Today;
        }
        catch
        {
            return true;
        }
    }
}
