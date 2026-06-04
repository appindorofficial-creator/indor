namespace IndorMvcApp.Services;

public readonly record struct ServicioFlowRoute(
    string Controller,
    string Action,
    string? HomeCarePriorityName = null);

/// <summary>
/// Maps catalog <see cref="Models.Servicio"/> entries to existing booking flows.
/// When <see cref="HomeCarePriorityName"/> is set, resolve its Id from HomeCarePriorities at render time.
/// </summary>
public static class ServicioFlowRules
{
    private static readonly (string[] Names, ServicioFlowRoute Route)[] Routes =
    [
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

    /// <summary>Fallback for remodeling / custom projects until dedicated quote flows exist.</summary>
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
            11 => Routes[0].Route,
            12 => Routes[1].Route,
            7 => Routes[2].Route,
            8 => Routes[3].Route,
            5 => Routes[4].Route,
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
        if (string.IsNullOrWhiteSpace(route.HomeCarePriorityName))
        {
            return null;
        }

        return homeCarePriorityIds.TryGetValue(route.HomeCarePriorityName, out var id) ? id : null;
    }
}
