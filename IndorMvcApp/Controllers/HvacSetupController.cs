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
public class HvacSetupController : Controller
{
    private const string DraftSessionKey = "HvacSetupDraft";
    private static readonly string[] AllowedLabelExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxLabelBytes = 8 * 1024 * 1024;

    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public HvacSetupController(
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

        var existing = await _db.PropiedadHvacSistemas.FirstOrDefaultAsync(h => h.PropiedadId == propiedad.Id);
        if (existing != null)
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

        var draft = GetDraft() ?? new HvacSetupDraft { PropiedadId = propiedad.Id };
        draft.PropiedadId = propiedad.Id;
        draft.EntryMode = "scan";
        SaveDraft(draft);

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return View(new HvacScanLabelViewModel
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 2,
            PageTitle = "Scan HVAC Label",
            Address = FormatAddress(propiedad, info),
            ImageUrl = ResolvePropertyImage(propiedad, info),
            BackUrl = Url.Action(nameof(Add), new { propiedadId = propiedad.Id }) ?? "/",
            ScanHint = "We'll scan the manufacturer label to fill in your system details automatically.",
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
        var hints = HvacOpenAiHintsService.Extract(propiedad, info);
        var draft = GetDraft() ?? new HvacSetupDraft { PropiedadId = propiedad.Id, EntryMode = "scan" };

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
        var hints = HvacOpenAiHintsService.Extract(propiedad, info);
        var draft = GetDraft() ?? new HvacSetupDraft { PropiedadId = propiedad.Id };
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
    public async Task<IActionResult> Manual(AddHvacSystemViewModel model)
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

        var draft = GetDraft() ?? new HvacSetupDraft { PropiedadId = propiedad.Id };
        draft.PropiedadId = propiedad.Id;
        draft.EntryMode = "manual";
        draft.SystemType = model.SystemType;
        draft.Brand = NullIfEmpty(model.Brand);
        draft.Model = NullIfEmpty(model.Model);
        draft.SerialNumber = NullIfEmpty(model.SerialNumber);
        draft.InstallYear = model.InstallYear;
        draft.FilterSize = NullIfEmpty(model.FilterSize);
        draft.LastServiceDate = model.LastServiceDate?.Date;
        draft.FilterRemindersEnabled = model.FilterRemindersEnabled;
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
    public async Task<IActionResult> Review(HvacReviewViewModel model)
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

        if (!model.ConfirmInfo || !model.AuthorizeStorage)
        {
            if (!model.ConfirmInfo)
            {
                ModelState.AddModelError(nameof(model.ConfirmInfo), "Please confirm your equipment information.");
            }

            if (!model.AuthorizeStorage)
            {
                ModelState.AddModelError(nameof(model.AuthorizeStorage), "Please authorize INDOR to store this equipment data.");
            }
        }

        if (!ModelState.IsValid)
        {
            var info = MyHomeDisplayService.DeserializeProperty(propiedad);
            return View(BuildReviewModel(propiedad, info, draft, model));
        }

        var record = new PropiedadHvacSistema
        {
            PropiedadId = propiedad.Id,
            UserId = userId,
            SystemType = draft.SystemType,
            Brand = draft.Brand,
            Model = draft.Model,
            SerialNumber = draft.SerialNumber,
            InstallYear = draft.InstallYear,
            FilterSize = draft.FilterSize,
            LastServiceDate = draft.LastServiceDate,
            FilterRemindersEnabled = draft.FilterRemindersEnabled,
            FilterReminderDays = 90,
            OpenAiDataSource = propiedad.AttomSyncStatus ?? "OpenAI House Fact",
            LabelImagePath = draft.LabelImagePath,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        _db.PropiedadHvacSistemas.Add(record);
        await _db.SaveChangesAsync();
        await AddHistoryEntryAsync(propiedad, record);
        ClearDraft();

        return RedirectToAction(nameof(Saved), new { id = propiedad.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Saved(int id)
    {
        var propiedad = await LoadPropertyAsync(id);
        if (propiedad == null) return NotFound();

        var record = await _db.PropiedadHvacSistemas.FirstOrDefaultAsync(h => h.PropiedadId == propiedad.Id);
        if (record == null) return RedirectToAction(nameof(Add), new { propiedadId = propiedad.Id });

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        var estimatedAge = record.InstallYear.HasValue
            ? $"{Math.Max(0, DateTime.Today.Year - record.InstallYear.Value)} years"
            : "—";

        return View(new HvacSavedViewModel
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 5,
            PageTitle = "HVAC system added",
            Address = FormatAddress(propiedad, info),
            ImageUrl = ResolvePropertyImage(propiedad, info),
            BackUrl = Url.Action("Index", "Home") ?? "/",
            SystemTypeLabel = HvacOpenAiHintsService.SystemTypeLabel(record.SystemType),
            Brand = record.Brand ?? "—",
            Model = record.Model ?? "—",
            SerialNumber = record.SerialNumber ?? "—",
            InstallYearLabel = record.InstallYear?.ToString() ?? "—",
            EstimatedAgeLabel = estimatedAge,
            EquipmentImageUrl = record.LabelImagePath,
            FilterSize = record.FilterSize ?? "—",
            LastServiceLabel = record.LastServiceDate?.ToString("MMM d, yyyy") ?? "—",
            FilterRemindersEnabled = record.FilterRemindersEnabled,
            FilterReminderDays = record.FilterReminderDays,
            FilterReminderSetupComplete = record.FilterReminderSetupComplete,
            HouseFactsUrl = Url.Action("Index", "Home") + "#section-myhome",
            MyHomeUrl = Url.Action("Index", "Home") ?? "/",
            AddAnotherUrl = Url.Action(nameof(Add), new { propiedadId = propiedad.Id }) ?? "#"
        });
    }

    private HvacSetupStartViewModel BuildStartModel(Propiedad propiedad, PropertyInfoViewModel? info) =>
        new()
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 1,
            PageTitle = "Add HVAC System",
            Address = FormatAddress(propiedad, info),
            ImageUrl = ResolvePropertyImage(propiedad, info),
            BackUrl = Url.Action("Index", "Home") ?? "/"
        };

    private AddHvacSystemViewModel BuildManualModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        HvacSetupDraft draft) =>
        BuildManualModel(propiedad, info, new AddHvacSystemViewModel
        {
            PropiedadId = draft.PropiedadId,
            SystemType = draft.SystemType,
            Brand = draft.Brand,
            Model = draft.Model,
            SerialNumber = draft.SerialNumber,
            InstallYear = draft.InstallYear,
            FilterSize = draft.FilterSize,
            LastServiceDate = draft.LastServiceDate,
            FilterRemindersEnabled = draft.FilterRemindersEnabled
        });

    private AddHvacSystemViewModel BuildManualModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        AddHvacSystemViewModel posted)
    {
        posted.CurrentStep = 3;
        posted.PageTitle = "Enter system details";
        posted.Address = FormatAddress(propiedad, info);
        posted.ImageUrl = ResolvePropertyImage(propiedad, info);
        posted.BackUrl = Url.Action(nameof(Add), new { propiedadId = propiedad.Id }) ?? "/";
        posted.InstallYearOptions = HvacOpenAiHintsService.BuildInstallYearOptions();
        posted.FilterSizeOptions = HvacOpenAiHintsService.CommonFilterSizes.ToList();
        posted.OpenAiHintNote = "Confirm or edit the details before continuing.";
        return posted;
    }

    private HvacReviewViewModel BuildReviewModel(
        Propiedad propiedad,
        PropertyInfoViewModel? info,
        HvacSetupDraft draft,
        HvacReviewViewModel? posted = null) =>
        new()
        {
            PropiedadId = propiedad.Id,
            CurrentStep = 4,
            PageTitle = "Review & consent",
            Address = FormatAddress(propiedad, info),
            ImageUrl = ResolvePropertyImage(propiedad, info),
            BackUrl = draft.EntryMode == "scan"
                ? Url.Action(nameof(ScanLabel), new { propiedadId = propiedad.Id }) ?? "/"
                : Url.Action(nameof(Manual), new { propiedadId = propiedad.Id }) ?? "/",
            SystemType = draft.SystemType,
            SystemTypeLabel = HvacOpenAiHintsService.SystemTypeLabel(draft.SystemType),
            Brand = string.IsNullOrWhiteSpace(draft.Brand) ? "—" : draft.Brand,
            Model = string.IsNullOrWhiteSpace(draft.Model) ? "—" : draft.Model,
            SerialNumber = string.IsNullOrWhiteSpace(draft.SerialNumber) ? "—" : draft.SerialNumber,
            InstallYear = draft.InstallYear,
            InstallYearLabel = draft.InstallYear?.ToString() ?? "—",
            ConfirmInfo = posted?.ConfirmInfo ?? false,
            AuthorizeStorage = posted?.AuthorizeStorage ?? false,
            LabelImageUrl = draft.LabelImagePath
        };

    private static void ApplyHintsToDraft(HvacSetupDraft draft, HvacOpenAiHints hints)
    {
        draft.SystemType = hints.SystemType ?? draft.SystemType ?? "CentralAC";
        draft.Brand ??= hints.Brand;
        draft.Model ??= hints.Model;
        draft.SerialNumber ??= hints.SerialNumber;
        draft.InstallYear ??= hints.InstallYear;
        draft.FilterSize ??= hints.FilterSize;
        draft.LastServiceDate ??= hints.LastServiceDate;
    }

    private static bool HasDraftValues(HvacSetupDraft draft) =>
        !string.IsNullOrWhiteSpace(draft.Brand)
        || !string.IsNullOrWhiteSpace(draft.Model)
        || !string.IsNullOrWhiteSpace(draft.SerialNumber)
        || draft.InstallYear.HasValue;

    private HvacSetupDraft? GetDraft()
    {
        var json = HttpContext.Session.GetString(DraftSessionKey);
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<HvacSetupDraft>(json);
        }
        catch
        {
            return null;
        }
    }

    private void SaveDraft(HvacSetupDraft draft) =>
        HttpContext.Session.SetString(DraftSessionKey, JsonSerializer.Serialize(draft));

    private void ClearDraft() => HttpContext.Session.Remove(DraftSessionKey);

    private async Task<bool> HasSavedSystemAsync(int propiedadId) =>
        await _db.PropiedadHvacSistemas.AnyAsync(h => h.PropiedadId == propiedadId);

    private async Task<string?> SaveLabelImageAsync(IFormFile file, int propiedadId)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedLabelExtensions.Contains(extension) || file.Length > MaxLabelBytes)
        {
            return null;
        }

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "hvac-labels");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"label-{propiedadId}-{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var fullPath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/uploads/hvac-labels/{fileName}";
    }

    private static string FormatAddress(Propiedad propiedad, PropertyInfoViewModel? info) =>
        !string.IsNullOrWhiteSpace(info?.FormattedAddress)
            ? info!.FormattedAddress!
            : (propiedad.Direccion ?? "—");

    private static string ResolvePropertyImage(Propiedad propiedad, PropertyInfoViewModel? info) =>
        "/welcome-house.png";

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
