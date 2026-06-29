using System.ComponentModel.DataAnnotations;

namespace IndorMvcApp.Validation;

/// <summary>
/// Required US phone: must be 10 digits (optional leading country code 1).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UsPhoneRequiredAttribute : ValidationAttribute
{
    public UsPhoneRequiredAttribute()
        : base("Enter a valid 10-digit US phone number (e.g. 555 123 4567).")
    {
    }

    public static bool IsValidRequired(string? value, out string? errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "Phone number is required.";
            return false;
        }

        if (!UsPhoneOptionalAttribute.IsValidOptional(value))
        {
            errorMessage = "Enter a valid 10-digit US phone number (e.g. 555 123 4567).";
            return false;
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var text = value as string;
        return IsValidRequired(text, out var message)
            ? ValidationResult.Success
            : new ValidationResult(message ?? ErrorMessage ?? "Enter a valid 10-digit US phone number.");
    }
}
