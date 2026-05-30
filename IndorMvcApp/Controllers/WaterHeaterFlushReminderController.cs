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
public class WaterHeaterFlushReminderController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public WaterHeaterFlushReminderController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Intro(int id)
    {
        var record = await LoadWaterHeaterAsync(id);
        if (record == null) return NotFound();

        return View(new WaterHeaterFlushReminderIntroViewModel
        {
            PropiedadId = id,
            HeaterTypeLabel = WaterHeaterOpenAiHintsService.HeaterTypeLabel(record.HeaterType)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Setup(int id)
    {
        var record = await LoadWaterHeaterAsync(id);
        if (record == null) return NotFound();

        return View(BuildSetupModel(record));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Setup(WaterHeaterFlushReminderSetupViewModel model)
    {
        var record = await LoadWaterHeaterAsync(model.PropiedadId);
        if (record == null) return NotFound();

        if (!model.RemindOneWeekBefore && !model.RemindOneDayBefore)
        {
            ModelState.AddModelError(string.Empty, "Select at least one reminder option.");
        }

        if (!model.FlushNotificationsConsent)
        {
            ModelState.AddModelError(nameof(model.FlushNotificationsConsent),
                "Please agree to receive reminder notifications.");
        }

        if (!ModelState.IsValid || !model.NextFlushDate.HasValue)
        {
            if (!model.NextFlushDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.NextFlushDate), "Please pick a date.");
            }

            model.LocationOptions = WaterHeaterFlushReminderService.CommonLocations.ToList();
            return View(model);
        }

        record.NextFlushDate = model.NextFlushDate.Value.Date;
        record.FlushLocation = NullIfEmpty(model.FlushLocation) ?? "Basement";
        record.RemindOneWeekBefore = model.RemindOneWeekBefore;
        record.RemindOneDayBefore = model.RemindOneDayBefore;
        record.AutoRepeatEnabled = model.AutoRepeatEnabled;
        record.FlushNotificationsConsent = true;
        record.FlushRemindersEnabled = true;
        record.FlushReminderDays = WaterHeaterFlushReminderService.FlushIntervalDays;
        record.FlushReminderSetupComplete = true;
        record.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await WaterHeaterFlushReminderService.UpsertFlushMaintenanceAsync(_db, record);
        await AddCompletionHistoryAsync(record);

        return RedirectToAction(nameof(Saved), new { id = model.PropiedadId });
    }

    [HttpGet]
    public async Task<IActionResult> Saved(int id)
    {
        var record = await LoadWaterHeaterAsync(id);
        if (record == null) return NotFound();

        if (!record.FlushReminderSetupComplete || !record.NextFlushDate.HasValue)
        {
            return RedirectToAction(nameof(Intro), new { id });
        }

        return View(new WaterHeaterFlushReminderSavedViewModel
        {
            PropiedadId = id,
            LocationLabel = record.FlushLocation ?? "Basement",
            NextFlushLabel = record.NextFlushDate.Value.ToString("MMM d, yyyy"),
            RemindersLabel = WaterHeaterFlushReminderService.ReminderSummary(
                record.RemindOneWeekBefore,
                record.RemindOneDayBefore),
            AutoRepeatEnabled = record.AutoRepeatEnabled
        });
    }

    private WaterHeaterFlushReminderSetupViewModel BuildSetupModel(PropiedadWaterHeaterSistema record) =>
        new()
        {
            PropiedadId = record.PropiedadId,
            NextFlushDate = WaterHeaterFlushReminderService.DefaultNextFlushDate(record),
            FlushLocation = record.FlushLocation ?? "Basement",
            RemindOneWeekBefore = record.RemindOneWeekBefore,
            RemindOneDayBefore = record.RemindOneDayBefore,
            AutoRepeatEnabled = record.AutoRepeatEnabled,
            FlushNotificationsConsent = record.FlushNotificationsConsent || !record.FlushReminderSetupComplete,
            LocationOptions = WaterHeaterFlushReminderService.CommonLocations.ToList()
        };

    private async Task AddCompletionHistoryAsync(PropiedadWaterHeaterSistema record)
    {
        var due = record.NextFlushDate?.ToString("MMM d, yyyy") ?? "—";
        var reminders = WaterHeaterFlushReminderService.ReminderSummary(
            record.RemindOneWeekBefore,
            record.RemindOneDayBefore);

        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = record.PropiedadId,
            RecordType = "Reminder",
            Title = "Water heater flush reminder configured",
            Description = $"Next flush {due} at {record.FlushLocation ?? "Basement"}. Reminders: {reminders}.",
            Source = "User",
            FechaCreacion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    private async Task<PropiedadWaterHeaterSistema?> LoadWaterHeaterAsync(int propiedadId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        var propiedad = await _db.Propiedades
            .FirstOrDefaultAsync(p => p.Id == propiedadId && p.UserId == userId && p.Activo);

        if (propiedad == null) return null;

        return await PropiedadWaterHeaterQueryHelper.TryGetByPropiedadIdAsync(_db, propiedadId);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
