using System.Globalization;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IndorMvcApp.Services;

public static class ScheduleDisplayService
{
    private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

    public static async Task<ScheduleSectionViewModel> BuildAsync(
        AppDbContext db,
        string userId,
        int? propiedadId,
        IUrlHelper url)
    {
        var propIds = propiedadId.HasValue
            ? [propiedadId.Value]
            : await db.Propiedades
                .Where(p => p.UserId == userId && p.Activo)
                .Select(p => p.Id)
                .ToListAsync();

        var microservicios = await db.Microservicios
            .AsNoTracking()
            .Where(m => m.Activo)
            .ToListAsync();

        var trashMicro = FindMicro(microservicios, 3, "Stress-Free Trash");
        var lawnMicro = FindMicro(microservicios, 2, "Always Perfect Lawn");
        var safeAirMicro = FindMicro(microservicios, 1, "Safe Air 365");
        var cleaningMicro = FindMicro(microservicios, 4, "Cleaning Pro");

        var programaciones = await db.ProgramacionesMicroservicio
            .AsNoTracking()
            .Include(p => p.Microservicio)
            .Where(p => p.UserId == userId && p.Estado == "Scheduled")
            .ToListAsync();

        var mantenimiento = propIds.Count == 0
            ? []
            : await db.PropiedadMantenimiento
                .AsNoTracking()
                .Where(m => propIds.Contains(m.PropiedadId) && m.Status != "Completed")
                .ToListAsync();

        var hvacRecords = propiedadId.HasValue
            ? await db.PropiedadHvacSistemas
                .AsNoTracking()
                .Where(h => h.PropiedadId == propiedadId.Value)
                .ToListAsync()
            : [];

        var waterHeaterRecords = propiedadId.HasValue
            ? await db.PropiedadWaterHeaterSistemas
                .AsNoTracking()
                .Where(w => w.PropiedadId == propiedadId.Value)
                .ToListAsync()
            : [];

        var trashSolicitudes = await db.SolicitudesTrash
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        var lawnSolicitudes = await db.SolicitudesLawn
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        var safeAirSolicitudes = await db.SolicitudesSafeAir
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        var cleaningSolicitudes = await db.SolicitudesCleaningPro
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        var hvacSolicitudes = await db.SolicitudesHvacMaintenance
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Estado == "Submitted")
            .OrderByDescending(s => s.FechaConfirmacion ?? s.FechaActualizacion ?? s.FechaCreacion)
            .ToListAsync();

        var hvacMaintenancePriorityId = await db.HomeCarePriorities
            .AsNoTracking()
            .Where(p => p.Activo && p.Nombre == "HVAC maintenance")
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync()
            ?? await db.HomeCarePriorities
                .AsNoTracking()
                .Where(p => p.Activo && p.Orden == 7)
                .Select(p => (int?)p.Id)
                .FirstOrDefaultAsync();

        var comingUp = new List<ScheduleReminderItemViewModel>();

        foreach (var item in programaciones)
        {
            comingUp.Add(MapProgramacion(
                item,
                trashMicro,
                lawnMicro,
                safeAirMicro,
                cleaningMicro,
                trashSolicitudes,
                lawnSolicitudes,
                safeAirSolicitudes,
                cleaningSolicitudes,
                url));
        }

        foreach (var item in hvacSolicitudes)
        {
            comingUp.Add(MapHvacSolicitud(item, url));
        }

        foreach (var item in mantenimiento)
        {
            if (programaciones.Any(p =>
                    p.PropiedadId == item.PropiedadId
                    && IsDuplicateMaintenance(p, item)))
            {
                continue;
            }

            if (string.Equals(item.Title, "HVAC Tune-Up", StringComparison.OrdinalIgnoreCase)
                && hvacSolicitudes.Any(s => s.PropiedadId == item.PropiedadId))
            {
                continue;
            }

            comingUp.Add(MapMantenimiento(item, hvacRecords, waterHeaterRecords, hvacSolicitudes, url));
        }

        comingUp = comingUp
            .OrderBy(i => i.SortDate)
            .ToList();

