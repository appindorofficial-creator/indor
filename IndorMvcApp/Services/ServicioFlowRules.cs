namespace IndorMvcApp.Services;

public readonly record struct ServicioFlowRoute(
    string Controller,
    string Action,
    string? HomeCarePriorityName = null,
    bool RouteUsesServicioId = false);

/// <summary>
/// Maps catalog <see cref="Models.Servicio"/> entries to existing booking flows.
/// When <see cref="HomeCarePriorityName"/> is set, resolve its Id from HomeCarePriorities at render time.
/// When <see cref="RouteUsesServicioId"/> is true, use the catalog Servicio Id as asp-route-id.
/// </summary>
public static class ServicioFlowRules
{
    public static readonly ServicioFlowRoute RemodelingRoute =
        new("RemodelingServicio", "RemodelingService", RouteUsesServicioId: true);

    private static readonly (string[] Names, ServicioFlowRoute Route)[] Routes =
    [
        (
            [
                "Dream Kitchen",
                "Cocina de Ensueño",
            ],
            RemodelingRoute
        ),
        (
            [
                "Modern Bath Pro",
                "Baño Moderno Pro",
            ],
            RemodelingRoute
        ),
        (
            [
                "Total Interior Renovation",
                "Renovación Interior Total",
            ],
            RemodelingRoute
        ),
        (
            [
                "Space Expansion",
                "Expansión de Espacios",
            ],
            RemodelingRoute
        ),
        (
            [
                "Perfect Patio",
                "Terraza Perfecta",
            ],
            RemodelingRoute
        ),
        (
            [
                "Perfect Floors",
                "Pisos Perfectos",
            ],
            RemodelingRoute
        ),
        (
            [
                "Professional Interior Painting",
                "Pintura Interior Profesional",
            ],
            RemodelingRoute
        ),
        (
            [
                "Pro Concrete Driveway",
                "Entrada de Concreto Pro",
                "Entrada de concreto Pro",
                "Driveway de concreto Pro",
            ],
            RemodelingRoute
        ),
        (
            [
                "Premium Exterior Painting",
                "Pintura Exterior Premium",
            ],
            new ServicioFlowRoute("ExteriorPaint", "ExteriorPaintReview", "Exterior paint")
        ),
        (
            [
                "Home Security",
                "Seguridad del Hogar",
            ],
            RemodelingRoute
        ),
        (
            [
                "Air Conditioning Installation",
                "Instalación Aire Acondicionado",
                "Instalación de Aire Acondicionado",
            ],
            new ServicioFlowRoute("HvacMaintenance", "HvacMaintenanceService", "HVAC maintenance")
        ),
        (
            [
                "Water Heater Pro",
                "Calentador de Agua Pro",
            ],
            new ServicioFlowRoute("WaterHeaterFlush", "WaterHeaterFlushService", "Water heater flush")
        ),
        (
            [
                "Impactful Exteriors",
                "Exteriores que Impactan",
            ],
            new ServicioFlowRoute("PowerWash", "PowerWashService", "Power wash exterior")
        ),
    ];

    /// <summary>Legacy fallback — catalog items should never use this.</summary>
    public static readonly ServicioFlowRoute SupportRoute = new("Perfil", "Soporte");

    public readonly record struct ServicioOfferingLink(string Controller, string Action, int? RouteId);

    /// <summary>
    /// Resolves the MVC link for a catalog <see cref="Models.Servicio"/> card.
    /// Never returns <see cref="SupportRoute"/> — unmapped items use <see cref="RemodelingRoute"/>.
    /// </summary>
    public static ServicioOfferingLink BuildOfferingLink(
        Models.Servicio servicio,
        IReadOnlyDictionary<string, int> homeCarePriorityIds)
    {
        var route = TryGetRoute(servicio.Nombre, servicio.Orden, out var mapped)
            ? mapped
            : RemodelingRoute;

        int? routeId = route.RouteUsesServicioId
            ? servicio.Id
            : ResolveRouteId(route, homeCarePriorityIds, servicio.Orden);

        if (!route.RouteUsesServicioId && !routeId.HasValue)
        {
            route = RemodelingRoute;
            routeId = servicio.Id;
        }

        return new ServicioOfferingLink(route.Controller, route.Action, routeId);
    }

    public static bool TryGetRoute(string? nombre, int orden, out ServicioFlowRoute route)
    {
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            foreach (var entry in Routes)
            {
                if (entry.Names.Any(n => string.Equals(n, nombre.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    route = entry.Route;
                    return true;
                }
            }
        }

        route = orden switch
        {
            1 or 2 or 3 or 4 or 6 or 9 or 10 or 12 or 13 => RemodelingRoute,
            11 => new ServicioFlowRoute("ExteriorPaint", "ExteriorPaintReview", "Exterior paint"),
            7 => new ServicioFlowRoute("HvacMaintenance", "HvacMaintenanceService", "HVAC maintenance"),
            8 => new ServicioFlowRoute("WaterHeaterFlush", "WaterHeaterFlushService", "Water heater flush"),
            5 => new ServicioFlowRoute("PowerWash", "PowerWashService", "Power wash exterior"),
            _ => RemodelingRoute,
        };

        return !string.IsNullOrWhiteSpace(route.Controller);
    }

    public static bool HasFlow(string? nombre, int orden) =>
        TryGetRoute(nombre, orden, out _);

    public static int? ResolveRouteId(
        ServicioFlowRoute route,
        IReadOnlyDictionary<string, int> homeCarePriorityIds,
        int servicioOrden = 0)
    {
        if (route.RouteUsesServicioId)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(route.HomeCarePriorityName))
        {
            if (TryResolvePriorityId(route.HomeCarePriorityName, homeCarePriorityIds, out var id))
            {
                return id;
            }
        }

        var fallbackName = servicioOrden switch
        {
            5 => "Power wash exterior",
            7 => "HVAC maintenance",
            8 => "Water heater flush",
            11 => "Exterior paint",
            _ => null
        };

        if (fallbackName != null
            && TryResolvePriorityId(fallbackName, homeCarePriorityIds, out var ordenId))
        {
            return ordenId;
        }

        return null;
    }

    private static bool TryResolvePriorityId(
        string priorityName,
        IReadOnlyDictionary<string, int> homeCarePriorityIds,
        out int id)
    {
        if (homeCarePriorityIds.TryGetValue(priorityName.Trim(), out id))
        {
            return true;
        }

        foreach (var entry in homeCarePriorityIds)
        {
            if (string.Equals(entry.Key, priorityName, StringComparison.OrdinalIgnoreCase))
            {
                id = entry.Value;
                return true;
            }
        }

        var normalizedTarget = NormalizePriorityName(priorityName);
        foreach (var entry in homeCarePriorityIds)
        {
            if (NormalizePriorityName(entry.Key) == normalizedTarget)
            {
                id = entry.Value;
                return true;
            }
        }

        id = default;
        return false;
    }

    private static string NormalizePriorityName(string value) =>
        new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
