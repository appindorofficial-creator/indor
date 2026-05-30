using IndorMvcApp.Data;
using IndorMvcApp.Models;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public static class HvacFilterReplacementService
{
    public const string ModeEvery2Months = "Every2Months";
    public const string ModeEvery3Months = "Every3Months";
    public const string ModeCustom = "Custom";

    public static int DaysForMode(string? mode) => mode switch
    {
        ModeEvery2Months => 60,
        ModeEvery3Months => 90,
        _ => 90
    };

    public static string FrequencyLabel(string? mode) => mode switch
    {
        ModeEvery2Months => "Every 2 months",
        ModeEvery3Months => "Every 3 months",
        ModeCustom => "Custom schedule",
        _ => "Every 3 months"
    };

    public static string DefaultModeForPets(bool hasPets) =>
        hasPets ? ModeEvery2Months : ModeEvery3Months;

    public static string ReminderSummary(bool week, bool day) => (week, day) switch
    {
        (true, true) => "1 week before & 1 day before",
        (true, false) => "1 week before",
        (false, true) => "1 day before",
        _ => "Off"
    };

    public static async Task UpsertFilterMaintenanceAsync(AppDbContext db, PropiedadHvacSistema record)
    {
        var existing = await db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == record.PropiedadId && m.Title.Contains("HVAC filter"))
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        if (!record.FilterRemindersEnabled)
        {
            if (existing != null)
            {
                existing.Status = "Completed";
                existing.FechaActualizacion = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();
            return;
        }

        var dueDate = record.NextFilterChangeDate?.Date
            ?? DateTime.Today.AddDays(record.FilterReminderDays);

        if (existing == null)
        {
            db.PropiedadMantenimiento.Add(new PropiedadMantenimiento
            {
                PropiedadId = record.PropiedadId,
                Title = "HVAC filter replacement",
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

    public static DateTime ComputeNextDateFromToday(PropiedadHvacSistema record) =>
        DateTime.Today.AddDays(record.FilterReminderDays);

    private static string BuildNotes(PropiedadHvacSistema record)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(record.FilterSize))
        {
            parts.Add($"Filter: {record.FilterSize}");
        }

        parts.Add(record.HasPets == true ? "Pet household" : "No pets");
        parts.Add(FrequencyLabel(record.FilterScheduleMode));
        return parts.Count > 0 ? string.Join(" • ", parts) : "Home";
    }
}
