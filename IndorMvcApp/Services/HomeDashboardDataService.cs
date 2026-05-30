using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public sealed class HomeDashboardData
{
    public List<PropiedadMantenimiento> Mantenimiento { get; init; } = [];
    public List<PropiedadDocumento> Documentos { get; init; } = [];
    public List<PropiedadHistorial> Historial { get; init; } = [];
    public PropiedadHvacSistema? HvacRecord { get; init; }
    public PropiedadWaterHeaterSistema? WaterHeaterRecord { get; init; }
}

public static class HomeDashboardDataService
{
    public static async Task<HomeDashboardData> LoadAsync(AppDbContext db, int propiedadId)
    {
        var mantenimiento = await TryLoadListAsync(
            () => db.PropiedadMantenimiento
                .AsNoTracking()
                .Where(m => m.PropiedadId == propiedadId)
                .OrderBy(m => m.DueDate)
                .ToListAsync());

        var documentos = await TryLoadListAsync(
            () => db.PropiedadDocumentos
                .AsNoTracking()
                .Where(d => d.PropiedadId == propiedadId)
                .OrderByDescending(d => d.FechaCreacion)
                .Take(5)
                .ToListAsync());

        var historial = await TryLoadListAsync(
            () => db.PropiedadHistorial
                .AsNoTracking()
                .Where(h => h.PropiedadId == propiedadId)
                .OrderByDescending(h => h.FechaCreacion)
                .Take(5)
                .ToListAsync());

        var hvacRecord = await PropiedadHvacQueryHelper.TryGetByPropiedadIdAsync(db, propiedadId);
        var waterHeaterRecord = await PropiedadWaterHeaterQueryHelper.TryGetByPropiedadIdAsync(db, propiedadId);

        return new HomeDashboardData
        {
            Mantenimiento = mantenimiento,
            Documentos = documentos,
            Historial = historial,
            HvacRecord = hvacRecord,
            WaterHeaterRecord = waterHeaterRecord
        };
    }

    private static async Task<List<T>> TryLoadListAsync<T>(Func<Task<List<T>>> query)
    {
        try
        {
            return await query();
        }
        catch (Exception ex) when (IsMissingTable(ex))
        {
            return [];
        }
    }

    internal static bool IsMissingTable(Exception ex)
    {
        for (var current = ex; current != null; current = current.InnerException)
        {
            if (current is SqlException sql && sql.Number == 208)
            {
                return true;
            }
        }

        return ex.Message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase);
    }
}
