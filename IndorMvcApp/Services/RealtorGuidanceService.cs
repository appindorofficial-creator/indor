using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public class RealtorGuidanceService
{
    private readonly AppDbContext _db;

    public RealtorGuidanceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SolicitudRealtor?> GetOwnedAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.SolicitudesRealtor
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return null;
        }
    }

    public async Task<SolicitudRealtor> StartDraftAsync(
        string userId,
        int? propiedadId,
        string? preferredArea,
        CancellationToken cancellationToken = default)
    {
        var record = new SolicitudRealtor
        {
            PropiedadId = propiedadId,
            UserId = userId,
            NeedType = "GeneralGuidance",
            PreferredArea = preferredArea,
            Timeframe = "ASAP",
            Status = "Draft",
            GuidanceStep = 1,
            FechaCreacion = DateTime.UtcNow,
        };

        _db.SolicitudesRealtor.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task SaveStepAsync(SolicitudRealtor record, int completedStep, CancellationToken cancellationToken = default)
    {
        record.GuidanceStep = Math.Max(record.GuidanceStep, completedStep + 1);
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task FinalizeAsync(SolicitudRealtor record, CancellationToken cancellationToken = default)
    {
        record.Status = "MatchingInProgress";
        record.GuidanceStep = 4;
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public static string ResolveContactPhone(ApplicationUser user) =>
        !string.IsNullOrWhiteSpace(user.Telefono) ? user.Telefono.Trim()
        : !string.IsNullOrWhiteSpace(user.PhoneNumber) ? user.PhoneNumber.Trim()
        : string.Empty;

    public static IReadOnlyList<string> ParsePriorities(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();

    public static string JoinPriorities(IEnumerable<string> values) =>
        string.Join(",", values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct(StringComparer.OrdinalIgnoreCase).Take(3));
}