        return new ScheduleSectionViewModel
        {
            HasProperty = propiedadId.HasValue,
            PropiedadId = propiedadId,
            QuickAddItems = BuildQuickAdd(
                propiedadId,
                trashMicro,
                lawnMicro,
                hvacRecords,
                hvacMaintenancePriorityId,
                url),
            ComingUpItems = comingUp,
            CreateReminderUrl = propiedadId.HasValue
                ? url.Action("MaintenanceCreate", "MyHome", new { id = propiedadId.Value }) ?? "#"
                : url.Action("AddProperty", "Propietario") ?? "#",
            BookServiceUrl = "#section-services"
        };
    }

    private static List<ScheduleQuickAddItemViewModel> BuildQuickAdd(
        int? propiedadId,
        Microservicio? trashMicro,
        Microservicio? lawnMicro,
        List<PropiedadHvacSistema> hvacRecords,
        int? hvacMaintenancePriorityId,
        IUrlHelper url)
    {
        var items = new List<ScheduleQuickAddItemViewModel>();

        if (trashMicro != null)
        {
            items.Add(new ScheduleQuickAddItemViewModel
            {
                Label = "Trash",
                IconClass = "fa-trash-can",
                ToneClass = "sch-tone-trash",
                Url = url.Action("TrashService", "Trash", new { id = trashMicro.Id }) ?? "#"
            });
        }

        if (propiedadId.HasValue)
        {
            items.Add(new ScheduleQuickAddItemViewModel
            {
                Label = "Filter",
                IconClass = "fa-table-cells",
                ToneClass = "sch-tone-filter",
                Url = ResolveFilterQuickAddUrl(propiedadId.Value, hvacRecords, url)
            });
        }

        if (lawnMicro != null)
        {
            items.Add(new ScheduleQuickAddItemViewModel
            {
                Label = "Lawn",
                IconClass = "fa-seedling",
                ToneClass = "sch-tone-lawn",
                Url = url.Action("LawnService", "Lawn", new { id = lawnMicro.Id }) ?? "#"
            });
        }

        if (propiedadId.HasValue)
        {
            items.Add(new ScheduleQuickAddItemViewModel
            {
                Label = "HVAC",
                IconClass = "fa-snowflake",
                ToneClass = "sch-tone-hvac",
                Url = ResolveHvacQuickAddUrl(hvacMaintenancePriorityId, propiedadId.Value, url)
            });

            items.Add(new ScheduleQuickAddItemViewModel
            {
                Label = "Water Heater",
                IconClass = "fa-fire-flame-simple",
                ToneClass = "sch-tone-water",
                Url = url.Action("Intro", "WaterHeaterFlushReminder", new { id = propiedadId.Value }) ?? "#"
            });
        }

        return items;
    }

    private static string ResolveFilterQuickAddUrl(
        int propiedadId,
        List<PropiedadHvacSistema> hvacRecords,
        IUrlHelper url)
    {
        var hvac = hvacRecords.FirstOrDefault(h => h.PropiedadId == propiedadId);
        if (hvac == null)
        {
            return url.Action("Add", "HvacSetup", new { propiedadId }) ?? "#";
        }

        return url.Action("Pets", "HvacFilterReplacement", new { id = propiedadId }) ?? "#";
    }

    private static string ResolveHvacQuickAddUrl(int? hvacMaintenancePriorityId, int propiedadId, IUrlHelper url)
    {
        if (hvacMaintenancePriorityId.HasValue)
        {
            return url.Action("HvacMaintenanceService", "HvacMaintenance", new { id = hvacMaintenancePriorityId.Value }) ?? "#";
        }

        return url.Action("Add", "HvacSetup", new { propiedadId }) ?? "#";
    }

    private static ScheduleReminderItemViewModel MapProgramacion(
        ProgramacionMicroservicio item,
        Microservicio? trashMicro,
        Microservicio? lawnMicro,
        Microservicio? safeAirMicro,
        Microservicio? cleaningMicro,
        List<SolicitudTrash> trashSolicitudes,
        List<SolicitudLawn> lawnSolicitudes,
        List<SolicitudSafeAir> safeAirSolicitudes,
        List<SolicitudCleaningPro> cleaningSolicitudes,
        IUrlHelper url)
    {
        var microName = item.Microservicio?.Nombre ?? "Reminder";
        var notes = ParseNotes(item.Notas);
        var tone = ResolveTone(item.MicroservicioId, microName, trashMicro, lawnMicro, safeAirMicro);
        var (title, subtitle) = BuildProgramacionCopy(item, microName, notes, trashMicro, lawnMicro, safeAirMicro);

        return new ScheduleReminderItemViewModel
        {
            SourceKey = $"prog-{item.Id}",
            Title = title,
            Subtitle = subtitle,
            DateLabel = FormatDateLabel(item.FechaProgramada),
            SortDate = item.FechaProgramada.Date,
            IconClass = tone.icon,
            ToneClass = tone.tone,
            EditUrl = ResolveProgramacionEditUrl(
                item,
                trashMicro,
                lawnMicro,
                safeAirMicro,
                cleaningMicro,
                trashSolicitudes,
                lawnSolicitudes,
                safeAirSolicitudes,
                cleaningSolicitudes,
                url),
            EditLabel = "Edit"
        };
    }

    private static ScheduleReminderItemViewModel MapHvacSolicitud(
        SolicitudHvacMaintenance item,
        IUrlHelper url)
    {
        var dueDate = item.FechaVisita?.Date ?? DateTime.Today.AddDays(7);

        return new ScheduleReminderItemViewModel
        {
            SourceKey = $"hvac-sol-{item.Id}",
            Title = "HVAC Tune-Up",
            Subtitle = $"{HvacMaintenanceDisplayLabels.FormatTimeWindow(item.VentanaHorario)} · Tune-up visit",
            DateLabel = FormatDateLabel(dueDate),
            SortDate = dueDate,
            IconClass = "fa-fan",
            ToneClass = "sch-tone-hvac",
            EditUrl = url.Action("HvacMaintenanceConfirmed", "HvacMaintenance", new { id = item.Id }) ?? "#",
            EditLabel = "View"
        };
    }

    private static ScheduleReminderItemViewModel MapMantenimiento(
        PropiedadMantenimiento item,
        List<PropiedadHvacSistema> hvacRecords,
        List<PropiedadWaterHeaterSistema> waterHeaterRecords,
        List<SolicitudHvacMaintenance> hvacSolicitudes,
        IUrlHelper url)
    {
        var tone = ResolveMaintenanceTone(item.Title);
        var dueDate = item.DueDate?.Date ?? DateTime.Today.AddDays(30);

        return new ScheduleReminderItemViewModel
        {
            SourceKey = $"maint-{item.Id}",
            Title = FormatMaintenanceTitle(item.Title),
            Subtitle = BuildMaintenanceSubtitle(item),
            DateLabel = FormatDateLabel(dueDate),
            SortDate = dueDate,
            IconClass = tone.icon,
            ToneClass = tone.tone,
            EditUrl = ResolveMaintenanceEditUrl(item, hvacRecords, waterHeaterRecords, hvacSolicitudes, url),
            EditLabel = "Edit"
        };
    }

    private static (string title, string subtitle) BuildProgramacionCopy(
        ProgramacionMicroservicio item,
        string microName,
        Dictionary<string, string> notes,
        Microservicio? trashMicro,
        Microservicio? lawnMicro,
        Microservicio? safeAirMicro)
    {
        if (trashMicro != null && item.MicroservicioId == trashMicro.Id)
        {
            var days = (item.FechaProgramada.Date - DateTime.Today).Days;
            var title = days switch
            {
                0 => "Trash pickup today",
                1 => "Trash out tonight",
                _ => "Trash pickup reminder"
            };

            var service = notes.GetValueOrDefault("Service");
            var reminder = notes.GetValueOrDefault("Reminder");
            var subtitleParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(service))
            {
                subtitleParts.Add(service);
            }

            subtitleParts.Add(string.IsNullOrWhiteSpace(reminder) ? "8:00 PM" : reminder);

            return (title, string.Join(" · ", subtitleParts.Where(p => !string.IsNullOrWhiteSpace(p))));
        }

        if (lawnMicro != null && item.MicroservicioId == lawnMicro.Id)
        {
            var frequency = notes.Values.FirstOrDefault(v =>
                v.Contains("subscription", StringComparison.OrdinalIgnoreCase)
                || v.Contains("Weekly", StringComparison.OrdinalIgnoreCase)
                || v.Contains("Biweekly", StringComparison.OrdinalIgnoreCase))
                ?? notes.Values.FirstOrDefault() ?? "Scheduled service";

            return ("Mow lawn", $"{frequency} · Next visit");
        }

        if (safeAirMicro != null && item.MicroservicioId == safeAirMicro.Id)
        {
            return ("Change HVAC filter", "Filter replacement · Next due");
        }

        if (notes.TryGetValue("Frequency", out var freq) && !string.IsNullOrWhiteSpace(freq))
        {
            return (microName, $"{freq} · Next due");
        }

        var fallbackSubtitle = string.IsNullOrWhiteSpace(item.Notas)
            ? "Scheduled reminder"
            : ShortenNotes(item.Notas);

        return (microName, fallbackSubtitle);
    }

    private static string BuildMaintenanceSubtitle(PropiedadMantenimiento item)
    {
        if (item.Title.Contains("HVAC filter", StringComparison.OrdinalIgnoreCase))
        {
            var freq = item.Notes?.Split('•', StringSplitOptions.TrimEntries)
                .FirstOrDefault(p => p.Contains("month", StringComparison.OrdinalIgnoreCase))
                ?? "Every 3 months";
            return $"{freq} · Next due";
        }

        if (item.Title.Contains("water heater flush", StringComparison.OrdinalIgnoreCase))
        {
            var repeats = item.Notes?.Contains("12 months", StringComparison.OrdinalIgnoreCase) == true
                ? "Every 12 months"
                : "Annual maintenance";
            return $"{repeats} · Next due";
        }

        return string.IsNullOrWhiteSpace(item.Notes) ? "Home maintenance" : item.Notes;
    }

    private static string FormatMaintenanceTitle(string title)
    {
        if (title.Contains("HVAC filter", StringComparison.OrdinalIgnoreCase))
        {
            return "Change HVAC filter";
        }

        if (title.Contains("water heater flush", StringComparison.OrdinalIgnoreCase))
        {
            return "Flush water heater";
        }

        return title;
    }

    private static string ResolveProgramacionEditUrl(
        ProgramacionMicroservicio item,
        Microservicio? trashMicro,
        Microservicio? lawnMicro,
        Microservicio? safeAirMicro,
        Microservicio? cleaningMicro,
        List<SolicitudTrash> trashSolicitudes,
        List<SolicitudLawn> lawnSolicitudes,
        List<SolicitudSafeAir> safeAirSolicitudes,
        List<SolicitudCleaningPro> cleaningSolicitudes,
        IUrlHelper url)
    {
        if (trashMicro != null && item.MicroservicioId == trashMicro.Id)
        {
            var solicitud = trashSolicitudes.FirstOrDefault(s => s.MicroservicioId == trashMicro.Id);
            if (solicitud != null)
            {
                var action = solicitud.Estado is "Submitted" or "HelpCompleted"
                    ? "TrashReview"
                    : "TrashSetup";
                return url.Action(action, "Trash", new { id = solicitud.Id }) ?? "#";
            }

            return url.Action("TrashService", "Trash", new { id = trashMicro.Id }) ?? "#";
        }

        if (lawnMicro != null && item.MicroservicioId == lawnMicro.Id)
        {
            var solicitud = lawnSolicitudes.FirstOrDefault(s => s.MicroservicioId == lawnMicro.Id);
            if (solicitud != null)
            {
                var action = solicitud.Estado is "Submitted" or "AddonsCompleted"
                    ? "LawnReview"
                    : "LawnSetup";
                return url.Action(action, "Lawn", new { id = solicitud.Id }) ?? "#";
            }

            return url.Action("LawnService", "Lawn", new { id = lawnMicro.Id }) ?? "#";
        }

        if (safeAirMicro != null && item.MicroservicioId == safeAirMicro.Id)
        {
            var solicitud = safeAirSolicitudes.FirstOrDefault(s => s.MicroservicioId == safeAirMicro.Id);
            if (solicitud != null)
            {
                return url.Action("SafeAirSchedule", "SafeAir", new { id = solicitud.Id }) ?? "#";
            }

            return url.Action("SafeAirService", "SafeAir", new { id = safeAirMicro.Id }) ?? "#";
        }

        if (cleaningMicro != null && item.MicroservicioId == cleaningMicro.Id)
        {
            var solicitud = cleaningSolicitudes.FirstOrDefault(s => s.MicroservicioId == cleaningMicro.Id);
            if (solicitud != null)
            {
                var action = solicitud.Estado is "Submitted" or "CustomizeCompleted" or "SetupCompleted"
                    ? "CleaningProReview"
                    : "CleaningProSetup";
                return url.Action(action, "CleaningPro", new { id = solicitud.Id }) ?? "#";
            }

            return url.Action("CleaningProService", "CleaningPro", new { id = cleaningMicro.Id }) ?? "#";
        }

        return url.Action("Schedule", "Microservicios", new { id = item.MicroservicioId }) ?? "#";
    }

    private static string ResolveMaintenanceEditUrl(
        PropiedadMantenimiento item,
        List<PropiedadHvacSistema> hvacRecords,
        List<PropiedadWaterHeaterSistema> waterHeaterRecords,
        List<SolicitudHvacMaintenance> hvacSolicitudes,
        IUrlHelper url)
    {
        if (string.Equals(item.Title, "HVAC Tune-Up", StringComparison.OrdinalIgnoreCase))
        {
            var solicitud = hvacSolicitudes
                .Where(s => s.PropiedadId == item.PropiedadId)
                .OrderByDescending(s => s.FechaConfirmacion ?? s.FechaActualizacion ?? s.FechaCreacion)
                .FirstOrDefault();
            if (solicitud != null)
            {
                return url.Action("HvacMaintenanceConfirmed", "HvacMaintenance", new { id = solicitud.Id }) ?? "#";
            }
        }

        if (item.Title.Contains("HVAC filter", StringComparison.OrdinalIgnoreCase))
        {
            var hvac = hvacRecords.FirstOrDefault(h => h.PropiedadId == item.PropiedadId);
            if (hvac?.FilterRemindersEnabled == true)
            {
                return url.Action("Notifications", "HvacFilterReplacement", new { id = item.PropiedadId }) ?? "#";
            }

            return url.Action("Pets", "HvacFilterReplacement", new { id = item.PropiedadId }) ?? "#";
        }

        if (item.Title.Contains("water heater flush", StringComparison.OrdinalIgnoreCase))
        {
            var record = waterHeaterRecords.FirstOrDefault(w => w.PropiedadId == item.PropiedadId);
            if (record?.FlushReminderSetupComplete == true)
            {
                return url.Action("Setup", "WaterHeaterFlushReminder", new { id = item.PropiedadId }) ?? "#";
            }

            return url.Action("Intro", "WaterHeaterFlushReminder", new { id = item.PropiedadId }) ?? "#";
        }

        return url.Action("MaintenanceEdit", "MyHome", new { id = item.Id }) ?? "#";
    }

    private static bool IsDuplicateMaintenance(ProgramacionMicroservicio programacion, PropiedadMantenimiento mantenimiento)
    {
        var microName = programacion.Microservicio?.Nombre ?? string.Empty;
        if (mantenimiento.Title.Contains("HVAC filter", StringComparison.OrdinalIgnoreCase)
            && (microName.Contains("Safe Air", StringComparison.OrdinalIgnoreCase)
                || microName.Contains("filter", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    private static Dictionary<string, string> ParseNotes(string? raw)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return result;
        }

        foreach (var segment in raw.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = segment.IndexOf(':');
            if (idx <= 0)
            {
                continue;
            }

            var key = segment[..idx].Trim();
            var value = segment[(idx + 1)..].Trim();
            result[key] = value;
        }

        return result;
    }

    private static string ShortenNotes(string notes) =>
        notes.Length <= 72 ? notes : notes[..69] + "…";

    private static string FormatDateLabel(DateTime date) =>
        date.ToString("ddd, MMM dd", CultureInfo.InvariantCulture);

    private static (string icon, string tone) ResolveTone(
        int microservicioId,
        string microName,
        Microservicio? trashMicro,
        Microservicio? lawnMicro,
        Microservicio? safeAirMicro)
    {
        if (trashMicro != null && microservicioId == trashMicro.Id)
        {
            return ("fa-trash-can", "sch-tone-trash");
        }

        if (lawnMicro != null && microservicioId == lawnMicro.Id)
        {
            return ("fa-seedling", "sch-tone-lawn");
        }

        if (safeAirMicro != null && microservicioId == safeAirMicro.Id)
        {
            return ("fa-snowflake", "sch-tone-hvac");
        }

        if (microName.Contains("clean", StringComparison.OrdinalIgnoreCase))
        {
            return ("fa-broom", "sch-tone-filter");
        }

        return ("fa-calendar-check", "sch-tone-general");
    }

    private static (string icon, string tone) ResolveMaintenanceTone(string title)
    {
        if (title.Contains("HVAC filter", StringComparison.OrdinalIgnoreCase)
            || title.Contains("hvac", StringComparison.OrdinalIgnoreCase))
        {
            return ("fa-snowflake", "sch-tone-hvac");
        }

        if (title.Contains("water heater", StringComparison.OrdinalIgnoreCase))
        {
            return ("fa-fire-flame-simple", "sch-tone-water");
        }

        if (title.Contains("lawn", StringComparison.OrdinalIgnoreCase)
            || title.Contains("mow", StringComparison.OrdinalIgnoreCase))
        {
            return ("fa-seedling", "sch-tone-lawn");
        }

        if (title.Contains("trash", StringComparison.OrdinalIgnoreCase))
        {
            return ("fa-trash-can", "sch-tone-trash");
        }

        if (title.Contains("gutter", StringComparison.OrdinalIgnoreCase))
        {
            return ("fa-house-chimney", "sch-tone-general");
        }

        return ("fa-wrench", "sch-tone-general");
    }

    private static Microservicio? FindMicro(IEnumerable<Microservicio> items, int id, string name) =>
        items.FirstOrDefault(m => m.Id == id)
        ?? items.FirstOrDefault(m => NameComparer.Equals(m.Nombre, name));
}
