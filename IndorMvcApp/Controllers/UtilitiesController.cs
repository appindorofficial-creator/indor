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
public class UtilitiesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UtilitiesController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
        return View(UtilitiesDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, string utilityId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        var model = UtilitiesDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, utilityId);
        if (model == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
        return View(model);
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
