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
public class SchoolsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SchoolsController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        if (string.Equals(tab, "compare", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Compare), new { id });
        }

        if (string.Equals(tab, "district", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(Map), new { id });
        }

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        HouseFactPreviewContext.ApplyReturnUrlToView(this);
        return View(SchoolsDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public async Task<IActionResult> Map(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(SchoolsDisplayService.BuildMap(bundle.Value.Propiedad, bundle.Value.Info));
    }

    [HttpGet]
    public async Task<IActionResult> Profile(int id, string schoolId)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        var model = SchoolsDisplayService.BuildProfile(bundle.Value.Propiedad, bundle.Value.Info, schoolId);
        if (model == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Compare(int id)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
        return View(SchoolsDisplayService.BuildCompare(bundle.Value.Propiedad, bundle.Value.Info));
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
