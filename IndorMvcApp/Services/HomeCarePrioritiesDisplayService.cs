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
        IUrlHelper urlHelper)
    {
        if (config == null || !config.Activo)
        {
            return null;
        }

        var cards = items
            .Where(i => i.Activo)
            .OrderBy(i => i.Orden)
            .ThenBy(i => i.Id)
            .Select(i => new HomeCarePriorityCardViewModel
            {
                Title = i.Nombre,
                Subtitle = i.Subtitulo,
                ImageUrl = i.ImagenUrl ?? string.Empty,
                Icon = i.IconoClase,
                Url = ResolveItemUrl(urlHelper, i, propiedadId)
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
            Subtitle = config.Subtitulo,
            IconClass = config.IconoClase,
            ViewAllText = config.ViewAllTexto,
            ViewAllUrl = ResolveViewAllUrl(urlHelper, config, propiedadId),
            Items = cards
        };
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
            config.ViewAllController ?? "MyHome",
            config.ViewAllAction ?? "Maintenance",
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
