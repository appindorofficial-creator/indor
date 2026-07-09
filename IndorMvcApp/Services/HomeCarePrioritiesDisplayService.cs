using System.Globalization;
using IndorMvcApp.Localization;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class HomeCarePrioritiesDisplayService
{
    public static HomeCarePrioritiesSectionViewModel? Build(
        HomeCarePrioritiesConfig? config,
        IEnumerable<HomeCarePriority> items,
        int? propiedadId,
        IUrlHelper urlHelper,
        bool isSpanish = false)
    {
        if (config == null || !config.Activo)
        {
            return null;
        }

        var ordered = items
            .Where(i => i.Activo)
            .OrderBy(i => i.Orden)
            .ThenBy(i => i.Id)
            .ToList();

        var cards = ordered
            .Select((item, index) => new HomeCarePriorityCardViewModel
            {
                Orden = index + 1,
                Title = item.Nombre,
                Subtitle = item.Subtitulo,
                ImageUrl = item.ImagenUrl ?? string.Empty,
                Icon = item.IconoClase,
                Url = ResolveItemUrl(urlHelper, item, propiedadId)
            })
            .ToList();

        if (cards.Count == 0)
        {
            return null;
        }

        return new HomeCarePrioritiesSectionViewModel
        {
            PropiedadId = propiedadId,
            Title = config.Titulo,
            Subtitle = FormatCountSubtitle(cards.Count, isSpanish),
            IconClass = config.IconoClase,
            ViewAllText = config.ViewAllTexto,
            ViewAllUrl = ResolveViewAllUrl(urlHelper, config, propiedadId),
            Items = cards
        };
    }

    private static string FormatCountSubtitle(int count, bool isSpanish)
    {
        var key = count == 1
            ? "{0} maintenance task for your home"
            : "{0} maintenance tasks for your home";
        var template = isSpanish && UiTranslations.Spanish.TryGetValue(key, out var es) ? es : key;
        return string.Format(CultureInfo.CurrentCulture, template, count);
    }

    private static string? ResolveViewAllUrl(
        IUrlHelper urlHelper,
        HomeCarePrioritiesConfig config,
        int? propiedadId)
    {
        if (!propiedadId.HasValue)
        {
            return null;
        }

        return ResolveUrl(
            urlHelper,
            config.ViewAllController ?? "Home",
            config.ViewAllAction ?? "HomeCareGuide",
            propiedadId);
    }

    private static string? ResolveItemUrl(IUrlHelper urlHelper, HomeCarePriority item, int? propiedadId)
    {
        if (!string.IsNullOrWhiteSpace(item.LinkUrl))
        {
            return item.LinkUrl;
        }

        if (!string.IsNullOrWhiteSpace(item.LinkController)
            && !string.Equals(item.LinkController, "MyHome", StringComparison.OrdinalIgnoreCase))
        {
            return urlHelper.Action(item.LinkAction ?? "Index", item.LinkController, new { id = item.Id });
        }

        if (!propiedadId.HasValue)
        {
            return null;
        }

        return ResolveUrl(
            urlHelper,
            item.LinkController ?? "MyHome",
            item.LinkAction ?? "Maintenance",
            propiedadId);
    }

    private static string? ResolveUrl(IUrlHelper urlHelper, string? controller, string? action, int? routeId)
    {
        if (string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
        {
            return null;
        }

        return routeId.HasValue
            ? urlHelper.Action(action, controller, new { id = routeId.Value })
            : urlHelper.Action(action, controller);
    }
}
