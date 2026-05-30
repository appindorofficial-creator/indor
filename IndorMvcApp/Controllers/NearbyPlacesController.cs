using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndorMvcApp.Data;
using IndorMvcApp.Helpers;
using IndorMvcApp.Models;
using IndorMvcApp.Services;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Controllers;

[Authorize]
public class NearbyPlacesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public NearbyPlacesController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Parks(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View("Parks", NearbyPlacesDisplayService.BuildParks(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public async Task<IActionResult> Airports(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View("Airports", NearbyPlacesDisplayService.BuildAirports(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public async Task<IActionResult> Hospitals(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();
        ConfigureView(id);
        return View("Hospitals", NearbyPlacesDisplayService.BuildHospitals(bundle.Value.Propiedad, bundle.Value.Info));
    }

    private void ConfigureView(int id)
    {
        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
    }

    private async Task<(Propiedad Propiedad, PropertyInfoViewModel? Info)?> LoadBundleAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return null;

        var propiedad = await _db.Propiedades
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.Activo);

        if (propiedad == null) return null;

        var info = MyHomeDisplayService.DeserializeProperty(propiedad);
        return (propiedad, info);
    }
}
