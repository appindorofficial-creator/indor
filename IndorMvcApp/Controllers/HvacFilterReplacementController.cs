using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class HvacFilterReplacementController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HvacFilterReplacementController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Pets(int id)
    {
        var record = await LoadHvacAsync(id);
        if (record == null) return NotFound();

        return View(new HvacFilterPetsViewModel
        {
            PropiedadId = id,
            Step = 1,
            HasPets = record.HasPets,
            FilterSize = record.FilterSize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pets(HvacFilterPetsViewModel model)
    {
        var record = await LoadHvacAsync(model.PropiedadId);
        if (record == null) return NotFound();

        if (!model.HasPets.HasValue)
        {
            ModelState.AddModelError(nameof(model.HasPets), "Please select an option.");
            model.Step = 1;
            return View(model);
        }

        record.HasPets = model.HasPets.Value;
        record.FilterScheduleMode ??= HvacFilterReplacementService.DefaultModeForPets(model.HasPets.Value);
        record.FilterReminderDays = HvacFilterReplacementService.DaysForMode(record.FilterScheduleMode);
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Schedule), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Schedule(int id)
    {
        var record = await LoadHvacAsync(id);
        if (record == null) return NotFound();

        if (!record.HasPets.HasValue)
        {
            return RedirectToAction(nameof(Pets), new { id });
        }

        var hasPets = record.HasPets.Value;
        var mode = record.FilterScheduleMode
            ?? HvacFilterReplacementService.DefaultModeForPets(hasPets);

        return View(new HvacFilterScheduleViewModel
        {
            PropiedadId = id,
            Step = 2,
            HasPets = hasPets,
            ScheduleMode = mode,
            FilterSize = record.FilterSize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(HvacFilterScheduleViewModel model)
    {
        var record = await LoadHvacAsync(model.PropiedadId);
        if (record == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Step = 2;
            model.HasPets = record.HasPets ?? false;
            return View(model);
        }

        record.FilterScheduleMode = model.ScheduleMode;
        record.FilterReminderDays = model.ScheduleMode == HvacFilterReplacementService.ModeCustom
            ? HvacFilterReplacementService.DaysForMode(
                HvacFilterReplacementService.DefaultModeForPets(model.HasPets))
            : HvacFilterReplacementService.DaysForMode(model.ScheduleMode);
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ChooseDate), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> ChooseDate(int id)
    {
        var record = await LoadHvacAsync(id);
        if (record == null) return NotFound();

        if (!record.HasPets.HasValue)
        {
            return RedirectToAction(nameof(Pets), new { id });
        }

        if (string.IsNullOrWhiteSpace(record.FilterScheduleMode))
        {
            return RedirectToAction(nameof(Schedule), new { id });
        }

        var defaultDate = record.NextFilterChangeDate?.Date
            ?? HvacFilterReplacementService.ComputeNextDateFromToday(record);

        return View(new HvacFilterChooseDateViewModel
        {
            PropiedadId = id,
            Step = 3,
            HasPets = record.HasPets.Value,
            ScheduleMode = record.FilterScheduleMode,
            FrequencyLabel = HvacFilterReplacementService.FrequencyLabel(record.FilterScheduleMode),
            NextChangeDate = defaultDate,
            FilterSize = record.FilterSize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChooseDate(HvacFilterChooseDateViewModel model)
    {
        var record = await LoadHvacAsync(model.PropiedadId);
        if (record == null) return NotFound();

        if (!ModelState.IsValid || !model.NextChangeDate.HasValue)
        {
            model.Step = 3;
            model.HasPets = record.HasPets ?? false;
            model.ScheduleMode = record.FilterScheduleMode ?? HvacFilterReplacementService.ModeEvery3Months;
            model.FrequencyLabel = HvacFilterReplacementService.FrequencyLabel(model.ScheduleMode);
            if (!model.NextChangeDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.NextChangeDate), "Please pick a date.");
            }

            return View(model);
        }

        record.NextFilterChangeDate = model.NextChangeDate.Value.Date;
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Notifications), new { id = model.PropiedadId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlreadyChanged(int id)
    {
        var record = await LoadHvacAsync(id);
        if (record == null) return NotFound();

        record.LastServiceDate = DateTime.Today;
        record.NextFilterChangeDate = HvacFilterReplacementService.ComputeNextDateFromToday(record);
        record.FechaActualizacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Notifications), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Notifications(int id)
    {
        var record = await LoadHvacAsync(id);
        if (record == null) return NotFound();

        if (!record.NextFilterChangeDate.HasValue)
        {
            return RedirectToAction(nameof(ChooseDate), new { id });
        }

        var due = record.NextFilterChangeDate.Value;

        return View(new HvacFilterNotificationsViewModel
        {
            PropiedadId = id,
            Step = 4,
            HasPets = record.HasPets ?? false,
            FrequencyLabel = HvacFilterReplacementService.FrequencyLabel(record.FilterScheduleMode),
            NextChangeLabel = due.ToString("MMMM d, yyyy"),
            RemindOneWeekBefore = record.RemindOneWeekBefore,
            RemindOneDayBefore = record.RemindOneDayBefore,
            FilterNotificationsConsent = record.FilterNotificationsConsent || !record.FilterReminderSetupComplete,
            FilterSize = record.FilterSize
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notifications(HvacFilterNotificationsViewModel model)
    {
        var record = await LoadHvacAsync(model.PropiedadId);
        if (record == null) return NotFound();

        if (!model.RemindOneWeekBefore && !model.RemindOneDayBefore)
        {
            ModelState.AddModelError(string.Empty, "Select at least one reminder option.");
        }

        if (!model.FilterNotificationsConsent)
        {
            ModelState.AddModelError(nameof(model.FilterNotificationsConsent), "Please agree to receive reminder notifications.");
        }

        if (!ModelState.IsValid)
        {
            model.Step = 4;
            model.HasPets = record.HasPets ?? false;
            model.FrequencyLabel = HvacFilterReplacementService.FrequencyLabel(record.FilterScheduleMode);
            model.NextChangeLabel = record.NextFilterChangeDate?.ToString("MMMM d, yyyy") ?? "—";
            return View(model);
        }

        record.RemindOneWeekBefore = model.RemindOneWeekBefore;
        record.RemindOneDayBefore = model.RemindOneDayBefore;
        record.FilterNotificationsConsent = true;
        record.FilterRemindersEnabled = true;
        record.FilterReminderSetupComplete = true;
        record.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await HvacFilterReplacementService.UpsertFilterMaintenanceAsync(_db, record);
        await AddCompletionHistoryAsync(record);

        TempData["FilterReminderSaved"] = true;
        return RedirectToAction("Saved", "HvacSetup", new { id = model.PropiedadId });
    }

    private async Task AddCompletionHistoryAsync(PropiedadHvacSistema record)
    {
        var due = record.NextFilterChangeDate?.ToString("MMM d, yyyy") ?? "—";
        var reminders = HvacFilterReplacementService.ReminderSummary(
            record.RemindOneWeekBefore,
            record.RemindOneDayBefore);

        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = record.PropiedadId,
            RecordType = "Reminder",
            Title = "HVAC filter reminder configured",
            Description = $"Next change {due}. Reminders: {reminders}.",
            Source = "User",
            FechaCreacion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    private async Task<PropiedadHvacSistema?> LoadHvacAsync(int propiedadId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        var propiedad = await _db.Propiedades
            .FirstOrDefaultAsync(p => p.Id == propiedadId && p.UserId == userId && p.Activo);

        if (propiedad == null) return null;

        return await _db.PropiedadHvacSistemas
            .FirstOrDefaultAsync(h => h.PropiedadId == propiedadId);
    }
}
