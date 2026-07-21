using IndorMvcApp.Models;
using IndorMvcApp.Services;

namespace IndorMvcApp.Localization;

public static class IndorLocalizerExtensions
{
    /// <summary>
    /// Localize catalog/DB English strings. Home Care Essentials brand names use the same
    /// <see cref="Microservicio.ResolveBrandNombre"/> map as the home catalog.
    /// </summary>
    public static string Catalog(this IIndorLocalizer localizer, string? text)
    {
        var value = text?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        const string alwaysAvailableSuffix = " 24/7";
        if (value.EndsWith(alwaysAvailableSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var baseValue = value[..^alwaysAvailableSuffix.Length].TrimEnd();
            return $"{Microservicio.ResolveBrandNombre(0, baseValue, null, localizer.IsSpanish)}{alwaysAvailableSuffix}";
        }

        return Microservicio.ResolveBrandNombre(0, value, null, localizer.IsSpanish);
    }
}
