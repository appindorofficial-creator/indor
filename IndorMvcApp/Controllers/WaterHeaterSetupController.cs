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
public class WaterHeaterSetupController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public WaterHeaterSetupController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Add(int? propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return RedirectToAction("AddProperty", "Propietario");

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var existing = await PropiedadWaterHeaterQueryHelper.TryGetByPropiedadIdAsync(_db, propiedad.Id);
        var hints = WaterHeaterOpenAiHintsService.Extract(propiedad, info);

        if (existing != null)
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        return View(BuildAddModel(propiedad, info, hints, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddWaterHeaterViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var propiedad = await LoadPropertyAsync(model.PropiedadId);
        if (propiedad == null) return NotFound();

        if (!ModelState.IsValid)
        {
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            var hints = WaterHeaterOpenAiHintsService.Extract(propiedad, info);
            return View(BuildAddModel(propiedad, info, hints, model));
        }

        PropiedadWaterHeaterSistema? record;
        try
        {
            record = await _db.PropiedadWaterHeaterSistemas
                .FirstOrDefaultAsync(h => h.PropiedadId == propiedad.Id);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            ModelState.AddModelError(string.Empty, "Water heater storage is not available yet. Run CreatePropiedadWaterHeaterTables.sql on the database.");
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            var hints = WaterHeaterOpenAiHintsService.Extract(propiedad, info);
            return View(BuildAddModel(propiedad, info, hints, model));
        }

        if (record == null)
        {
            record = new PropiedadWaterHeaterSistema
            {
                PropiedadId = propiedad.Id,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };
            _db.PropiedadWaterHeaterSistemas.Add(record);
        }

        record.HeaterType = model.HeaterType;
        record.Brand = NullIfEmpty(model.Brand);
        record.Model = NullIfEmpty(model.Model);
        record.SerialNumber = NullIfEmpty(model.SerialNumber);
        record.InstallYear = model.InstallYear;
        record.TankSize = string.Equals(model.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
            ? null
            : NullIfEmpty(model.TankSize);
        record.LastServiceDate = model.LastServiceDate?.Date;
        record.FlushRemindersEnabled = model.FlushRemindersEnabled;
        record.FlushReminderDays = 365;
        record.OpenAiDataSource = propiedad.AttomSyncStatus ?? "OpenAI House Fact";
        record.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await UpsertFlushReminderAsync(propiedad.Id, record);
        await AddHistoryEntryAsync(propiedad, record);

        return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Saved(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var record = await PropiedadWaterHeaterQueryHelper.TryGetByPropiedadIdAsync(_db, propiedad.Id);
        if (record == null) return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });

        var dueDate = record.FlushRemindersEnabled
            ? (record.LastServiceDate?.Date ?? DateTime.Today).AddDays(record.FlushReminderDays)
            : (DateTime?)null;

        var dueDays = dueDate.HasValue ? (dueDate.Value.Date - DateTime.Today).Days : 0;
        var dueLabel = dueDate.HasValue
            ? dueDays >= 330 ? "Due in 12 months" : $"Due in {Math.Max(dueDays, 1)} days"
            : "Reminders off";

        return View(new WaterHeaterSavedViewModel
        {
            PropiedadId = propiedad.Id,
            HeaterTypeLabel = WaterHeaterOpenAiHintsService.HeaterTypeLabel(record.HeaterType),
            Brand = record.Brand ?? "—",
            Model = record.Model ?? "—",
            InstallYearLabel = record.InstallYear?.ToString() ?? "—",
            TankSizeLabel = string.Equals(record.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
                ? "—"
                : record.TankSize ?? "—",
            LastServiceLabel = record.LastServiceDate?.ToString("MMM d, yyyy") ?? "—",
            FlushRemindersEnabled = record.FlushRemindersEnabled,
            FlushReminderMonths = 12,
            NextReminderDueLabel = dueLabel,
            HouseFactsUrl = Url.Action("Index", "Home") + "#section-myhome"
        });
    }

    private AddWaterHeaterViewModel BuildAddModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        WaterHeaterOpenAiHints hints,
        AddWaterHeaterViewModel? posted)
    {
        var address = !string.IsNullOrWhiteSpace(info?.FormattedAddress)
            ? info!.FormattedAddress!
            : (propiedad.Direccion ?? "—");

        return new AddWaterHeaterViewModel
        {
            PropiedadId = propiedad.Id,
            Address = address,
            HeaterType = posted?.HeaterType ?? hints.HeaterType ?? "Tank",
            Brand = posted?.Brand ?? hints.Brand,
            Model = posted?.Model ?? hints.Model,
            SerialNumber = posted?.SerialNumber ?? hints.SerialNumber,
            InstallYear = posted?.InstallYear ?? hints.InstallYear,
            TankSize = posted?.TankSize ?? hints.TankSize,
            LastServiceDate = posted?.LastServiceDate ?? hints.LastServiceDate,
            FlushRemindersEnabled = posted?.FlushRemindersEnabled ?? true,
            InstallYearOptions = WaterHeaterOpenAiHintsService.BuildInstallYearOptions(),
            TankSizeOptions = WaterHeaterOpenAiHintsService.CommonTankSizes.ToList(),
            OpenAiHintNote = hints.HasAny
                ? "Suggested values come from your OpenAI House Fact profile. Confirm or edit before saving."
                : "Add the basics so INDOR can personalize reminders and keep home records updated."
        };
    }

    private async Task UpsertFlushReminderAsync(int propiedadId, PropiedadWaterHeaterSistema record)
    {
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == propiedadId && m.Title.Contains("water heater flush", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        if (!record.FlushRemindersEnabled)
        {
            if (existing != null)
            {
                existing.Status = "Completed";
                existing.FechaActualizacion = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return;
        }

        var baseDate = record.LastServiceDate?.Date ?? DateTime.Today;
        var dueDate = baseDate.AddDays(record.FlushReminderDays);

        if (existing == null)
        {
            _db.PropiedadMantenimiento.Add(new PropiedadMantenimiento
            {
                PropiedadId = propiedadId,
                Title = "Annual water heater flush",
                DueDate = dueDate,
                Status = "Upcoming",
                Notes = "Utility Room",
                FechaCreacion = DateTime.UtcNow
            });
        }
        else
        {
            existing.DueDate = dueDate;
            existing.Status = "Upcoming";
            existing.FechaActualizacion = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    private async Task AddHistoryEntryAsync(Propiedad propiedad, PropiedadWaterHeaterSistema record)
    {
        var brandModel = string.Join(" ", new[] { record.Brand, record.Model }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var description = string.IsNullOrWhiteSpace(brandModel)
            ? $"{WaterHeaterOpenAiHintsService.HeaterTypeLabel(record.HeaterType)} water heater added to your home."
            : $"{brandModel.Trim()} added to your home.";

        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = propiedad.Id,
            RecordType = "System",
            Title = "Water heater added",
            Description = description,
            CompletionDate = DateTime.UtcNow,
            Source = "User",
            FechaCreacion = DateTime.UtcNow
        });

        if (record.FlushRemindersEnabled)
        {
            var dueDate = (record.LastServiceDate?.Date ?? DateTime.Today).AddDays(record.FlushReminderDays);
            _db.PropiedadHistorial.Add(new PropiedadHistorial
            {
                PropiedadId = propiedad.Id,
                RecordType = "Reminder",
                Title = "Water heater flush reminder created",
                Description = $"Next reminder set for {dueDate:MMM d, yyyy}.",
                Source = "User",
                FechaCreacion = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task<Propiedad?> LoadPropertyAsync(int? propiedadId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        if (propiedadId.HasValue)
        {
            return await _db.Propiedades
                .FirstOrDefaultAsync(p => p.Id == propiedadId.Value && p.UserId == userId && p.Activo);
        }

        return await _db.Propiedades
            .Where(p => p.UserId == userId && p.Activo)
            .OrderByDescending(p => p.FechaCreacion)
            .FirstOrDefaultAsync();
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
