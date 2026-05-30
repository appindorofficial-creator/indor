using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public static class PropiedadHvacQueryHelper
{
    /// <summary>
    /// Returns HVAC record when the table exists; null if missing or not migrated yet.
    /// </summary>
    public static async Task<PropiedadHvacSistema?> TryGetByPropiedadIdAsync(AppDbContext db, int propiedadId)
    {
        try
        {
            return await db.PropiedadHvacSistemas
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.PropiedadId == propiedadId);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }
}
