using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public static class PropiedadWaterHeaterQueryHelper
{
    public static async Task<PropiedadWaterHeaterSistema?> TryGetByPropiedadIdAsync(AppDbContext db, int propiedadId)
    {
        try
        {
            return await db.PropiedadWaterHeaterSistemas
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.PropiedadId == propiedadId);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }
}
