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
public class PermitsImprovementsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermitsImprovementsController(AppDbContext db, UserManager<ApplicationUser> userManager)
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
        return View(PermitsImprovementsDisplayService.BuildIndex(bundle.Value.Propiedad, bundle.Value.Info, tab));
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, string permitId, string? tab)
    {
        var bundle = await LoadBundleAsync(id);
        if (bundle == null) return NotFound();

        var model = PermitsImprovementsDisplayService.BuildDetail(bundle.Value.Propiedad, bundle.Value.Info, permitId, tab);
        if (model == null) return NotFound();

        ViewBag.PropiedadId = id;
        ViewBag.MyHomeNav = "summary";
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
