using System.Text.Json;
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
    private const string DraftSessionKey = "WaterHeaterSetupDraft";
    private static readonly string[] AllowedLabelExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxLabelBytes = 8 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public WaterHeaterSetupController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Add(int? propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return RedirectToAction("AddProperty", "Propietario");

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        ClearDraft();
        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(BuildStartModel(propiedad, info));
    }

    [HttpGet]
    public async Task<IActionResult> ScanLabel(int propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        var draft = GetDraft() ?? new WaterHeaterSetupDraft { PropiedadId = propiedad.Id };
        draft.PropiedadId = propiedad.Id;
        draft.EntryMode = "scan";
        SaveDraft(draft);

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(new WaterHeaterScanLabelViewModel
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 2,
            PageTitle = "Scan Water Heater Label",
            Address = FormatAddress(propiedad, info),
            ImageUrl = "/welcome-house.png",
            BackUrl = Url.Action(nameof(Add), new { propiedadId = propiedad.Id }) ?? "/",
            ScanHint = "Center the label inside the frame. Make sure the brand, model, and serial number are visible.",
            PreviewImageUrl = draft.LabelImagePath
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ScanLabel(int propiedadId, IFormFile? labelFile)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var hints = WaterHeaterOpenAiHintsService.Extract(propiedad, info);
        var draft = GetDraft() ?? new WaterHeaterSetupDraft { PropiedadId = propiedad.Id, EntryMode = "scan" };

        if (labelFile is { Length: > 0 })
        {
            var savedPath = await SaveLabelImageAsync(labelFile, propiedad.Id);
            if (savedPath != null)
            {
                draft.LabelImagePath = savedPath;
            }
        }

        ApplyHintsToDraft(draft, hints);
        SaveDraft(draft);

        return RedirectToAction(nameof(DetailsFound), new { propiedadId = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> DetailsFound(int propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        var draft = GetDraft();
        if (draft == null || draft.PropiedadId != propiedad.Id || draft.EntryMode != "scan")
        {
            return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(BuildDetailsFoundModel(propiedad, info, draft));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DetailsFound(int propiedadId, string? action)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return NotFound();

        if (string.Equals(action, "rescan", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ScanLabel), new { propiedadId = propiedad.Id });
        }

        return RedirectToAction(nameof(Review), new { propiedadId = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Manual(int propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var hints = WaterHeaterOpenAiHintsService.Extract(propiedad, info);
        var draft = GetDraft() ?? new WaterHeaterSetupDraft { PropiedadId = propiedad.Id };
        draft.PropiedadId = propiedad.Id;
        draft.EntryMode = "manual";
        if (!HasDraftValues(draft))
        {
            ApplyHintsToDraft(draft, hints);
        }

        SaveDraft(draft);
        return View(BuildManualModel(propiedad, info, draft));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manual(AddWaterHeaterViewModel model)
    {
        var propiedad = await LoadPropertyAsync(model.PropiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        if (!ModelState.IsValid)
        {
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            return View(BuildManualModel(propiedad, info, model));
        }

        var draft = GetDraft() ?? new WaterHeaterSetupDraft { PropiedadId = propiedad.Id };
        draft.PropiedadId = propiedad.Id;
        draft.EntryMode = "manual";
        draft.HeaterType = model.HeaterType;
        draft.Brand = NullIfEmpty(model.Brand);
        draft.Model = NullIfEmpty(model.Model);
        draft.SerialNumber = NullIfEmpty(model.SerialNumber);
        draft.InstallYear = model.InstallYear;
        draft.TankSize = string.Equals(model.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
            ? null
            : NullIfEmpty(model.TankSize);
        draft.LastServiceDate = model.LastServiceDate?.Date;
        draft.FlushRemindersEnabled = model.FlushRemindersEnabled;
        SaveDraft(draft);

        return RedirectToAction(nameof(Review), new { propiedadId = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Review(int propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        var draft = GetDraft();
        if (draft == null || draft.PropiedadId != propiedad.Id)
        {
            return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });
        }

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(BuildReviewModel(propiedad, info, draft));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(WaterHeaterReviewViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var propiedad = await LoadPropertyAsync(model.PropiedadId);
        if (propiedad == null) return NotFound();

        if (await HasSavedSystemAsync(propiedad.Id))
        {
            return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
        }

        var draft = GetDraft();
        if (draft == null || draft.PropiedadId != propiedad.Id)
        {
            return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });
        }

        if (!model.ConfirmSave)
        {
            ModelState.AddModelError(nameof(model.ConfirmSave), "Please confirm before saving.");
        }

        if (!ModelState.IsValid)
        {
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            return View(BuildReviewModel(propiedad, info, draft, model));
        }

        PropiedadWaterHeaterSistema record;
        try
        {
            record = new PropiedadWaterHeaterSistema
            {
                PropiedadId = propiedad.Id,
                UserId = userId,
                HeaterType = draft.HeaterType,
                Brand = draft.Brand,
                Model = draft.Model,
                SerialNumber = draft.SerialNumber,
                InstallYear = draft.InstallYear,
                TankSize = string.Equals(draft.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : draft.TankSize,
                LastServiceDate = draft.LastServiceDate,
                FlushRemindersEnabled = draft.FlushRemindersEnabled,
                FlushReminderDays = 365,
                OpenAiDataSource = propiedad.AttomSyncStatus ?? "OpenAI House Fact",
                LabelImagePath = draft.LabelImagePath,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = DateTime.UtcNow
            };

            _db.PropiedadWaterHeaterSistemas.Add(record);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            ModelState.AddModelError(string.Empty, "Water heater storage is not available yet. Run CreatePropiedadWaterHeaterTables.sql on the database.");
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            return View(BuildReviewModel(propiedad, info, draft, model));
        }

        await AddHistoryEntryAsync(propiedad, record);
        ClearDraft();

        return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Saved(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var record = await PropiedadWaterHeaterQueryHelper.TryGetByPropiedadIdAsync(_db, propiedad.Id);
        if (record == null) return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var estimatedAge = record.InstallYear.HasValue
            ? $"{Math.Max(0, DateTime.Today.Year - record.InstallYear.Value)} years"
            : "—";

        return View(new WaterHeaterSavedViewModel
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 5,
            PageTitle = "Water Heater Added",
            Address = FormatAddress(propiedad, info),
            ImageUrl = "/welcome-house.png",
            BackUrl = Url.Action("Index", "Home") ?? "/",
            HeaterTypeLabel = WaterHeaterOpenAiHintsService.HeaterTypeLabel(record.HeaterType),
            Brand = record.Brand ?? "—",
            Model = record.Model ?? "—",
            SerialNumber = record.SerialNumber ?? "—",
            InstallYearLabel = record.InstallYear?.ToString() ?? "—",
            TankSizeLabel = string.Equals(record.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
                ? "—"
                : record.TankSize ?? "—",
            EstimatedAgeLabel = estimatedAge,
            EquipmentImageUrl = record.LabelImagePath,
            FlushRemindersEnabled = record.FlushRemindersEnabled,
            FlushReminderSetupComplete = record.FlushReminderSetupComplete,
            HouseFactsUrl = Url.Action("Index", "Home") + "#section-myhome",
            MyHomeUrl = Url.Action("Index", "Home") ?? "/"
        });
    }

    private WaterHeaterSetupStartViewModel BuildStartModel(Propiedad propiedad, PropertyInfoViewModel? info) =>
        new()
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 1,
            PageTitle = "Scan Water Heater Label",
            Address = FormatAddress(propiedad, info),
            ImageUrl = "/welcome-house.png",
            BackUrl = Url.Action("Index", "Home") ?? "/"
        };

    private AddWaterHeaterViewModel BuildManualModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        WaterHeaterSetupDraft draft) =>
        BuildManualModel(propiedad, info, new AddWaterHeaterViewModel
        {
            PropiedadId = draft.PropiedadId,
            HeaterType = draft.HeaterType,
            Brand = draft.Brand,
            Model = draft.Model,
            SerialNumber = draft.SerialNumber,
            InstallYear = draft.InstallYear,
            TankSize = draft.TankSize,
            LastServiceDate = draft.LastServiceDate,
            FlushRemindersEnabled = draft.FlushRemindersEnabled
        });

    private AddWaterHeaterViewModel BuildManualModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        AddWaterHeaterViewModel posted)
    {
        posted.CurrentStep = 3;
        posted.PageTitle = "Enter water heater details";
        posted.Address = FormatAddress(propiedad, info);
        posted.ImageUrl = "/welcome-house.png";
        posted.BackUrl = Url.Action(nameof(Add), new { propiedadId = propiedad.Id }) ?? "/";
        posted.InstallYearOptions = WaterHeaterOpenAiHintsService.BuildInstallYearOptions();
        posted.TankSizeOptions = WaterHeaterOpenAiHintsService.CommonTankSizes.ToList();
        posted.OpenAiHintNote = "Confirm or edit the details before continuing.";
        return posted;
    }

    private WaterHeaterDetailsFoundViewModel BuildDetailsFoundModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        WaterHeaterSetupDraft draft)
    {
        var estimatedAge = draft.InstallYear.HasValue
            ? $"{Math.Max(0, DateTime.Today.Year - draft.InstallYear.Value)} years"
            : null;

        return new WaterHeaterDetailsFoundViewModel
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 3,
            PageTitle = "Water Heater Details Found",
            Address = FormatAddress(propiedad, info),
            ImageUrl = "/welcome-house.png",
            BackUrl = Url.Action(nameof(ScanLabel), new { propiedadId = propiedad.Id }) ?? "/",
            HeaterTypeLabel = WaterHeaterOpenAiHintsService.HeaterTypeLabel(draft.HeaterType),
            Brand = string.IsNullOrWhiteSpace(draft.Brand) ? "—" : draft.Brand,
            Model = string.IsNullOrWhiteSpace(draft.Model) ? "—" : draft.Model,
            SerialNumber = string.IsNullOrWhiteSpace(draft.SerialNumber) ? "—" : draft.SerialNumber,
            InstallYearLabel = draft.InstallYear?.ToString() ?? "—",
            TankSizeLabel = string.Equals(draft.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
                ? "—"
                : (string.IsNullOrWhiteSpace(draft.TankSize) ? "—" : draft.TankSize),
            EstimatedAgeLabel = estimatedAge,
            LabelImageUrl = draft.LabelImagePath
        };
    }

    private WaterHeaterReviewViewModel BuildReviewModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        WaterHeaterSetupDraft draft,
        WaterHeaterReviewViewModel? posted = null) =>
        new()
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 4,
            PageTitle = "Review & Save",
            Address = FormatAddress(propiedad, info),
            ImageUrl = "/welcome-house.png",
            BackUrl = draft.EntryMode == "scan"
                ? Url.Action(nameof(DetailsFound), new { propiedadId = propiedad.Id }) ?? "/"
                : Url.Action(nameof(Manual), new { propiedadId = propiedad.Id }) ?? "/",
            HeaterType = draft.HeaterType,
            HeaterTypeLabel = WaterHeaterOpenAiHintsService.HeaterTypeLabel(draft.HeaterType),
            Brand = string.IsNullOrWhiteSpace(draft.Brand) ? "—" : draft.Brand,
            Model = string.IsNullOrWhiteSpace(draft.Model) ? "—" : draft.Model,
            SerialNumber = string.IsNullOrWhiteSpace(draft.SerialNumber) ? "—" : draft.SerialNumber,
            InstallYear = draft.InstallYear,
            InstallYearLabel = draft.InstallYear?.ToString() ?? "—",
            TankSize = draft.TankSize,
            TankSizeLabel = string.Equals(draft.HeaterType, "Tankless", StringComparison.OrdinalIgnoreCase)
                ? "—"
                : (string.IsNullOrWhiteSpace(draft.TankSize) ? "—" : draft.TankSize),
            LabelImageUrl = draft.LabelImagePath,
            ConfirmSave = posted?.ConfirmSave ?? false
        };

    private static void ApplyHintsToDraft(WaterHeaterSetupDraft draft, WaterHeaterOpenAiHints hints)
    {
        draft.HeaterType = hints.HeaterType ?? draft.HeaterType ?? "Tank";
        draft.Brand ??= hints.Brand;
        draft.Model ??= hints.Model;
        draft.SerialNumber ??= hints.SerialNumber;
        draft.InstallYear ??= hints.InstallYear;
        draft.TankSize ??= hints.TankSize;
        draft.LastServiceDate ??= hints.LastServiceDate;
    }

    private static bool HasDraftValues(WaterHeaterSetupDraft draft) =>
        !string.IsNullOrWhiteSpace(draft.Brand)
        || !string.IsNullOrWhiteSpace(draft.Model)
        || !string.IsNullOrWhiteSpace(draft.SerialNumber)
        || draft.InstallYear.HasValue;

    private WaterHeaterSetupDraft? GetDraft()
    {
        var json = HttpContext.Session.GetString(DraftSessionKey);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<WaterHeaterSetupDraft>(json);
        }
        catch
        {
            return null;
        }
    }

    private void SaveDraft(WaterHeaterSetupDraft draft) =>
        HttpContext.Session.SetString(DraftSessionKey, JsonSerializer.Serialize(draft));

    private void ClearDraft() => HttpContext.Session.Remove(DraftSessionKey);

    private async Task<bool> HasSavedSystemAsync(int propiedadId)
    {
        try
        {
            return await _db.PropiedadWaterHeaterSistemas.AnyAsync(h => h.PropiedadId == propiedadId);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return false;
        }
    }

    private async Task<string?> SaveLabelImageAsync(IFormFile file, int propiedadId)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedLabelExtensions.Contains(extension) || file.Length > MaxLabelBytes)
        {
            return null;
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "water-heater-labels");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"label-{propiedadId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/uploads/water-heater-labels/{fileName}";
    }

    private static string FormatAddress(Propiedad propiedad, PropertyInfoViewModel? info) =>
        !string.IsNullOrWhiteSpace(info?.FormattedAddress)
            ? info!.FormattedAddress!
            : (propiedad.Direccion ?? "—");

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
