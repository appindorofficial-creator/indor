using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IndorMvcApp.Services;

/// <summary>
/// Caches the shared, near-static catalog tables shown on the Home dashboard.
/// These tables are identical for every user and only change when an admin edits
/// them, so keeping a single in-memory snapshot removes ~13 DB round-trips from
/// every Home/Index request (a big win on Azure SQL where each round-trip has
/// latency). A short TTL means admin edits become visible within a couple of
/// minutes without any manual cache invalidation; call <see cref="Invalidate"/>
/// after an admin edit if you want the change to appear immediately.
/// </summary>
public sealed class HomeCatalogCache
{
    private const string CacheKey = "home-catalog-snapshot:v1";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _cache;

    public HomeCatalogCache(IMemoryCache cache) => _cache = cache;

    public async Task<HomeCatalogSnapshot> GetAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out HomeCatalogSnapshot? cached) && cached != null)
        {
            return cached;
        }

        var snapshot = await LoadAsync(db, cancellationToken);
        _cache.Set(CacheKey, snapshot, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Ttl
        });
        return snapshot;
    }

    public void Invalidate() => _cache.Remove(CacheKey);

    private static async Task<HomeCatalogSnapshot> LoadAsync(AppDbContext db, CancellationToken ct)
    {
        var microservicios = await db.Microservicios.AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Id)
            .ToListAsync(ct);

        var servicios = await db.Servicios.AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync(ct);

        var inspecciones = await db.Inspecciones.AsNoTracking()
            .Where(i => i.Activo)
            .OrderBy(i => i.Orden)
            .ThenBy(i => i.Id)
            .ToListAsync(ct);

        var serviciosEmergencia = await db.ServiciosEmergencia.AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ThenBy(s => s.Id)
            .ToListAsync(ct);

        var homeCarePriorities = await db.HomeCarePriorities.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ThenBy(p => p.Id)
            .ToListAsync(ct);

        var homeCarePriorityIds = homeCarePriorities
            .GroupBy(p => p.Nombre.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);

        var movingConfig = await db.MovingSetupConfig.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Activo, ct);
        var movingServicios = await db.MovingSetupServicios.AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Orden)
            .ToListAsync(ct);
        var movingEnlaces = await db.MovingSetupEnlacesRapidos.AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.Orden)
            .ToListAsync(ct);

        var prioritiesConfig = await db.HomeCarePrioritiesConfig.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Activo, ct);

        var planesMembresia = await db.PlanesMembresia.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync(ct);

        var planesInternet = await db.PlanesInternet.AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Orden)
            .ToListAsync(ct);

        return new HomeCatalogSnapshot
        {
            Microservicios = microservicios,
            Servicios = servicios,
            Inspecciones = inspecciones,
            ServiciosEmergencia = serviciosEmergencia,
            HomeCarePriorities = homeCarePriorities,
            HomeCarePriorityIds = homeCarePriorityIds,
            MovingConfig = movingConfig,
            MovingServicios = movingServicios,
            MovingEnlaces = movingEnlaces,
            PrioritiesConfig = prioritiesConfig,
            PlanesMembresia = planesMembresia,
            PlanesInternet = planesInternet
        };
    }
}

/// <summary>Immutable snapshot of the shared Home catalog data cached in memory.</summary>
public sealed class HomeCatalogSnapshot
{
    public required List<Microservicio> Microservicios { get; init; }
    public required List<Servicio> Servicios { get; init; }
    public required List<Inspeccion> Inspecciones { get; init; }
    public required List<ServicioEmergencia> ServiciosEmergencia { get; init; }
    public required List<HomeCarePriority> HomeCarePriorities { get; init; }
    public required Dictionary<string, int> HomeCarePriorityIds { get; init; }
    public MovingSetupConfig? MovingConfig { get; init; }
    public required List<MovingSetupServicio> MovingServicios { get; init; }
    public required List<MovingSetupEnlaceRapido> MovingEnlaces { get; init; }
    public HomeCarePrioritiesConfig? PrioritiesConfig { get; init; }
    public required List<PlanMembresia> PlanesMembresia { get; init; }
    public required List<PlanInternet> PlanesInternet { get; init; }
}
