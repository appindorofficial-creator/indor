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
            new ServicioFlowRoute("SmokeDetector", "SmokeDetectorService", "Smoke Detector")
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

    /// <summary>Legacy fallback — should not be used when all services are mapped.</summary>
    public static readonly ServicioFlowRoute SupportRoute = new("Perfil", "Soporte");

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
            1 or 2 or 3 or 4 or 6 or 9 or 10 or 13 => RemodelingRoute,
            11 => new ServicioFlowRoute("ExteriorPaint", "ExteriorPaintReview", "Exterior paint"),
            12 => new ServicioFlowRoute("SmokeDetector", "SmokeDetectorService", "Smoke Detector"),
            7 => new ServicioFlowRoute("HvacMaintenance", "HvacMaintenanceService", "HVAC maintenance"),
            8 => new ServicioFlowRoute("WaterHeaterFlush", "WaterHeaterFlushService", "Water heater flush"),
            5 => new ServicioFlowRoute("PowerWash", "PowerWashService", "Power wash exterior"),
            _ => default,
        };

        return !string.IsNullOrWhiteSpace(route.Controller);
    }

    public static bool HasFlow(string? nombre, int orden) =>
        TryGetRoute(nombre, orden, out _);

    public static int? ResolveRouteId(
        ServicioFlowRoute route,
        IReadOnlyDictionary<string, int> homeCarePriorityIds)
    {
        if (route.RouteUsesServicioId)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(route.HomeCarePriorityName))
        {
            return null;
        }

        return homeCarePriorityIds.TryGetValue(route.HomeCarePriorityName, out var id) ? id : null;
    }
}
