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
public class HvacSetupController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HvacSetupController(AppDbContext db, UserManager<ApplicationUser> userManager)
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
        var existing = await _db.PropiedadHvacSistemas.FirstOrDefaultAsync(h => h.PropiedadId == propiedad.Id);
        var hints = HvacOpenAiHintsService.Extract(propiedad, info);

        if (existing != null)
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        return View(BuildAddModel(propiedad, info, hints, null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddHvacSystemViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var propiedad = await LoadPropertyAsync(model.PropiedadId);
        if (propiedad == null) return NotFound();

        if (!ModelState.IsValid)
        {
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            var hints = HvacOpenAiHintsService.Extract(propiedad, info);
            return View(BuildAddModel(propiedad, info, hints, model));
        }

        var record = await _db.PropiedadHvacSistemas.FirstOrDefaultAsync(h => h.PropiedadId == propiedad.Id);
        if (record == null)
        {
            record = new PropiedadHvacSistema
            {
                PropiedadId = propiedad.Id,
                UserId = userId,
                FechaCreacion = DateTime.UtcNow
            };
            _db.PropiedadHvacSistemas.Add(record);
        }

        record.SystemType = model.SystemType;
        record.Brand = NullIfEmpty(model.Brand);
        record.Model = NullIfEmpty(model.Model);
        record.SerialNumber = NullIfEmpty(model.SerialNumber);
        record.InstallYear = model.InstallYear;
        record.FilterSize = NullIfEmpty(model.FilterSize);
        record.LastServiceDate = model.LastServiceDate?.Date;
        record.FilterRemindersEnabled = model.FilterRemindersEnabled;
        record.FilterReminderDays = 90;
        record.OpenAiDataSource = propiedad.AttomSyncStatus ?? "OpenAI House Fact";
        record.FechaActualizacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await UpsertFilterReminderAsync(propiedad.Id, record);
        await AddHistoryEntryAsync(propiedad, record);

        return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Saved(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var record = await _db.PropiedadHvacSistemas.FirstOrDefaultAsync(h => h.PropiedadId == propiedad.Id);
        if (record == null) return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });

        var dueDate = record.FilterRemindersEnabled
            ? (record.LastServiceDate?.Date ?? DateTime.Today).AddDays(record.FilterReminderDays)
            : (DateTime?)null;

        var dueLabel = dueDate.HasValue
            ? $"Due in {(dueDate.Value.Date - DateTime.Today).Days} days"
            : "Reminders off";

        return View(new HvacSavedViewModel
        {
            PropiedadId = propiedad.Id,
            SystemTypeLabel = HvacOpenAiHintsService.SystemTypeLabel(record.SystemType),
            Brand = record.Brand ?? "—",
            Model = record.Model ?? "—",
            InstallYearLabel = record.InstallYear?.ToString() ?? "—",
            FilterSize = record.FilterSize ?? "—",
            LastServiceLabel = record.LastServiceDate?.ToString("MMM d, yyyy") ?? "—",
            FilterRemindersEnabled = record.FilterRemindersEnabled,
            FilterReminderDays = record.FilterReminderDays,
            NextReminderDueLabel = dueLabel,
            HouseFactsUrl = Url.Action("Index", "Home") + "#section-myhome"
        });
    }

    private AddHvacSystemViewModel BuildAddModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        HvacOpenAiHints hints,
        AddHvacSystemViewModel? posted)
    {
        var address = !string.IsNullOrWhiteSpace(info?.FormattedAddress)
            ? info!.FormattedAddress!
            : (propiedad.Direccion ?? "—");

        return new AddHvacSystemViewModel
        {
            PropiedadId = propiedad.Id,
            Address = address,
            SystemType = posted?.SystemType ?? hints.SystemType ?? "CentralAC",
            Brand = posted?.Brand ?? hints.Brand,
            Model = posted?.Model ?? hints.Model,
            SerialNumber = posted?.SerialNumber ?? hints.SerialNumber,
            InstallYear = posted?.InstallYear ?? hints.InstallYear,
            FilterSize = posted?.FilterSize ?? hints.FilterSize,
            LastServiceDate = posted?.LastServiceDate ?? hints.LastServiceDate,
            FilterRemindersEnabled = posted?.FilterRemindersEnabled ?? true,
            InstallYearOptions = HvacOpenAiHintsService.BuildInstallYearOptions(),
            FilterSizeOptions = HvacOpenAiHintsService.CommonFilterSizes.ToList(),
            OpenAiHintNote = hints.HasAny
                ? "Suggested values come from your OpenAI House Fact profile. Confirm or edit before saving."
                : "Add the basics so INDOR can personalize maintenance reminders."
        };
    }

    private async Task UpsertFilterReminderAsync(int propiedadId, PropiedadHvacSistema record)
    {
        var existing = await _db.PropiedadMantenimiento
            .Where(m => m.PropiedadId == propiedadId && m.Title.Contains("HVAC filter"))
            .OrderByDescending(m => m.FechaCreacion)
            .FirstOrDefaultAsync();

        if (!record.FilterRemindersEnabled)
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
        var dueDate = baseDate.AddDays(record.FilterReminderDays);

        if (existing == null)
        {
            _db.PropiedadMantenimiento.Add(new PropiedadMantenimiento
            {
                PropiedadId = propiedadId,
                Title = "HVAC filter replacement",
                DueDate = dueDate,
                Status = "Upcoming",
                Notes = "Living Room",
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

    private async Task AddHistoryEntryAsync(Propiedad propiedad, PropiedadHvacSistema record)
    {
        var brandModel = string.Join(" ", new[] { record.Brand, record.Model }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var description = string.IsNullOrWhiteSpace(brandModel)
            ? $"{HvacOpenAiHintsService.SystemTypeLabel(record.SystemType)} added to your home."
            : $"{brandModel.Trim()} added to your home.";

        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = propiedad.Id,
            RecordType = "System",
            Title = "HVAC system added",
            Description = description,
            CompletionDate = DateTime.UtcNow,
            Source = "User",
            FechaCreacion = DateTime.UtcNow
        });

        if (record.FilterRemindersEnabled)
        {
            var dueDate = (record.LastServiceDate?.Date ?? DateTime.Today).AddDays(record.FilterReminderDays);
            _db.PropiedadHistorial.Add(new PropiedadHistorial
            {
                PropiedadId = propiedad.Id,
                RecordType = "Reminder",
                Title = "HVAC filter reminder created",
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
