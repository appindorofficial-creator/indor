using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IndorMvcApp.Services;

public static class MovingSetupDisplayService
{
    private static readonly Dictionary<string, (string Controller, string Action)> DefaultFlows =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Moving"] = ("Moving", "MovingService"),
            ["Cleaning"] = ("Cleaning", "CleaningService"),
            ["Packing Help"] = ("Packing", "PackingService"),
            ["Furniture & Assembly"] = ("FurnitureAssembly", "FurnitureAssemblyService"),
            ["TV Wall Mounting"] = ("TvWallMounting", "TvWallMountingService"),
            ["Utilities Setup"] = ("UtilitiesSetup", "UtilitiesSetupAddress"),
            ["General Help"] = ("GeneralHelp", "GeneralHelpRequest"),
        };

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
                Url = ResolveServicioUrl(urlHelper, s)
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

        var featuredCtaUrl = ResolveFeaturedCtaUrl(urlHelper, config, servicios);

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
            FeaturedCtaUrl = featuredCtaUrl,
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
                    Url = ResolveQuickLinkUrl(urlHelper, e)
                })
                .ToList()
        };
    }

    private static string? ResolveServicioUrl(IUrlHelper urlHelper, MovingSetupServicio servicio)
    {
        var url = ResolveUrl(urlHelper, servicio.LinkController, servicio.LinkAction, servicio.LinkRouteId);

        if (!IsStaleMovingSetupLink(servicio) && !string.IsNullOrWhiteSpace(url))
        {
            return url;
        }

        if (DefaultFlows.TryGetValue(servicio.Nombre, out var flow))
        {
            return urlHelper.Action(flow.Action, flow.Controller, new { id = servicio.Id });
        }

        return url;
    }

    private static bool IsStaleMovingSetupLink(MovingSetupServicio servicio) =>
        string.Equals(servicio.LinkController, "MovingSetup", StringComparison.OrdinalIgnoreCase)
        && string.Equals(servicio.LinkAction, "Index", StringComparison.OrdinalIgnoreCase);

    private static bool IsStaleMovingSetupCta(MovingSetupConfig config) =>
        string.Equals(config.FeaturedCtaController, "MovingSetup", StringComparison.OrdinalIgnoreCase)
        && string.Equals(config.FeaturedCtaAction, "Index", StringComparison.OrdinalIgnoreCase);

    /// <summary>"Start moving setup" opens the Moving booking flow, not the catalog index.</summary>
    private static string? ResolveFeaturedCtaUrl(
        IUrlHelper urlHelper,
        MovingSetupConfig config,
        IEnumerable<MovingSetupServicio> servicios)
    {
        var movingServicio = servicios.FirstOrDefault(s =>
            string.Equals(s.Nombre, "Moving", StringComparison.OrdinalIgnoreCase));

        if (!IsStaleMovingSetupCta(config)
            && !string.IsNullOrWhiteSpace(config.FeaturedCtaController)
            && !string.IsNullOrWhiteSpace(config.FeaturedCtaAction))
        {
            var configuredUrl = ResolveUrl(
                urlHelper,
                config.FeaturedCtaController,
                config.FeaturedCtaAction,
                config.FeaturedCtaRouteId);
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl;
            }
        }

        if (movingServicio != null)
        {
            return urlHelper.Action("MovingService", "Moving", new { id = movingServicio.Id });
        }

        return urlHelper.Action("Index", "MovingSetup");
    }

    private static string? ResolveQuickLinkUrl(IUrlHelper urlHelper, MovingSetupEnlaceRapido enlace)
    {
        if (!string.IsNullOrWhiteSpace(enlace.LinkUrl))
        {
            return enlace.LinkUrl;
        }

        if (IsStaleMovingSetupLink(enlace.LinkController, enlace.LinkAction))
        {
            return urlHelper.Action("Index", "MovingSetup");
        }

        return ResolveUrl(urlHelper, enlace.LinkController, enlace.LinkAction, enlace.LinkRouteId);
    }

    private static bool IsStaleMovingSetupLink(string? controller, string? action) =>
        string.Equals(controller, "MovingSetup", StringComparison.OrdinalIgnoreCase)
        && string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase);

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
