namespace IndorMvcApp.Services;

/// <summary>
/// Shared contact-phone rules for Property Administrator service flows.
/// UI always uses _PropertyAdminUrgentContactPhone (unified required label).
/// Use <see cref="IsProvided"/> for server-side checks.
/// </summary>
public static class PropertyAdministratorContactPhone
{
    public static bool IsProvided(string? value) => !string.IsNullOrWhiteSpace(value);
}
