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

    public RealtorRequestController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

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

        return View(new RealtorRequestSentViewModel
        {
            SolicitudId = record.Id,
            NeedLabel = RealtorRequestDisplayService.NeedLabel(record.NeedType),
            AreaLabel = string.IsNullOrWhiteSpace(record.PreferredArea) ? "—" : record.PreferredArea,
            TimeframeLabel = RealtorRequestDisplayService.TimeframeLabel(record.Timeframe),
            StatusLabel = RealtorRequestDisplayService.StatusLabel(record.Status)
        });
    }

    private async Task AddHistoryEntryAsync(Propiedad propiedad, SolicitudRealtor record)
    {
        _db.PropiedadHistorial.Add(new PropiedadHistorial
        {
            PropiedadId = propiedad.Id,
            RecordType = "Request",
            Title = "Realtor request submitted",
            Description = $"{RealtorRequestDisplayService.NeedLabel(record.NeedType)} • {record.PreferredArea ?? "Area not specified"}.",
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
