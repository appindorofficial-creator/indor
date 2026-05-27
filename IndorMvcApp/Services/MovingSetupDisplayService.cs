using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class MovingSetupDisplayService
{
    public static MovingSetupSectionViewModel? Build(
        MovingSetupConfig? config,
        IEnumerable<MovingSetupServicio> servicios,
        IEnumerable<MovingSetupEnlaceRapido> enlaces,
        IUrlHelper urlHelper)
    {
        if (config == null || !config.Activo)
        {
            return null;
        }

        var serviceItems = servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .Select(s => new MovingSetupServiceItemViewModel
            {
                Id = s.Id,
                Nombre = s.Nombre,
                IconoClase = s.IconoClase,
                Url = ResolveUrl(urlHelper, s.LinkController, s.LinkAction, s.LinkRouteId)
            })
            .ToList();

        if (serviceItems.Count == 0)
        {
            return null;
        }

        var texts = SplitPipe(config.FeaturedCaracteristicas);
        var icons = SplitPipe(config.FeaturedIconosCaracteristicas);
        var features = new List<MovingSetupFeatureViewModel>();
        for (var i = 0; i < texts.Length; i++)
        {
            features.Add(new MovingSetupFeatureViewModel
            {
                Icon = i < icons.Length && !string.IsNullOrWhiteSpace(icons[i]) ? icons[i] : "fa-check",
                Text = texts[i]
            });
        }

        return new MovingSetupSectionViewModel
        {
            Titulo = config.Titulo,
            Subtitulo = config.Subtitulo,
            IconoClase = config.IconoClase,
            ViewAllTexto = config.ViewAllTexto,
            ViewAllUrl = string.IsNullOrWhiteSpace(config.ViewAllUrl)
                ? urlHelper.Action("Index", "MovingSetup")
                : config.ViewAllUrl,
            FeaturedEtiqueta = config.FeaturedEtiqueta,
            FeaturedTitulo = config.FeaturedTitulo,
            FeaturedDescripcion = config.FeaturedDescripcion,
            FeaturedImagenUrl = config.FeaturedImagenUrl,
            FeaturedCtaTexto = config.FeaturedCtaTexto,
            FeaturedCtaUrl = ResolveUrl(
                urlHelper,
                config.FeaturedCtaController ?? "MovingSetup",
                config.FeaturedCtaAction ?? "Index",
                config.FeaturedCtaRouteId),
            FeaturedCaracteristicas = features,
            Servicios = serviceItems,
            EnlacesRapidos = enlaces
                .Where(e => e.Activo)
                .OrderBy(e => e.Orden)
                .Select(e => new MovingSetupQuickLinkViewModel
                {
                    Id = e.Id,
                    Nombre = e.Nombre,
                    IconoClase = e.IconoClase,
                    Url = !string.IsNullOrWhiteSpace(e.LinkUrl)
                        ? e.LinkUrl
                        : ResolveUrl(urlHelper, e.LinkController, e.LinkAction, e.LinkRouteId)
                })
                .ToList()
        };
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

    private static string[] SplitPipe(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
