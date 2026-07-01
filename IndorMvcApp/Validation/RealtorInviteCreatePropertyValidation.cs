namespace IndorMvcApp.Validation;

public static class RealtorInviteCreatePropertyValidation
{
    public static IReadOnlyDictionary<string, string> Validate(
        string? address, string? city, string? stateCode, string? postalCode)
    {
        var errors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(address))
        {
            errors["Address"] = "Property address is required.";
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            errors["City"] = "City is required.";
        }

        if (string.IsNullOrWhiteSpace(stateCode))
        {
            errors["StateCode"] = "State is required.";
        }

        if (!UsZipCodeAttribute.IsValidRequired(postalCode, out var zipError))
        {
            errors["PostalCode"] = zipError!;
        }

        return errors;
    }
}
