using IndorMvcApp.Models;

namespace IndorMvcApp.Validation;

public static class RealtorInviteClientValidation
{
    public static IReadOnlyDictionary<string, string> Validate(
        string? fullName, string? email, string? phone, string? clientRole)
    {
        var errors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!PersonNameAttribute.IsValidName(fullName, out var nameError))
        {
            errors[nameof(fullName)] = nameError!;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors[nameof(email)] = "Email address is required.";
        }
        else if (!ValidEmailAttribute.IsValidAddress(email, out var emailError))
        {
            errors[nameof(email)] = emailError!;
        }

        if (!UsPhoneRequiredAttribute.IsValidRequired(phone, out var phoneError))
        {
            errors[nameof(phone)] = phoneError!;
        }

        if (string.IsNullOrWhiteSpace(clientRole) ||
            !RealtorClientRoles.All.Contains(clientRole, StringComparer.OrdinalIgnoreCase))
        {
            errors[nameof(clientRole)] = "Please select a client role.";
        }

        return errors;
    }

    public static bool IsValid(
        string? fullName, string? email, string? phone, string? clientRole) =>
        Validate(fullName, email, phone, clientRole).Count == 0;
}
