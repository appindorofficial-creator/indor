using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public static class WaterHeaterFlushReminderService
{
    public const int FlushIntervalDays = 365;

    public static readonly string[] CommonLocations =
    [
        "Basement",
        "Garage",
        "Utility room",
        "Laundry room",
        "Closet",
        "Other"
    ];

    public static string ReminderSummary(bool week, bool day) => (week, day) switch
    {
        (true, true) => "1 week before & 1 day before",
        (true, false) => "1 week before",
        (false, true) => "1 day before",
        _ => "Off"
    };

    public static DateTime DefaultNextFlushDate(PropiedadWaterHeaterSistema record) =>
        record.NextFlushDate?.Date
        ?? record.LastServiceDate?.Date.AddDays(FlushIntervalDays)
        ?? DateTime.Today.AddDays(FlushIntervalDays);

    public static async Task UpsertFlushMaintenanceAsync(AppDbContext db, PropiedadWaterHeaterSistema record)
    {
        var existing = await db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == record.PropiedadId
                && m.Title.Contains("water heater flush", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        if (!record.FlushRemindersEnabled || !record.FlushReminderSetupComplete)
        {
            if (existing != null)
            {
                existing.Status = "Completed";
                existing.FechaActualizacion = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return;
        }

        var dueDate = record.NextFlushDate?.Date ?? DefaultNextFlushDate(record);

        if (existing == null)
        {
            db.PropiedadMantenimiento.Add(new PropiedadMantenimiento
            {
                PropiedadId = record.PropiedadId,
                Title = "Annual water heater flush",
                DueDate = dueDate,
                Status = "Upcoming",
                Notes = BuildNotes(record),
                FechaCreacion = DateTime.UtcNow
            });
        }
        else
        {
            existing.DueDate = dueDate;
            existing.Status = "Upcoming";
            existing.Notes = BuildNotes(record);
            existing.FechaActualizacion = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private static string BuildNotes(PropiedadWaterHeaterSistema record)
    {
        var location = string.IsNullOrWhiteSpace(record.FlushLocation) ? "Home" : record.FlushLocation.Trim();
        return record.AutoRepeatEnabled
            ? $"{location} • Repeats every 12 months"
            : location;
    }
}
