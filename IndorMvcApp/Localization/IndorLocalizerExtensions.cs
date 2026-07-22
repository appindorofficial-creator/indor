using IndorMvcApp.Models;
using IndorMvcApp.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

    /// <summary>
    /// Rewrites DataAnnotations / Identity ModelState error messages through
    /// <see cref="IIndorLocalizer"/> so server-side validation matches the UI culture.
    /// </summary>
    public static void LocalizeModelState(this ModelStateDictionary modelState, IIndorLocalizer localizer)
    {
        if (!localizer.IsSpanish || modelState.ErrorCount == 0)
        {
            return;
        }

        foreach (var key in modelState.Keys.ToList())
        {
            var entry = modelState[key];
            if (entry is null || entry.Errors.Count == 0)
            {
                continue;
            }

            var messages = entry.Errors
                .Select(e => string.IsNullOrEmpty(e.ErrorMessage)
                    ? e.ErrorMessage
                    : localizer[e.ErrorMessage])
                .ToList();

            entry.Errors.Clear();
            foreach (var message in messages)
            {
                entry.Errors.Add(new ModelError(message ?? string.Empty));
            }
        }
    }
}
