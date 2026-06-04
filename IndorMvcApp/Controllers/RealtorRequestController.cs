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
public class RealtorRequestController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RealtorGuidanceService _guidance;

    public RealtorRequestController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        RealtorGuidanceService guidance)
    {
        _db = db;
        _userManager = userManager;
        _guidance = guidance;
    }

    [HttpGet]
    public async Task<IActionResult> Request(int? propiedadId)
    {
        var propiedad = await LoadPropertyAsync(propiedadId);
        var defaultArea = propiedad != null
            ? RealtorRequestDisplayService.ExtractCityFromAddress(
                MyHomeDisplayService.DeserializeProperty(propiedad)?.FormattedAddress ?? propiedad.Direccion)
            : null;

        return View(new RealtorRequestFormViewModel
        {
            PropiedadId = propiedad?.Id,
            PreferredArea = defaultArea,
            NeedType = "Buy",
            Timeframe = "ASAP"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Request(RealtorRequestFormViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        if (string.Equals(model.NeedType, "GeneralGuidance", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(StartGuidance), new { propiedadId = model.PropiedadId });
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return await SaveSimpleRequestAsync(model, userId);
    }

    [HttpGet]
    public async Task<IActionResult> StartGuidance(int? propiedadId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        try
        {
            var propiedad = await LoadPropertyAsync(propiedadId);
            var defaultArea = propiedad != null
                ? RealtorRequestDisplayService.ExtractCityFromAddress(
                    MyHomeDisplayService.DeserializeProperty(propiedad)?.FormattedAddress ?? propiedad.Direccion)
                : null;

            var record = await _guidance.StartDraftAsync(userId, propiedad?.Id, defaultArea);
            return RedirectToAction(nameof(GuidanceStep1), new { id = record.Id });
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            TempData["RealtorError"] = "Run AlterSolicitudRealtorGeneralGuidance.sql and CreateSolicitudRealtorTables.sql on the database.";
            return RedirectToAction(nameof(Request), new { propiedadId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GuidanceStep1(int id)
    {
        var record = await LoadGuidanceAsync(id);
        if (record == null) return NotFound();

        return View(new RealtorGuidanceStep1ViewModel
        {
            SolicitudId = record.Id,
            PropiedadId = record.PropiedadId,
            RentComfortRange = record.RentComfortRange ?? "2000-3000",
            Timeframe = record.Timeframe
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuidanceStep1(RealtorGuidanceStep1ViewModel model)
    {
        var record = await LoadGuidanceAsync(model.SolicitudId);
        if (record == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        record.RentComfortRange = model.RentComfortRange;
        record.Timeframe = model.Timeframe;
        await _guidance.SaveStepAsync(record, 1);
        return RedirectToAction(nameof(GuidanceStep2), new { id = record.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GuidanceStep2(int id)
    {
        var record = await LoadGuidanceAsync(id);
        if (record == null) return NotFound();

        return View(new RealtorGuidanceStep2ViewModel
        {
            SolicitudId = record.Id,
            HomeType = record.HomeType ?? "Apartment",
            Bedrooms = record.Bedrooms ?? "2",
            Bathrooms = record.Bathrooms ?? "2",
            Occupants = record.Occupants ?? "2"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuidanceStep2(RealtorGuidanceStep2ViewModel model)
    {
        var record = await LoadGuidanceAsync(model.SolicitudId);
        if (record == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        record.HomeType = model.HomeType;
        record.Bedrooms = model.Bedrooms;
        record.Bathrooms = model.Bathrooms;
        record.Occupants = model.Occupants;
        await _guidance.SaveStepAsync(record, 2);
        return RedirectToAction(nameof(GuidanceStep3), new { id = record.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GuidanceStep3(int id)
    {
        var record = await LoadGuidanceAsync(id);
        if (record == null) return NotFound();

        return View(new RealtorGuidanceStep3ViewModel
        {
            SolicitudId = record.Id,
            Pets = record.Pets ?? "Dog",
            OutdoorSpaceImportance = record.OutdoorSpaceImportance ?? "VeryImportant",
            ParkingNeed = record.ParkingNeed ?? "Yes",
            PreferredArea = record.PreferredArea,
            OpenToNearbyAreas = record.OpenToNearbyAreas,
            Priorities = RealtorGuidanceService.ParsePriorities(record.Priorities).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuidanceStep3(RealtorGuidanceStep3ViewModel model)
    {
        var record = await LoadGuidanceAsync(model.SolicitudId);
        if (record == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Priorities.Count > 3)
        {
            ModelState.AddModelError(nameof(model.Priorities), "Select up to 3 priorities.");
            return View(model);
        }

        record.Pets = model.Pets;
        record.OutdoorSpaceImportance = model.OutdoorSpaceImportance;
        record.ParkingNeed = model.ParkingNeed;
        record.PreferredArea = NullIfEmpty(model.PreferredArea);
        record.OpenToNearbyAreas = model.OpenToNearbyAreas;
        record.Priorities = RealtorGuidanceService.JoinPriorities(model.Priorities);
        await _guidance.SaveStepAsync(record, 3);
        return RedirectToAction(nameof(GuidanceStep4), new { id = record.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GuidanceStep4(int id)
    {
        var record = await LoadGuidanceAsync(id);
        if (record == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        return View(new RealtorGuidanceStep4ViewModel
        {
            SolicitudId = record.Id,
            ContactPhone = record.ContactPhone ?? (user != null ? RealtorGuidanceService.ResolveContactPhone(user) : null),
            ContactEmail = record.ContactEmail ?? user?.Email,
            PreferredContactMethod = record.PreferredContactMethod ?? "Text",
            GuidanceNotes = record.GuidanceNotes,
            Summary = RealtorRequestDisplayService.BuildGuidanceSummary(record)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuidanceStep4(RealtorGuidanceStep4ViewModel model)
    {
        var record = await LoadGuidanceAsync(model.SolicitudId);
        if (record == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.Summary = RealtorRequestDisplayService.BuildGuidanceSummary(record);
            return View(model);
        }

        record.ContactPhone = NullIfEmpty(model.ContactPhone);
        record.ContactEmail = NullIfEmpty(model.ContactEmail);
        record.PreferredContactMethod = model.PreferredContactMethod;
        record.GuidanceNotes = NullIfEmpty(model.GuidanceNotes);
        await _guidance.FinalizeAsync(record);

        if (record.PropiedadId.HasValue)
        {
            var propiedad = await _db.Propiedades.FirstOrDefaultAsync(p => p.Id == record.PropiedadId.Value);
            if (propiedad != null)
            {
                await AddHistoryEntryAsync(propiedad, record);
            }
        }

        return RedirectToAction(nameof(Sent), new { id = record.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Sent(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        SolicitudRealtor? record;
        try
        {
            record = await _db.SolicitudesRealtor
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            return NotFound();
        }

        if (record == null) return NotFound();

        var vm = new RealtorRequestSentViewModel
        {
            SolicitudId = record.Id,
            IsGeneralGuidance = record.IsGeneralGuidance,
            NeedLabel = RealtorRequestDisplayService.NeedLabel(record.NeedType),
            AreaLabel = string.IsNullOrWhiteSpace(record.PreferredArea) ? "—" : record.PreferredArea,
            TimeframeLabel = RealtorRequestDisplayService.TimeframeLabel(record.Timeframe),
            StatusLabel = RealtorRequestDisplayService.StatusLabel(record.Status)
        };

        if (record.IsGeneralGuidance)
        {
            vm.RentComfortLabel = RealtorRequestDisplayService.RentComfortLabel(record.RentComfortRange);
            vm.HomeTypeLabel = RealtorRequestDisplayService.HomeTypeLabel(record.HomeType);
            vm.BedroomsLabel = RealtorRequestDisplayService.CountLabel(record.Bedrooms);
            vm.BathroomsLabel = RealtorRequestDisplayService.CountLabel(record.Bathrooms);
            vm.PetsLabel = RealtorRequestDisplayService.PetsLabel(record.Pets);
        }

        return View(vm);
    }

    private async Task<IActionResult> SaveSimpleRequestAsync(RealtorRequestFormViewModel model, string userId)
    {
        Propiedad? propiedad = null;
        if (model.PropiedadId.HasValue)
        {
            propiedad = await LoadPropertyAsync(model.PropiedadId);
        }

        try
        {
            var record = new SolicitudRealtor
            {
                PropiedadId = propiedad?.Id,
                UserId = userId,
                NeedType = model.NeedType,
                PreferredArea = NullIfEmpty(model.PreferredArea),
                Timeframe = model.Timeframe,
                PriceRange = NullIfEmpty(model.PriceRange),
                Notes = NullIfEmpty(model.Notes),
                Status = "MatchingInProgress",
                FechaCreacion = DateTime.UtcNow
            };

            _db.SolicitudesRealtor.Add(record);
            await _db.SaveChangesAsync();

            if (propiedad != null)
            {
                await AddHistoryEntryAsync(propiedad, record);
            }

            return RedirectToAction(nameof(Sent), new { id = record.Id });
        }
        catch (Exception ex) when (HomeDashboardDataService.IsMissingTable(ex))
        {
            ModelState.AddModelError(string.Empty,
                "Realtor requests are not available yet. Run CreateSolicitudRealtorTables.sql on the database.");
            return View(model);
        }
    }

    private async Task<SolicitudRealtor?> LoadGuidanceAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        var record = await _guidance.GetOwnedAsync(id, userId);
        if (record == null || !record.IsGeneralGuidance)
        {
            return null;
        }

        return record;
    }

    private async Task AddHistoryEntryAsync(Propiedad propiedad, SolicitudRealtor record)
    {
        var label = record.IsGeneralGuidance
            ? "General guidance"
            : RealtorRequestDisplayService.NeedLabel(record.NeedType);

        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = propiedad.Id,
            RecordType = "Request",
            Title = "Realtor request submitted",
            Description = $"{label} • {record.PreferredArea ?? "Area not specified"}.",
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
