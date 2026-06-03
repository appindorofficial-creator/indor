using IndorMvcApp.Models;

namespace IndorMvcApp.Helpers;

public static class UserDisplayName
{
    public static string Format(ApplicationUser? user)
    {
        if (user == null)
        {
            return string.Empty;
        }

        return Format(user.Nombre, user.Apellidos);
    }

    public static string Format(string? nombre, string? apellidos)
    {
        return string.Join(" ",
                new[] { nombre, apellidos }
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p!.Trim()))
            .Trim();
    }
}
